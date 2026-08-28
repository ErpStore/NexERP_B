import { provideRouter } from '@angular/router';
import { render } from '@testing-library/angular';
import axe, { type Result } from 'axe-core';
import { afterAll, beforeAll, describe, expect, it } from 'vitest';

import { installGridJsdomSupport, uninstallGridJsdomSupport } from '../data-grid/test-fixtures';
import { provideToast } from '../feedback';
import { provideConfirmDialog } from '../overlay';
import { DocumentEditorComponent } from './document-editor.component';
import type { DocumentEditorMode } from './document-editor.model';
import { RecordingOperations, testConfig, type TestHeader, type TestLine } from './test-fixtures';

/**
 * Runtime accessibility scan of the editor shell in every state it can be in
 * (M2-C08-01 Testing Requirement 18).
 *
 * jsdom limitation, stated rather than hidden: jsdom applies no stylesheet and
 * computes no layout, so axe's `color-contrast` rule cannot run here. Contrast
 * is covered by computation in `src/app/core/theme/contrast.spec.ts`
 * (M2-C04-01). The repository-wide axe-in-CI pass is still M5-09's.
 */

/** A full axe pass in jsdom is slow. Not a hang. */
const AXE_IS_SLOW = 60_000;

async function criticalViolations(container: HTMLElement): Promise<Result[]> {
  const results = await axe.run(container, {
    resultTypes: ['violations'],
    rules: { 'color-contrast': { enabled: false } },
  });
  return results.violations.filter((violation) => violation.impact === 'critical');
}

async function renderEditor(mode: DocumentEditorMode, ops: RecordingOperations) {
  const view = await render(DocumentEditorComponent<TestHeader, TestLine>, {
    inputs: {
      config: testConfig(ops, {
        sideRegions: [{ id: 'attachments', label: 'Attachments' }],
      }),
      mode,
      documentId: mode === 'create' ? null : 7,
    },
    providers: [provideRouter([]), provideConfirmDialog(), provideToast()],
  });
  await view.fixture.whenStable();
  return view;
}

describe('document editor accessibility', () => {
  beforeAll(installGridJsdomSupport);
  afterAll(uninstallGridJsdomSupport);

  for (const mode of ['create', 'edit', 'view'] as const) {
    it(
      `has zero critical axe violations in ${mode} mode`,
      async () => {
        const view = await renderEditor(mode, new RecordingOperations());

        expect(await criticalViolations(view.container)).toEqual([]);
      },
      AXE_IS_SLOW,
    );
  }

  it(
    'has zero critical axe violations in the load-error state',
    async () => {
      const ops = new RecordingOperations();
      ops.loadFailure = { status: 500, title: 'Something went wrong.', traceId: '00-9f2a-1a2b-01' };
      const view = await renderEditor('edit', ops);

      expect(await criticalViolations(view.container)).toEqual([]);
    },
    AXE_IS_SLOW,
  );
});
