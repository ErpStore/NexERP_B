import { TestBed } from '@angular/core/testing';
import { beforeEach, describe, expect, it } from 'vitest';

import { DENIED_SCREEN_RIGHT } from './auth.models';
import { PermissionService } from './permission.service';

describe('PermissionService', () => {
  let service: PermissionService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(PermissionService);
  });

  it('an absent screen key is denied on every right — matching RightsHelper.cs deny-by-default', () => {
    service.setRights({
      Currency: { view: true, create: true, edit: true, delete: true, hidden: false },
    });

    expect(service.forScreen('Sales Order')()).toEqual(DENIED_SCREEN_RIGHT);
    expect(service.has('Sales Order', 'view')).toBe(false);
  });

  it('a present screen key returns its own rights, view: true and hidden: true both hold at once', () => {
    service.setRights({
      'Sales Order': { view: true, create: false, edit: false, delete: false, hidden: true },
    });

    const right = service.forScreen('Sales Order')();

    // hidden is a navigation-listing hint, never a second access gate (auth.models.ts).
    expect(right.view).toBe(true);
    expect(right.hidden).toBe(true);
  });

  it('hasBootstrapped is false until setRights has been called at least once', () => {
    expect(service.hasBootstrapped()).toBe(false);

    service.setRights({});

    expect(service.hasBootstrapped()).toBe(true);
  });

  it('hasNoRights is true only once bootstrapped with an empty map — not before bootstrap', () => {
    expect(service.hasNoRights()).toBe(false); // not bootstrapped yet — not the same as "zero rights"

    service.setRights({});

    expect(service.hasNoRights()).toBe(true);

    service.setRights({
      Currency: { view: true, create: false, edit: false, delete: false, hidden: false },
    });

    expect(service.hasNoRights()).toBe(false);
  });

  it('clear resets both the rights map and the bootstrapped flag', () => {
    service.setRights({
      Currency: { view: true, create: false, edit: false, delete: false, hidden: false },
    });

    service.clear();

    expect(service.hasBootstrapped()).toBe(false);
    expect(service.forScreen('Currency')()).toEqual(DENIED_SCREEN_RIGHT);
  });
});
