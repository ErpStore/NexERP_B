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
 */
@Injectable({ providedIn: 'root' })
export class ConfirmDialogService {
  private readonly primeng = inject(ConfirmationService);
  private readonly currentRequest = signal<ConfirmRequest | null>(null);
  private reason: string | null = null;

  /** What the host component is currently rendering. */
  readonly request = this.currentRequest.asReadonly();

  /**
   * Resolves `{ confirmed: false }` for every non-acceptance - the Cancel
   * button, `Esc`, the backdrop and the close icon alike. There is exactly one
   * way to say yes.
   */
  confirm(request: ConfirmRequest): Promise<ConfirmResult> {
    this.currentRequest.set(request);
    this.reason = null;
    return new Promise<ConfirmResult>((resolve) => {
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
    });
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
