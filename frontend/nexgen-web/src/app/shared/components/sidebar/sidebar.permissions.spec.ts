import { computed, signal } from '@angular/core';
import { provideRouter } from '@angular/router';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';

import { NavFilterService } from '../../../core/navigation/nav-filter.service';
import type { NavLink, NavTree } from '../../../core/navigation/navigation.models';
import { SidebarComponent } from './sidebar.component';

/**
 * The actual deny-by-default / `hidden` / null-`screenName` *logic* is
 * `nav-filter.service.spec.ts`'s job (`core/navigation/`, which may depend on the auth
 * layer's permission service). This file proves `SidebarComponent` faithfully **renders**
 * whatever `NavFilterService` hands it — nothing under `shared/components/` may import
 * from the authentication module (`permission-denied-state.component.spec.ts`'s repo-wide
 * scan enforces this), so a stub `NavFilterService` stands in here instead.
 */
function stubNavFilter(tree: NavTree): Pick<NavFilterService, 'filteredTree' | 'isVisible'> {
  return {
    filteredTree: signal(tree).asReadonly(),
    isVisible: computed<(link: NavLink) => boolean>(() => () => true),
  };
}

async function renderWithTree(tree: NavTree) {
  return render(SidebarComponent, {
    inputs: { mode: 'expanded' },
    providers: [provideRouter([]), { provide: NavFilterService, useValue: stubNavFilter(tree) }],
  });
}

describe('app-sidebar — renders exactly the tree NavFilterService hands it', () => {
  it('renders every top link and every group/section/link the filtered tree contains', async () => {
    await renderWithTree({
      top: [{ label: 'Dashboard', route: '/dashboard', screenName: 'Dashboard' }],
      groups: [
        {
          label: 'Master',
          icon: 'book',
          sections: [
            {
              heading: 'Account Master',
              links: [
                { label: 'Currency Master', route: '/masters/currencies', screenName: 'Currency' },
              ],
            },
          ],
        },
      ],
    });

    expect(screen.getByRole('link', { name: /Dashboard/ })).toBeTruthy();
    await userEvent.click(screen.getByRole('button', { name: 'Master' }));
    expect(screen.getByRole('link', { name: /Currency Master/ })).toBeTruthy();
  });

  it('renders nothing beyond what the filtered tree contains — an excluded item never appears', async () => {
    await renderWithTree({
      top: [{ label: 'Dashboard', route: '/dashboard', screenName: 'Dashboard' }],
      groups: [],
    });

    expect(screen.queryByRole('button', { name: 'Master' })).toBeNull();
    expect(screen.queryByRole('link', { name: /Currency Master/ })).toBeNull();
  });
});
