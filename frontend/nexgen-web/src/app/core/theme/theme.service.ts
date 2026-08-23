import { DOCUMENT, Injectable, computed, inject, signal } from '@angular/core';

import { DEFAULT_DENSITY, type Density, isDensity } from './density';

/** What the user asked for. `system` is the default and follows the OS live. */
export const THEME_PREFERENCES = ['light', 'dark', 'system'] as const;
export type ThemePreference = (typeof THEME_PREFERENCES)[number];

/** What is actually painted, after `system` is resolved. */
export type ColourScheme = 'light' | 'dark';

/**
 * Storage keys. `nexgen.theme` is fixed by KB-051 Theme persistence and is
 * also read by the pre-paint script in `src/index.html` - change it in one
 * place only if you change it in both.
 */
export const THEME_STORAGE_KEY = 'nexgen.theme';
export const DENSITY_STORAGE_KEY = 'nexgen.density';

export const DARK_SCHEME_QUERY = '(prefers-color-scheme: dark)';

function isThemePreference(value: unknown): value is ThemePreference {
  return typeof value === 'string' && (THEME_PREFERENCES as readonly string[]).includes(value);
}

/**
 * The SPA's colour-scheme and density state.
 *
 * Signals, not RxJS (ADR-007, client state). Root-provided, and deliberately
 * *not* part of the auth/session service: theme is device-scoped, so folding
 * it into session state would lose it on logout.
 *
 * TODO(M3-3): persist the preference server-side once the Admin & Settings
 * wave adds a settings endpoint. It is `localStorage` today because there is
 * no HTTP surface for it (INV-006, negative result) and because the existing
 * server entity cannot hold the answer: `UserThemePreference` is a single
 * `bool IsDarkMode`
 * (V.SMART/V.SMART.Shared/Data/Master/MasterScreeenManagement_Module/UserThemePreference.cs:20)
 * and has no representation for `system`. Whether it becomes tri-state is
 * Q-33 in docs/kb/open-questions.md - do not extend the entity from here.
 */
@Injectable({ providedIn: 'root' })
export class ThemeService {
  private readonly document = inject(DOCUMENT);

  private readonly preferenceSignal = signal<ThemePreference>('system');
  private readonly densitySignal = signal<Density>(DEFAULT_DENSITY);
  private readonly systemPrefersDark = signal(false);

  /** `light | dark | system`, as chosen by the user. */
  readonly preference = this.preferenceSignal.asReadonly();

  /** The density in force. */
  readonly density = this.densitySignal.asReadonly();

  /** `system` resolved against `prefers-color-scheme`. What is painted. */
  readonly resolvedScheme = computed<ColourScheme>(() => {
    const preference = this.preferenceSignal();
    if (preference === 'system') {
      return this.systemPrefersDark() ? 'dark' : 'light';
    }
    return preference;
  });

  private mediaQuery: MediaQueryList | null = null;

  constructor() {
    this.preferenceSignal.set(this.readStoredPreference());
    this.densitySignal.set(this.readStoredDensity());
    this.watchSystemScheme();
    this.apply();
  }

  setPreference(preference: ThemePreference): void {
    this.preferenceSignal.set(preference);
    this.write(THEME_STORAGE_KEY, preference);
    this.apply();
  }

  setDensity(density: Density): void {
    this.densitySignal.set(density);
    this.write(DENSITY_STORAGE_KEY, density);
    this.apply();
  }

  /**
   * Writes the resolved scheme and density onto `<html>`. Only CSS variable
   * values change as a result - no component is re-created and no element is
   * added or removed, so switching theme cannot shift layout.
   */
  private apply(): void {
    const root = this.document.documentElement;
    root.setAttribute('data-theme', this.resolvedScheme());
    root.setAttribute('data-density', this.densitySignal());
  }

  private watchSystemScheme(): void {
    const view = this.document.defaultView;
    if (!view || typeof view.matchMedia !== 'function') {
      return;
    }
    this.mediaQuery = view.matchMedia(DARK_SCHEME_QUERY);
    this.systemPrefersDark.set(this.mediaQuery.matches);
    const onChange = (event: MediaQueryListEvent): void => {
      this.systemPrefersDark.set(event.matches);
      this.apply();
    };
    if (typeof this.mediaQuery.addEventListener === 'function') {
      this.mediaQuery.addEventListener('change', onChange);
    } else {
      // Safari < 14 and some embedded webviews only have the legacy API.
      this.mediaQuery.addListener(onChange);
    }
  }

  private readStoredPreference(): ThemePreference {
    const stored = this.read(THEME_STORAGE_KEY);
    // A corrupt value must never stop the app rendering - fall back silently.
    return isThemePreference(stored) ? stored : 'system';
  }

  private readStoredDensity(): Density {
    const stored = this.read(DENSITY_STORAGE_KEY);
    return isDensity(stored) ? stored : DEFAULT_DENSITY;
  }

  /** A hostile `localStorage` (private mode, full quota) degrades, never throws. */
  private read(key: string): string | null {
    try {
      return this.document.defaultView?.localStorage.getItem(key) ?? null;
    } catch {
      return null;
    }
  }

  private write(key: string, value: string): void {
    try {
      this.document.defaultView?.localStorage.setItem(key, value);
    } catch {
      // Preference is lost for this session; the app keeps working.
    }
  }
}
