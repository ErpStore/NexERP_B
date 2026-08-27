import { provideRouter } from '@angular/router';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';

import type { NavGroup } from '../../../core/navigation/navigation.models';
import { NavGroupComponent } from './nav-group.component';

const GROUP: NavGroup = {
  label: 'Master',
  icon: 'book',
  sections: [
    {
      heading: 'Admin Master',
      links: [{ label: 'User Master', route: '/admin/users', screenName: 'User' }],
    },
    {
      links: [{ label: 'My Company Details', route: '/settings/company', screenName: 'Company' }],
    },
  ],
};

describe('app-nav-group (expanded mode)', () => {
  it('is collapsed by default and expands on click, revealing its sections', async () => {
    await render(NavGroupComponent, {
      inputs: { group: GROUP },
      providers: [provideRouter([])],
    });

    expect(screen.queryByText('User Master')).toBeNull();

    const trigger = screen.getByRole('button', { name: 'Master' });
    expect(trigger.getAttribute('aria-expanded')).toBe('false');

    await userEvent.click(trigger);

    expect(trigger.getAttribute('aria-expanded')).toBe('true');
    expect(screen.getByText('Admin Master')).toBeTruthy();
    expect(screen.getByRole('link', { name: /User Master/ })).toBeTruthy();
    // A section with no heading still renders its links.
    expect(screen.getByRole('link', { name: /My Company Details/ })).toBeTruthy();
  });
});

describe('app-nav-group (rail mode)', () => {
  it('shows only the icon trigger, no inline panel', async () => {
    const { container } = await render(NavGroupComponent, {
      inputs: { group: GROUP, rail: true },
      providers: [provideRouter([])],
    });

    expect(screen.getByRole('button', { name: 'Master' })).toBeTruthy();
    expect(container.querySelector('.app-nav-group__panel')).toBeNull();
    expect(screen.queryByText('User Master')).toBeNull();
  });
});
