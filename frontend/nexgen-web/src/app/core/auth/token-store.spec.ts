import { describe, expect, it } from 'vitest';

import { TokenStore } from './token-store';

describe('TokenStore', () => {
  it('starts with no session', () => {
    const store = new TokenStore();

    expect(store.hasSession()).toBe(false);
    expect(store.accessToken).toBeNull();
    expect(store.refreshToken).toBeNull();
    expect(store.accessTokenExpiresAtUtc).toBeNull();
    expect(store.tenant).toBeNull();
  });

  it('setSession stores all four fields and hasSession becomes true', () => {
    const store = new TokenStore();
    const expiry = new Date('2026-01-01T00:15:00Z');

    store.setSession('access-1', 'refresh-1', expiry, 'acme');

    expect(store.accessToken).toBe('access-1');
    expect(store.refreshToken).toBe('refresh-1');
    expect(store.accessTokenExpiresAtUtc).toBe(expiry);
    expect(store.tenant).toBe('acme');
    expect(store.hasSession()).toBe(true);
  });

  it('rotate replaces both tokens together but leaves tenant untouched', () => {
    const store = new TokenStore();
    store.setSession('access-1', 'refresh-1', new Date(), 'acme');

    const newExpiry = new Date('2026-01-01T00:30:00Z');
    store.rotate('access-2', 'refresh-2', newExpiry);

    expect(store.accessToken).toBe('access-2');
    expect(store.refreshToken).toBe('refresh-2');
    expect(store.accessTokenExpiresAtUtc).toBe(newExpiry);
    expect(store.tenant).toBe('acme');
  });

  it('clear resets everything and hasSession becomes false', () => {
    const store = new TokenStore();
    store.setSession('access-1', 'refresh-1', new Date(), 'acme');

    store.clear();

    expect(store.accessToken).toBeNull();
    expect(store.refreshToken).toBeNull();
    expect(store.accessTokenExpiresAtUtc).toBeNull();
    expect(store.tenant).toBeNull();
    expect(store.hasSession()).toBe(false);
  });

  it('never appears as an own enumerable property — the whole point of `#`-private state', () => {
    const store = new TokenStore();
    store.setSession('super-secret-access-token', 'super-secret-refresh-token', new Date(), 'acme');

    const serialised = JSON.stringify(store);

    expect(serialised).toBe('{}');
    expect(Object.keys(store)).toEqual([]);
  });
});
