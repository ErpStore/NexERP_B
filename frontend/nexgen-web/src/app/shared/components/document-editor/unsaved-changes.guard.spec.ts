import { Component } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter, withComponentInputBinding } from '@angular/router';
import { RouterTestingHarness } from '@angular/router/testing';
import { screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { afterAll, beforeAll, describe, expect, it, vi } from 'vitest';

import { installGridJsdomSupport, uninstallGridJsdomSupport } from '../data-grid/test-fixtures';
import { provideToast } from '../feedback';
import { provideConfirmDialog } from '../overlay';
import { DocumentEditorComponent } from './document-editor.component';
import { RecordingOperations, testConfig, type TestHeader, type TestLine } from './test-fixtures';
import { unsavedChangesGuard } from './unsaved-changes.guard';

/**
 * The dirty guard (M2-C08-01 Testing Requirements 4-6).
 *
 * The guard is exercised through the real router, not by calling the function
 * with a hand-made component: what is being asserted is that navigation is
 * actually blocked, which a direct call cannot show.
 */

@Component({ selector: 'app-elsewhere', template: '<p>elsewhere</p>' })
class ElsewhereComponent {}

type Editor = DocumentEditorComponent<TestHeader, TestLine>;

async function navigateToEditor(ops: RecordingOperations) {
  TestBed.configureTestingModule({
    providers: [
      provideRouter(
        [
          {
            path: 'doc',
            component: DocumentEditorComponent,
            canDeactivate: [unsavedChangesGuard],
            data: { config: testConfig(ops), mode: 'create' },
          },
          { path: 'elsewhere', component: ElsewhereComponent },
        ],
        withComponentInputBinding(),
      ),
      provideConfirmDialog(),
      provideToast(),
    ],
  });
  const harness = await RouterTestingHarness.create();
  const editor = (await harness.navigateByUrl('/doc', DocumentEditorComponent)) as Editor;
  await harness.fixture.whenStable();
  return { harness, editor, router: TestBed.inject(Router) };
}

/** A pending navigation keeps `whenStable()` from settling - flush the microtask/timer queue instead. */
async function flush(): Promise<void> {
  await new Promise((resolve) => setTimeout(resolve, 0));
}

function makeDirty(editor: Editor): void {
  const control = editor.headerForm().get('reference');
  control?.setValue('typed by the operator');
  control?.markAsDirty();
}

describe('unsavedChangesGuard', () => {
  beforeAll(installGridJsdomSupport);
  afterAll(uninstallGridJsdomSupport);

  it('blocks navigation away from a dirty document and offers Save / Discard / Stay', async () => {
    const ops = new RecordingOperations();
    const { harness, editor, router } = await navigateToEditor(ops);
    makeDirty(editor);
    harness.detectChanges();
    expect(editor.dirty()).toBe(true);

    const navigation = router.navigateByUrl('/elsewhere');
    await flush();
    harness.detectChanges();
    await flush();

    // All three outcomes are offered - `UnsavedChangesModal.razor` has three,
    // and `app-confirm-dialog` models only two (INV-006's M2-C04-03 amendment).
    expect(screen.getByRole('button', { name: 'Save and leave' })).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Discard changes' })).toBeTruthy();
    const stay = screen.getByRole('button', { name: 'Stay on this page' });

    await userEvent.click(stay);
    harness.detectChanges();

    expect(await navigation).toBe(false);
    expect(router.url).toBe('/doc');
  });

  it('does not block navigation away from a clean document', async () => {
    const ops = new RecordingOperations();
    const { harness, editor, router } = await navigateToEditor(ops);
    expect(editor.dirty()).toBe(false);

    const navigated = await router.navigateByUrl('/elsewhere');
    harness.detectChanges();

    expect(navigated).toBe(true);
    expect(router.url).toBe('/elsewhere');
    expect(screen.queryByRole('button', { name: 'Stay on this page' })).toBeNull();
  });

  it('discarding lets the navigation through', async () => {
    const ops = new RecordingOperations();
    const { harness, editor, router } = await navigateToEditor(ops);
    makeDirty(editor);
    harness.detectChanges();

    const navigation = router.navigateByUrl('/elsewhere');
    await flush();
    harness.detectChanges();
    await flush();
    await userEvent.click(screen.getByRole('button', { name: 'Discard changes' }));
    harness.detectChanges();

    expect(await navigation).toBe(true);
    expect(router.url).toBe('/elsewhere');
  });

  it('registers beforeunload only while dirty and removes it on destroy', async () => {
    const added = vi.spyOn(window, 'addEventListener');
    const removed = vi.spyOn(window, 'removeEventListener');
    const ops = new RecordingOperations();
    const { harness, editor } = await navigateToEditor(ops);

    const beforeUnloadAdds = () =>
      added.mock.calls.filter(([type]) => type === 'beforeunload').length;
    const beforeUnloadRemovals = () =>
      removed.mock.calls.filter(([type]) => type === 'beforeunload').length;

    expect(beforeUnloadAdds()).toBe(0);

    makeDirty(editor);
    harness.detectChanges();
    expect(beforeUnloadAdds()).toBe(1);

    editor.headerForm().markAsPristine();
    harness.detectChanges();
    expect(beforeUnloadRemovals()).toBe(1);

    makeDirty(editor);
    harness.detectChanges();
    expect(beforeUnloadAdds()).toBe(2);

    harness.fixture.destroy();
    expect(beforeUnloadRemovals()).toBe(2);

    added.mockRestore();
    removed.mockRestore();
  });
});
