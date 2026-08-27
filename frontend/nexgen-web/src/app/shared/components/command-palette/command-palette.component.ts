import {
  ChangeDetectionStrategy,
  Component,
  computed,
  effect,
  inject,
  model,
  output,
  signal,
  untracked,
  viewChild,
  type ElementRef,
} from '@angular/core';

import { fuzzyFilter } from '../../../core/navigation/fuzzy-match';
import { NavFilterService } from '../../../core/navigation/nav-filter.service';
import type { NavLink } from '../../../core/navigation/navigation.models';
import { RecentScreensService } from '../../../core/navigation/recent-screens.service';
import { ModalComponent } from '../overlay/modal.component';

/**
 * M2-C03 — the ⌘K / Ctrl+K command palette. Fuzzy-searches **only the permission-filtered
 * nav tree** (`NavFilterService`, the same seam `SidebarComponent` reads — nothing under
 * `shared/components/` may import the auth module directly): a screen the caller lacks
 * rights to must never appear here, even by exact name, or the palette becomes a directory
 * of the tenant's configuration the caller cannot otherwise see (KB-051).
 *
 * Recent-first when the query is empty. `Esc` and outside-click close via `app-modal`
 * (which restores focus to the invoker); `ArrowUp`/`ArrowDown` move the highlighted result,
 * `Enter` activates it. The listbox pattern keeps focus on the input at all times and moves
 * `aria-activedescendant` instead — screen readers announce the highlighted option without
 * focus ever leaving the text field.
 */
@Component({
  selector: 'app-command-palette',
  imports: [ModalComponent],
  templateUrl: './command-palette.component.html',
  styleUrl: './command-palette.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CommandPaletteComponent {
  private readonly navFilter = inject(NavFilterService);
  private readonly recentScreens = inject(RecentScreensService);

  readonly visible = model(false);
  readonly navigated = output<NavLink>();

  protected readonly query = signal('');
  protected readonly activeIndex = signal(0);

  private readonly searchInput = viewChild<ElementRef<HTMLInputElement>>('searchInput');

  private readonly permittedLinks = computed(() => {
    const tree = this.navFilter.filteredTree();
    const links: NavLink[] = [...tree.top];
    for (const group of tree.groups) {
      for (const section of group.sections) {
        links.push(...section.links);
      }
    }
    return links;
  });

  /** Recent-first when the query is empty (and only recents that are still permitted —
   * the same right-revocation-since-visited concern `SidebarComponent` handles). */
  protected readonly results = computed<readonly NavLink[]>(() => {
    const q = this.query().trim();
    const permitted = this.permittedLinks();
    if (!q) {
      const permittedRoutes = new Set(permitted.map((l) => l.route));
      return this.recentScreens.recent().filter((l) => permittedRoutes.has(l.route));
    }
    return fuzzyFilter(q, permitted, (l) => l.label);
  });

  constructor() {
    // Reset on every open so a stale query/highlight from the last use never carries over.
    effect(() => {
      if (this.visible()) {
        untracked(() => {
          this.query.set('');
          this.activeIndex.set(0);
          queueMicrotask(() => this.searchInput()?.nativeElement.focus());
        });
      }
    });

    // The result set changed (new query, or the rights map itself changed) — clamp the
    // highlight back into range rather than pointing at a row that no longer exists.
    effect(() => {
      const count = this.results().length;
      untracked(() => {
        if (this.activeIndex() >= count) {
          this.activeIndex.set(Math.max(0, count - 1));
        }
      });
    });
  }

  protected onQueryChange(value: string): void {
    this.query.set(value);
    this.activeIndex.set(0);
  }

  protected activeId(): string | null {
    const results = this.results();
    return results[this.activeIndex()] ? `app-command-palette-option-${this.activeIndex()}` : null;
  }

  protected onKeydown(event: KeyboardEvent): void {
    const count = this.results().length;
    if (count === 0 && event.key !== 'Escape') {
      return;
    }
    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        this.activeIndex.update((i) => (i + 1) % count);
        break;
      case 'ArrowUp':
        event.preventDefault();
        this.activeIndex.update((i) => (i - 1 + count) % count);
        break;
      case 'Enter':
        event.preventDefault();
        this.activate(this.results()[this.activeIndex()]);
        break;
      default:
        break;
    }
  }

  protected activate(link: NavLink | undefined): void {
    if (!link) {
      return;
    }
    this.recentScreens.record(link);
    this.navigated.emit(link);
    this.visible.set(false);
  }
}
