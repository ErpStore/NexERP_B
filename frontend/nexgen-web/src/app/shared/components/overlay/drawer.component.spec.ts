import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { beforeAll, beforeEach, describe, expect, it } from 'vitest';

import { DRAWER_RESIZE_STEP, DrawerComponent } from './drawer.component';
import { installMatchMedia } from './jsdom-overlay-support';

const TEMPLATE = `
  <ul>
    <li>SO-0001</li>
    <li>SO-0002</li>
  </ul>
  <button type="button" (click)="open = true">Open SO-0001</button>
  <app-drawer title="Sales order SO-0001" persistKey="sales-order" [(visible)]="open">
    <button type="button">Approve</button>
  </app-drawer>`;

async function setup() {
  const view = await render(TEMPLATE, {
    imports: [DrawerComponent],
    componentProperties: { open: false },
  });
  return { view, invoker: screen.getByRole('button', { name: 'Open SO-0001' }) };
}

describe('app-drawer', () => {
  beforeAll(installMatchMedia);

  beforeEach(() => {
    localStorage.clear();
  });

  it('announces itself as a named modal dialog despite PrimeNG hard-coding role="complementary"', async () => {
    const { invoker } = await setup();

    await userEvent.click(invoker);

    const drawer = await screen.findByRole('dialog');
    expect(drawer.getAttribute('aria-modal')).toBe('true');
    expect(drawer.getAttribute('aria-label')).toBe('Sales order SO-0001');
  });

  it('keeps the list behind it rendered - the whole reason a drawer exists', async () => {
    const { invoker } = await setup();

    await userEvent.click(invoker);
    await screen.findByRole('dialog');

    expect(screen.getByText('SO-0001')).toBeTruthy();
    expect(screen.getByText('SO-0002')).toBeTruthy();
  });

  it('moves focus in, closes on Escape and restores focus to the invoker', async () => {
    const { invoker } = await setup();

    invoker.focus();
    await userEvent.click(invoker);
    const drawer = await screen.findByRole('dialog');
    expect(drawer.contains(document.activeElement)).toBe(true);

    await userEvent.keyboard('{Escape}');

    expect(screen.queryByRole('dialog')).toBeNull();
    expect(document.activeElement).toBe(invoker);
  });

  it('keeps Tab inside the drawer', async () => {
    const { invoker } = await setup();

    await userEvent.click(invoker);
    const drawer = await screen.findByRole('dialog');
    expect(drawer.contains(document.activeElement)).toBe(true);

    await userEvent.tab();
    await userEvent.tab();
    await userEvent.tab();
    await userEvent.tab();

    expect(drawer.contains(document.activeElement)).toBe(true);
  });

  it('locks background scroll while open', async () => {
    const { invoker } = await setup();

    await userEvent.click(invoker);
    await screen.findByRole('dialog');

    expect(document.body.classList.contains('p-overflow-hidden')).toBe(true);
  });

  it('resizes from the keyboard - a drag-only handle is inoperable without a mouse', async () => {
    const { invoker } = await setup();

    await userEvent.click(invoker);
    await screen.findByRole('dialog');
    const handle = screen.getByRole('separator', { name: 'Resize panel' });
    const before = Number(handle.getAttribute('aria-valuenow'));

    handle.focus();
    await userEvent.keyboard('{ArrowLeft}');

    expect(Number(handle.getAttribute('aria-valuenow'))).toBe(before + DRAWER_RESIZE_STEP);

    await userEvent.keyboard('{ArrowRight}{ArrowRight}');

    expect(Number(handle.getAttribute('aria-valuenow'))).toBe(before - DRAWER_RESIZE_STEP);
  });

  it('writes the chosen width to local storage, keyed per screen', async () => {
    const { invoker } = await setup();

    await userEvent.click(invoker);
    await screen.findByRole('dialog');
    const handle = screen.getByRole('separator', { name: 'Resize panel' });
    handle.focus();
    await userEvent.keyboard('{ArrowLeft}');

    expect(localStorage.getItem('nexgen.drawer.width.sales-order')).toBe(
      handle.getAttribute('aria-valuenow'),
    );
    // Another screen's drawer is unaffected.
    expect(localStorage.getItem('nexgen.drawer.width.default')).toBeNull();
  });

  it('adopts a width persisted by an earlier visit', async () => {
    // One `render` per spec file: `TestBed` cannot be reconfigured once
    // instantiated, so "across mounts" is asserted as the two halves of the
    // round trip - this half reads what the previous test's half wrote.
    localStorage.setItem('nexgen.drawer.width.sales-order', '704');
    const { invoker } = await setup();

    await userEvent.click(invoker);
    await screen.findByRole('dialog');

    expect(screen.getByRole('separator').getAttribute('aria-valuenow')).toBe('704');
  });
});
