import { ChangeDetectionStrategy, Component, input, model, output, viewChild } from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';

import type { NavGroup, NavLink } from '../../../core/navigation/navigation.models';
import { PopoverComponent } from '../overlay/popover.component';
import { NavItemComponent } from '../nav-item/nav-item.component';

/**
 * M2-C03 — one entry in the sidebar's single level of accordion (see
 * `navigation.models.ts`'s file header for why a group's `sections` are static headings,
 * not a second accordion level).
 *
 * **Two renderings of the same content, not two behaviours.** Expanded mode (240 px) has
 * room to show the group inline, so a click toggles an inline panel. Rail mode (56 px) does
 * not have room for a single label, let alone a whole group's worth — a click there opens
 * the identical section content in an `app-popover` anchored to the icon instead. This is a
 * deliberate simplification of the Blazor mini-rail's flyout-into-the-top-bar mechanic
 * (`NavMenu.razor`'s `SelectModule`/`OnFlyoutChanged`), not a port of it: same content,
 * anchored to the icon rather than relocated into a header bar. Recorded as a disclosed
 * deviation, not a silent one.
 */
@Component({
  selector: 'app-nav-group',
  imports: [NavItemComponent, PopoverComponent, NgTemplateOutlet],
  templateUrl: './nav-group.component.html',
  styleUrl: './nav-group.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NavGroupComponent {
  readonly group = input.required<NavGroup>();
  readonly rail = input(false);
  readonly expanded = model(false);
  readonly favourites = input<ReadonlySet<string>>(new Set());

  readonly linkActivated = output<NavLink>();
  readonly favouriteToggled = output<NavLink>();

  private readonly popover = viewChild<PopoverComponent>('flyout');

  protected toggleExpanded(): void {
    this.expanded.update((v) => !v);
  }

  protected openFlyout(event: Event): void {
    this.popover()?.toggle(event);
  }

  protected isFavourite(link: NavLink): boolean {
    return this.favourites().has(link.route);
  }
}
