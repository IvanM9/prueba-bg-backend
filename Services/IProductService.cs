using bg_backend.DTOs;

namespace bg_backend.Services;

public interface IProductService
{
    Task<List<ProductDto>> GetProductsAsync(ProductFilter filter);
    Task<ProductDto?> GetProductByIdAsync(int id);
}
