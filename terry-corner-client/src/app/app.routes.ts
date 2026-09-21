import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/home/components/home-page/home-page.component').then(
        (m) => m.HomePageComponent,
      ),
    title: 'Terry Corner — Made For The Crave',
  },
  {
    path: 'menu',
    loadComponent: () =>
      import('./features/menu/components/menu-page/menu-page.component').then(
        (m) => m.MenuPageComponent,
      ),
    title: 'Menu — Terry Corner',
  },
  {
    path: 'cart',
    loadComponent: () =>
      import('./features/cart/components/cart-page/cart-page.component').then(
        (m) => m.CartPageComponent,
      ),
    title: 'Your Tray — Terry Corner',
  },
  {
    path: '**',
    redirectTo: '',
  },
];
