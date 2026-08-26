import { HttpErrorResponse } from '@angular/common/http';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';

import {
  DataGridErrorComponent,
  GRID_ERROR_FALLBACK_MESSAGE,
  toGridProblem,
} from './data-grid-error.component';
import {
  BR_SO_001_CANCEL_MESSAGE,
  BR_SO_001_DELETE_MESSAGE,
  EXPORT_ROW_LIMIT_MESSAGE,
  TEST_TRACE_ID,
  businessRuleProblem,
  screenRightDeniedProblem,
  unhandledProblem,
} from './fixtures/problem-details';

async function setup(error: unknown, showRetry = true) {
  const retries: number[] = [];
  const view = await render(DataGridErrorComponent, {
    inputs: { error, showRetry },
    on: { retry: () => retries.push(1) },
  });
  return { ...view, retries };
}

describe('DataGridErrorComponent - 409, the business-rule branch', () => {
  /**
   * Test 9. The sentence comes from
   * `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/MfgPoService.cs:488`
   * (BR-SO-001) and is asserted **byte-for-byte**: the component's entire
   * obligation with respect to business logic is fidelity of transport.
   */
  it('renders the server sentence byte-for-byte', async () => {
    await setup(businessRuleProblem());

    const node = screen.getByText(BR_SO_001_DELETE_MESSAGE);
    expect(node.textContent).toBe(BR_SO_001_DELETE_MESSAGE);
  });

  it('renders the sibling refusal byte-for-byte too (MfgPoService.cs:598)', async () => {
    await setup(businessRuleProblem(BR_SO_001_CANCEL_MESSAGE));

    expect(screen.getByText(BR_SO_001_CANCEL_MESSAGE).textContent).toBe(BR_SO_001_CANCEL_MESSAGE);
  });

  /**
   * Test 10, stated as three separate failure modes because each one has
   * shipped in some product at some point: a prefix ("Error: ..."), a
   * truncation, and a translation lookup that falls back to its own key.
   */
  it('does not prefix, truncate or translate the message', async () => {
    const { container } = await setup(businessRuleProblem());

    // The element carrying the message carries **only** the message. The
    // severity word `app-inline-alert` renders for screen readers is a sibling
    // node, deliberately - it is not a prefix on the server's sentence.
    const text = container.querySelector('.app-inline-alert__message')?.textContent ?? '';
    expect(text).toBe(BR_SO_001_DELETE_MESSAGE);
    expect(text).toHaveLength(BR_SO_001_DELETE_MESSAGE.length);
    expect(text.startsWith('Error')).toBe(false);
    expect(text.endsWith('...')).toBe(false);
    // A missed @ngx-translate lookup renders its key. No key-shaped token here.
    expect(/^[a-z]+(\.[a-z]+)+$/.test(text)).toBe(false);
  });

  it('renders the export row-ceiling refusal as a business-rule message, not a generic one', async () => {
    // CurrencyExcelController.cs:112-116 - an over-limit export is a 409, so
    // export failure shares this branch with a delete refusal.
    await setup(businessRuleProblem(EXPORT_ROW_LIMIT_MESSAGE));

    expect(screen.getByText(EXPORT_ROW_LIMIT_MESSAGE)).toBeDefined();
    expect(screen.queryByText(GRID_ERROR_FALLBACK_MESSAGE)).toBeNull();
  });

  it('announces assertively and offers no Retry - retrying does not change a refusal', async () => {
    const { container } = await setup(businessRuleProblem());

    expect(container.querySelector('[aria-live="assertive"]')).not.toBeNull();
    expect(screen.queryByRole('button', { name: 'Retry' })).toBeNull();
  });
});

describe('DataGridErrorComponent - 403, the permission branch', () => {
  /** Test 8. Inline, naming the missing right, and no navigation (KB-050). */
  it('renders the permission-denied state inline, naming the screen and the right', async () => {
    const { container } = await setup(screenRightDeniedProblem('Sales Order', 'edit'));

    expect(container.querySelector('.app-permission-denied-state')).not.toBeNull();
    const text = (container.textContent ?? '').replace(/\s+/g, ' ');
    expect(text).toContain('edit');
    expect(text).toContain('Sales Order');
  });

  it('does not navigate away', async () => {
    const before = window.location.href;
    await setup(screenRightDeniedProblem());

    expect(window.location.href).toBe(before);
    expect(screen.queryByRole('link')).toBeNull();
  });

  /**
   * The account-gate 403s (`ApiProblems.cs:69-75`) carry no `screen`/`right`.
   * Inventing them would put words in the server's mouth, so those fall through
   * to the generic error state with the server's own title.
   */
  it('falls back to the error state when the server named no screen right', async () => {
    const { container } = await setup({
      status: 403,
      title: 'Your trial has expired.',
      traceId: TEST_TRACE_ID,
    });

    expect(container.querySelector('.app-permission-denied-state')).toBeNull();
    expect(screen.getByText('Your trial has expired.')).toBeDefined();
  });
});

describe('DataGridErrorComponent - everything else', () => {
  /** Test 7. `ApiProblems.cs:134-139` - a 500 carries a constant title and traceId only. */
  it('renders a 500 with the server message, the correlation id and a working Retry', async () => {
    const { retries } = await setup(unhandledProblem());

    expect(screen.getByText('An unexpected error occurred.')).toBeDefined();
    expect(screen.getByText(`Reference: ${TEST_TRACE_ID}`)).toBeDefined();

    await userEvent.click(screen.getByRole('button', { name: 'Retry' }));
    expect(retries).toHaveLength(1);
  });

  it('announces politely - a transport failure waits for a pause', async () => {
    const { container } = await setup(unhandledProblem());

    expect(container.querySelector('[aria-live="polite"]')).not.toBeNull();
  });

  it('shows the fallback sentence only when the server sent no words at all', async () => {
    await setup(new HttpErrorResponse({ status: 0, statusText: 'Unknown Error' }));

    expect(screen.getByText(GRID_ERROR_FALLBACK_MESSAGE)).toBeDefined();
  });
});

describe('toGridProblem', () => {
  it('unwraps an HttpErrorResponse to the problem body it carries', () => {
    const body = businessRuleProblem();
    const problem = toGridProblem(new HttpErrorResponse({ status: 409, error: body }));

    expect(problem?.title).toBe(BR_SO_001_DELETE_MESSAGE);
    expect(problem?.status).toBe(409);
    expect(problem?.traceId).toBe(TEST_TRACE_ID);
  });

  it('takes the status from the response when the body is not problem+json', () => {
    const problem = toGridProblem(
      new HttpErrorResponse({ status: 502, error: 'gateway is down', statusText: 'Bad Gateway' }),
    );

    expect(problem?.status).toBe(502);
  });

  it('passes a body already unwrapped by DataGridQueryState straight through', () => {
    expect(toGridProblem(screenRightDeniedProblem())?.screen).toBe('Currency');
  });

  it('answers null for no error, so the grid can tell "failed" from "empty"', () => {
    expect(toGridProblem(null)).toBeNull();
    expect(toGridProblem(undefined)).toBeNull();
  });
});
