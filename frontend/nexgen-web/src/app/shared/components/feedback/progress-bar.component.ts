import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { ProgressBar } from 'primeng/progressbar';

/**
 * Progress, over `p-progressbar`.
 *
 * Two modes, and the distinction matters to a screen reader: determinate
 * carries `aria-valuenow`; indeterminate **omits** it, because a `valuenow` of
 * nothing-in-particular is a lie about how far along the work is. KB-051 State
 * patterns puts the indeterminate bar on a refetch - previous data stays on
 * screen, a thin bar says more is coming.
 */
@Component({
  selector: 'app-progress-bar',
  templateUrl: './progress-bar.component.html',
  styleUrl: './progress-bar.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ProgressBar],
})
export class ProgressBarComponent {
  /** 0-100. Leave undefined for an indeterminate bar. */
  readonly value = input<number | undefined>(undefined);
  readonly label = input('Progress');
  readonly showValue = input(false);

  /**
   * PrimeNG 22.1.0 writes `aria-level="42%"` on the progress bar - `aria-level`
   * is neither allowed on `role="progressbar"` nor a valid percentage value,
   * and axe reports both as critical. Cleared through the pass-through.
   */
  readonly passThrough = { root: { 'aria-level': null } };

  readonly indeterminate = computed(() => this.value() === undefined);
  readonly mode = computed<'determinate' | 'indeterminate'>(() =>
    this.indeterminate() ? 'indeterminate' : 'determinate',
  );
}
