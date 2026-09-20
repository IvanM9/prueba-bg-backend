using bg_backend.DTOs;
using bg_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bg_backend.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>Listar productos con filtros opcionales (name, code, category).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<ProductDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? name,
        [FromQuery] string? code,
        [FromQuery] string? category)
    {
        var products = await _productService.GetProductsAsync(new ProductFilter(name, code, category));
        return Ok(products);
    }

    /// <summary>Consultar detalle de un producto.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProduct(int id)
    {
        var product = await _productService.GetProductByIdAsync(id);

        if (product is null)
            return NotFound(new { message = "Producto no encontrado" });

        return Ok(product);
    }
}
