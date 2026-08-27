import { ChangeDetectionStrategy, Component, input } from '@angular/core';

import { BreadcrumbsComponent } from '../breadcrumbs/breadcrumbs.component';

/**
 * M2-C03 — replaces `PageHeader.razor`. Composes the breadcrumb trail (derived, not
 * hand-passed — see `BreadcrumbsComponent`) with the page title and two content-projection
 * slots: `appPageStatus` for a status badge next to the title, `appPageActions` for the
 * page-level action buttons. Neither slot is required; an empty slot renders nothing extra.
 */
@Component({
  selector: 'app-page-header',
  imports: [BreadcrumbsComponent],
  templateUrl: './page-header.component.html',
  styleUrl: './page-header.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PageHeaderComponent {
  readonly title = input.required<string>();
}
