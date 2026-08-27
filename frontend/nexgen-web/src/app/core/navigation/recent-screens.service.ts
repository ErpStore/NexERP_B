import { Injectable, signal } from '@angular/core';

import type { NavLink } from './navigation.models';

const STORAGE_KEY = 'nexgen.nav.recent';
const MAX_ENTRIES = 8;

/**
 * M2-C03 — screens the caller has actually opened, most-recent first. Local only, per
 * KB-050's rule (only the token custody question in `M2-C02` is a security decision;
 * "which screens did I open" is a device convenience, same tier as the theme preference in
 * `ThemeService`, and is written to `localStorage` for the same reason: there is no server
 * slot for it and no security property depends on it surviving a device change).
 *
 * Not the auth session: this deliberately survives logout, matching a browser's own history
 * — the *next* person to sign in on a shared machine seeing "Recent: Currency Master" is no
 * more revealing than their browser's address-bar autocomplete already is, and clearing it
 * on every login would defeat the point of "recent" on a machine one user returns to daily.
 */
@Injectable({ providedIn: 'root' })
export class RecentScreensService {
  private readonly items = signal<readonly NavLink[]>(this.readStored());

  readonly recent = this.items.asReadonly();

  record(link: NavLink): void {
    const withoutDuplicate = this.items().filter((l) => l.route !== link.route);
    const next = [link, ...withoutDuplicate].slice(0, MAX_ENTRIES);
    this.items.set(next);
    this.write(next);
  }

  clear(): void {
    this.items.set([]);
    this.write([]);
  }

  /** A hostile `localStorage` (private mode, full quota) degrades to an empty list, never
   * throws — same discipline as `ThemeService`. */
  private readStored(): readonly NavLink[] {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) {
        return [];
      }
      const parsed: unknown = JSON.parse(raw);
      if (!Array.isArray(parsed)) {
        return [];
      }
      return parsed.filter(isNavLink).slice(0, MAX_ENTRIES);
    } catch {
      return [];
    }
  }

  private write(items: readonly NavLink[]): void {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(items));
    } catch {
      // Full quota or a hostile storage — the in-memory signal still holds this session's
      // recents, only persistence across reloads is lost.
    }
  }
}

function isNavLink(value: unknown): value is NavLink {
  return (
    typeof value === 'object' &&
    value !== null &&
    typeof (value as NavLink).label === 'string' &&
    typeof (value as NavLink).route === 'string'
  );
}
