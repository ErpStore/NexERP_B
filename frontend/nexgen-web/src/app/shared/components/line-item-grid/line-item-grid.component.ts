import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  afterNextRender,
  computed,
  contentChild,
  inject,
  input,
  output,
  signal,
  type TemplateRef,
} from '@angular/core';
import { TableModule } from 'primeng/table';

import {
  handlesKey,
  nextFocusedCell,
  type GridBounds,
  type GridCell,
} from '../data-grid/grid-keyboard-navigation';
import { ModalComponent } from '../overlay/modal.component';
import { ConfirmDialogService } from '../overlay/confirm-dialog.service';
import { buildClipboardPastePreview, type ClipboardPastePreview } from './clipboard-paste';
import { decideLineGridKey, isLineGridShortcut } from './line-grid-keyboard';
import {
  LineItemForm,
  type LineItemFormArrayModel,
  type LineItemRowFactory,
} from './line-item-form';
import {
  LINE_ITEM_GRID_READONLY_BREAKPOINT_PX,
  type LineItemColumn,
  type LineItemRowError,
  type LineItemRowEvent,
  type LineItemRowId,
} from './line-item-grid.model';
import { LineItemFooterComponent } from './line-item-footer.component';
import { LineItemRowComponent, type LineItemCellCommit } from './line-item-row.component';

/**
 * The editable, virtualised, keyboard-first line-item grid every document
 * editor composes (M2-C07). Specification: `docs/kb/execution/tasks/M2-C07.md`.
 *
 * **It is an editing surface. It computes nothing.** Every domain
 * consequence of a change - what an item resolves to, what a quantity is
 * clamped against, whether a row is valid beyond shape - travels out over
 * `rowEvent` and comes back as a `respond(patch)` call; see
 * `line-item-grid.model.ts` for the exact contract, and this task's own
 * *Business Rules* section for what is forbidden here with citations.
 */
@Component({
  selector: 'app-line-item-grid',
  templateUrl: './line-item-grid.component.html',
  styleUrl: './line-item-grid.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [TableModule, ModalComponent, LineItemRowComponent, LineItemFooterComponent],
})
export class LineItemGridComponent<TLine> {
  readonly #host = inject<ElementRef<HTMLElement>>(ElementRef);
  readonly #confirmDialog = inject(ConfirmDialogService);

  readonly columns = input.required<readonly LineItemColumn<TLine>[]>();
  readonly lines = input.required<LineItemFormArrayModel<TLine>>();
  readonly createRow = input.required<LineItemRowFactory<TLine>>();
  /** Keyed by the `rowId` the caller was handed on `rowEvent` - see this file's README for the correlation. */
  readonly rowErrors = input<readonly LineItemRowError[]>([]);
  readonly readOnly = input(false);
  readonly ariaLabel = input('Line items');

  readonly rowEvent = output<LineItemRowEvent<TLine>>();
  /**
   * "Open whatever picker makes sense for this document" - the grid names
   * no picker and imports none (M2-C06 is a soft dependency for exactly
   * this reason). Once the caller has a selection, it hands the rows back
   * through {@link pullLines}, a public method rather than a second input:
   * a picker result is a one-off event, not a value this component should
   * hold state for between renders.
   */
  readonly pullFromUpstream = output<void>();

  readonly footerTemplate = contentChild<TemplateRef<unknown>>('footer');

  readonly #focused = signal<GridCell>({ row: 0, col: 0 });
  readonly #busyRows = signal<ReadonlySet<LineItemRowId>>(new Set());
  readonly #viewportWidth = signal(
    typeof window === 'undefined' ? Number.POSITIVE_INFINITY : window.innerWidth,
  );
  readonly #pastePreview = signal<ClipboardPastePreview<TLine> | null>(null);
  readonly #pasteAnchor = signal<{ readonly row: number; readonly col: number } | null>(null);

  readonly focusedCell = this.#focused.asReadonly();

  #form: LineItemForm<TLine> | null = null;

  constructor() {
    if (typeof window !== 'undefined') {
      const onResize = () => this.#viewportWidth.set(window.innerWidth);
      window.addEventListener('resize', onResize, { passive: true });
      inject(DestroyRef).onDestroy(() => window.removeEventListener('resize', onResize));
    }
    // Empty state (Target Result): zero lines renders one empty editable
    // row, never a bare "no data". One `append` on first render, not a
    // template-level fallback, so the row genuinely exists in the caller's
    // `FormArray` and can be typed into immediately.
    afterNextRender(() => {
      if (this.rows().length === 0 && !this.effectiveReadOnly()) {
        this.form().append({});
        this.#bumpVersion();
      }
    });
  }

  /** Below the breakpoint, editing is not offered - KB-051 Responsive behaviour. */
  readonly belowMobileBreakpoint = computed(
    () => this.#viewportWidth() < LINE_ITEM_GRID_READONLY_BREAKPOINT_PX,
  );
  readonly effectiveReadOnly = computed(() => this.readOnly() || this.belowMobileBreakpoint());

  /**
   * Constructed **once**, from the first read - not a `computed()`, and
   * deliberately so. `LineItemForm` carries a per-instance id sequence
   * (`line-item-form.ts`); reconstructing it on every change-detection pass
   * (which a `computed()` would do if the caller ever passes an inline
   * arrow function to `[createRow]`) would reset that counter and risk a
   * fresh row colliding with an existing row's id. The `FormArray` itself
   * is still read live through `this.lines()` inside the methods that need
   * it - only the wrapper object is cached.
   */
  form(): LineItemForm<TLine> {
    this.#form ??= new LineItemForm(this.lines(), this.createRow());
    return this.#form;
  }

  /**
   * Classic Reactive Forms are not signal-backed: pushing or removing a
   * control on a `FormArray` notifies no signal on its own. This is bumped
   * by every method below that changes which rows exist or their order, so
   * `rows`/`rowIds`/`bounds` - which read it before reading the array -
   * recompute exactly when a structural change actually happened, and never
   * on an ordinary value edit (those already reach the DOM through the
   * `[formControl]` binding itself).
   */
  readonly #version = signal(0);
  #bumpVersion(): void {
    this.#version.update((v) => v + 1);
  }

  readonly rows = computed(() => {
    this.#version();
    return this.form().rows();
  });
  readonly rowIds = computed(() => {
    this.#version();
    return this.form().rowIds();
  });

  /** `p-table` types `value` as a mutable array; the grid's own `rows()` stays readonly, same trade-off `DataGridComponent.tableRows` already makes. */
  readonly tableRows = computed(() => [...this.rows()]);

  readonly bounds = computed<GridBounds>(() => ({
    rowCount: this.rows().length,
    colCount: this.columns().length,
    viewportRows: 10,
    includeHeader: false,
  }));

  readonly errorsByRowId = computed(() => {
    const map = new Map<LineItemRowId, readonly string[]>();
    for (const entry of this.rowErrors()) {
      map.set(entry.rowId, entry.messages);
    }
    return map;
  });

  readonly invalidRowCount = computed(
    () => this.rowErrors().filter((e) => e.messages.length > 0).length,
  );

  /**
   * `rowIds()[index]` is `LineItemRowId | undefined` under `strict` -
   * `p-table`'s `#body` template only ever hands back an `rowIndex` for a
   * row that genuinely exists, so this is a real invariant, not a guess;
   * asserting it here keeps every template call site free of an `!` of its
   * own.
   */
  rowIdAt(index: number): LineItemRowId {
    return this.rowIds()[index]!;
  }

  errorsFor(rowId: LineItemRowId): readonly string[] {
    return this.errorsByRowId().get(rowId) ?? [];
  }

  isBusy(rowId: LineItemRowId): boolean {
    return this.#busyRows().has(rowId);
  }

  onCellFocus(row: number, col: number): void {
    this.#focused.set({ row, col });
  }

  // --- Keyboard ---------------------------------------------------------

  onKeydown(event: KeyboardEvent): void {
    if (this.effectiveReadOnly()) {
      return;
    }
    if (handlesKey(event.key) && !event.altKey) {
      const target = event.target as HTMLInputElement | null;
      if (this.#arrowShouldNavigate(event, target)) {
        const next = nextFocusedCell(this.#focused(), event, this.bounds());
        if (next) {
          event.preventDefault();
          this.#focused.set(next);
          this.#applyFocus(next);
        }
      }
      return;
    }
    if (!isLineGridShortcut(event)) {
      return;
    }
    const action = decideLineGridKey(event);
    const rowId = this.rowIds()[this.#focused().row];
    if (!rowId) {
      return;
    }
    switch (action.kind) {
      case 'commit-and-add-row':
        event.preventDefault();
        this.#onEnter(rowId);
        break;
      case 'revert-row':
        event.preventDefault();
        this.form().revert(rowId);
        break;
      case 'duplicate-row':
        event.preventDefault();
        this.#onDuplicate(rowId);
        break;
      case 'delete-row':
        event.preventDefault();
        void this.#onDelete(rowId);
        break;
      case 'move-row':
        event.preventDefault();
        this.#onMoveRow(rowId, action.delta);
        break;
      default:
        break;
    }
  }

  /**
   * Left/right navigate cells only once the caret has nothing left to move -
   * see `caretExhausted`'s own comment. Up/down always navigate: there is no
   * multi-line editor in this grid for a vertical caret to contest them.
   */
  #arrowShouldNavigate(event: KeyboardEvent, target: HTMLInputElement | null): boolean {
    if (event.key === 'ArrowUp' || event.key === 'ArrowDown') {
      return true;
    }
    if (event.key !== 'ArrowLeft' && event.key !== 'ArrowRight') {
      return true;
    }
    if (!target || typeof target.selectionStart !== 'number') {
      return true;
    }
    const start = target.selectionStart ?? 0;
    const end = target.selectionEnd ?? target.value.length;
    if (start !== end) {
      return false;
    }
    return event.key === 'ArrowLeft' ? start === 0 : end === target.value.length;
  }

  #onEnter(rowId: LineItemRowId): void {
    const group = this.form().groupOf(rowId);
    if (group && group.invalid) {
      // Mirrors `ValidateLastRowAsync` (`MfgPOUpsert.razor:2686`): the grid
      // will not add a row while the current one fails shape validation.
      group.markAllAsTouched();
      return;
    }
    const newId = this.form().append({});
    this.#bumpVersion();
    this.rowEvent.emit({ type: 'row-added', rowId: newId });
    const newIndex = this.form().indexOf(newId);
    const col = this.#firstEditableColumnIndex();
    const next = { row: newIndex, col };
    this.#focused.set(next);
    this.#applyFocus(next);
  }

  #onDuplicate(rowId: LineItemRowId): void {
    const newId = this.form().duplicate(rowId);
    if (!newId) {
      return;
    }
    this.#bumpVersion();
    this.rowEvent.emit({ type: 'row-duplicated', rowId: newId, sourceRowId: rowId });
    const next = { row: this.form().indexOf(newId), col: this.#focused().col };
    this.#focused.set(next);
    this.#applyFocus(next);
  }

  async #onDelete(rowId: LineItemRowId): Promise<void> {
    const group = this.form().groupOf(rowId);
    const holdsData = group
      ? Object.values(group.getRawValue() as Record<string, unknown>).some(
          (v) => v !== null && v !== undefined && v !== '',
        )
      : false;
    if (holdsData) {
      const result = await this.#confirmDialog.confirm({
        header: 'Delete line',
        message: 'This line has data. Delete it anyway?',
        destructive: true,
      });
      if (!result.confirmed) {
        return;
      }
    }
    if (this.form().remove(rowId)) {
      this.#bumpVersion();
      this.rowEvent.emit({ type: 'row-removed', rowId });
    }
  }

  #onMoveRow(rowId: LineItemRowId, delta: -1 | 1): void {
    const from = this.form().indexOf(rowId);
    const to = this.form().move(rowId, delta);
    if (to === from) {
      return;
    }
    this.#bumpVersion();
    this.rowEvent.emit({ type: 'rows-reordered', rowId, fromIndex: from, toIndex: to });
    const next = { row: to, col: this.#focused().col };
    this.#focused.set(next);
    this.#applyFocus(next);
  }

  /** The toolbar's **Add line** button - the pointer-friendly equivalent of `Enter` on the last row. */
  onAddLineClick(): void {
    const newId = this.form().append({});
    this.#bumpVersion();
    this.rowEvent.emit({ type: 'row-added', rowId: newId });
    const next = { row: this.form().indexOf(newId), col: this.#firstEditableColumnIndex() };
    this.#focused.set(next);
    this.#applyFocus(next);
  }

  /** The stacked mobile-list rendering (<768px, KB-051): a value straight from the row, formatting left to the caller's own column config elsewhere. */
  stackedValue(
    group: LineItemFormArrayModel<TLine>['controls'][number],
    column: LineItemColumn<TLine>,
  ): unknown {
    return (group.getRawValue() as Record<string, unknown>)[column.field];
  }

  #firstEditableColumnIndex(): number {
    const columns = this.columns();
    const index = columns.findIndex((c) => c.editor !== 'readonly');
    return index === -1 ? 0 : index;
  }

  // --- Cell commits -> rowEvent -------------------------------------------

  onCellCommitted(rowId: LineItemRowId, commit: LineItemCellCommit): void {
    const group = this.form().groupOf(rowId);
    const column = this.columns().find((c) => c.field === commit.field);
    if (!group || !column) {
      return;
    }
    const kind = column.onEditCommitted?.(group.getRawValue() as TLine, commit.value);
    if (!kind) {
      this.form().commit(rowId);
      return;
    }
    this.#setBusy(rowId, true);
    let responded = false;
    this.rowEvent.emit({
      type: kind,
      rowId,
      field: column.field,
      value: commit.value,
      respond: (patch) => {
        if (responded) {
          return;
        }
        responded = true;
        this.#setBusy(rowId, false);
        this.form().applyPatch(rowId, patch);
      },
    });
  }

  #setBusy(rowId: LineItemRowId, busy: boolean): void {
    const next = new Set(this.#busyRows());
    if (busy) {
      next.add(rowId);
    } else {
      next.delete(rowId);
    }
    this.#busyRows.set(next);
  }

  // --- Public API for the caller ------------------------------------------

  /**
   * Appends rows pulled from `RecordPickerDialog` (or any upstream source),
   * commits them as their own baseline, focuses the first one added, and
   * raises one `'lines-pulled'` notification - see `pullFromUpstream`'s own
   * doc comment for why this is a method the caller calls back into rather
   * than a second input.
   */
  pullLines(initialValues: readonly Partial<TLine>[]): void {
    if (initialValues.length === 0) {
      return;
    }
    const ids = initialValues.map((value) => this.form().append(value));
    this.#bumpVersion();
    this.rowEvent.emit({ type: 'lines-pulled', rowId: ids[ids.length - 1]!, rowIds: ids });
    const next = { row: this.form().indexOf(ids[0]!), col: this.#firstEditableColumnIndex() };
    this.#focused.set(next);
    this.#applyFocus(next);
  }

  // --- Clipboard paste -----------------------------------------------------

  readonly pastePreview = this.#pastePreview.asReadonly();

  onPaste(event: ClipboardEvent): void {
    if (this.effectiveReadOnly()) {
      return;
    }
    const text = event.clipboardData?.getData('text/plain');
    if (!text || (!text.includes('\t') && !text.includes('\n'))) {
      // A single-cell paste is left to the native input behaviour - the
      // preview dialog is for the genuinely multi-cell case.
      return;
    }
    event.preventDefault();
    const anchor = this.#focused();
    this.#pasteAnchor.set({ row: anchor.row, col: anchor.col });
    this.#pastePreview.set(buildClipboardPastePreview(text, this.columns(), anchor.col));
  }

  confirmPaste(): void {
    const preview = this.#pastePreview();
    const anchor = this.#pasteAnchor();
    if (!preview || !anchor || !preview.allValid) {
      return;
    }
    const form = this.form();
    let addedRows = false;
    for (const rowPreview of preview.rows) {
      const rowIndex = anchor.row + rowPreview.rowOffset;
      let rowId = form.rowIds()[rowIndex];
      if (!rowId) {
        rowId = form.append({});
        addedRows = true;
      }
      const patch: Record<string, unknown> = {};
      for (const cell of rowPreview.cells) {
        patch[cell.field] = cell.raw;
      }
      form.applyPatch(rowId, patch as Partial<TLine>);
    }
    if (addedRows) {
      this.#bumpVersion();
    }
    this.cancelPaste();
  }

  cancelPaste(): void {
    this.#pastePreview.set(null);
    this.#pasteAnchor.set(null);
  }

  // --- Focus -----------------------------------------------------------

  #applyFocus(cell: GridCell): void {
    const host = this.#host.nativeElement;
    const select = () =>
      host.querySelector<HTMLElement>(
        `[data-row="${cell.row}"][data-col="${cell.col}"] :is(input,textarea,select,button,[tabindex])`,
      );
    const immediate = select();
    if (immediate) {
      immediate.focus();
      return;
    }
    const viewport = host.querySelector<HTMLElement>('.p-virtualscroller');
    if (viewport) {
      viewport.scrollTop = cell.row * 36;
    }
    setTimeout(() => select()?.focus(), 0);
  }
}
