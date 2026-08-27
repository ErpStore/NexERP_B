import { afterEach, describe, expect, it, vi } from 'vitest';

import { FavouritesService } from './favourites.service';

const A = { label: 'Currency Master', route: '/masters/currencies', screenName: 'Currency' };

describe('FavouritesService', () => {
  afterEach(() => {
    localStorage.clear();
  });

  it('toggle adds, then removes, the same link', () => {
    const service = new FavouritesService();

    service.toggle(A);
    expect(service.isFavourite(A.route)).toBe(true);
    expect(service.favourites()).toEqual([A]);

    service.toggle(A);
    expect(service.isFavourite(A.route)).toBe(false);
    expect(service.favourites()).toEqual([]);
  });

  it('persists across a fresh service instance', () => {
    new FavouritesService().toggle(A);
    const reloaded = new FavouritesService();

    expect(reloaded.isFavourite(A.route)).toBe(true);
  });

  it('degrades to an empty list, never throws, when localStorage is hostile', () => {
    const spy = vi.spyOn(Storage.prototype, 'getItem').mockImplementation(() => {
      throw new Error('quota');
    });
    try {
      expect(() => new FavouritesService()).not.toThrow();
    } finally {
      spy.mockRestore();
    }
  });
});
