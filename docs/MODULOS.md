# Guia de modulos

Esta guia sirve para ubicar rapidamente cada carpeta y archivo importante del proyecto. Esta organizada por frontend, backend, base de datos y Docker.

## Raiz del proyecto

```text
PaginaWeb/
|-- frontend/
|-- backend/
|-- database/
|-- docs/
|-- docker-compose.yml
|-- .env.example
|-- .gitignore
|-- .dockerignore
|-- PaginaWeb.sln
`-- README.md
```

| Ruta | Para que sirve |
|---|---|
| `frontend/` | Contiene la aplicacion Angular que ve el usuario |
| `backend/` | Contiene la API REST en ASP.NET Core |
| `database/` | Contiene scripts SQL Server |
| `docs/` | Contiene documentacion tecnica del proyecto |
| `docker-compose.yml` | Levanta SQL Server, backend, frontend e inicializador de base |
| `.env.example` | Ejemplo de variables de entorno |
| `PaginaWeb.sln` | Solucion .NET para abrir o compilar el backend |
| `README.md` | Guia general del proyecto |

## Frontend

Raiz: `frontend/`.

```text
frontend/
|-- public/
|-- src/
|-- angular.json
|-- package.json
|-- package-lock.json
|-- Dockerfile
|-- nginx.conf
|-- proxy.conf.json
|-- tsconfig.json
|-- tsconfig.app.json
`-- tsconfig.spec.json
```

| Ruta | Para que sirve |
|---|---|
| `frontend/angular.json` | Configuracion del proyecto Angular, estilos, build y assets |
| `frontend/package.json` | Scripts `npm`, dependencias y herramientas de frontend |
| `frontend/package-lock.json` | Versiones exactas instaladas por npm |
| `frontend/Dockerfile` | Compila Angular y deja el resultado listo para Nginx |
| `frontend/nginx.conf` | Sirve la SPA y redirecciona `/api` al backend |
| `frontend/proxy.conf.json` | Proxy usado cuando se ejecuta Angular en desarrollo |
| `frontend/tsconfig.json` | Configuracion TypeScript general |
| `frontend/tsconfig.app.json` | Configuracion TypeScript de la app |
| `frontend/tsconfig.spec.json` | Configuracion TypeScript de pruebas |

## Frontend: assets

Ruta: `frontend/public/`.

| Ruta | Para que sirve |
|---|---|
| `frontend/public/favicon.ico` | Icono de la pestana del navegador |
| `frontend/public/assets/images/` | Imagenes usadas por la pagina |
| `frontend/public/assets/images/localshop-logo.png` | Logo principal |
| `frontend/public/assets/images/localshop-logo-small.png` | Logo compacto |
| `frontend/public/assets/images/banner-*.jpg` | Banners de empresas |
| `frontend/public/assets/images/banner-*.png` | Versiones alternativas de banners |
| `frontend/public/assets/images/store*.png` | Imagenes de tiendas |
| `frontend/public/assets/images/pla*.png` | Imagenes de productos |
| `frontend/public/assets/images/*.svg` | Iconos usados por la interfaz |

## Frontend: entrada de la aplicacion

| Ruta | Para que sirve |
|---|---|
| `frontend/src/main.ts` | Arranca Angular y monta la aplicacion |
| `frontend/src/index.html` | HTML base donde Angular inyecta la app |
| `frontend/src/styles.css` | Estilos globales y carga de estilos compartidos |
| `frontend/src/app/app.config.ts` | Configura providers de Angular y cliente HTTP |
| `frontend/src/app/app.routes.ts` | Define todas las rutas navegables |
| `frontend/src/app/app.spec.ts` | Prueba base del proyecto Angular |

## Frontend: layout

Ruta: `frontend/src/app/layout/`.

| Archivo | Para que sirve |
|---|---|
| `app-shell.ts` | Componente principal. Mantiene estado de navegacion y datos visibles en la cabecera |
| `app-shell.html` | Estructura del layout, menu y `router-outlet` |
| `app-shell.css` | Estilos del layout principal |

## Frontend: modelos

Ruta: `frontend/src/app/core/models/`.

| Archivo | Para que sirve |
|---|---|
| `auth.models.ts` | Define datos de empresa, sesion de empresa y respuestas de autenticacion empresarial |
| `cart.models.ts` | Define item de carrito, datos de envio, agrupacion por empresa y checkout |
| `customer.models.ts` | Define cliente, sesion de cliente, perfil e historial de compras |
| `marketplace.models.ts` | Define empresas, productos, categorias y respuesta de vitrina |
| `order.models.ts` | Define pedidos e items vistos desde empresa o comprador |

## Frontend: servicios

Ruta: `frontend/src/app/core/services/`.

| Archivo | Para que sirve |
|---|---|
| `auth.service.ts` | Registro, login, perfil, actualizacion y eliminacion de cuenta empresarial |
| `customer.service.ts` | Registro, login, perfil, historial, actualizacion y eliminacion de cuenta de cliente |
| `marketplace.service.ts` | Consulta datos publicos del marketplace desde la API |
| `cart.service.ts` | Agrega productos, cambia cantidades, elimina items, guarda datos de envio y maneja carrito invitado |
| `order.service.ts` | Envia el checkout y consulta pedidos recibidos por la empresa |
| `product.service.ts` | Crea, actualiza, publica, oculta y archiva productos |
| `session.service.ts` | Centraliza tokens de cliente y empresa en `localStorage` |

## Frontend: validadores

Ruta: `frontend/src/app/core/validators/`.

| Archivo | Para que sirve |
|---|---|
| `form.validators.ts` | Valida correo, telefono, ciudad, direccion, nombres, contrasenas y formularios del sistema |

Los formularios usan estos validadores para evitar repetir reglas en cada pantalla.

## Frontend: marketplace

Ruta principal: `frontend/src/app/features/marketplace/`.

```text
features/marketplace/
|-- pages/
|   |-- marketplace-page/
|   |-- products-page/
|   |-- catalog-page/
|   `-- businesses-page/
|-- panels/
|   |-- home-panel/
|   |-- products-panel/
|   |-- catalog-panel/
|   `-- businesses-panel/
`-- services/
    `-- marketplace-page-state.service.ts
```

### Paginas del marketplace

| Ruta | Para que sirve |
|---|---|
| `frontend/src/app/features/marketplace/pages/marketplace-page/` | Pagina inicial. Reune secciones principales de la vitrina |
| `frontend/src/app/features/marketplace/pages/products-page/` | Pantalla de productos destacados |
| `frontend/src/app/features/marketplace/pages/catalog-page/` | Catalogo general con filtros y busqueda |
| `frontend/src/app/features/marketplace/pages/businesses-page/` | Listado de empresas disponibles |

Cada carpeta de pagina contiene:

| Archivo | Uso |
|---|---|
| `*.ts` | Logica, carga de datos y eventos |
| `*.html` | Vista de la pagina |
| `*.css` | Estilos propios de la pagina |

### Paneles del marketplace

| Ruta | Para que sirve |
|---|---|
| `frontend/src/app/features/marketplace/panels/home-panel/` | Panel de inicio reutilizable |
| `frontend/src/app/features/marketplace/panels/products-panel/` | Panel de productos |
| `frontend/src/app/features/marketplace/panels/catalog-panel/` | Panel de catalogo |
| `frontend/src/app/features/marketplace/panels/businesses-panel/` | Panel de empresas |
| `frontend/src/app/features/marketplace/services/marketplace-page-state.service.ts` | Estado compartido entre paginas y paneles del marketplace |

## Frontend: perfil publico de empresa

Ruta: `frontend/src/app/features/businesses/pages/business-profile-page/`.

| Archivo | Para que sirve |
|---|---|
| `business-profile-page.ts` | Busca la empresa por `slug`, prepara productos y maneja agregar al carrito |
| `business-profile-page.html` | Muestra banner, datos de la empresa, productos y acciones del usuario |
| `business-profile-page.css` | Estilos del perfil publico |

Esta pantalla corresponde a la ruta `/empresa/:slug`.

## Frontend: portal empresarial

Ruta: `frontend/src/app/features/auth/pages/portal-page/`.

| Archivo | Para que sirve |
|---|---|
| `portal-page.ts` | Maneja registro, login, perfil, productos, pedidos y cierre de sesion empresarial |
| `portal-page.html` | Formularios y paneles del portal de empresa |
| `portal-page.css` | Estilos del portal empresarial |

Servicios relacionados:

| Ruta | Uso |
|---|---|
| `frontend/src/app/core/services/auth.service.ts` | Sesion y perfil de empresa |
| `frontend/src/app/core/services/product.service.ts` | Administracion de productos |
| `frontend/src/app/core/services/order.service.ts` | Pedidos recibidos |
| `frontend/src/app/core/services/session.service.ts` | Token `X-Owner-Token` |

Backend relacionado:

| Ruta | Uso |
|---|---|
| `backend/Controllers/AuthController.cs` | Registro, login y perfil empresarial |
| `backend/Controllers/AdminController.cs` | Productos y pedidos de empresa |
| `backend/Repositories/SqlStoreRepository.Auth.cs` | Persistencia de empresa y sesion |
| `backend/Repositories/SqlStoreRepository.Products.cs` | Persistencia de productos |
| `backend/Repositories/SqlStoreRepository.Orders.cs` | Consulta de pedidos recibidos |

## Frontend: cuenta del comprador

Ruta: `frontend/src/app/features/customers/pages/customer-account-page/`.

| Archivo | Para que sirve |
|---|---|
| `customer-account-page.ts` | Registro, login, perfil, historial, edicion y eliminacion de cuenta |
| `customer-account-page.html` | Formularios y vista de historial del comprador |
| `customer-account-page.css` | Estilos de la cuenta del comprador |

Servicios relacionados:

| Ruta | Uso |
|---|---|
| `frontend/src/app/core/services/customer.service.ts` | Sesion, perfil e historial |
| `frontend/src/app/core/services/cart.service.ts` | Fusiona carrito invitado al iniciar sesion |
| `frontend/src/app/core/services/session.service.ts` | Token `X-Customer-Token` |

Backend relacionado:

| Ruta | Uso |
|---|---|
| `backend/Controllers/CustomersController.cs` | Endpoints de clientes |
| `backend/Repositories/SqlStoreRepository.Customers.cs` | Persistencia de clientes y sesiones |

## Frontend: carrito

Ruta: `frontend/src/app/features/cart/pages/cart-page/`.

| Archivo | Para que sirve |
|---|---|
| `cart-page.ts` | Muestra items, calcula totales, cambia cantidades y prepara datos de envio |
| `cart-page.html` | Vista del carrito, cantidades, resumen y formulario de entrega |
| `cart-page.css` | Estilos de carrito |

Servicios relacionados:

| Ruta | Uso |
|---|---|
| `frontend/src/app/core/services/cart.service.ts` | Estado del carrito |
| `frontend/src/app/core/services/customer.service.ts` | Verifica si el cliente tiene sesion |

Regla principal: se pueden agregar productos sin sesion, pero para comprar se debe iniciar sesion o registrarse como cliente.

## Frontend: pago

Ruta: `frontend/src/app/features/cart/pages/payment-page/`.

| Archivo | Para que sirve |
|---|---|
| `payment-page.ts` | Valida sesion de cliente, prepara resumen y llama al checkout |
| `payment-page.html` | Vista de confirmacion, metodo de pago y resumen del pedido |
| `payment-page.css` | Estilos de la pantalla de pago |

Servicios relacionados:

| Ruta | Uso |
|---|---|
| `frontend/src/app/core/services/order.service.ts` | Envia `POST /api/orders/checkout` |
| `frontend/src/app/core/services/cart.service.ts` | Lee carrito y limpia compra finalizada |
| `frontend/src/app/core/services/customer.service.ts` | Confirma sesion del cliente |

Backend relacionado:

| Ruta | Uso |
|---|---|
| `backend/Controllers/OrdersController.cs` | Recibe el checkout |
| `backend/Repositories/SqlStoreRepository.Orders.cs` | Valida stock, crea pedidos y descuenta inventario |

## Frontend: estilos compartidos

Ruta: `frontend/src/app/shared/styles/marketplace-shared.css`.

Este archivo concentra estilos reutilizables:

| Grupo | Uso |
|---|---|
| Layout | Contenedores, secciones y espacios |
| Navegacion | Cabeceras, menus y enlaces |
| Botones | Botones principales, secundarios y estados |
| Formularios | Inputs, labels, campos requeridos y mensajes |
| Tarjetas | Productos, empresas y resumenes |
| Estados | Alertas, errores, exito, vacios y carga |
| Grillas | Distribucion de productos, empresas y paneles |

Cuando un estilo se repite en varias pantallas debe ir aqui. Cuando solo aplica a una pantalla debe quedarse en el `.css` de esa pantalla.

## Backend

Raiz: `backend/`.

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
|-- Dockerfile
|-- TiendaMicroempresas.Api.csproj
`-- TiendaMicroempresas.Api.http
```

| Ruta | Para que sirve |
|---|---|
| `backend/Program.cs` | Punto de entrada de la API |
| `backend/appsettings.json` | Configuracion general |
| `backend/appsettings.Development.json` | Configuracion local |
| `backend/Dockerfile` | Publica la API para Docker |
| `backend/TiendaMicroempresas.Api.csproj` | Proyecto .NET y dependencias |
| `backend/TiendaMicroempresas.Api.http` | Archivo para probar endpoints desde editores compatibles |
| `backend/Properties/launchSettings.json` | Perfiles de ejecucion local |

## Backend: controladores

Ruta: `backend/Controllers/`.

| Archivo | Rutas | Para que sirve |
|---|---|---|
| `StoreController.cs` | `/api/store/overview` | Entrega empresas y productos publicos |
| `AuthController.cs` | `/api/auth/register`, `/api/auth/login`, `/api/auth/me` | Maneja sesion empresarial |
| `CustomersController.cs` | `/api/customers/register`, `/api/customers/login`, `/api/customers/me`, `/api/customers/orders` | Maneja clientes |
| `AdminController.cs` | `/api/admin/catalog`, `/api/admin/products`, `/api/admin/orders` | Maneja datos privados de empresa |
| `OrdersController.cs` | `/api/orders/checkout` | Registra compras |

## Backend: contratos

Ruta: `backend/Contracts/`.

### Auth

| Archivo | Para que sirve |
|---|---|
| `backend/Contracts/Auth/AuthResponse.cs` | Respuesta al iniciar sesion o registrarse como empresa |
| `backend/Contracts/Auth/LoginOwnerRequest.cs` | Datos para login empresarial |
| `backend/Contracts/Auth/RegisterOwnerRequest.cs` | Datos para crear empresa |
| `backend/Contracts/Auth/UpdateOwnerProfileRequest.cs` | Datos para editar perfil empresarial |

### Customers

| Archivo | Para que sirve |
|---|---|
| `backend/Contracts/Customers/CustomerAuthResponse.cs` | Respuesta de login o registro de cliente |
| `backend/Contracts/Customers/CustomerDto.cs` | Datos publicos y privados del cliente autenticado |
| `backend/Contracts/Customers/CustomerOrderDto.cs` | Pedido dentro del historial del cliente |
| `backend/Contracts/Customers/CustomerOrdersHistoryResponse.cs` | Respuesta del historial de compras |
| `backend/Contracts/Customers/LoginCustomerRequest.cs` | Datos para login de cliente |
| `backend/Contracts/Customers/RegisterCustomerRequest.cs` | Datos para registro de cliente |
| `backend/Contracts/Customers/UpdateCustomerProfileRequest.cs` | Datos para editar perfil de cliente |

### Orders

| Archivo | Para que sirve |
|---|---|
| `backend/Contracts/Orders/BusinessCheckoutResultDto.cs` | Resultado del checkout agrupado por empresa |
| `backend/Contracts/Orders/BusinessOrderDto.cs` | Pedido visto desde el portal empresarial |
| `backend/Contracts/Orders/BusinessOrderItemDto.cs` | Item de pedido visto por la empresa |
| `backend/Contracts/Orders/BusinessOrdersFeedResponse.cs` | Lista de pedidos recibidos por empresa |
| `backend/Contracts/Orders/CheckoutCartItemRequest.cs` | Item enviado desde el carrito |
| `backend/Contracts/Orders/CheckoutCartRequest.cs` | Request completo de checkout |
| `backend/Contracts/Orders/CheckoutCartResponse.cs` | Respuesta de checkout |
| `backend/Contracts/Orders/CreateOrderRequest.cs` | Request para crear pedido |
| `backend/Contracts/Orders/OrderCreatedResponse.cs` | Respuesta al crear un pedido |
| `backend/Contracts/Orders/OrderItemRequest.cs` | Item usado para crear un pedido |

### Products

| Archivo | Para que sirve |
|---|---|
| `backend/Contracts/Products/AdminCatalogResponse.cs` | Catalogo privado de una empresa |
| `backend/Contracts/Products/FeaturedProductDto.cs` | Producto destacado en la vitrina |
| `backend/Contracts/Products/ProductDto.cs` | Producto completo usado por API y frontend |
| `backend/Contracts/Products/UpsertProductRequest.cs` | Datos para crear o editar producto |

### Store

| Archivo | Para que sirve |
|---|---|
| `backend/Contracts/Store/MarketplaceBusinessDto.cs` | Empresa mostrada en el marketplace |
| `backend/Contracts/Store/SetupStatusResponse.cs` | Estado inicial de configuracion |
| `backend/Contracts/Store/StoreOverviewResponse.cs` | Respuesta completa de la vitrina publica |

## Backend: repositorios

Ruta: `backend/Repositories/`.

| Archivo | Para que sirve |
|---|---|
| `IStoreRepository.cs` | Interfaz que define las operaciones de datos |
| `SqlStoreRepository.cs` | Clase principal del repositorio SQL |
| `SqlStoreRepository.Auth.cs` | Empresa, login empresarial, perfil y sesiones |
| `SqlStoreRepository.Customers.cs` | Cliente, login, perfil, sesiones e historial |
| `SqlStoreRepository.Products.cs` | Productos, stock, visibilidad y archivado |
| `SqlStoreRepository.Orders.cs` | Checkout, creacion de pedidos, transacciones y pedidos recibidos |
| `SqlStoreRepository.Queries.cs` | Consultas publicas del marketplace |
| `SqlStoreRepository.Helpers.cs` | Utilidades de hash, slugs, validacion y SQL |

## Backend: extensiones

Ruta: `backend/Extensions/`.

| Archivo | Para que sirve |
|---|---|
| `ServiceCollectionExtensions.cs` | Agrupa registro de servicios usados por la API |

## Base de datos

Raiz: `database/sqlserver/`.

```text
database/sqlserver/
|-- docker-init.sql
|-- init.sql
`-- migrations/
    `-- 2026-05-03-add-product-archive.sql
```

| Ruta | Para que sirve |
|---|---|
| `database/sqlserver/docker-init.sql` | Script de arranque para Docker. Crea base, tablas, columnas necesarias y datos demo |
| `database/sqlserver/init.sql` | Script completo para reconstruir la base desde cero |
| `database/sqlserver/migrations/2026-05-03-add-product-archive.sql` | Migracion que agrega archivado de productos |

## Base de datos: tablas

| Tabla | Que guarda |
|---|---|
| `Businesses` | Empresas, credenciales, datos publicos, ciudad, categoria, banner y configuracion |
| `BusinessSessions` | Tokens activos para empresas |
| `Products` | Productos, precio, stock, imagen, categoria, estado publicado y archivado |
| `Customers` | Compradores, credenciales y datos de contacto |
| `CustomerSessions` | Tokens activos para clientes |
| `Orders` | Pedido, cliente, empresa, envio, metodo de pago, total y estado |
| `OrderItems` | Items de cada pedido, cantidad, precio y subtotal |

## Docker

| Ruta | Para que sirve |
|---|---|
| `docker-compose.yml` | Define servicios `sqlserver`, `database-init`, `backend` y `frontend` |
| `frontend/Dockerfile` | Compila Angular y copia archivos al contenedor Nginx |
| `frontend/nginx.conf` | Sirve la aplicacion y redirige `/api` |
| `backend/Dockerfile` | Restaura, compila y publica la API |
| `.dockerignore` | Evita copiar archivos innecesarios al contexto Docker |
| `backend/.dockerignore` | Ignora archivos internos del backend en Docker |
| `frontend/.dockerignore` | Ignora archivos internos del frontend en Docker |

## Rutas frecuentes para modificar

| Cambio necesario | Ruta principal |
|---|---|
| Cambiar campos obligatorios de formularios | `frontend/src/app/core/validators/form.validators.ts` y el `.html` de la pantalla |
| Cambiar asteriscos o estilo de campos requeridos | `frontend/src/app/shared/styles/marketplace-shared.css` |
| Cambiar registro de cliente | `frontend/src/app/features/customers/pages/customer-account-page/` y `backend/Controllers/CustomersController.cs` |
| Cambiar registro de empresa | `frontend/src/app/features/auth/pages/portal-page/` y `backend/Controllers/AuthController.cs` |
| Cambiar carrito | `frontend/src/app/features/cart/pages/cart-page/` y `frontend/src/app/core/services/cart.service.ts` |
| Cambiar checkout | `frontend/src/app/features/cart/pages/payment-page/`, `frontend/src/app/core/services/order.service.ts`, `backend/Controllers/OrdersController.cs` y `backend/Repositories/SqlStoreRepository.Orders.cs` |
| Cambiar productos | `frontend/src/app/core/services/product.service.ts`, `backend/Controllers/AdminController.cs` y `backend/Repositories/SqlStoreRepository.Products.cs` |
| Cambiar marketplace publico | `frontend/src/app/features/marketplace/`, `backend/Controllers/StoreController.cs` y `backend/Repositories/SqlStoreRepository.Queries.cs` |
| Cambiar estructura de base de datos | `database/sqlserver/docker-init.sql`, `database/sqlserver/init.sql` y `database/sqlserver/migrations/` |

## Comandos utiles

Compilar frontend:

```powershell
cd frontend
npm run build
```

Compilar backend:

```powershell
dotnet build
```

Levantar todo con Docker:

```powershell
docker compose up --build
```

Reconstruir manteniendo `localhost:4200`:

```powershell
docker compose up --build -d frontend
```

Revisar contenedores:

```powershell
docker compose ps -a
```

Ver logs:

```powershell
docker compose logs --tail 120
```
