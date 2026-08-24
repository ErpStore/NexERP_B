import {
  ChangeDetectionStrategy,
  Component,
  effect,
  input,
  model,
  output,
  untracked,
  viewChild,
  type ElementRef,
} from '@angular/core';
import { Dialog } from 'primeng/dialog';

import { focusFirstElementIn, OverlayFocusKeeper } from './overlay-focus';

/**
 * `sm` 480px, `md` 640px, `lg` 900px, `full` the viewport less a gutter.
 * Below 768px (`--breakpoint-sm`) `lg` and `full` become full-screen sheets -
 * a 900px modal on a tablet is unusable. See `modal.component.scss`.
 */
export type ModalSize = 'sm' | 'md' | 'lg' | 'full';

/**
 * A modal dialog over `p-dialog`.
 *
 * **Use `app-drawer` instead when the list behind the overlay still matters**
 * (KB-051 Do not: "Use modals for anything that needs the list behind it").
 * An operator reconciling a record against the list it came from needs both
 * visible; a modal takes the list away.
 *
 * Open/closed state belongs to the calling screen - `[(visible)]` - and there
 * is deliberately no global modal registry. Across ~140 screens a registry is
 * how modal state stops being traceable to the code that opened it.
 *
 * No ERP business rule lives here.
 */
@Component({
  selector: 'app-modal',
  templateUrl: './modal.component.html',
  styleUrl: './modal.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Dialog],
})
export class ModalComponent {
  /** The accessible name of the dialog. Required: an unlabelled dialog is a WCAG failure. */
  readonly title = input.required<string>();
  readonly size = input<ModalSize>('md');
  /** Two-way. The screen owns the state; the modal only reflects it. */
  readonly visible = model(false);
  /**
   * `false` removes the close icon and stops `Esc` closing - for a modal whose
   * outcome must be chosen rather than escaped. Use it sparingly: an overlay a
   * keyboard user cannot leave is a trap.
   */
  readonly closable = input(true);
  readonly opened = output<void>();
  readonly closed = output<void>();

  private readonly body = viewChild<ElementRef<HTMLElement>>('body');
  private readonly focus = new OverlayFocusKeeper();

  constructor() {
    // Runs while `visible` is still only a signal change, before the dialog
    // has rendered and taken focus, so the invoker is still the active
    // element. `onShow` would be too late.
    effect(() => {
      if (this.visible()) {
        untracked(() => {
          this.focus.capture();
        });
      }
    });
  }

  onShow(): void {
    focusFirstElementIn(this.dialogRoot());
    this.opened.emit();
  }

  onHide(): void {
    this.focus.restore();
    this.closed.emit();
  }

  private dialogRoot(): HTMLElement | null {
    const body = this.body()?.nativeElement;
    return body?.closest<HTMLElement>('[role="dialog"]') ?? body ?? null;
  }
}
