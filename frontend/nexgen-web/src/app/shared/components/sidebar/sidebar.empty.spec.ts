import { computed, signal } from '@angular/core';
import { provideRouter } from '@angular/router';
import { render, screen } from '@testing-library/angular';
import { describe, expect, it } from 'vitest';

import { NavFilterService } from '../../../core/navigation/nav-filter.service';
import type { NavLink } from '../../../core/navigation/navigation.models';
import { SidebarComponent } from './sidebar.component';

describe('app-sidebar — zero-permission empty state', () => {
  it('a caller with no permitted screens sees the explanatory message, not an empty rail', async () => {
    const stub = {
      filteredTree: signal({ top: [], groups: [] }).asReadonly(),
      isVisible: computed<(link: NavLink) => boolean>(() => () => false),
    };

    await render(SidebarComponent, {
      inputs: { mode: 'expanded' },
      providers: [provideRouter([]), { provide: NavFilterService, useValue: stub }],
    });

    expect(screen.getByText('No screens are available for your account.')).toBeTruthy();
    expect(screen.getByText('Contact your administrator.')).toBeTruthy();
    expect(screen.queryByRole('link')).toBeNull();
    expect(screen.queryByRole('button')).toBeNull();
  });
});
