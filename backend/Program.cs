using TiendaMicroempresas.Api.Repositories;

var builder = WebApplication.CreateBuilder(args);

const string corsPolicy = "frontend";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:4200"];

builder.Services.AddCors(options =>
{
    options.AddPolicy(corsPolicy, policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddSingleton<IStoreRepository, SqlStoreRepository>();

var app = builder.Build();

app.UseCors(corsPolicy);

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    service = "LocalShop API"
}));

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
        "/api/admin/catalog",
        "/api/admin/orders",
        "/api/admin/products",
        "/api/orders",
        "/api/orders/checkout"
    }
}));

app.MapControllers();

app.Run();
