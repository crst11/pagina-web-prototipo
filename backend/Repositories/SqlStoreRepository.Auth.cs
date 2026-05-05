using System.Data;
using System.Data.Odbc;
using TiendaMicroempresas.Api.Contracts.Auth;
using TiendaMicroempresas.Api.Contracts.Store;

namespace TiendaMicroempresas.Api.Repositories;

// Autenticacion y gestion de perfil de empresarios.
// Cubre registro, login, actualizacion de perfil, logout y eliminacion de cuenta.
public sealed partial class SqlStoreRepository
{
    public async Task<AuthResponse> RegisterOwnerAsync(RegisterOwnerRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = CreateOpenConnection();
        var normalizedEmail = NormalizeEmail(request.Email);
        var businessName = RequireText(request.BusinessName, "Ingresa el nombre de la empresa.");

        EnsureEmailAvailable(connection, null, normalizedEmail, null);
        EnsureBusinessNameAvailable(connection, null, businessName, null);

        var salt = CreateSalt();
        var hash = HashPassword(request.Password, salt);
        var businessId = InsertAndGetId(
            connection,
            null,
            """
            INSERT INTO dbo.Businesses
            (
                Slug,
                OwnerName,
                BusinessName,
                Email,
                PasswordHash,
                PasswordSalt,
                Phone,
                City,
                Address,
                Tagline,
                Description,
                ShippingLeadTime,
                MinimumOrderAmount,
                LogoUrl,
                BannerUrl,
                WebsiteUrl
            )
            VALUES (?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?, ?);
            SELECT CAST(SCOPE_IDENTITY() AS INT);
            """,
            BuildUniqueSlug(connection, null, businessName, null),
            RequireText(request.OwnerName, "Ingresa el nombre de la persona responsable."),
            businessName,
            normalizedEmail,
            hash,
            salt,
            RequireText(request.Phone, "Ingresa un telefono de contacto."),
            RequireText(request.City, "Ingresa la ciudad principal de operacion."),
            RequireText(request.Address, "Ingresa la direccion comercial."),
            RequireText(request.Tagline, "Ingresa una frase corta para presentar la empresa."),
            RequireText(request.Description, "Ingresa una descripcion clara del negocio."),
            RequireText(request.ShippingLeadTime, "Ingresa el tiempo estimado de entrega."),
            RequireMoney(request.MinimumOrderAmount, "El pedido minimo por empresa no puede ser negativo."),
            DefaultLogoUrl,
            DefaultBannerUrl,
            NormalizeOptionalText(request.WebsiteUrl));

        return await CreateSessionResponseAsync(connection, businessId);
    }

    public async Task<AuthResponse> LoginOwnerAsync(LoginOwnerRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = CreateOpenConnection();
        using var command = CreateCommand(
            connection,
            null,
            """
            SELECT BusinessId, PasswordHash, PasswordSalt
            FROM dbo.Businesses
            WHERE Email = ?;
            """,
            NormalizeEmail(request.Email));

        using var reader = command.ExecuteReader(CommandBehavior.SingleRow);
        if (!reader.Read())
        {
            throw new InvalidOperationException("Correo o contrasena incorrectos.");
        }

        var businessId = reader.GetInt32(0);
        var passwordHash = reader.GetString(1);
        var passwordSalt = reader.GetString(2);

        if (!VerifyPassword(request.Password, passwordSalt, passwordHash))
        {
            throw new InvalidOperationException("Correo o contrasena incorrectos.");
        }

        return await CreateSessionResponseAsync(connection, businessId);
    }

    public Task<MarketplaceBusinessDto> GetBusinessByTokenAsync(string token, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = CreateOpenConnection();
        var businessId = GetBusinessIdByToken(connection, null, token);
        return Task.FromResult(GetBusinessById(connection, null, businessId, publishedOnly: false));
    }

    public Task<MarketplaceBusinessDto> UpdateOwnerProfileAsync(string token, UpdateOwnerProfileRequest request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = CreateOpenConnection();
        var businessId = GetBusinessIdByToken(connection, null, token);
        var normalizedEmail = NormalizeEmail(request.Email);
        var businessName = RequireText(request.BusinessName, "Ingresa el nombre de la empresa.");

        EnsureEmailAvailable(connection, null, normalizedEmail, businessId);
        EnsureBusinessNameAvailable(connection, null, businessName, businessId);

        ExecuteNonQuery(
            connection,
            null,
            """
            UPDATE dbo.Businesses
            SET Slug = ?,
                OwnerName = ?,
                BusinessName = ?,
                Email = ?,
                Phone = ?,
                City = ?,
                Address = ?,
                Tagline = ?,
                Description = ?,
                ShippingLeadTime = ?,
                MinimumOrderAmount = ?,
                LogoUrl = ?,
                BannerUrl = ?,
                WebsiteUrl = ?,
                UpdatedAt = SYSDATETIME()
            WHERE BusinessId = ?;
            """,
            BuildUniqueSlug(connection, null, businessName, businessId),
            RequireText(request.OwnerName, "Ingresa el nombre de la persona responsable."),
            businessName,
            normalizedEmail,
            RequireText(request.Phone, "Ingresa un telefono de contacto."),
            RequireText(request.City, "Ingresa la ciudad principal de operacion."),
            RequireText(request.Address, "Ingresa la direccion comercial."),
            RequireText(request.Tagline, "Ingresa una frase corta para presentar la empresa."),
            RequireText(request.Description, "Ingresa una descripcion clara del negocio."),
            RequireText(request.ShippingLeadTime, "Ingresa el tiempo estimado de entrega."),
            RequireMoney(request.MinimumOrderAmount, "El pedido minimo por empresa no puede ser negativo."),
            RequireImageUrl(request.LogoUrl, DefaultLogoUrl),
            RequireImageUrl(request.BannerUrl, DefaultBannerUrl),
            NormalizeOptionalText(request.WebsiteUrl),
            businessId);

        return Task.FromResult(GetBusinessById(connection, null, businessId, publishedOnly: false));
    }

    public Task LogoutAsync(string token, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = CreateOpenConnection();
        ExecuteNonQuery(connection, null, "DELETE FROM dbo.BusinessSessions WHERE SessionToken = ?;", token);
        return Task.CompletedTask;
    }

    public Task DeleteBusinessAsync(string token, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var connection = CreateOpenConnection();
        var businessId = GetBusinessIdByToken(connection, null, token);

        using var transaction = connection.BeginTransaction();
        try
        {
            ExecuteNonQuery(connection, transaction, "UPDATE dbo.Orders SET BusinessId = NULL WHERE BusinessId = ?;", businessId);
            ExecuteNonQuery(connection, transaction, "DELETE FROM dbo.BusinessSessions WHERE BusinessId = ?;", businessId);
            ExecuteNonQuery(connection, transaction, "UPDATE dbo.Products SET IsArchived = 1, IsPublished = 0, IsFeatured = 0 WHERE BusinessId = ?;", businessId);
            ExecuteNonQuery(connection, transaction, "DELETE FROM dbo.Businesses WHERE BusinessId = ?;", businessId);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        return Task.CompletedTask;
    }

    private Task<AuthResponse> CreateSessionResponseAsync(OdbcConnection connection, int businessId)
    {
        var token = Guid.NewGuid().ToString("N");
        ExecuteNonQuery(
            connection,
            null,
            """
            INSERT INTO dbo.BusinessSessions (BusinessId, SessionToken, ExpiresAt)
            VALUES (?, ?, DATEADD(DAY, 7, SYSDATETIME()));
            """,
            businessId,
            token);

        return Task.FromResult(new AuthResponse(token, GetBusinessById(connection, null, businessId, publishedOnly: false)));
    }
}
