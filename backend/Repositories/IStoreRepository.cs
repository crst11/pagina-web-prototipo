using TiendaMicroempresas.Api.Contracts.Auth;
using TiendaMicroempresas.Api.Contracts.Customers;
using TiendaMicroempresas.Api.Contracts.Orders;
using TiendaMicroempresas.Api.Contracts.Products;
using TiendaMicroempresas.Api.Contracts.Store;

namespace TiendaMicroempresas.Api.Repositories;

/// <summary>
/// Contrato del repositorio central de LocalShop.
///
/// Define todas las operaciones de acceso a datos disponibles para los
/// controladores MVC. La implementación concreta es <see cref="SqlStoreRepository"/>,
/// que ejecuta las consultas SQL contra SQL Server mediante ODBC.
///
/// Al depender de esta interfaz (no de la clase concreta), los controladores
/// quedan desacoplados del motor de base de datos, facilitando pruebas
/// y cambios futuros de implementación.
/// </summary>
public interface IStoreRepository
{
    // ─── Marketplace ──────────────────────────────────────────────────────────

    /// <summary>Indica si hay al menos una empresa registrada en la plataforma.</summary>
    Task<bool> HasBusinessesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retorna el snapshot completo del marketplace: empresas, productos
    /// destacados, categorías y totales de la vitrina.
    /// </summary>
    Task<StoreOverviewResponse> GetMarketplaceOverviewAsync(CancellationToken cancellationToken);

    // ─── Autenticación Empresarial (Owner) ────────────────────────────────────

    /// <summary>Registra una nueva empresa en la plataforma.</summary>
    /// <exception cref="InvalidOperationException">
    /// Si el correo o el nombre de empresa ya están registrados.
    /// </exception>
    Task<AuthResponse> RegisterOwnerAsync(RegisterOwnerRequest request, CancellationToken cancellationToken);

    /// <summary>Autentica a un empresario y genera un nuevo token de sesión.</summary>
    /// <exception cref="InvalidOperationException">
    /// Si las credenciales son incorrectas o la empresa no existe.
    /// </exception>
    Task<AuthResponse> LoginOwnerAsync(LoginOwnerRequest request, CancellationToken cancellationToken);

    /// <summary>Retorna el perfil de la empresa asociada al token de sesión.</summary>
    /// <exception cref="InvalidOperationException">Si el token es inválido o expiró.</exception>
    Task<MarketplaceBusinessDto> GetBusinessByTokenAsync(string token, CancellationToken cancellationToken);

    /// <summary>Actualiza el perfil de la empresa autenticada.</summary>
    /// <exception cref="InvalidOperationException">Si el token es inválido o los datos violan restricciones únicas.</exception>
    Task<MarketplaceBusinessDto> UpdateOwnerProfileAsync(string token, UpdateOwnerProfileRequest request, CancellationToken cancellationToken);

    /// <summary>Invalida el token de sesión del empresario (cierra sesión).</summary>
    Task LogoutAsync(string token, CancellationToken cancellationToken);

    /// <summary>Elimina permanentemente la empresa y todos sus datos asociados.</summary>
    Task DeleteBusinessAsync(string token, CancellationToken cancellationToken);

    // ─── Autenticación de Clientes (Customer) ─────────────────────────────────

    /// <summary>Registra un nuevo cliente comprador en la plataforma.</summary>
    /// <exception cref="InvalidOperationException">Si el correo ya está registrado.</exception>
    Task<CustomerAuthResponse> RegisterCustomerAsync(RegisterCustomerRequest request, CancellationToken cancellationToken);

    /// <summary>Autentica a un cliente y genera un nuevo token de sesión.</summary>
    /// <exception cref="InvalidOperationException">Si las credenciales son incorrectas.</exception>
    Task<CustomerAuthResponse> LoginCustomerAsync(LoginCustomerRequest request, CancellationToken cancellationToken);

    /// <summary>Retorna el perfil del cliente asociado al token de sesión.</summary>
    Task<CustomerDto> GetCustomerByTokenAsync(string token, CancellationToken cancellationToken);

    /// <summary>Actualiza el perfil del cliente autenticado.</summary>
    Task<CustomerDto> UpdateCustomerProfileAsync(string token, UpdateCustomerProfileRequest request, CancellationToken cancellationToken);

    /// <summary>Elimina la cuenta del cliente autenticado.</summary>
    Task DeleteCustomerProfileAsync(string token, CancellationToken cancellationToken);

    /// <summary>Invalida el token de sesión del cliente (cierra sesión).</summary>
    Task LogoutCustomerAsync(string token, CancellationToken cancellationToken);

    /// <summary>Retorna el historial de pedidos del cliente autenticado.</summary>
    Task<CustomerOrdersHistoryResponse> GetCustomerOrdersAsync(string token, CancellationToken cancellationToken);

    // ─── Panel Empresarial (Admin) ────────────────────────────────────────────

    /// <summary>Retorna el catálogo de productos de la empresa autenticada.</summary>
    Task<AdminCatalogResponse> GetAdminCatalogAsync(string token, CancellationToken cancellationToken);

    /// <summary>Crea un nuevo producto en el catálogo de la empresa autenticada.</summary>
    Task<ProductDto> CreateProductAsync(string token, UpsertProductRequest request, CancellationToken cancellationToken);

    /// <summary>Actualiza un producto existente del catálogo de la empresa autenticada.</summary>
    Task<ProductDto> UpdateProductAsync(string token, int productId, UpsertProductRequest request, CancellationToken cancellationToken);

    /// <summary>Elimina (archiva) un producto del catálogo de la empresa autenticada.</summary>
    Task DeleteProductAsync(string token, int productId, CancellationToken cancellationToken);

    // ─── Pedidos ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Crea un pedido individual del cliente a una empresa específica.
    /// Valida stock, pedido mínimo y sesión del cliente antes de confirmar.
    /// </summary>
    Task<OrderCreatedResponse> CreateOrderAsync(CreateOrderRequest request, string customerToken, CancellationToken cancellationToken);

    /// <summary>
    /// Procesa el carrito completo del cliente en una sola transacción.
    /// Genera un pedido independiente por cada empresa involucrada.
    /// </summary>
    Task<CheckoutCartResponse> CheckoutCartAsync(CheckoutCartRequest request, string? customerToken, CancellationToken cancellationToken);

    /// <summary>Retorna el feed de pedidos recibidos por la empresa autenticada.</summary>
    Task<BusinessOrdersFeedResponse> GetBusinessOrdersAsync(string token, CancellationToken cancellationToken);
}

