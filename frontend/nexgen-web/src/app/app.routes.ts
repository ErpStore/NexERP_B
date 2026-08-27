import { Routes } from '@angular/router';

import { authGuard, requireScreen } from './core/auth/auth.guard';

/**
 * M2-C02 — `/login` is public; everything else sits behind `authGuard`. The former
 * unguarded placeholder route now also carries `requireScreen(...)`, proving the pattern
 * every screen attaching from `M2-D01` onward follows — see `auth.guard.ts`'s doc comment
 * for why the missing-right *rendering* is the routed component's own job, not the guard's.
 */
export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./features/auth/login/login.component').then((m) => m.LoginComponent),
  },
  {
    path: '',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        canActivate: [requireScreen('Dashboard', 'view')],
        loadComponent: () =>
          import('./features/placeholder/placeholder.component').then(
            (m) => m.PlaceholderComponent,
          ),
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
