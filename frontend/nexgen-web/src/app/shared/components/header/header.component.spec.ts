import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { afterEach, describe, expect, it, vi } from 'vitest';

import { HeaderComponent } from './header.component';

function removeMatchMedia(): void {
  Object.defineProperty(window, 'matchMedia', {
    value: undefined,
    configurable: true,
    writable: true,
  });
}

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

describe('app-header', () => {
  // jsdom-shared-document discipline (R-76, test-setup.ts): a matchMedia mock left in place
  // corrupts every later spec file in the same worker.
  afterEach(() => {
    removeMatchMedia();
  });

  it('emits menuToggled, searchRequested and logoutRequested from their respective controls', async () => {
    mockViewportWidth(1600);
    const menuToggled = vi.fn();
    const searchRequested = vi.fn();
    const logoutRequested = vi.fn();
    await render(HeaderComponent, {
      inputs: { userName: 'Alice', tenantName: 'Acme Manufacturing' },
      on: { menuToggled, searchRequested, logoutRequested },
    });

    await userEvent.click(screen.getByRole('button', { name: 'Toggle navigation menu' }));
    expect(menuToggled).toHaveBeenCalledTimes(1);

    await userEvent.click(screen.getByRole('button', { name: /Search/ }));
    expect(searchRequested).toHaveBeenCalledTimes(1);

    await userEvent.click(screen.getByRole('button', { name: /Account menu/ }));
    await userEvent.click(await screen.findByRole('menuitem', { name: /Log out/ }));
    expect(logoutRequested).toHaveBeenCalledTimes(1);
  });

  it('at the lg band, the tenant name and FY selector render in the main row', async () => {
    mockViewportWidth(1600);
    await render(HeaderComponent, {
      inputs: { userName: 'Alice', tenantName: 'Acme Manufacturing' },
    });

    expect(screen.getByText('Acme Manufacturing')).toBeTruthy();
    expect(screen.getByRole('combobox', { name: 'Financial year' })).toBeTruthy();
  });

  it('below 1024, the tenant name and FY selector move into the user menu, not disappear', async () => {
    mockViewportWidth(900);
    await render(HeaderComponent, {
      inputs: { userName: 'Alice', tenantName: 'Acme Manufacturing' },
    });

    // Not in the main row.
    expect(screen.queryByText('Acme Manufacturing')).toBeNull();

    // Present once the user menu opens.
    await userEvent.click(screen.getByRole('button', { name: /Account menu/ }));
    expect(await screen.findByText('Acme Manufacturing')).toBeTruthy();
    expect(screen.getByRole('combobox', { name: 'Financial year' })).toBeTruthy();
  });
});
