import {
  HEADER_ROW_INDEX,
  clampCell,
  type GridBounds,
  type GridCell,
} from '../data-grid/grid-keyboard-navigation';

/**
 * The editing half of `LineItemGrid`'s keyboard model (M2-C07). Reuses
 * M2-C05-01's structural primitives - {@link GridCell}, {@link clampCell} -
 * for the cases they already solve (a coordinate, its bounds, pulling it
 * back inside them) and adds only what editing needs on top: `Tab`
 * committing forward across *editable* cells, `Enter` ending a row, `Esc`
 * reverting one, and the row-lifecycle shortcuts DataGrid never had a reason
 * to have.
 *
 * Pure on purpose, same reasoning as `grid-keyboard-navigation.ts`: this
 * module decides *what* should happen; the component decides how to make it
 * true in the DOM (which control gets focus, which service call fires).
 */

/** Bounds plus per-cell editability - a column can be `editor: 'readonly'`, or a row can lock a cell via `editableWhen`. */
export interface LineGridBounds extends GridBounds {
  readonly isEditable: (row: number, col: number) => boolean;
}

export type LineGridAction =
  | { readonly kind: 'move-focus'; readonly cell: GridCell }
  | { readonly kind: 'commit-and-add-row' }
  | { readonly kind: 'revert-row' }
  | { readonly kind: 'duplicate-row' }
  | { readonly kind: 'delete-row' }
  | { readonly kind: 'move-row'; readonly delta: -1 | 1 }
  | { readonly kind: 'none' };

/**
 * The keys `LineItemGrid` claims for row-lifecycle shortcuts, over and above
 * what `handlesKey` (structural arrows) already claims. Checked before
 * `preventDefault` is called on any of them, for the same reason
 * `grid-keyboard-navigation.ts` checks first: a key this module does not
 * consume must be left for the browser, or focus and `Tab` order break.
 */
export function isLineGridShortcut(
  event: Pick<KeyboardEvent, 'key' | 'ctrlKey' | 'metaKey' | 'altKey'>,
): boolean {
  if (event.key === 'Enter' || event.key === 'Escape' || event.key === 'Delete') {
    return true;
  }
  if (event.key.toLowerCase() === 'd' && (event.ctrlKey || event.metaKey)) {
    return true;
  }
  return event.altKey && (event.key === 'ArrowUp' || event.key === 'ArrowDown');
}

/**
 * The first editable column at or after `from` in `row` - `Tab`'s landing
 * spot, and a new row's initial focus. `-1` when the row has no editable
 * column at all (a fully `readOnly` grid), which the caller must treat as
 * "stay put," not as a coordinate.
 */
export function firstEditableColumn(bounds: LineGridBounds, row: number, from = 0): number {
  for (let col = from; col < bounds.colCount; col++) {
    if (bounds.isEditable(row, col)) {
      return col;
    }
  }
  return -1;
}

/** {@link firstEditableColumn}, scanning backwards from `from` (inclusive). `Shift+Tab`'s landing spot. */
export function lastEditableColumn(
  bounds: LineGridBounds,
  row: number,
  from = bounds.colCount - 1,
): number {
  for (let col = Math.min(from, bounds.colCount - 1); col >= 0; col--) {
    if (bounds.isEditable(row, col)) {
      return col;
    }
  }
  return -1;
}

/**
 * The next editable cell in reading order, wrapping across row boundaries.
 * `Tab`/`Shift+Tab` themselves are **not** intercepted in the live component
 * - every editable cell renders a real focusable control and a `readonly`
 * one renders none, so native DOM tab order already does this for free, and
 * more robustly than a hand-rolled roving-tabindex reimplementation would.
 * This function exists for the moves the browser cannot do on its own:
 * `Enter`'s "focus the new row's first editable cell", and restoring focus
 * after `Ctrl+D` or `Alt+ArrowUp/Down` change which row a cell's coordinate
 * refers to.
 */
export function nextEditableCell(
  current: GridCell,
  bounds: LineGridBounds,
  backwards: boolean,
): GridCell | null {
  if (bounds.rowCount <= 0) {
    return null;
  }
  if (!backwards) {
    const sameRow = firstEditableColumn(bounds, current.row, current.col + 1);
    if (sameRow !== -1) {
      return { row: current.row, col: sameRow };
    }
    for (let row = current.row + 1; row < bounds.rowCount; row++) {
      const col = firstEditableColumn(bounds, row);
      if (col !== -1) {
        return { row, col };
      }
    }
    return null;
  }
  const sameRow = lastEditableColumn(bounds, current.row, current.col - 1);
  if (sameRow !== -1) {
    return { row: current.row, col: sameRow };
  }
  for (let row = current.row - 1; row >= 0; row--) {
    const col = lastEditableColumn(bounds, row);
    if (col !== -1) {
      return { row, col };
    }
  }
  return null;
}

/**
 * Decides what one keydown means, for every key this component claims via
 * {@link isLineGridShortcut}. `Tab`/`Shift+Tab` are deliberately not among
 * them - native DOM tab order already moves between editable cells (see
 * {@link nextEditableCell}'s own comment) - and structural arrows
 * (`ArrowUp/Down/Left/Right`, `Home`, `End`, `PageUp/Down`) are handled by
 * asking `grid-keyboard-navigation.ts`'s `nextFocusedCell` directly, only
 * when {@link caretExhausted} says the focused control has no more caret
 * left to move, so an arrow key edits a cell's text before it ever moves off
 * one.
 */
export function decideLineGridKey(
  event: Pick<KeyboardEvent, 'key' | 'ctrlKey' | 'metaKey' | 'altKey'>,
): LineGridAction {
  if (event.altKey && event.key === 'ArrowUp') {
    return { kind: 'move-row', delta: -1 };
  }
  if (event.altKey && event.key === 'ArrowDown') {
    return { kind: 'move-row', delta: 1 };
  }
  if (event.key.toLowerCase() === 'd' && (event.ctrlKey || event.metaKey)) {
    return { kind: 'duplicate-row' };
  }
  if (event.key === 'Delete') {
    return { kind: 'delete-row' };
  }
  if (event.key === 'Escape') {
    return { kind: 'revert-row' };
  }
  if (event.key === 'Enter') {
    return { kind: 'commit-and-add-row' };
  }
  return { kind: 'none' };
}

/**
 * `true` when a focused text-like control has no caret movement left to
 * give an arrow key - empty, or the caret sits at the edge the key points
 * toward - so the component may treat this `ArrowLeft/Right` (or the
 * vertical pair) as a cell-to-cell move instead of a text edit. The
 * standard technique that lets one arrow key both edit a cell's text and
 * navigate the grid without either fighting the other.
 */
export function caretExhausted(
  input: {
    readonly value: string;
    readonly selectionStart: number | null;
    readonly selectionEnd: number | null;
  } | null,
  direction: 'left' | 'right',
): boolean {
  if (!input) {
    return true;
  }
  const start = input.selectionStart ?? 0;
  const end = input.selectionEnd ?? input.value.length;
  if (start !== end) {
    return false;
  }
  return direction === 'left' ? start === 0 : end === input.value.length;
}

/** `HEADER_ROW_INDEX` re-exported so callers of this module need one import for both halves of the keyboard model. */
export { HEADER_ROW_INDEX, clampCell };
export type { GridBounds, GridCell };
