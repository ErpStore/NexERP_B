/**
 * LEARNING — Functional route guards
 * ----------------------------------
 * A guard returns true / UrlTree to allow or redirect navigation.
 * Prefer functional guards (`CanActivateFn`) over class-based guards in modern Angular.
 */
import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  if (auth.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/login']);
};
