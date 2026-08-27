import { ChangeDetectionStrategy, Component, forwardRef } from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';

import { LineItemCellBase } from './base-cell';

/**
 * A never-editable cell - a server-computed running balance, a resolved
 * item code. Renders no focusable control at all: native `Tab` order then
 * skips it for free, which is the whole reason `Tab`/`Shift+Tab` need no
 * manual interception elsewhere in this component (see
 * `line-grid-keyboard.ts`'s note on `nextEditableCell`).
 */
@Component({
  selector: 'app-line-item-read-only-cell',
  template: `<span class="app-line-item-cell--readonly">{{ display() }}</span>`,
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => ReadOnlyCellComponent),
      multi: true,
    },
  ],
})
export class ReadOnlyCellComponent extends LineItemCellBase<string> {
  display(): string {
    return this.value() ?? '';
  }
}
