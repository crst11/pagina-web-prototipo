using Microsoft.AspNetCore.Mvc;
using TiendaMicroempresas.Api.Contracts.Auth;
using TiendaMicroempresas.Api.Contracts.Customers;
using TiendaMicroempresas.Api.Contracts.Orders;
using TiendaMicroempresas.Api.Contracts.Products;
using TiendaMicroempresas.Api.Contracts.Store;
using TiendaMicroempresas.Api.Repositories;

namespace TiendaMicroempresas.Api.Controllers;

/// <summary>
/// Controlador de autenticación y gestión de perfil para empresarios.
///
/// ## Rol en la arquitectura MVC
/// Actua como la capa de <b>Controlador</b>: recibe las solicitudes HTTP,
/// valida los datos de entrada y delega toda la lógica al repositorio.
/// No contiene lógica de negocio ni acceso directo a la base de datos.
///
/// ## Endpoints disponibles
/// - <c>GET  /api/auth/setup-status</c> — Estado inicial de la plataforma
/// - <c>POST /api/auth/register</c>     — Registrar nueva empresa
/// - <c>POST /api/auth/login</c>        — Iniciar sesión empresarial
/// - <c>GET  /api/auth/me</c>           — Obtener perfil de la empresa
/// - <c>PUT  /api/auth/me</c>           — Actualizar perfil de la empresa
/// - <c>POST /api/auth/logout</c>       — Cerrar sesión empresarial
/// - <c>DELETE /api/auth/me</c>         — Eliminar cuenta empresarial
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IStoreRepository repository) : ControllerBase
{
    /// <summary>
    /// Verifica si la plataforma ya tiene empresas registradas.
    /// Usada por el frontend para mostrar u ocultar el flujo de configuración inicial.
    /// </summary>
    [HttpGet("setup-status")]
    [ProducesResponseType<SetupStatusResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SetupStatusResponse>> GetSetupStatus(CancellationToken cancellationToken)
    {
        return Ok(new SetupStatusResponse(await repository.HasBusinessesAsync(cancellationToken)));
    }

    /// <summary>
    /// Registra una nueva empresa en la plataforma.
    /// Retorna el perfil de la empresa y un token de sesión al completarse con éxito.
    /// </summary>
    /// <param name="request">Datos de registro de la empresa.</param>
    [HttpPost("register")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status201Created)]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterOwnerRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var auth = await repository.RegisterOwnerAsync(request, cancellationToken);
            return Created("/api/auth/me", auth);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    /// <summary>
    /// Autentica a un empresario y genera un nuevo token de sesión.
    /// </summary>
    /// <param name="request">Correo y contraseña del empresario.</param>
    [HttpPost("login")]
    [ProducesResponseType<AuthResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginOwnerRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            return Ok(await repository.LoginOwnerAsync(request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    /// <summary>
    /// Retorna el perfil completo de la empresa autenticada.
    /// Requiere el token de sesión en la cabecera <c>X-Owner-Token</c>.
    /// </summary>
    /// <param name="token">Token de sesión del empresario.</param>
    [HttpGet("me")]
    [ProducesResponseType<MarketplaceBusinessDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<MarketplaceBusinessDto>> Me(
        [FromHeader(Name = "X-Owner-Token")] string? token,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(new { message = "Debes iniciar sesion." });
        }

        try
        {
            return Ok(await repository.GetBusinessByTokenAsync(token, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Unauthorized(new { message = exception.Message });
        }
    }

    /// <summary>
    /// Actualiza el perfil de la empresa autenticada.
    /// Requiere el token de sesión en la cabecera <c>X-Owner-Token</c>.
    /// </summary>
    [HttpPut("me")]
    [ProducesResponseType<MarketplaceBusinessDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<MarketplaceBusinessDto>> UpdateMe(
        [FromHeader(Name = "X-Owner-Token")] string? token,
        [FromBody] UpdateOwnerProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(new { message = "Debes iniciar sesion." });
        }

        try
        {
            return Ok(await repository.UpdateOwnerProfileAsync(token, request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            return Unauthorized(new { message = exception.Message });
        }
    }

    /// <summary>
    /// Invalida el token de sesión del empresario (cierra sesión).
    /// Requiere el token de sesión en la cabecera <c>X-Owner-Token</c>.
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> Logout(
        [FromHeader(Name = "X-Owner-Token")] string? token,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(new { message = "Debes iniciar sesion." });
        }

        try
        {
            await repository.LogoutAsync(token, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            return Unauthorized(new { message = exception.Message });
        }
    }

    /// <summary>
    /// Elimina permanentemente la cuenta empresarial y todos sus datos.
    /// Requiere el token de sesión en la cabecera <c>X-Owner-Token</c>.
    /// </summary>
    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> DeleteMe(
        [FromHeader(Name = "X-Owner-Token")] string? token,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(new { message = "Debes iniciar sesion." });
        }

        try
        {
            await repository.DeleteBusinessAsync(token, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            return Unauthorized(new { message = exception.Message });
        }
    }
}
