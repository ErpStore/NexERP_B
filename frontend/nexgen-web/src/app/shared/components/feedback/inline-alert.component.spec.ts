import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';

import { InlineAlertComponent } from './inline-alert.component';

const SERVER_MESSAGE = 'Cannot delete this Sales Order as a Sales DC transaction exists.';

describe('app-inline-alert', () => {
  it('renders a business-rule message verbatim, assertively', async () => {
    await render(InlineAlertComponent, {
      inputs: { severity: 'error', message: SERVER_MESSAGE },
    });

    const alert = screen.getByRole('alert');
    expect(alert.textContent).toContain(SERVER_MESSAGE);
    expect(alert.getAttribute('aria-live')).toBe('assertive');
  });

  it('states the severity in words as well as colour', async () => {
    const view = await render(InlineAlertComponent, {
      inputs: { severity: 'warn', message: 'Price list expires tomorrow.' },
    });

    expect(screen.getByText('Warning:')).toBeTruthy();
    expect(view.container.querySelector('svg')).toBeTruthy();
  });

  it('announces politely for everything short of an error', async () => {
    await render(InlineAlertComponent, {
      inputs: { severity: 'info', message: 'Prices are indicative.' },
    });

    expect(screen.getByRole('alert').getAttribute('aria-live')).toBe('polite');
  });

  it('dismisses only when the caller allows it', async () => {
    const dismissed = vi.fn();
    const view = await render(InlineAlertComponent, {
      inputs: { severity: 'info', message: 'Prices are indicative.', dismissible: true },
      on: { dismissed },
    });

    await userEvent.click(screen.getByRole('button', { name: 'Dismiss' }));
    view.fixture.detectChanges();

    expect(dismissed).toHaveBeenCalledTimes(1);
    expect(screen.queryByText('Prices are indicative.')).toBeNull();
  });

  it('has no dismiss control by default', async () => {
    await render(InlineAlertComponent, {
      inputs: { severity: 'error', message: SERVER_MESSAGE },
    });

    expect(screen.queryByRole('button', { name: 'Dismiss' })).toBeNull();
  });
});
