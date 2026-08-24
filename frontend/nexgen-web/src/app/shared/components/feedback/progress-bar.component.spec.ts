import { render, screen } from '@testing-library/angular';
import { describe, expect, it } from 'vitest';

import { ProgressBarComponent } from './progress-bar.component';

describe('app-progress-bar', () => {
  it('reports its position when determinate', async () => {
    await render(ProgressBarComponent, { inputs: { value: 42, label: 'Importing rows' } });

    const bar = screen.getByRole('progressbar');
    expect(bar.getAttribute('aria-valuenow')).toBe('42');
    expect(bar.getAttribute('aria-valuemin')).toBe('0');
    expect(bar.getAttribute('aria-valuemax')).toBe('100');
    expect(bar.getAttribute('aria-label')).toBe('Importing rows');
  });

  it('omits aria-valuenow when indeterminate, rather than inventing a position', async () => {
    await render(ProgressBarComponent, { inputs: { label: 'Refreshing' } });

    const bar = screen.getByRole('progressbar');
    expect(bar.getAttribute('aria-valuenow')).toBeNull();
  });
});
