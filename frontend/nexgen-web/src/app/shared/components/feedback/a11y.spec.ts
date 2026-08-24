import { render } from '@testing-library/angular';
import axe, { type Result } from 'axe-core';
import { Toast } from 'primeng/toast';
import { afterEach, beforeAll, describe, expect, it } from 'vitest';

import { installMatchMedia } from '../overlay/jsdom-overlay-support';
import { BusyOverlayComponent } from './busy-overlay.component';
import { EmptyStateComponent } from './empty-state.component';
import { ErrorStateComponent } from './error-state.component';
import { InlineAlertComponent } from './inline-alert.component';
import { PermissionDeniedStateComponent } from './permission-denied-state.component';
import { ProgressBarComponent } from './progress-bar.component';
import { SkeletonFormComponent } from './skeleton-form.component';
import { SkeletonTableComponent } from './skeleton-table.component';
import { SkeletonComponent } from './skeleton.component';
import { provideToast, TOAST_POLITE_PASS_THROUGH, ToastService } from './toast.service';

/**
 * Runtime accessibility scan over every feedback component, in both themes.
 *
 * jsdom limitation, stated rather than hidden: jsdom applies no stylesheet and
 * computes no layout, so axe's `color-contrast` rule cannot run here. Contrast
 * is covered by computation in `src/app/core/theme/contrast.spec.ts`
 * (M2-C04-01). The repository-wide axe-in-CI pass is still M5-09.
 */

/** A full axe pass in jsdom is slow. Not a hang. */
const AXE_IS_SLOW = 60_000;

const TEMPLATE = `
  <p-toast [pt]="pt" />

  <app-inline-alert severity="error" message="Cannot delete this Sales Order as a Sales DC transaction exists." />
  <app-inline-alert severity="info" message="Prices are indicative." [dismissible]="true" />

  <app-busy-overlay [busy]="true" message="Posting the sales order">
    <p>Line detail</p>
  </app-busy-overlay>

  <app-skeleton width="8rem" />
  <app-skeleton-table [rows]="3" [columns]="3" label="Loading sales orders" />
  <app-skeleton-form [fields]="4" [columns]="2" label="Loading the document header" />

  <app-progress-bar [value]="42" label="Importing rows" />
  <app-progress-bar label="Refreshing" />

  <app-empty-state
    variant="no-data"
    title="No sales orders yet"
    description="Create one to get started."
    actionLabel="New Sales Order"
  />
  <app-empty-state variant="filtered" title="No results for these filters" />

  <app-error-state
    message="Cannot delete this Sales Order as a Sales DC transaction exists."
    traceId="00-9f2a-1a2b-01"
  />

  <app-permission-denied-state screen="Sales Order" right="edit" />`;

async function renderFeedback(theme: 'light' | 'dark') {
  document.documentElement.setAttribute('data-theme', theme);
  const view = await render(TEMPLATE, {
    imports: [
      Toast,
      InlineAlertComponent,
      BusyOverlayComponent,
      SkeletonComponent,
      SkeletonTableComponent,
      SkeletonFormComponent,
      ProgressBarComponent,
      EmptyStateComponent,
      ErrorStateComponent,
      PermissionDeniedStateComponent,
    ],
    providers: [provideToast()],
    componentProperties: { pt: TOAST_POLITE_PASS_THROUGH },
  });

  // A toast is only in the DOM once something has been said.
  view.fixture.debugElement.injector.get(ToastService).error('Posting failed.');
  view.fixture.detectChanges();
  await view.fixture.whenStable();
  return view;
}

async function criticalViolations(container: HTMLElement): Promise<Result[]> {
  const results = await axe.run(container, {
    resultTypes: ['violations'],
    rules: { 'color-contrast': { enabled: false } },
  });
  return results.violations.filter((violation) => violation.impact === 'critical');
}

describe('feedback layer accessibility', () => {
  beforeAll(installMatchMedia);

  afterEach(() => {
    document.documentElement.removeAttribute('data-theme');
  });

  for (const theme of ['light', 'dark'] as const) {
    it(
      `has zero critical axe violations across every feedback component in the ${theme} theme`,
      async () => {
        await renderFeedback(theme);

        expect(await criticalViolations(document.body)).toEqual([]);
      },
      AXE_IS_SLOW,
    );
  }
});
