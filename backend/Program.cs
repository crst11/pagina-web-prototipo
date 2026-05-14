// ═══════════════════════════════════════════════════════════════════════════
// LocalShop API — Punto de entrada de la aplicación
//
// Configura y arranca el servidor web ASP.NET Core con los servicios
// necesarios para la plataforma LocalShop.
//
// Arquitectura:
//   ┌─────────────────────────────────────────────────────┐
//   │  Controllers  →  IStoreRepository  →  SQL Server    │
//   │  (MVC: Vista) →  (Modelo/Repo)    →  (Datos)        │
//   └─────────────────────────────────────────────────────┘
//
// Flujo de una solicitud HTTP:
//   Angular Frontend → CORS Policy "frontend" → Controller → Repository → BD
// ═══════════════════════════════════════════════════════════════════════════

using TiendaMicroempresas.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ── Registro de servicios ──────────────────────────────────────────────────
// Los servicios se agrupan en métodos de extensión para mantener este archivo
// limpio y facilitar la lectura y el mantenimiento.
builder.Services.AddLocalShopCors(builder.Configuration);   // Política CORS para Angular
builder.Services.AddLocalShopServices();                    // Controllers + Repositorio

var app = builder.Build();

// ── Middleware ─────────────────────────────────────────────────────────────
app.UseCors("frontend");   // Permite solicitudes del frontend Angular

// ── Endpoints de diagnóstico ───────────────────────────────────────────────
// Endpoint de health check: confirma que la API está en línea.
app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "LocalShop API" }));

// Endpoint raíz: documenta los endpoints disponibles en la API.
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

// ── Controladores MVC ──────────────────────────────────────────────────────
app.MapControllers();

app.Run();

