import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import type { MenuItem } from 'primeng/api';
import { beforeAll, describe, expect, it } from 'vitest';

import { ContextMenuComponent } from './context-menu.component';
import { installMatchMedia } from './jsdom-overlay-support';

const ITEMS: MenuItem[] = [{ label: 'Open' }, { label: 'Duplicate' }, { label: 'Cancel line' }];

const TEMPLATE = `
  <app-context-menu [items]="items" label="Actions for SO-0001">
    <span>SO-0001</span>
  </app-context-menu>`;

async function setup() {
  const view = await render(TEMPLATE, {
    imports: [ContextMenuComponent],
    componentProperties: { items: ITEMS },
  });
  return { view, trigger: screen.getByRole('button', { name: 'Actions for SO-0001' }) };
}

describe('app-context-menu', () => {
  beforeAll(installMatchMedia);

  it('renders a visible trigger - a right-click-only menu is inoperable by keyboard', async () => {
    const { trigger } = await setup();

    expect(trigger.getAttribute('aria-haspopup')).toBe('menu');
  });

  it('opens from the keyboard through the trigger', async () => {
    const { trigger } = await setup();

    trigger.focus();
    await userEvent.keyboard('{Enter}');

    expect(await screen.findByRole('menu')).toBeTruthy();
    expect(screen.getByText('Cancel line')).toBeTruthy();
  });

  it('opens on Shift+F10, the platform convention', async () => {
    const { trigger } = await setup();

    trigger.focus();
    await userEvent.keyboard('{Shift>}{F10}{/Shift}');

    expect(await screen.findByRole('menu')).toBeTruthy();
  });

  it('moves through the items with the arrow keys', async () => {
    const { trigger } = await setup();

    trigger.focus();
    await userEvent.keyboard('{Enter}');
    const menu = await screen.findByRole('menu');

    await userEvent.keyboard('{ArrowDown}');
    const first = menu.getAttribute('aria-activedescendant');
    await userEvent.keyboard('{ArrowDown}');
    const second = menu.getAttribute('aria-activedescendant');

    expect(first).toBeTruthy();
    expect(second).not.toBe(first);
  });

  it('closes on Escape and restores focus to the trigger', async () => {
    const { trigger } = await setup();

    trigger.focus();
    await userEvent.keyboard('{Enter}');
    await screen.findByRole('menu');

    await userEvent.keyboard('{Escape}');

    expect(screen.queryByRole('menu')).toBeNull();
    expect(document.activeElement).toBe(trigger);
  });
});
