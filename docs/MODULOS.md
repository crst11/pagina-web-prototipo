# Modulos del proyecto

Esta guia muestra donde encontrar cada parte funcional del sistema despues de la separacion por modulos.

## Frontend Angular

Ruta base: `frontend/src/app`.

### `layout`

Contiene el contenedor raiz de la aplicacion.

- `layout/app-shell.ts`: monta el `RouterOutlet`.
- `layout/app-shell.html`: punto donde Angular cambia de pagina segun la ruta.
- `layout/app-shell.css`: estilos minimos del contenedor raiz.

### `core`

Codigo compartido que no pertenece a una pantalla especifica.

- `core/models/store.models.ts`: interfaces de empresas, productos, carrito, pedidos y payloads HTTP.
- `core/services/store.service.ts`: comunicacion con la API, token de sesion y manejo de errores.
- `core/services/cart.service.ts`: estado del carrito multiempresa y notificaciones de carrito.

Regla de mantenimiento: si algo se usa en varias paginas, va en `core`. Si solo lo usa una pantalla, queda dentro de su `feature`.

### `shared`

Recursos visuales compartidos.

- `shared/styles/marketplace-shared.css`: clases comunes de layout, botones, tarjetas, grillas, feedback y toast.

Este CSS esta registrado en `angular.json` como estilo global para evitar duplicarlo en cada componente.

### `features/marketplace`

Modulo de vitrina publica y productos.

- `features/marketplace/pages/marketplace-page/marketplace-page.ts`
- `features/marketplace/pages/marketplace-page/marketplace-page.html`

Responsabilidades:

- Cargar `GET /api/store/overview`.
- Mostrar productos destacados y productos filtrados.
- Buscar por texto y filtrar por categoria.
- Abrir perfiles de empresa.
- Agregar productos al carrito.

Ruta: `/`.

### `features/auth`

Modulo de inicio de sesion, registro y portal empresarial.

- `features/auth/pages/portal-page/portal-page.ts`
- `features/auth/pages/portal-page/portal-page.html`
- `features/auth/pages/portal-page/portal-page.css`

Responsabilidades:

- Registrar empresas.
- Iniciar y cerrar sesion empresarial.
- Editar perfil publico de la empresa autenticada.
- Crear, editar y eliminar productos propios.
- Ver pedidos recibidos por la empresa.

Ruta: `/portal`.

### `features/businesses`

Modulo de empresas y perfil publico.

- `features/businesses/pages/business-profile-page/business-profile-page.ts`
- `features/businesses/pages/business-profile-page/business-profile-page.html`
- `features/businesses/pages/business-profile-page/business-profile-page.css`

Responsabilidades:

- Leer el `slug` de la URL.
- Buscar la empresa dentro del snapshot del marketplace.
- Mostrar datos publicos, contacto y catalogo publicado.
- Agregar productos de esa empresa al carrito.

Ruta: `/empresa/:slug`.

### `features/cart`

Modulo de carrito y checkout multiempresa.

- `features/cart/pages/cart-page/cart-page.ts`
- `features/cart/pages/cart-page/cart-page.html`
- `features/cart/pages/cart-page/cart-page.css`

Responsabilidades:

- Agrupar productos por empresa.
- Validar pedidos minimos por empresa.
- Enviar checkout con un solo formulario.
- Limpiar carrito despues de compra exitosa.

Ruta: `/carrito`.

## Rutas y carga diferida

Las rutas viven en `frontend/src/app/app.routes.ts`.

Cada pagina se carga con `loadComponent`, asi Angular separa el bundle por funcionalidad:

- `marketplace-page`
- `portal-page`
- `business-profile-page`
- `cart-page`

Esto facilita mantenimiento porque cada modulo se puede revisar sin abrir toda la aplicacion.

## Backend ASP.NET Core

Ruta base: `backend`.

- `Controllers/AuthController.cs`: registro, login, perfil autenticado y logout.
- `Controllers/AdminController.cs`: administracion privada de catalogo y pedidos empresariales.
- `Controllers/StoreController.cs`: vitrina publica del marketplace.
- `Controllers/OrdersController.cs`: checkout y creacion de pedidos.
- `Contracts/`: DTOs de entrada y salida de la API.
- `Repositories/IStoreRepository.cs`: contrato de persistencia.
- `Repositories/SqlStoreRepository.cs`: implementacion contra SQL Server.
- `Repositories/DemoStoreRuntime.cs`: soporte para datos demo si no hay conexion disponible.

## Coordinacion con la base de datos

- `database/sqlserver/init.sql` crea las mismas tablas que consume `SqlStoreRepository`.
- `database/sqlserver/docker-init.sql` hace lo mismo para Docker, pero sin borrar datos existentes.
- `database/sqlserver/migrations/2026-05-03-add-product-archive.sql` actualiza bases existentes sin borrar datos.
- Empresas y sesiones usan `Businesses` y `BusinessSessions`.
- La vitrina lee `Businesses` + `Products`, excluyendo productos archivados.
- El portal administra productos propios por token `X-Owner-Token`.
- El checkout crea `Orders` y `OrderItems` dentro de una transaccion y descuenta inventario.
- El borrado de productos es logico: marca `Products.IsArchived = 1`, oculta el producto y conserva pedidos historicos.
- El panel empresarial lee `Orders.IsNew` para contar pedidos nuevos y despues los marca como vistos con `ViewedAt`.

Si vas a conservar una base ya creada, ejecuta primero la migracion. Si puedes reconstruir datos demo desde cero, usa `init.sql`.

## Docker

La raiz del proyecto contiene:

- `docker-compose.yml`: levanta SQL Server, inicializa la base, publica backend y sirve frontend.
- `backend/Dockerfile`: compila .NET 10 e instala ODBC Driver 18 para SQL Server.
- `frontend/Dockerfile`: compila Angular con Node 24 LTS y sirve archivos con Nginx.
- `frontend/nginx.conf`: sirve la SPA y redirige `/api` al backend.
- `.env.example`: documenta clave y puertos configurables para evitar choques con otros proyectos.
- `.env.example`: clave demo del SQL Server del contenedor.

Comando principal:

```powershell
docker compose up --build
```

Puertos configurables:

- `FRONTEND_PORT`: por defecto `4200`.
- `BACKEND_PORT`: por defecto `5057`.
- `MSSQL_HOST_PORT`: por defecto `14333`.

## Archivos que no son codigo fuente

Estos elementos se regeneran y no se deben versionar:

- `frontend/node_modules/`
- `frontend/dist/`
- `frontend/.angular/`
- `frontend/*.log`
- `frontend/*preview.png`
- `backend/bin/`
- `backend/obj/`
- `backend/build-check/`

La raiz del proyecto tiene `.gitignore` para mantenerlos fuera del repositorio.
