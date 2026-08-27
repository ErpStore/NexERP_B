import { Directive, output } from '@angular/core';

import { BaseFormControl } from '../../form/base-control';

/**
 * What every `LineItemGrid` cell editor shares, on top of
 * `BaseFormControl` (M2-C04-02): a `committed` event, fired once a value is
 * *done changing* - on blur, not on every keystroke.
 *
 * The distinction matters because `rowEvent` (see `line-item-grid.model.ts`)
 * fires on **commit**, not on `valueChanges`: an item cascade or a balance
 * recalculation is a real server call, and firing one per keystroke while an
 * operator is still typing a quantity would be both wrong (mid-edit values
 * are not real values yet) and a self-inflicted denial of service on the
 * API `LineItemGrid` talks to.
 *
 * `writeValue`/`registerOnChange` (inherited) still update on every
 * keystroke, because the `FormArray` itself - and any *shape-only*
 * validator on the field - must reflect what is on screen immediately, per
 * `LineItemColumn`'s own doc comment: "the field is shape-only... no domain
 * event fires for it" is the case with no `committed` consumer at all, and
 * that path still needs live `FormControl` state.
 */
@Directive()
export abstract class LineItemCellBase<TValue> extends BaseFormControl<TValue> {
  /** Fired on blur (or an editor-specific equivalent "done" gesture) with the final value. */
  readonly committed = output<TValue | null>();

  protected commit(): void {
    this.markTouched();
    this.committed.emit(this.value());
  }
}
