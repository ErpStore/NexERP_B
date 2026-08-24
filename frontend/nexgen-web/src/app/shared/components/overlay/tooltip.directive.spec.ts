import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { beforeAll, describe, expect, it } from 'vitest';

import { installMatchMedia } from './jsdom-overlay-support';
import { TooltipDirective } from './tooltip.directive';

const TEMPLATE = `
  <button type="button" appTooltip="Recalculate taxes">Recalculate</button>`;

async function setup() {
  await render(TEMPLATE, { imports: [TooltipDirective] });
  return { trigger: screen.getByRole('button', { name: 'Recalculate' }) };
}

function tooltipText(): string | null {
  return document.querySelector('.p-tooltip-text')?.textContent ?? null;
}

describe('[appTooltip]', () => {
  beforeAll(installMatchMedia);

  it('opens on focus, not on hover alone', async () => {
    const { trigger } = await setup();

    expect(tooltipText()).toBeNull();
    trigger.focus();

    expect(tooltipText()).toBe('Recalculate taxes');
  });

  it('closes again on blur', async () => {
    const { trigger } = await setup();

    trigger.focus();
    trigger.blur();

    expect(tooltipText()).toBeNull();
  });

  it('dismisses on Escape while the trigger keeps focus', async () => {
    const { trigger } = await setup();

    trigger.focus();
    expect(tooltipText()).toBe('Recalculate taxes');

    await userEvent.keyboard('{Escape}');

    expect(tooltipText()).toBeNull();
    expect(document.activeElement).toBe(trigger);
  });

  it('also opens on hover, so the mouse behaviour is unchanged', async () => {
    const { trigger } = await setup();

    await userEvent.hover(trigger);

    expect(tooltipText()).toBe('Recalculate taxes');
  });
});
