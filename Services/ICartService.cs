using bg_backend.DTOs;

namespace bg_backend.Services;

public interface ICartService
{
    Task<CartDto> GetCartAsync(int userId);
    Task<(CartDto? Cart, string? Error)> AddItemAsync(int userId, AddCartItemRequest request);
    Task<(CartDto? Cart, string? Error)> UpdateItemQuantityAsync(int userId, int productId, UpdateCartItemRequest request);
    Task<bool> RemoveItemAsync(int userId, int productId);
    Task ClearCartAsync(int userId);
}
