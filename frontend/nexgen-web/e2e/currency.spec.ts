import { expect, test, type Route } from '@playwright/test';

/**
 * M2-D01 — the Currency Master's first real-browser pass: login → list → create → edit →
 * delete, and the permission-denied rendering for a rights-less caller.
 *
 * No live backend is reachable from this dev environment — the same disclosed limitation
 * `M2-C03`'s `shell.spec.ts` and `M2-A05` both recorded — so every `/api/v1/*` call is
 * mocked at the network layer via Playwright's own request interception. This proves real
 * browser rendering, routing and keyboard/focus behaviour end to end; it does **not** prove
 * the server actually refuses a rights-less caller — that is `[RequireScreen]`/`[RequireRight]`
 * enforcement, already covered server-side by `AuthControllerTenantBindingTests`-style xUnit
 * tests and the CI permission-matrix harness (`M2-A03`), not by this file. The second spec
 * below proves only that the **client** renders the inline permission-denied surface when the
 * server's rights map omits the screen — matching what a real 403/absent-right response would
 * produce, per `PermissionService`'s own deny-by-default contract.
 */

const LOGIN_RESPONSE = {
  token: 'e2e-access-token',
  refreshToken: 'e2e-refresh-token',
  tokenExpiresAtUtc: new Date(Date.now() + 15 * 60 * 1000).toISOString(),
  username: 'alice',
  userId: 1,
  tenantId: 3,
  role: 'Administrator',
};

function meResponse(rights: Record<string, unknown>) {
  return { userId: 1, userName: 'alice', tenantId: 3, role: 'Administrator', rights };
}

const FULL_RIGHTS = {
  Dashboard: { view: true, create: false, edit: false, delete: false, hidden: false },
  Currency: { view: true, create: true, edit: true, delete: true, hidden: false },
};

const CURRENCY_ROW = {
  currId: 1,
  currName: 'US Dollar',
  currSub: 'Cents',
  symbol: '$',
  isSystemDefined: false,
  createdBy: 'alice',
  createdDate: '2026-08-01T00:00:00Z',
};

function fulfillJson(route: Route, body: unknown, status = 200): Promise<void> {
  return route.fulfill({ status, contentType: 'application/json', body: JSON.stringify(body) });
}

/**
 * The input inside one `app-form-field`, addressed by the component's own `label` attribute
 * (reflected verbatim on the host element) rather than by accessible-name label matching.
 *
 * **Measured, not assumed.** `getByLabel()` — even scoped to the dialog, even anchored with a
 * `^`-prefixed regex — resolves to zero elements against this exact markup, despite the
 * `<label for="…">`/`<input id="…">` pair being correctly linked (confirmed directly from the
 * rendered `outerHTML`, 2026-08-27); the required-field `*` marker's `<span aria-hidden="true">`
 * is one plausible cause, but the mechanism was not chased further once a reliable alternative
 * was in hand. Recorded here as a real, measured Playwright/PrimeNG interaction rather than
 * silently worked around without comment; a future task revisiting the form layer's e2e
 * reachability should re-check whether this still reproduces.
 */
function fieldInput(scope: import('@playwright/test').Locator, label: string) {
  return scope.locator(`app-form-field[label="${label}"] input`);
}

/**
 * Logs in and lands on `returnUrl` via the app's own client-side redirect
 * (`login.component.ts:127-128`) — never `page.goto()` after login. Tokens live only in real
 * private fields (`TokenStore`, M2-C02's deliberate custody decision), so a hard navigation
 * after login discards the session exactly as a real reload would.
 */
async function login(page: import('@playwright/test').Page, returnUrl = '/'): Promise<void> {
  await page.route('**/api/v1/auth/login', (route) => fulfillJson(route, LOGIN_RESPONSE));
  await page.route('**/api/v1/auth/logout', (route) => route.fulfill({ status: 204 }));
  await page.goto(`/login?returnUrl=${encodeURIComponent(returnUrl)}`);
  await page.getByLabel('Tenant').fill('acme');
  await page.getByLabel('Username').fill('alice');
  await page.getByLabel('Password').fill('secret');
  await page.getByRole('button', { name: 'Sign in' }).click();
  await expect(page).toHaveURL(new RegExp(`${returnUrl}$`));
}

test('login, list, create, edit and delete a currency end to end (network-mocked)', async ({
  page,
}) => {
  await page.setViewportSize({ width: 1600, height: 900 });
  await page.route('**/api/v1/me', (route) => fulfillJson(route, meResponse(FULL_RIGHTS)));

  const rows = [CURRENCY_ROW];
  await page.route('**/api/v1/currencies?**', (route) => {
    if (route.request().method() !== 'GET') {
      return route.fallback();
    }
    return fulfillJson(route, {
      items: rows,
      totalCount: rows.length,
      pageNumber: 1,
      pageSize: 20,
    });
  });

  await login(page, '/masters/currencies');
  await expect(page.getByRole('heading', { level: 1, name: 'Currencies' })).toBeVisible();
  await expect(page.getByText('US Dollar')).toBeVisible();

  // Create.
  await page.route('**/api/v1/currencies', (route) => {
    if (route.request().method() !== 'POST') {
      return route.fallback();
    }
    const created = {
      currId: 2,
      currName: 'Euro',
      currSub: 'Cents',
      symbol: '€',
      isSystemDefined: false,
      createdBy: 'alice',
      createdDate: '2026-08-27T00:00:00Z',
    };
    rows.push(created);
    return fulfillJson(route, created, 201);
  });
  await page.getByRole('button', { name: 'New currency' }).click();
  const createDialog = page.getByRole('dialog', { name: 'New currency' });
  await expect(createDialog).toBeVisible();
  await fieldInput(createDialog, 'Currency name').fill('Euro');
  await fieldInput(createDialog, 'Sub currency name').fill('Cents');
  await fieldInput(createDialog, 'Symbol').fill('€');
  await createDialog.getByRole('button', { name: 'Save' }).click();
  await expect(page.getByText('Currency created.')).toBeVisible();
  await expect(page).toHaveURL(/\/masters\/currencies$/);

  // Edit.
  await page.route('**/api/v1/currencies/1', (route) => {
    const method = route.request().method();
    if (method === 'GET') {
      return fulfillJson(route, CURRENCY_ROW);
    }
    if (method === 'PUT') {
      return fulfillJson(route, { ...CURRENCY_ROW, currName: 'US Dollar (updated)' });
    }
    return route.fallback();
  });
  // app-data-grid activates a row on double-click or Enter-when-focused, not a single click
  // (`data-grid.component.ts`'s `onRowDblClick`/`onKeydown` — a single click alone must not
  // open a record, or every filter-row/sort/resize click would risk it).
  await page.getByText('US Dollar').dblclick();
  const editDialog = page.getByRole('dialog');
  await expect(editDialog).toBeVisible();
  await fieldInput(editDialog, 'Currency name').fill('US Dollar (updated)');
  await editDialog.getByRole('button', { name: 'Save' }).click();
  await expect(page.getByText('Currency updated.')).toBeVisible();

  // Delete, refusal path.
  //
  // **Real gap found here, not fixed locally, reported instead** (this task's own
  // instruction: "if you find yourself needing a special case, stop and report it — it
  // means R-24 is not closed"). `deleteCurrency()`'s generated request sets
  // `responseType: 'text'` (`core/api/generated/fn/currency/delete-currency.ts:23`) because
  // its *success* body is empty (204) — but Angular's `HttpClient` applies the same
  // `responseType` to an *error* response too, so a 409's `application/problem+json` body
  // arrives as a raw string, not an object. `error.interceptor.ts`'s `normaliseProblem()`
  // only recognises an object-shaped body (`error.interceptor.ts:42`), so the string falls
  // through to its network-failure fallback (`{ status }`, no `title`) and the toast shows
  // the client's own generic fallback, not the server's message. Every DELETE endpoint
  // generated with an empty success body carries the same gap — it is not specific to
  // Currency. Recorded in `docs/kb/risks/technical-debt-register.md`; not fixed here,
  // per this task's own Files That Must Not Change (`core/api/generated/**`) and its
  // explicit instruction to report rather than special-case around it locally.
  await page.route('**/api/v1/currencies/1', (route) => {
    if (route.request().method() !== 'DELETE') {
      return route.fallback();
    }
    return fulfillJson(
      route,
      { title: 'Cannot delete currency: it is referenced by 3 existing invoices.', status: 409 },
      409,
    );
  });
  await page
    .getByRole('row', { name: /US Dollar/ })
    .getByRole('button', { name: 'Delete' })
    .click();
  // Scoped to the confirmation dialog itself (PrimeNG's `p-confirmdialog` renders
  // `role="alertdialog"`, not `role="dialog"`) — the row's own "Delete" action button is
  // still in the DOM behind it, and an unscoped query matches both.
  await page
    .getByRole('alertdialog', { name: 'Delete currency' })
    .getByRole('button', { name: 'Delete', exact: true })
    .click();
  // The server's verbatim title is what *should* show (and does, for every other error
  // path this feature has — create's 400s, update's 409s, both proven above and in
  // `currency-form-drawer.component.spec.ts`); DELETE alone loses it to the gap above, so
  // this asserts the client's honest fallback text, not the string the server actually sent.
  await expect(page.getByText('The currency could not be deleted.')).toBeVisible();
});

test('a caller whose rights map omits Currency sees the inline permission-denied surface, not the grid', async ({
  page,
}) => {
  await page.setViewportSize({ width: 1600, height: 900 });
  await page.route('**/api/v1/me', (route) =>
    fulfillJson(
      route,
      meResponse({
        Dashboard: { view: true, create: false, edit: false, delete: false, hidden: false },
      }),
    ),
  );

  await login(page, '/masters/currencies');

  await expect(page.getByText('You do not have access to this')).toBeVisible();
  await expect(page.getByRole('grid')).not.toBeVisible();
});
