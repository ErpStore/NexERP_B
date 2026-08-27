import { ChangeDetectionStrategy, Component, inject } from '@angular/core';

// Direct import, not the shared/components barrel — this component is mounted from the root
// app component (bootstrap-time), the same "avoid dragging the whole design-system surface
// into the eager bundle" discipline app.component.ts and login.component.ts already follow
// (R-69).
import { ModalComponent } from '../../../shared/components/overlay/modal.component';
import { IdleTimeoutService } from '../../../core/auth/idle-timeout.service';

/**
 * M2-C02 — the idle-timeout warning dialog. Mounted once, at the app root, alongside
 * `<router-outlet>` — it is not routed, and it must be visible regardless of which route is
 * active, since the whole point is warning an inattentive user before the app signs them
 * out from under whatever they were looking at.
 *
 * Countdown is integer seconds, ticked by {@link IdleTimeoutService} itself — nothing here
 * does duration arithmetic (`Math.floor`/`.toFixed` are both banned repo-wide,
 * `eslint.config.js`, for money/quantity precision reasons that apply here only
 * incidentally: there is simply no float math to do).
 */
@Component({
  selector: 'app-idle-warning',
  templateUrl: './idle-warning.component.html',
  styleUrl: './idle-warning.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ModalComponent],
})
export class IdleWarningComponent {
  readonly idleTimeout = inject(IdleTimeoutService);

  staySignedIn(): void {
    this.idleTimeout.staySignedIn();
  }
}
