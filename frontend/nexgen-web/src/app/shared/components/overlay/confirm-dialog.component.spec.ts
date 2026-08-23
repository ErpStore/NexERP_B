import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { ConfirmationService } from 'primeng/api';
import { beforeAll, describe, expect, it } from 'vitest';

import { ConfirmDialogComponent } from './confirm-dialog.component';
import {
  ConfirmDialogService,
  type ConfirmRequest,
  type ConfirmResult,
} from './confirm-dialog.service';
import { installMatchMedia } from './jsdom-overlay-support';

const CANCEL_LINE: ConfirmRequest = {
  header: 'Cancel line',
  message: 'Cancel line 3 of SO-0001?',
  reasonRequired: true,
  destructive: true,
};

/**
 * A stand-in for a screen: it holds the promise the dialog resolves, exactly
 * as a real caller would.
 */
class Caller {
  result: ConfirmResult | null = null;
  pending: Promise<void> | null = null;

  constructor(private readonly service: ConfirmDialogService) {}

  ask(request: ConfirmRequest): void {
    this.pending = this.service.confirm(request).then((result) => {
      this.result = result;
    });
  }
}

async function setup(request: ConfirmRequest = CANCEL_LINE) {
  const view = await render(ConfirmDialogComponent, {
    providers: [ConfirmationService],
  });
  const caller = new Caller(view.fixture.debugElement.injector.get(ConfirmDialogService));
  caller.ask(request);
  view.fixture.detectChanges();
  await view.fixture.whenStable();
  return { view, caller };
}

describe('app-confirm-dialog', () => {
  beforeAll(installMatchMedia);

  it('shows the question and the confirm/cancel labels', async () => {
    await setup({ header: 'Delete draft', message: 'Delete this draft?', confirmLabel: 'Delete' });

    expect(screen.getByText('Delete this draft?')).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Delete' })).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Cancel' })).toBeTruthy();
  });

  it('disables Confirm until a required reason is non-empty after trim', async () => {
    await setup();
    const confirm = screen.getByRole<HTMLButtonElement>('button', { name: 'Confirm' });
    const reason = screen.getByRole('textbox');

    expect(confirm.disabled).toBe(true);

    await userEvent.type(reason, '   ');
    expect(confirm.disabled).toBe(true);

    await userEvent.type(reason, 'Customer withdrew the order');
    expect(confirm.disabled).toBe(false);
  });

  it('passes the trimmed reason to the caller on confirm', async () => {
    const { caller } = await setup();

    await userEvent.type(screen.getByRole('textbox'), '  Customer withdrew  ');
    await userEvent.click(screen.getByRole('button', { name: 'Confirm' }));
    await caller.pending;

    expect(caller.result).toEqual({ confirmed: true, reason: 'Customer withdrew' });
  });

  it('maps Escape to cancel, never to confirm', async () => {
    const { caller } = await setup();

    await userEvent.type(screen.getByRole('textbox'), 'Typed but not submitted');
    await userEvent.keyboard('{Escape}');
    await caller.pending;

    expect(caller.result).toEqual({ confirmed: false, reason: null });
  });

  it('maps the backdrop to cancel, never to confirm', async () => {
    const { caller } = await setup();
    const mask = document.querySelector('.p-dialog-mask');

    expect(mask).toBeTruthy();
    mask?.dispatchEvent(new MouseEvent('mousedown', { bubbles: true }));
    await caller.pending;

    expect(caller.result).toEqual({ confirmed: false, reason: null });
  });

  it('gives the destructive variant emphasis without changing the keyboard model', async () => {
    await setup();
    const confirm = screen.getByRole('button', { name: 'Confirm' });

    expect(confirm.classList.contains('app-confirm-dialog__button--destructive')).toBe(true);
    // Still an ordinary button in the tab order, still reachable by Tab.
    expect(confirm.getAttribute('tabindex')).toBeNull();
    expect(confirm.getAttribute('type')).toBe('button');
  });

  it('needs no reason when the caller did not ask for one', async () => {
    const { caller } = await setup({ header: 'Approve', message: 'Approve SO-0001?' });

    expect(screen.queryByRole('textbox')).toBeNull();
    await userEvent.click(screen.getByRole('button', { name: 'Confirm' }));
    await caller.pending;

    expect(caller.result).toEqual({ confirmed: true, reason: null });
  });
});
