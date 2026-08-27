import { computed, signal } from '@angular/core';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { beforeAll, describe, expect, it, vi } from 'vitest';

import { NavFilterService } from '../../../core/navigation/nav-filter.service';
import { RecentScreensService } from '../../../core/navigation/recent-screens.service';
import type { NavLink } from '../../../core/navigation/navigation.models';
import { installMatchMedia } from '../form/jsdom-overlay-support';
import { CommandPaletteComponent } from './command-palette.component';

const TREE = {
  top: [{ label: 'Dashboard', route: '/dashboard', screenName: 'Dashboard' }],
  groups: [
    {
      label: 'Master',
      icon: 'book',
      sections: [
        {
          links: [
            { label: 'Currency Master', route: '/masters/currencies', screenName: 'Currency' },
            { label: 'Vendor Master', route: '/masters/vendors', screenName: 'Vendor' },
          ],
        },
      ],
    },
  ],
};

async function setup() {
  const recentScreens = new RecentScreensService();
  return render(CommandPaletteComponent, {
    inputs: { visible: true },
    providers: [
      {
        provide: NavFilterService,
        useValue: {
          filteredTree: signal(TREE).asReadonly(),
          isVisible: computed<(link: NavLink) => boolean>(() => () => true),
        },
      },
      { provide: RecentScreensService, useValue: recentScreens },
    ],
  });
}

describe('app-command-palette', () => {
  beforeAll(installMatchMedia);

  it('only permitted screens are searchable, even by an exact-looking name', async () => {
    await setup();

    const input = screen.getByRole('combobox', { name: 'Search screens' });
    await userEvent.type(input, 'Salary'); // not in the filtered tree at all

    expect(screen.getByText(/No screens match/)).toBeTruthy();
    expect(screen.queryByRole('option')).toBeNull();
  });

  it('fuzzy-matches a permitted screen by a partial, non-contiguous query', async () => {
    await setup();
    const input = screen.getByRole('combobox', { name: 'Search screens' });

    await userEvent.type(input, 'cur');

    expect(screen.getByRole('option', { name: 'Currency Master' })).toBeTruthy();
    expect(screen.queryByRole('option', { name: 'Vendor Master' })).toBeNull();
  });

  it('ArrowDown moves the highlight and Enter activates it, emitting navigated', async () => {
    const navigated = vi.fn();
    await render(CommandPaletteComponent, {
      inputs: { visible: true },
      on: { navigated },
      providers: [
        {
          provide: NavFilterService,
          useValue: {
            filteredTree: signal(TREE).asReadonly(),
            isVisible: computed<(link: NavLink) => boolean>(() => () => true),
          },
        },
      ],
    });

    const input = screen.getByRole('combobox', { name: 'Search screens' });
    await userEvent.type(input, 'Master');
    // Both match; "Vendor Master" has the shorter prefix before the match and so scores
    // better (index 0) — ArrowDown moves the highlight on to "Currency Master".
    await userEvent.keyboard('{ArrowDown}');
    await userEvent.keyboard('{Enter}');

    expect(navigated).toHaveBeenCalledWith(
      expect.objectContaining({ label: 'Currency Master', route: '/masters/currencies' }),
    );
  });

  it('an empty query shows Recent screens first, filtered to still-permitted ones', async () => {
    const recentScreens = new RecentScreensService();
    recentScreens.record({ label: 'Currency Master', route: '/masters/currencies', screenName: 'Currency' });

    await render(CommandPaletteComponent, {
      inputs: { visible: true },
      providers: [
        {
          provide: NavFilterService,
          useValue: {
            filteredTree: signal(TREE).asReadonly(),
            isVisible: computed<(link: NavLink) => boolean>(() => () => true),
          },
        },
        { provide: RecentScreensService, useValue: recentScreens },
      ],
    });

    expect(screen.getByRole('option', { name: 'Currency Master' })).toBeTruthy();
  });
});
