import { Injectable, computed, signal } from '@angular/core';

import { DENIED_SCREEN_RIGHT, type ScreenRight, type ScreenRights } from './auth.models';

/**
 * M2-C02 — the client permission store.
 *
 * **RENDERING ONLY. THIS IS NOT A SECURITY BOUNDARY.** The server re-checks the caller's
 * `UserRight` rows on **every** request (ADR-004 §3) — hiding a button here is a UX
 * affordance, never enforcement. A permission store anyone treats as security is the classic
 * failure mode this task exists to avoid, and in this codebase it would be a regression:
 * ADR-004 exists precisely because permission checks currently live **only** in the UI
 * (Confirmed, `docs/kb/architecture/auth-and-permissions.md` § the critical finding). Every
 * consumer of this service — `requireScreen`, `*appHasRight`, `forScreen()` — repeats this
 * rule in its own doc comment so it cannot be missed by reading only one file.
 *
 * **Deny-by-default, matching `RightsHelper.cs` exactly.** A screen the caller holds no
 * `UserRight` row for has no key in the map at all (BR-AUTH-002) — `forScreen()` for a
 * missing key returns {@link DENIED_SCREEN_RIGHT}, every field `false`, never `undefined`
 * treated as "allow".
 */
@Injectable({ providedIn: 'root' })
export class PermissionService {
  readonly #rights = signal<ScreenRights>({});

  readonly rights = this.#rights.asReadonly();

  readonly #bootstrapped = signal(false);

  /** True once a real (possibly empty) rights map has been set — distinguishes "not
   * bootstrapped yet" from "bootstrapped with zero rights", which is a real, renderable
   * state (Q-09's would-be self-registration outcome), not an error. */
  readonly hasBootstrapped = this.#bootstrapped.asReadonly();

  /** True when the caller has zero screen rights at all — a valid, distinct state from "not
   * bootstrapped yet". The login/shell layer renders an explanatory panel for this, not a
   * blank application. */
  readonly hasNoRights = computed(
    () => this.#bootstrapped() && Object.keys(this.#rights()).length === 0,
  );

  /** Replaces the whole rights map — called once per successful bootstrap/login. */
  setRights(rights: ScreenRights): void {
    this.#rights.set(rights);
    this.#bootstrapped.set(true);
  }

  clear(): void {
    this.#rights.set({});
    this.#bootstrapped.set(false);
  }

  /**
   * A signal of one screen's rights. Missing key → {@link DENIED_SCREEN_RIGHT} (every field
   * `false`). `computed()` from this to derive an individual boolean rather than reading
   * `.rights()` directly and indexing — keeps every call site honest about the default.
   */
  forScreen(screenName: string) {
    return computed<ScreenRight>(() => this.#rights()[screenName] ?? DENIED_SCREEN_RIGHT);
  }

  /** Convenience for a single boolean check, e.g. inside a `computed()` elsewhere. Still
   * rendering-only — see the class doc comment. */
  has(screenName: string, right: keyof Omit<ScreenRight, 'hidden'>): boolean {
    return (this.#rights()[screenName] ?? DENIED_SCREEN_RIGHT)[right];
  }
}
