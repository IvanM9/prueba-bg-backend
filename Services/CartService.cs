using bg_backend.Data;
using bg_backend.DTOs;
using bg_backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace bg_backend.Services;

public class CartService : ICartService
{
    private const decimal DiscountThreshold = 100m;
    private const decimal DiscountRate = 0.10m;

    private readonly AppDbContext _context;

    public CartService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CartDto> GetCartAsync(int userId)
    {
        var items = await _context.CartItems
            .Include(ci => ci.Product)
            .Where(ci => ci.UserId == userId)
            .ToListAsync();

        return BuildCartDto(items);
    }

    public async Task<(CartDto? Cart, string? Error)> AddItemAsync(int userId, AddCartItemRequest request)
    {
        var product = await _context.Products.FindAsync(request.ProductId);
        if (product is null || !product.IsActive)
            return (null, "Producto no encontrado");

        var existingItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == request.ProductId);

        var newTotalQuantity = (existingItem?.Quantity ?? 0) + request.Quantity;
        if (newTotalQuantity > product.Stock)
            return (null, $"Stock insuficiente. Disponible: {product.Stock}");

        if (existingItem is null)
        {
            _context.CartItems.Add(new CartItem
            {
                UserId = userId,
                ProductId = request.ProductId,
                Quantity = request.Quantity
            });
        }
        else
        {
            existingItem.Quantity = newTotalQuantity;
        }

        await _context.SaveChangesAsync();
        return (await GetCartAsync(userId), null);
    }

    public async Task<(CartDto? Cart, string? Error)> UpdateItemQuantityAsync(int userId, int productId, UpdateCartItemRequest request)
    {
        var cartItem = await _context.CartItems
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);

        if (cartItem is null)
            return (null, "Producto no está en el carrito");

        if (request.Quantity > cartItem.Product.Stock)
            return (null, $"Stock insuficiente. Disponible: {cartItem.Product.Stock}");

        cartItem.Quantity = request.Quantity;
        await _context.SaveChangesAsync();
        return (await GetCartAsync(userId), null);
    }

    public async Task<bool> RemoveItemAsync(int userId, int productId)
    {
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(ci => ci.UserId == userId && ci.ProductId == productId);

        if (cartItem is null)
            return false;

        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task ClearCartAsync(int userId)
    {
        var items = await _context.CartItems.Where(ci => ci.UserId == userId).ToListAsync();
        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync();
    }

    public static CartDto BuildCartDto(List<CartItem> items)
    {
        var itemDtos = items.Select(ci => new CartItemDto(
            ProductId: ci.ProductId,
            ProductName: ci.Product.Name,
            ProductCode: ci.Product.Code,
            UnitPrice: ci.Product.Price,
            Quantity: ci.Quantity,
            AvailableStock: ci.Product.Stock,
            LineSubtotal: ci.Product.Price * ci.Quantity
        )).ToList();

        var subtotal = itemDtos.Sum(i => i.LineSubtotal);
        var discount = subtotal > DiscountThreshold ? Math.Round(subtotal * DiscountRate, 2) : 0m;
        var total = subtotal - discount;

        return new CartDto(itemDtos, subtotal, discount, total);
    }
}
