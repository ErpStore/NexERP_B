import { HttpErrorResponse } from '@angular/common/http';
import {
  ChangeDetectionStrategy,
  Component,
  DestroyRef,
  computed,
  effect,
  inject,
  input,
  model,
  output,
  signal,
  untracked,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

import type { ApiProblem } from '@/app/core/http/api-problem';
import { CurrencyFeatureService } from '../../currency.service';
import type { CurrencyVM } from '../../models';
import { BusyOverlayComponent } from '@/app/shared/components/feedback/busy-overlay.component';
import { InlineAlertComponent } from '@/app/shared/components/feedback/inline-alert.component';
import { ToastService } from '@/app/shared/components/feedback/toast.service';
import { FormFieldComponent } from '@/app/shared/components/form/form-field.component';
import { FormLayoutComponent } from '@/app/shared/components/form/form-layout.component';
import { FormSectionComponent } from '@/app/shared/components/form/form-section.component';
import {
  applyServerErrors,
  clearServerErrors,
} from '@/app/shared/components/form/server-validation';
import { TextInputComponent } from '@/app/shared/components/form/text-input.component';
import { DrawerComponent } from '@/app/shared/components/overlay/drawer.component';

/** Which currency the drawer is open for. `'create'` needs no id; `'edit'` does. */
export type CurrencyFormDrawerMode = 'create' | 'edit';

/**
 * M2-D01 — the create/edit surface, route-addressable per `currency.routes.ts`
 * (`/masters/currencies/new`, `/masters/currencies/:id`). Reachable **only** through
 * `CurrencyListComponent`, which owns the routing decision of when this is open; this
 * component only renders what it is told.
 *
 * **Validators mirror `CurrencyVM.cs`'s `DataAnnotations` exactly** (`CurrName`/`CurrSub`
 * required, ≤100; `Symbol` required, matching the server's own **unanchored** character class)
 * — including the server's verbatim messages, via `app-form-field`'s `errorMessages` input,
 * not this directory's generic defaults. `IsSystemDefined` is not a form field: Blazor's own
 * `CurrencyUpsert.razor` never exposes it as one (Confirmed — it has exactly three inputs,
 * `CurrName`/`CurrSub`/`Symbol`); it is grid-only display plus the server-side gate this
 * component mirrors below.
 */
@Component({
  selector: 'app-currency-form-drawer',
  templateUrl: './currency-form-drawer.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    ReactiveFormsModule,
    DrawerComponent,
    FormLayoutComponent,
    FormSectionComponent,
    FormFieldComponent,
    TextInputComponent,
    BusyOverlayComponent,
    InlineAlertComponent,
  ],
})
export class CurrencyFormDrawerComponent {
  protected readonly currencyApi = inject(CurrencyFeatureService);
  private readonly toast = inject(ToastService);
  private readonly destroyRef = inject(DestroyRef);

  /** `CurrencyVM.cs`'s own `DataAnnotations` messages, verbatim — `app-form-field`'s
   * generic defaults ("This field is required.") are not the server's product copy. */
  protected readonly currNameErrors = {
    required: 'Please Enter Currency Name',
    maxlength: 'Currency Name cannot exceed 100 characters',
  };
  protected readonly currSubErrors = {
    required: 'Please Enter Sub Currency Name',
    maxlength: 'Sub Currency Name cannot exceed 100 characters',
  };
  protected readonly symbolErrors = {
    required: 'Please Enter Symbol',
    pattern: 'Only valid currency symbols are allowed (₹, $, €, £, ¥, ₩, ₿, ₽)',
  };

  readonly visible = model(false);
  readonly mode = input.required<CurrencyFormDrawerMode>();
  /** Required when `mode() === 'edit'`; ignored for `'create'`. */
  readonly currencyId = input<number | null>(null);
  readonly canEdit = input(false);

  readonly saved = output<void>();

  protected readonly loading = signal(false);
  protected readonly loadError = signal<ApiProblem | null>(null);
  protected readonly formErrors = signal<readonly string[]>([]);
  /** The row being edited, once loaded — `isSystemDefined` gates the form below. */
  protected readonly loaded = signal<CurrencyVM | null>(null);

  // `Validators.required` is a static, this-free ValidatorFn; the unbound-method rule cannot
  // tell that from a genuine method reference — same waiver `form-field.component.ts` takes.
  /* eslint-disable @typescript-eslint/unbound-method */
  protected readonly form = new FormGroup({
    currName: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
    currSub: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(100)],
    }),
    // Unanchored on purpose, matching CurrencyVM.cs:23 exactly — "$$" and "a$b" both pass
    // server-side, so the client must not tighten this to ^[...]$.
    symbol: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/[$€₹¥£₩₿₽]/)],
    }),
  });
  /* eslint-enable @typescript-eslint/unbound-method */

  /** A system-defined currency refuses edit server-side (`CurrencyService.cs:145-146`) even
   * with the Edit right — Blazor's own `CurrencyUpsert.razor:200` mirrors the same refusal
   * client-side. This form does the same, rather than letting the user submit into a
   * guaranteed 409. */
  protected readonly systemDefined = computed(() => this.loaded()?.isSystemDefined ?? false);
  protected readonly formDisabled = computed(
    () => (this.mode() === 'edit' && (!this.canEdit() || this.systemDefined())) || this.loading(),
  );

  protected readonly title = computed(() =>
    this.mode() === 'create' ? 'New currency' : (this.loaded()?.currName ?? 'Edit currency'),
  );

  constructor() {
    effect(() => {
      const open = this.visible();
      const mode = this.mode();
      const id = this.currencyId();
      untracked(() => {
        if (!open) {
          return;
        }
        this.formErrors.set([]);
        this.loadError.set(null);
        clearServerErrors(this.form);
        if (mode === 'create') {
          this.loaded.set(null);
          this.form.reset({ currName: '', currSub: '', symbol: '' });
          this.form.enable();
          return;
        }
        if (id === null) {
          return;
        }
        this.#loadForEdit(id);
      });
    });

    // Disable/enable the FormGroup imperatively — `[formGroup]`'s own disabled state does not
    // travel through `app-text-input`'s ControlValueAccessor otherwise.
    effect(() => {
      if (this.formDisabled()) {
        untracked(() => this.form.disable());
      } else {
        untracked(() => this.form.enable());
      }
    });
  }

  #loadForEdit(id: number): void {
    this.loading.set(true);
    this.currencyApi
      .getById(id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (vm) => {
          this.loading.set(false);
          this.loaded.set(vm);
          this.form.reset({
            currName: vm.currName,
            currSub: vm.currSub,
            symbol: vm.symbol,
          });
        },
        error: (error: unknown) => {
          this.loading.set(false);
          this.loadError.set(problemFrom(error));
        },
      });
  }

  onLayoutSubmit(): void {
    if (this.form.invalid || this.formDisabled()) {
      return;
    }
    clearServerErrors(this.form);
    this.formErrors.set([]);
    const value = this.form.getRawValue();
    const vm: CurrencyVM = {
      currName: value.currName,
      currSub: value.currSub,
      symbol: value.symbol,
    };

    const request$ =
      this.mode() === 'create'
        ? this.currencyApi.create(vm)
        : this.currencyApi.update(this.currencyId()!, vm);

    request$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.toast.success(this.mode() === 'create' ? 'Currency created.' : 'Currency updated.');
        this.saved.emit();
      },
      error: (error: unknown) => {
        const problem = problemFrom(error);
        const result = applyServerErrors(this.form, problem);
        this.formErrors.set(result.formLevel);
      },
    });
  }
}

/** The server's `ProblemDetails`, as `core/http/error.interceptor.ts` normalised it. */
function problemFrom(error: unknown): ApiProblem {
  if (error instanceof HttpErrorResponse && error.error && typeof error.error === 'object') {
    return error.error as ApiProblem;
  }
  return {};
}
