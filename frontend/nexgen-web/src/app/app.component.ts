import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Toast } from 'primeng/toast';

import { TOAST_POLITE_PASS_THROUGH } from './shared/components/feedback/toast.service';
import { ConfirmDialogComponent } from './shared/components/overlay/confirm-dialog.component';
import { ConfirmDialogService } from './shared/components/overlay/confirm-dialog.service';

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
}
