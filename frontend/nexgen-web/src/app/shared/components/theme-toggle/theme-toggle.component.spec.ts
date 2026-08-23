import { readFileSync } from 'node:fs';
import { resolve } from 'node:path';

import { TestBed } from '@angular/core/testing';
import { fireEvent, render, screen } from '@testing-library/angular';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';

import { ThemeService } from '@/app/core/theme/theme.service';
import { ThemeToggleComponent } from './theme-toggle.component';

function radios(): HTMLElement[] {
  return screen.getAllByRole('radio');
}

describe('ThemeToggleComponent', () => {
  beforeEach(() => {
    localStorage.clear();
  });

  afterEach(() => {
    localStorage.clear();
    document.documentElement.removeAttribute('data-theme');
    document.documentElement.removeAttribute('data-density');
  });

  it('is a radio group with an accessible name and three named options', async () => {
    await render(ThemeToggleComponent);

    expect(screen.getByRole('radiogroup', { name: 'Colour theme' })).toBeDefined();
    expect(radios().map((r) => r.textContent?.trim())).toEqual(['Light', 'Dark', 'System']);
  });

  it('announces the selected mode, and only one, through aria-checked', async () => {
    await render(ThemeToggleComponent);

    expect(radios().map((r) => r.getAttribute('aria-checked'))).toEqual(['false', 'false', 'true']);
  });

  it('is reachable by Tab as a single stop: a roving tabindex', async () => {
    await render(ThemeToggleComponent);

    // The selected option is the group's tab stop; the others are skipped and
    // reached with the arrow keys instead. That is the ARIA radio-group
    // pattern, and it is why the group costs one Tab press, not three.
    expect(radios().map((r) => r.getAttribute('tabindex'))).toEqual(['-1', '-1', '0']);
  });

  it('changes the preference when an option is activated', async () => {
    await render(ThemeToggleComponent);
    const theme = TestBed.inject(ThemeService);

    fireEvent.click(screen.getByRole('radio', { name: 'Dark' }));

    expect(theme.preference()).toBe('dark');
    expect(document.documentElement.getAttribute('data-theme')).toBe('dark');
  });

  it('does not re-create the host component when the theme changes', async () => {
    const { fixture } = await render(ThemeToggleComponent);
    const instanceBefore = fixture.componentInstance;
    const nodeBefore = screen.getByRole('radiogroup');

    fireEvent.click(screen.getByRole('radio', { name: 'Light' }));
    fixture.detectChanges();

    expect(fixture.componentInstance).toBe(instanceBefore);
    expect(screen.getByRole('radiogroup')).toBe(nodeBefore);
    expect(radios().map((r) => r.getAttribute('aria-checked'))).toEqual(['true', 'false', 'false']);
  });

  it('moves the selection with the arrow keys, wrapping at both ends', async () => {
    const { fixture } = await render(ThemeToggleComponent);
    const theme = TestBed.inject(ThemeService);

    // Starts on System (index 2). ArrowRight wraps to Light.
    fireEvent.keyDown(radios()[2] as HTMLElement, { key: 'ArrowRight' });
    fixture.detectChanges();
    expect(theme.preference()).toBe('light');

    fireEvent.keyDown(radios()[0] as HTMLElement, { key: 'ArrowLeft' });
    fixture.detectChanges();
    expect(theme.preference()).toBe('system');

    fireEvent.keyDown(radios()[2] as HTMLElement, { key: 'Home' });
    fixture.detectChanges();
    expect(theme.preference()).toBe('light');

    fireEvent.keyDown(radios()[0] as HTMLElement, { key: 'End' });
    fixture.detectChanges();
    expect(theme.preference()).toBe('system');
  });

  it('ignores keys that are not part of the radio-group pattern', async () => {
    await render(ThemeToggleComponent);
    const theme = TestBed.inject(ThemeService);

    fireEvent.keyDown(radios()[2] as HTMLElement, { key: 'a' });

    expect(theme.preference()).toBe('system');
  });

  it('uses a native button, so Enter and Space activate it without extra code', async () => {
    await render(ThemeToggleComponent);

    // jsdom does not synthesise the user-agent's Enter/Space -> click
    // translation, so this asserts the mechanism rather than replaying it:
    // every option is a real <button type="button">, which the UA activates
    // on both keys. The manual keyboard pass at review covers the rest.
    for (const option of radios()) {
      expect(option.tagName).toBe('BUTTON');
      expect(option.getAttribute('type')).toBe('button');
    }
  });

  it('honours prefers-reduced-motion through the global stylesheet', () => {
    // jsdom applies no stylesheet and computes no layout, so this asserts the
    // stylesheet TEXT, not a computed style. The real check is the manual
    // pass with reduced motion enabled, required at review.
    const styles = readFileSync(resolve(process.cwd(), 'src/styles/styles.scss'), 'utf8');
    expect(styles).toMatch(/@media \(prefers-reduced-motion: reduce\)/);
    expect(styles).toMatch(/--motion-fast:\s*0ms;/);
    expect(styles).toMatch(/transition-duration:\s*0ms !important;/);
  });
});
