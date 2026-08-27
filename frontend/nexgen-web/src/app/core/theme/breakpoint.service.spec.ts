import { TestBed } from '@angular/core/testing';
import { afterEach, describe, expect, it } from 'vitest';

import { BreakpointService } from './breakpoint.service';

/** jsdom has no `window.matchMedia` at all — mocked per test, matching a fixed viewport
 * width against the same `min-width` queries `minWidthQuery()` builds. */
function mockViewportWidth(width: number): void {
  const implementation = (query: string): MediaQueryList => {
    const match = /min-width:\s*(\d+)px/.exec(query);
    const threshold = match ? Number(match[1]) : 0;
    return {
      matches: width >= threshold,
      media: query,
      onchange: null,
      addEventListener: () => undefined,
      removeEventListener: () => undefined,
      addListener: () => undefined,
      removeListener: () => undefined,
      dispatchEvent: () => false,
    };
  };
  Object.defineProperty(window, 'matchMedia', {
    value: implementation,
    configurable: true,
    writable: true,
  });
}

function removeMatchMedia(): void {
  Object.defineProperty(window, 'matchMedia', {
    value: undefined,
    configurable: true,
    writable: true,
  });
}

describe('BreakpointService', () => {
  // jsdom-shared-document discipline (R-76, test-setup.ts): a matchMedia mock left in place
  // corrupts every later spec file in the same worker, including PrimeNG overlays that
  // consult it internally — leaking one here previously hung ~10 unrelated files.
  afterEach(() => {
    removeMatchMedia();
  });

  it('reports lg at 1440 and above, with the full-sidebar convenience true', () => {
    mockViewportWidth(1600);
    const service = TestBed.inject(BreakpointService);

    expect(service.band()).toBe('lg');
    expect(service.isFullSidebar()).toBe(true);
    expect(service.isRailSidebar()).toBe(false);
  });

  it('reports md (rail) between 1024 and 1439', () => {
    mockViewportWidth(1200);
    const service = TestBed.inject(BreakpointService);

    expect(service.band()).toBe('md');
    expect(service.isRailSidebar()).toBe(true);
  });

  it('reports sm (overlay) between 768 and 1023', () => {
    mockViewportWidth(900);
    const service = TestBed.inject(BreakpointService);

    expect(service.band()).toBe('sm');
    expect(service.isOverlaySidebar()).toBe(true);
  });

  it('reports xs (read-only) below 768', () => {
    mockViewportWidth(500);
    const service = TestBed.inject(BreakpointService);

    expect(service.band()).toBe('xs');
    expect(service.isReadOnlyBand()).toBe(true);
  });

  it('degrades to xs, not a throw, on a platform with no matchMedia at all', () => {
    removeMatchMedia();
    const service = TestBed.inject(BreakpointService);

    expect(service.band()).toBe('xs');
  });
});
