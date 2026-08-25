import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

import {
  DATA_GRID_DEFAULT_COLUMN_WIDTH_PX,
  DATA_GRID_MAX_COLUMN_WIDTH_PX,
  DATA_GRID_MIN_COLUMN_WIDTH_PX,
  columnAlign,
  type DataGridColumn,
  type DataGridSort,
  type DataGridSortDirection,
} from './data-grid.model';
import {
  HEADER_ROW_INDEX,
  rovingTabIndex,
  type GridBounds,
  type GridCell,
} from './grid-keyboard-navigation';

/**
 * The column-header row of `DataGrid` (M2-C05-01).
 *
 * It is a component on a `<tr>` rather than a wrapper element because a
 * `<thead>` may contain only `<tr>`: an `<app-data-grid-header>` element
 * between them is invalid table markup, and browsers hoist it out of the
 * table, which breaks both the layout and the accessibility tree.
 *
 * Sorting is a **request**, never a local reorder - this row emits an intent
 * and the parent's query state turns it into one HTTP call. `aria-sort` is
 * therefore always describing what the server returned.
 */
@Component({
  // A <thead> may contain only <tr>. An <app-data-grid-header> element between
  // them is invalid table markup that the browser hoists out of the table,
  // breaking both the layout and the accessibility tree. An attribute selector
  // on the <tr> is the only way to keep this a component, so the prefix rule is
  // waived here and nowhere else.
  // eslint-disable-next-line @angular-eslint/component-selector
  selector: 'tr[appDataGridHeader]',
  templateUrl: './data-grid-header.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: { class: 'app-data-grid__header-row' },
})
export class DataGridHeaderComponent<TRow> {
  readonly minWidthPx = DATA_GRID_MIN_COLUMN_WIDTH_PX;
  readonly maxWidthPx = DATA_GRID_MAX_COLUMN_WIDTH_PX;

  readonly columns = input.required<readonly DataGridColumn<TRow>[]>();
  readonly sort = input<readonly DataGridSort[]>([]);
  readonly focused = input.required<GridCell>();
  readonly bounds = input.required<GridBounds>();
  /** Renders the select-all cell. Only `'multiple'` has one. */
  readonly showSelectAll = input(false);
  readonly allOnPageSelected = input(false);
  readonly someOnPageSelected = input(false);
  readonly resizable = input(true);
  readonly columnWidths = input<Readonly<Record<string, string>>>({});

  readonly sortToggle = output<string>();
  readonly selectAllToggle = output<boolean>();
  /** A resize intent in pixels, positive to widen. Not `resize` - that is a DOM event name. */
  readonly columnResize = output<{ field: string; deltaPx: number }>();
  readonly cellFocus = output<GridCell>();

  readonly headerRow = HEADER_ROW_INDEX;

  /** Data columns start at 1 when a select-all cell occupies column 0. */
  readonly columnOffset = computed(() => (this.showSelectAll() ? 1 : 0));

  align(column: DataGridColumn<TRow>): string {
    return columnAlign(column);
  }

  width(column: DataGridColumn<TRow>): string | null {
    return this.columnWidths()[column.field] ?? column.width ?? null;
  }

  /**
   * A focusable `role="separator"` is a widget, and WAI-ARIA requires the
   * range attributes on one. They are also the only way a screen-reader user
   * learns the column got wider when they pressed the arrow key.
   */
  resizeWidth(column: DataGridColumn<TRow>): number {
    const declared = this.width(column);
    const parsed = declared === null ? Number.NaN : Number.parseFloat(declared);
    return Number.isFinite(parsed) ? Math.round(parsed) : DATA_GRID_DEFAULT_COLUMN_WIDTH_PX;
  }

  isSortable(column: DataGridColumn<TRow>): boolean {
    return column.sortable !== false;
  }

  direction(column: DataGridColumn<TRow>): DataGridSortDirection | null {
    return this.sort().find((term) => term.field === column.field)?.direction ?? null;
  }

  /**
   * `aria-sort` belongs on the header cell and takes `'none'` when the column
   * is sortable but not currently sorted; a non-sortable column omits it
   * entirely rather than claiming to be sortable-but-unsorted.
   */
  ariaSort(column: DataGridColumn<TRow>): 'ascending' | 'descending' | 'none' | null {
    if (!this.isSortable(column)) {
      return null;
    }
    const direction = this.direction(column);
    if (direction === null) {
      return 'none';
    }
    return direction === 'asc' ? 'ascending' : 'descending';
  }

  tabIndexFor(col: number): 0 | -1 {
    return rovingTabIndex(this.focused(), { row: HEADER_ROW_INDEX, col }, this.bounds());
  }

  onCellFocus(col: number): void {
    this.cellFocus.emit({ row: HEADER_ROW_INDEX, col });
  }

  onHeaderActivate(column: DataGridColumn<TRow>, event: Event): void {
    if (!this.isSortable(column)) {
      return;
    }
    event.preventDefault();
    this.sortToggle.emit(column.field);
  }

  onSelectAll(event: Event): void {
    this.selectAllToggle.emit((event.target as HTMLInputElement).checked);
  }

  /**
   * Keyboard column resizing. `ArrowLeft`/`ArrowRight` on the focused handle
   * move the boundary in 16 px steps; the event is stopped so the grid's own
   * navigation does not also consume it and move the focused cell.
   */
  onResizeKeydown(column: DataGridColumn<TRow>, event: KeyboardEvent): void {
    if (event.key !== 'ArrowLeft' && event.key !== 'ArrowRight') {
      return;
    }
    event.preventDefault();
    event.stopPropagation();
    this.columnResize.emit({
      field: column.field,
      deltaPx: event.key === 'ArrowRight' ? 16 : -16,
    });
  }

  onResizePointerDown(column: DataGridColumn<TRow>, event: PointerEvent): void {
    event.preventDefault();
    event.stopPropagation();
    const startX = event.clientX;
    const handle = event.target as HTMLElement;
    const move = (moveEvent: PointerEvent) => {
      this.columnResize.emit({ field: column.field, deltaPx: moveEvent.clientX - startX });
    };
    const up = () => {
      handle.ownerDocument.removeEventListener('pointermove', move);
      handle.ownerDocument.removeEventListener('pointerup', up);
    };
    handle.ownerDocument.addEventListener('pointermove', move);
    handle.ownerDocument.addEventListener('pointerup', up);
  }
}
