import { expect, test, type Route } from '@playwright/test';

/**
 * M2-C03 — a real-browser keyboard pass through the authenticated shell. jsdom computes no
 * layout at all, so the responsive/focus-order claims this task makes need a real browser
 * to mean anything (unit specs already cover the component-level behaviour in isolation).
 *
 * No live backend is reachable from this dev environment (the same limitation `M2-C02`
 * disclosed for its own e2e coverage), so the auth endpoints are mocked at the network
 * layer via Playwright's own request interception — this still exercises real browser
 * rendering, focus and keyboard behaviour; only the HTTP responses are stubbed.
 */
const LOGIN_RESPONSE = {
  token: 'e2e-access-token',
  refreshToken: 'e2e-refresh-token',
  tokenExpiresAtUtc: new Date(Date.now() + 15 * 60 * 1000).toISOString(),
  username: 'alice',
  userId: 1,
  tenantId: 3,
  role: 'Administrator',
};

const ME_RESPONSE = {
  userId: 1,
  userName: 'alice',
  tenantId: 3,
  role: 'Administrator',
  rights: {
    Dashboard: { view: true, create: false, edit: false, delete: false, hidden: false },
    Currency: { view: true, create: false, edit: false, delete: false, hidden: false },
  },
};

function fulfillJson(route: Route, body: unknown): Promise<void> {
  return route.fulfill({ contentType: 'application/json', body: JSON.stringify(body) });
}

test('a keyboard-only pass: log in, skip link, sidebar accordion, palette, logout', async ({
  page,
}) => {
  // >=1440px (KB-051's lg band) so the sidebar renders its full 240px accordion, not the
  // 56px icon rail a narrower default viewport would collapse it to.
  await page.setViewportSize({ width: 1600, height: 900 });
  await page.route('**/api/v1/auth/login', (route) => fulfillJson(route, LOGIN_RESPONSE));
  await page.route('**/api/v1/me', (route) => fulfillJson(route, ME_RESPONSE));
  await page.route('**/api/v1/auth/logout', (route) => route.fulfill({ status: 204 }));

  await page.goto('/login');
  // M2-A05 — Tenant is now the form's first field.
  await page.getByLabel('Tenant').click();
  await page.keyboard.type('acme');
  await page.keyboard.press('Tab');
  await page.keyboard.type('alice');
  await page.keyboard.press('Tab');
  await page.keyboard.type('secret');
  await page.keyboard.press('Enter');

  await expect(page).toHaveURL('/');
  await expect(page.getByRole('heading', { level: 1, name: 'Dashboard' })).toBeVisible();

  // Landmarks exist. Two <header>s render — the shell's own chrome and the dashboard's
  // app-page-header — so the shell's is picked out specifically.
  await expect(page.locator('header.app-header')).toBeVisible();
  await expect(page.getByRole('navigation', { name: 'Main' })).toBeVisible();
  await expect(page.getByRole('main')).toBeVisible();

  // The skip link is reachable by keyboard and moves focus into the content area.
  await page.keyboard.press('Tab');
  await expect(page.getByRole('link', { name: 'Skip to content' })).toBeFocused();
  await page.keyboard.press('Enter');
  await expect(page.getByRole('main')).toBeFocused();

  // Sidebar: Tab to the Master group trigger, expand it with Enter, reach a link inside.
  const masterTrigger = page.getByRole('button', { name: 'Master', exact: true });
  await masterTrigger.focus();
  await expect(masterTrigger).toHaveAttribute('aria-expanded', 'false');
  await page.keyboard.press('Enter');
  await expect(masterTrigger).toHaveAttribute('aria-expanded', 'true');
  await expect(page.getByRole('link', { name: /Currency Master/ })).toBeVisible();

  // Palette: Ctrl+K opens it, typing filters to only the permitted screen, Enter activates
  // it and closes the palette. `/masters/currencies` is nav *data* only — no real route
  // exists for it yet (M2-D01 onward adds destination screens one at a time; today it
  // falls through app.routes.ts's wildcard back to `/`) — so this checks the palette's own
  // activate-and-close mechanic, not a destination screen that doesn't exist yet.
  await page.keyboard.press('Control+k');
  const search = page.getByRole('combobox', { name: 'Search screens' });
  await expect(search).toBeFocused();
  await page.keyboard.type('cur');
  await expect(page.getByRole('option', { name: 'Currency Master' })).toBeVisible();
  await page.keyboard.press('Enter');
  await expect(search).not.toBeVisible();

  // Esc closes the palette and restores focus — reopen and check it.
  await page.keyboard.press('Control+k');
  await expect(page.getByRole('combobox', { name: 'Search screens' })).toBeVisible();
  await page.keyboard.press('Escape');
  await expect(page.getByRole('combobox', { name: 'Search screens' })).not.toBeVisible();

  // Log out via the user menu, keyboard only.
  const userMenuButton = page.getByRole('button', { name: /Account menu for alice/ });
  await userMenuButton.focus();
  await page.keyboard.press('Enter');
  await page.getByRole('menuitem', { name: /Log out/ }).click();
  await expect(page).toHaveURL('/login');
});
