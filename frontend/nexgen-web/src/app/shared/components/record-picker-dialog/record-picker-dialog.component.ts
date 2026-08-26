import { NgTemplateOutlet } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  contentChild,
  effect,
  inject,
  input,
  model,
  output,
  signal,
  untracked,
  viewChild,
  type ElementRef,
  type TemplateRef,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';

import {
  DataGridComponent,
  createDataGridQueryState,
  toWireQuery,
  type DataGridColumn,
  type DataGridRowId,
  type DataGridRowIdFn,
} from '../data-grid';
import {
  EmptyStateComponent,
  ErrorStateComponent,
  PermissionDeniedStateComponent,
} from '../feedback';
import { TextInputComponent } from '../form';
import { ModalComponent, type ModalSize } from '../overlay';
import { RecordPickerFooterComponent } from './record-picker-footer.component';
import {
  RECORD_PICKER_DEFAULT_PAGE_SIZE,
  RECORD_PICKER_DEFAULT_SEARCH_PARAM,
  RECORD_PICKER_SEARCH_DEBOUNCE_MS,
  asProblem,
  isPermissionDenied,
  type RecordPickerCellState,
  type RecordPickerCellStateFn,
  type RecordPickerExport,
  type RecordPickerExportRequest,
  type RecordPickerFetchPage,
  type RecordPickerSelectionMode,
} from './record-picker-dialog.model';
import { RecordSelection } from './record-selection';

/**
 * `RecordPickerDialog` (M2-C06) - the replacement for
 * `V.SMART/V.SMART.Shared/Components/DetailsModal.razor`, the component through
 * which the whole ERP performs "pull lines from an upstream document".
 *
 * **Generic, and deliberately ignorant.** It searches, pages, selects, orders
 * and returns records. It knows nothing about sales orders, delivery challans,
 * items or quantities, and it decides nothing: which records are eligible,
 * which are already pulled, what a selection *means* - all of that is the
 * server's, reaching the dialog only as `fetchPage` rows and `disabledRowIds`
 * (ADR-004; KB-080 section 3).
 *
 * **Composed, not rebuilt.** The table is `DataGrid` (M2-C05-01); the frame is
 * `app-modal` (M2-C04-03), which is a PrimeNG `p-dialog` with `[modal]="true"`,
 * a focus trap and `Esc` handling already proven by its own specs. There is no
 * second table and no second component library here (ADR-007).
 *
 * Four behaviours of the old component are carried across on purpose:
 * selection order is the return order (`DetailsModal.razor:150-154`,
 * `:186-195`); "type anything, match anywhere" search (`:120-124`), now a
 * server query; select-all covers what the user can see (`:181-198`), now
 * "this page"; and export produces a server-generated file (`:241-244`), now a
 * blob, per ADR-005.
 *
 * Three are deliberately changed. `Esc` **closes** - `:10-11` sets
 * `data-bs-keyboard="false"`, and KB-051's accessibility commitments require a
 * modal to be escapable; a picker is a non-destructive selection dialog.
 * Confirm is **disabled** while nothing is selected - `:90` always enables it
 * and the only guard, at `:156-168`, is unreachable. And the button says
 * **Export**, not "Print", because it has never printed.
 *
 * The Blazor component itself is **not** modified: it keeps serving its 33 call
 * sites, defects and all, until each is migrated in its module wave.
 */
@Component({
  selector: 'app-record-picker-dialog',
  templateUrl: './record-picker-dialog.component.html',
  styleUrl: './record-picker-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    NgTemplateOutlet,
    FormsModule,
    ModalComponent,
    DataGridComponent,
    TextInputComponent,
    RecordPickerFooterComponent,
    EmptyStateComponent,
    ErrorStateComponent,
    PermissionDeniedStateComponent,
  ],
})
export class RecordPickerDialogComponent<TRow> {
  readonly #destroyRef = inject(DestroyRef);

  // --- Inputs ---------------------------------------------------------------

  /** Two-way. The calling screen owns open/closed state; there is no registry. */
  readonly visible = model(false);
  /** The dialog's accessible name. */
  readonly header = input.required<string>();
  readonly columns = input.required<readonly DataGridColumn<TRow>[]>();
  /**
   * One page of candidates. A **function**, not an endpoint: that is what lets
   * one component serve every call site without importing a generated
   * operation.
   */
  readonly fetchPage = input.required<RecordPickerFetchPage<TRow>>();
  /** Stable row identity. Selection is keyed by it, which is why it survives paging. */
  readonly getRowId = input.required<DataGridRowIdFn<TRow>>();
  readonly selectionMode = input<RecordPickerSelectionMode>('multiple');
  /** Ids to pre-select as their pages arrive. */
  readonly initialSelection = input<readonly DataGridRowId[]>([]);
  /**
   * Rows the caller has decided may not be picked - typically already pulled.
   * The dialog never derives this; it comes from server data.
   */
  readonly disabledRowIds = input<readonly DataGridRowId[]>([]);
  /** The accessible reason a disabled row cannot be picked. */
  readonly disabledReason = input('Already added to this document');
  /** Caller-supplied cell call-outs. Replaces hardcoded domain highlighting. */
  readonly getCellState = input<RecordPickerCellStateFn<TRow> | undefined>(undefined);
  readonly confirmLabel = input('Add selected');
  readonly cancelLabel = input('Close');
  /** Optional. Absent means no export control is rendered at all. */
  readonly exportRequest = input<RecordPickerExportRequest | undefined>(undefined);
  readonly exportFileName = input('export.xlsx');
  /**
   * The wire name of the free-text search parameter. See
   * `RECORD_PICKER_DEFAULT_SEARCH_PARAM` - M2-B02 defines none, so this is a
   * default rather than a contract.
   */
  readonly searchParam = input(RECORD_PICKER_DEFAULT_SEARCH_PARAM);
  readonly searchLabel = input('Search records');
  readonly searchPlaceholder = input('Type to search');
  readonly searchDebounceMs = input(RECORD_PICKER_SEARCH_DEBOUNCE_MS);
  readonly pageSize = input(RECORD_PICKER_DEFAULT_PAGE_SIZE);
  readonly size = input<ModalSize>('lg');
  /** "No pending quotations" - name the thing, per KB-051's state patterns. */
  readonly emptyTitle = input('No records available');

  // --- Outputs --------------------------------------------------------------

  /** The chosen rows, **in the order they were ticked**. */
  readonly confirmed = output<readonly TRow[]>();
  /** `Esc`, Close, or the dialog's own close control. */
  readonly cancelled = output<void>();

  /** Optional slot above the grid - a scope filter, or a legend for `getCellState`. */
  readonly headerTemplate = contentChild<TemplateRef<unknown>>('pickerHeader');

  readonly body = viewChild<ElementRef<HTMLElement>>('body');
  readonly searchBox = viewChild<ElementRef<HTMLElement>>('searchBox');

  // --- State ----------------------------------------------------------------

  /**
   * Insertion-ordered, keyed by `getRowId`, and held **above** the grid rather
   * than in the row objects - which is what makes it survive paging and
   * searching. Component-local by construction: it is an instance field, so it
   * is narrower than a provider at this component's injector and can never be
   * reached from root.
   */
  readonly selection = new RecordSelection<TRow>(() => this.getRowId());

  /**
   * The query state, in **detached** mode: the dialog must not write to the
   * page URL. The page behind it owns the URL, and a picker's search term is
   * transient. `autoLoad: false` holds the first request until the dialog is
   * actually opened - a closed picker must not query.
   */
  readonly grid = createDataGridQueryState<TRow>({
    source: (query) => this.fetchPage()(query),
    mode: 'detached',
    pageSize: RECORD_PICKER_DEFAULT_PAGE_SIZE,
    autoLoad: false,
  });

  readonly #searchDraft = signal('');
  readonly #exporting = signal(false);
  #searchTimer: ReturnType<typeof setTimeout> | null = null;
  /** Set while confirming, so closing the dialog does not also emit `cancelled`. */
  #confirming = false;

  readonly searchTerm = this.#searchDraft.asReadonly();
  readonly exporting = this.#exporting.asReadonly();

  constructor() {
    this.#destroyRef.onDestroy(() => this.#clearSearchTimer());

    // Opening is what issues the first request, and it always starts clean: a
    // new pick is a new question, so the previous search term and the previous
    // selection do not leak into it.
    effect(() => {
      if (this.visible()) {
        untracked(() => this.#onOpen());
      }
    });

    // Pre-selection can only be honoured once the rows an id refers to exist.
    effect(() => {
      const rows = this.grid.rows();
      const ids = new Set(this.initialSelection());
      untracked(() => this.selection.adopt(rows, ids));
    });

    // Disabled rows and cell call-outs are applied to the rendered grid after
    // it has rendered. See `#decorate` for why this is done here and not by
    // editing `DataGrid`.
    effect(() => {
      this.grid.rows();
      this.disabledRowIds();
      this.getCellState();
      this.columns();
      untracked(() => setTimeout(() => this.#decorate(), 0));
    });
  }

  // --- Derived --------------------------------------------------------------

  readonly disabledIds = computed(() => new Set(this.disabledRowIds()));

  /** Mirrors `DataGrid`'s own default-visibility filter, for cell decoration. */
  readonly visibleColumns = computed(() =>
    this.columns().filter((column) => column.defaultVisible !== false),
  );

  readonly selectedRows = computed(() => this.selection.selected());
  readonly selectedCount = computed(() => this.selection.count());

  readonly problem = computed(() => asProblem(this.grid.error()));
  readonly permissionDenied = computed(() => isPermissionDenied(this.grid.error()));
  readonly hasError = computed(() => this.grid.error() !== null);
  /** A failure with nothing on screen: the grid has nothing to show, so it is hidden. */
  readonly failedEmpty = computed(() => this.hasError() && this.grid.rows().length === 0);

  readonly errorMessage = computed(() => this.problem()?.title ?? 'The request failed.');
  readonly errorDetail = computed(() => this.problem()?.detail);
  readonly errorTraceId = computed(() => this.problem()?.traceId);
  readonly deniedScreen = computed(() => this.problem()?.screen ?? 'this list');
  readonly deniedRight = computed(() => this.problem()?.right ?? 'view');

  readonly isFiltered = computed(() => this.searchTerm().trim() !== '');
  readonly pageRowCount = computed(() => this.grid.rows().length);

  readonly selectablePageRows = computed(() => {
    const disabled = this.disabledIds();
    const idOf = this.getRowId();
    return this.grid.rows().filter((row) => !disabled.has(idOf(row)));
  });

  readonly allSelectableOnPageSelected = computed(() => {
    const rows = this.selectablePageRows();
    return rows.length > 0 && rows.every((row) => this.selection.isSelected(row));
  });

  /** "Select all 10 on this page" - the scope stated in words, not implied. */
  readonly selectAllLabel = computed(() =>
    this.allSelectableOnPageSelected()
      ? `Deselect all ${this.selectablePageRows().length} on this page`
      : `Select all ${this.selectablePageRows().length} on this page`,
  );

  readonly canExport = computed(() => this.exportRequest() !== undefined);

  // --- Open / close ---------------------------------------------------------

  #onOpen(): void {
    this.#confirming = false;
    this.#clearSearchTimer();
    this.#searchDraft.set('');
    this.selection.reset();
    // Exactly one request per open, with the caller's page size applied.
    this.grid.apply({ page: 1, pageSize: this.pageSize(), filters: {} });
  }

  /** `app-modal` has taken focus; move it to the search field. */
  onOpened(): void {
    setTimeout(() => this.searchBox()?.nativeElement.querySelector('input')?.focus(), 0);
  }

  /**
   * Fired for `Esc`, the close icon and the Close button alike. `Esc`
   * cancelling is a **deliberate divergence** from `DetailsModal.razor:10-11`.
   */
  onClosed(): void {
    if (!this.#confirming) {
      this.cancelled.emit();
    }
    this.#confirming = false;
  }

  close(): void {
    this.visible.set(false);
  }

  // --- Search ---------------------------------------------------------------

  /**
   * Free-text search, debounced, and **sent to the server**.
   * `DetailsModal.razor:120-124` filtered an in-memory list with a
   * case-insensitive `Contains` across every column, which cannot reach past
   * the page the user is on once paging is real. The user-visible contract -
   * type anything, match anywhere - is unchanged; where it runs is not.
   */
  onSearchInput(value: string | null): void {
    const term = value ?? '';
    this.#searchDraft.set(term);
    this.#clearSearchTimer();
    this.#searchTimer = setTimeout(() => {
      this.#searchTimer = null;
      this.#commitSearch(term);
    }, this.searchDebounceMs());
  }

  clearSearch(): void {
    this.#searchDraft.set('');
    this.#clearSearchTimer();
    this.#commitSearch('');
  }

  #commitSearch(term: string): void {
    const filters: Record<string, string> = {};
    if (term !== '') {
      filters[this.searchParam()] = term;
    }
    // A search change always returns to page 1: page 4 of the old result set is
    // a different, usually empty, page of the new one.
    this.grid.apply({ filters, page: 1 });
  }

  #clearSearchTimer(): void {
    if (this.#searchTimer !== null) {
      clearTimeout(this.#searchTimer);
      this.#searchTimer = null;
    }
  }

  retry(): void {
    this.grid.refresh();
  }

  // --- Selection ------------------------------------------------------------

  /**
   * The grid proposes; the picker disposes.
   *
   * `DataGrid` owns no selection order and knows nothing about disabled rows,
   * so its proposed array is reconciled here rather than adopted: rows the
   * caller disabled are refused, rows dropped from the current page are
   * removed, and everything else is appended - which leaves an already-selected
   * row at the position the user first gave it. That is what keeps a
   * select-all from silently reordering an operator's earlier choices.
   */
  onSelectionProposed(next: readonly TRow[]): void {
    const idOf = this.getRowId();
    const disabled = this.disabledIds();

    if (this.selectionMode() === 'single') {
      const row = next.find((candidate) => !disabled.has(idOf(candidate)));
      if (row) {
        this.selection.replaceWith(row);
      } else {
        this.selection.clear();
      }
      return;
    }

    const nextIds = new Set(next.map((row) => idOf(row)));
    for (const row of this.grid.rows()) {
      const id = idOf(row);
      if (!nextIds.has(id)) {
        this.selection.remove(id);
      }
    }
    for (const row of next) {
      if (!disabled.has(idOf(row))) {
        this.selection.add(row);
      }
    }
  }

  /** The explicit, visibly-labelled page select-all. */
  toggleSelectAllOnPage(): void {
    if (this.allSelectableOnPageSelected()) {
      this.selection.clearPage(this.grid.rows());
    } else {
      this.selection.selectPage(this.grid.rows(), this.disabledIds());
    }
  }

  /**
   * `Enter` or a double click. In single-select mode this is the whole
   * interaction - a master pick must not cost two keystrokes.
   */
  onRowActivate(row: TRow): void {
    if (this.disabledIds().has(this.getRowId()(row))) {
      return;
    }
    if (this.selectionMode() === 'single') {
      this.selection.replaceWith(row);
      this.confirm();
      return;
    }
    this.selection.toggle(row);
  }

  confirm(): void {
    if (this.selection.isEmpty()) {
      return;
    }
    this.#confirming = true;
    this.confirmed.emit(this.selection.selected());
    this.visible.set(false);
  }

  cancel(): void {
    this.close();
  }

  // --- Export ---------------------------------------------------------------

  /**
   * Delegates to a **server** endpoint and downloads what comes back. No
   * client-side file generation, and no file-generation library (ADR-005).
   *
   * TODO(M2-C05-03): route this through `GridExportService` once that task
   * lands. It does not exist at the time of writing, so the request function is
   * called directly and the contract - the server produces the file - is
   * identical either way.
   */
  onExport(): void {
    const request = this.exportRequest();
    if (!request || this.exporting()) {
      return;
    }
    this.#exporting.set(true);
    request(toWireQuery(this.grid.state()))
      .pipe(takeUntilDestroyed(this.#destroyRef))
      .subscribe({
        next: (result) => {
          this.#exporting.set(false);
          this.#download(result);
        },
        error: () => this.#exporting.set(false),
      });
  }

  #download(result: Blob | RecordPickerExport): void {
    const blob = result instanceof Blob ? result : result.blob;
    const fileName =
      result instanceof Blob ? this.exportFileName() : (result.fileName ?? this.exportFileName());
    // jsdom implements neither; the request still happened, which is what the
    // ADR-005 test asserts.
    if (typeof URL.createObjectURL !== 'function') {
      return;
    }
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = fileName;
    anchor.click();
    URL.revokeObjectURL(url);
  }

  // --- Rendered-row decoration ---------------------------------------------

  /**
   * Marks disabled rows and applies `getCellState`, on the grid's own DOM.
   *
   * **Why here.** `DataGrid` (M2-C05-01) has neither a `disabledRowIds` input
   * nor a cell-state hook, and this task must not edit it - "if it needs a new
   * capability, that is a change request against M2-C05-01, not an in-place
   * edit". So the two decorations are applied after render, from the picker's
   * own body element, and re-applied whenever the rows change. Correctness does
   * **not** depend on this: a disabled row is refused in
   * `onSelectionProposed` and excluded from select-all whatever the DOM says.
   * This is the part a screen reader and an eye need. Recorded as a change
   * request in the technical-debt register.
   */
  #decorate(): void {
    const root = this.body()?.nativeElement;
    if (!root) {
      return;
    }
    const rows = this.grid.rows();
    const disabled = this.disabledIds();
    const idOf = this.getRowId();
    const reason = this.disabledReason();
    const cellState = this.getCellState();
    const columns = this.visibleColumns();
    const offset = this.selectionMode() === 'multiple' ? 1 : 0;
    const rendered = root.querySelectorAll<HTMLElement>('tr.app-data-grid__row');

    rendered.forEach((tr, index) => {
      const row = rows[index];
      if (row === undefined) {
        return;
      }
      const isDisabled = disabled.has(idOf(row));
      tr.classList.toggle('app-record-picker__row--disabled', isDisabled);
      if (isDisabled) {
        tr.setAttribute('aria-disabled', 'true');
        tr.setAttribute('title', reason);
      } else {
        tr.removeAttribute('aria-disabled');
        tr.removeAttribute('title');
      }
      const box = tr.querySelector<HTMLInputElement>('input[type="checkbox"]');
      if (box) {
        box.disabled = isDisabled;
      }
      if (!cellState) {
        return;
      }
      columns.forEach((column, columnIndex) => {
        const cell = tr.querySelector<HTMLElement>(`[data-col="${columnIndex + offset}"]`);
        if (cell) {
          this.#applyCellState(cell, cellState(row, column.field));
        }
      });
    });
  }

  #applyCellState(cell: HTMLElement, state: RecordPickerCellState | null): void {
    const noteClass = 'app-record-picker__cell-note';
    let note = cell.querySelector<HTMLElement>(`.${noteClass}`);
    if (!state) {
      note?.remove();
      delete cell.dataset['cellState'];
      return;
    }
    cell.dataset['cellState'] = state.tone;
    // A tone never travels alone: the label is real text, so the meaning does
    // not depend on colour (KB-051 status vocabulary).
    if (!note) {
      note = document.createElement('span');
      note.className = noteClass;
      cell.appendChild(note);
    }
    note.textContent = ` ${state.label}`;
  }
}
