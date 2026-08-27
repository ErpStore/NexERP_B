import { Injectable, signal } from '@angular/core';

import type { NavLink } from './navigation.models';

const STORAGE_KEY = 'nexgen.nav.favourites';

/**
 * M2-C03 — screens the caller has explicitly pinned. Local only, same reasoning as
 * {@link RecentScreensService}: a device convenience, not a security-scoped preference, so
 * it is not folded into `AuthService`/`PermissionService` and it survives logout.
 */
@Injectable({ providedIn: 'root' })
export class FavouritesService {
  private readonly items = signal<readonly NavLink[]>(this.readStored());

  readonly favourites = this.items.asReadonly();

  isFavourite(route: string): boolean {
    return this.items().some((l) => l.route === route);
  }

  toggle(link: NavLink): void {
    const next = this.isFavourite(link.route)
      ? this.items().filter((l) => l.route !== link.route)
      : [...this.items(), link];
    this.items.set(next);
    this.write(next);
  }

  private readStored(): readonly NavLink[] {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) {
        return [];
      }
      const parsed: unknown = JSON.parse(raw);
      return Array.isArray(parsed) ? parsed.filter(isNavLink) : [];
    } catch {
      return [];
    }
  }

  private write(items: readonly NavLink[]): void {
    try {
      localStorage.setItem(STORAGE_KEY, JSON.stringify(items));
    } catch {
      // Full quota or a hostile storage — the in-memory signal is still correct for this
      // session, only persistence across reloads is lost.
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
