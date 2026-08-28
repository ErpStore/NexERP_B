import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideLocationMocks } from '@angular/common/testing';
import { TestBed } from '@angular/core/testing';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import axe, { type Result } from 'axe-core';
import { afterAll, beforeAll, describe, expect, it } from 'vitest';

import { PermissionService } from '@/app/core/auth/permission.service';
import {
  installGridJsdomSupport,
  uninstallGridJsdomSupport,
} from '@/app/shared/components/data-grid/test-fixtures';
import { provideToast } from '@/app/shared/components/feedback/toast.service';
import { provideConfirmDialog } from '@/app/shared/components/overlay/confirm-dialog.service';
import { installMatchMedia } from '@/app/shared/components/overlay/jsdom-overlay-support';
import { CurrencyListComponent } from './pages/currency-list/currency-list.component';

/**
 * M2-D01 — runtime accessibility scan over the Currency list, the first real
 * `app-data-grid` consumer. Mirrors `data-grid/data-grid.a11y.spec.ts`'s own method and its
 * disclosed jsdom limitation (`color-contrast` cannot run without a stylesheet; contrast is
 * covered by computation in `core/theme/contrast.spec.ts`).
 */
const AXE_IS_SLOW = 60_000;

async function violations(root: HTMLElement, theme: 'light' | 'dark'): Promise<Result[]> {
  document.documentElement.setAttribute('data-theme', theme);
  const results = await axe.run(root, {
    resultTypes: ['violations'],
    rules: { 'color-contrast': { enabled: false } },
  });
  return results.violations.filter((violation) => violation.impact === 'critical');
}

describe('Currency list accessibility', () => {
  beforeAll(() => {
    installMatchMedia();
    installGridJsdomSupport();
  });
  afterAll(() => {
    uninstallGridJsdomSupport();
    document.documentElement.removeAttribute('data-theme');
  });

  it(
    'reports no critical axe violation on a populated grid, in either theme',
    async () => {
      const permissions = new PermissionService();
      permissions.setRights({
        Currency: { view: true, create: true, edit: true, delete: true, hidden: false },
      });
      TestBed.configureTestingModule({
        providers: [
          provideHttpClient(),
          provideHttpClientTesting(),
          provideLocationMocks(),
          provideToast(),
          provideConfirmDialog(),
          provideRouter(
            [{ path: 'masters/currencies', component: CurrencyListComponent }],
            withComponentInputBinding(),
          ),
          { provide: PermissionService, useValue: permissions },
        ],
      });
      const harness = await RouterTestingHarness.create('/masters/currencies');
      const http = TestBed.inject(HttpTestingController);
      http
        .expectOne(() => true)
        .flush({
          items: [
            {
              currId: 1,
              currName: 'US Dollar',
              currSub: 'Cents',
              symbol: '$',
              isSystemDefined: false,
            },
            { currId: 2, currName: 'Euro', currSub: 'Cents', symbol: '€', isSystemDefined: true },
          ],
          totalCount: 2,
          pageNumber: 1,
          pageSize: 20,
        });
      harness.detectChanges();
      await harness.fixture.whenStable();
      const root = harness.routeNativeElement as HTMLElement;

      expect(await violations(root, 'light')).toEqual([]);
      expect(await violations(root, 'dark')).toEqual([]);
    },
    AXE_IS_SLOW,
  );

  it(
    'reports no critical axe violation on the permission-denied surface',
    async () => {
      const permissions = new PermissionService();
      permissions.setRights({
        Dashboard: { view: true, create: false, edit: false, delete: false, hidden: false },
      });
      TestBed.configureTestingModule({
        providers: [
          provideHttpClient(),
          provideHttpClientTesting(),
          provideLocationMocks(),
          provideToast(),
          provideConfirmDialog(),
          provideRouter(
            [{ path: 'masters/currencies', component: CurrencyListComponent }],
            withComponentInputBinding(),
          ),
          { provide: PermissionService, useValue: permissions },
        ],
      });
      const harness = await RouterTestingHarness.create('/masters/currencies');
      harness.detectChanges();
      const root = harness.routeNativeElement as HTMLElement;

      expect(await violations(root, 'light')).toEqual([]);
    },
    AXE_IS_SLOW,
  );
});
