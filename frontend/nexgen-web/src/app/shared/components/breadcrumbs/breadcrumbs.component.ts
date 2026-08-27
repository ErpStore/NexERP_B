import { ChangeDetectionStrategy, Component, DestroyRef, inject, signal } from '@angular/core';
import { ActivatedRoute, NavigationEnd, Router, RouterLink } from '@angular/router';

export interface BreadcrumbItem {
  readonly label: string;
  /** `null` for the last crumb — the current page, never a link. */
  readonly url: string | null;
}

/**
 * M2-C03 — replaces `PageHeader.razor`'s hand-passed `BreadcrumbItem` list
 * (`PageHeader.razor:16,27`) with a trail **derived from route `data`**, so a page cannot
 * forget to set it. Every routed segment that wants a crumb sets
 * `data: { breadcrumb: 'Currency Master' }`; a segment with no `breadcrumb` key (the shell's
 * own layout route, for instance) contributes no crumb and is otherwise invisible here.
 *
 * An ordered list in a `<nav aria-label="Breadcrumb">` (KB-051 Accessibility commitments).
 * The last crumb is `aria-current="page"` and is not a link — recreating the page you are
 * already on adds a navigation, not a shortcut.
 */
@Component({
  selector: 'app-breadcrumbs',
  imports: [RouterLink],
  templateUrl: './breadcrumbs.component.html',
  styleUrl: './breadcrumbs.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class BreadcrumbsComponent {
  private readonly router = inject(Router);

  protected readonly items = signal<readonly BreadcrumbItem[]>(this.buildTrail());

  constructor() {
    const sub = this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        this.items.set(this.buildTrail());
      }
    });
    inject(DestroyRef).onDestroy(() => sub.unsubscribe());
  }

  private buildTrail(): readonly BreadcrumbItem[] {
    const items: BreadcrumbItem[] = [];
    let route: ActivatedRoute | undefined = this.router.routerState.root;
    let url = '';

    while (route) {
      const segment = route.snapshot.url.map((s) => s.path).join('/');
      if (segment) {
        url += `/${segment}`;
      }
      const label = route.snapshot.data['breadcrumb'] as string | undefined;
      if (label) {
        items.push({ label, url });
      }
      route = route.firstChild ?? undefined;
    }

    // The last crumb is the current page: drop its link, it is where the user already is.
    return items.map((item, index) => (index === items.length - 1 ? { ...item, url: null } : item));
  }
}
