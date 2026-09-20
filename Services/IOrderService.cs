using bg_backend.DTOs;

namespace bg_backend.Services;

public record CheckoutResult(OrderDetailDto? Order, List<string>? Errors)
{
    public bool IsSuccess => Order is not null;
    public bool IsCartEmpty => Errors is not null && Errors.Any(e => e.Contains("vacío"));
}

public interface IOrderService
{
    Task<CheckoutResult> CheckoutAsync(int userId);

    Task<List<OrderSummaryDto>> GetUserOrdersAsync(int userId);
    Task<OrderDetailDto?> GetOrderByIdAsync(int userId, int orderId);
}
