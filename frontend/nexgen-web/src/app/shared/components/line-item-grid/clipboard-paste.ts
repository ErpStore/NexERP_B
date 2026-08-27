import { parseUserInput, parseUserInputAsQty } from '../../utils/decimal';
import type { LineItemColumn } from './line-item-grid.model';

/**
 * Parses a clipboard paste into a preview of what would be applied - the
 * bulk-entry path *Investigation Requirements* names as "a genuine ERP need
 * for 200-line entry". Pure: no `FormArray`, no DOM, no side effect. The
 * component applies the preview only after the operator confirms it.
 *
 * The wire format is the one every spreadsheet (Excel, Sheets, the source
 * ERP's own grids) produces on copy: tab-separated cells, newline-separated
 * rows. Nothing here reads `navigator.clipboard` itself - the component
 * reads the `ClipboardEvent`'s `dataTransfer` and hands this module the raw
 * text, so the parsing stays testable without a real clipboard.
 */

export interface ClipboardPasteCell {
  readonly field: string;
  readonly raw: string;
  /** `undefined` for a column with no decimal-safe parse to attempt (text, select, ...). */
  readonly parsed?: { readonly ok: true } | { readonly ok: false; readonly reason: string };
}

export interface ClipboardPasteRowPreview {
  /** Offset from the paste's anchor row - `0` is the row the paste started on. */
  readonly rowOffset: number;
  readonly cells: readonly ClipboardPasteCell[];
}

export interface ClipboardPastePreview<TLine> {
  readonly rows: readonly ClipboardPasteRowPreview[];
  readonly targetColumns: readonly LineItemColumn<TLine>[];
  /** `true` when every decimal-safe cell parsed - the confirm action is disabled otherwise. */
  readonly allValid: boolean;
}

/** Splits pasted text into rows and cells. Trailing empty trailing row (a copy's final newline) is dropped. */
export function parseClipboardGrid(text: string): readonly (readonly string[])[] {
  const rows = text.replace(/\r\n/g, '\n').replace(/\r/g, '\n').split('\n');
  while (rows.length > 0 && rows[rows.length - 1] === '') {
    rows.pop();
  }
  return rows.map((row) => row.split('\t'));
}

/**
 * Builds the preview: pastes land starting at `anchorColumnIndex` in the
 * visible column list and extend as far right and down as the pasted data
 * goes, clamped to the columns actually available. A column past the end of
 * `columns` is silently dropped - pasting a 6-column block into a 4-column
 * grid fills the 4 it has, rather than erroring.
 */
export function buildClipboardPastePreview<TLine>(
  text: string,
  columns: readonly LineItemColumn<TLine>[],
  anchorColumnIndex: number,
): ClipboardPastePreview<TLine> {
  const grid = parseClipboardGrid(text);
  const usableColumns = columns.slice(anchorColumnIndex);
  let allValid = true;

  const rows: ClipboardPasteRowPreview[] = grid.map((rawCells, rowOffset) => {
    const cells: ClipboardPasteCell[] = rawCells.slice(0, usableColumns.length).map((raw, i) => {
      const column = usableColumns[i]!;
      const parsed = parseCellForColumn(raw, column);
      if (parsed && !parsed.ok) {
        allValid = false;
      }
      return { field: column.field, raw, parsed };
    });
    return { rowOffset, cells };
  });

  return { rows, targetColumns: usableColumns.slice(0, grid[0]?.length ?? 0), allValid };
}

function parseCellForColumn<TLine>(
  raw: string,
  column: LineItemColumn<TLine>,
): ClipboardPasteCell['parsed'] {
  if (column.editor === 'decimal') {
    const result = parseUserInput(raw, { places: column.decimalPlaces });
    return result.kind === 'value' || result.kind === 'empty'
      ? { ok: true }
      : { ok: false, reason: result.kind === 'error' ? result.reason : 'incomplete entry' };
  }
  if (column.editor === 'integer') {
    const result = parseUserInputAsQty(raw, { places: column.decimalPlaces });
    return result.kind === 'value' || result.kind === 'empty'
      ? { ok: true }
      : { ok: false, reason: result.kind === 'error' ? result.reason : 'incomplete entry' };
  }
  // Text/select/date/typeahead/checkbox/readonly: no decimal parse to fail on.
  return undefined;
}
