import { defineConfig, devices } from '@playwright/test';

const PORT = 4300;
const BASE_URL = `http://127.0.0.1:${PORT}`;

/**
 * Scaffold configuration. One smoke spec today; M2-D01 is where E2E becomes a
 * blocking gate.
 */
export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: !!process.env['CI'],
  retries: process.env['CI'] ? 1 : 0,
  reporter: [['list'], ['html', { open: 'never' }]],
  use: {
    baseURL: BASE_URL,
    trace: 'on-first-retry',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
  webServer: {
    command: `npm run start -- --port ${PORT} --host 127.0.0.1`,
    url: BASE_URL,
    reuseExistingServer: !process.env['CI'],
    timeout: 180_000,
  },
});
