import { act, cleanup, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import axe from 'axe-core';
import { useEffect } from 'react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { readGlobalCss } from '@/test/tokens-source';

import { type Density, isDensity } from './density';
import { DENSITY_STORAGE_KEY, ThemeProvider, useTheme } from './ThemeProvider';
import { ThemeToggle } from './ThemeToggle';
import {
  COLOR_SCHEME_PREFERENCES,
  DARK_MEDIA_QUERY,
  THEME_STORAGE_KEY,
  useColorScheme,
} from './useColorScheme';

/**
 * A controllable matchMedia. jsdom has none worth the name, and the harness stub
 * in src/test/setup.ts cannot be flipped -- and "the OS theme changed while the
 * user was looking at the page" is precisely the behaviour under test.
 */
type Listener = (event: MediaQueryListEvent) => void;
let systemDark = false;
const listeners = new Set<Listener>();

function installMatchMedia() {
  systemDark = false;
  listeners.clear();
  window.matchMedia = ((query: string) => ({
    media: query,
    matches: query === DARK_MEDIA_QUERY ? systemDark : false,
    onchange: null,
    addListener: () => {},
    removeListener: () => {},
    addEventListener: (_type: string, listener: Listener) => listeners.add(listener),
    removeEventListener: (_type: string, listener: Listener) => listeners.delete(listener),
    dispatchEvent: () => false,
  })) as unknown as typeof window.matchMedia;
}

function setSystemDark(value: boolean) {
  // act() because this is React state changing from OUTSIDE React -- the OS
  // theme changed while the page was open, which is the case under test.
  act(() => {
    systemDark = value;
    for (const listener of listeners) {
      listener({ matches: value } as MediaQueryListEvent);
    }
  });
}

/** Counts its own mounts, so "no remount" can be asserted rather than assumed. */
let mountCount = 0;
function MountProbe() {
  useEffect(() => {
    mountCount += 1;
  }, []);
  const { resolved, preference } = useTheme();
  return (
    <div>
      <span data-testid="resolved">{resolved}</span>
      <span data-testid="preference">{preference}</span>
    </div>
  );
}

function renderApp() {
  return render(
    <ThemeProvider>
      <MountProbe />
      <ThemeToggle />
    </ThemeProvider>,
  );
}

/**
 * Exposes the density half of the context. Kept OUT of MountProbe deliberately:
 * MountProbe renders before ThemeToggle, so an extra focusable element there
 * would sit ahead of the radio group and break the single-Tab-stop assertion.
 */
function DensityProbe() {
  const { density, setDensity } = useTheme();
  return (
    <div>
      <span data-testid="density">{density}</span>
      <button
        type="button"
        onClick={() => {
          setDensity('compact');
        }}
      >
        Compact
      </button>
    </div>
  );
}

function renderDensityApp() {
  return render(
    <ThemeProvider>
      <DensityProbe />
    </ThemeProvider>,
  );
}

/**
 * Makes the Storage API hostile, which is not exotic: Safari private mode throws
 * on read and a full quota throws on write. The theme layer must degrade to its
 * defaults rather than stop the app rendering.
 */
function breakStorage(method: 'getItem' | 'setItem') {
  vi.spyOn(Storage.prototype, method).mockImplementation(() => {
    throw new Error('storage unavailable');
  });
}

/** Uses the hook directly, so the no-matchMedia case is not confounded by Mantine. */
function SchemeProbe() {
  const { preference, resolved } = useColorScheme();
  return <span data-testid="scheme">{`${preference}:${resolved}`}</span>;
}

/** Reads the context with no provider above it -- the misuse guard's only caller. */
function OrphanProbe() {
  useTheme();
  return null;
}

const themeAttribute = () => document.documentElement.dataset.theme;

beforeEach(() => {
  mountCount = 0;
  window.localStorage.clear();
  delete document.documentElement.dataset.theme;
  delete document.documentElement.dataset.density;
  installMatchMedia();
});

afterEach(() => {
  // Vitest runs with globals: false, so RTL's automatic cleanup is NOT installed.
  // Without this every render leaks into the next test.
  cleanup();
  vi.restoreAllMocks();
  window.localStorage.clear();
});

describe('colour-scheme preference', () => {
  it('defaults to system and follows prefers-color-scheme', () => {
    setSystemDark(true);
    renderApp();

    expect(screen.getByTestId('preference').textContent).toBe('system');
    expect(screen.getByTestId('resolved').textContent).toBe('dark');
    expect(themeAttribute()).toBe('dark');
  });

  it('flips live when the OS theme changes, with no reload and no remount', () => {
    renderApp();
    expect(screen.getByTestId('resolved').textContent).toBe('light');
    expect(mountCount).toBe(1);

    setSystemDark(true);

    expect(screen.getByTestId('resolved').textContent).toBe('dark');
    expect(themeAttribute()).toBe('dark');
    // The whole point: variable values change, the subtree does not.
    expect(mountCount).toBe(1);
  });

  it('lets an explicit choice override the media query and survive a remount', async () => {
    const user = userEvent.setup();
    setSystemDark(true);
    const view = renderApp();

    await user.click(screen.getByRole('radio', { name: 'Light' }));
    expect(screen.getByTestId('resolved').textContent).toBe('light');
    expect(window.localStorage.getItem(THEME_STORAGE_KEY)).toBe('light');

    view.unmount();
    renderApp();
    expect(screen.getByTestId('preference').textContent).toBe('light');
    expect(screen.getByTestId('resolved').textContent).toBe('light');
  });

  it('falls back to system on a corrupt stored value without throwing', () => {
    window.localStorage.setItem(THEME_STORAGE_KEY, 'chartreuse');
    setSystemDark(true);

    expect(() => renderApp()).not.toThrow();
    expect(screen.getByTestId('preference').textContent).toBe('system');
    expect(screen.getByTestId('resolved').textContent).toBe('dark');
  });

  it('switching theme mutates attributes only -- never the subtree', async () => {
    const user = userEvent.setup();
    renderApp();

    await user.click(screen.getByRole('radio', { name: 'Dark' }));
    expect(themeAttribute()).toBe('dark');
    await user.click(screen.getByRole('radio', { name: 'Light' }));
    expect(themeAttribute()).toBe('light');

    expect(mountCount).toBe(1);
  });
});

describe('ThemeToggle keyboard model', () => {
  it('is one Tab stop, arrow-navigable, and announces the selected mode', async () => {
    const user = userEvent.setup();
    renderApp();

    expect(screen.getByRole('radiogroup', { name: 'Colour scheme' })).toBeDefined();

    await user.tab();
    // Roving tabindex: focus lands on the selected option, which is System.
    expect(document.activeElement).toBe(screen.getByRole('radio', { name: 'System' }));
    expect(screen.getByRole('radio', { name: 'System' }).getAttribute('aria-checked')).toBe('true');

    await user.keyboard('{ArrowRight}');
    expect(screen.getByTestId('preference').textContent).toBe('light');
    expect(document.activeElement).toBe(screen.getByRole('radio', { name: 'Light' }));

    await user.keyboard('{ArrowLeft}');
    expect(screen.getByTestId('preference').textContent).toBe('system');

    // Native button activation covers both Enter and Space.
    await user.keyboard('{ArrowRight}{ArrowRight}');
    expect(screen.getByTestId('preference').textContent).toBe('dark');
    await user.keyboard('{Enter}');
    expect(screen.getByTestId('preference').textContent).toBe('dark');
    await user.keyboard('[Space]');
    expect(screen.getByTestId('preference').textContent).toBe('dark');
  });

  it('walks the arrow-key ring in COLOR_SCHEME_PREFERENCES order', async () => {
    // ThemeToggle's RING is written out per preference rather than computed by
    // indexing the tuple. That removes two unreachable guards, but it also lets
    // the two drift -- so the agreement is asserted, not assumed.
    const user = userEvent.setup();
    renderApp();
    expect(screen.getAllByRole('radio')).toHaveLength(COLOR_SCHEME_PREFERENCES.length);

    await user.tab();
    const walked: string[] = [];
    for (let lap = 0; lap < COLOR_SCHEME_PREFERENCES.length; lap += 1) {
      await user.keyboard('{ArrowRight}');
      walked.push(screen.getByTestId('preference').textContent ?? '');
    }

    // Starting from the default 'system', one full lap must reproduce the
    // render order exactly.
    expect(walked).toEqual([...COLOR_SCHEME_PREFERENCES]);
  });

  it.each(['light', 'dark'] as const)(
    'reports no critical axe violation in the %s theme',
    async (scheme) => {
      window.localStorage.setItem(THEME_STORAGE_KEY, scheme);
      const { container } = renderApp();

      const results = await axe.run(container, {
        // jsdom computes no layout, so colour-contrast cannot run here. It is
        // covered exhaustively and deterministically by contrast.test.ts instead.
        rules: { 'color-contrast': { enabled: false } },
      });
      const critical = results.violations.filter((violation) => violation.impact === 'critical');
      expect(critical.map((violation) => violation.id)).toEqual([]);
    },
  );
});

describe('density', () => {
  it('defaults to default density and writes it onto <html>', () => {
    renderDensityApp();
    expect(screen.getByTestId('density').textContent).toBe('default');
    expect(document.documentElement.dataset.density).toBe('default');
  });

  it('restores a stored density', () => {
    window.localStorage.setItem(DENSITY_STORAGE_KEY, 'compact' satisfies Density);
    renderDensityApp();
    expect(screen.getByTestId('density').textContent).toBe('compact');
    expect(document.documentElement.dataset.density).toBe('compact');
  });

  it('ignores a corrupt stored density without throwing', () => {
    window.localStorage.setItem(DENSITY_STORAGE_KEY, 'roomy');
    expect(() => renderDensityApp()).not.toThrow();
    expect(screen.getByTestId('density').textContent).toBe('default');
  });

  it('persists a density change', async () => {
    const user = userEvent.setup();
    renderDensityApp();

    await user.click(screen.getByRole('button', { name: 'Compact' }));

    expect(screen.getByTestId('density').textContent).toBe('compact');
    expect(document.documentElement.dataset.density).toBe('compact');
    expect(window.localStorage.getItem(DENSITY_STORAGE_KEY)).toBe('compact');
  });

  it('rejects a non-string candidate', () => {
    expect(isDensity(null)).toBe(false);
    expect(isDensity(30)).toBe(false);
    expect(isDensity('compact')).toBe(true);
  });
});

describe('hostile localStorage', () => {
  it('falls back to system and default density when reads throw', () => {
    breakStorage('getItem');
    setSystemDark(true);

    expect(() => renderDensityApp()).not.toThrow();
    expect(screen.getByTestId('density').textContent).toBe('default');

    cleanup();
    expect(() => renderApp()).not.toThrow();
    expect(screen.getByTestId('preference').textContent).toBe('system');
    expect(screen.getByTestId('resolved').textContent).toBe('dark');
  });

  it('still applies a density change for the session when writes throw', async () => {
    const user = userEvent.setup();
    renderDensityApp();
    breakStorage('setItem');

    await user.click(screen.getByRole('button', { name: 'Compact' }));

    expect(screen.getByTestId('density').textContent).toBe('compact');
    expect(document.documentElement.dataset.density).toBe('compact');
  });

  it('still applies a scheme change for the session when writes throw', async () => {
    const user = userEvent.setup();
    renderApp();
    breakStorage('setItem');

    await user.click(screen.getByRole('radio', { name: 'Dark' }));

    expect(screen.getByTestId('preference').textContent).toBe('dark');
    expect(themeAttribute()).toBe('dark');
  });
});

describe('degraded environments', () => {
  it('resolves to light where matchMedia does not exist', () => {
    // Old WebViews and non-browser renderers have no matchMedia. Losing the OS
    // signal must cost the live subscription, not the render.
    Reflect.deleteProperty(window, 'matchMedia');

    expect(() => render(<SchemeProbe />)).not.toThrow();
    expect(screen.getByTestId('scheme').textContent).toBe('system:light');
  });

  it('refuses to hand out the theme context outside the provider', () => {
    expect(() => render(<OrphanProbe />)).toThrow(/useTheme must be used inside/);
  });
});

describe('global stylesheet commitments', () => {
  /**
   * jsdom applies no stylesheet and evaluates no media query, so a computed-style
   * assertion here would pass against an empty rule and prove nothing. The
   * stylesheet itself is the artefact under test; a human pass with
   * prefers-reduced-motion enabled is required at review.
   */
  it('neutralises every transition duration under reduced motion', () => {
    const css = readGlobalCss();
    const block = /@media \(prefers-reduced-motion: reduce\) \{([\s\S]*?)\n\}/.exec(css);
    expect(block?.[1]).toBeDefined();
    const body = block?.[1] ?? '';
    expect(body).toContain('*,');
    expect(body).toMatch(/transition-duration:\s*0s\s*!important/);
    expect(body).toMatch(/animation-duration:\s*0\.01ms\s*!important/);
  });

  it('draws a 2px focus ring at 2px offset from the focus-ring token', () => {
    const css = readGlobalCss();
    expect(css).toMatch(/:focus-visible \{\s*outline: 2px solid var\(--focus-ring\);/);
    expect(css).toMatch(/outline-offset: 2px;/);
  });

  it('defaults numeric contexts to tabular figures', () => {
    expect(readGlobalCss()).toMatch(/font-variant-numeric: tabular-nums/);
  });

  it('self-hosts both faces with font-display: swap and no CDN', () => {
    const css = readGlobalCss();
    expect(css).toContain("font-family: 'Inter'");
    expect(css).toContain("font-family: 'JetBrains Mono'");
    expect(css).not.toMatch(/fonts\.googleapis\.com|fonts\.gstatic\.com/);
    const faces = css.match(/@font-face/g) ?? [];
    const swaps = css.match(/font-display: swap/g) ?? [];
    expect(swaps).toHaveLength(faces.length);
    for (const src of css.match(/src: url\('([^']+)'\)/g) ?? []) {
      expect(src).toContain("url('/fonts/");
    }
  });
});
