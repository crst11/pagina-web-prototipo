using Microsoft.AspNetCore.Mvc;
using TiendaMicroempresas.Api.Contracts.Auth;
using TiendaMicroempresas.Api.Contracts.Customers;
using TiendaMicroempresas.Api.Contracts.Orders;
using TiendaMicroempresas.Api.Contracts.Products;
using TiendaMicroempresas.Api.Contracts.Store;
using TiendaMicroempresas.Api.Repositories;

namespace TiendaMicroempresas.Api.Controllers;

/// <summary>
/// Controlador del panel empresarial (administración de catálogo y pedidos).
///
/// Todos los endpoints requieren autenticación vía la cabecera
/// <c>X-Owner-Token</c>. El repositorio valida que el token sea válido
/// y corresponda a una empresa activa antes de ejecutar cualquier operación.
///
/// ## Endpoints disponibles
/// - <c>GET    /api/admin/orders</c>              — Feed de pedidos recibidos
/// - <c>GET    /api/admin/catalog</c>             — Catálogo de productos
/// - <c>POST   /api/admin/products</c>            — Crear producto
/// - <c>PUT    /api/admin/products/{id}</c>       — Actualizar producto
/// - <c>DELETE /api/admin/products/{id}</c>       — Eliminar producto
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class AdminController(IStoreRepository repository) : ControllerBase
{
    /// <summary>
    /// Retorna el feed de pedidos recibidos por la empresa autenticada.
    /// Los pedidos se muestran del más reciente al más antiguo.
    /// </summary>
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

    /// <summary>
    /// Retorna el catálogo completo de productos de la empresa autenticada,
    /// incluyendo productos inactivos que no son visibles en el marketplace.
    /// </summary>
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

    /// <summary>
    /// Crea un nuevo producto en el catálogo de la empresa autenticada.
    /// </summary>
    /// <param name="request">Datos del nuevo producto (nombre, precio, stock, etc.).</param>
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

    /// <summary>
    /// Actualiza los datos de un producto existente en el catálogo.
    /// </summary>
    /// <param name="productId">Identificador del producto a actualizar.</param>
    /// <param name="request">Nuevos datos del producto.</param>
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

    /// <summary>
    /// Elimina (archiva) un producto del catálogo.
    /// El producto deja de ser visible en el marketplace pero sus datos
    /// se conservan en la base de datos para integridad del historial de pedidos.
    /// </summary>
    /// <param name="productId">Identificador del producto a eliminar.</param>
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
