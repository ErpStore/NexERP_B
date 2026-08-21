import { Routes } from '@angular/router';

/**
 * One placeholder route, lazily loaded so route-level code splitting is proven
 * from the first commit. No guard: guards arrive with M2-C02.
 */
export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/placeholder/placeholder.component').then((m) => m.PlaceholderComponent),
  },
  { path: '**', redirectTo: '' },
];
