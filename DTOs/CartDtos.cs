using System.ComponentModel.DataAnnotations;

namespace bg_backend.DTOs;

public record AddCartItemRequest(
    [property: Required] int ProductId,
    [property: Range(1, int.MaxValue)] int Quantity
);

public record UpdateCartItemRequest(
    [property: Range(1, int.MaxValue)] int Quantity
);

public record CartItemDto(
    int ProductId,
    string ProductName,
    string ProductCode,
    decimal UnitPrice,
    int Quantity,
    int AvailableStock,
    decimal LineSubtotal
);

public record CartDto(
    List<CartItemDto> Items,
    decimal Subtotal,
    decimal Discount,
    decimal Total
);
