using bg_backend.Data;
using bg_backend.DTOs;
using Microsoft.EntityFrameworkCore;

namespace bg_backend.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> GetProductsAsync(ProductFilter filter)
    {
        var query = _context.Products.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(p => EF.Functions.ILike(p.Name, $"%{filter.Name}%"));

        if (!string.IsNullOrWhiteSpace(filter.Code))
            query = query.Where(p => EF.Functions.ILike(p.Code, $"%{filter.Code}%"));

        if (!string.IsNullOrWhiteSpace(filter.Category))
            query = query.Where(p => EF.Functions.ILike(p.Category, filter.Category));

        return await query
            .OrderBy(p => p.Name)
            .Select(p => new ProductDto(p.Id, p.Name, p.Code, p.Category, p.Price, p.Stock, p.IsActive))
            .ToListAsync();
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id)
    {
        return await _context.Products
            .Where(p => p.Id == id && p.IsActive)
            .Select(p => new ProductDto(p.Id, p.Name, p.Code, p.Category, p.Price, p.Stock, p.IsActive))
            .FirstOrDefaultAsync();
    }
}
