import { DOCUMENT, Injectable, computed, inject, signal } from '@angular/core';

import { BREAKPOINTS, minWidthQuery, type BreakpointName } from './breakpoints';

/**
 * M2-C03 — the live counterpart to `breakpoints.ts`'s pure functions.
 *
 * Signals, not RxJS (ADR-007, client state), root-provided so the shell,
 * sidebar and header all read one answer. One `matchMedia` listener per
 * named band (`sm`/`md`/`lg` — `xs` has no query, it is simply "none of the
 * others match"), the same `matchMedia` + `addEventListener('change')`
 * pattern `ThemeService` already uses for `prefers-color-scheme`, so a
 * hostile or SSR-less `document.defaultView` degrades the same way: no
 * listener, `band` stays `'xs'`.
 */
@Injectable({ providedIn: 'root' })
export class BreakpointService {
  private readonly document = inject(DOCUMENT);

  private readonly matches = signal<Record<Exclude<BreakpointName, 'xs'>, boolean>>({
    sm: false,
    md: false,
    lg: false,
  });

  /** The active KB-051 band: `xs` (<768) · `sm` (768–1023) · `md` (1024–1439) · `lg` (≥1440). */
  readonly band = computed<BreakpointName>(() => {
    const m = this.matches();
    if (m.lg) return 'lg';
    if (m.md) return 'md';
    if (m.sm) return 'sm';
    return 'xs';
  });

  /** `lg` only — the one band with the full 240 px sidebar. */
  readonly isFullSidebar = computed(() => this.band() === 'lg');
  /** `md` — the 56 px rail. */
  readonly isRailSidebar = computed(() => this.band() === 'md');
  /** `sm` — the overlay drawer. */
  readonly isOverlaySidebar = computed(() => this.band() === 'sm');
  /** `xs` — read-and-approve only (KB-051 §Responsive behaviour). */
  readonly isReadOnlyBand = computed(() => this.band() === 'xs');

  constructor() {
    const view = this.document.defaultView;
    if (!view || typeof view.matchMedia !== 'function') {
      return;
    }
    for (const name of ['sm', 'md', 'lg'] as const) {
      const query = view.matchMedia(minWidthQuery(name));
      this.matches.update((m) => ({ ...m, [name]: query.matches }));
      const onChange = (event: MediaQueryListEvent): void => {
        this.matches.update((m) => ({ ...m, [name]: event.matches }));
      };
      if (typeof query.addEventListener === 'function') {
        query.addEventListener('change', onChange);
      } else {
        // Safari < 14 and some embedded webviews only have the legacy API — same fallback
        // ThemeService uses for prefers-color-scheme.
        query.addListener(onChange);
      }
    }
  }
}

/** Re-exported for callers that only need the static table, not the live signal. */
export { BREAKPOINTS };
