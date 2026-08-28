import { FormControl, FormGroup } from '@angular/forms';
import { of, throwError } from 'rxjs';

import type { ApiProblem } from '@/app/core/http/api-problem';

import type { LineItemColumn, LineItemRowFactory } from '../line-item-grid';
import type {
  DocumentEditorConfig,
  DocumentEditorOperations,
  DocumentSnapshot,
} from './document-editor.model';

/**
 * **Test fixtures only.** Nothing here is bundled into the application, and
 * nothing here names a real ERP document, field or endpoint - the shell is
 * generic by contract, so its own tests must be too. `reference`, `notes` and
 * `description` are deliberately meaningless.
 */

export interface TestHeader {
  reference: string | null;
  notes: string | null;
}

export interface TestLine {
  description: string;
}

export const createTestLineRow: LineItemRowFactory<TestLine> = (initial) =>
  new FormGroup({
    description: new FormControl<string>(initial.description ?? '', { nonNullable: true }),
  });

export const TEST_LINE_COLUMNS: readonly LineItemColumn<TestLine>[] = [
  { field: 'description', title: 'Description', editor: 'text' },
];

export function testSnapshot(
  overrides: Partial<DocumentSnapshot<TestHeader, TestLine>> = {},
): DocumentSnapshot<TestHeader, TestLine> {
  return {
    id: 7,
    header: { reference: 'REF-7', notes: 'loaded note' },
    lines: [{ description: 'loaded line' }],
    status: 'Open',
    documentNumber: 'DOC/7',
    ...overrides,
  };
}

/** Records every call, so a test can assert what the shell did and did not do. */
export class RecordingOperations implements DocumentEditorOperations<TestHeader, TestLine> {
  readonly loads: (string | number)[] = [];
  readonly creates: unknown[] = [];
  readonly updates: unknown[] = [];
  readonly rowEvents: unknown[] = [];

  /** Set to a problem body to make the next save fail. */
  saveFailure: ApiProblem | null = null;
  loadFailure: ApiProblem | null = null;
  snapshot: DocumentSnapshot<TestHeader, TestLine> = testSnapshot();

  readonly load = (id: string | number) => {
    this.loads.push(id);
    return this.loadFailure ? throwError(() => this.loadFailure) : of(this.snapshot);
  };

  readonly create = (payload: unknown) => {
    this.creates.push(payload);
    return this.saveFailure ? throwError(() => this.saveFailure) : of(this.snapshot);
  };

  readonly update = (id: string | number, payload: unknown) => {
    this.updates.push({ id, payload });
    return this.saveFailure ? throwError(() => this.saveFailure) : of(this.snapshot);
  };

  readonly rowEvent = (event: unknown): void => {
    this.rowEvents.push(event);
  };
}

export function testConfig(
  operations: DocumentEditorOperations<TestHeader, TestLine>,
  overrides: Partial<DocumentEditorConfig<TestHeader, TestLine>> = {},
): DocumentEditorConfig<TestHeader, TestLine> {
  return {
    documentType: 'test-document',
    title: 'Test Document',
    noun: 'Test Document',
    resource: 'test-documents',
    header: {
      kind: 'flat',
      sections: [
        {
          id: 'primary',
          title: 'Primary',
          fields: [{ name: 'reference', label: 'Reference', control: 'text', required: true }],
        },
        {
          id: 'secondary',
          title: 'Secondary',
          collapsible: true,
          fields: [{ name: 'notes', label: 'Notes', control: 'textarea' }],
        },
      ],
    },
    lineGrids: [
      {
        id: 'lines',
        title: 'Lines',
        columns: TEST_LINE_COLUMNS,
        createRow: createTestLineRow,
      },
    ],
    totals: { title: 'Totals', rows: [{ key: 'grandTotal', label: 'Grand Total' }] },
    operations,
    ...overrides,
  };
}
