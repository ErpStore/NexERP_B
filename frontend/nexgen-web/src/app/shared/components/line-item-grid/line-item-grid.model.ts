import type { Money, Qty } from '../../utils/decimal';
import type { DataGridColumn } from '../data-grid';

/**
 * The row/column/event vocabulary of `LineItemGrid` (M2-C07). Specification:
 * `docs/kb/execution/tasks/M2-C07.md`.
 *
 * **Nothing here computes anything.** A column says how a cell is edited; an
 * event says what happened. Neither decides what the change means - that is
 * always the caller's, over `rowEvent`, per ADR-007 `:144-152` and this
 * task's own *Business Rules* section. See `README.md` in this folder for
 * the worked event-contract table (M0-13-style evidence, not prose alone).
 */

// --- Row identity ------------------------------------------------------------

/**
 * A stable per-row id, independent of array position. `Alt+ArrowUp/Down`
 * moves a row's *index*; its id - and therefore its focus, its busy state
 * and its pending server round trip - must follow it, not stay behind.
 *
 * Deliberately a branded string, not the row's own primary key: an unsaved
 * row has no `PoSubId` yet (`AddRow()`, `MfgPOUpsert.razor:2706-2713`,
 * constructs a bare `MfgPoSubVM` with no id), so row identity here is a
 * client-only concern, generated once when the row enters the form and never
 * reused.
 */
declare const ROW_ID_BRAND: unique symbol;
export type LineItemRowId = string & { readonly [ROW_ID_BRAND]: 'LineItemRowId' };

/** Constructs a {@link LineItemRowId}. The only place that does. */
export function makeRowId(seed: string): LineItemRowId {
  return seed as LineItemRowId;
}

// --- Columns -------------------------------------------------------------

export type LineItemCellEditorKind =
  'text' | 'decimal' | 'integer' | 'date' | 'select' | 'typeahead' | 'checkbox' | 'readonly';

/**
 * What a committed edit in one column means, in the vocabulary `rowEvent`
 * speaks. `null` (the default, via omitting `onEditCommitted`) means the
 * field is shape-only - a remark, a reference number - and no domain event
 * fires for it; the `FormArray` alone is the record of the change.
 */
export type LineItemDomainEventKind =
  'item-selected' | 'quantity-changed' | 'rate-changed' | 'row-cancel-toggled';

/**
 * One editable column. Extends {@link DataGridColumn} - the same `field`,
 * `title`, `align`, `width`, `numeric`, `priority` vocabulary M2-C05-01
 * defined - with what editing adds. A column that only ever *displays* still
 * uses `DataGridColumn` fields; `editor: 'readonly'` is what a column that
 * can never be edited (e.g. a server-computed running balance) declares.
 */
export interface LineItemColumn<TLine> extends DataGridColumn<TLine> {
  /** Which cell editor renders this column. */
  readonly editor: LineItemCellEditorKind;
  /**
   * Per-row override of whether this cell is editable right now - e.g. an
   * item column that locks once `PoSubId > 0` and the line has been
   * transacted against. Defaults to always-editable when omitted; `readOnly`
   * on the grid itself overrides this in either direction.
   */
  readonly editableWhen?: (row: TLine) => boolean;
  /**
   * Decimal places for a `'decimal'` column. Never a bare literal at the
   * call site - falls back to the grid's own `decimalPlaces` input, which
   * itself must come from server settings (`Companydetails.DecimalPlaces`),
   * never a literal either.
   */
  readonly decimalPlaces?: number;
  /** Options for a `'select'` column. Required when `editor === 'select'`. */
  readonly options?: readonly { readonly value: unknown; readonly label: string }[];
  /** Caller-supplied search for a `'typeahead'` column. Required when `editor === 'typeahead'`. */
  readonly typeaheadLoader?: (
    query: string,
  ) => Promise<readonly { readonly value: unknown; readonly label: string }[]>;
  /**
   * Maps a committed edit in this column to the `rowEvent` the grid raises,
   * naming both the {@link LineItemDomainEventKind} and the raw value
   * carried. Omit for a field with no domain meaning of its own - the grid
   * still commits it to the `FormArray`, it just raises no event.
   *
   * The function receives the **row as committed so far** and the new raw
   * value; it returns only the discriminant, never a computed replacement -
   * `rowEvent`'s payload is filled in by the grid from this and the raw
   * value is what reaches the caller, unmodified.
   */
  readonly onEditCommitted?: (row: TLine, value: unknown) => LineItemDomainEventKind | null;
}

// --- Row events ------------------------------------------------------------

interface LineItemRowEventBase {
  readonly rowId: LineItemRowId;
}

/**
 * A domain field committed. **The grid does not know what this means** - it
 * marks the row busy and read-only, and waits. `respond` is called exactly
 * once by the caller: with a patch to apply, or `null` to revert the row to
 * its last committed values (a refusal, e.g. the 409 case in *Error state*).
 *
 * This is the whole of instruction 11/12's "the grid applies the caller's
 * returned row state and does not compute a replacement" - there is no
 * other path by which a domain value reaches the row.
 */
export interface LineItemDomainEvent<TLine> extends LineItemRowEventBase {
  readonly type: LineItemDomainEventKind;
  /** The column `field` whose commit raised this event. */
  readonly field: string;
  /** The raw value the operator entered or selected - never pre-interpreted. */
  readonly value: unknown;
  readonly respond: (patch: Partial<TLine> | null) => void;
}

/**
 * A row lifecycle operation the grid has **already applied locally** -
 * row-add, duplicate, remove, reorder, an upstream pull, resolved through
 * {@link LineItemFormArray}. Informational: there is nothing to respond to,
 * because nothing here is a domain decision (see *Business Rules*: "the
 * `Slno`... renumbering is presentation, not a decision" - the same reasoning
 * covers add/duplicate/remove/reorder identically).
 */
export interface LineItemLifecycleEvent extends LineItemRowEventBase {
  readonly type: 'row-added' | 'row-duplicated' | 'row-removed' | 'rows-reordered' | 'lines-pulled';
  /** `'row-duplicated'` only: the row it was copied from. */
  readonly sourceRowId?: LineItemRowId;
  /** `'rows-reordered'` only. */
  readonly fromIndex?: number;
  /** `'rows-reordered'` only. */
  readonly toIndex?: number;
  /** `'lines-pulled'` only: every row id the pull added, `rowId` is the last (focused) one. */
  readonly rowIds?: readonly LineItemRowId[];
}

/** The single discriminated union `rowEvent` emits. */
export type LineItemRowEvent<TLine> = LineItemDomainEvent<TLine> | LineItemLifecycleEvent;

/** `true` for the four events that carry a `respond` callback and busy the row. */
export function isDomainEvent<TLine>(
  event: LineItemRowEvent<TLine>,
): event is LineItemDomainEvent<TLine> {
  return (
    event.type === 'item-selected' ||
    event.type === 'quantity-changed' ||
    event.type === 'rate-changed' ||
    event.type === 'row-cancel-toggled'
  );
}

// --- Row/gutter state --------------------------------------------------------

/** One row's validation state, keyed by `rowId` - always **from the server**. */
export interface LineItemRowError {
  readonly rowId: LineItemRowId;
  readonly messages: readonly string[];
}

/** Money and quantity re-exported here so a column-config file needs one import. */
export type { Money, Qty };

// --- Constants ---------------------------------------------------------------

/** KB-050 Performance targets: keystroke-to-paint on a 200-row fixture. */
export const LINE_ITEM_GRID_TARGET_LATENCY_MS = 50;

/** The working size the target is measured at; 1,000 must not degrade further. */
export const LINE_ITEM_GRID_MEASURED_ROW_COUNT = 200;

/** Below this viewport width the grid renders read-only (KB-051 Responsive behaviour). */
export const LINE_ITEM_GRID_READONLY_BREAKPOINT_PX = 768;
