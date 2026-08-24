import { inject, Injectable, type Provider } from '@angular/core';
import { MessageService } from 'primeng/api';

/** KB-051 Feedback: success dismisses itself after 4 s. */
export const TOAST_SUCCESS_LIFE_MS = 4000;
/** Information is not an achievement; it gets the same 4 s. */
export const TOAST_INFO_LIFE_MS = 4000;
/** A warning is worth a longer read than a confirmation. */
export const TOAST_WARN_LIFE_MS = 8000;

/**
 * The typed toast API - and **the only file in the SPA that imports
 * `MessageService`** (asserted by grep in `toast.service.spec.ts` and by the
 * task's own verification command). Call sites say `toast.error(message)`, so
 * replacing the toast implementation later touches this file and nothing else.
 *
 * Policy, from KB-051 Feedback:
 *   - success auto-dismisses at 4 s;
 *   - **error is sticky** and needs an explicit dismiss, because an error a
 *     screen-reader user has to race a timer to hear is an error they miss;
 *   - a toast never carries the only copy of something the user must act on -
 *     that belongs in `app-inline-alert` or `app-error-state`, next to the
 *     thing that failed.
 *
 * A server business-rule message (a 409) is passed through **verbatim**:
 * `ApiProblems.cs:47-53` puts the domain team's wording in `title`, and those
 * strings are product UX. Never reword, prefix, translate or truncate one.
 */
@Injectable({ providedIn: 'root' })
export class ToastService {
  private readonly messages = inject(MessageService);

  success(summary: string, detail?: string): void {
    this.messages.add({
      severity: 'success',
      summary,
      detail,
      life: TOAST_SUCCESS_LIFE_MS,
    });
  }

  info(summary: string, detail?: string): void {
    this.messages.add({
      severity: 'info',
      summary,
      detail,
      life: TOAST_INFO_LIFE_MS,
    });
  }

  warn(summary: string, detail?: string): void {
    this.messages.add({
      severity: 'warn',
      summary,
      detail,
      life: TOAST_WARN_LIFE_MS,
    });
  }

  /** Sticky. The user dismisses it; nothing else does. */
  error(summary: string, detail?: string): void {
    this.messages.add({
      severity: 'error',
      summary,
      detail,
      sticky: true,
      closable: true,
    });
  }

  clear(): void {
    this.messages.clear();
  }
}

/**
 * Provides PrimeNG's `MessageService` without any other file having to import
 * it. `app.config.ts` calls this, so the grep in the acceptance criteria -
 * "`MessageService` is touched only by `feedback/toast.service.ts`" - stays
 * literally true rather than nearly true.
 */
export function provideToast(): Provider[] {
  return [MessageService];
}

/**
 * PrimeNG 22.1.0 hard-codes `role="alert" aria-live="assertive"` on every
 * toast item (`primeng-toast.mjs`). KB-051 asks for a **polite** live region:
 * a saved-successfully toast that interrupts whatever a screen-reader user is
 * reading is worse than one they hear a moment later. The same element carries
 * `[pBind]="ptm('message')"`, so this pass-through overrides the attribute
 * without forking the component. `app.component.html` binds it; the spec
 * asserts the rendered attribute rather than trusting the mechanism.
 */
export const TOAST_POLITE_PASS_THROUGH = {
  message: { 'aria-live': 'polite' },
};
