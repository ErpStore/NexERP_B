import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  inject,
  input,
  viewChild,
} from '@angular/core';
import { BlockUI } from 'primeng/blockui';
import { ProgressSpinner } from 'primeng/progressspinner';

/**
 * A busy state **scoped to the region it belongs to**, over `p-blockui`.
 *
 * This replaces `ProcessingOverlay.razor`, which covers the whole page with an
 * animated logo. KB-051 State patterns is explicit that saving must "never
 * block the page": an operator who cannot see the document they just submitted
 * cannot check it while they wait.
 *
 * `fullPage` exists for the rare operation that genuinely owns the whole
 * window - a tenant switch, a session end. It is opt-in and should be
 * justified at the call site, not reached for by habit.
 *
 * It announces busy-ness (`aria-busy` on the region, a polite live message)
 * and **does not steal focus**: moving focus into a spinner strands whoever
 * was typing.
 */
@Component({
  selector: 'app-busy-overlay',
  templateUrl: './busy-overlay.component.html',
  styleUrl: './busy-overlay.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [BlockUI, ProgressSpinner],
})
export class BusyOverlayComponent {
  readonly busy = input(false);
  /** Said out loud, so make it specific: "Posting the sales order", not "Loading". */
  readonly message = input('Working');
  /** Opt-in only. Blocks the entire page; see the class comment. */
  readonly fullPage = input(false);

  private readonly host = inject(ElementRef<HTMLElement>);
  private readonly region = viewChild<ElementRef<HTMLElement>>('region');

  /** `p-blockui`'s `target` contract: it blocks whatever this returns. */
  readonly self = this;

  getBlockableElement(): HTMLElement {
    return this.region()?.nativeElement ?? (this.host.nativeElement as HTMLElement);
  }
}
