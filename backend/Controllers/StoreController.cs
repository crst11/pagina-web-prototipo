using Microsoft.AspNetCore.Mvc;
using TiendaMicroempresas.Api.Contracts.Auth;
using TiendaMicroempresas.Api.Contracts.Customers;
using TiendaMicroempresas.Api.Contracts.Orders;
using TiendaMicroempresas.Api.Contracts.Products;
using TiendaMicroempresas.Api.Contracts.Store;
using TiendaMicroempresas.Api.Repositories;

namespace TiendaMicroempresas.Api.Controllers;

/// <summary>
/// Controlador público de la vitrina empresarial.
///
/// Expone los datos del marketplace sin requerir autenticación.
/// Es consumido por el frontend al cargar la página de inicio
/// para obtener empresas, productos y categorías disponibles.
///
/// ## Endpoints disponibles
/// - <c>GET /api/store/overview</c> — Snapshot completo del marketplace
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class StoreController(IStoreRepository repository) : ControllerBase
{
    /// <summary>
    /// Retorna el snapshot completo del marketplace.
    /// Incluye empresas activas, productos destacados, categorías y contadores totales.
    /// Este endpoint no requiere autenticación: es público para todos los visitantes.
    /// </summary>
    [HttpGet("overview")]
    [ProducesResponseType<StoreOverviewResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<StoreOverviewResponse>> GetOverview(CancellationToken cancellationToken)
    {
        return Ok(await repository.GetMarketplaceOverviewAsync(cancellationToken));
    }
}
