import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  afterNextRender,
  computed,
  effect,
  inject,
} from '@angular/core';
import { NavigationStart, Router, RouterOutlet } from '@angular/router';
import { Toast } from 'primeng/toast';

import { AuthService } from './core/auth/auth.service';
import { IdleTimeoutService } from './core/auth/idle-timeout.service';
import { TOAST_POLITE_PASS_THROUGH } from './shared/components/feedback/toast.service';
import { ConfirmDialogComponent } from './shared/components/overlay/confirm-dialog.component';
import { ConfirmDialogService } from './shared/components/overlay/confirm-dialog.service';
// Direct import (R-69 discipline, see confirmHostRequested's doc comment below) — this is
// the same @defer-gated pattern the confirm-dialog host already uses, so idle-warning's
// PrimeNG dialog dependency also leaves the initial chunk.
import { IdleWarningComponent } from './features/auth/idle-warning/idle-warning.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Toast, ConfirmDialogComponent, IdleWarningComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppComponent {
  /**
   * KB-051 Responsive behaviour treats `<768` as read-and-approve, where the
   * primary action sits at the bottom of the screen - which is exactly where a
   * bottom-right toast would land. Below the breakpoint the stack moves to the
   * top and spans the width instead.
   */
  readonly toastBreakpoints = { '768px': { top: '0', right: '0', left: '0' } };

  /** Polite, not assertive - see TOAST_POLITE_PASS_THROUGH. */
  readonly toastPassThrough = TOAST_POLITE_PASS_THROUGH;

  /**
   * The `@defer` trigger for the confirm-dialog host (M2-C13, R-69). Latches
   * `true` on the first `ConfirmDialogService.confirm()` and stays true, so the
   * single host mounts once and lives for the rest of the session. The service
   * queues that first request until the host has subscribed - see
   * `confirm-dialog.service.ts`.
   *
   * The imports above are deliberately **direct file imports**, never the
   * `shared/components` barrel: R-69 measured the barrel dragging every form
   * control and `decimal.js` into the initial chunk, taking it to 1.31 MB and
   * failing the build outright. Do not "tidy" them into one.
   */
  readonly confirmHostRequested = inject(ConfirmDialogService).hostRequested;

  readonly #authStatus = inject(AuthService).status;

  /** `true` once `status()` has ever reached `'authenticated'`, and latched from then on —
   * the idle-warning host's own `@defer` trigger, same shape as `confirmHostRequested` above
   * and for the same reason (R-69): an anonymous caller on `/login` never needs the dialog's
   * PrimeNG dependency. */
  readonly idleWarningRequested = computed(() => this.#authStatus() === 'authenticated');

  constructor() {
    const auth = inject(AuthService);
    const idleTimeout = inject(IdleTimeoutService);
    const router = inject(Router);
    const destroyRef = inject(DestroyRef);

    // M2-C02 — activity resets, wired once. `afterNextRender` because these are plain DOM
    // listeners, not signal-driven; `document`/`window` are unavailable during SSR (this
    // workspace has none today, but the guard costs nothing and avoids a future trap).
    afterNextRender(() => {
      const reset = () => idleTimeout.recordActivity();
      document.addEventListener('pointerdown', reset);
      document.addEventListener('keydown', reset);
      document.addEventListener('visibilitychange', reset);
      const routeSub = router.events.subscribe((event) => {
        if (event instanceof NavigationStart) {
          reset();
        }
      });

      destroyRef.onDestroy(() => {
        document.removeEventListener('pointerdown', reset);
        document.removeEventListener('keydown', reset);
        document.removeEventListener('visibilitychange', reset);
        routeSub.unsubscribe();
      });
    });

    // Starts the clock the moment a session exists, stops it the moment it doesn't — so an
    // anonymous caller on `/login` never runs a timer that could sign out... nothing.
    let started = false;
    effect(() => {
      if (auth.status() === 'authenticated' && !started) {
        started = true;
        idleTimeout.start(() => {
          void auth.logout().then(() => {
            void router.navigate(['/login'], { queryParams: { reason: 'idle-timeout' } });
          });
        });
      } else if (auth.status() !== 'authenticated' && started) {
        started = false;
        idleTimeout.stop();
      }
    });
  }
}
