import { Routes } from '@angular/router';

import { authGuard, requireScreen } from './core/auth/auth.guard';

/**
 * M2-C03 — the real layout tree, replacing `M2-C02`'s bare `authGuard` route and its
 * throwaway placeholder child. `auth-layout` wraps `/login` (public); `shell` wraps every
 * authenticated route under `authGuard`. `print-layout` exists (`layout/print-layout/`) but
 * is not yet attached to a route — nothing prints anything yet; it is built ahead of the
 * first task that needs it, per KB-050 §Project structure, not wired to a placeholder
 * destination.
 *
 * Every authenticated leaf route also carries `requireScreen(screen, right)` — that guard
 * only gates authentication (see `auth.guard.ts`'s own doc comment for why), so the
 * missing-*right* rendering is each routed component's own job, demonstrated today by
 * `DashboardComponent`. Each leaf's `data.breadcrumb` feeds `BreadcrumbsComponent`, which
 * derives the trail from route data rather than a per-page input.
 */
export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./layout/auth-layout/auth-layout.component').then((m) => m.AuthLayoutComponent),
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/auth/login/login.component').then((m) => m.LoginComponent),
      },
    ],
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/shell/shell.component').then((m) => m.ShellComponent),
    children: [
      {
        path: '',
        canActivate: [requireScreen('Dashboard', 'view')],
        data: { breadcrumb: 'Dashboard' },
        loadComponent: () =>
          import('./features/dashboard/dashboard.component').then((m) => m.DashboardComponent),
      },
      // M2-D01 — the first vertical slice. requireScreen only gates authentication (see this
      // file's own doc comment); CurrencyListComponent renders the missing-view-right surface
      // itself, the same pattern DashboardComponent established.
      {
        path: 'masters/currencies',
        canActivate: [requireScreen('Currency', 'view')],
        loadChildren: () =>
          import('./features/masters/currency/currency.routes').then((m) => m.currencyRoutes),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
