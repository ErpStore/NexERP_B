import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  afterNextRender,
  computed,
  inject,
  signal,
} from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';

import { AuthService } from '../../../core/auth/auth.service';
import type { LoginFailure } from '../../../core/auth/auth.models';
// Direct file imports, not the shared/components barrel — `/login` is the first route an
// anonymous caller ever loads, and the barrel drags the whole design-system surface
// (data-grid, line-item-grid, record-picker-dialog…) into this one lazy chunk just to
// tree-shake it back out. Same discipline `app.component.ts` already established for the
// eager bundle (R-69); this is its lazy-chunk equivalent.
import { FormFieldComponent } from '../../../shared/components/form/form-field.component';
import { TextInputComponent } from '../../../shared/components/form/text-input.component';
import { InlineAlertComponent } from '../../../shared/components/feedback/inline-alert.component';

interface LoginForm {
  tenant: FormControl<string>;
  username: FormControl<string>;
  password: FormControl<string>;
}

/**
 * M2-C02 — the SPA `/login` route.
 *
 * **Request shape is `{ tenant, username, password }` — M2-A05 added `tenant`.** The tenant
 * field is a plain text identifier (matched server-side against `Tenants.Name` **or**
 * `Tenants.Hostname`, ADR-002 §5) rather than a dropdown: there is no endpoint that lists
 * tenants for an anonymous caller to populate one from, and building one is out of scope —
 * it would itself need a right, and an anonymous caller has none. `auth.service.ts`'s own
 * doc comment records why this field did not exist before M2-A05.
 *
 * Never reveals which credential was wrong (server contract: one `401` body for every
 * authentication failure). A `403` trial-expired failure renders its own verbatim message,
 * never the generic invalid-credentials text (KB-108).
 */
@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, FormFieldComponent, TextInputComponent, InlineAlertComponent],
})
export class LoginComponent {
  readonly #auth = inject(AuthService);
  readonly #router = inject(Router);
  readonly #route = inject(ActivatedRoute);

  // TextInputComponent exposes no public focus() method, so the native <input> its own
  // template renders is reached by querying the component's own host element — simpler and
  // more reliably typed than a viewChild(..., { read: ElementRef }) generic read, which
  // Angular's own type-aware ESLint pass could not fully resolve here.
  readonly #hostElement = inject<ElementRef<HTMLElement>>(ElementRef);

  readonly form = new FormGroup<LoginForm>({
    tenant: new FormControl('', {
      nonNullable: true,
      validators: [(control) => Validators.required(control)],
    }),
    username: new FormControl('', {
      nonNullable: true,
      validators: [(control) => Validators.required(control)],
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [(control) => Validators.required(control)],
    }),
  });

  readonly submitting = signal(false);
  readonly failure = signal<LoginFailure | null>(null);

  /** The one thing rendered for every credential failure — never field-specific. Distinct,
   * verbatim messages for the account-gate reasons a real business decision produced. */
  readonly errorMessage = computed(() => {
    const failure = this.failure();
    if (!failure) {
      return null;
    }
    switch (failure.reason) {
      case 'invalid-credentials':
        return 'Invalid username or password.';
      case 'trial-expired':
      case 'account-gate':
      case 'tenant-unresolved':
        return failure.message ?? 'Sign-in was refused.';
      case 'network':
        return 'Could not reach the server. Check your connection and try again.';
      default:
        return 'Something went wrong. Please try again.';
    }
  });

  constructor() {
    afterNextRender(() => this.#focusFirstField());
  }

  /** The form's first `<input>` — first-child order in the template, not a ref lookup, so
   * this stays correct regardless of which wrapper renders it. M2-A05 made that field
   * `tenant`, not `username`; renamed from `#focusUsername` for the same reason. */
  #focusFirstField(): void {
    this.#hostElement.nativeElement.querySelector('input')?.focus();
  }

  async submit(): Promise<void> {
    if (this.form.invalid || this.submitting()) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);
    this.failure.set(null);

    const { tenant, username, password } = this.form.getRawValue();
    const result = await this.#auth.login(tenant, username, password);

    this.submitting.set(false);

    if (result.ok) {
      const returnUrl = this.#route.snapshot.queryParamMap.get('returnUrl');
      await this.#router.navigateByUrl(returnUrl && returnUrl.startsWith('/') ? returnUrl : '/');
      return;
    }

    this.failure.set(result.failure);
    // Focus goes back to the first field, not the error — a screen reader already announces
    // the alert via its own live region (InlineAlertComponent), and returning focus to the
    // form is what lets the very next keystroke start a retry.
    this.#focusFirstField();
  }
}
