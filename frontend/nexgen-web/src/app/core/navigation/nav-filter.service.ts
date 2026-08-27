import { Injectable, computed, inject } from '@angular/core';

import { PermissionService } from '../auth/permission.service';
import { NAVIGATION_TREE } from './navigation.config';
import type { NavLink, NavTree } from './navigation.models';

/**
 * M2-C03 — the one seam between the nav tree and `PermissionService`, so that
 * `shared/components/sidebar/` (and everything under `shared/components/` generally) never
 * imports `core/auth/` directly — `permission-denied-state.component.spec.ts`'s own
 * repo-wide scan enforces exactly that boundary, and this service is how `SidebarComponent`
 * still gets filtered data without crossing it. `core/navigation/` plays the same role for
 * navigation that `core/auth/` plays for auth — a domain seam, not a design-system
 * primitive — so the coupling belongs here, not in `shared/components/`.
 *
 * **Rendering only** (ADR-004), same as `PermissionService` itself: this decides what the
 * sidebar *draws*. The route-level `requireScreen` guard in `app.routes.ts` is what actually
 * refuses a hand-typed URL.
 */
@Injectable({ providedIn: 'root' })
export class NavFilterService {
  private readonly permissions = inject(PermissionService);

  /** `screenName: null` (today, only "Price Comparison") is never gated — Blazor has no
   * `BaseUserRightsComponent` on that page either (INV-033), so nothing here invents one.
   * Otherwise: absent from the rights map, `view: false`, or `hidden: true` are all denied,
   * reproducing `RightsHelper.cs`'s `view && !hidden` exactly. */
  readonly isVisible = computed(() => {
    const rightsMap = this.permissions.rights();
    return (link: NavLink): boolean => {
      if (link.screenName === null) {
        return true;
      }
      const right = rightsMap[link.screenName];
      return !!right && right.view && !right.hidden;
    };
  });

  readonly filteredTree = computed<NavTree>(() => {
    const visible = this.isVisible();
    const top = NAVIGATION_TREE.top.filter(visible);
    const groups = NAVIGATION_TREE.groups
      .map((group) => ({
        ...group,
        sections: group.sections
          .map((section) => ({ ...section, links: section.links.filter(visible) }))
          .filter((section) => section.links.length > 0),
      }))
      .filter((group) => group.sections.length > 0);
    return { top, groups };
  });
}
