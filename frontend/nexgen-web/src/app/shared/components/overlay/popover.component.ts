import {
  ChangeDetectionStrategy,
  Component,
  input,
  output,
  viewChild,
  type ElementRef,
} from '@angular/core';
import { Popover } from 'primeng/popover';

import { focusFirstElementIn, OverlayFocusKeeper } from './overlay-focus';

/**
 * A non-modal panel anchored to its trigger, over `p-popover`.
 *
 * The trigger stays the caller's own button - `(click)="filters.toggle($event)"`
 * - so it is an ordinary control that `Enter` and `Space` already activate;
 * there is no bespoke key handling to get wrong. `Esc` closes and focus goes
 * back to the trigger.
 *
 * Use it for a small secondary surface: a column chooser, a filter panel, a
 * row of details. Anything that needs the list behind it *and* is a whole
 * record belongs in `app-drawer`; anything the user must answer belongs in
 * `app-modal`.
 */
@Component({
  selector: 'app-popover',
  templateUrl: './popover.component.html',
  styleUrl: './popover.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Popover],
})
export class PopoverComponent {
  /** The accessible name of the panel. */
  readonly label = input.required<string>();
  readonly opened = output<void>();
  readonly closed = output<void>();

  private readonly panel = viewChild.required(Popover);
  private readonly body = viewChild<ElementRef<HTMLElement>>('body');
  private readonly focus = new OverlayFocusKeeper();

  /** Called from the trigger: `(click)="panel.toggle($event)"`. */
  toggle(event: Event): void {
    this.focus.capture();
    this.panel().toggle(event);
  }

  hide(): void {
    this.panel().hide();
  }

  onShow(): void {
    focusFirstElementIn(this.body()?.nativeElement ?? null);
    this.opened.emit();
  }

  onHide(): void {
    this.focus.restore();
    this.closed.emit();
  }
}
