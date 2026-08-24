import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { beforeAll, describe, expect, it } from 'vitest';

import { installMatchMedia } from './jsdom-overlay-support';
import { PopoverComponent } from './popover.component';

const TEMPLATE = `
  <button type="button" (click)="panel.toggle($event)">Columns</button>
  <app-popover #panel label="Choose columns">
    <button type="button">Reset columns</button>
  </app-popover>`;

async function setup() {
  await render(TEMPLATE, { imports: [PopoverComponent] });
  return { trigger: screen.getByRole('button', { name: 'Columns' }) };
}

describe('app-popover', () => {
  beforeAll(installMatchMedia);

  it('opens from the keyboard, because the trigger is an ordinary button', async () => {
    const { trigger } = await setup();

    trigger.focus();
    await userEvent.keyboard('{Enter}');

    expect(await screen.findByRole('group', { name: 'Choose columns' })).toBeTruthy();
  });

  it('moves focus into the panel on open', async () => {
    const { trigger } = await setup();

    await userEvent.click(trigger);
    const panel = await screen.findByRole('group', { name: 'Choose columns' });

    expect(panel.contains(document.activeElement)).toBe(true);
  });

  it('closes on Escape and restores focus to the trigger', async () => {
    const { trigger } = await setup();

    trigger.focus();
    await userEvent.click(trigger);
    await screen.findByRole('group', { name: 'Choose columns' });

    await userEvent.keyboard('{Escape}');

    expect(screen.queryByRole('group', { name: 'Choose columns' })).toBeNull();
    expect(document.activeElement).toBe(trigger);
  });
});
