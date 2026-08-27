import { Injectable, inject } from '@angular/core';

import { AuthService } from './auth.service';

/**
 * M2-C02 — the single-flight half of the 401 refresh. Three concurrent 401s must produce
 * exactly **one** `POST /api/v1/auth/refresh` call, with all three waiters sharing its
 * result — not three independent refreshes racing to rotate the same one-time-use refresh
 * token out from under each other (`RefreshTokenService.RotateAsync`, M2-A04: the second
 * caller to present an already-rotated token gets `Revoked`, not a fresh pair).
 *
 * A plain cached `Promise`, not an RxJS `Subject`/`shareReplay` — `AuthService.refreshTokens`
 * is already `Promise`-based (via `firstValueFrom`), and a promise is trivially "share this
 * one in-flight call with late subscribers" by construction: every `await` on the same
 * promise instance observes the same resolution, no operator needed.
 */
@Injectable({ providedIn: 'root' })
export class RefreshCoordinator {
  readonly #auth = inject(AuthService);

  #inFlight: Promise<boolean> | null = null;

  refresh(): Promise<boolean> {
    if (!this.#inFlight) {
      this.#inFlight = this.#auth.refreshTokens().finally(() => {
        this.#inFlight = null;
      });
    }
    return this.#inFlight;
  }
}
