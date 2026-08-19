import { fileURLToPath, URL } from 'node:url';

import react from '@vitejs/plugin-react';
import { defineConfig } from 'vitest/config';

import pkg from './package.json' with { type: 'json' };

export default defineConfig({
  // Build identity for the placeholder route. package.json version, resolved at
  // config time -- there is no git dependency in the build.
  define: {
    __APP_VERSION__: JSON.stringify(pkg.version),
  },
  plugins: [react()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  test: {
    environment: 'jsdom',
    setupFiles: ['src/test/setup.ts'],
    // Explicit imports, no ambient describe/it/expect. Keeps test files honest
    // about what they depend on and keeps tsconfig free of vitest globals.
    globals: false,
    include: ['src/**/*.{test,spec}.{ts,tsx}'],
    // e2e/ is Playwright's; Vitest must not try to run it.
    exclude: ['node_modules/**', 'dist/**', 'e2e/**'],
    coverage: {
      provider: 'v8',
      reporter: ['text', 'lcov'],
      include: ['src/**/*.{ts,tsx}'],
      exclude: ['src/test/**', 'src/**/*.test.{ts,tsx}', 'src/main.tsx', 'src/**/*.d.ts'],
      // Thresholds are set to the MEASURED starting value (M2-C01), so the
      // number can only ever be raised. Do not lower these.
      thresholds: {
        statements: 82,
        branches: 100,
        functions: 80,
        lines: 82,
      },
    },
  },
});
