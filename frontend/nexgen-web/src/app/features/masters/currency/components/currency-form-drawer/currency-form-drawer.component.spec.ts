import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { render, screen, waitFor } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { beforeAll, describe, expect, it, vi } from 'vitest';

import { provideToast, ToastService } from '@/app/shared/components/feedback/toast.service';
import { installMatchMedia } from '@/app/shared/components/overlay/jsdom-overlay-support';
import { CurrencyFormDrawerComponent } from './currency-form-drawer.component';

const ROW = {
  currId: 1,
  currName: 'US Dollar',
  currSub: 'Cents',
  symbol: '$',
  isSystemDefined: false,
};

// Every field here is required, so `app-form-field` renders a trailing `*` marker inside the
// `<label>` (aria-hidden, but still part of its text content) — `getByLabelText`'s exact-match
// default would need the literal "Currency name *", so every query below matches by prefix
// instead of the field's bare name.
const CURRENCY_NAME = /^Currency name/;
const CURR_SUB = /^Sub currency name/;
const SYMBOL = /^Symbol/;

async function setup(
  overrides: Partial<{ mode: 'create' | 'edit'; currencyId: number | null; canEdit: boolean }> = {},
) {
  const saved: void[] = [];
  const { fixture } = await render(CurrencyFormDrawerComponent, {
    inputs: {
      visible: true,
      mode: overrides.mode ?? 'create',
      currencyId: overrides.currencyId ?? null,
      canEdit: overrides.canEdit ?? true,
    },
    on: { saved: () => saved.push(undefined) },
    providers: [provideHttpClient(), provideHttpClientTesting(), provideToast()],
  });
  const http = fixture.debugElement.injector.get(HttpTestingController);
  return { fixture, http, saved };
}

describe('CurrencyFormDrawerComponent', () => {
  beforeAll(installMatchMedia);

  it("shows the server's verbatim message when a required field is left empty", async () => {
    const { http } = await setup();
    await userEvent.click(screen.getByRole('button', { name: 'Save' }));

    expect(screen.getByText('Please Enter Currency Name')).toBeTruthy();
    expect(screen.getByText('Please Enter Sub Currency Name')).toBeTruthy();
    expect(screen.getByText('Please Enter Symbol')).toBeTruthy();
    http.expectNone(() => true);
  });

  it("accepts a Symbol the server's own unanchored regex accepts, even though it looks stricter", async () => {
    const { http } = await setup();
    await userEvent.type(screen.getByLabelText(CURRENCY_NAME), 'Test Dollar');
    await userEvent.type(screen.getByLabelText(CURR_SUB), 'Cents');
    await userEvent.type(screen.getByLabelText(SYMBOL), '$$');
    await userEvent.click(screen.getByRole('button', { name: 'Save' }));

    const req = http.expectOne('/api/v1/currencies');
    expect(req.request.body).toEqual({ currName: 'Test Dollar', currSub: 'Cents', symbol: '$$' });
    req.flush(
      { currId: 9, currName: 'Test Dollar', currSub: 'Cents', symbol: '$$' },
      { status: 201, statusText: 'Created' },
    );
  });

  it('emits saved and toasts on a successful create', async () => {
    const { http, saved, fixture } = await setup();
    // No `<p-toast>` host is rendered in this component-level test — that lives in
    // `app.component.html`, out of this file's scope — so the toast call is spied rather than
    // asserted by rendered text.
    const toastSuccess = vi.spyOn(fixture.debugElement.injector.get(ToastService), 'success');

    await userEvent.type(screen.getByLabelText(CURRENCY_NAME), 'Euro');
    await userEvent.type(screen.getByLabelText(CURR_SUB), 'Cents');
    await userEvent.type(screen.getByLabelText(SYMBOL), '€');
    await userEvent.click(screen.getByRole('button', { name: 'Save' }));

    http
      .expectOne('/api/v1/currencies')
      .flush(
        { currId: 2, currName: 'Euro', currSub: 'Cents', symbol: '€' },
        { status: 201, statusText: 'Created' },
      );

    await waitFor(() => expect(saved.length).toBe(1));
    expect(toastSuccess).toHaveBeenCalledWith('Currency created.');
  });

  it('maps a 400 ProblemDetails.errors dictionary onto the matching controls, by key', async () => {
    const { http } = await setup();
    await userEvent.type(screen.getByLabelText(CURRENCY_NAME), 'Duplicate');
    await userEvent.type(screen.getByLabelText(CURR_SUB), 'Cents');
    await userEvent.type(screen.getByLabelText(SYMBOL), '$');
    await userEvent.click(screen.getByRole('button', { name: 'Save' }));

    http.expectOne('/api/v1/currencies').flush(
      {
        title: 'One or more validation errors occurred.',
        errors: { CurrName: ['That name is already in use.'] },
      },
      { status: 400, statusText: 'Bad Request' },
    );

    await waitFor(() => expect(screen.getByText('That name is already in use.')).toBeTruthy());
  });

  it('shows a 409 business-rule refusal as the form-level message, verbatim', async () => {
    const { http } = await setup({ mode: 'edit', currencyId: 1 });
    http.expectOne('/api/v1/currencies/1').flush(ROW);
    await waitFor(() =>
      expect(screen.getByLabelText<HTMLInputElement>(CURRENCY_NAME).value).toBe('US Dollar'),
    );

    await userEvent.click(screen.getByRole('button', { name: 'Save' }));
    http
      .expectOne('/api/v1/currencies/1')
      .flush({ title: 'Currency name already exists.' }, { status: 409, statusText: 'Conflict' });

    await waitFor(() => expect(screen.getByText('Currency name already exists.')).toBeTruthy());
  });

  it("disables the form for a system-defined currency, mirroring CurrencyUpsert.razor's own client-side refusal", async () => {
    const { http } = await setup({ mode: 'edit', currencyId: 1, canEdit: true });
    http.expectOne('/api/v1/currencies/1').flush({ ...ROW, isSystemDefined: true });

    await waitFor(() =>
      expect(
        screen.getByText('This is a system-defined currency and cannot be edited.'),
      ).toBeTruthy(),
    );
    expect(screen.queryByRole('button', { name: 'Save' })).toBeNull();
  });
});
