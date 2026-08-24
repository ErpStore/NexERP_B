import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';

import { EmptyStateComponent } from './empty-state.component';

describe('app-empty-state', () => {
  it('offers the create action for the "no data yet" variant', async () => {
    const action = vi.fn();
    await render(EmptyStateComponent, {
      inputs: {
        variant: 'no-data',
        title: 'No sales orders yet',
        description: 'Create one to get started.',
        actionLabel: 'New Sales Order',
      },
      on: { action },
    });

    expect(screen.getByRole('heading', { name: 'No sales orders yet' })).toBeTruthy();
    expect(screen.queryByRole('button', { name: 'Clear filters' })).toBeNull();

    await userEvent.click(screen.getByRole('button', { name: 'New Sales Order' }));

    expect(action).toHaveBeenCalledTimes(1);
  });

  it('offers Clear filters for the "filtered out" variant, and no create action', async () => {
    const clearFilters = vi.fn();
    await render(EmptyStateComponent, {
      inputs: {
        variant: 'filtered',
        title: 'No results for these filters',
        actionLabel: 'New Sales Order',
      },
      on: { clearFilters },
    });

    expect(screen.getByRole('heading', { name: 'No results for these filters' })).toBeTruthy();
    expect(screen.queryByRole('button', { name: 'New Sales Order' })).toBeNull();

    await userEvent.click(screen.getByRole('button', { name: 'Clear filters' }));

    expect(clearFilters).toHaveBeenCalledTimes(1);
  });

  it('marks the two variants apart in the DOM, so a screen cannot conflate them', async () => {
    const view = await render(EmptyStateComponent, {
      inputs: { variant: 'no-data', title: 'No sales orders yet' },
    });

    expect(view.container.querySelector('[data-variant="no-data"]')).toBeTruthy();
    expect(view.container.querySelector('[data-variant="filtered"]')).toBeNull();
  });

  it('is a real heading and a status region, not a paragraph in the middle of a page', async () => {
    await render(EmptyStateComponent, {
      inputs: { variant: 'no-data', title: 'No sales orders yet' },
    });

    expect(screen.getByRole('status')).toBeTruthy();
    expect(screen.getByRole('heading', { level: 2 })).toBeTruthy();
  });
});
