import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/marketplace/pages/marketplace-page/marketplace-page').then(
        (module) => module.MarketplacePage,
      ),
  },
  {
    path: 'productos',
    loadComponent: () =>
      import('./features/marketplace/pages/products-page/products-page').then((module) => module.ProductsPage),
  },
  {
    path: 'vitrina',
    loadComponent: () =>
      import('./features/marketplace/pages/catalog-page/catalog-page').then((module) => module.CatalogPage),
  },
  {
    path: 'empresas',
    loadComponent: () =>
      import('./features/marketplace/pages/businesses-page/businesses-page').then((module) => module.BusinessesPage),
  },
  {
    path: 'empresa/:slug',
    loadComponent: () =>
      import('./features/businesses/pages/business-profile-page/business-profile-page').then(
        (module) => module.BusinessProfilePage,
      ),
  },
  {
    path: 'portal',
    loadComponent: () =>
      import('./features/auth/pages/portal-page/portal-page').then((module) => module.PortalPage),
  },
  {
    path: 'cliente',
    loadComponent: () =>
      import('./features/customers/pages/customer-account-page/customer-account-page').then(
        (module) => module.CustomerAccountPage,
      ),
  },
  {
    path: 'carrito',
    loadComponent: () =>
      import('./features/cart/pages/cart-page/cart-page').then((module) => module.CartPage),
  },
  {
    path: 'pago',
    loadComponent: () =>
      import('./features/cart/pages/payment-page/payment-page').then((module) => module.PaymentPage),
  },
  {
    path: '**',
    redirectTo: '',
  },
];
