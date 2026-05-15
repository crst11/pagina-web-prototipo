# PaginaWebLocalShop - Marketplace Multiempresa (B2B)

Aplicacion web orientada a conectar microempresas con compradores corporativos. Permite registrar empresas, gestionar un catalogo de productos y recibir pedidos en un entorno multiempresa unificado.

## Funcionalidad del Codigo

El sistema opera dividiendo la logica entre un cliente interactivo y un servidor que expone servicios. 
- **Frontend**: Administra las sesiones de usuario en el almacenamiento local del navegador, captura la interaccion del usuario mediante formularios, y gestiona el carrito de compras a nivel local hasta la confirmacion del pedido.
- **Backend**: Actua como guarderia de datos y reglas de negocio. Recibe solicitudes HTTP del cliente, verifica permisos de seguridad, ejecuta validaciones de integridad, y persiste datos de manera permanente.

## Arquitectura del Proyecto (Patron MVC)

El proyecto utiliza el patron Modelo-Vista-Controlador (MVC) implementado en ambas capas de la aplicacion para mantener una separacion de responsabilidades estricta.

### Implementacion MVC en Backend (.NET 8)

- **Modelo**: Constituido por `Repositories/IStoreRepository.cs` y su implementacion `SqlStoreRepository.*.cs`. Define y ejecuta todas las operaciones directas contra SQL Server usando ODBC. Controla las transacciones y la validacion de datos a nivel de base de datos.
- **Vista**: Compuesta por los contratos y DTOs ubicados en el directorio `Contracts/`. Estos objetos representan las estructuras de datos que la API recibe o devuelve via HTTP (JSON).
- **Controlador**: Clases ubicadas en `Controllers/` (ej: `AuthController`, `OrdersController`). Su responsabilidad es interceptar las solicitudes HTTP, validar parametros de entrada, revisar tokens de autorizacion y delegar la logica al repositorio.

### Implementacion MVC en Frontend (Angular 17)

- **Modelo**: Definido por los archivos de interfaces en `core/models/*.ts` y los servicios en `core/services/*.ts`. Gestionan la definicion de tipos de datos, el almacenamiento de estado local (como el token de sesion) y la interaccion con la API REST.
- **Vista**: Correspondiente a las plantillas HTML y hojas de estilo CSS en la raiz de cada componente. Proveen la interfaz visual y capturan los eventos del usuario.
- **Controlador**: Representado por las clases TypeScript de los componentes (`*.ts`). Interceptan las acciones de la vista, invocan a los servicios del modelo, y actualizan las propiedades que alteran la interfaz.

## Interaccion mediante API REST

La comunicacion entre el frontend y el backend se realiza mediante servicios API REST sin estado (stateless).
- El frontend realiza peticiones HTTP (GET, POST, PUT, DELETE) hacia endpoints especificos del backend.
- La transferencia de datos utiliza formato JSON.
- Para operaciones protegidas, el frontend envia tokens de sesion en las cabeceras HTTP (`X-Owner-Token` para cuentas empresariales y `X-Customer-Token` para clientes compradores).
- El backend procesa la peticion y responde con codigos de estado HTTP estandar (ej: 200 OK, 201 Created, 400 Bad Request, 401 Unauthorized) junto con el cuerpo de la respuesta en JSON.

## Estructuras de Datos Especificas

El frontend implementa el uso de estructuras de datos clasicas:

- **Pila (Stack) - LIFO**: Ubicada en `frontend/src/app/core/structures/stack.ts`. Registra el historial de acciones en el carrito de compras, posibilitando una funcion de reversion de cambios (Undo).
- **Cola (Queue) - FIFO**: Ubicada en `frontend/src/app/core/structures/queue.ts`. Administra el sistema de notificaciones de pantalla, garantizando que los mensajes se rendericen en estricto orden cronologico.

## Rutas y Endpoints

### Frontend (Rutas de Navegacion)
Definidas en `frontend/src/app/app.routes.ts`:
- `/`: Vitrina principal publica.
- `/productos`: Listado general de productos destacados.
- `/vitrina`: Catalogo avanzado de productos con opciones de filtrado.
- `/empresas`: Directorio de empresas registradas en la plataforma.
- `/empresa/:slug`: Perfil publico de una empresa en particular.
- `/portal`: Interfaz de autenticacion y administracion privada para empresas.
- `/cliente`: Interfaz de autenticacion e historial de ordenes para compradores.
- `/carrito`: Visualizacion y gestion de los productos pre-seleccionados.
- `/pago`: Vista de confirmacion final e ingreso de datos logisticos (Checkout).

### Backend (Endpoints Principales)
Controladores en `Controllers/`:
- **StoreController** (`GET /api/store/overview`): Suministra el resumen publico de la plataforma.
- **AuthController** (`POST /api/auth/register`, `POST /api/auth/login`): Gestiona la identidad y autenticacion empresarial.
- **AdminController** (`GET /api/admin/catalog`, `POST /api/admin/products`): Facilita la administracion privada del inventario.
- **CustomersController** (`POST /api/customers/register`, `POST /api/customers/login`): Administracion de cuentas y accesos para compradores.
- **OrdersController** (`POST /api/orders/checkout`): Gestiona el procesamiento transaccional del carrito dividiendo ordenes por proveedor.

## Ejecucion del Entorno

La ejecucion local del proyecto esta estandarizada mediante Docker Compose. Este metodo inicializa el frontend, la API y la base de datos SQL Server pre-cargada con datos de demostracion.

1. Iniciar terminal en el directorio principal del proyecto.
2. Ejecutar el comando de construccion y despliegue:
```bash
docker compose up --build -d
```
3. Direcciones de acceso local:
   - Frontend: `http://localhost:4200`
   - Backend API: `http://localhost:5057`
   - SQL Server: `localhost:14333`

### Datos de Prueba Pre-configurados
Las credenciales de acceso para las empresas de prueba son (Contrasena general: `Empresa2026!`):
- contacto@andespack.co
- direccion@auracafe.co
- comercial@lumenverde.co
