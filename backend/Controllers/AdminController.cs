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
public sealed class AdminController(IStoreRepository repository) : ControllerBase
{

    [HttpGet("orders")]
    [ProducesResponseType<BusinessOrdersFeedResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<BusinessOrdersFeedResponse>> GetOrders(
        [FromHeader(Name = "X-Owner-Token")] string? token,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(new { message = "Debes iniciar sesion como empresario." });
        }

        try
        {
            return Ok(await repository.GetBusinessOrdersAsync(token, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            if (exception.Message.Contains("sesion", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new { message = exception.Message });
            }

            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet("catalog")]
    [ProducesResponseType<AdminCatalogResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminCatalogResponse>> GetCatalog(
        [FromHeader(Name = "X-Owner-Token")] string? token,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(new { message = "Debes iniciar sesion como empresario." });
        }

        try
        {
            return Ok(await repository.GetAdminCatalogAsync(token, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            if (exception.Message.Contains("sesion", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new { message = exception.Message });
            }

            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPost("products")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<ProductDto>> CreateProduct(
        [FromHeader(Name = "X-Owner-Token")] string? token,
        [FromBody] UpsertProductRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(new { message = "Debes iniciar sesion como empresario." });
        }

        try
        {
            var product = await repository.CreateProductAsync(token, request, cancellationToken);
            return Created($"/api/admin/products/{product.ProductId}", product);
        }
        catch (InvalidOperationException exception)
        {
            if (exception.Message.Contains("sesion", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new { message = exception.Message });
            }

            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpPut("products/{productId:int}")]
    [ProducesResponseType<ProductDto>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ProductDto>> UpdateProduct(
        [FromHeader(Name = "X-Owner-Token")] string? token,
        int productId,
        [FromBody] UpsertProductRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(new { message = "Debes iniciar sesion como empresario." });
        }

        try
        {
            return Ok(await repository.UpdateProductAsync(token, productId, request, cancellationToken));
        }
        catch (InvalidOperationException exception)
        {
            if (exception.Message.Contains("sesion", StringComparison.OrdinalIgnoreCase))
            {
                return Unauthorized(new { message = exception.Message });
            }

            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpDelete("products/{productId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> DeleteProduct(
        [FromHeader(Name = "X-Owner-Token")] string? token,
        int productId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Unauthorized(new { message = "Debes iniciar sesion como empresario." });
        }

        try
        {
            await repository.DeleteProductAsync(token, productId, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            if (exception.Message.Contains("sesion", StringComparison.OrdinalIgnoreCase))
            {
                return new UnauthorizedObjectResult(new { message = exception.Message });
            }

            return new BadRequestObjectResult(new { message = exception.Message });
        }
    }
}
