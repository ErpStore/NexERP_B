import { useCallback, useEffect, useState } from 'react';

/**
 * The colour-scheme preference: a tri-state, with `system` as the default.
 *
 * PERSISTENCE IS LOCAL, DELIBERATELY AND TEMPORARILY.
 *
 * TODO(M3-3, Admin & Settings wave): move this to the server once a settings
 * endpoint exists. It cannot be done today for two independent reasons, both
 * Confirmed:
 *   1. There is no HTTP surface for the theme at all. IUserThemePreferenceService
 *      (V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SettingsService/
 *      UserThemePreferenceService.cs) is referenced only by Blazor's
 *      MainLayout.razor; V.SMART.Api has no theme endpoint, DTO or route.
 *   2. The server entity cannot hold this value. UserThemePreference is a single
 *      `bool IsDarkMode` defaulting to false
 *      (V.SMART/V.SMART.Shared/Data/Master/MasterScreeenManagement_Module/
 *      UserThemePreference.cs:20) and has NO representation for `system`.
 * Whether that entity becomes tri-state or `system` resolves client-side before
 * it is persisted is an open product/backend question -- Q-33 in
 * docs/kb/open-questions.md. This task raises it; it does not decide it and does
 * not touch the schema.
 *
 * Blazor's own theme (ThemeStateService + UserThemePreference) is untouched and
 * keeps working. During the strangler period the two preferences are INDEPENDENT:
 * switching theme here does not change it in Blazor.
 */
export type ColorSchemePreference = 'light' | 'dark' | 'system';
export type ResolvedColorScheme = 'light' | 'dark';

export const COLOR_SCHEME_PREFERENCES = ['light', 'dark', 'system'] as const;

/** Also read by the pre-paint inline script in index.html. Keep the two in step. */
export const THEME_STORAGE_KEY = 'nexgen.theme';
export const DARK_MEDIA_QUERY = '(prefers-color-scheme: dark)';

export function isColorSchemePreference(value: unknown): value is ColorSchemePreference {
  return (
    typeof value === 'string' && (COLOR_SCHEME_PREFERENCES as readonly string[]).includes(value)
  );
}

/**
 * An unrecognised or unreadable stored value falls back to `system` silently.
 * A corrupt preference must never stop the app rendering -- it is the only error
 * path this layer has.
 */
export function readStoredPreference(): ColorSchemePreference {
  try {
    const stored: unknown = window.localStorage.getItem(THEME_STORAGE_KEY);
    return isColorSchemePreference(stored) ? stored : 'system';
  } catch {
    return 'system';
  }
}

function prefersDark(): boolean {
  if (typeof window.matchMedia !== 'function') return false;
  return window.matchMedia(DARK_MEDIA_QUERY).matches;
}

export function resolveScheme(
  preference: ColorSchemePreference,
  systemPrefersDark: boolean,
): ResolvedColorScheme {
  if (preference === 'system') return systemPrefersDark ? 'dark' : 'light';
  return preference;
}

export function useColorScheme(): {
  preference: ColorSchemePreference;
  resolved: ResolvedColorScheme;
  setPreference: (next: ColorSchemePreference) => void;
} {
  const [preference, setPreferenceState] = useState<ColorSchemePreference>(readStoredPreference);
  const [systemDark, setSystemDark] = useState<boolean>(prefersDark);

  // Live subscription: with the preference on `system`, an OS theme change flips
  // the app immediately. No reload, no remount -- only the state that feeds
  // data-theme changes.
  useEffect(() => {
    if (typeof window.matchMedia !== 'function') return;
    const media = window.matchMedia(DARK_MEDIA_QUERY);
    // No setState here: the initial value is already read by the useState
    // initializer above, and setting it again in the effect body is a cascading
    // render (react-hooks/set-state-in-effect). This effect only SUBSCRIBES.
    const onChange = (event: MediaQueryListEvent) => {
      setSystemDark(event.matches);
    };
    media.addEventListener('change', onChange);
    return () => {
      media.removeEventListener('change', onChange);
    };
  }, []);

  const setPreference = useCallback((next: ColorSchemePreference) => {
    setPreferenceState(next);
    try {
      window.localStorage.setItem(THEME_STORAGE_KEY, next);
    } catch {
      // Private mode or a full quota: the choice still applies for this session.
    }
  }, []);

  return { preference, resolved: resolveScheme(preference, systemDark), setPreference };
}
