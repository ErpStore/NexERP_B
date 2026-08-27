import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';
import { ReactiveFormsModule, type FormControl } from '@angular/forms';

import { CheckboxCellComponent } from './cells/checkbox-cell.component';
import { DateCellComponent } from './cells/date-cell.component';
import { DecimalCellComponent } from './cells/decimal-cell.component';
import { IntegerCellComponent } from './cells/integer-cell.component';
import { ReadOnlyCellComponent } from './cells/read-only-cell.component';
import { SelectCellComponent } from './cells/select-cell.component';
import { TextCellComponent } from './cells/text-cell.component';
import { TypeaheadCellComponent } from './cells/typeahead-cell.component';
import type { LineItemFormGroup } from './line-item-form';
import type { LineItemColumn } from './line-item-grid.model';
import { RowErrorGutterComponent } from './row-error-gutter.component';

/** What a cell reports up when its value commits - the row does not decide what it means. */
export interface LineItemCellCommit {
  readonly field: string;
  readonly value: unknown;
}

/**
 * One row's rendering: a cell per visible column, switched on
 * `column.editor`, each bound to its own control in the row's `FormGroup`.
 *
 * **`OnPush` and nothing else.** No business logic, no server call, no
 * decision about validity beyond what `errors` already says. This is the
 * component `render-count.spec.ts`'s pattern exists to keep isolated: typing
 * in row 187 must re-evaluate this component's own view and no sibling's.
 */
@Component({
  // eslint-disable-next-line @angular-eslint/component-selector
  selector: 'tr[appLineItemRow]',
  templateUrl: './line-item-row.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    ReactiveFormsModule,
    RowErrorGutterComponent,
    TextCellComponent,
    DecimalCellComponent,
    IntegerCellComponent,
    DateCellComponent,
    SelectCellComponent,
    TypeaheadCellComponent,
    CheckboxCellComponent,
    ReadOnlyCellComponent,
  ],
})
export class LineItemRowComponent<TLine> {
  readonly group = input.required<LineItemFormGroup<TLine>>();
  readonly columns = input.required<readonly LineItemColumn<TLine>[]>();
  readonly rowId = input.required<string>();
  readonly rowIndex = input.required<number>();
  /** This row's own server round trip is in flight - every cell goes read-only, the rest of the grid does not. */
  readonly busy = input(false);
  readonly readOnly = input(false);
  readonly errors = input<readonly string[]>([]);
  /** Roving-tabindex coordinate of the currently focused cell, so this row knows if it owns it. */
  readonly focusedRow = input<number>(-1);
  readonly focusedCol = input(0);

  readonly cellCommitted = output<LineItemCellCommit>();
  readonly cellFocus = output<number>();

  readonly hasErrors = computed(() => this.errors().length > 0);
  readonly #isRowFocused = computed(() => this.rowIndex() === this.focusedRow());

  /**
   * Roving-tabindex fallback for a cell with no focusable control of its
   * own (`editor: 'readonly'`). Editable cells never need this - a real
   * `<input>` is already in native tab order - but a `readonly` column must
   * still be a legal `ArrowLeft/Right`/`Home`/`End` landing spot inside the
   * ARIA grid, or the ARIA `grid` model has a hole in it.
   */
  tabIndexFor(colIndex: number): 0 | -1 {
    return this.#isRowFocused() && colIndex === this.focusedCol() ? 0 : -1;
  }

  controlFor(column: LineItemColumn<TLine>): FormControl {
    // Dynamic, config-driven field access - the same trade-off
    // `DataGridColumn.field: string` already makes: a column model cannot be
    // both a static compile-time key and caller-configured data.
    return (this.group().controls as Record<string, FormControl>)[column.field]!;
  }

  isEditable(column: LineItemColumn<TLine>): boolean {
    if (this.readOnly() || this.busy()) {
      return false;
    }
    if (column.editor === 'readonly') {
      return false;
    }
    return column.editableWhen ? column.editableWhen(this.rowGroupValue()) : true;
  }

  onCommit(column: LineItemColumn<TLine>, value: unknown): void {
    this.cellCommitted.emit({ field: column.field, value });
  }

  private rowGroupValue(): TLine {
    return this.group().getRawValue() as TLine;
  }
}
