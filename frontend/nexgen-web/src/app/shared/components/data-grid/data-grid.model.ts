/**
 * The column and query-state vocabulary of `DataGrid` (M2-C05-01).
 *
 * The column model deliberately mirrors the ERP's existing grid metadata,
 * `V.SMART/V.SMART.Shared/ViewModels/GridColumn.cs:3-13`
 * (`Title`, `Field`, `IsVisible`, `IsDate`, `Width`, `Align`, `IsDetailColumn`),
 * so that **M2-C05-02** can round-trip persisted preferences without a
 * translation table that drifts. Where the names differ they differ on
 * purpose:
 *
 *   - `Align` is `"text-center"` in Blazor - a Bootstrap class name, not a
 *     value. Here it is the value (`'left' | 'center' | 'right'`); the CSS is
 *     the component's business.
 *   - `IsVisible` becomes `defaultVisible`, because with M2-C05-02 in the
 *     picture the *current* visibility is per-user state, not column metadata.
 *   - `IsDetailColumn` has no equivalent: it marks a column that the Blazor
 *     list renders inside an expansion row, which is not a capability this
 *     child implements.
 *
 * Nothing here decides anything about the domain. A column says how a value is
 * displayed; it never says what the value means.
 */

/** Horizontal alignment of a column's cells and its header. */
export type DataGridAlign = 'left' | 'center' | 'right';

/** Row height, per KB-051 Principles: comfortable 36 px, compact 30 px. */
export type DataGridDensity = 'comfortable' | 'compact';

/** Row-selection behaviour. `'none'` renders no selection affordance at all. */
export type DataGridSelectionMode = 'none' | 'single' | 'multiple';

/**
 * Responsive drop order (KB-051 Responsive behaviour). `'low'` columns are the
 * ones removed first, between 768 px and 1023 px.
 */
export type DataGridColumnPriority = 'high' | 'normal' | 'low';

export type DataGridSortDirection = 'asc' | 'desc';

/** Which filter control the header's filter row renders for a column. */
export type DataGridFilterKind = 'none' | 'text' | 'date';

/** A stable identity for a row - what selection and focus are keyed by. */
export type DataGridRowId = string | number;

/** Extracts {@link DataGridRowId} from a row. Supplied by the caller. */
export type DataGridRowIdFn<TRow> = (row: TRow) => DataGridRowId;

/**
 * One column. `field` is the property read from the row *and* the wire name of
 * the sort term, so it matches the server's `SortableFields` allow-list
 * (`V.SMART/V.SMART.Api/Contracts/PagedQuery.cs:82`). A filter that binds to a
 * different query parameter says so through `filterParam`.
 */
export interface DataGridColumn<TRow> {
  /** Row property and sort wire name. */
  readonly field: string;
  /** Header text. Already translated by the caller. */
  readonly title: string;
  /** Defaults to `'right'` for `numeric` columns and `'left'` otherwise. */
  readonly align?: DataGridAlign;
  /** Any CSS width. Mirrors `GridColumn.Width`, whose default is `"120px"`. */
  readonly width?: string;
  /** Mirrors `GridColumn.IsDate`. Presentation only - no parsing happens here. */
  readonly isDate?: boolean;
  /** Right-aligned and `tabular-nums`. Never implies client-side arithmetic. */
  readonly numeric?: boolean;
  /** Pins the column to an edge while the grid scrolls horizontally. */
  readonly frozen?: 'left' | 'right' | null;
  /** May the user hide it? Consumed by M2-C05-02. Default `true`. */
  readonly hideable?: boolean;
  /** Included in M2-C05-03's export? Default `true`. */
  readonly exportable?: boolean;
  /** Mirrors `GridColumn.IsVisible`. Default `true`. */
  readonly defaultVisible?: boolean;
  /** Responsive drop order. Default `'normal'`. */
  readonly priority?: DataGridColumnPriority;
  /** Default `true`. A column the server cannot sort must set `false`. */
  readonly sortable?: boolean;
  /** Filter control in the header filter row. Default `'none'`. */
  readonly filter?: DataGridFilterKind;
  /** Query-parameter name for the filter, when it is not `field`. */
  readonly filterParam?: string;
  /**
   * Renders the cell from the whole row rather than `row[field]`. Formatting
   * only: a function here that adds, multiplies or compares money is the
   * defect this component exists to prevent (KB-080 section 3, principle 4).
   */
  readonly value?: (row: TRow) => string | number | null | undefined;
}

/** One sort term. Multi-column sort is a list of these, most significant first. */
export interface DataGridSort {
  readonly field: string;
  readonly direction: DataGridSortDirection;
}

/**
 * The complete query state of a grid. `filters` is keyed by **query-parameter
 * name** (`filterParam ?? field`), so it is already in the vocabulary the
 * server and the URL both use; only paging and sort need translating.
 */
export interface DataGridState {
  /** 1-based, matching `PagedQuery.PageNumber` (`PagedQuery.cs:58-61`). */
  readonly page: number;
  readonly pageSize: number;
  readonly sort: readonly DataGridSort[];
  readonly filters: Readonly<Record<string, string>>;
}

/** `PagedQuery.DefaultPageSize` - `V.SMART/V.SMART.Api/Contracts/PagedQuery.cs:46`. */
export const DATA_GRID_DEFAULT_PAGE_SIZE = 20;

/** `PagedQuery.MaxPageSize` - `V.SMART/V.SMART.Api/Contracts/PagedQuery.cs:55`. */
export const DATA_GRID_MAX_PAGE_SIZE = 100;

/**
 * The page sizes the live Blazor list offers (`CurrencyList.razor:85-87`),
 * plus 100 - the server's documented maximum.
 */
export const DATA_GRID_PAGE_SIZE_OPTIONS: readonly number[] = [10, 20, 50, 100];

/** Rows on a page at or above which virtual scrolling engages (KB-051 Data display). */
export const DATA_GRID_VIRTUAL_ROW_THRESHOLD = 500;

/** Resolved row height per density, matching `--row-height-*` in `src/styles/tokens.css:104-105`. */
export const DATA_GRID_ROW_HEIGHT_PX: Readonly<Record<DataGridDensity, number>> = {
  comfortable: 36,
  compact: 30,
};

/** `GridColumn.Width`'s own default is `"120px"` (`GridColumn.cs:10`). */
export const DATA_GRID_DEFAULT_COLUMN_WIDTH_PX = 120;

/** The narrowest a column may be dragged - below this the header text vanishes. */
export const DATA_GRID_MIN_COLUMN_WIDTH_PX = 48;

/** A sanity ceiling, so a runaway drag cannot make one column wider than a screen. */
export const DATA_GRID_MAX_COLUMN_WIDTH_PX = 1200;

/** Free-text filter debounce, per the task specification. */
export const DATA_GRID_FILTER_DEBOUNCE_MS = 300;

/**
 * The server rejects a `pageSize` outside 1..100 with a 400
 * (`V.SMART/V.SMART.Api/Contracts/PagedQuery.cs:63-67`). Clamping here turns a
 * hand-edited URL into a large page instead of an error page; the server stays
 * the enforcement point either way, which is the point of clamping rather than
 * trusting.
 */
export function clampPageSize(pageSize: number): number {
  if (!Number.isFinite(pageSize) || pageSize < 1) {
    return DATA_GRID_DEFAULT_PAGE_SIZE;
  }
  return Math.min(Math.trunc(pageSize), DATA_GRID_MAX_PAGE_SIZE);
}

/** {@link clampPageSize}, applied to a whole state object. */
export function clampPageSizeState(state: DataGridState): DataGridState {
  const pageSize = clampPageSize(state.pageSize);
  return pageSize === state.pageSize ? state : { ...state, pageSize };
}

/** The state a grid starts in before any URL or caller override. */
export function defaultDataGridState(pageSize = DATA_GRID_DEFAULT_PAGE_SIZE): DataGridState {
  return { page: 1, pageSize, sort: [], filters: {} };
}

/** `true` when the column should be rendered right-aligned and tabular. */
export function isNumericColumn<TRow>(column: DataGridColumn<TRow>): boolean {
  return column.numeric === true;
}

/** The effective alignment of a column, applying the numeric default. */
export function columnAlign<TRow>(column: DataGridColumn<TRow>): DataGridAlign {
  return column.align ?? (isNumericColumn(column) ? 'right' : 'left');
}

/** The query-parameter name a column's filter binds to. */
export function columnFilterParam<TRow>(column: DataGridColumn<TRow>): string {
  return column.filterParam ?? column.field;
}

/**
 * The raw cell value, before any formatting the template applies.
 *
 * A property that is neither a string, a number nor a boolean renders as
 * nothing rather than as `[object Object]`. A row with a nested object in it
 * needs a `column.value` function, and making that explicit is better than
 * silently printing a stringified object into a list an operator reads.
 */
export function cellValue<TRow>(
  column: DataGridColumn<TRow>,
  row: TRow,
): string | number | null | undefined {
  if (column.value) {
    return column.value(row);
  }
  const value = (row as Record<string, unknown>)[column.field];
  if (value === null || value === undefined) {
    return value;
  }
  if (typeof value === 'number' || typeof value === 'string') {
    return value;
  }
  return typeof value === 'boolean' ? String(value) : null;
}
