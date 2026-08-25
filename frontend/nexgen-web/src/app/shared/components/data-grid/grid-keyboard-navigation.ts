/**
 * The ARIA `grid` keyboard model, as pure functions (M2-C05-01, implementation
 * requirement 12).
 *
 * It is written here rather than delegated because PrimeNG 22.1.0's own
 * `p-table` keyboard handling is a starting point, not this specification:
 * it covers selection and cell editing, not cell-by-cell traversal of a
 * `role="grid"`. Each key below was checked against the required list rather
 * than assumed to be covered.
 *
 * Pure on purpose. The component owns the DOM - which cell is rendered, and
 * where the virtual scroller has to be moved for an off-screen cell to exist -
 * and this module owns only *which* cell should be next. That split is what
 * makes the model testable without a browser.
 */

/** The header row participates in traversal and sits above body row 0. */
export const HEADER_ROW_INDEX = -1;

/** A cell coordinate. `row` is `HEADER_ROW_INDEX` for the header row. */
export interface GridCell {
  readonly row: number;
  readonly col: number;
}

export interface GridBounds {
  /** Body rows on the current page. */
  readonly rowCount: number;
  /** Visible columns. */
  readonly colCount: number;
  /** Rows a `PageUp`/`PageDown` moves by. */
  readonly viewportRows: number;
  /** Include the header row in traversal. Default `true`. */
  readonly includeHeader?: boolean;
}

/** The keys this module claims. Anything else is left to the browser. */
const HANDLED_KEYS: readonly string[] = [
  'ArrowUp',
  'ArrowDown',
  'ArrowLeft',
  'ArrowRight',
  'Home',
  'End',
  'PageUp',
  'PageDown',
];

/** `true` when {@link nextFocusedCell} will act on this key. */
export function handlesKey(key: string): boolean {
  return HANDLED_KEYS.includes(key);
}

/**
 * The cell focus moves to, or `null` when the key is not part of the model or
 * the move would leave the grid. Returning `null` matters: the caller must not
 * call `preventDefault` on a key it did not consume, or `Tab` stops working.
 *
 * `Ctrl`/`Cmd` + `Home`/`End` jump to the first/last cell of the whole grid,
 * plain `Home`/`End` to the first/last cell of the current row.
 */
export function nextFocusedCell(
  current: GridCell,
  event: Pick<KeyboardEvent, 'key' | 'ctrlKey' | 'metaKey'>,
  bounds: GridBounds,
): GridCell | null {
  if (!handlesKey(event.key) || bounds.colCount <= 0) {
    return null;
  }
  const firstRow = (bounds.includeHeader ?? true) ? HEADER_ROW_INDEX : 0;
  const lastRow = bounds.rowCount - 1;
  if (lastRow < firstRow) {
    return null;
  }
  const lastCol = bounds.colCount - 1;
  const jump = event.ctrlKey || event.metaKey;
  const page = Math.max(1, bounds.viewportRows);

  let { row, col } = current;
  switch (event.key) {
    case 'ArrowUp':
      row -= 1;
      break;
    case 'ArrowDown':
      row += 1;
      break;
    case 'ArrowLeft':
      col -= 1;
      break;
    case 'ArrowRight':
      col += 1;
      break;
    case 'Home':
      col = 0;
      if (jump) {
        row = firstRow;
      }
      break;
    case 'End':
      col = lastCol;
      if (jump) {
        row = lastRow;
      }
      break;
    case 'PageUp':
      row -= page;
      break;
    case 'PageDown':
      row += page;
      break;
    default:
      return null;
  }

  const next: GridCell = {
    row: clamp(row, firstRow, lastRow),
    col: clamp(col, 0, lastCol),
  };
  return next.row === current.row && next.col === current.col ? null : next;
}

/**
 * The whole grid is one tab stop: exactly one cell carries `tabindex="0"` and
 * every other carries `tabindex="-1"`.
 *
 * When the focused cell no longer exists - the page shrank, or a column was
 * hidden - the tab stop falls back to the first cell rather than disappearing,
 * which would drop the grid out of the tab order entirely.
 */
export function rovingTabIndex(focused: GridCell, cell: GridCell, bounds: GridBounds): 0 | -1 {
  const resolved = clampCell(focused, bounds);
  return resolved.row === cell.row && resolved.col === cell.col ? 0 : -1;
}

/** Pulls a cell coordinate back inside the current bounds. */
export function clampCell(cell: GridCell, bounds: GridBounds): GridCell {
  const firstRow = (bounds.includeHeader ?? true) ? HEADER_ROW_INDEX : 0;
  const lastRow = Math.max(firstRow, bounds.rowCount - 1);
  return {
    row: clamp(cell.row, firstRow, lastRow),
    col: clamp(cell.col, 0, Math.max(0, bounds.colCount - 1)),
  };
}

/**
 * `aria-rowindex` is 1-based and counts the header row as row 1, so body row 0
 * is row 2. Screen readers read it against `aria-rowcount`, which is the
 * **server** total - so the numbers a user hears are absolute positions in the
 * result set, not positions on the page.
 */
export function ariaRowIndex(row: number, pageOffset: number): number {
  return row === HEADER_ROW_INDEX ? 1 : pageOffset + row + 2;
}

function clamp(value: number, min: number, max: number): number {
  return Math.min(Math.max(value, min), max);
}
