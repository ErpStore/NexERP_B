import { ChangeDetectionStrategy, Component, computed, input, output, signal } from '@angular/core';
import { Message } from 'primeng/message';

export type AlertSeverity = 'info' | 'warn' | 'error' | 'success';

/**
 * An alert that sits **next to the thing it is about** - above a form, inside
 * a card, under a grid - rather than floating past as a toast.
 *
 * Icon **plus** text, never colour alone (WCAG 1.4.1): the severity is stated
 * in words for a screen reader and drawn as a shape for anyone who cannot
 * separate the hues.
 *
 * For a business-rule refusal, pass the server's `title` **verbatim**: a 409
 * carries the domain team's own wording there (`ApiProblems.cs:47-53`), and
 * that wording is the product's UX. Rewording it here would be inventing a
 * message the business never approved.
 */
@Component({
  selector: 'app-inline-alert',
  templateUrl: './inline-alert.component.html',
  styleUrl: './inline-alert.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [Message],
})
export class InlineAlertComponent {
  readonly severity = input<AlertSeverity>('info');
  /** Shown verbatim. Required: an alert with no message is a coloured box. */
  readonly message = input.required<string>();
  readonly dismissible = input(false);
  readonly dismissed = output<void>();

  private readonly hidden = signal(false);
  readonly visible = computed(() => !this.hidden());

  /** The word a screen reader hears before the message. */
  readonly severityLabel = computed<string>(() => {
    switch (this.severity()) {
      case 'error':
        return 'Error';
      case 'warn':
        return 'Warning';
      case 'success':
        return 'Success';
      default:
        return 'Information';
    }
  });

  /**
   * PrimeNG 22.1.0 puts `role="alert" aria-live="polite"` on the message host
   * itself (`primeng-message.mjs`), so the live region already exists and a
   * second one inside it would double-announce. Only the urgency is changed:
   * an error interrupts, everything else waits for a pause.
   */
  readonly passThrough = computed(() => ({
    root: { 'aria-live': this.severity() === 'error' ? 'assertive' : 'polite' },
  }));

  dismiss(): void {
    this.hidden.set(true);
    this.dismissed.emit();
  }
}
