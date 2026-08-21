import { render, screen } from '@testing-library/angular';
import { describe, expect, it } from 'vitest';

import { PlaceholderComponent } from './placeholder.component';

describe('PlaceholderComponent', () => {
  it('shows the application name as the single level-1 heading', async () => {
    await render(PlaceholderComponent);

    expect(screen.getByRole('heading', { level: 1 }).textContent).toContain('NexGen ERP');
    expect(screen.getByTestId('build-version').textContent).toContain('Version');
  });
});
