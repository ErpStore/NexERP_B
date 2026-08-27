import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';

import { FavouritesService } from '../../../core/navigation/favourites.service';
import { NavFilterService } from '../../../core/navigation/nav-filter.service';
import type { NavLink } from '../../../core/navigation/navigation.models';
import { RecentScreensService } from '../../../core/navigation/recent-screens.service';
import { EmptyStateComponent } from '../feedback/empty-state.component';
import { NavGroupComponent } from '../nav-group/nav-group.component';
import { NavItemComponent } from '../nav-item/nav-item.component';

export type SidebarVisualMode = 'expanded' | 'rail' | 'overlay';

/**
 * M2-C03 — the permission-filtered sidebar. **Rendering only** (ADR-004): every filter
 * below decides what to *draw*, never what the server allows; a route additionally always
 * carries `requireScreen` (`app.routes.ts`), which is the actual refusal.
 *
 * Deny-by-default, matching `RightsHelper.cs` exactly: an item's `screenName` absent from
 * the rights map, or present with `view: false`, or present with `hidden: true`, is
 * filtered out — reproducing `view && !hidden`. An item with `screenName: null` (today,
 * only "Price Comparison") is never filtered by rights at all: Blazor gates nothing there
 * either (INV-033), so the SPA does not invent a gate. This filter is the *only* input
 * the sidebar reads to decide what to draw — the R-31 regression this deliberately avoids
 * has its own dedicated spec alongside this file.
 *
 * **Reads `NavFilterService` (`core/navigation/`), never the auth layer's permission
 * service directly** — nothing under `shared/components/` may import from the
 * authentication module (`permission-denied-state.component.spec.ts`'s repo-wide scan
 * enforces this), so the actual rights coupling lives in that one seam instead.
 */
@Component({
  selector: 'app-sidebar',
  imports: [NavItemComponent, NavGroupComponent, EmptyStateComponent],
  templateUrl: './sidebar.component.html',
  styleUrl: './sidebar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SidebarComponent {
  private readonly navFilter = inject(NavFilterService);
  private readonly recentScreens = inject(RecentScreensService);
  private readonly favouritesService = inject(FavouritesService);

  readonly mode = input.required<SidebarVisualMode>();

  protected readonly isRail = computed(() => this.mode() === 'rail');

  protected readonly filteredTree = this.navFilter.filteredTree;

  protected readonly isEmpty = computed(
    () => this.filteredTree().top.length === 0 && this.filteredTree().groups.length === 0,
  );

  protected readonly favouriteRoutes = computed(
    () => new Set(this.favouritesService.favourites().map((l) => l.route)),
  );

  protected readonly recent = computed(() =>
    this.recentScreens.recent().filter(this.navFilter.isVisible()),
  );
  protected readonly favourites = computed(() =>
    this.favouritesService.favourites().filter(this.navFilter.isVisible()),
  );

  protected isFavourite(link: NavLink): boolean {
    return this.favouriteRoutes().has(link.route);
  }

  protected onLinkActivated(link: NavLink): void {
    this.recentScreens.record(link);
  }

  protected onFavouriteToggled(link: NavLink): void {
    this.favouritesService.toggle(link);
  }
}
