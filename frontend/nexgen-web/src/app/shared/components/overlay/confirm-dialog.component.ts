import {
  afterEveryRender,
  afterNextRender,
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  signal,
  untracked,
  viewChild,
  type ElementRef,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ConfirmDialog } from 'primeng/confirmdialog';

import { FormFieldComponent } from '../form/form-field.component';
import { TextareaComponent } from '../form/textarea.component';
import { ConfirmDialogService } from './confirm-dialog.service';
import { focusFirstElementIn, OverlayFocusKeeper } from './overlay-focus';

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
  private readonly body = viewChild<ElementRef<HTMLElement>>('body');
  private readonly focus = new OverlayFocusKeeper();

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
    // **This host is deferred (M2-C13), so it may not exist when the first
    // confirmation is asked for.** `p-confirmdialog` subscribes to
    // `requireConfirmation$` in its own constructor
    // (`primeng-confirmdialog.mjs`), i.e. while this component's template is
    // being created - so by the first post-render hook the subscription is in
    // place and any request queued during the lazy load can be replayed
    // without being dropped by that plain `Subject`. Done from
    // `afterNextRender` rather than from the constructor or `ngAfterViewInit`
    // because replaying immediately sets `p-confirmdialog`'s `visible` signal,
    // and a post-render hook is the point at which that schedules a new
    // change-detection pass instead of mutating one already in progress.
    afterNextRender(() => {
      this.service.markHostMounted();
    });

    this.reasonControl.valueChanges.pipe(takeUntilDestroyed()).subscribe((value) => {
      this.reason.set(value ?? '');
    });

    // Runs while the request is still only a signal change, before the dialog
    // has rendered, so the invoker is still the active element - the same
    // point `app-modal` captures at, and for the same reason.
    effect(() => {
      if (this.request() !== null) {
        untracked(() => {
          this.focus.capture();
        });
      }
    });

    // **Focus does not enter this dialog on its own.** Measured in PrimeNG
    // 22.1.2: `p-confirmdialog` hard-codes `[focusOnShow]="false"` on the
    // `p-dialog` it renders and relies on `pAutoFocus` sitting on *its own*
    // accept/reject buttons (`primeng-confirmdialog.mjs`). This component
    // supplies its own `#footer`, so those buttons never exist, nothing takes
    // focus, and a keyboard user is left behind an open modal - which also
    // makes PrimeNG's own focus trap useless, because a trap only holds focus
    // that is already inside.
    //
    // It is done from `afterEveryRender` rather than from an `effect` on the
    // view query because `p-dialog` **moves** its wrapper to `document.body`
    // (`appendContainer()`, `primeng-dialog.mjs`) once the enter transition
    // starts, and moving a node blurs whatever inside it had focus. Focusing
    // when the content first renders is therefore undone a moment later;
    // `afterEveryRender` runs again after the move. `focusFirstElementIn` is a
    // no-op when focus is already inside, so the repetition costs nothing and
    // never steals focus from the control the operator is using.
    afterEveryRender(() => {
      if (this.request() === null) {
        return;
      }
      const panel = dialogPanelOf(this.body()?.nativeElement ?? null);
      if (panel?.isConnected === true) {
        focusFirstElementIn(panel);
      }
    });
  }

  /**
   * Every close - Cancel, `Esc`, the backdrop, the close icon - clears the
   * reason and puts focus back on the element that opened the dialog.
   * Restoration is done here rather than left to PrimeNG: what it restores is
   * whatever it decided to focus on show, and with a custom footer it decided
   * nothing (see the constructor).
   */
  onDialogHide(): void {
    this.reset();
    this.focus.restore();
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

/**
 * The rendered dialog panel that `body` sits inside. `p-confirmdialog` puts
 * `role="alertdialog"` on the panel it renders, and focus has to be moved into
 * the panel, not into the message block, or `Tab` starts below the header.
 */
function dialogPanelOf(body: HTMLElement | null): HTMLElement | null {
  return body?.closest<HTMLElement>('[role="alertdialog"]') ?? body;
}
