import { ChangeDetectionStrategy, Component, inject } from '@angular/core';

import { APP_INFO } from '../../core/config/app-config';
import { PermissionService } from '../../core/auth/permission.service';
// Direct file imports, not the shared/components barrel — same R-69 discipline
// app.component.ts/login.component.ts/placeholder.component.ts already followed.
import { EmptyStateComponent } from '../../shared/components/feedback/empty-state.component';
import { PermissionDeniedStateComponent } from '../../shared/components/feedback/permission-denied-state.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';

/**
 * M2-C03 — the shell's real landing route, replacing `M2-C02`'s scaffold
 * `features/placeholder/` (KB-050 §Project structure names this task as the one that
 * removes it). It is `app.routes.ts`'s one `requireScreen('Dashboard', 'view')`-guarded
 * route, so it is also where the deny-by-default rendering pattern every real screen
 * attaches to from `M2-D01` onward is actually exercised, not just asserted true by the
 * guard. The rights-checking `@if` chain is carried forward from the scaffold unchanged,
 * per its own doc comment predicting exactly this move.
 *
 * Order matters: a caller with **zero** rights at all (Q-09's would-be self-registration
 * outcome) gets the explanatory empty state, never the generic "missing this one right"
 * message — see `PermissionService.hasNoRights`'s doc comment. The sidebar
 * (`NavFilterService`) shows the equivalent state in the nav column independently; this is
 * the content-area half of the same concern.
 */
@Component({
  selector: 'app-dashboard',
  imports: [PageHeaderComponent, EmptyStateComponent, PermissionDeniedStateComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent {
  protected readonly appName = APP_INFO.name;
  protected readonly version = APP_INFO.version;

  readonly #permissions = inject(PermissionService);
  protected readonly hasNoRights = this.#permissions.hasNoRights;
  protected readonly dashboardRight = this.#permissions.forScreen('Dashboard');
}
