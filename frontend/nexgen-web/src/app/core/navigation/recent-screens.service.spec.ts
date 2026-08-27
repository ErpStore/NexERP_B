import { afterEach, describe, expect, it, vi } from 'vitest';

import { RecentScreensService } from './recent-screens.service';

const A = { label: 'Currency Master', route: '/masters/currencies', screenName: 'Currency' };
const B = { label: 'Vendor Master', route: '/masters/vendors', screenName: 'Vendor' };

describe('RecentScreensService', () => {
  afterEach(() => {
    localStorage.clear();
  });

  it('records visits most-recent-first, de-duplicating an already-recorded route', () => {
    const service = new RecentScreensService();
    service.record(A);
    service.record(B);
    service.record(A); // re-visit — moves back to the front, not duplicated

    expect(service.recent().map((l) => l.route)).toEqual([A.route, B.route]);
  });

  it('caps at 8 entries, dropping the oldest', () => {
    const service = new RecentScreensService();
    for (let i = 0; i < 10; i++) {
      service.record({ label: `Screen ${i}`, route: `/s${i}`, screenName: `S${i}` });
    }

    expect(service.recent()).toHaveLength(8);
    expect(service.recent()[0]?.route).toBe('/s9');
    expect(service.recent().some((l) => l.route === '/s0')).toBe(false);
  });

  it('persists across a fresh service instance (same localStorage)', () => {
    new RecentScreensService().record(A);
    const reloaded = new RecentScreensService();

    expect(reloaded.recent().map((l) => l.route)).toEqual([A.route]);
  });

  it('clear() empties the list and the persisted value', () => {
    const service = new RecentScreensService();
    service.record(A);
    service.clear();

    expect(service.recent()).toEqual([]);
    expect(new RecentScreensService().recent()).toEqual([]);
  });

  it('degrades to an empty list, never throws, when localStorage is hostile', () => {
    const spy = vi.spyOn(Storage.prototype, 'getItem').mockImplementation(() => {
      throw new Error('quota');
    });
    try {
      expect(() => new RecentScreensService()).not.toThrow();
    } finally {
      spy.mockRestore();
    }
  });
});
