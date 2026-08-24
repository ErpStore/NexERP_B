import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { beforeAll, describe, expect, it } from 'vitest';

import { installMatchMedia } from './jsdom-overlay-support';
import { ModalComponent } from './modal.component';

const TEMPLATE = `
  <button type="button" (click)="open = true">Edit line</button>
  <app-modal title="Edit line" [(visible)]="open">
    <input aria-label="Quantity" />
    <button type="button">Save</button>
  </app-modal>`;

async function setup() {
  const view = await render(TEMPLATE, {
    imports: [ModalComponent],
    componentProperties: { open: false },
  });
  return { view, invoker: screen.getByRole('button', { name: 'Edit line' }) };
}

describe('app-modal', () => {
  beforeAll(installMatchMedia);

  it('is labelled and marked as a modal dialog', async () => {
    const { invoker } = await setup();

    await userEvent.click(invoker);

    const dialog = await screen.findByRole('dialog');
    expect(dialog.getAttribute('aria-modal')).toBe('true');
    expect(screen.getByText('Edit line', { selector: 'span, h1, h2, h3, div' })).toBeTruthy();
  });

  it('moves focus into the dialog on open', async () => {
    const { invoker } = await setup();

    await userEvent.click(invoker);
    const dialog = await screen.findByRole('dialog');

    expect(dialog.contains(document.activeElement)).toBe(true);
  });

  it('locks background scroll while open and restores it on close', async () => {
    const { invoker } = await setup();

    await userEvent.click(invoker);
    await screen.findByRole('dialog');
    expect(document.body.classList.contains('p-overflow-hidden')).toBe(true);

    await userEvent.keyboard('{Escape}');

    expect(document.body.classList.contains('p-overflow-hidden')).toBe(false);
  });

  it('closes on Escape and returns focus to the exact invoking element', async () => {
    const { invoker } = await setup();

    invoker.focus();
    await userEvent.click(invoker);
    await screen.findByRole('dialog');

    await userEvent.keyboard('{Escape}');

    expect(screen.queryByRole('dialog')).toBeNull();
    expect(document.activeElement).toBe(invoker);
  });

  it('keeps Tab inside the dialog', async () => {
    const { invoker } = await setup();

    await userEvent.click(invoker);
    const dialog = await screen.findByRole('dialog');

    await userEvent.tab();
    await userEvent.tab();
    await userEvent.tab();
    await userEvent.tab();

    expect(dialog.contains(document.activeElement)).toBe(true);
  });
});
