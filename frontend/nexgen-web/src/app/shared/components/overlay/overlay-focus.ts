/**
 * The one focus contract every overlay in this directory shares.
 *
 * PrimeNG's `p-dialog` and `p-drawer` bring their own focus *trap*
 * (`[focusTrap]`) and their own `Esc` handling, and this task's job is to
 * verify that by test rather than to reimplement it. What they do **not**
 * reliably do is put focus back on the exact element that opened the overlay
 * once it closes - and that is the half a keyboard user notices, because
 * losing it drops them at `<body>` and makes them re-tab through the page.
 *
 * So: trap and `Esc` come from PrimeNG, restoration comes from here, and both
 * halves are asserted in `modal.component.spec.ts` and
 * `drawer.component.spec.ts`.
 */
export class OverlayFocusKeeper {
  private invoker: HTMLElement | null = null;

  /**
   * Remember what had focus. Call this as the overlay is asked to open, not
   * after it has rendered - by then the overlay has already taken focus.
   */
  capture(): void {
    const active = document.activeElement;
    this.invoker = active instanceof HTMLElement && active !== document.body ? active : null;
  }

  /**
   * Put focus back where it was. A no-op when the invoker has since left the
   * DOM: focusing a detached element silently sends focus to `<body>`, which
   * is the very outcome this exists to avoid, so the caller's own fallback
   * (usually leaving focus alone) is better.
   */
  restore(): void {
    const target = this.invoker;
    this.invoker = null;
    if (target && target.isConnected) {
      target.focus();
    }
  }

  /** The element focus will return to, for assertions and for diagnostics. */
  get invokingElement(): HTMLElement | null {
    return this.invoker;
  }
}

/**
 * What counts as focusable for the purposes of moving focus *into* an overlay.
 * Deliberately narrow: the first thing a keyboard user lands on should be an
 * actual control, not a decorative element someone gave a `tabindex`.
 */
const FOCUSABLE =
  'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]),' +
  ' textarea:not([disabled]), [tabindex]:not([tabindex="-1"])';

/**
 * Move focus to the first control inside a just-opened overlay, and fall back
 * to the overlay container itself when it has none.
 *
 * PrimeNG's own `focusOnShow` is left enabled and runs first; this only acts
 * when focus is still outside. It exists because PrimeNG decides what is
 * focusable partly from layout, and there are environments - jsdom under test
 * being the obvious one - where it declines to move focus at all. An overlay
 * that opens without taking focus strands a keyboard user behind it.
 */
export function focusFirstElementIn(root: HTMLElement | null): void {
  if (!root || root.contains(document.activeElement)) {
    return;
  }
  const first = root.querySelector<HTMLElement>(FOCUSABLE);
  if (first) {
    first.focus();
    return;
  }
  if (!root.hasAttribute('tabindex')) {
    root.setAttribute('tabindex', '-1');
  }
  root.focus();
}
