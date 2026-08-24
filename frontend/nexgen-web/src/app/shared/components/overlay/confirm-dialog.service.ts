import { inject, Injectable, signal, type Provider } from '@angular/core';
import { ConfirmationService } from 'primeng/api';

/**
 * What a screen asks for. Everything here is presentation: **which** actions
 * need confirming, and **which** need a reason, is the screen's decision, and
 * whether the action is legal at all is the server's (BR-SO-003 and its
 * downstream-transaction checks live in `V.SMART.Shared`, not here).
 */
export interface ConfirmRequest {
  /** The dialog's accessible name. */
  readonly header: string;
  /** The question, in words the operator can act on. */
  readonly message: string;
  readonly confirmLabel?: string;
  readonly cancelLabel?: string;
  /**
   * Emphasis only - a red confirm button and a warning icon. It changes
   * nothing about the keyboard model: `Esc` still cancels, `Tab` still
   * cycles, and Confirm is never the element that receives initial focus by
   * virtue of being destructive.
   */
  readonly destructive?: boolean;
  /**
   * Collect a free-text reason and refuse to confirm without one.
   *
   * This is the **capability** BR-SO-003 needs, not the rule. The rule - that
   * cancelling a Sales Order or one of its lines requires a reason - is
   * enforced by the server; a screen passing `reasonRequired` is mirroring
   * that server rule in the UI so the operator is not told about it only
   * after a round trip.
   */
  readonly reasonRequired?: boolean;
  readonly reasonLabel?: string;
}

export interface ConfirmResult {
  readonly confirmed: boolean;
  /** Trimmed. `null` whenever the dialog was cancelled or no reason was asked for. */
  readonly reason: string | null;
}

/**
 * The typed way to ask for confirmation.
 *
 * PrimeNG's own `Confirmation` object carries `accept`/`reject` callbacks that
 * take **no arguments** (`primeng/api`), so a free-text reason cannot travel
 * back through it. The reason therefore lives here, written by
 * `app-confirm-dialog` at the moment of acceptance and read by the callback
 * that resolves the promise.
 *
 * **The host is deferred, and this service is what makes that safe (M2-C13).**
 * `app.component.html` renders `<app-confirm-dialog />` inside an `@defer
 * (when confirmHostRequested())` block so `p-confirmdialog`, `p-dialog`,
 * `app-form-field` and `app-textarea` stay out of the initial chunk (R-69).
 * That creates a window: the very call that *triggers* the mount happens while
 * nothing is subscribed yet, and PrimeNG's `requireConfirmation$` is a plain
 * `Subject` (`primeng/api`, `requireConfirmationSource = new Subject()`), so an
 * emission with no subscriber is dropped silently and the caller's promise
 * would never resolve. Requests made before the host has mounted are therefore
 * **queued here** and replayed by `markHostMounted()`.
 */
@Injectable({ providedIn: 'root' })
export class ConfirmDialogService {
  private readonly primeng = inject(ConfirmationService);
  private readonly currentRequest = signal<ConfirmRequest | null>(null);
  private readonly requested = signal(false);
  private reason: string | null = null;

  /**
   * `false` until `app-confirm-dialog` has been created *and* its inner
   * `p-confirmdialog` has subscribed to `requireConfirmation$`. Deliberately a
   * plain field, not a signal: it is written from a post-render hook and never
   * read from a template.
   */
  private hostMounted = false;
  /** Confirmations asked for before the host mounted, in the order asked. */
  private queued: (() => void)[] = [];

  /** What the host component is currently rendering. */
  readonly request = this.currentRequest.asReadonly();

  /**
   * The `@defer` trigger for the host: latches `true` on the first `confirm()`
   * and never returns to `false`, because the single host stays mounted for the
   * rest of the session once it exists.
   */
  readonly hostRequested = this.requested.asReadonly();

  /**
   * Resolves `{ confirmed: false }` for every non-acceptance - the Cancel
   * button, `Esc`, the backdrop and the close icon alike. There is exactly one
   * way to say yes.
   *
   * Safe to call before the deferred host exists: the request waits for it.
   */
  confirm(request: ConfirmRequest): Promise<ConfirmResult> {
    this.currentRequest.set(request);
    this.reason = null;
    this.requested.set(true);
    return new Promise<ConfirmResult>((resolve) => {
      const emit = (): void => {
        this.primeng.confirm({
          header: request.header,
          message: request.message,
          accept: () => {
            resolve({ confirmed: true, reason: this.reason });
            this.currentRequest.set(null);
          },
          reject: () => {
            resolve({ confirmed: false, reason: null });
            this.currentRequest.set(null);
          },
        });
      };

      if (this.hostMounted) {
        emit();
      } else {
        this.queued.push(emit);
      }
    });
  }

  /**
   * Called by `app-confirm-dialog` only, from a post-render hook - by which
   * point its `p-confirmdialog` has run its constructor and is subscribed.
   * Replays anything asked for while the host was still being loaded.
   */
  markHostMounted(): void {
    if (this.hostMounted) {
      return;
    }
    this.hostMounted = true;
    const pending = this.queued;
    this.queued = [];
    for (const emit of pending) {
      emit();
    }
  }

  /** Called by `app-confirm-dialog` only, immediately before acceptance. */
  captureReason(reason: string | null): void {
    this.reason = reason;
  }
}

/**
 * Provides PrimeNG's `ConfirmationService`, so `app.config.ts` wires the
 * confirm layer without reaching into `primeng/api` itself.
 */
export function provideConfirmDialog(): Provider[] {
  return [ConfirmationService];
}
