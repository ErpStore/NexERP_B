import { fileURLToPath, URL } from 'node:url';

import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

import pkg from './package.json' with { type: 'json' };

// Build configuration only. No API base URL is baked in here: the app reads
// VITE_API_BASE_URL at runtime-of-build from the environment (see .env.example),
// deliberately with no default, so a missing configuration fails loudly instead
// of silently pointing at localhost the way the Angular pilot does
// (frontend/vsmart-erp/src/environments/environment.prod.ts:3).
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
  build: {
    // One explicit vendor chunk so the entry chunk stays measurable against
    // KB-050's "< 250 KB gzip" budget. Feature-level splitting is React.lazy in
    // the route tree, not manualChunks.
    rollupOptions: {
      output: {
        manualChunks: {
          react: ['react', 'react-dom', 'react-router'],
        },
      },
    },
  },
});
