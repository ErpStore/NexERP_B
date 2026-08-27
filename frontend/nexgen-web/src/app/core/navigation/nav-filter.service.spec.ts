import { TestBed } from '@angular/core/testing';
import { describe, expect, it } from 'vitest';

import { PermissionService } from '../auth/permission.service';
import { NavFilterService } from './nav-filter.service';
import { flattenLinks } from './navigation.models';

function serviceWithRights(rights: Parameters<PermissionService['setRights']>[0]): NavFilterService {
  const permissions = new PermissionService();
  permissions.setRights(rights);
  TestBed.configureTestingModule({
    providers: [{ provide: PermissionService, useValue: permissions }],
  });
  return TestBed.inject(NavFilterService);
}

describe('NavFilterService — deny-by-default over the real NAVIGATION_TREE', () => {
  it('with rights for 3 screens, exactly those 3 named-screenName links survive filtering', () => {
    const filter = serviceWithRights({
      Dashboard: { view: true, create: false, edit: false, delete: false, hidden: false },
      Currency: { view: true, create: false, edit: false, delete: false, hidden: false },
      User: { view: true, create: false, edit: false, delete: false, hidden: false },
    });

    // Every link with a real screenName is filtered on it; "Price Comparison" (the one
    // null-screenName item, INV-033) is never gated, so it survives regardless.
    const links = flattenLinks(filter.filteredTree()).filter((l) => l.screenName !== null);
    expect(links.map((l) => l.screenName).sort()).toEqual(['Currency', 'Dashboard', 'User']);
  });

  it('a screenName absent from the rights map is denied (deny-by-default)', () => {
    const filter = serviceWithRights({
      Dashboard: { view: true, create: false, edit: false, delete: false, hidden: false },
    });

    expect(filter.isVisible()({ label: 'x', route: '/x', screenName: 'Currency' })).toBe(false);
  });

  it('view: true, hidden: true is denied — hidden is not a second grant', () => {
    const filter = serviceWithRights({
      Currency: { view: true, create: false, edit: false, delete: false, hidden: true },
    });

    expect(filter.isVisible()({ label: 'x', route: '/x', screenName: 'Currency' })).toBe(false);
  });

  it('view: false is denied regardless of hidden', () => {
    const filter = serviceWithRights({
      Currency: { view: false, create: false, edit: false, delete: false, hidden: false },
    });

    expect(filter.isVisible()({ label: 'x', route: '/x', screenName: 'Currency' })).toBe(false);
  });

  it('a null screenName is always visible, with zero rights bootstrapped', () => {
    const filter = serviceWithRights({});

    expect(filter.isVisible()({ label: 'x', route: '/x', screenName: null })).toBe(true);
  });

  it('a group with every child filtered out is absent from the filtered tree', () => {
    const filter = serviceWithRights({
      Dashboard: { view: true, create: false, edit: false, delete: false, hidden: false },
    });

    expect(filter.filteredTree().groups.some((g) => g.label === 'Maintenance')).toBe(false);
  });

  it('a section with every link filtered out is absent, but its sibling section survives', () => {
    const filter = serviceWithRights({
      Dashboard: { view: true, create: false, edit: false, delete: false, hidden: false },
      // Only one item from Master > Admin Master; every other Master section is empty.
      User: { view: true, create: false, edit: false, delete: false, hidden: false },
    });

    const master = filter.filteredTree().groups.find((g) => g.label === 'Master');
    expect(master?.sections).toHaveLength(1);
    expect(master?.sections[0]?.heading).toBe('Admin Master');
  });
});
