import { afterAll, afterEach, beforeAll } from 'vitest';

import { server } from './msw/server';

// onUnhandledRequest: 'error' is the point of the harness. A test that reaches
// the network is a test that is not testing what it claims to.
beforeAll(() => {
  server.listen({ onUnhandledRequest: 'error' });
});
afterEach(() => {
  server.resetHandlers();
});
afterAll(() => {
  server.close();
});

// jsdom implements neither of these; Mantine's responsive styles call both.
if (!window.matchMedia) {
  window.matchMedia = (query: string) => ({
    matches: false,
    media: query,
    onchange: null,
    addListener: () => {},
    removeListener: () => {},
    addEventListener: () => {},
    removeEventListener: () => {},
    dispatchEvent: () => false,
  });
}
