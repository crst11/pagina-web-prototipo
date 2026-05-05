# Marketplace Multiempresa

Aplicacion web para que cualquier empresario registre su empresa, inicie sesion cuando lo necesite, actualice su perfil publico y publique productos en una vitrina compartida.

## Que hace el proyecto

- Permite registrar multiples empresas con credenciales independientes.
- Guarda los perfiles empresariales y productos en SQL Server.
- Mantiene sesion por token para que cada empresario edite solo su propio catalogo.
- Muestra un marketplace publico con 3 empresas demo profesionales y 9 productos iniciales.
- Permite crear pedidos sin que el comprador tenga que registrarse.

## Estructura del repositorio

- `frontend/`: Angular 21 con la vitrina publica y el portal empresarial.
- `backend/`: ASP.NET Core Web API con autenticacion, productos, pedidos y persistencia SQL Server.
- `database/sqlserver/init.sql`: script que reconstruye la base completa y carga las 3 empresas demo.
- `docs/ARQUITECTURA.md`: guia rapida para revisar responsabilidades, flujos y archivos clave.
- `docs/MODULOS.md`: mapa de modulos para ubicar login, empresas, vitrina/productos y carrito.

## Modulos principales del frontend

- `frontend/src/app/features/auth`: inicio de sesion, registro y portal empresarial.
- `frontend/src/app/features/businesses`: perfiles publicos de empresas.
- `frontend/src/app/features/marketplace`: vitrina publica y productos.
- `frontend/src/app/features/cart`: carrito y checkout multiempresa.
- `frontend/src/app/core`: modelos y servicios compartidos.
- `frontend/src/app/shared`: estilos compartidos.

## Cuentas demo

Clave para todas: `Empresa2026!`

- `contacto@andespack.co` -> Andes Pack Studio
- `direccion@auracafe.co` -> Aura Cafe Ejecutivo
- `comercial@lumenverde.co` -> Lumen Verde Bienestar

## Empresas demo sembradas

### Andes Pack Studio
- Ciudad: Medellin
- Direccion: Cra. 43A #18 Sur-135, El Poblado, Medellin
- Enfoque: empaques corporativos reutilizables y presentacion de marca

### Aura Cafe Ejecutivo
- Ciudad: Bogota
- Direccion: Calle 85 #12-36, Chapinero, Bogota
- Enfoque: coffee breaks, desayunos y hospitalidad ejecutiva

### Lumen Verde Bienestar
- Ciudad: Cali
- Direccion: Avenida 6N #28N-45, Granada, Cali
- Enfoque: bienestar corporativo, fruta fresca y alimentacion saludable

## Flujo recomendado para correrlo

### Opcion recomendada: Docker

Este modo evita problemas de versiones locales, rutas `Path/PATH`, SQL Server instalado en Windows o configuraciones ODBC del equipo.

```powershell
docker compose up --build
```

Si `4200` ya esta ocupado por otro proyecto, usa otro puerto sin tocar el codigo:

```powershell
$env:FRONTEND_PORT=4201
docker compose up --build
```

Servicios:

- Frontend: [http://localhost:4200](http://localhost:4200)
- Backend: [http://localhost:5057](http://localhost:5057)
- Health backend: [http://localhost:5057/health](http://localhost:5057/health)
- SQL Server Docker: `localhost,14333`

Credenciales SQL Server del contenedor:

- Usuario: `sa`
- Clave: `LocalShop2026!Docker`

Para cambiar clave o puertos, copia `.env.example` a `.env` y ajusta `MSSQL_SA_PASSWORD`, `FRONTEND_PORT`, `BACKEND_PORT` o `MSSQL_HOST_PORT`.

El script `database/sqlserver/docker-init.sql` crea la base, aplica cambios faltantes y carga datos demo solo si la base esta vacia.

### Opcion local sin Docker

1. Inicializa la base:

```powershell
sqlcmd -S localhost -E -i database\sqlserver\init.sql
```

Si `sqlcmd` falla por configuracion local, puedes ejecutar el mismo script con PowerShell usando `SqlClient` o `Invoke-Sqlcmd`.

Si ya tienes datos y no quieres recrear la base, aplica la migracion no destructiva:

```powershell
sqlcmd -S localhost -E -i database\sqlserver\migrations\2026-05-03-add-product-archive.sql
```

2. Inicia el backend:

```powershell
cd backend
dotnet run --launch-profile http
```

3. Inicia el frontend:

```powershell
cd frontend
npm start -- --host 0.0.0.0 --port 4200
```

## URLs principales

- Frontend: [http://localhost:4200](http://localhost:4200)
- Marketplace API: [http://localhost:5057/api/store/overview](http://localhost:5057/api/store/overview)
- Login: `POST http://localhost:5057/api/auth/login`
- Registro: `POST http://localhost:5057/api/auth/register`

## Endpoints clave

### Publicos

- `GET /api/store/overview`: devuelve empresas, productos destacados, categorias y totales.
- `POST /api/orders`: crea un pedido para una empresa especifica.

### Empresariales

- `POST /api/auth/register`: crea una nueva empresa y abre sesion.
- `POST /api/auth/login`: inicia sesion.
- `GET /api/auth/me`: devuelve el perfil completo de la empresa autenticada.
- `PUT /api/auth/me`: actualiza el perfil de la empresa autenticada.
- `POST /api/auth/logout`: cierra sesion.
- `GET /api/admin/catalog`: devuelve el perfil autenticado y sus categorias activas.
- `POST /api/admin/products`: crea producto.
- `PUT /api/admin/products/{productId}`: actualiza producto.
- `DELETE /api/admin/products/{productId}`: elimina producto.

## Verificacion realizada

Se verifico manualmente:

- Carga del marketplace desde `GET /api/store/overview`.
- Registro de una empresa temporal.
- Inicio de sesion con empresa demo.
- Lectura y actualizacion de perfil autenticado.
- Creacion y eliminacion de un producto desde la API privada.
- Creacion de un pedido.
- Restauracion final de la base con solo las 3 empresas demo.
- Compilacion de `backend` y `frontend`.

## Notas tecnicas

- El frontend ya no usa `localStorage` para guardar empresas ni productos; solo conserva el token de sesion.
- La sesion empresarial se manda al backend mediante el header `X-Owner-Token`.
- El script `database/sqlserver/init.sql` borra y recrea la estructura para dejar siempre el entorno limpio.
- El script `database/sqlserver/docker-init.sql` es idempotente y esta pensado para Docker.
- Las imagenes de ejemplo viven en `frontend/public/assets/images`.
- El borrado de productos en el backend es logico (`IsArchived`) para no dañar pedidos ya creados.
- El frontend consume `/api`; en Docker lo resuelve Nginx y en desarrollo local lo resuelve `frontend/proxy.conf.json`.

## Estructura

```text
PaginaWeb/
|-- backend/
|   |-- Contracts/          # DTOs y contratos HTTP de la API
|   |-- Controllers/        # Endpoints de auth, vitrina, pedidos y administracion
|   |-- Repositories/       # Acceso a datos SQL Server y runtime demo
|   |-- Dockerfile          # Imagen del backend ASP.NET Core
|   |-- Program.cs          # Configuracion de servicios, CORS, controladores y healthcheck
|   `-- appsettings*.json   # Configuracion local y de desarrollo
|-- database/
|   `-- sqlserver/
|       |-- init.sql        # Script completo para recrear la base local
|       |-- docker-init.sql # Script idempotente usado por Docker
|       `-- migrations/     # Cambios no destructivos de esquema
|-- docs/
|   |-- ARQUITECTURA.md     # Explicacion de capas, flujos y archivos clave
|   |-- MODULOS.md          # Mapa de modulos funcionales
|   |-- ENTREGA.md          # Este resumen de entrega
|   `-- screenshots/        # Capturas reales del sistema
|-- frontend/
|   |-- public/assets/      # Imagenes y recursos estaticos
|   |-- src/app/core/       # Modelos y servicios compartidos
|   |-- src/app/features/   # Modulos: auth, empresas, vitrina y carrito
|   |-- src/app/layout/     # Shell principal de navegacion
|   |-- Dockerfile          # Imagen del frontend Angular
|   |-- nginx.conf          # Servidor SPA y proxy hacia backend
|   `-- proxy.conf.json     # Proxy para desarrollo local
|-- docker-compose.yml      # Orquesta SQL Server, inicializador DB, backend y frontend
|-- .env.example            # Variables de puertos y clave SQL Server
`-- README.md               # Instrucciones principales del proyecto
