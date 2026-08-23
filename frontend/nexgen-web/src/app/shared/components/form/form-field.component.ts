import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  contentChild,
  forwardRef,
  inject,
  input,
  signal,
  type AfterContentInit,
} from '@angular/core';
import { NgControl, Validators } from '@angular/forms';

import { defaultErrorMessage } from './error-messages';
import { FormFieldContext } from './types';

// `Validators.required` is a static, this-free ValidatorFn; the unbound-method
// rule cannot tell that from a genuine method reference.
// eslint-disable-next-line @typescript-eslint/unbound-method
const REQUIRED_VALIDATOR = Validators.required;

let nextFormFieldId = 0;

/**
 * **The single validation-display mechanism.**
 *
 * Every control in this directory renders through it, and no other file in
 * `form/` renders error markup. That is deliberate: one place decides where a
 * label sits, how an error is worded and announced, and what
 * `aria-describedby` points at. 140 screens compose these, so the a11y
 * contract has to be written once.
 *
 * Layout, per KB-051 Forms: **label above the field**, hint under the label,
 * error inline below the control. Labels above are a density decision, not a
 * stylistic one - they are scannable in a dense form, which is what an
 * operator entering 20-50 lines for a shift is actually doing.
 */
@Component({
  selector: 'app-form-field',
  templateUrl: './form-field.component.html',
  styleUrl: './form-field.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  providers: [{ provide: FormFieldContext, useExisting: forwardRef(() => FormFieldComponent) }],
})
export class FormFieldComponent extends FormFieldContext implements AfterContentInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly uid = `app-field-${++nextFormFieldId}`;

  readonly label = input.required<string>();
  readonly hint = input<string | undefined>(undefined);
  /** Override the required marker when the validator cannot be inspected. */
  readonly required = input<boolean | undefined>(undefined);
  /** Per-error-key message overrides. Does not create a second display mechanism. */
  readonly errorMessages = input<Readonly<Record<string, string>> | undefined>(undefined);
  /** Explicit id, when a screen needs a stable one. */
  readonly fieldId = input<string | undefined>(undefined);

  private readonly projectedControl = contentChild(NgControl);
  /** Bumped on every control event; the computeds below depend on it. */
  private readonly revision = signal(0);
  private readonly groupLabel = signal(false);

  readonly controlId = computed(() => `${this.fieldId() ?? this.uid}-control`);
  readonly labelId = computed(() => `${this.fieldId() ?? this.uid}-label`);
  readonly hintId = computed(() => `${this.fieldId() ?? this.uid}-hint`);
  readonly errorId = computed(() => `${this.fieldId() ?? this.uid}-error`);
  readonly usesGroupLabel = this.groupLabel.asReadonly();

  readonly isRequired = computed(() => {
    const explicit = this.required();
    if (explicit !== undefined) {
      return explicit;
    }
    this.revision();
    return this.projectedControl()?.control?.hasValidator(REQUIRED_VALIDATOR) ?? false;
  });

  /**
   * An error shows once the user has had a go at the field - touched or
   * dirty. Shouting "required" at an untouched create form is noise.
   */
  readonly invalid = computed(() => {
    this.revision();
    const control = this.projectedControl()?.control;
    return !!control && control.invalid && (control.touched || control.dirty);
  });

  readonly errorText = computed(() => {
    this.revision();
    if (!this.invalid()) {
      return null;
    }
    return defaultErrorMessage(
      this.projectedControl()?.control?.errors ?? null,
      this.errorMessages(),
    );
  });

  readonly describedBy = computed(() => {
    const ids = [this.hint() ? this.hintId() : null, this.invalid() ? this.errorId() : null].filter(
      (id): id is string => id !== null,
    );
    return ids.length > 0 ? ids.join(' ') : null;
  });

  ngAfterContentInit(): void {
    const control = this.projectedControl()?.control;
    if (!control) {
      return;
    }
    // `AbstractControl.events` is the only stream that reports *touched* as
    // well as value and status - which matters, because touched is what
    // reveals the error.
    const subscription = control.events.subscribe(() => this.revision.update((n) => n + 1));
    this.destroyRef.onDestroy(() => subscription.unsubscribe());
    this.revision.update((n) => n + 1);
  }

  useGroupLabel(): void {
    this.groupLabel.set(true);
  }
}
