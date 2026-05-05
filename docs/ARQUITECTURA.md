# Arquitectura

Este documento explica como se organiza LocalShop por capas, como viajan los datos entre frontend, backend y base de datos, y donde se encuentra cada parte tecnica del proyecto.

## Capas principales

| Capa | Ruta | Responsabilidad |
|---|---|---|
| Frontend | `frontend/` | Interfaz de usuario, rutas, formularios, carrito, sesion local y llamadas HTTP |
| Backend | `backend/` | API REST, autenticacion, reglas de negocio, validaciones y acceso a datos |
| Base de datos | `database/sqlserver/` | Tablas, datos iniciales, sesiones, pedidos y migraciones |
| Docker | `docker-compose.yml` | Orquesta frontend, backend, SQL Server e inicializador de base |

## Comunicacion entre capas

```text
Navegador
  |
  | http://localhost:4200
  v
Angular servido por Nginx
  |
  | /api/*
  v
ASP.NET Core Web API
  |
  | SqlConnection
  v
SQL Server - TiendaMicroempresas
```

En Docker, `frontend/nginx.conf` sirve la aplicacion Angular y redirige las rutas `/api` al servicio `backend`. Por eso la web se mantiene en `http://localhost:4200` aunque la API este en otro contenedor.

## Archivos de arranque

| Ruta | Funcion |
|---|---|
| `frontend/src/main.ts` | Inicia Angular |
| `frontend/src/app/app.config.ts` | Registra configuracion de Angular, HTTP y providers |
| `frontend/src/app/app.routes.ts` | Define las pantallas disponibles |
| `frontend/src/app/layout/app-shell.ts` | Contenedor principal de la aplicacion |
| `backend/Program.cs` | Inicia la API, registra controladores, CORS, Swagger, health check y repositorio |
| `backend/Extensions/ServiceCollectionExtensions.cs` | Separa parte de la configuracion de servicios |
| `docker-compose.yml` | Define contenedores, puertos, variables y dependencias |
| `database/sqlserver/docker-init.sql` | Prepara la base usada por Docker |

## Frontend Angular

Raiz del codigo: `frontend/src/app/`.

```text
frontend/src/app/
|-- app.config.ts
|-- app.routes.ts
|-- core/
|   |-- models/
|   |-- services/
|   `-- validators/
|-- features/
|   |-- auth/
|   |-- businesses/
|   |-- cart/
|   |-- customers/
|   `-- marketplace/
|-- layout/
`-- shared/
```

### Rutas de navegacion

Las rutas estan en `frontend/src/app/app.routes.ts`.

| Ruta web | Carpeta del componente | Uso |
|---|---|---|
| `/` | `frontend/src/app/features/marketplace/pages/marketplace-page/` | Inicio del marketplace |
| `/productos` | `frontend/src/app/features/marketplace/pages/products-page/` | Productos destacados |
| `/vitrina` | `frontend/src/app/features/marketplace/pages/catalog-page/` | Catalogo general con filtros |
| `/empresas` | `frontend/src/app/features/marketplace/pages/businesses-page/` | Listado de empresas |
| `/empresa/:slug` | `frontend/src/app/features/businesses/pages/business-profile-page/` | Perfil publico de una empresa |
| `/portal` | `frontend/src/app/features/auth/pages/portal-page/` | Portal empresarial |
| `/cliente` | `frontend/src/app/features/customers/pages/customer-account-page/` | Cuenta del comprador |
| `/carrito` | `frontend/src/app/features/cart/pages/cart-page/` | Carrito de compras |
| `/pago` | `frontend/src/app/features/cart/pages/payment-page/` | Confirmacion del pedido |

### Core del frontend

`frontend/src/app/core/` contiene codigo compartido que no pertenece a una sola pantalla.

| Ruta | Funcion |
|---|---|
| `frontend/src/app/core/models/auth.models.ts` | Tipos de empresa, sesion empresarial y respuestas de autenticacion |
| `frontend/src/app/core/models/cart.models.ts` | Item de carrito, datos de envio y datos de checkout |
| `frontend/src/app/core/models/customer.models.ts` | Cliente, sesion de cliente y compras del cliente |
| `frontend/src/app/core/models/marketplace.models.ts` | Empresas, productos y respuesta publica del marketplace |
| `frontend/src/app/core/models/order.models.ts` | Pedidos para empresa y cliente |
| `frontend/src/app/core/services/auth.service.ts` | Registro, login, perfil y cierre de sesion empresarial |
| `frontend/src/app/core/services/customer.service.ts` | Registro, login, perfil e historial del comprador |
| `frontend/src/app/core/services/marketplace.service.ts` | Carga datos publicos desde `/api/store/overview` |
| `frontend/src/app/core/services/cart.service.ts` | Maneja carrito invitado y carrito de cliente |
| `frontend/src/app/core/services/order.service.ts` | Envia checkout y consulta pedidos empresariales |
| `frontend/src/app/core/services/product.service.ts` | Crea, edita y archiva productos desde el portal |
| `frontend/src/app/core/services/session.service.ts` | Guarda y lee tokens en `localStorage` |
| `frontend/src/app/core/validators/form.validators.ts` | Reglas reutilizables para formularios |

### Features del frontend

| Ruta | Responsabilidad |
|---|---|
| `frontend/src/app/features/marketplace/` | Vitrina publica, catalogo, productos, empresas y paneles reutilizables |
| `frontend/src/app/features/businesses/` | Perfil publico de una empresa por `slug` |
| `frontend/src/app/features/auth/` | Portal de administracion empresarial |
| `frontend/src/app/features/customers/` | Cuenta, login, registro e historial del comprador |
| `frontend/src/app/features/cart/` | Carrito y pagina de pago |

### Layout y estilos

| Ruta | Funcion |
|---|---|
| `frontend/src/app/layout/app-shell.ts` | Logica del contenedor principal |
| `frontend/src/app/layout/app-shell.html` | Menu, navegacion y salida de rutas |
| `frontend/src/app/layout/app-shell.css` | Estilos del contenedor general |
| `frontend/src/app/shared/styles/marketplace-shared.css` | Botones, formularios, mensajes, grillas, tarjetas y componentes compartidos |
| `frontend/src/styles.css` | Estilos globales de la aplicacion |
| `frontend/public/assets/images/` | Imagenes de productos, banners, logo e iconos |

## Backend ASP.NET Core

Raiz del codigo: `backend/`.

```text
backend/
|-- Program.cs
|-- Controllers/
|-- Contracts/
|-- Extensions/
|-- Repositories/
|-- Properties/
|-- appsettings.json
|-- appsettings.Development.json
`-- TiendaMicroempresas.Api.csproj
```

### Controladores

Los controladores reciben peticiones HTTP, validan datos basicos y llaman a `IStoreRepository`.

| Ruta | Endpoints | Responsabilidad |
|---|---|---|
| `backend/Controllers/StoreController.cs` | `/api/store/overview` | Datos publicos del marketplace |
| `backend/Controllers/AuthController.cs` | `/api/auth/*` | Registro, login y perfil de empresa |
| `backend/Controllers/CustomersController.cs` | `/api/customers/*` | Registro, login, perfil e historial del comprador |
| `backend/Controllers/AdminController.cs` | `/api/admin/*` | Catalogo propio, productos y pedidos de empresa |
| `backend/Controllers/OrdersController.cs` | `/api/orders/*` | Creacion de pedidos y checkout |

### Contratos

`backend/Contracts/` define los objetos que usa la API para recibir y responder datos.

| Ruta | Contenido |
|---|---|
| `backend/Contracts/Auth/` | Requests y responses del portal empresarial |
| `backend/Contracts/Customers/` | Requests, responses, perfil e historial del comprador |
| `backend/Contracts/Orders/` | Requests de checkout, items de pedido y respuestas de ordenes |
| `backend/Contracts/Products/` | DTOs de producto y requests para crear o actualizar |
| `backend/Contracts/Store/` | Respuesta del marketplace publico |

### Repositorio SQL

El backend usa la interfaz `backend/Repositories/IStoreRepository.cs`. La implementacion real es `SqlStoreRepository`, separada en archivos parciales para no mezclar dominios.

| Ruta | Funcion |
|---|---|
| `backend/Repositories/IStoreRepository.cs` | Define las operaciones que pueden usar los controladores |
| `backend/Repositories/SqlStoreRepository.cs` | Clase principal, conexion y tipos internos |
| `backend/Repositories/SqlStoreRepository.Auth.cs` | Registro, login, token y perfil empresarial |
| `backend/Repositories/SqlStoreRepository.Customers.cs` | Registro, login, token, perfil e historial de clientes |
| `backend/Repositories/SqlStoreRepository.Products.cs` | Crear, editar, publicar, ocultar y archivar productos |
| `backend/Repositories/SqlStoreRepository.Orders.cs` | Checkout, creacion de pedidos, inventario y pedidos recibidos |
| `backend/Repositories/SqlStoreRepository.Queries.cs` | Consultas publicas del marketplace |
| `backend/Repositories/SqlStoreRepository.Helpers.cs` | Hash de claves, slugs, validaciones y utilidades SQL |

## Base de datos SQL Server

Ruta: `database/sqlserver/`.

| Ruta | Funcion |
|---|---|
| `database/sqlserver/docker-init.sql` | Script usado por el contenedor `database-init`. Crea tablas y datos si hacen falta |
| `database/sqlserver/init.sql` | Script completo para crear la base desde cero |
| `database/sqlserver/migrations/2026-05-03-add-product-archive.sql` | Migracion para archivar productos |

### Tablas principales

| Tabla | Uso en la aplicacion |
|---|---|
| `Businesses` | Perfil publico, credenciales y configuracion comercial de empresas |
| `BusinessSessions` | Tokens `X-Owner-Token` de empresas |
| `Products` | Catalogo, stock, precio, imagenes, visibilidad y archivo |
| `Customers` | Datos del comprador y credenciales |
| `CustomerSessions` | Tokens `X-Customer-Token` de clientes |
| `Orders` | Datos generales del pedido, cliente, empresa, entrega y pago |
| `OrderItems` | Productos, cantidades y precios asociados a cada pedido |

## Flujo de compra

1. El usuario entra a `/`, `/vitrina`, `/productos`, `/empresas` o `/empresa/:slug`.
2. Las pantallas publicas usan `frontend/src/app/core/services/marketplace.service.ts`.
3. El servicio llama a `GET /api/store/overview`.
4. La API entra por `backend/Controllers/StoreController.cs`.
5. El controlador usa `IStoreRepository.GetStoreOverviewAsync`.
6. La consulta se resuelve en `backend/Repositories/SqlStoreRepository.Queries.cs`.
7. El comprador agrega productos al carrito.
8. El carrito se maneja en `frontend/src/app/core/services/cart.service.ts`.
9. Si no hay sesion, el carrito queda como carrito invitado en `sessionStorage`.
10. Para pagar, la pantalla `frontend/src/app/features/cart/pages/payment-page/` exige sesion de cliente.
11. La sesion de cliente se maneja con `frontend/src/app/core/services/customer.service.ts` y `session.service.ts`.
12. El pago llama a `frontend/src/app/core/services/order.service.ts`.
13. `order.service.ts` envia `POST /api/orders/checkout` con `X-Customer-Token`.
14. La API recibe en `backend/Controllers/OrdersController.cs`.
15. El checkout se ejecuta en `backend/Repositories/SqlStoreRepository.Orders.cs`.
16. La base guarda registros en `Orders` y `OrderItems` y descuenta stock en `Products`.

## Flujo de empresa

1. La empresa entra a `/portal`.
2. La pantalla vive en `frontend/src/app/features/auth/pages/portal-page/`.
3. El registro y login se hacen desde `frontend/src/app/core/services/auth.service.ts`.
4. La API recibe en `backend/Controllers/AuthController.cs`.
5. La persistencia se ejecuta en `backend/Repositories/SqlStoreRepository.Auth.cs`.
6. El token empresarial se guarda en `BusinessSessions`.
7. El frontend conserva el token mediante `frontend/src/app/core/services/session.service.ts`.
8. Las acciones privadas usan el header `X-Owner-Token`.
9. Productos y pedidos pasan por `backend/Controllers/AdminController.cs`.
10. Productos se manejan en `SqlStoreRepository.Products.cs`.
11. Pedidos recibidos se consultan desde `SqlStoreRepository.Orders.cs`.

## Seguridad

| Tema | Implementacion |
|---|---|
| Claves | PBKDF2 con salt en `backend/Repositories/SqlStoreRepository.Helpers.cs` |
| Sesion empresarial | Header `X-Owner-Token`, tabla `BusinessSessions` |
| Sesion cliente | Header `X-Customer-Token`, tabla `CustomerSessions` |
| Checkout | Requiere token de cliente en `OrdersController` |
| Portal empresarial | Requiere token de empresa en `AuthController` y `AdminController` |
| CORS | Configurado desde `backend/Program.cs` y extensiones |

## Manejo de eliminaciones

| Accion | Comportamiento |
|---|---|
| Archivar producto | Marca el producto como archivado para que no se venda, conservando historial |
| Eliminar empresa | Oculta productos y mantiene pedidos existentes |
| Eliminar cliente | Conserva pedidos para la empresa, pero retira la asociacion activa del cliente |

## Despliegue local

| Servicio | Puerto local | Definicion |
|---|---|---|
| Frontend | `4200` | `docker-compose.yml`, servicio `frontend` |
| Backend | `5057` | `docker-compose.yml`, servicio `backend` |
| SQL Server | `14333` | `docker-compose.yml`, servicio `sqlserver` |
| Inicializador SQL | Sin puerto | `docker-compose.yml`, servicio `database-init` |

Comando recomendado para aplicar cambios:

```powershell
docker compose up --build -d frontend
```

Este comando mantiene la pagina en `http://localhost:4200`.
