import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import axe, { type Result } from 'axe-core';
import type { MenuItem } from 'primeng/api';
import { ConfirmationService } from 'primeng/api';
import { afterEach, beforeAll, describe, expect, it } from 'vitest';

import { ConfirmDialogComponent } from './confirm-dialog.component';
import { ConfirmDialogService } from './confirm-dialog.service';
import { ContextMenuComponent } from './context-menu.component';
import { DrawerComponent } from './drawer.component';
import { installMatchMedia } from './jsdom-overlay-support';
import { ModalComponent } from './modal.component';
import { PopoverComponent } from './popover.component';
import { TooltipDirective } from './tooltip.directive';

/**
 * Runtime accessibility scan over every overlay, in both themes, **while it is
 * open** - a closed overlay proves nothing.
 *
 * jsdom limitation, stated rather than hidden: jsdom applies no stylesheet and
 * computes no layout, so axe's `color-contrast` rule cannot run here. Contrast
 * is covered by computation in `src/app/core/theme/contrast.spec.ts`
 * (M2-C04-01). The repository-wide axe-in-CI pass is still M5-09.
 */

/** A full axe pass in jsdom is slow. Not a hang. */
const AXE_IS_SLOW = 60_000;

const ITEMS: MenuItem[] = [{ label: 'Open' }, { label: 'Duplicate' }];

const TEMPLATE = `
  <ul>
    <li>SO-0001</li>
  </ul>

  <button type="button" appTooltip="Recalculate taxes">Recalculate</button>
  <button type="button" (click)="modalOpen = true">Edit line</button>
  <button type="button" (click)="drawerOpen = true">Open SO-0001</button>
  <button type="button" (click)="panel.toggle($event)">Columns</button>

  <app-modal title="Edit line" [(visible)]="modalOpen">
    <label for="qty">Quantity</label>
    <input id="qty" />
  </app-modal>

  <app-drawer title="Sales order SO-0001" [(visible)]="drawerOpen">
    <button type="button">Approve</button>
  </app-drawer>

  <app-popover #panel label="Choose columns">
    <button type="button">Reset columns</button>
  </app-popover>

  <app-context-menu [items]="items" label="Actions for SO-0001">
    <span>SO-0001</span>
  </app-context-menu>`;

async function renderOverlays(theme: 'light' | 'dark') {
  document.documentElement.setAttribute('data-theme', theme);
  return render(TEMPLATE, {
    imports: [
      ModalComponent,
      DrawerComponent,
      PopoverComponent,
      ContextMenuComponent,
      TooltipDirective,
    ],
    componentProperties: { modalOpen: false, drawerOpen: false, items: ITEMS },
  });
}

async function criticalViolations(container: HTMLElement): Promise<Result[]> {
  const results = await axe.run(container, {
    resultTypes: ['violations'],
    rules: { 'color-contrast': { enabled: false } },
  });
  return results.violations.filter((violation) => violation.impact === 'critical');
}

describe('overlay layer accessibility', () => {
  beforeAll(installMatchMedia);

  afterEach(() => {
    document.documentElement.removeAttribute('data-theme');
  });

  for (const theme of ['light', 'dark'] as const) {
    it(
      `has zero critical axe violations with the modal open in the ${theme} theme`,
      async () => {
        await renderOverlays(theme);
        await userEvent.click(screen.getByRole('button', { name: 'Edit line' }));
        await screen.findByRole('dialog');

        expect(await criticalViolations(document.body)).toEqual([]);
      },
      AXE_IS_SLOW,
    );

    it(
      `has zero critical axe violations with the drawer open in the ${theme} theme`,
      async () => {
        await renderOverlays(theme);
        await userEvent.click(screen.getByRole('button', { name: 'Open SO-0001' }));
        await screen.findByRole('dialog');

        expect(await criticalViolations(document.body)).toEqual([]);
      },
      AXE_IS_SLOW,
    );

    it(
      `has zero critical axe violations with the popover, menu and tooltip open in the ${theme} theme`,
      async () => {
        await renderOverlays(theme);
        await userEvent.click(screen.getByRole('button', { name: 'Columns' }));
        await screen.findByRole('group', { name: 'Choose columns' });
        screen.getByRole('button', { name: 'Recalculate' }).focus();
        await userEvent.click(screen.getByRole('button', { name: 'Actions for SO-0001' }));
        await screen.findByRole('menu');

        expect(await criticalViolations(document.body)).toEqual([]);
      },
      AXE_IS_SLOW,
    );

    it(
      `has zero critical axe violations with the confirm dialog open in the ${theme} theme`,
      async () => {
        document.documentElement.setAttribute('data-theme', theme);
        const view = await render(ConfirmDialogComponent, {
          providers: [ConfirmationService],
        });
        void view.fixture.debugElement.injector.get(ConfirmDialogService).confirm({
          header: 'Cancel line',
          message: 'Cancel line 3 of SO-0001?',
          reasonRequired: true,
          destructive: true,
        });
        view.fixture.detectChanges();
        await view.fixture.whenStable();

        expect(await criticalViolations(document.body)).toEqual([]);
      },
      AXE_IS_SLOW,
    );
  }
});
