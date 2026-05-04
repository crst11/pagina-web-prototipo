using System.Data.Odbc;
using Microsoft.AspNetCore.Mvc;
using TiendaMicroempresas.Api.Contracts;
using TiendaMicroempresas.Api.Repositories;

namespace TiendaMicroempresas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController(IStoreRepository repository) : ControllerBase
{
    [HttpPost("checkout")]
    [ProducesResponseType<CheckoutCartResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CheckoutCartResponse>> CheckoutCart(
        [FromBody] CheckoutCartRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Phone) ||
            string.IsNullOrWhiteSpace(request.City) ||
            string.IsNullOrWhiteSpace(request.Address))
        {
            return BadRequest(new { message = "Completa los datos principales de contacto y envio." });
        }

        if (request.Items is null || request.Items.Count == 0)
        {
            return BadRequest(new { message = "Agrega al menos un producto al carrito antes de comprar." });
        }

        if (request.Items.Any(item => item.BusinessId <= 0 || item.ProductId <= 0 || item.Quantity <= 0))
        {
            return BadRequest(new { message = "Revisa las empresas, productos y cantidades del carrito antes de continuar." });
        }

        try
        {
            var response = await repository.CheckoutCartAsync(request, cancellationToken);
            return Created("/api/orders/checkout", response);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (OdbcException)
        {
            try
            {
                var response = DemoStoreRuntime.CheckoutCart(request);
                return Created("/api/orders/checkout", response);
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
        }
    }

    [HttpPost]
    [ProducesResponseType<OrderCreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderCreatedResponse>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Phone) ||
            string.IsNullOrWhiteSpace(request.City) ||
            string.IsNullOrWhiteSpace(request.Address))
        {
            return BadRequest(new { message = "Completa los datos principales de contacto y envio." });
        }

        if (request.BusinessId <= 0)
        {
            return BadRequest(new { message = "Selecciona una empresa valida para el pedido." });
        }

        if (request.Items is null || request.Items.Count == 0)
        {
            return BadRequest(new { message = "El pedido debe incluir al menos un producto." });
        }

        if (request.Items.Any(item => item.Quantity <= 0))
        {
            return BadRequest(new { message = "Todas las cantidades del pedido deben ser mayores a cero." });
        }

        try
        {
            var createdOrder = await repository.CreateOrderAsync(request, cancellationToken);
            return Created($"/api/orders/{createdOrder.OrderId}", createdOrder);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (OdbcException)
        {
            try
            {
                var createdOrder = DemoStoreRuntime.CreateOrder(request);
                return Created($"/api/orders/{createdOrder.OrderId}", createdOrder);
            }
            catch (InvalidOperationException exception)
            {
                return BadRequest(new { message = exception.Message });
            }
        }
    }
}
