using Microsoft.AspNetCore.Mvc;
using TiendaMicroempresas.Api.Contracts.Auth;
using TiendaMicroempresas.Api.Contracts.Customers;
using TiendaMicroempresas.Api.Contracts.Orders;
using TiendaMicroempresas.Api.Contracts.Products;
using TiendaMicroempresas.Api.Contracts.Store;
using TiendaMicroempresas.Api.Repositories;

namespace TiendaMicroempresas.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class StoreController(IStoreRepository repository) : ControllerBase
{
    [HttpGet("overview")]
    [ProducesResponseType<StoreOverviewResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<StoreOverviewResponse>> GetOverview(CancellationToken cancellationToken)
    {
        return Ok(await repository.GetMarketplaceOverviewAsync(cancellationToken));
    }
}
