import { ApplicationInitStatus } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideLocationMocks } from '@angular/common/testing';
import { Router } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { TranslateService } from '@ngx-translate/core';
import { PrimeNG } from 'primeng/config';
import { describe, expect, it, beforeEach, afterEach } from 'vitest';

import { AppComponent } from './app.component';
import { appConfig } from './app.config';
import { AppConfigService } from './core/config/app-config';

/**
 * This spec exists to prove that the *real* provider set in app.config.ts
 * composes. A broken providePrimeNG, a missing router registration or a
 * mis-wired i18n loader must fail here, not three tasks later.
 */
describe('AppComponent, through the real app.config.ts providers', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    TestBed.configureTestingModule({
      providers: [...appConfig.providers, provideHttpClientTesting(), provideLocationMocks()],
    });

    http = TestBed.inject(HttpTestingController);
    http.expectOne('config/app-config.json').flush({ apiBaseUrl: '/api' });
    await TestBed.inject(ApplicationInitStatus).donePromise;
  });

  afterEach(() => {
    http.verify();
  });

  it('resolves the runtime API base URL from configuration, not from source', () => {
    expect(TestBed.inject(AppConfigService).apiBaseUrl).toBe('/api');
  });

  it('provides PrimeNG and the router', () => {
    expect(TestBed.inject(PrimeNG)).toBeTruthy();
    expect(TestBed.inject(Router).config.length).toBeGreaterThan(0);
  });

  it('provides a runtime-switchable i18n service with the en bundle', async () => {
    const translate = TestBed.inject(TranslateService);
    expect(await translate.get('app.name').toPromise()).toBe('NexGen ERP');
  });

  it('renders the lazily loaded /login route through the root component', async () => {
    // M2-C02 — the root path now sits behind authGuard; an anonymous caller (the only kind
    // this spec's providers can produce, with no session established) is redirected to
    // /login rather than reaching the placeholder directly. This spec's own job — proving
    // the real app.config.ts provider set composes, guards included — is still exactly what
    // this asserts; only the destination changed, correctly, with the guard's arrival.
    const harness = await RouterTestingHarness.create();
    await harness.navigateByUrl('/');
    const heading = harness.routeNativeElement?.querySelector('h1');
    expect(heading?.textContent).toContain('Sign in');
    expect(TestBed.inject(Router).url).toBe('/login?returnUrl=%2F');
  });

  it('creates the root component', () => {
    const fixture = TestBed.createComponent(AppComponent);
    expect(fixture.componentInstance).toBeTruthy();
  });
});
