import {
  ChangeDetectionStrategy,
  Component,
  computed,
  inject,
  signal,
  viewChild,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ConfirmDialog } from 'primeng/confirmdialog';

import { FormFieldComponent } from '../form/form-field.component';
import { TextareaComponent } from '../form/textarea.component';
import { ConfirmDialogService } from './confirm-dialog.service';

/**
 * The single confirm-dialog host. One instance lives in `app.component.html`;
 * screens never place their own. Ask for a confirmation through
 * `ConfirmDialogService.confirm()`, which returns a promise.
 *
 * **Deliberate departure from `BsModal.razor`, recorded rather than absorbed.**
 * The Blazor original lets Confirm be pressed with an empty reason and answers
 * with a toastr warning - "Please enter a valid reason before confirming." -
 * then returns without confirming (`V.SMART/V.SMART.Shared/Components/BsModal.razor:76-93`).
 * Here Confirm is *disabled* until the reason is non-empty after trim, as the
 * task specifies. Same outcome, and the state is visible before the click
 * rather than announced after it; the trade is that a screen-reader user meets
 * a disabled button rather than a message, so the reason field is marked
 * required and its own error explains why.
 *
 * **No ERP business rule lives here.** BR-SO-003's downstream-transaction
 * checks and quantity reversion are server-side and stay there. This dialog
 * collects a reason when asked to; it never decides that one is needed.
 */
@Component({
  selector: 'app-confirm-dialog',
  templateUrl: './confirm-dialog.component.html',
  styleUrl: './confirm-dialog.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ConfirmDialog, ReactiveFormsModule, FormFieldComponent, TextareaComponent],
})
export class ConfirmDialogComponent {
  private readonly service = inject(ConfirmDialogService);
  private readonly dialog = viewChild(ConfirmDialog);

  /**
   * PrimeNG 22.1.0 does not forward `closeAriaLabel` from `p-confirmdialog` to
   * the dialog it renders, leaving the close icon with no accessible name -
   * a critical axe violation. Named through the pass-through instead.
   */
  readonly passThrough = { pcCloseButton: { root: { 'aria-label': 'Close dialog' } } };

  readonly request = this.service.request;
  /** Mirrors the control's value so the template can react to every keystroke. */
  private readonly reason = signal('');

  readonly reasonControl = new FormControl<string | null>(null);

  readonly reasonRequired = computed(() => this.request()?.reasonRequired === true);
  readonly destructive = computed(() => this.request()?.destructive === true);
  readonly confirmLabel = computed(() => this.request()?.confirmLabel ?? 'Confirm');
  readonly cancelLabel = computed(() => this.request()?.cancelLabel ?? 'Cancel');
  readonly reasonLabel = computed(() => this.request()?.reasonLabel ?? 'Reason');

  /** Non-empty after trim. Whitespace is not a reason. */
  readonly canConfirm = computed(() => !this.reasonRequired() || this.reason().trim().length > 0);

  constructor() {
    this.reasonControl.valueChanges.pipe(takeUntilDestroyed()).subscribe((value) => {
      this.reason.set(value ?? '');
    });
  }

  /** Every close - Cancel, `Esc`, the backdrop, the close icon - clears the reason. */
  onDialogHide(): void {
    this.reset();
  }

  accept(): void {
    if (!this.canConfirm()) {
      return;
    }
    const trimmed = this.reason().trim();
    this.service.captureReason(this.reasonRequired() ? trimmed : trimmed || null);
    this.reset();
    this.dialog()?.onAccept();
  }

  cancel(): void {
    this.reset();
    this.dialog()?.onReject();
  }

  private reset(): void {
    this.reasonControl.reset(null, { emitEvent: false });
    this.reason.set('');
  }
}
