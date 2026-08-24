import { Directive, inject } from '@angular/core';
import { Tooltip } from 'primeng/tooltip';

/**
 * `[appTooltip]` - a short label for a control whose meaning is not obvious
 * from its icon alone.
 *
 * **It opens on focus as well as hover.** PrimeNG's `[pTooltip]` defaults to
 * `tooltipEvent="hover"`, which makes the tooltip invisible to anyone driving
 * the application from the keyboard. Rather than rely on every call site
 * remembering `tooltipEvent="both"`, this directive drives the underlying
 * `Tooltip` from the host's own `focus`/`blur`, and dismisses on `Esc`.
 *
 * **Never put information here that exists nowhere else.** A tooltip is
 * unreachable on touch, absent from print, and gone the moment focus moves.
 * If an operator needs the text to complete the task, it belongs in a hint on
 * the field (`app-form-field`'s `hint`) or in the body copy.
 */
@Directive({
  selector: '[appTooltip]',
  hostDirectives: [
    {
      directive: Tooltip,
      inputs: ['pTooltip: appTooltip', 'tooltipPosition: appTooltipPosition'],
    },
  ],
  host: {
    '(focus)': 'onFocus()',
    '(blur)': 'onBlur()',
    '(keydown.escape)': 'onEscape()',
  },
})
export class TooltipDirective {
  private readonly tooltip = inject(Tooltip, { self: true });

  onFocus(): void {
    this.tooltip.show();
  }

  onBlur(): void {
    this.tooltip.hide();
  }

  onEscape(): void {
    this.tooltip.hide();
  }
}
