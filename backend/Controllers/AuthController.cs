using Microsoft.AspNetCore.Mvc;
using Npgsql;
using TiendaMicroempresas.Api.Contracts.Auth;
using TiendaMicroempresas.Api.Contracts.Customers;
using TiendaMicroempresas.Api.Contracts.Orders;
using TiendaMicroempresas.Api.Contracts.Products;
using TiendaMicroempresas.Api.Contracts.Store;
using TiendaMicroempresas.Api.Repositories;

namespace TiendaMicroempresas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(IStoreRepository repository) : ControllerBase
{

    [HttpGet("setup-status")]
    [ProducesResponseType<SetupStatusResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SetupStatusResponse>> GetSetupStatus(CancellationToken cancellationToken)
    {
        return Ok(new SetupStatusResponse(await repository.HasBusinessesAsync(cancellationToken)));
    }

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
