import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideLocationMocks } from '@angular/common/testing';
import { TestBed } from '@angular/core/testing';
import { Location } from '@angular/common';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { screen, waitFor } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { afterEach, beforeAll, beforeEach, describe, expect, it } from 'vitest';

import { PermissionService } from '@/app/core/auth/permission.service';
import {
  installGridJsdomSupport,
  uninstallGridJsdomSupport,
} from '@/app/shared/components/data-grid/test-fixtures';
import { provideToast } from '@/app/shared/components/feedback/toast.service';
import { provideConfirmDialog } from '@/app/shared/components/overlay/confirm-dialog.service';
import { installMatchMedia } from '@/app/shared/components/overlay/jsdom-overlay-support';
import { CurrencyListComponent } from './currency-list.component';

const ROW = {
  currId: 1,
  currName: 'US Dollar',
  currSub: 'Cents',
  symbol: '$',
  isSystemDefined: false,
  createdBy: 'alice',
  createdDate: '2026-08-01T00:00:00Z',
};

async function setup(
  rights: Parameters<PermissionService['setRights']>[0],
  path = '/masters/currencies',
) {
  const permissions = new PermissionService();
  permissions.setRights(rights);

  TestBed.configureTestingModule({
    providers: [
      provideHttpClient(),
      provideHttpClientTesting(),
      provideLocationMocks(),
      provideToast(),
      provideConfirmDialog(),
      provideRouter(
        [
          { path: 'masters/currencies', component: CurrencyListComponent },
          {
            path: 'masters/currencies/new',
            component: CurrencyListComponent,
            data: { drawerMode: 'create' },
          },
          {
            path: 'masters/currencies/:id',
            component: CurrencyListComponent,
            data: { drawerMode: 'edit' },
          },
        ],
        withComponentInputBinding(),
      ),
      { provide: PermissionService, useValue: permissions },
    ],
  });
  const harness = await RouterTestingHarness.create(path);
  const http = TestBed.inject(HttpTestingController);
  harness.detectChanges();
  return { harness, http };
}

describe('CurrencyListComponent', () => {
  beforeAll(installMatchMedia);
  beforeEach(installGridJsdomSupport);
  afterEach(uninstallGridJsdomSupport);

  // `DataGridQueryState` is created as an unconditional field initializer (an injection-context
  // requirement — it cannot be deferred behind the template's own `@if`), so it still issues its
  // first request even on a render nobody can see the grid on. Real, disclosed behaviour, not a
  // security gap: the server's own `[RequireScreen]`/`[RequireRight]` refuses it independently
  // (ADR-004) — recorded in the Slice review as a seam `DataGridQueryState` itself would need to
  // close, which is out of this task's scope to change.

  it('renders the zero-rights empty state when the caller has no screen rights at all', async () => {
    const { http } = await setup({});
    http.expectOne(() => true).flush({ status: 403 }, { status: 403, statusText: 'Forbidden' });
    expect(screen.getByText('Your account has no screen permissions yet.')).toBeTruthy();
  });

  it('renders the inline permission-denied surface when Currency is absent from the rights map', async () => {
    const { http } = await setup({
      Dashboard: { view: true, create: false, edit: false, delete: false, hidden: false },
    });
    http.expectOne(() => true).flush({ status: 403 }, { status: 403, statusText: 'Forbidden' });
    expect(screen.getByText('You do not have access to this')).toBeTruthy();
  });

  it('lists currencies from the server, on the server-paged contract', async () => {
    const { http, harness } = await setup({
      Currency: { view: true, create: true, edit: true, delete: true, hidden: false },
    });
    const req = http.expectOne((r) => r.url === '/api/v1/currencies');
    req.flush({ items: [ROW], totalCount: 1, pageNumber: 1, pageSize: 20 });
    harness.detectChanges();
    await waitFor(() => expect(screen.getByText('US Dollar')).toBeTruthy());
  });

  it('hides the New control when the caller lacks Create', async () => {
    const { http, harness } = await setup({
      Currency: { view: true, create: false, edit: false, delete: false, hidden: false },
    });
    http.expectOne(() => true).flush({ items: [], totalCount: 0, pageNumber: 1, pageSize: 20 });
    harness.detectChanges();
    expect(screen.queryByRole('button', { name: 'New currency' })).toBeNull();
  });

  it('navigates to /masters/currencies/new when New currency is activated', async () => {
    const { http, harness } = await setup({
      Currency: { view: true, create: true, edit: false, delete: false, hidden: false },
    });
    http.expectOne(() => true).flush({ items: [], totalCount: 0, pageNumber: 1, pageSize: 20 });
    harness.detectChanges();

    // Two "New currency" controls render once the grid is confirmed empty — the page header's
    // (always present when Create is granted) and the empty state's own convenience action.
    // Either activates the same onNew(); the header's is first in document order.
    await userEvent.click(screen.getAllByRole('button', { name: 'New currency' })[0]!);
    harness.detectChanges();

    const location = TestBed.inject(Location);
    await waitFor(() => expect(location.path()).toBe('/masters/currencies/new'));
  });
});
