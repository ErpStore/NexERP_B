import {
  Directive,
  ElementRef,
  Renderer2,
  effect,
  inject,
  input,
  signal,
  type AfterViewInit,
} from '@angular/core';
import type { ControlValueAccessor } from '@angular/forms';

import { FormFieldContext } from './types';

/**
 * Everything every control in this directory shares:
 *
 *  - a signal-backed `ControlValueAccessor`, so a keystroke marks *this*
 *    component dirty and nothing else. The workspace is zoneless and every
 *    control is `OnPush`; `render-count.spec.ts` is the test that keeps it
 *    that way, because M2-C07 will put 200 of these on one screen.
 *  - the a11y wiring published by `app-form-field`. Most PrimeNG components
 *    expose `inputId` but **not** `ariaDescribedBy`, so the attributes are
 *    applied to the rendered focusable element instead of guessed at through
 *    a dozen different component inputs. One mechanism, one place.
 *
 * It is a `@Directive()` with no selector purely so Angular accepts the
 * `input()`s and lifecycle hook on an abstract base.
 */
@Directive()
export abstract class BaseFormControl<TValue> implements ControlValueAccessor, AfterViewInit {
  protected readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  protected readonly renderer = inject(Renderer2);
  /** `null` when a control is used outside `app-form-field` - permitted, but unlabelled. */
  protected readonly field = inject(FormFieldContext, { optional: true });

  /** Accessible name for a control used without `app-form-field`. */
  readonly ariaLabel = input<string | undefined>(undefined);
  readonly placeholder = input<string | undefined>(undefined);
  /** Readonly is **not** disabled: the value stays selectable, copyable and in the tab order. */
  readonly readonly = input(false);

  readonly value = signal<TValue | null>(null);
  readonly isDisabled = signal(false);

  /**
   * The focusable element inside this control's DOM. Overridden where the
   * PrimeNG surface renders something other than a plain input.
   */
  protected controlElementSelector = 'input, textarea, select, button';

  private onChangeFn: (value: TValue | null) => void = () => undefined;
  private onTouchedFn: () => void = () => undefined;

  constructor() {
    effect(() => {
      // Read the signals unconditionally so the effect re-runs on any change.
      const describedBy = this.field?.describedBy() ?? null;
      const invalid = this.field?.invalid() ?? false;
      const required = this.field?.isRequired() ?? false;
      const id = this.field?.controlId() ?? null;
      this.applyAccessibilityAttributes(describedBy, invalid, required, id);
    });
  }

  ngAfterViewInit(): void {
    // The effect above can run before the PrimeNG surface has rendered its
    // input; this second pass is what makes the attributes reliably present.
    this.applyAccessibilityAttributes(
      this.field?.describedBy() ?? null,
      this.field?.invalid() ?? false,
      this.field?.isRequired() ?? false,
      this.field?.controlId() ?? null,
    );
  }

  /**
   * True when the PrimeNG surface exposes something that is not a labelable
   * element (p-select puts role=combobox on a span), so a native
   * `<label for>` cannot name it and `aria-labelledby` has to.
   */
  protected usesAriaLabelledBy(): boolean {
    return false;
  }

  /** The element the aria attributes belong on. Overridable for group controls. */
  protected accessibilityTarget(): HTMLElement | null {
    return this.host.nativeElement.querySelector<HTMLElement>(this.controlElementSelector);
  }

  private applyAccessibilityAttributes(
    describedBy: string | null,
    invalid: boolean,
    required: boolean,
    controlId: string | null,
  ): void {
    const target = this.accessibilityTarget();
    if (!target) {
      return;
    }
    this.setOrRemove(target, 'aria-describedby', describedBy);
    if (this.usesAriaLabelledBy()) {
      this.setOrRemove(target, 'aria-labelledby', this.field?.labelId() ?? null);
    }
    this.setOrRemove(target, 'aria-invalid', invalid ? 'true' : null);
    this.setOrRemove(target, 'aria-required', required ? 'true' : null);
    if (controlId && !target.id) {
      this.renderer.setAttribute(target, 'id', controlId);
    }
  }

  private setOrRemove(target: HTMLElement, attribute: string, value: string | null): void {
    if (value === null) {
      this.renderer.removeAttribute(target, attribute);
    } else {
      this.renderer.setAttribute(target, attribute, value);
    }
  }

  // --- ControlValueAccessor -------------------------------------------------

  writeValue(value: TValue | null): void {
    this.value.set(value);
  }

  registerOnChange(fn: (value: TValue | null) => void): void {
    this.onChangeFn = fn;
  }

  registerOnTouched(fn: () => void): void {
    this.onTouchedFn = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.isDisabled.set(isDisabled);
  }

  /** Commit a new value to the form. */
  protected emitValue(value: TValue | null): void {
    this.value.set(value);
    this.onChangeFn(value);
  }

  /** Mark the control touched - the trigger for showing an error. */
  protected markTouched(): void {
    this.onTouchedFn();
  }

  /** `id` for the focusable element; falls back to nothing outside a form field. */
  protected inputId(): string | undefined {
    return this.field?.controlId();
  }
}
