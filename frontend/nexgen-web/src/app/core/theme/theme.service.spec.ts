import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { DENSITY_STORAGE_KEY, THEME_STORAGE_KEY, ThemeService } from './theme.service';

type MediaListener = (event: MediaQueryListEvent) => void;

let listeners: MediaListener[] = [];

/**
 * jsdom does not implement window.matchMedia at all, so it is installed here
 * rather than spied on. That is also why ThemeService guards for its absence:
 * a platform without matchMedia must still render.
 */
function mockPrefersDark(matches: boolean): void {
  listeners = [];
  const implementation = (query: string): MediaQueryList =>
    ({
      matches,
      media: query,
      onchange: null,
      addEventListener: (_: string, listener: MediaListener) => listeners.push(listener),
      removeEventListener: () => undefined,
      addListener: (listener: MediaListener) => listeners.push(listener),
      removeListener: () => undefined,
      dispatchEvent: () => false,
    }) as unknown as MediaQueryList;
  Object.defineProperty(window, 'matchMedia', {
    value: implementation,
    configurable: true,
    writable: true,
  });
}

function removeMatchMedia(): void {
  listeners = [];
  Object.defineProperty(window, 'matchMedia', {
    value: undefined,
    configurable: true,
    writable: true,
  });
}

/** Simulate the OS switching to dark while the app is running. */
function systemBecomes(dark: boolean): void {
  for (const listener of listeners) {
    listener({ matches: dark } as MediaQueryListEvent);
  }
}

function createService(): ThemeService {
  TestBed.resetTestingModule();
  TestBed.configureTestingModule({});
  return TestBed.inject(ThemeService);
}

function themeAttribute(): string | null {
  return document.documentElement.getAttribute('data-theme');
}

describe('ThemeService', () => {
  beforeEach(() => {
    localStorage.clear();
    mockPrefersDark(false);
  });

  afterEach(() => {
    vi.restoreAllMocks();
    removeMatchMedia();
    localStorage.clear();
    document.documentElement.removeAttribute('data-theme');
    document.documentElement.removeAttribute('data-density');
  });

  it('defaults to system and follows prefers-color-scheme', () => {
    mockPrefersDark(true);
    const service = createService();

    expect(service.preference()).toBe('system');
    expect(service.resolvedScheme()).toBe('dark');
    expect(themeAttribute()).toBe('dark');
  });

  it('follows a live prefers-color-scheme change with no reload', () => {
    const service = createService();
    expect(service.resolvedScheme()).toBe('light');

    systemBecomes(true);

    expect(service.resolvedScheme()).toBe('dark');
    expect(themeAttribute()).toBe('dark');
  });

  it('lets an explicit choice override the media query', () => {
    mockPrefersDark(true);
    const service = createService();

    service.setPreference('light');

    expect(service.resolvedScheme()).toBe('light');
    expect(themeAttribute()).toBe('light');

    systemBecomes(false);
    systemBecomes(true);
    expect(service.resolvedScheme()).toBe('light');
  });

  it('survives a reload: the preference is read back from storage', () => {
    createService().setPreference('dark');
    expect(localStorage.getItem(THEME_STORAGE_KEY)).toBe('dark');

    const reloaded = createService();

    expect(reloaded.preference()).toBe('dark');
    expect(reloaded.resolvedScheme()).toBe('dark');
  });

  it('falls back to system on a corrupt stored preference, without throwing', () => {
    localStorage.setItem(THEME_STORAGE_KEY, 'aubergine');

    const service = createService();

    expect(service.preference()).toBe('system');
    expect(service.resolvedScheme()).toBe('light');
  });

  it('degrades to defaults when localStorage throws (private mode, full quota)', () => {
    vi.spyOn(Storage.prototype, 'getItem').mockImplementation(() => {
      throw new Error('SecurityError');
    });
    vi.spyOn(Storage.prototype, 'setItem').mockImplementation(() => {
      throw new Error('QuotaExceededError');
    });

    const service = createService();
    expect(service.preference()).toBe('system');

    expect(() => service.setPreference('dark')).not.toThrow();
    expect(service.resolvedScheme()).toBe('dark');
  });

  it('keeps working when the platform has no matchMedia at all', () => {
    removeMatchMedia();

    const service = createService();

    expect(service.preference()).toBe('system');
    expect(service.resolvedScheme()).toBe('light');
  });

  describe('density', () => {
    it('defaults to default and writes data-density', () => {
      const service = createService();

      expect(service.density()).toBe('default');
      expect(document.documentElement.getAttribute('data-density')).toBe('default');
    });

    it('persists and restores a change', () => {
      createService().setDensity('compact');
      expect(localStorage.getItem(DENSITY_STORAGE_KEY)).toBe('compact');

      const reloaded = createService();

      expect(reloaded.density()).toBe('compact');
      expect(document.documentElement.getAttribute('data-density')).toBe('compact');
    });

    it('rejects a corrupt stored value', () => {
      localStorage.setItem(DENSITY_STORAGE_KEY, 'roomy');

      expect(createService().density()).toBe('default');
    });
  });

  it('changes only attribute values on <html>, so nothing can be re-created', () => {
    const service = createService();
    const root = document.documentElement;
    const attributesBefore = [...root.attributes].map((a) => a.name).sort();
    const childrenBefore = root.childNodes.length;
    const bodyBefore = document.body;

    service.setPreference('dark');
    service.setDensity('compact');

    // The only mutation is the *value* of two attributes that already exist:
    // no element is added, removed or replaced, so switching theme cannot
    // shift layout or force a component subtree to be rebuilt.
    expect([...root.attributes].map((a) => a.name).sort()).toEqual(attributesBefore);
    expect(root.childNodes.length).toBe(childrenBefore);
    expect(document.body).toBe(bodyBefore);
    expect(themeAttribute()).toBe('dark');
  });
});
