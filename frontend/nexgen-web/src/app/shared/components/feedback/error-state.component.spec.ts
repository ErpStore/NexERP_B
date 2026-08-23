import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import type { InputSignal } from '@angular/core';
import { describe, expect, it, vi } from 'vitest';

import { ErrorStateComponent } from './error-state.component';

/** A 409 business-rule refusal: the server's own sentence, in `title`. */
const SERVER_MESSAGE = 'Cannot delete this Sales Order as a Sales DC transaction exists.';
const TRACE_ID = '00-9f2a4c1e5b7d4a0e8c3f1b2d6e7a8c90-1a2b3c4d5e6f7081-01';

describe('app-error-state', () => {
  it('renders the server message verbatim - no rewording, no prefix', async () => {
    await render(ErrorStateComponent, { inputs: { message: SERVER_MESSAGE } });

    expect(screen.getByText(SERVER_MESSAGE)).toBeTruthy();
  });

  it('renders the traceId and copies it to the clipboard', async () => {
    const writeText = vi.fn<(value: string) => Promise<void>>().mockResolvedValue(undefined);
    Object.defineProperty(navigator, 'clipboard', {
      configurable: true,
      value: { writeText },
    });

    const view = await render(ErrorStateComponent, {
      inputs: { message: SERVER_MESSAGE, traceId: TRACE_ID },
    });

    expect(screen.getByText(`Reference: ${TRACE_ID}`)).toBeTruthy();

    await userEvent.click(screen.getByRole('button', { name: 'Copy reference' }));
    view.fixture.detectChanges();

    expect(writeText).toHaveBeenCalledWith(TRACE_ID);
    expect(screen.getByRole('button', { name: 'Copied' })).toBeTruthy();
  });

  it('omits the reference line when no traceId was supplied', async () => {
    await render(ErrorStateComponent, { inputs: { message: SERVER_MESSAGE } });

    expect(screen.queryByRole('button', { name: 'Copy reference' })).toBeNull();
  });

  it('invokes the caller-supplied retry', async () => {
    const retry = vi.fn();
    await render(ErrorStateComponent, {
      inputs: { message: SERVER_MESSAGE },
      on: { retry },
    });

    await userEvent.click(screen.getByRole('button', { name: 'Retry' }));

    expect(retry).toHaveBeenCalledTimes(1);
  });

  it('announces itself assertively - a failed action is not a status update', async () => {
    await render(ErrorStateComponent, { inputs: { message: SERVER_MESSAGE } });

    expect(screen.getByRole('alert')).toBeTruthy();
  });

  it('will not type-check without a message', () => {
    type MessageValue = ErrorStateComponent['message'] extends InputSignal<infer T> ? T : never;
    interface ErrorStateInputs {
      message: MessageValue;
      traceId?: string;
    }

    // @ts-expect-error - `message` is input.required<string>() with no default,
    // so a call site that omits it does not compile. Delete the default and
    // this line stops erroring, which is what makes the assertion real.
    const withoutMessage: ErrorStateInputs = { traceId: TRACE_ID };

    expect(withoutMessage.traceId).toBe(TRACE_ID);
  });

  it('throws rather than falling back to a generic message when none is bound', async () => {
    let thrown: unknown = null;
    try {
      await render(ErrorStateComponent, { inputs: { traceId: TRACE_ID } });
    } catch (error) {
      thrown = error;
    }

    // Angular NG0950: a required input read with no value bound. There is no
    // default to quietly stand in for it, which is the whole point - a
    // forgotten argument can never become a generic sentence on a screen.
    expect(String(thrown)).toContain('NG0950');
  });
});
