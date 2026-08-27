import { ChangeDetectionStrategy, Component, effect, forwardRef, input } from '@angular/core';
import { FormControl, NG_VALUE_ACCESSOR, ReactiveFormsModule } from '@angular/forms';

import { ComboboxComponent } from '../../form/combobox.component';
import type { ComboboxLoader, SelectOption } from '../../form/types';
import { LineItemCellBase } from './base-cell';

/**
 * Search-and-select against a server list - the item picker on a line, via
 * `app-combobox` (M2-C04-02). The caller's {@link ComboboxLoader} is the
 * only thing this cell knows about the server; it has never heard of
 * `HttpClient`, same as the control it wraps. Internal `FormControl`, not
 * `NgModel` - see `text-cell.component.ts`'s class comment.
 */
@Component({
  selector: 'app-line-item-typeahead-cell',
  template: `<app-combobox
    [formControl]="inner"
    [ariaLabel]="ariaLabel()"
    [readonly]="readonly() || isDisabled()"
    [loader]="loader()"
  />`,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ComboboxComponent, ReactiveFormsModule],
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => TypeaheadCellComponent),
      multi: true,
    },
  ],
})
export class TypeaheadCellComponent<TValue = unknown> extends LineItemCellBase<
  SelectOption<TValue>
> {
  readonly loader = input.required<ComboboxLoader<TValue>>();

  readonly inner = new FormControl<SelectOption<TValue> | null>(null);

  constructor() {
    super();
    effect(() => this.inner.setValue(this.value(), { emitEvent: false }));
    this.inner.valueChanges.subscribe((next) => {
      this.emitValue(next);
      this.commit();
    });
  }
}
