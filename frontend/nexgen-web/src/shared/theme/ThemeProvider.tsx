import { MantineProvider } from '@mantine/core';
import {
  createContext,
  type ReactNode,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
} from 'react';

import { type Density, isDensity } from './density';
import { theme } from './theme';
import {
  type ColorSchemePreference,
  type ResolvedColorScheme,
  useColorScheme,
} from './useColorScheme';

export const DENSITY_STORAGE_KEY = 'nexgen.density';

export interface ThemeContextValue {
  preference: ColorSchemePreference;
  resolved: ResolvedColorScheme;
  setPreference: (next: ColorSchemePreference) => void;
  density: Density;
  setDensity: (next: Density) => void;
}

const ThemeContext = createContext<ThemeContextValue | null>(null);

export function useTheme(): ThemeContextValue {
  const value = useContext(ThemeContext);
  if (!value) throw new Error('useTheme must be used inside <ThemeProvider>');
  return value;
}

function readStoredDensity(): Density {
  try {
    const stored: unknown = window.localStorage.getItem(DENSITY_STORAGE_KEY);
    return isDensity(stored) ? stored : 'default';
  } catch {
    return 'default';
  }
}

/**
 * Owns the resolved colour scheme and the density, and writes both onto <html>
 * as data attributes so plain CSS -- not just React -- can react to them.
 *
 * Theme is deliberately NOT in the Zustand auth store (M2-C02): it is
 * device-scoped, and folding it into session state loses it on logout.
 *
 * Switching theme mutates two attributes on <html>. Nothing unmounts, no element
 * changes size, and the app subtree keeps its identity -- asserted in
 * theme.test.tsx.
 */
export function ThemeProvider({ children }: { children: ReactNode }) {
  const { preference, resolved, setPreference } = useColorScheme();
  const [density, setDensityState] = useState<Density>(readStoredDensity);

  useEffect(() => {
    document.documentElement.dataset.theme = resolved;
  }, [resolved]);

  useEffect(() => {
    document.documentElement.dataset.density = density;
  }, [density]);

  const setDensity = useCallback((next: Density) => {
    setDensityState(next);
    try {
      window.localStorage.setItem(DENSITY_STORAGE_KEY, next);
    } catch {
      // Non-fatal: the choice still applies for this session.
    }
  }, []);

  const value = useMemo<ThemeContextValue>(
    () => ({ preference, resolved, setPreference, density, setDensity }),
    [preference, resolved, setPreference, density, setDensity],
  );

  return (
    <ThemeContext.Provider value={value}>
      {/* forceColorScheme keeps Mantine's own scheme in step with data-theme
          without Mantine writing to localStorage under a second key. */}
      <MantineProvider theme={theme} forceColorScheme={resolved}>
        {children}
      </MantineProvider>
    </ThemeContext.Provider>
  );
}
