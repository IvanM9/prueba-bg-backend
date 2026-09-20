namespace bg_backend.DTOs;

public record OrderItemDto(
    int ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal LineSubtotal
);

public record OrderSummaryDto(
    int Id,
    DateTime CreatedAt,
    decimal Subtotal,
    decimal Discount,
    decimal Total,
    int ItemCount
);

public record OrderDetailDto(
    int Id,
    DateTime CreatedAt,
    decimal Subtotal,
    decimal Discount,
    decimal Total,
    List<OrderItemDto> Items
);
