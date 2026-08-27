import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { NgTemplateOutlet } from '@angular/common';
import type { TemplateRef } from '@angular/core';

/**
 * The sticky footer row - a **slot**, never a calculator. See
 * `line-item-grid.model.ts` and the task's *Business Rules*: `LineItemGrid`
 * sums nothing. If the caller renders a provisional client-side echo here,
 * that is the caller's own choice, and the caller's own template already
 * carries whatever "provisional" labelling it wants - this component adds
 * none, so it cannot silently make an echo look authoritative.
 */
@Component({
  selector: 'app-line-item-footer',
  template: `
    <div class="app-line-item-footer" [class.app-line-item-footer--empty]="!template()">
      @if (template(); as tpl) {
        <ng-container [ngTemplateOutlet]="tpl" />
      }
      @if (invalidRowCount() > 0) {
        <span class="app-line-item-footer__error-count" role="status">
          {{ invalidRowCount() }} row{{ invalidRowCount() === 1 ? '' : 's' }} need attention
        </span>
      }
    </div>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [NgTemplateOutlet],
})
export class LineItemFooterComponent {
  readonly template = input<TemplateRef<unknown> | undefined>(undefined);
  readonly invalidRowCount = input(0);

  /** Exposed for the "jump to next invalid row" summary count (acceptance criteria, test 16). */
  readonly hasErrors = computed(() => this.invalidRowCount() > 0);
}
