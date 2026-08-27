import { describe, expect, it } from 'vitest';

import { NAVIGATION_TREE } from './navigation.config';
import { flattenLinks } from './navigation.models';

describe('NAVIGATION_TREE (INV-033)', () => {
  const links = flattenLinks(NAVIGATION_TREE);

  it('has no duplicate routes', () => {
    const routes = links.map((l) => l.route);
    const duplicates = routes.filter((route, i) => routes.indexOf(route) !== i);
    expect(duplicates).toEqual([]);
  });

  it('every route is absolute and kebab-case', () => {
    for (const link of links) {
      expect(link.route.startsWith('/')).toBe(true);
      expect(link.route).toMatch(/^\/[a-z0-9/-]+$/);
    }
  });

  it('every group has at least one link across its sections', () => {
    for (const group of NAVIGATION_TREE.groups) {
      const count = group.sections.reduce((sum, section) => sum + section.links.length, 0);
      expect(count).toBeGreaterThan(0);
    }
  });

  it('exactly one item — Price Comparison — has a null screenName, and it is documented', () => {
    const nullEntries = links.filter((l) => l.screenName === null);
    expect(nullEntries.map((l) => l.label)).toEqual(['Price Comparison']);
  });

  it('every other item has a non-empty screenName string', () => {
    for (const link of links) {
      if (link.label === 'Price Comparison') {
        continue;
      }
      expect(typeof link.screenName).toBe('string');
      expect((link.screenName ?? '').length).toBeGreaterThan(0);
    }
  });

  it('Dashboard is the one top-level link, and carries the screenName M2-C02 already guards on', () => {
    expect(NAVIGATION_TREE.top).toHaveLength(1);
    expect(NAVIGATION_TREE.top[0]).toMatchObject({
      route: '/dashboard',
      screenName: 'Dashboard',
    });
  });

  it('instantSearch has no entry anywhere — replaced by the command palette', () => {
    expect(links.some((l) => l.route.includes('instant-search'))).toBe(false);
    expect(links.some((l) => l.label.toLowerCase().includes('instant search'))).toBe(false);
  });
});
