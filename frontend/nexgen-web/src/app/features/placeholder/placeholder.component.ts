import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { CardModule } from 'primeng/card';

import { APP_INFO } from '../../core/config/app-config';
import { PermissionService } from '../../core/auth/permission.service';
// Direct file imports, not the shared/components barrel — same R-69 discipline
// `app.component.ts` and `login.component.ts` already follow for their own chunks.
import { EmptyStateComponent } from '../../shared/components/feedback/empty-state.component';
import { PermissionDeniedStateComponent } from '../../shared/components/feedback/permission-denied-state.component';

/**
 * Scaffold-only landing page. It exists to prove the provider stack composes,
 * that lazy routing works, and — since it is `app.routes.ts`'s one
 * `requireScreen('Dashboard', 'view')`-guarded route today — that the
 * deny-by-default rendering pattern every real screen attaches to from
 * `M2-D01` onward actually works, not just that the guard returns `true`.
 * `M2-C03` replaces this whole component with the real app shell; the
 * rights-checking `@if` chain below is what moves into that shell, not
 * something specific to this scaffold.
 *
 * Order matters: a caller with **zero** rights at all (Q-09's would-be
 * self-registration outcome) gets the explanatory empty state, never the
 * generic "missing this one right" message — see `PermissionService.hasNoRights`'s
 * doc comment.
 */
@Component({
  selector: 'app-placeholder',
  imports: [CardModule, EmptyStateComponent, PermissionDeniedStateComponent],
  templateUrl: './placeholder.component.html',
  styleUrl: './placeholder.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PlaceholderComponent {
  protected readonly appName = APP_INFO.name;
  protected readonly version = APP_INFO.version;

  readonly #permissions = inject(PermissionService);
  protected readonly hasNoRights = this.#permissions.hasNoRights;
  protected readonly dashboardRight = this.#permissions.forScreen('Dashboard');
}
