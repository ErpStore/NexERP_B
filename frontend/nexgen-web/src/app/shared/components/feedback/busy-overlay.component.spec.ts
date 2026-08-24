import { render, screen } from '@testing-library/angular';
import { beforeAll, describe, expect, it } from 'vitest';

import { BusyOverlayComponent } from './busy-overlay.component';
import { installMatchMedia } from '../overlay/jsdom-overlay-support';

const TEMPLATE = `
  <input aria-label="Document number" />
  <app-busy-overlay [busy]="busy" [fullPage]="fullPage" message="Posting the sales order">
    <p>Line detail</p>
  </app-busy-overlay>`;

async function setup(busy = true, fullPage = false) {
  const view = await render(TEMPLATE, {
    imports: [BusyOverlayComponent],
    componentProperties: { busy, fullPage },
  });
  return { view };
}

describe('app-busy-overlay', () => {
  beforeAll(installMatchMedia);

  it('marks its own region busy and announces what is happening', async () => {
    const { view } = await setup(true);

    expect(view.container.querySelector('[aria-busy="true"]')).toBeTruthy();
    expect(screen.getByRole('status').textContent).toContain('Posting the sales order');
  });

  it('keeps the content it covers in the DOM', async () => {
    await setup(true);

    expect(screen.getByText('Line detail')).toBeTruthy();
  });

  it('is region-scoped by default - the block lands inside the component', async () => {
    const { view } = await setup(true);

    const block = document.querySelector('.p-blockui');
    expect(block).toBeTruthy();
    expect(view.container.contains(block)).toBe(true);
    expect(document.body.classList.contains('p-overflow-hidden')).toBe(false);
  });

  it('blocks the whole page only when fullPage is opted into', async () => {
    const { view } = await setup(true, true);

    const block = document.querySelector('.p-blockui');
    expect(block).toBeTruthy();
    expect(view.container.contains(block)).toBe(false);
  });

  it('does not steal focus from whatever the user was typing in', async () => {
    const { view } = await setup(false);
    const input = screen.getByLabelText('Document number');
    input.focus();

    await view.rerender({ componentProperties: { busy: true, fullPage: false } });

    expect(document.activeElement).toBe(input);
  });
});
