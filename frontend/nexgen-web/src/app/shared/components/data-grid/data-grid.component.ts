import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  computed,
  contentChild,
  effect,
  inject,
  input,
  model,
  output,
  signal,
  untracked,
  type TemplateRef,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgTemplateOutlet } from '@angular/common';
import { TableModule } from 'primeng/table';

import { ProgressBarComponent } from '../feedback/progress-bar.component';
import { DatePickerComponent } from '../form/date-picker.component';
import { TextInputComponent } from '../form/text-input.component';
import { DataGridHeaderComponent } from './data-grid-header.component';
import { DataGridPaginationComponent } from './data-grid-pagination.component';
import {
  DATA_GRID_DEFAULT_COLUMN_WIDTH_PX,
  DATA_GRID_FILTER_DEBOUNCE_MS,
  DATA_GRID_MAX_COLUMN_WIDTH_PX,
  DATA_GRID_MIN_COLUMN_WIDTH_PX,
  DATA_GRID_PAGE_SIZE_OPTIONS,
  DATA_GRID_ROW_HEIGHT_PX,
  DATA_GRID_VIRTUAL_ROW_THRESHOLD,
  cellValue,
  columnAlign,
  columnFilterParam,
  defaultDataGridState,
  type DataGridColumn,
  type DataGridDensity,
  type DataGridRowIdFn,
  type DataGridSelectionMode,
  type DataGridSort,
  type DataGridState,
} from './data-grid.model';
import {
  HEADER_ROW_INDEX,
  ariaRowIndex,
  clampCell,
  handlesKey,
  nextFocusedCell,
  rovingTabIndex,
  type GridBounds,
  type GridCell,
} from './grid-keyboard-navigation';

/**
 * The server-paged list grid (M2-C05-01). One component, one component
 * library, for the 93 `<QuickGrid>`-and-Bootstrap list screens the Blazor app
 * renders today (`PaymentList.razor:134-238` and 92 others) - which is how
 * R-22's two-design-systems problem is retired for list screens.
 *
 * **Controlled, not self-driving.** The grid owns no query state. It receives
 * a {@link DataGridState}, renders it, and emits the state the user asked for
 * next; `DataGridQueryState` turns that into exactly one request. That is why
 * `p-table` is configured `[lazy]="true"` with `[lazyLoadOnInit]="false"` and
 * why no `pSortableColumn`, `p-paginator` or filter directive appears
 * anywhere: giving PrimeNG a second opinion about the page number is how a
 * grid silently starts sorting the current page instead of the result set.
 *
 * **What it never does.** No totals from the visible page, no money
 * arithmetic, no decision about whether a row may be edited, no filtering of
 * rows for a domain reason. The grid receives rows and renders them; every
 * domain decision stays behind the API (ADR-004, KB-080 section 3).
 */
@Component({
  selector: 'app-data-grid',
  templateUrl: './data-grid.component.html',
  styleUrl: './data-grid.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    NgTemplateOutlet,
    FormsModule,
    TableModule,
    ProgressBarComponent,
    DatePickerComponent,
    TextInputComponent,
    DataGridHeaderComponent,
    DataGridPaginationComponent,
  ],
})
export class DataGridComponent<TRow> {
  readonly #host = inject<ElementRef<HTMLElement>>(ElementRef);

  readonly columns = input.required<readonly DataGridColumn<TRow>[]>();
  readonly rows = input<readonly TRow[]>([]);
  /** The server's filtered, unpaged total. Drives `aria-rowcount` and the pager. */
  readonly totalCount = input(0);
  readonly state = input<DataGridState>(defaultDataGridState());
  /** First load - nothing on screen yet. Renders skeleton rows, never a blank table. */
  readonly loading = input(false);
  /** A reload with rows on screen. They stay put under a progress bar. */
  readonly refetching = input(false);
  /** The server's `ProblemDetails`, passed through untouched. Rendered by M2-C05-03. */
  readonly error = input<unknown>(null);
  readonly density = model<DataGridDensity>('comfortable');
  /** Stable row identity. Selection and focus are keyed by it. */
  readonly getRowId = input.required<DataGridRowIdFn<TRow>>();
  readonly selectionMode = input<DataGridSelectionMode>('none');
  /** Two-way. The **caller** owns the selection; the grid only proposes changes. */
  readonly selection = model<readonly TRow[]>([]);
  readonly pageSizeOptions = input<readonly number[]>(DATA_GRID_PAGE_SIZE_OPTIONS);
  readonly resizableColumns = input(true);
  /** Accessible name for the grid. Required for a `role="grid"` with no visible caption. */
  readonly ariaLabel = input('Data grid');
  /** Height of the scroll viewport once virtualisation engages. */
  readonly height = input('480px');
  /** Rows a `PageUp`/`PageDown` moves by. */
  readonly pageJumpRows = input(10);
  readonly filterDebounceMs = input(DATA_GRID_FILTER_DEBOUNCE_MS);

  /**
   * Per-user column visibility, keyed by `field`.
   *
   * TODO(M2-C05-02): typed and honoured here, but nothing persists it yet -
   * M2-C05-02 wires it to `UserColumnPreference`. Leaving the seam typed now
   * is what lets that task land without a breaking change to this input
   * surface.
   */
  readonly columnVisibility = model<Readonly<Record<string, boolean>> | undefined>(undefined);

  /** The next query state the user asked for. Feed it to `DataGridQueryState.apply`. */
  readonly stateChange = output<DataGridState>();
  /** `Enter` on the focused row, or a double click. */
  readonly rowActivate = output<TRow>();

  /** Row-level action buttons, rendered in a trailing cell. */
  readonly rowActions = contentChild<TemplateRef<{ $implicit: TRow }>>('rowActions');
  /** TODO(M2-C05-03): the empty state. Typed seam; the default placeholder stays accessible. */
  readonly emptyTemplate = contentChild<TemplateRef<unknown>>('empty');
  /** TODO(M2-C05-03): the error state. The `ProblemDetails` object reaches it untouched. */
  readonly errorTemplate = contentChild<TemplateRef<{ $implicit: unknown }>>('error');
  /** TODO(M2-C05-03): the toolbar slot, where export lands. */
  readonly toolbarTemplate = contentChild<TemplateRef<unknown>>('toolbar');

  readonly #focused = signal<GridCell>({ row: HEADER_ROW_INDEX, col: 0 });
  readonly #filterDraft = signal<Readonly<Record<string, string>>>({});
  readonly #resizeWidths = signal<Readonly<Record<string, string>>>({});
  #filterTimer: ReturnType<typeof setTimeout> | null = null;

  readonly focusedCell = this.#focused.asReadonly();
  readonly columnWidths = this.#resizeWidths.asReadonly();
  readonly headerRow = HEADER_ROW_INDEX;

  constructor() {
    // A debounce in flight when the screen navigates away would emit into a
    // destroyed component one keystroke later.
    inject(DestroyRef).onDestroy(() => {
      if (this.#filterTimer !== null) {
        clearTimeout(this.#filterTimer);
        this.#filterTimer = null;
      }
    });

    // The filter inputs follow the state they are given - a URL pasted into a
    // new tab, or the back button - except while the user is mid-keystroke.
    effect(() => {
      const filters = this.state().filters;
      untracked(() => {
        if (this.#filterTimer === null) {
          this.#filterDraft.set(filters);
        }
      });
    });

    // Focus survives a refetch: the same cell coordinates are restored once the
    // new rows have rendered. Without this, every page change would drop the
    // user at the top of the document.
    //
    // The re-focus is deferred by one turn deliberately. This effect runs
    // *before* the rows it reacted to are in the DOM, so focusing here would
    // land on the cell that is about to be destroyed - and destroying the
    // focused element is exactly what moves focus to <body>. Whether focus was
    // ours is therefore decided now, and acted on after the render.
    effect(() => {
      this.rows();
      const bounds = untracked(() => this.bounds());
      const cell = untracked(() => clampCell(this.#focused(), bounds));
      const hadFocus = this.#hasFocusInside();
      untracked(() => this.#focused.set(cell));
      if (hadFocus) {
        setTimeout(() => this.#applyFocus(cell), 0);
      }
    });
  }

  // --- Columns --------------------------------------------------------------

  readonly visibleColumns = computed(() => {
    const visibility = this.columnVisibility();
    return this.columns().filter((column) =>
      visibility
        ? (visibility[column.field] ?? column.defaultVisible !== false)
        : column.defaultVisible !== false,
    );
  });

  /** A leading select-all cell shifts every data column one to the right. */
  readonly columnOffset = computed(() => (this.selectionMode() === 'multiple' ? 1 : 0));
  readonly hasRowActions = computed(() => this.rowActions() !== undefined);
  readonly hasFilterRow = computed(() =>
    this.visibleColumns().some((column) => (column.filter ?? 'none') !== 'none'),
  );

  readonly bounds = computed<GridBounds>(() => ({
    rowCount: this.rows().length,
    colCount: this.visibleColumns().length + this.columnOffset(),
    viewportRows: this.pageJumpRows(),
  }));

  // --- Density and virtualisation ------------------------------------------

  readonly rowHeightPx = computed(() => DATA_GRID_ROW_HEIGHT_PX[this.density()]);
  /**
   * Virtualisation engages at 500 rows on the page (KB-051 Data display).
   * Measured on this stack: 10,000 rows hold 35 rendered `<tr>` at rest and 45
   * during a fling, at a 16.7 ms median frame - see KB-050 Performance targets.
   */
  readonly virtualised = computed(() => this.rows().length >= DATA_GRID_VIRTUAL_ROW_THRESHOLD);
  readonly scrollHeight = computed(() => (this.virtualised() ? this.height() : undefined));
  readonly skeletonRows = computed(() => Math.min(this.state().pageSize, 10));

  // --- Selection ------------------------------------------------------------

  readonly #selectedIds = computed(() => {
    const id = this.getRowId();
    return new Set(this.selection().map((row) => id(row)));
  });
  readonly allOnPageSelected = computed(
    () => this.rows().length > 0 && this.rows().every((row) => this.isSelected(row)),
  );
  readonly someOnPageSelected = computed(() => this.rows().some((row) => this.isSelected(row)));

  isSelected(row: TRow): boolean {
    return this.#selectedIds().has(this.getRowId()(row));
  }

  toggleRow(row: TRow): void {
    const mode = this.selectionMode();
    if (mode === 'none') {
      return;
    }
    const id = this.getRowId();
    if (mode === 'single') {
      this.selection.set(this.isSelected(row) ? [] : [row]);
      return;
    }
    const rowId = id(row);
    this.selection.set(
      this.isSelected(row)
        ? this.selection().filter((selected) => id(selected) !== rowId)
        : [...this.selection(), row],
    );
  }

  /**
   * The header checkbox covers **this page only** - the rows the user can
   * actually see. A control that quietly selected 12,000 unseen rows and then
   * deleted them is the accident this avoids.
   */
  onSelectAllToggle(checked: boolean): void {
    const id = this.getRowId();
    const pageIds = new Set(this.rows().map((row) => id(row)));
    if (checked) {
      const kept = this.selection().filter((row) => !pageIds.has(id(row)));
      this.selection.set([...kept, ...this.rows()]);
    } else {
      this.selection.set(this.selection().filter((row) => !pageIds.has(id(row))));
    }
  }

  // --- Query-state intents --------------------------------------------------

  sortDirection(field: string): DataGridSort | undefined {
    return this.state().sort.find((term) => term.field === field);
  }

  /** Ascending, descending, unsorted - the cycle a third click completes. */
  onSortToggle(field: string): void {
    const current = this.sortDirection(field);
    const sort: readonly DataGridSort[] = !current
      ? [{ field, direction: 'asc' }]
      : current.direction === 'asc'
        ? [{ field, direction: 'desc' }]
        : [];
    this.#emit({ sort, page: 1 });
  }

  onPageChange(page: number): void {
    this.#emit({ page });
  }

  onPageSizeChange(pageSize: number): void {
    this.#emit({ pageSize, page: 1 });
  }

  filterValue(column: DataGridColumn<TRow>): string {
    return this.#filterDraft()[columnFilterParam(column)] ?? '';
  }

  filterDate(column: DataGridColumn<TRow>): Date | null {
    const raw = this.filterValue(column);
    if (raw === '') {
      return null;
    }
    const parsed = new Date(raw);
    return Number.isNaN(parsed.getTime()) ? null : parsed;
  }

  /**
   * Debounced, and a filter change always returns to page 1: page 7 of the old
   * result set is a different, usually empty, page of the new one.
   */
  onFilterInput(column: DataGridColumn<TRow>, value: string | null): void {
    const param = columnFilterParam(column);
    const next = { ...this.#filterDraft() };
    if (value === null || value === '') {
      delete next[param];
    } else {
      next[param] = value;
    }
    this.#filterDraft.set(next);
    if (this.#filterTimer !== null) {
      clearTimeout(this.#filterTimer);
    }
    this.#filterTimer = setTimeout(() => {
      this.#filterTimer = null;
      this.#emit({ filters: this.#filterDraft(), page: 1 });
    }, this.filterDebounceMs());
  }

  /** `app-date-picker` holds a `Date`; the wire carries a date-only string. */
  onFilterDate(column: DataGridColumn<TRow>, value: Date | null): void {
    this.onFilterInput(column, value ? value.toISOString().slice(0, 10) : null);
  }

  #emit(patch: Partial<DataGridState>): void {
    this.stateChange.emit({ ...this.state(), ...patch });
  }

  // --- Keyboard -------------------------------------------------------------

  tabIndexFor(row: number, col: number): 0 | -1 {
    return rovingTabIndex(this.#focused(), { row, col }, this.bounds());
  }

  ariaRowIndexFor(row: number): number {
    const state = this.state();
    return ariaRowIndex(row, (state.page - 1) * state.pageSize);
  }

  onCellFocus(cell: GridCell): void {
    this.#focused.set(cell);
  }

  onKeydown(event: KeyboardEvent): void {
    if (handlesKey(event.key)) {
      const next = nextFocusedCell(this.#focused(), event, this.bounds());
      if (next) {
        event.preventDefault();
        this.#focused.set(next);
        this.#applyFocus(next);
      }
      return;
    }
    const row = this.#focusedRow();
    if (row === null) {
      return;
    }
    if (event.key === ' ' || event.key === 'Spacebar') {
      event.preventDefault();
      this.toggleRow(row);
      return;
    }
    if (event.key === 'Enter') {
      event.preventDefault();
      this.rowActivate.emit(row);
    }
  }

  #focusedRow(): TRow | null {
    const index = this.#focused().row;
    return index === HEADER_ROW_INDEX ? null : (this.rows()[index] ?? null);
  }

  #hasFocusInside(): boolean {
    const host = this.#host.nativeElement;
    return host.contains(host.ownerDocument.activeElement);
  }

  /**
   * Moves DOM focus to a cell, scrolling the virtual viewport first when the
   * target is outside the rendered window - a cell that is not in the DOM
   * cannot be focused, and `Ctrl+End` on a virtualised grid always targets one.
   */
  #applyFocus(cell: GridCell): void {
    const host = this.#host.nativeElement;
    const select = () =>
      host.querySelector<HTMLElement>(`[data-row="${cell.row}"][data-col="${cell.col}"]`);
    const immediate = select();
    if (immediate) {
      immediate.focus();
      return;
    }
    if (cell.row !== HEADER_ROW_INDEX) {
      const viewport = host.querySelector<HTMLElement>('.p-virtualscroller');
      if (viewport) {
        viewport.scrollTop = cell.row * this.rowHeightPx();
      }
    }
    // One turn of the event loop for the scroller to render the window.
    setTimeout(() => select()?.focus(), 0);
  }

  // --- Rendering helpers ----------------------------------------------------

  align(column: DataGridColumn<TRow>): string {
    return columnAlign(column);
  }

  width(column: DataGridColumn<TRow>): string | null {
    return this.#resizeWidths()[column.field] ?? column.width ?? null;
  }

  cell(column: DataGridColumn<TRow>, row: TRow): string {
    const value = cellValue(column, row);
    return value === null || value === undefined ? '' : String(value);
  }

  filterParam(column: DataGridColumn<TRow>): string {
    return columnFilterParam(column);
  }

  onResize({ field, deltaPx }: { field: string; deltaPx: number }): void {
    const host = this.#host.nativeElement;
    const cell = host.querySelector<HTMLElement>(`th[data-field="${field}"]`);
    const current = this.#resizeWidths()[field];
    const measured = cell?.offsetWidth;
    const base = current
      ? Number.parseFloat(current)
      : measured && measured > 0
        ? measured
        : DATA_GRID_DEFAULT_COLUMN_WIDTH_PX;
    const next = Math.min(
      DATA_GRID_MAX_COLUMN_WIDTH_PX,
      Math.max(DATA_GRID_MIN_COLUMN_WIDTH_PX, Math.round(base + deltaPx)),
    );
    this.#resizeWidths.set({ ...this.#resizeWidths(), [field]: `${next}px` });
  }

  onRowDblClick(row: TRow): void {
    this.rowActivate.emit(row);
  }

  /** What a screen reader is told after a page, sort or filter change. */
  readonly announcement = computed(() => {
    if (this.loading()) {
      return 'Loading rows';
    }
    const total = this.totalCount();
    if (total === 0) {
      return 'No rows match the current filters';
    }
    const state = this.state();
    const first = (state.page - 1) * state.pageSize + 1;
    const last = Math.min(state.page * state.pageSize, total);
    return `Showing rows ${first} to ${last} of ${total}`;
  });

  /** `p-table` types `value` as a mutable array; the grid's own input stays readonly. */
  readonly tableRows = computed(() => [...this.rows()]);

  readonly skeletonIndices = computed(() =>
    Array.from({ length: this.skeletonRows() }, (_, index) => index),
  );
}
