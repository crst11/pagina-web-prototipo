using Microsoft.AspNetCore.Mvc;
using TiendaMicroempresas.Api.Contracts.Auth;
using TiendaMicroempresas.Api.Contracts.Customers;
using TiendaMicroempresas.Api.Contracts.Orders;
using TiendaMicroempresas.Api.Contracts.Products;
using TiendaMicroempresas.Api.Contracts.Store;
using TiendaMicroempresas.Api.Repositories;

namespace TiendaMicroempresas.Api.Controllers;

/// <summary>
/// Controlador de pedidos del marketplace.
///
/// Gestiona la creación de pedidos individuales y el procesamiento
/// del carrito completo (checkout). Todos los endpoints requieren
/// autenticación del cliente via <c>X-Customer-Token</c>.
///
/// ## Endpoints disponibles
/// - <c>POST /api/orders</c>          — Crear pedido individual a una empresa
/// - <c>POST /api/orders/checkout</c> — Procesar carrito completo (multi-empresa)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController(IStoreRepository repository) : ControllerBase
{
    /// <summary>
    /// Procesa el carrito completo del cliente en una sola transacción.
    /// Genera un pedido independiente por cada empresa involucrada en el carrito.
    /// Valida el token del cliente, los datos de envío y todos los ítems antes de confirmar.
    /// </summary>
    /// <param name="request">Datos del cliente, dirección de envío e ítems del carrito.</param>
    /// <param name="customerToken">Token de sesión del cliente.</param>
    [HttpPost("checkout")]
    [ProducesResponseType<CheckoutCartResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CheckoutCartResponse>> CheckoutCart(
        [FromBody] CheckoutCartRequest request,
        [FromHeader(Name = "X-Customer-Token")] string? customerToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(customerToken))
        {
            return Unauthorized(new { message = "Registrate o inicia sesion como cliente para completar la compra." });
        }

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
            var response = await repository.CheckoutCartAsync(request, customerToken, cancellationToken);
            return Created("/api/orders/checkout", response);
        }
        catch (InvalidOperationException exception)
        {
            if (exception.Message.Contains("sesion del cliente", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new { message = exception.Message });
            }

            return BadRequest(new { message = exception.Message });
        }
    }

    /// <summary>
    /// Crea un pedido individual del cliente a una empresa específica.
    /// Valida stock, pedido mínimo por empresa y sesión del cliente.
    /// </summary>
    /// <param name="request">Datos del pedido: empresa, productos, datos de envío.</param>
    /// <param name="customerToken">Token de sesión del cliente.</param>
    [HttpPost]
    [ProducesResponseType<OrderCreatedResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderCreatedResponse>> CreateOrder(
        [FromBody] CreateOrderRequest request,
        [FromHeader(Name = "X-Customer-Token")] string? customerToken,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(customerToken))
        {
            return Unauthorized(new { message = "Registrate o inicia sesion como cliente para completar la compra." });
        }

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
            var createdOrder = await repository.CreateOrderAsync(request, customerToken, cancellationToken);
            return Created($"/api/orders/{createdOrder.OrderId}", createdOrder);
        }
        catch (InvalidOperationException exception)
        {
            if (exception.Message.Contains("sesion del cliente", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new { message = exception.Message });
            }

            return BadRequest(new { message = exception.Message });
        }
    }
}
