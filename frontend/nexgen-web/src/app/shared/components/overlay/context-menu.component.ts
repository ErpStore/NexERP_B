import { ChangeDetectionStrategy, Component, input, output, viewChild } from '@angular/core';
import type { MenuItem } from 'primeng/api';
import { ContextMenu } from 'primeng/contextmenu';

import { OverlayFocusKeeper } from './overlay-focus';

/**
 * A row/record action menu over `p-contextmenu`.
 *
 * **It ships with a visible trigger button, and that is the point.** A menu
 * reachable only by right-click is inoperable from the keyboard and invisible
 * on touch, so the trigger is rendered by default (`showTrigger`) and
 * `Shift+F10` on the target opens the same menu. Arrow keys move within it and
 * `Esc` closes it and returns focus to the trigger.
 */
@Component({
  selector: 'app-context-menu',
  templateUrl: './context-menu.component.html',
  styleUrl: './context-menu.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ContextMenu],
})
export class ContextMenuComponent {
  readonly items = input.required<MenuItem[]>();
  /** The trigger's accessible name - "Actions for SO-0001", not "Actions". */
  readonly label = input('Actions');
  /**
   * Hide the button only when the same actions are already reachable by
   * keyboard somewhere else on the row.
   */
  readonly showTrigger = input(true);
  readonly opened = output<void>();
  readonly closed = output<void>();

  /**
   * PrimeNG 22.1.0 puts `aria-level`, `aria-setsize` and `aria-posinset` on
   * every `role="menuitem"` - attributes ARIA allows on a treeitem, not a
   * menuitem, which axe reports as a critical `aria-allowed-attr` violation.
   * Cleared through the pass-through rather than by forking the component.
   */
  readonly menuPassThrough = {
    item: { 'aria-level': null, 'aria-setsize': null, 'aria-posinset': null },
  };

  private readonly menu = viewChild.required(ContextMenu);
  private readonly focus = new OverlayFocusKeeper();

  open(event: Event): void {
    event.preventDefault();
    this.focus.capture();
    this.menu().show(event);
  }

  /** `Shift+F10` is the platform convention for "open the context menu". */
  onKeydown(event: KeyboardEvent): void {
    if (event.key === 'F10' && event.shiftKey) {
      this.open(event);
    }
  }

  onShow(): void {
    this.opened.emit();
  }

  onHide(): void {
    this.focus.restore();
    this.closed.emit();
  }
}
