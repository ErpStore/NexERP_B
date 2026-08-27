import { inject } from '@angular/core';
import type { CanActivateFn } from '@angular/router';
import { Router } from '@angular/router';

import type { Right } from './auth.models';
import { AuthService } from './auth.service';

/**
 * No session → redirect to `/login?returnUrl=<attempted path>`. Waits for
 * {@link AuthService.whenBootstrapped} first, so a route activated during the app-start
 * bootstrap sees the settled answer, not a stale `'unknown'`.
 */
export const authGuard: CanActivateFn = async (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  await auth.whenBootstrapped();

  if (auth.status() === 'authenticated') {
    return true;
  }

  return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};

/**
 * **Factory** returning a `CanActivateFn`, attached per-route as `canActivate:
 * [requireScreen('Sales Order', 'view')]`. RENDERING-ONLY GATE — see
 * `permission.service.ts`'s class doc comment; the server re-checks independently on every
 * request (ADR-004 §3).
 *
 * **What this guard decides, and what it deliberately does not.** `CanActivateFn` can only
 * return `true`, `false`, or a `UrlTree` — there is no fourth option that means "activate,
 * but render something else." So the split here is: this guard answers exactly one
 * question, *is there a session at all* (redirects to `/login` if not, same as
 * {@link authGuard}) — it **never** blocks (`false`) or redirects for a missing right, both
 * of which this task's own Target Result rules out (KB-050 §Error handling, the 403 row:
 * "not a redirect and not a logout"). The route always activates for an authenticated
 * caller. **The missing-right *rendering* is the routed component's job**, via
 * `PermissionService.forScreen(screen)`/`*appHasRight` composed with
 * `PermissionDeniedStateComponent` in its own template — the same rendering-only read every
 * other consumer of this service does, not a special case invented for guards. This is the
 * pattern every screen attaching from `M2-D01` onward follows; the one placeholder route
 * this task ships is the first, working example of it.
 *
 * Deny-by-default: a screen with no matching `UserRight` row resolves `false` for every
 * right (matching `RightsHelper.cs`). `hidden`/`IsHide` plays no part in this decision — it
 * is a navigation-listing hint only (see `auth.models.ts`'s `ScreenRight` doc comment); a
 * screen with `view: true, hidden: true` still activates and renders normally.
 */
export function requireScreen(screen: string, right: Right): CanActivateFn {
  // Deliberately unused today — see the doc comment above for why this guard never reads
  // them. Kept as named parameters so the call-site contract (`requireScreen('Sales Order',
  // 'view')`) stays self-documenting, and so a future, deliberate decision to enforce here
  // does not have to change every route declaration's signature.
  void screen;
  void right;
  return async () => {
    const auth = inject(AuthService);
    const router = inject(Router);

    await auth.whenBootstrapped();

    if (auth.status() !== 'authenticated') {
      return router.createUrlTree(['/login']);
    }

    return true;
  };
}
