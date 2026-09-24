import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

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
    path: 'admin/login',
    loadComponent: () =>
      import('./features/account/components/login-page/login-page.component').then(
        (m) => m.LoginPageComponent,
      ),
    title: 'Admin Sign In — Terry Corner',
  },
  {
    path: 'account/login',
    redirectTo: 'admin/login',
  },
  {
    path: 'admin',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/admin/components/admin-shell/admin-shell.component').then(
        (m) => m.AdminShellComponent,
      ),
    children: [
      {
        path: '',
        redirectTo: 'products',
        pathMatch: 'full',
      },
      {
        path: 'products',
        loadComponent: () =>
          import(
            './features/admin/components/admin-products-page/admin-products-page.component'
          ).then((m) => m.AdminProductsPageComponent),
        title: 'Manage Meals & Prices — Terry Corner Admin',
      },
      {
        path: 'categories',
        loadComponent: () =>
          import(
            './features/admin/components/admin-categories-page/admin-categories-page.component'
          ).then((m) => m.AdminCategoriesPageComponent),
        title: 'Manage Categories — Terry Corner Admin',
      },
      {
        path: 'toppings',
        loadComponent: () =>
          import(
            './features/admin/components/admin-toppings-page/admin-toppings-page.component'
          ).then((m) => m.AdminToppingsPageComponent),
        title: 'Manage Toppings & Extras — Terry Corner Admin',
      },
      {
        path: 'promotions',
        loadComponent: () =>
          import(
            './features/admin/components/admin-promotions-page/admin-promotions-page.component'
          ).then((m) => m.AdminPromotionsPageComponent),
        title: 'Manage Deals — Terry Corner Admin',
      },
      {
        path: 'settings',
        loadComponent: () =>
          import(
            './features/admin/components/admin-settings-page/admin-settings-page.component'
          ).then((m) => m.AdminSettingsPageComponent),
        title: 'Database Setup — Terry Corner Admin',
      },
    ],
  },
  {
    path: '**',
    redirectTo: '',
  },
];
