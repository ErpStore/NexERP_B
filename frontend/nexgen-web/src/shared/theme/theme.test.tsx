import { act, cleanup, render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import axe from 'axe-core';
import { useEffect } from 'react';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { readGlobalCss } from '@/test/tokens-source';

import { DENSITY_STORAGE_KEY, ThemeProvider, useTheme } from './ThemeProvider';
import { ThemeToggle } from './ThemeToggle';
import {
  COLOR_SCHEME_PREFERENCES,
  DARK_MEDIA_QUERY,
  THEME_STORAGE_KEY,
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
