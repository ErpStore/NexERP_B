/**
 * `RecordPickerDialog` (M2-C06) - the `DetailsModal.razor` replacement.
 *
 * Three rules govern everything exported here:
 *   1. **Generic.** The picker takes a `fetchPage` function, never a resource.
 *      One component serves the 33 call sites `DetailsModal` serves today.
 *   2. **Server-paged and server-searched.** The dialog never holds the whole
 *      candidate set, and it filters nothing locally.
 *   3. **No business rule.** Eligibility, duplicate-line suppression, balance
 *      quantities and pricing are the server's. The dialog selects records.
 */
export { RecordPickerDialogComponent } from './record-picker-dialog.component';
export { RecordPickerFooterComponent } from './record-picker-footer.component';
export { RecordSelection } from './record-selection';

export {
  RECORD_PICKER_DEFAULT_PAGE_SIZE,
  RECORD_PICKER_DEFAULT_SEARCH_PARAM,
  RECORD_PICKER_SEARCH_DEBOUNCE_MS,
  asProblem,
  isPermissionDenied,
  toIdSet,
} from './record-picker-dialog.model';
export type {
  RecordPickerCellState,
  RecordPickerCellStateFn,
  RecordPickerExport,
  RecordPickerExportRequest,
  RecordPickerFetchPage,
  RecordPickerProblem,
  RecordPickerSelectionMode,
} from './record-picker-dialog.model';
