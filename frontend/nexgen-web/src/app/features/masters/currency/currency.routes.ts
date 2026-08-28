import type { Routes } from '@angular/router';

/**
 * M2-D01 — [KB-053](../../../../../../docs/kb/frontend-new/page-map.md) Masters — Accounts:
 * `/masters/currencies` (list), `/masters/currencies/new` (create),
 * `/masters/currencies/:id` (edit). All three are lazily loaded from `app.routes.ts` via
 * `loadChildren` on this file, so the feature is its own chunk.
 *
 * **The drawer is route-addressable, resolving KB-052 ("List + Drawer form") against
 * KB-053 (dedicated routes) explicitly** — `new`/`:id` render the *same* `CurrencyListComponent`
 * as `''`, with `data.drawerMode` (bound as a component input by `withComponentInputBinding()`,
 * `app.config.ts`) telling it to open the drawer over the grid rather than mounting a second
 * routed component. `new` is listed before `:id` so it is never captured by the parameter route.
 *
 * **A real, disclosed trade-off, recorded in the Slice review rather than assumed away:**
 * because these are three separate route configs (not nested children of one another),
 * Angular's default `RouteReuseStrategy` does not reuse the component instance between them —
 * opening or closing the drawer tears down and rebuilds `CurrencyListComponent`, including its
 * `DataGridQueryState`, which reissues the list request from the URL's query params. This is
 * the same request the same URL would produce on a fresh load, so it is not incorrect, only an
 * extra round-trip the shared grid-state mechanism does not currently avoid.
 */
export const currencyRoutes: Routes = [
  {
    path: '',
    data: { breadcrumb: 'Currencies' },
    loadComponent: () =>
      import('./pages/currency-list/currency-list.component').then((m) => m.CurrencyListComponent),
  },
  {
    path: 'new',
    data: { breadcrumb: 'Currencies', drawerMode: 'create' },
    loadComponent: () =>
      import('./pages/currency-list/currency-list.component').then((m) => m.CurrencyListComponent),
  },
  {
    path: ':id',
    data: { breadcrumb: 'Currencies', drawerMode: 'edit' },
    loadComponent: () =>
      import('./pages/currency-list/currency-list.component').then((m) => m.CurrencyListComponent),
  },
];
