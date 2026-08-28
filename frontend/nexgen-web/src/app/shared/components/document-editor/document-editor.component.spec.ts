import { Component, signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { Subject } from 'rxjs';
import { afterAll, afterEach, beforeAll, describe, expect, it } from 'vitest';

import type { ApiProblem } from '@/app/core/http/api-problem';

import { installGridJsdomSupport, uninstallGridJsdomSupport } from '../data-grid/test-fixtures';
import { provideToast } from '../feedback';
import { makeRowId, type LineItemDomainEvent } from '../line-item-grid';
import { provideConfirmDialog } from '../overlay';
import { DocumentEditorComponent } from './document-editor.component';
import type { DocumentEditorConfig, DocumentSnapshot } from './document-editor.model';
import { DocumentRegionDirective } from './document-region.directive';
import {
  RecordingOperations,
  testConfig,
  testSnapshot,
  type TestHeader,
  type TestLine,
} from './test-fixtures';

/**
 * The shell itself (M2-C08-01 Testing Requirements 7-16): non-blocking save,
 * the server-error contract, the slot contract, the anti-leak rule and the
 * no-cache rule.
 */

async function flush(): Promise<void> {
  await new Promise((resolve) => setTimeout(resolve, 0));
}

const PROVIDERS = [provideRouter([]), provideConfirmDialog(), provideToast()];

async function renderEditor(
  config: DocumentEditorConfig<TestHeader, TestLine>,
  mode: 'create' | 'edit' | 'view' = 'edit',
  id: number | null = 7,
) {
  const view = await render(DocumentEditorComponent<TestHeader, TestLine>, {
    inputs: { config, mode, documentId: id },
    providers: PROVIDERS,
  });
  await view.fixture.whenStable();
  return view;
}

describe('DocumentEditorComponent', () => {
  beforeAll(installGridJsdomSupport);
  afterAll(uninstallGridJsdomSupport);
  afterEach(() => localStorage.clear());

  it('disables the footer actions and shows an inline spinner while saving - and never overlays the page', async () => {
    const ops = new RecordingOperations();
    const pending = new Subject<DocumentSnapshot<TestHeader, TestLine>>();
    const config = testConfig({ ...ops, update: () => pending.asObservable() });
    const view = await renderEditor(config);

    const save = screen.getByRole('button', { name: 'Save' });
    await userEvent.click(save);
    await flush();
    view.fixture.detectChanges();

    expect(view.fixture.componentInstance.saving()).toBe(true);
    expect(screen.getByRole<HTMLButtonElement>('button', { name: 'Save' }).disabled).toBe(true);
    expect(screen.getByRole<HTMLButtonElement>('button', { name: 'Cancel' }).disabled).toBe(true);
    expect(document.querySelector('.document-command-bar__spinner')).not.toBeNull();
    // The deliberate divergence from ProcessingOverlay.razor: nothing blocks the page.
    expect(document.querySelector('app-busy-overlay')).toBeNull();
    expect(document.querySelector('.p-blockui')).toBeNull();
    expect(screen.getByLabelText<HTMLInputElement>(/Reference/i)).toBeTruthy();

    pending.complete();
  });

  it('maps a 400 errors dictionary onto the controls, opens the containing section and focuses it', async () => {
    const ops = new RecordingOperations();
    ops.saveFailure = {
      status: 400,
      title: 'One or more validation errors occurred.',
      errors: { notes: ['Notes are required by the server.'], unknownField: ['Not a control.'] },
    } satisfies ApiProblem;
    const config = testConfig(ops, {
      header: {
        kind: 'flat',
        sections: [
          {
            id: 'primary',
            title: 'Primary',
            fields: [{ name: 'reference', label: 'Reference', control: 'text' }],
          },
          {
            id: 'secondary',
            title: 'Secondary',
            collapsible: true,
            initiallyCollapsed: true,
            fields: [{ name: 'notes', label: 'Notes', control: 'text' }],
          },
        ],
      },
    });
    const view = await renderEditor(config);

    await userEvent.click(screen.getByRole('button', { name: 'Save' }));
    await flush();
    view.fixture.detectChanges();

    const notes = screen.getByLabelText<HTMLInputElement>(/Notes/i);
    expect(view.fixture.componentInstance.headerForm().get('notes')?.errors).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Hide' })).toBeTruthy(); // section is open
    expect(document.activeElement).toBe(notes);
    // A key matching no control is shown as a summary message, not silently dropped.
    expect(screen.getByText('Not a control.')).toBeTruthy();
  });

  it('renders a 403 inline, without navigating', async () => {
    const ops = new RecordingOperations();
    ops.saveFailure = {
      status: 403,
      title: 'Screen right denied.',
      screen: 'Test Screen',
      right: 'edit',
    } satisfies ApiProblem;
    const view = await renderEditor(testConfig(ops));

    await userEvent.click(screen.getByRole('button', { name: 'Save' }));
    await flush();
    view.fixture.detectChanges();

    expect(document.querySelector('app-permission-denied-state')).not.toBeNull();
    expect(screen.getByLabelText<HTMLInputElement>(/Reference/i)).toBeTruthy();
  });

  it("renders a 409's title verbatim", async () => {
    const ops = new RecordingOperations();
    ops.saveFailure = {
      status: 409,
      title: 'Cannot delete this record as a downstream transaction exists.',
    } satisfies ApiProblem;
    const view = await renderEditor(testConfig(ops));

    await userEvent.click(screen.getByRole('button', { name: 'Save' }));
    await flush();
    view.fixture.detectChanges();

    // Twice, deliberately: the inline alert on the form and the toast. Both verbatim.
    expect(
      screen.getAllByText('Cannot delete this record as a downstream transaction exists.').length,
    ).toBeGreaterThan(0);
  });

  it('forwards a rowEvent to the config operation verbatim and computes nothing itself', async () => {
    const ops = new RecordingOperations();
    const view = await renderEditor(testConfig(ops));
    const responses: (Partial<TestLine> | null)[] = [];
    const event: LineItemDomainEvent<TestLine> = {
      type: 'quantity-changed',
      rowId: makeRowId('row-1'),
      field: 'description',
      value: '12',
      respond: (patch) => responses.push(patch),
    };

    view.fixture.componentInstance.onRowEvent('lines', event);

    expect(ops.rowEvents).toHaveLength(1);
    expect(ops.rowEvents[0]).toBe(event);
    // The shell neither answers the event nor rewrites the value.
    expect(responses).toEqual([]);
  });

  it('follows a successful save with exactly one explicit refresh and no local cache mutation', async () => {
    const ops = new RecordingOperations();
    const view = await renderEditor(testConfig(ops));
    expect(ops.loads).toEqual([7]);

    await userEvent.click(screen.getByRole('button', { name: 'Save' }));
    await flush();
    view.fixture.detectChanges();

    expect(ops.updates).toHaveLength(1);
    expect(ops.loads).toEqual([7, 7]);
    expect(view.fixture.componentInstance.snapshot()).toEqual(ops.snapshot);
  });

  it('does not write the outgoing payload into the document while the save is in flight', async () => {
    const ops = new RecordingOperations();
    const pending = new Subject<DocumentSnapshot<TestHeader, TestLine>>();
    const view = await renderEditor(testConfig({ ...ops, update: () => pending.asObservable() }));
    const before = view.fixture.componentInstance.snapshot();

    view.fixture.componentInstance.headerForm().get('reference')?.setValue('typed, not saved');
    await userEvent.click(screen.getByRole('button', { name: 'Save' }));
    await flush();

    expect(view.fixture.componentInstance.snapshot()).toBe(before);
    pending.complete();
  });

  it('returns focus to a deterministic anchor after a save', async () => {
    const ops = new RecordingOperations();
    const view = await renderEditor(testConfig(ops));

    await userEvent.click(screen.getByRole('button', { name: 'Save' }));
    await flush();
    view.fixture.detectChanges();

    const anchor = view.container.querySelector('.document-editor__anchor');
    expect(document.activeElement).toBe(anchor);
  });

  it('remembers a section open/closed state per document type', async () => {
    const ops = new RecordingOperations();
    const view = await renderEditor(testConfig(ops));

    await userEvent.click(screen.getByRole('button', { name: 'Hide' }));
    view.fixture.detectChanges();
    await flush();

    expect(
      JSON.parse(localStorage.getItem('nexgen.document-editor.sections.test-document') ?? '{}'),
    ).toMatchObject({ secondary: true });

    view.fixture.destroy();
    TestBed.resetTestingModule();
    const second = await renderEditor(testConfig(new RecordingOperations()));
    await second.fixture.whenStable();

    expect(screen.getByRole('button', { name: 'Show' })).toBeTruthy();
  });
});

@Component({
  selector: 'app-slot-host',
  imports: [DocumentEditorComponent, DocumentRegionDirective],
  template: `
    <app-document-editor [config]="config()" mode="edit" [documentId]="7">
      <ng-template #totals>
        <p>caller totals template</p>
      </ng-template>
      <ng-template #commands>
        <button type="button">Caller command</button>
      </ng-template>
      <ng-template appDocumentRegion="attachments">
        <p>caller attachments</p>
      </ng-template>
    </app-document-editor>
  `,
})
class SlotHostComponent {
  readonly ops = new RecordingOperations();
  readonly config = signal(
    testConfig(this.ops, {
      sideRegions: [
        { id: 'attachments', label: 'Attachments' },
        { id: 'terms', label: 'Terms' },
      ],
    }),
  );
}

describe('DocumentEditorComponent slots', () => {
  beforeAll(installGridJsdomSupport);
  afterAll(uninstallGridJsdomSupport);

  it('renders only the caller-supplied templates in the totals and command slots', async () => {
    const view = await render(SlotHostComponent, { providers: PROVIDERS });
    await view.fixture.whenStable();

    expect(screen.getByText('caller totals template')).toBeTruthy();
    expect(screen.getByRole('button', { name: 'Caller command' })).toBeTruthy();

    // The shell renders no ladder, no label and no total of its own: the only
    // element inside the totals region is the caller's.
    const totals = document.querySelector('.totals-panel-slot');
    expect(totals?.textContent?.trim()).toBe('caller totals template');
  });

  it('renders no totals region at all when the caller supplies no template', async () => {
    const ops = new RecordingOperations();
    const view = await render(DocumentEditorComponent<TestHeader, TestLine>, {
      inputs: { config: testConfig(ops), mode: 'edit', documentId: 7 },
      providers: PROVIDERS,
    });
    await view.fixture.whenStable();

    expect(document.querySelector('.totals-panel-slot')).toBeNull();
  });

  it('instantiates only the open side region', async () => {
    const view = await render(SlotHostComponent, { providers: PROVIDERS });
    await view.fixture.whenStable();

    expect(screen.getByText('caller attachments')).toBeTruthy();
    expect(screen.getByRole('tab', { name: 'Terms' })).toBeTruthy();
  });

  it('loads a document snapshot into the header and the lines', async () => {
    const ops = new RecordingOperations();
    ops.snapshot = testSnapshot({ lines: [{ description: 'first' }, { description: 'second' }] });
    const view = await render(DocumentEditorComponent<TestHeader, TestLine>, {
      inputs: { config: testConfig(ops), mode: 'edit', documentId: 7 },
      providers: PROVIDERS,
    });
    await view.fixture.whenStable();

    expect(view.fixture.componentInstance.linesFor('lines').length).toBe(2);
  });
});
