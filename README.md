# PaginaWebLocalShop - Marketplace Multiempresa (B2B)

Aplicación web profesional (Marketplace) diseñada para conectar microempresas con compradores corporativos. Permite que cualquier empresario registre su empresa, gestione su catálogo de productos y reciba pedidos en un entorno multiempresa unificado.

## 🏗️ Arquitectura del Proyecto (Patrón MVC)

El proyecto está diseñado bajo una arquitectura limpia utilizando el patrón **Modelo-Vista-Controlador (MVC)**, aplicado y adaptado tanto en el backend como en el frontend para garantizar escalabilidad, separación de responsabilidades y facilidad de mantenimiento.

### Backend (.NET 8)

| Capa MVC | Implementación en el Proyecto | Responsabilidad |
|---|---|---|
| **Modelo** | `Repositories/IStoreRepository.cs` y `SqlStoreRepository.*.cs` | Encapsula el acceso a datos. Define y ejecuta todas las operaciones directas contra SQL Server usando ODBC para máximo rendimiento. Las transacciones y la validación de integridad referencial ocurren aquí. |
| **Vista** | Contratos (`Contracts/`) como DTOs y Requests | Representan las estructuras de datos exactas que la API expone o recibe. Son los objetos inmutables que viajan por HTTP (JSON) definiendo cómo se "ven" los datos para el cliente. |
| **Controlador** | `Controllers/*.cs` (ej: `AuthController`, `OrdersController`) | Reciben las solicitudes HTTP, validan los datos básicos de entrada, autorizan mediante tokens y delegan la lógica pura al Repositorio. No contienen reglas de acceso a datos. |

### Frontend (Angular 17)

| Capa MVC | Implementación en el Proyecto | Responsabilidad |
|---|---|---|
| **Modelo** | `core/models/*.ts` y `core/services/*.ts` | Definen las interfaces TypeScript (tipado fuerte) y los servicios Angular (`Injectables`) que gestionan la comunicación HTTP con el backend y el estado global (ej. sesión en `localStorage`). |
| **Vista** | `*.html` y `*.css` de cada componente | Las plantillas HTML dinámicas que renderizan los datos al usuario. Utilizan un diseño modular basado en "features" (carrito, auth, catálogo). |
| **Controlador** | `*.ts` de cada componente (Clases TypeScript) | Interceptan los eventos del usuario (clics, envíos de formulario), llaman a los servicios (Modelo) y actualizan las propiedades enlazadas a la Vista. |

---

## 🧠 Estructuras de Datos Aplicadas

Para resolver problemas específicos de lógica de negocio y mejorar la experiencia del usuario, el frontend implementa estructuras de datos clásicas:

1. **Pila (Stack) - LIFO (Last-In, First-Out)**
   - **Ubicación:** `frontend/src/app/core/structures/stack.ts`
   - **Uso:** Implementada para registrar el historial de acciones del usuario en el carrito de compras (agregar producto, eliminar producto, cambiar cantidades). Permite la funcionalidad de "Deshacer" (Undo) revirtiendo el estado al paso inmediatamente anterior.

2. **Cola (Queue) - FIFO (First-In, First-Out)**
   - **Ubicación:** `frontend/src/app/core/structures/queue.ts`
   - **Uso:** Implementada para gestionar el sistema de notificaciones en pantalla (Toasts/Alerts). Garantiza que los mensajes de error o éxito se procesen y muestren al usuario en el orden exacto en que ocurrieron.

---

## 🚀 Características Principales

- **Multiempresa (Tenancy):** Registro de múltiples empresas con sesiones, productos y credenciales independientes.
- **Carrito y Checkout Global:** El cliente puede agregar productos de diferentes empresas en un solo carrito; el sistema genera pedidos individuales (split-orders) automáticamente en el backend.
- **Seguridad:** Autenticación por Tokens (`X-Owner-Token` y `X-Customer-Token`). Hasheo de contraseñas con `PBKDF2` y `salt` criptográfico en SQL Server.
- **Borrado Lógico (Soft Bench):** Los productos pueden archivarse para mantener intacto el historial de facturación y pedidos de la empresa.

---

## 🛠️ Stack Tecnológico

- **Frontend:** Angular 17, TypeScript, CSS3 puro (sin frameworks externos para máximo control), Nginx (Proxy Reverso y Servidor SPA).
- **Backend:** C#, .NET 8 (ASP.NET Core Web API).
- **Base de Datos:** SQL Server 2022. Acceso a datos de alto rendimiento mediante ADO.NET (ODBC).
- **Infraestructura:** Docker y Docker Compose para orquestación del entorno de desarrollo.

---

## ⚙️ Guía de Ejecución Rápida (Docker)

La forma recomendada de ejecutar el proyecto es mediante Docker Compose, lo cual despliega la base de datos (con datos y empresas de prueba), el backend y el frontend automáticamente.

1. Asegúrate de tener Docker Desktop iniciado.
2. Abre una terminal en la raíz del proyecto y ejecuta:

```bash
docker compose up --build -d
```

3. El proyecto estará disponible en:
   - **Frontend (Marketplace):** [http://localhost:4200](http://localhost:4200)
   - **Backend (API Base URL):** [http://localhost:5057](http://localhost:5057)
   - **Swagger / Health Check:** [http://localhost:5057/health](http://localhost:5057/health)

*(Nota: El script inicial de Docker crea automáticamente la base de datos e inserta 3 empresas de prueba funcionales).*

---

## 🧪 Datos de Prueba Incluidos

Puedes iniciar sesión en el portal empresarial con cualquiera de las siguientes cuentas pre-configuradas (La contraseña para todas es `Empresa2026!`):

- **Andes Pack Studio:** `contacto@andespack.co`
- **Aura Café Ejecutivo:** `direccion@auracafe.co`
- **Lumen Verde Bienestar:** `comercial@lumenverde.co`
