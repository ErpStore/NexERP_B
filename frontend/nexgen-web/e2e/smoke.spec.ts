import { expect, test } from '@playwright/test';

// M2-C02 put the root route behind authGuard: an anonymous visitor to "/" is redirected to
// /login rather than seeing the placeholder directly. See app.component.spec.ts for the same
// change's unit-level equivalent.
test('an anonymous visitor to "/" is redirected to /login', async ({ page }) => {
  await page.goto('/');
  await expect(page).toHaveURL(/\/login(\?.*)?$/);
  await expect(page.getByRole('heading', { level: 1 })).toHaveText('Sign in');
});
