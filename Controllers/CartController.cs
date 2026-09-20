using bg_backend.Common;
using bg_backend.DTOs;
using bg_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bg_backend.Controllers;

[ApiController]
[Route("api/cart")]
[Authorize]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCart()
    {
        var cart = await _cartService.GetCartAsync(User.GetUserId());
        return Ok(cart);
    }

    /// <summary>Agregar producto al carrito validando stock disponible.</summary>
    [HttpPost("items")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddItem([FromBody] AddCartItemRequest request)
    {
        var (cart, error) = await _cartService.AddItemAsync(User.GetUserId(), request);

        if (error is not null)
        {
            // Distinguir 404 de 409 según el tipo de error
            if (error.Contains("no encontrado"))
                return NotFound(new { message = error });
            return Conflict(new { message = error });
        }

        return StatusCode(StatusCodes.Status201Created, cart);
    }

    /// <summary>Actualizar cantidad de un producto validando stock.</summary>
    [HttpPut("items/{productId:int}")]
    [ProducesResponseType(typeof(CartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateItem(int productId, [FromBody] UpdateCartItemRequest request)
    {
        var (cart, error) = await _cartService.UpdateItemQuantityAsync(User.GetUserId(), productId, request);

        if (error is not null)
        {
            if (error.Contains("no está en el carrito"))
                return NotFound(new { message = error });
            return Conflict(new { message = error });
        }

        return Ok(cart);
    }

    /// <summary>Eliminar producto del carrito.</summary>
    [HttpDelete("items/{productId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(int productId)
    {
        var removed = await _cartService.RemoveItemAsync(User.GetUserId(), productId);

        if (!removed)
            return NotFound(new { message = "Producto no está en el carrito" });

        return NoContent();
    }

    /// <summary>Vaciar carrito completo.</summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ClearCart()
    {
        await _cartService.ClearCartAsync(User.GetUserId());
        return NoContent();
    }
}
