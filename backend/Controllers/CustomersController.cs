using Microsoft.AspNetCore.Mvc;
using TiendaMicroempresas.Api.Contracts.Auth;
using TiendaMicroempresas.Api.Contracts.Customers;
using TiendaMicroempresas.Api.Contracts.Orders;
using TiendaMicroempresas.Api.Contracts.Products;
using TiendaMicroempresas.Api.Contracts.Store;
using TiendaMicroempresas.Api.Repositories;

namespace TiendaMicroempresas.Api.Controllers;

/// <summary>
/// Controlador del portal de clientes (compradores).
///
/// Gestiona el registro, autenticación, actualización de perfil,
/// historial de pedidos y cierre de sesión de los clientes compradores.
///
/// Los endpoints protegidos requieren la cabecera <c>X-Customer-Token</c>.
///
/// ## Endpoints disponibles
/// - <c>POST   /api/customers/register</c>  — Registrar nuevo cliente
/// - <c>POST   /api/customers/login</c>     — Iniciar sesión como cliente
/// - <c>GET    /api/customers/me</c>        — Obtener perfil del cliente
/// - <c>PUT    /api/customers/me</c>        — Actualizar perfil del cliente
/// - <c>DELETE /api/customers/me</c>        — Eliminar cuenta del cliente
/// - <c>POST   /api/customers/logout</c>   — Cerrar sesión del cliente
/// - <c>GET    /api/customers/orders</c>   — Historial de pedidos del cliente
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class CustomersController(IStoreRepository repository) : ControllerBase
{
    /// <summary>
    /// Registra un nuevo cliente en la plataforma.
    /// Valida que los campos obligatorios estén completos antes de delegar al repositorio.
    /// </summary>
    /// <param name="request">Datos del nuevo cliente (nombre, correo, contraseña, dirección).</param>
    [HttpPost("register")]
    [ProducesResponseType<CustomerAuthResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerAuthResponse>> Register(
        [FromBody] RegisterCustomerRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.Phone) ||
            string.IsNullOrWhiteSpace(request.City) ||
            string.IsNullOrWhiteSpace(request.Address))
        {
            return BadRequest(new { message = "Completa los datos principales del comprador." });
        }

        try
        {
            var response = await repository.RegisterCustomerAsync(request, cancellationToken);
            return Created("/api/customers/me", response);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (OdbcException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "La base de datos de clientes no esta disponible." });
        }
    }

    /// <summary>
    /// Autentica a un cliente y genera un nuevo token de sesión.
    /// </summary>
    /// <param name="request">Correo y contraseña del cliente.</param>
    [HttpPost("login")]
    [ProducesResponseType<CustomerAuthResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CustomerAuthResponse>> Login(
        [FromBody] LoginCustomerRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(new { message = "Ingresa correo y contrasena." });
        }

        try
        {
            return Ok(await repository.LoginCustomerAsync(request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (OdbcException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "La base de datos de clientes no esta disponible." });
        }
    }

    /// <summary>
    /// Retorna el perfil del cliente autenticado.
    /// </summary>
    /// <param name="token">Token de sesión del cliente.</param>
    [HttpGet("me")]
    [ProducesResponseType<CustomerDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CustomerDto>> Me(
        [FromHeader(Name = "X-Customer-Token")] string? token,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(new { message = "Inicia sesion como cliente para continuar." });
        }

        try
        {
            return Ok(await repository.GetCustomerByTokenAsync(token, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Unauthorized(new { message = exception.Message });
        }
        catch (OdbcException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "La base de datos de clientes no esta disponible." });
        }
    }

    /// <summary>
    /// Actualiza el perfil del cliente autenticado.
    /// El correo electrónico no puede modificarse desde este endpoint.
    /// </summary>
    [HttpPut("me")]
    [ProducesResponseType<CustomerDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CustomerDto>> UpdateMe(
        [FromHeader(Name = "X-Customer-Token")] string? token,
        [FromBody] UpdateCustomerProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(new { message = "Inicia sesion como cliente para continuar." });
        }

        try
        {
            return Ok(await repository.UpdateCustomerProfileAsync(token, request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (OdbcException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "La base de datos de clientes no esta disponible." });
        }
    }

    /// <summary>
    /// Elimina permanentemente la cuenta del cliente autenticado.
    /// </summary>
    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteMe(
        [FromHeader(Name = "X-Customer-Token")] string? token,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(new { message = "Inicia sesion como cliente para continuar." });
        }

        try
        {
            await repository.DeleteCustomerProfileAsync(token, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (OdbcException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "La base de datos de clientes no esta disponible." });
        }
    }

    /// <summary>
    /// Invalida el token de sesión del cliente (cierra sesión).
    /// Si el token no existe o ya expiró, retorna 204 igualmente (idempotente).
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        [FromHeader(Name = "X-Customer-Token")] string? token,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(token))
        {
            await repository.LogoutCustomerAsync(token, cancellationToken);
        }

        return NoContent();
    }

    /// <summary>
    /// Retorna el historial completo de pedidos del cliente autenticado.
    /// Incluye el conteo total y el gasto acumulado.
    /// </summary>
    [HttpGet("orders")]
    [ProducesResponseType<CustomerOrdersHistoryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CustomerOrdersHistoryResponse>> Orders(
        [FromHeader(Name = "X-Customer-Token")] string? token,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(new { message = "Inicia sesion como cliente para ver tu historial." });
        }

        try
        {
            return Ok(await repository.GetCustomerOrdersAsync(token, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Unauthorized(new { message = exception.Message });
        }
        catch (OdbcException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { message = "La base de datos de clientes no esta disponible." });
        }
    }
}
