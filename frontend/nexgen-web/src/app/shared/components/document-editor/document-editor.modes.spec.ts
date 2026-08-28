import { provideRouter } from '@angular/router';
import { render, screen } from '@testing-library/angular';
import { afterAll, afterEach, beforeAll, describe, expect, it } from 'vitest';

import { installGridJsdomSupport, uninstallGridJsdomSupport } from '../data-grid/test-fixtures';
import { provideToast } from '../feedback';
import { provideConfirmDialog } from '../overlay';
import { DocumentEditorComponent } from './document-editor.component';
import type { DocumentEditorMode } from './document-editor.model';
import { RecordingOperations, testConfig, type TestHeader, type TestLine } from './test-fixtures';

/**
 * Mode handling (M2-C08-01 Testing Requirements 1-3 and 17): one route plus a
 * mode, replacing the existing create / create-with-parent / update / details
 * split (KB-053 §Route conventions).
 */

function setViewportWidth(width: number): void {
  Object.defineProperty(window, 'innerWidth', { value: width, configurable: true });
}

async function renderEditor(mode: DocumentEditorMode, id: number | null, ops: RecordingOperations) {
  return await render(DocumentEditorComponent<TestHeader, TestLine>, {
    inputs: { config: testConfig(ops), mode, documentId: id },
    providers: [provideRouter([]), provideConfirmDialog(), provideToast()],
  });
}

describe('DocumentEditorComponent modes', () => {
  beforeAll(installGridJsdomSupport);
  afterAll(uninstallGridJsdomSupport);
  afterEach(() => setViewportWidth(1440));

  it('mode="create" renders an empty form with one empty line row and focus in the first header field', async () => {
    setViewportWidth(1440);
    const ops = new RecordingOperations();
    const view = await renderEditor('create', null, ops);
    await view.fixture.whenStable();

    expect(ops.loads).toEqual([]);
    const reference = screen.getByLabelText<HTMLInputElement>(/Reference/i);
    expect(reference.value).toBe('');
    expect(document.activeElement).toBe(reference);

    // The grid seeds exactly one empty editable row - M2-C07's own empty state.
    expect(screen.getAllByRole('row').length).toBeGreaterThan(1);
  });

  it('mode="edit" loads by id and populates the header and the lines', async () => {
    setViewportWidth(1440);
    const ops = new RecordingOperations();
    const view = await renderEditor('edit', 7, ops);
    await view.fixture.whenStable();

    expect(ops.loads).toEqual([7]);
    expect(screen.getByLabelText<HTMLInputElement>(/Reference/i).value).toBe('REF-7');
    expect(screen.getByText('DOC/7')).toBeTruthy();
    expect(screen.getByText('Open')).toBeTruthy();
  });

  it('mode="view" renders controls read-only but copyable, not disabled (KB-051 Forms)', async () => {
    setViewportWidth(1440);
    const ops = new RecordingOperations();
    const view = await renderEditor('view', 7, ops);
    await view.fixture.whenStable();

    const reference = screen.getByLabelText<HTMLInputElement>(/Reference/i);
    expect(reference.readOnly).toBe(true);
    expect(reference.disabled).toBe(false);
    // Save and Save + New are not offered at all in view mode.
    expect(screen.queryByRole('button', { name: 'Save' })).toBeNull();
    expect(screen.queryByRole('button', { name: 'Save + New' })).toBeNull();
  });

  it('renders read-only below 768 px, whatever the mode says', async () => {
    setViewportWidth(500);
    const ops = new RecordingOperations();
    const view = await renderEditor('edit', 7, ops);
    await view.fixture.whenStable();

    expect(view.fixture.componentInstance.readOnly()).toBe(true);
    expect(screen.getByLabelText<HTMLInputElement>(/Reference/i).readOnly).toBe(true);
  });
});
