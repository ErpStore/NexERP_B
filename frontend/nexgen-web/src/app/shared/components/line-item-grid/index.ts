/**
 * The `LineItemGrid` surface (M2-C07). Specification: `docs/kb/execution/tasks/M2-C07.md`.
 *
 * An editing surface, not a calculator: it renders cells, a keyboard model
 * and row lifecycle operations, and reports what happened over `rowEvent`.
 * Every domain consequence - what a value resolves to, whether it is valid
 * beyond shape - is the caller's, over the `respond()` callback on each
 * domain event. See `line-item-grid.model.ts` for the exact contract.
 */
export { LineItemGridComponent } from './line-item-grid.component';
export type { LineItemCellCommit } from './line-item-row.component';

export {
  isDomainEvent,
  makeRowId,
  LINE_ITEM_GRID_MEASURED_ROW_COUNT,
  LINE_ITEM_GRID_READONLY_BREAKPOINT_PX,
  LINE_ITEM_GRID_TARGET_LATENCY_MS,
} from './line-item-grid.model';
export type {
  LineItemCellEditorKind,
  LineItemColumn,
  LineItemDomainEvent,
  LineItemDomainEventKind,
  LineItemLifecycleEvent,
  LineItemRowError,
  LineItemRowEvent,
  LineItemRowId,
} from './line-item-grid.model';

export { LineItemForm } from './line-item-form';
export type {
  LineItemFormArrayModel,
  LineItemFormGroup,
  LineItemRowFactory,
} from './line-item-form';

export {
  caretExhausted,
  decideLineGridKey,
  firstEditableColumn,
  isLineGridShortcut,
  lastEditableColumn,
  nextEditableCell,
} from './line-grid-keyboard';
export type { LineGridAction, LineGridBounds } from './line-grid-keyboard';

export { buildClipboardPastePreview, parseClipboardGrid } from './clipboard-paste';
export type {
  ClipboardPasteCell,
  ClipboardPastePreview,
  ClipboardPasteRowPreview,
} from './clipboard-paste';
