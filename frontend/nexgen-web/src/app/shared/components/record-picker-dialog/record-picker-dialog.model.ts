import type { Observable } from 'rxjs';

import type { DataGridPage, DataGridRowId, DataGridWireQuery } from '../data-grid';

/**
 * The input vocabulary of `RecordPickerDialog` (M2-C06) - the replacement for
 * `V.SMART/V.SMART.Shared/Components/DetailsModal.razor`.
 *
 * Nothing here names a resource, a screen or a domain field. The picker is a
 * generic primitive: it searches, pages, selects, orders and returns records.
 * Every rule about *what may be pulled* and *what happens when it is* stays
 * behind the API (ADR-004; KB-080 section 3).
 *
 * The three parallel arrays the old component carried - `Columns`,
 * `ColumnFields` and `HiddenColumns`, kept in index lockstep at
 * `DetailsModal.razor:104-106` and `:58-62` - are deliberately **not**
 * reproduced. One `DataGridColumn` list carries title, field and visibility,
 * so a column cannot drift out of step with its data key.
 */

/**
 * `'multiple'` is what all 41 live `<DetailsModal>` instances do today - every
 * one is a multi-row pull from an upstream document (INV-054).
 *
 * `'single'` is **new capability**, not preserved behaviour: no call site in
 * `V.SMART.Shared/Pages/**` is a single-record master pick. Master picking is
 * done by the routable pages `CustomerSelection.razor` and
 * `VendorSelection.razor` instead. It is built here because those pages are
 * the picker's next consumers, and it is recorded as new rather than as
 * parity so no future session mistakes it for a migrated behaviour.
 */
export type RecordPickerSelectionMode = 'single' | 'multiple';

/**
 * One page of candidate records. Supplied by the **caller**, which is what
 * makes one component serve 33 call sites: the dialog imports no generated
 * operation and hardcodes no endpoint (M2-B10 stays a soft dependency).
 */
export type RecordPickerFetchPage<TRow> = (
  query: DataGridWireQuery,
) => Observable<DataGridPage<TRow>>;

/** A downloadable produced **by the server**. `fileName` falls back to the caller's input. */
export interface RecordPickerExport {
  readonly blob: Blob;
  readonly fileName?: string;
}

/**
 * The export request. Optional; when absent no export control is rendered.
 *
 * Export stays server-side per [ADR-005]. The old dialog was already
 * server-side - its "Print" button called
 * `ExcelExportService.ExportPendingListToExcel` and downloaded an `.xlsx`
 * (`DetailsModal.razor:241-244`) - and only the base64-through-JS-interop hop
 * is dropped, in favour of a blob. No client-side file-generation library is
 * added by this component, and none may be.
 */
export type RecordPickerExportRequest = (
  query: DataGridWireQuery,
) => Observable<Blob | RecordPickerExport>;

/**
 * How a single cell is called out.
 *
 * `label` is **required, and that is the point**: `DetailsModal.razor:218-230`
 * signalled "this line is new" and "this quantity changed" with background
 * colour alone, which fails KB-051's status vocabulary. A tone must always
 * arrive with words.
 */
export interface RecordPickerCellState {
  readonly tone: 'info' | 'success' | 'warning' | 'danger';
  /** Short, human, and rendered as text next to the value - never colour alone. */
  readonly label: string;
}

/**
 * Caller-supplied cell call-outs, replacing the two hardcoded domain
 * highlights at `DetailsModal.razor:218-230`. A shared component that knows
 * those field names is domain leakage into presentation - precisely what this
 * migration exists to stop - so the decision belongs to the caller and,
 * eventually, to the server's row data. The field names themselves are
 * recorded in the investigation registry, not written here.
 */
export type RecordPickerCellStateFn<TRow> = (
  row: TRow,
  field: string,
) => RecordPickerCellState | null;

/**
 * The wire name of the free-text search parameter.
 *
 * **`M2-B02` defines no such parameter.** `PagedQuery` carries only
 * `pageNumber`, `pageSize` and `sort`
 * (`V.SMART/V.SMART.Api/Contracts/PagedQuery.cs:36-44`), and the one shipped
 * query DTO, `CurrencyQuery`, has per-field filters and no free-text term.
 * `'search'` is therefore this component's **default, not a contract** - it is
 * an input so a caller can name whatever its endpoint accepts. Recorded as an
 * open question rather than asserted as a rule.
 */
export const RECORD_PICKER_DEFAULT_SEARCH_PARAM = 'search';

/** Keystroke debounce before the search term becomes a request. */
export const RECORD_PICKER_SEARCH_DEBOUNCE_MS = 300;

/** Rows per page inside the dialog. Narrower than a list screen's 20. */
export const RECORD_PICKER_DEFAULT_PAGE_SIZE = 10;

/**
 * The subset of RFC 7807 the dialog reads, including the two extension members
 * a screen-right refusal carries (`V.SMART/V.SMART.Api/Middleware/ApiProblems.cs:89-104`).
 * Not a generated type - the generated one is M2-B10's.
 */
export interface RecordPickerProblem {
  readonly status?: number;
  readonly title?: string;
  readonly detail?: string;
  readonly traceId?: string;
  readonly screen?: string;
  readonly right?: string;
}

/** Narrows whatever `DataGridQueryState.error` holds to a problem body, when it is one. */
export function asProblem(error: unknown): RecordPickerProblem | null {
  // Every member is optional, so a plain object already satisfies the shape:
  // reading a field that is not there yields `undefined`, which is what the
  // callers below test for. No cast, and no pretence that the body was
  // validated.
  return typeof error === 'object' && error !== null ? error : null;
}

/**
 * A 403 renders the permission-denied panel rather than the generic error one.
 * The dialog itself decides nothing about permissions - the server did, and
 * this only reads which panel says so (ADR-004).
 */
export function isPermissionDenied(error: unknown): boolean {
  return asProblem(error)?.status === 403;
}

/** The id set form of a caller's id list. */
export function toIdSet(ids: readonly DataGridRowId[]): ReadonlySet<DataGridRowId> {
  return new Set(ids);
}
