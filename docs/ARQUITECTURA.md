# Arquitectura y Revision Rapida

## Resumen

El proyecto esta dividido en dos capas:

- `frontend/`: interfaz publica y portal empresarial.
- `backend/`: API REST que persiste empresas, productos, sesiones y pedidos en SQL Server.

La idea central es simple: una empresa se registra, el backend le entrega un token, ese token permite administrar solo su perfil y sus productos, y la vitrina publica consume un resumen agregado de todas las empresas publicadas.

## Flujo de datos

1. El frontend carga `GET /api/store/overview`.
2. El backend arma un snapshot del marketplace con empresas, productos destacados y categorias.
3. Cuando un empresario se registra o inicia sesion, el backend devuelve `token + business`.
4. El frontend guarda solo el token en `localStorage`.
5. Para editar perfil o productos, el frontend manda `X-Owner-Token` al backend.
6. Los pedidos publicos se crean con `POST /api/orders` y descuentan inventario.

## Archivos clave del frontend

### `frontend/src/app/app.routes.ts`

Mapa de rutas de Angular.

- Usa `loadComponent` para cargar cada modulo solo cuando se visita su ruta.
- Apunta a marketplace, portal, perfiles de empresa y carrito.

### `frontend/src/app/features/marketplace/pages/marketplace-page`

Modulo de vitrina publica y productos.

- Carga el marketplace.
- Filtra por busqueda y categoria.
- Muestra empresas y productos destacados.
- Permite agregar productos al carrito.

### `frontend/src/app/features/auth/pages/portal-page`

Modulo de inicio de sesion y portal empresarial.

- Registra empresas.
- Inicia y cierra sesion.
- Edita el perfil publico.
- Administra productos propios.
- Muestra pedidos recibidos.

### `frontend/src/app/features/businesses/pages/business-profile-page`

Modulo de empresas.

- Muestra el perfil publico de una empresa por `slug`.
- Lista sus productos publicados.
- Agrega productos de esa empresa al carrito.

### `frontend/src/app/features/cart/pages/cart-page`

Modulo de carrito.

- Agrupa productos por empresa.
- Valida minimos de compra.
- Envia pedidos multiempresa con un formulario.

### `frontend/src/app/core/services/store.service.ts`

Puente entre Angular y la API.

- Llama a los endpoints HTTP.
- Guarda y limpia el token de sesion.
- Convierte errores HTTP en mensajes legibles.

### `frontend/src/app/core/services/cart.service.ts`

Estado compartido del carrito.

- Guarda productos agregados.
- Calcula cantidad y total.
- Muestra avisos temporales cuando el carrito cambia.

### `frontend/src/app/core/models/store.models.ts`

Contrato tipado compartido por la UI.

- Define empresas, productos, pedidos y payloads del portal.

### `frontend/src/app/shared/styles/marketplace-shared.css`

Estilos compartidos.

- Controla botones, tarjetas, grillas, feedback, toast y layout comun.
- Esta registrado como estilo global en `frontend/angular.json`.

## Archivos clave del backend

### `backend/Program.cs`

Arranque de ASP.NET Core.

- Configura CORS para el frontend local.
- Registra controladores.
- Inyecta `SqlStoreRepository` como repositorio principal.

### `backend/Repositories/IStoreRepository.cs`

Contrato de la capa de datos.

- Expone operaciones para overview, autenticacion, perfil, productos y pedidos.

### `backend/Repositories/SqlStoreRepository.cs`

Implementacion principal contra SQL Server.

- Registra empresas con hash PBKDF2.
- Crea y valida sesiones.
- Construye el snapshot del marketplace.
- Restringe el CRUD de productos a la empresa autenticada.
- Crea pedidos con transaccion y descuenta inventario.

### `backend/Controllers/AuthController.cs`

Endpoints de autenticacion.

- Registro
- Login
- Perfil autenticado
- Logout

### `backend/Controllers/AdminController.cs`

Endpoints privados del portal empresarial.

- Lectura de catalogo propio
- Crear, editar y eliminar productos

### `backend/Controllers/StoreController.cs`

Endpoint publico del marketplace.

- `GET /api/store/overview`

### `backend/Controllers/OrdersController.cs`

Endpoint publico de pedidos.

- Valida datos del comprador
- Crea el pedido para una empresa concreta

## Base de datos

### `database/sqlserver/init.sql`

Reconstruye todo el entorno.

Tablas principales:

- `Businesses`: perfil empresarial y credenciales
- `BusinessSessions`: sesiones activas por token
- `Products`: catalogo por empresa
- `Orders`: encabezado del pedido
- `OrderItems`: detalle del pedido

La tabla `Products` usa `IsArchived` para eliminar productos del catalogo sin romper pedidos antiguos. La vitrina, el portal y el checkout ignoran productos archivados, pero `OrderItems` conserva la referencia historica.

## Seguridad y reglas importantes

- Los correos de empresa son unicos.
- Cada token identifica una sola empresa.
- Un empresario solo puede editar o borrar productos propios.
- El pedido valida empresa, inventario y pedido minimo antes de confirmar.
- Los productos ocultos (`IsPublished = 0`) no salen en la vitrina publica.
- Los productos eliminados quedan archivados (`IsArchived = 1`) para proteger el historial de pedidos.
- Al consultar pedidos empresariales, `IsNew` se devuelve al frontend y luego se marca como visto en base de datos.

## Como revisar rapidamente si algo falla

### Si la vitrina no carga

- Verifica `http://localhost:5057/api/store/overview`.
- Si falla, reinicia backend y valida la base con `database/sqlserver/init.sql`.

### Si el portal no deja iniciar sesion

- Revisa que exista token en `localStorage` solo despues del login.
- Revisa `POST /api/auth/login`.
- Si la data quedo inconsistente, vuelve a correr `init.sql`.

### Si un producto no aparece en la vitrina

- Confirma que tenga `isPublished = true`.
- Confirma que la empresa exista y el producto tenga stock y categoria validos.

## Estado base esperado despues del script

- 3 empresas demo
- 9 productos publicados
- 0 pedidos
- 0 sesiones activas
