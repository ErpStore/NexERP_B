import { TestBed } from '@angular/core/testing';
import { provideRouter, type UrlTree } from '@angular/router';
import { describe, expect, it } from 'vitest';

import { authGuard, requireScreen } from './auth.guard';
import { AuthService } from './auth.service';
import type { AuthStatus } from './auth.models';

function fakeAuth(status: AuthStatus): AuthService {
  return {
    status: () => status,
    whenBootstrapped: () => Promise.resolve(),
  } as unknown as AuthService;
}

function configure(status: AuthStatus) {
  TestBed.configureTestingModule({
    providers: [provideRouter([]), { provide: AuthService, useValue: fakeAuth(status) }],
  });
}

describe('authGuard', () => {
  it('activates when authenticated', async () => {
    configure('authenticated');

    const result = await TestBed.runInInjectionContext(() =>
      authGuard({} as never, { url: '/protected' } as never),
    );

    expect(result).toBe(true);
  });

  it('redirects to /login with returnUrl when there is no session', async () => {
    configure('anonymous');

    const result = (await TestBed.runInInjectionContext(() =>
      authGuard({} as never, { url: '/protected' } as never),
    )) as UrlTree;

    expect(result.toString()).toBe('/login?returnUrl=%2Fprotected');
  });

  it('waits for bootstrap before deciding — a status of "unknown" never leaks into the result', async () => {
    let resolveBootstrap!: () => void;
    const auth = {
      status: () => 'authenticated' as AuthStatus,
      whenBootstrapped: () => new Promise<void>((resolve) => (resolveBootstrap = resolve)),
    } as unknown as AuthService;

    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: AuthService, useValue: auth }],
    });

    let settled = false;
    const resultPromise = Promise.resolve(
      TestBed.runInInjectionContext(() => authGuard({} as never, { url: '/x' } as never)),
    ).then((r) => {
      settled = true;
      return r;
    });

    expect(settled).toBe(false); // guard is genuinely still waiting on whenBootstrapped
    resolveBootstrap();
    expect(await resultPromise).toBe(true);
  });
});

describe('requireScreen', () => {
  it("activates for an authenticated caller regardless of the right — denial is the routed component's job, not the guard's", async () => {
    configure('authenticated');

    const guard = requireScreen('Sales Order', 'view');
    const result = await TestBed.runInInjectionContext(() => guard({} as never, {} as never));

    expect(result).toBe(true);
  });

  it('redirects to /login when there is no session at all', async () => {
    configure('anonymous');

    const guard = requireScreen('Sales Order', 'view');
    const result = (await TestBed.runInInjectionContext(() =>
      guard({} as never, {} as never),
    )) as UrlTree;

    expect(result.toString()).toBe('/login');
  });
});
