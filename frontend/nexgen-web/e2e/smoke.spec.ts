import { expect, test } from '@playwright/test';

test('the placeholder route renders the application name', async ({ page }) => {
  await page.goto('/');
  await expect(page.getByRole('heading', { level: 1 })).toHaveText('NexGen ERP');
  await expect(page.getByTestId('build-version')).toContainText('Version');
});
