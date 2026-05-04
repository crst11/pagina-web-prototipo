# Frontend LocalShop

Aplicacion Angular 21 organizada por modulos funcionales para facilitar mantenimiento.

## Como correr

Con Docker desde la raiz del proyecto:

```powershell
docker compose up --build
```

Modo local:

```powershell
npm install
npm start -- --host 0.0.0.0 --port 4200
```

URL local: `http://localhost:4200`.

En modo local, `proxy.conf.json` envia `/api` al backend en `http://localhost:5057`.

## Como compilar

```powershell
npm run build
```

El resultado queda en `dist/`, que es generado y no debe versionarse.

## Estructura

- `src/app/layout`: shell raiz y `RouterOutlet`.
- `src/app/core`: modelos y servicios compartidos.
- `src/app/shared`: estilos compartidos.
- `src/app/features/auth`: login, registro y portal empresarial.
- `src/app/features/businesses`: perfiles publicos de empresas.
- `src/app/features/marketplace`: vitrina publica y productos.
- `src/app/features/cart`: carrito y checkout multiempresa.

## Rutas

- `/`: vitrina publica.
- `/portal`: login, registro y administracion empresarial.
- `/empresa/:slug`: perfil publico de empresa.
- `/carrito`: carrito multiempresa.

Las rutas usan `loadComponent` en `src/app/app.routes.ts`, por eso cada pantalla se carga separada.

## Archivos importantes

- `src/app/core/services/store.service.ts`: llamadas HTTP y token de sesion.
- `src/app/core/services/cart.service.ts`: estado compartido del carrito.
- `src/app/core/models/store.models.ts`: tipos de empresas, productos, pedidos y formularios.
- `src/app/shared/styles/marketplace-shared.css`: estilos comunes registrados en `angular.json`.

Para una guia mas completa, revisa `../docs/MODULOS.md`.
