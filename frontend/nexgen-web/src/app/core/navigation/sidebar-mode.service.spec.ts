import { afterEach, describe, expect, it, vi } from 'vitest';

import { SidebarModeService } from './sidebar-mode.service';

describe('SidebarModeService', () => {
  afterEach(() => {
    localStorage.clear();
  });

  it('defaults to expanded', () => {
    expect(new SidebarModeService().mode()).toBe('expanded');
  });

  it('toggle flips between expanded and rail, and persists', () => {
    const service = new SidebarModeService();

    service.toggle();
    expect(service.mode()).toBe('rail');
    expect(new SidebarModeService().mode()).toBe('rail');

    service.toggle();
    expect(service.mode()).toBe('expanded');
  });

  it('degrades to expanded, never throws, when localStorage is hostile', () => {
    const spy = vi.spyOn(Storage.prototype, 'getItem').mockImplementation(() => {
      throw new Error('quota');
    });
    try {
      expect(() => new SidebarModeService().mode()).not.toThrow();
      expect(new SidebarModeService().mode()).toBe('expanded');
    } finally {
      spy.mockRestore();
    }
  });
});
