import { expect, test } from '@playwright/test';

test('the placeholder route renders the app name', async ({ page }) => {
  await page.goto('/');
  await expect(page.getByRole('heading', { level: 1 })).toHaveText('NexGen ERP');
});
