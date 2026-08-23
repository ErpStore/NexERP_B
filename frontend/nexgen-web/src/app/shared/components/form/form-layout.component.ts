import { DOCUMENT } from '@angular/common';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  ElementRef,
  afterNextRender,
  computed,
  inject,
  input,
  output,
  signal,
  type AfterContentInit,
} from '@angular/core';
import { FormGroup } from '@angular/forms';

import { BREAKPOINTS, bandFor, type BreakpointName } from '@/app/core/theme/breakpoints';

/** Columns per KB-051 Responsive behaviour: >=1440 three, 1024-1439 two, <=1023 one. */
const COLUMNS_BY_BAND: Record<BreakpointName, number> = { xs: 1, sm: 1, md: 2, lg: 3 };

/**
 * The host of a form screen: the typed `FormGroup`, the responsive column
 * grid, the loading skeleton, the form-level error area and the sticky footer
 * action slot.
 *
 * **The screen owns the `<form [formGroup]>` element and puts this inside it.**
 * That is not a stylistic choice: Angular resolves a projected
 * `formControlName` through the *declaration* injector tree, so a
 * `FormGroupDirective` living inside this component would be invisible to the
 * fields the caller projects into it, and every field would throw NG01050.
 * The typed group is still handed in as an input - this component reads its
 * state, it just does not render the element.
 *
 * It **exposes** dirty state; it does not implement the unsaved-changes
 * guard. That guard is a router `CanDeactivateFn` in M2-C03/M2-C08 - a form
 * control cannot block navigation, and pretending otherwise would scatter
 * routing concerns across 140 screens.
 */
@Component({
  selector: 'app-form-layout',
  templateUrl: './form-layout.component.html',
  styleUrl: './form-layout.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FormLayoutComponent implements AfterContentInit {
  private readonly document = inject(DOCUMENT);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly destroyRef = inject(DestroyRef);

  readonly form = input.required<FormGroup>();
  /** `create` autofocuses the first field; `edit` leaves the focus where the user put it. */
  readonly mode = input<'create' | 'edit'>('edit');
  readonly loading = input(false);
  readonly submitting = input(false);
  /** How many skeleton rows to draw - match the real field count so the page does not jump. */
  readonly skeletonFields = input(6);
  /**
   * Form-level messages, shown verbatim. A 409's business-rule message
   * arrives here (KB-050 Error handling), as does anything
   * `applyServerErrors` could not match to a control.
   */
  readonly formErrors = input<readonly string[]>([]);
  /** Force a column count; otherwise it follows the viewport band. */
  readonly columns = input<number | undefined>(undefined);

  readonly formSubmit = output<void>();

  private readonly viewportWidth = signal(this.document.defaultView?.innerWidth ?? BREAKPOINTS.lg);
  private readonly revision = signal(0);

  readonly columnCount = computed(
    () => this.columns() ?? COLUMNS_BY_BAND[bandFor(this.viewportWidth())],
  );

  /** The dirty state a `CanDeactivateFn` guard consumes. */
  readonly dirty = computed(() => {
    this.revision();
    return this.form().dirty;
  });

  readonly skeletonRows = computed(() =>
    Array.from({ length: this.skeletonFields() }, (_, i) => i),
  );

  constructor() {
    const view = this.document.defaultView;
    if (view) {
      const onResize = (): void => this.viewportWidth.set(view.innerWidth);
      view.addEventListener('resize', onResize);
      this.destroyRef.onDestroy(() => view.removeEventListener('resize', onResize));
    }

    afterNextRender(() => this.autofocusFirstField());
  }

  ngAfterContentInit(): void {
    const subscription = this.form().events.subscribe(() => this.revision.update((n) => n + 1));
    this.destroyRef.onDestroy(() => subscription.unsubscribe());
  }

  /**
   * Call from the screen's `(ngSubmit)`. Marking everything touched is what
   * makes an untouched required field show its error on the first submit.
   */
  onSubmit(): void {
    this.form().markAllAsTouched();
    this.formSubmit.emit();
  }

  private autofocusFirstField(): void {
    if (this.mode() !== 'create' || this.loading()) {
      return;
    }
    const first = this.host.nativeElement.querySelector<HTMLElement>(
      'input:not([type="hidden"]):not([disabled]), textarea:not([disabled]), select:not([disabled])',
    );
    first?.focus();
  }
}
