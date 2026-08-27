import { ChangeDetectionStrategy, Component, input, output } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

import type { NavLink } from '../../../core/navigation/navigation.models';

/**
 * M2-C03 — one leaf destination. Purely presentational: it renders whatever `link` it is
 * given and reads no service — the same discipline `PermissionDeniedStateComponent`
 * follows. Filtering happens once, in `SidebarComponent`'s `filteredTree`, not per item.
 */
@Component({
  selector: 'app-nav-item',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './nav-item.component.html',
  styleUrl: './nav-item.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NavItemComponent {
  readonly link = input.required<NavLink>();
  /** `rail` hides the label, showing only room for an icon-sized target — nav items carry
   * no icon of their own (only groups do), so rail mode falls back to the first letter. */
  readonly rail = input(false);
  readonly favourite = input(false);

  readonly activated = output<NavLink>();
  readonly favouriteToggled = output<NavLink>();

  protected onClick(): void {
    this.activated.emit(this.link());
  }

  protected onFavouriteClick(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    this.favouriteToggled.emit(this.link());
  }
}
