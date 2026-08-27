import { DestroyRef, Injectable, InjectionToken, inject, signal } from '@angular/core';

/** Configuration, not a literal (Target Result / Acceptance Criteria). Override with
 * `{ provide: IDLE_TIMEOUT_MINUTES, useValue: N }` in `app.config.ts` if the default needs
 * to change; nothing in this service hardcodes a duration. */
export const IDLE_TIMEOUT_MINUTES = new InjectionToken<number>('IDLE_TIMEOUT_MINUTES', {
  providedIn: 'root',
  factory: () => 30,
});

/** How long before expiry the warning dialog appears, with a live countdown. */
export const IDLE_WARNING_SECONDS = new InjectionToken<number>('IDLE_WARNING_SECONDS', {
  providedIn: 'root',
  factory: () => 60,
});

/**
 * M2-C02 — the R-17 replacement. `SessionTimeoutService.cs` is a Blazor `AddSingleton` with
 * one shared `_lastActivity` field, so **every concurrent user shares one idle clock**
 * (Confirmed, `V.SMART/V.SMART.Shared/Services/SessionTimeoutService.cs:11`). This service is
 * deliberately the opposite shape: `@Injectable({ providedIn: 'root' })` gives each **tab**
 * its own instance with its own instance fields (not static/module state), so two tabs — or
 * two users on the same browser at different times — never observe or reset each other's
 * clock. The regression test (`idle-timeout.service.spec.ts`) constructs two instances
 * directly and asserts exactly that.
 *
 * **A UX behaviour, not a security control.** The server's own token expiry
 * (`Jwt:ExpiresMinutes`, M2-A04) is the actual bound on how long a session can be used; this
 * timer only makes an *unattended browser tab* sign itself out sooner than that, for privacy
 * on a shared machine. It is not a substitute for server-side expiry and must not be treated
 * as one.
 *
 * Wiring activity resets (`pointerdown`, `keydown`, `visibilitychange`, route change) is the
 * caller's job — see `app.component.ts` — so this service stays framework-router-agnostic
 * and unit-testable with fake timers alone.
 */
@Injectable({ providedIn: 'root' })
export class IdleTimeoutService {
  readonly #timeoutMinutes = inject(IDLE_TIMEOUT_MINUTES);
  readonly #warningSeconds = inject(IDLE_WARNING_SECONDS);
  readonly #destroyRef = inject(DestroyRef);

  readonly #isWarning = signal(false);
  readonly #secondsRemaining = signal(0);

  readonly isWarning = this.#isWarning.asReadonly();
  readonly secondsRemaining = this.#secondsRemaining.asReadonly();

  #warnTimer: ReturnType<typeof setTimeout> | null = null;
  #countdownTimer: ReturnType<typeof setInterval> | null = null;
  #onExpire: (() => void) | null = null;
  #running = false;

  /** Starts the clock. `onExpire` fires exactly once, when the countdown reaches zero without
   * `staySignedIn()` having been called — the caller (bootstrap) wires it to a hard logout
   * plus a reason shown on the login page. */
  start(onExpire: () => void): void {
    this.#onExpire = onExpire;
    this.#running = true;
    this.#scheduleWarning();
    this.#destroyRef.onDestroy(() => this.stop());
  }

  stop(): void {
    this.#running = false;
    this.#clearTimers();
  }

  /** Any user activity — resets the clock and dismisses the warning if it was showing. */
  recordActivity(): void {
    if (!this.#running) {
      return;
    }
    this.#clearTimers();
    this.#isWarning.set(false);
    this.#scheduleWarning();
  }

  /** The dialog's "Stay signed in" action — identical to any other activity. */
  staySignedIn(): void {
    this.recordActivity();
  }

  #scheduleWarning(): void {
    const warnAfterMs = Math.max(0, this.#timeoutMinutes * 60_000 - this.#warningSeconds * 1000);
    this.#warnTimer = setTimeout(() => this.#beginWarning(), warnAfterMs);
  }

  #beginWarning(): void {
    this.#isWarning.set(true);
    this.#secondsRemaining.set(this.#warningSeconds);
    this.#countdownTimer = setInterval(() => {
      const next = this.#secondsRemaining() - 1;
      if (next <= 0) {
        this.#clearTimers();
        this.#isWarning.set(false);
        this.#onExpire?.();
        return;
      }
      this.#secondsRemaining.set(next);
    }, 1000);
  }

  #clearTimers(): void {
    if (this.#warnTimer !== null) {
      clearTimeout(this.#warnTimer);
      this.#warnTimer = null;
    }
    if (this.#countdownTimer !== null) {
      clearInterval(this.#countdownTimer);
      this.#countdownTimer = null;
    }
  }
}
