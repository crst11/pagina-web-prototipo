using System.Data.Odbc;
using Microsoft.AspNetCore.Mvc;
using TiendaMicroempresas.Api.Contracts;
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
        try
        {
            return Ok(await repository.GetMarketplaceOverviewAsync(cancellationToken));
        }
        catch (OdbcException)
        {
            return Ok(DemoStoreRuntime.GetOverview());
        }
    }
}
