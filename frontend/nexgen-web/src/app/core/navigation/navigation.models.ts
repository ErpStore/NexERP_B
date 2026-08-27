/**
 * M2-C03 — the nav tree's typed shape. `navigation.config.ts` is the one instance of it;
 * everything under `shared/components/sidebar|nav-group|nav-item|command-palette` reads
 * only these types, never the Blazor source.
 *
 * **Single-level accordion (KB-051 §Application shell).** Only the top-level {@link NavGroup}
 * expands/collapses. A group's `sections` are *not* independently collapsible — where the
 * Blazor mini-rail's `NavGroups` dictionary nests a second level of `MudNavGroup` (e.g.
 * "Master" → "Admin Master" → items), that second level becomes a `NavSection.heading`: a
 * static sub-label rendered inside the one expanded panel, not a second accordion. See
 * INV-033 for why the expanded `<MudNavMenu>` tree, not the mini-rail dictionary, is this
 * file's source (the two disagree slightly on the Human Resource group's membership).
 */

/** One clickable destination. */
export interface NavLink {
  /** As shown in the sidebar and the palette. */
  readonly label: string;
  /**
   * The Angular route, absolute from the app root (e.g. `/masters/currencies`). Per
   * KB-053's route conventions — kebab-case, `List` suffix dropped, `{module}/{entity}`.
   */
  readonly route: string;
  /**
   * `Screens.ScreenName` verbatim, for `PermissionService.forScreen()`. **`null` is not
   * "unmapped"** — it means this task investigated the item and found no `UserRight` row
   * governs it (e.g. a page with no `BaseUserRightsComponent` override). A `null` entry
   * without a same-line comment explaining why is a defect in this file, not a valid state
   * (INV-033's own rule: "no silent omissions").
   */
  readonly screenName: string | null;
  /** A `pi-*` PrimeIcons suffix, without the `pi-` prefix. Only set on the rare top-level
   * link that renders outside any {@link NavGroup} (today, just Dashboard) — a link inside
   * a group's `sections` takes its icon from the group. */
  readonly icon?: string;
}

/** A non-interactive sub-heading inside one expanded group — see the file header. */
export interface NavSection {
  /** Omitted for a group's own top-level links (e.g. "My Company Details" directly under
   * "Master", not nested in a named sub-section). */
  readonly heading?: string;
  readonly links: readonly NavLink[];
}

/** One entry in the sidebar's single level of accordion. */
export interface NavGroup {
  readonly label: string;
  /** A `pi-*` PrimeIcons suffix, without the `pi-` prefix (e.g. `"warehouse"`). */
  readonly icon: string;
  readonly sections: readonly NavSection[];
}

export interface NavTree {
  /** Rendered above the accordion, never inside a group — currently just Dashboard. */
  readonly top: readonly NavLink[];
  readonly groups: readonly NavGroup[];
}

/** Every {@link NavLink} in a tree, group membership discarded — what the command palette
 * fuzzy-searches and what `navigation.config.spec.ts` walks to check for duplicate routes. */
export function flattenLinks(tree: NavTree): readonly NavLink[] {
  const links: NavLink[] = [...tree.top];
  for (const group of tree.groups) {
    for (const section of group.sections) {
      links.push(...section.links);
    }
  }
  return links;
}
