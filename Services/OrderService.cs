using System.Data;
using bg_backend.Data;
using bg_backend.DTOs;
using bg_backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace bg_backend.Services;

public class OrderService : IOrderService
{
    private const decimal DiscountThreshold = 100m;
    private const decimal DiscountRate = 0.10m;

    private readonly AppDbContext _context;
    private readonly ILogger<OrderService> _logger;

    public OrderService(AppDbContext context, ILogger<OrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<CheckoutResult> CheckoutAsync(int userId)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database
                .BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var cartItems = await _context.CartItems
                    .Include(ci => ci.Product)
                    .Where(ci => ci.UserId == userId)
                    .ToListAsync();

                if (cartItems.Count == 0)
                    return new CheckoutResult(null, new List<string> { "El carrito está vacío" });

                // Re-validar stock DENTRO de la transacción
                var errors = new List<string>();
                foreach (var item in cartItems)
                {
                    if (!item.Product.IsActive)
                        errors.Add($"El producto '{item.Product.Name}' ya no está disponible");
                    else if (item.Quantity > item.Product.Stock)
                        errors.Add($"Stock insuficiente para '{item.Product.Name}'. Disponible: {item.Product.Stock}, solicitado: {item.Quantity}");
                }

                if (errors.Count > 0)
                {
                    await transaction.RollbackAsync();
                    return new CheckoutResult(null, errors);
                }

                // Calcular totales (misma regla que CartService: > $100 → 10%)
                var subtotal = cartItems.Sum(ci => ci.Product.Price * ci.Quantity);
                var discount = subtotal > DiscountThreshold ? Math.Round(subtotal * DiscountRate, 2) : 0m;
                var total = subtotal - discount;

                // Crear orden con snapshots de nombre/precio
                var order = new Order
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    Subtotal = subtotal,
                    Discount = discount,
                    Total = total,
                    Items = cartItems.Select(ci => new OrderItem
                    {
                        ProductId = ci.ProductId,
                        ProductName = ci.Product.Name,
                        UnitPrice = ci.Product.Price,
                        Quantity = ci.Quantity,
                        LineSubtotal = ci.Product.Price * ci.Quantity
                    }).ToList()
                };

                // Decrementar stock
                foreach (var item in cartItems)
                    item.Product.Stock -= item.Quantity;

                // Vaciar carrito
                _context.CartItems.RemoveRange(cartItems);
                _context.Orders.Add(order);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Orden {OrderId} creada para usuario {UserId}. Total: {Total}",
                    order.Id, userId, total);

                return new CheckoutResult(new OrderDetailDto(
                    order.Id,
                    order.CreatedAt,
                    order.Subtotal,
                    order.Discount,
                    order.Total,
                    order.Items.Select(i => new OrderItemDto(
                        i.ProductId, i.ProductName, i.UnitPrice, i.Quantity, i.LineSubtotal
                    )).ToList()
                ), null);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public async Task<List<OrderSummaryDto>> GetUserOrdersAsync(int userId)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Select(o => new OrderSummaryDto(
                o.Id, o.CreatedAt, o.Subtotal, o.Discount, o.Total, o.Items.Count
            ))
            .ToListAsync();
    }

    public async Task<OrderDetailDto?> GetOrderByIdAsync(int userId, int orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        // Importante: filtrar por userId para que un usuario no vea órdenes ajenas (IDOR)

        if (order is null) return null;

        return new OrderDetailDto(
            order.Id,
            order.CreatedAt,
            order.Subtotal,
            order.Discount,
            order.Total,
            order.Items.Select(i => new OrderItemDto(
                i.ProductId, i.ProductName, i.UnitPrice, i.Quantity, i.LineSubtotal
            )).ToList()
        );
    }
}
