# LocalShop - Marketplace multiempresa

LocalShop es una aplicacion web para microempresas. Permite publicar productos, administrar perfiles comerciales y recibir pedidos desde una vitrina compartida. Los compradores pueden revisar empresas, ver productos y agregar articulos al carrito sin iniciar sesion. Para confirmar una compra deben registrarse o iniciar sesion como clientes.

## Tecnologias

| Capa | Tecnologia | Ubicacion |
|---|---|---|
| Frontend | Angular 21 | `frontend/` |
| Backend | ASP.NET Core Web API (.NET 10) | `backend/` |
| Base de datos | SQL Server 2022 | `database/sqlserver/` |
| Contenedores | Docker Compose | `docker-compose.yml` |
| Servidor web | Nginx | `frontend/nginx.conf` |

## Estructura general

```text
PaginaWeb/
|-- frontend/                         Aplicacion web Angular
|   |-- src/
|   |   |-- main.ts                    Punto de entrada del frontend
|   |   |-- styles.css                 Estilos globales
|   |   `-- app/                       Codigo principal de Angular
|   |-- public/                        Imagenes, logos y favicon
|   |-- angular.json                   Configuracion Angular
|   |-- package.json                   Scripts y dependencias npm
|   |-- Dockerfile                     Compilacion del frontend para Docker
|   `-- nginx.conf                     Servidor SPA y proxy hacia la API
|-- backend/                          API REST en .NET
|   |-- Program.cs                     Configuracion principal de la API
|   |-- Controllers/                   Endpoints HTTP
|   |-- Contracts/                     Objetos request/response
|   |-- Repositories/                  Acceso a datos y reglas SQL
|   |-- Extensions/                    Registro de servicios
|   |-- appsettings.json               Configuracion base
|   |-- appsettings.Development.json   Configuracion local
|   `-- Dockerfile                     Publicacion del backend para Docker
|-- database/
|   `-- sqlserver/                     Scripts de base de datos
|       |-- docker-init.sql            Inicializacion usada por Docker
|       |-- init.sql                   Script completo para recrear la base
|       `-- migrations/                Cambios incrementales
|-- docs/
|   |-- ARQUITECTURA.md                Explicacion tecnica de capas y flujos
|   `-- MODULOS.md                     Guia detallada de carpetas y archivos
|-- docker-compose.yml                Servicios locales
|-- .env.example                      Variables de entorno de ejemplo
|-- PaginaWeb.sln                     Solucion .NET
`-- README.md                         Guia principal
```

## Donde se encuentra cada parte

| Necesidad | Ruta |
|---|---|
| Cambiar rutas de la pagina | `frontend/src/app/app.routes.ts` |
| Cambiar configuracion de Angular | `frontend/src/app/app.config.ts` |
| Cambiar barra, menu o layout general | `frontend/src/app/layout/` |
| Cambiar pagina principal | `frontend/src/app/features/marketplace/pages/marketplace-page/` |
| Cambiar catalogo general | `frontend/src/app/features/marketplace/pages/catalog-page/` |
| Cambiar listado de productos | `frontend/src/app/features/marketplace/pages/products-page/` |
| Cambiar listado de empresas | `frontend/src/app/features/marketplace/pages/businesses-page/` |
| Cambiar perfil publico de empresa | `frontend/src/app/features/businesses/pages/business-profile-page/` |
| Cambiar portal empresarial | `frontend/src/app/features/auth/pages/portal-page/` |
| Cambiar cuenta del comprador | `frontend/src/app/features/customers/pages/customer-account-page/` |
| Cambiar carrito | `frontend/src/app/features/cart/pages/cart-page/` |
| Cambiar pago | `frontend/src/app/features/cart/pages/payment-page/` |
| Cambiar estilos compartidos de formularios, botones y tarjetas | `frontend/src/app/shared/styles/marketplace-shared.css` |
| Cambiar modelos TypeScript | `frontend/src/app/core/models/` |
| Cambiar llamadas HTTP del frontend | `frontend/src/app/core/services/` |
| Cambiar validaciones de formularios | `frontend/src/app/core/validators/form.validators.ts` |
| Cambiar endpoints de la API | `backend/Controllers/` |
| Cambiar contratos de entrada y salida | `backend/Contracts/` |
| Cambiar consultas SQL o reglas de persistencia | `backend/Repositories/` |
| Cambiar tablas o datos iniciales | `database/sqlserver/` |
| Cambiar servicios Docker | `docker-compose.yml` |

## Ejecucion con Docker

Desde la raiz del proyecto:

```powershell
docker compose up --build
```

Servicios principales:

| Servicio | URL o puerto | Archivo relacionado |
|---|---|---|
| Aplicacion web | `http://localhost:4200` | `frontend/nginx.conf` |
| API | `http://localhost:5057` | `backend/Program.cs` |
| Health check | `http://localhost:5057/health` | `backend/Program.cs` |
| SQL Server | `localhost,14333` | `docker-compose.yml` |

Para reconstruir despues de cambios en frontend, backend o base:

```powershell
docker compose up --build -d frontend
```

La aplicacion debe seguir usando `http://localhost:4200`. No hace falta crear otro servidor para ver los cambios.

## Ejecucion local sin Docker completo

1. Levantar SQL Server y el inicializador de base:

```powershell
docker compose up -d sqlserver database-init
```

2. Ejecutar el backend:

```powershell
cd backend
dotnet run --launch-profile http
```

3. Ejecutar el frontend:

```powershell
cd frontend
npm install
npm start -- --host 0.0.0.0 --port 4200
```

## Frontend

Codigo principal: `frontend/src/app/`.

| Ruta | Funcion |
|---|---|
| `frontend/src/main.ts` | Arranca Angular y carga `AppShell` |
| `frontend/src/styles.css` | Importa estilos globales |
| `frontend/src/app/app.config.ts` | Configura providers de Angular y HTTP |
| `frontend/src/app/app.routes.ts` | Define las rutas visibles de la web |
| `frontend/src/app/layout/` | Contenedor principal, navegacion y estructura base |
| `frontend/src/app/core/models/` | Interfaces compartidas |
| `frontend/src/app/core/services/` | Servicios para API, sesion, carrito y pedidos |
| `frontend/src/app/core/validators/` | Validadores de formularios |
| `frontend/src/app/features/` | Paginas por modulo funcional |
| `frontend/src/app/shared/styles/` | Estilos reutilizables |
| `frontend/public/assets/images/` | Imagenes, banners, logos e iconos |

Cada pantalla normalmente tiene tres archivos:

| Extension | Funcion |
|---|---|
| `.ts` | Logica del componente, estado, llamadas a servicios y eventos |
| `.html` | Estructura visual de la pantalla |
| `.css` | Estilos propios de esa pantalla |

## Backend

Codigo principal: `backend/`.

| Ruta | Funcion |
|---|---|
| `backend/Program.cs` | Registra servicios, CORS, controladores, Swagger, health check y repositorio |
| `backend/Controllers/` | Recibe las peticiones HTTP |
| `backend/Contracts/` | Define datos que entran y salen de la API |
| `backend/Repositories/IStoreRepository.cs` | Contrato del repositorio usado por los controladores |
| `backend/Repositories/SqlStoreRepository*.cs` | Implementacion SQL Server organizada por dominio |
| `backend/Extensions/ServiceCollectionExtensions.cs` | Configuracion de servicios auxiliares |
| `backend/appsettings.json` | Configuracion general |
| `backend/appsettings.Development.json` | Configuracion para desarrollo |
| `backend/TiendaMicroempresas.Api.csproj` | Dependencias y version del proyecto .NET |

## Base de datos

Ruta principal: `database/sqlserver/`.

| Archivo | Uso |
|---|---|
| `database/sqlserver/docker-init.sql` | Crea o actualiza la base cuando se levanta Docker. Conserva datos existentes cuando es posible |
| `database/sqlserver/init.sql` | Recrea toda la base desde cero. Es util para reinicios completos |
| `database/sqlserver/migrations/2026-05-03-add-product-archive.sql` | Agrega soporte para archivar productos sin perder historial |

Base por defecto: `TiendaMicroempresas`.

| Tabla | Uso |
|---|---|
| `Businesses` | Empresas registradas, perfil publico y credenciales |
| `BusinessSessions` | Tokens activos de empresas |
| `Products` | Productos publicados, stock, precios y estado |
| `Customers` | Compradores registrados |
| `CustomerSessions` | Tokens activos de clientes |
| `Orders` | Encabezado de cada pedido |
| `OrderItems` | Productos incluidos en cada pedido |

## Cuentas empresariales de prueba

Todas usan la clave `Empresa2026!`.

| Correo | Empresa | Ciudad |
|---|---|---|
| `contacto@andespack.co` | Andes Pack Studio | Medellin |
| `direccion@auracafe.co` | Aura Cafe Ejecutivo | Bogota |
| `comercial@lumenverde.co` | Lumen Verde Bienestar | Cali |

## Flujo de comprador

1. Entra a la vitrina publica desde `/`.
2. Revisa empresas y productos.
3. Agrega productos al carrito desde el marketplace, catalogo o perfil de empresa.
4. Puede modificar cantidades en `/carrito` sin tener cuenta.
5. Para entrar al pago en `/pago`, debe registrarse o iniciar sesion como cliente.
6. La compra se envia al backend por `POST /api/orders/checkout`.
7. La API valida sesion, stock, productos publicados y totales.
8. El pedido queda guardado para el cliente y para la empresa.

## Flujo de empresa

1. Entra a `/portal`.
2. Registra la empresa o inicia sesion.
3. Administra datos publicos del negocio.
4. Crea, edita, publica, oculta o archiva productos.
5. Consulta pedidos recibidos.

## Endpoints principales

### Publicos

| Metodo | Ruta | Controlador |
|---|---|---|
| GET | `/api/store/overview` | `backend/Controllers/StoreController.cs` |

### Clientes

| Metodo | Ruta | Controlador |
|---|---|---|
| POST | `/api/customers/register` | `backend/Controllers/CustomersController.cs` |
| POST | `/api/customers/login` | `backend/Controllers/CustomersController.cs` |
| GET | `/api/customers/me` | `backend/Controllers/CustomersController.cs` |
| PUT | `/api/customers/me` | `backend/Controllers/CustomersController.cs` |
| DELETE | `/api/customers/me` | `backend/Controllers/CustomersController.cs` |
| GET | `/api/customers/orders` | `backend/Controllers/CustomersController.cs` |
| POST | `/api/orders/checkout` | `backend/Controllers/OrdersController.cs` |

### Empresas

| Metodo | Ruta | Controlador |
|---|---|---|
| POST | `/api/auth/register` | `backend/Controllers/AuthController.cs` |
| POST | `/api/auth/login` | `backend/Controllers/AuthController.cs` |
| GET | `/api/auth/me` | `backend/Controllers/AuthController.cs` |
| PUT | `/api/auth/me` | `backend/Controllers/AuthController.cs` |
| DELETE | `/api/auth/me` | `backend/Controllers/AuthController.cs` |
| GET | `/api/admin/catalog` | `backend/Controllers/AdminController.cs` |
| POST | `/api/admin/products` | `backend/Controllers/AdminController.cs` |
| PUT | `/api/admin/products/{id}` | `backend/Controllers/AdminController.cs` |
| DELETE | `/api/admin/products/{id}` | `backend/Controllers/AdminController.cs` |
| GET | `/api/admin/orders` | `backend/Controllers/AdminController.cs` |

## Reglas de negocio importantes

- El carrito funciona para usuarios invitados.
- La confirmacion de compra requiere cuenta de cliente.
- Las rutas de cliente privadas usan `X-Customer-Token`.
- Las rutas de empresa privadas usan `X-Owner-Token`.
- Los productos eliminados se archivan para conservar pedidos anteriores.
- El checkout valida inventario, estado del producto y pedido minimo.
- Las claves se guardan con hash PBKDF2.
- Los tokens de sesion se guardan en SQL Server.

## Documentacion adicional

- `docs/ARQUITECTURA.md`: explica capas, comunicacion, seguridad y flujos.
- `docs/MODULOS.md`: detalla carpetas, archivos y responsabilidad de cada parte.
