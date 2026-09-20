using bg_backend.Common;
using bg_backend.DTOs;
using bg_backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace bg_backend.Controllers;

[ApiController]
[Route("api/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>Finalizar compra: generar orden, disminuir stock y vaciar carrito (transaccional).</summary>
    [HttpPost]
    [ProducesResponseType(typeof(OrderDetailDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Checkout()
    {
        var result = await _orderService.CheckoutAsync(User.GetUserId());

        if (!result.IsSuccess)
        {
            if (result.IsCartEmpty)
                return BadRequest(new { message = "El carrito está vacío" });
            return Conflict(new { message = "No se pudo completar la compra", errors = result.Errors });
        }

        return CreatedAtAction(nameof(GetOrder), new { id = result.Order!.Id }, result.Order);
    }

    /// <summary>Consultar historial de compras del usuario.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<OrderSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders()
    {
        var orders = await _orderService.GetUserOrdersAsync(User.GetUserId());
        return Ok(orders);
    }

    /// <summary>Consultar detalle de una compra realizada.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(OrderDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(User.GetUserId(), id);

        if (order is null)
            return NotFound(new { message = "Orden no encontrada" });

        return Ok(order);
    }
}
