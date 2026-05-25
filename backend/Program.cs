

using TiendaMicroempresas.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddLocalShopCors(builder.Configuration);
builder.Services.AddLocalShopServices();

var app = builder.Build();

// Diagnostic: log which connection string is active (mask password)
{
    var connStr = builder.Configuration.GetConnectionString("DefaultConnection") ?? "(null)";
    var masked = System.Text.RegularExpressions.Regex.Replace(connStr, @"Password=[^;]+", "Password=***");
    var logger = app.Services.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
    logger.LogWarning("Connection string [DefaultConnection]: {ConnStr}", masked);

    // Check for old Supabase key that Render might still have
    var oldConn = builder.Configuration.GetConnectionString("Supabase");
    if (oldConn != null)
        logger.LogWarning("WARNING: Old 'Supabase' connection string is also present!");

    // Check for raw environment variables
    var envConn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
    var envOld = Environment.GetEnvironmentVariable("ConnectionStrings__Supabase");
    if (envConn != null) logger.LogWarning("ENV ConnectionStrings__DefaultConnection is SET");
    if (envOld != null) logger.LogWarning("ENV ConnectionStrings__Supabase is SET");
}

app.UseCors("frontend");

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "LocalShop API" }));

app.MapGet("/", () => Results.Ok(new
{
    api = "LocalShop API",
    status = "Activa",
    endpoints = new[]
    {
        "/api/store/overview",
        "/api/auth/register",
        "/api/auth/login",
        "/api/auth/me",
        "/api/customers/register",
        "/api/customers/login",
        "/api/customers/orders",
        "/api/admin/catalog",
        "/api/admin/orders",
        "/api/admin/products",
        "/api/orders",
        "/api/orders/checkout"
    }
}));

app.MapControllers();

app.Run();

