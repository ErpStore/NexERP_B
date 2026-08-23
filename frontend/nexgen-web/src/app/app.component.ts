import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Toast } from 'primeng/toast';

import { TOAST_POLITE_PASS_THROUGH } from './shared/components/feedback/toast.service';
import { ConfirmDialogComponent } from './shared/components/overlay/confirm-dialog.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Toast, ConfirmDialogComponent],
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
}
