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
    path: 'carrito',
    loadComponent: () =>
      import('./features/cart/pages/cart-page/cart-page').then((module) => module.CartPage),
  },
  {
    path: '**',
    redirectTo: '',
  },
];
