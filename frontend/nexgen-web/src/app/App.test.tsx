import { render, screen } from '@testing-library/react';
import { expect, test } from 'vitest';

import { App } from './App';

// This test exists to prove the PROVIDER STACK COMPOSES. A wrong provider order
// -- MantineProvider outside QueryClientProvider, a missing i18n instance --
// must fail here, not three tasks later. The assertion on the heading is
// incidental; the render is the subject.
test('renders the placeholder route through the full provider stack', () => {
  render(<App />);

  expect(screen.getByRole('heading', { level: 1 }).textContent).toBe('NexGen ERP');
});
