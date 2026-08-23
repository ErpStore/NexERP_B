import { InjectionToken, type Signal } from '@angular/core';

/*
 * Public types for the form layer (M2-C04-02).
 *
 * Nothing in this file implements an ERP rule. It declares the *contracts* the
 * controls consume - the decimal port owned by M2-C10, the precision policy
 * traceable to the server's `Companydetails.DecimalPlaces`, and the small
 * option/loader shapes the choice controls take from their caller.
 */

// --- Money and quantity ----------------------------------------------------

declare const DECIMAL_BRAND: unique symbol;

/**
 * An amount of money. **Opaque on purpose**: it is not a `number`, so a
 * component physically cannot do arithmetic on it.
 *
 * TODO(M2-C10): re-export this from `@/app/shared/utils/decimal` once that
 * module merges, and delete the declaration here. The brand is deliberately
 * structural-incompatible with `number` so the swap is a compile error if the
 * shapes ever diverge.
 */
export interface Money {
  readonly [DECIMAL_BRAND]: 'Money';
}

/** A quantity. Same reasoning as {@link Money}; a different brand so the two do not mix. */
export interface Qty {
  readonly [DECIMAL_BRAND]: 'Qty';
}

export type ScaledDecimal = Money | Qty;

/**
 * The decimal operations the numeric controls need. **M2-C10 owns the
 * implementation.** This task declares the port and nothing else: a
 * `parseFloat` in a control is the defect M2-C10 exists to prevent, and it
 * would stay invisible until an invoice failed to reconcile.
 *
 * TODO(M2-C10): provide the real implementation for `DECIMAL_PORT`. Until it
 * lands the three numeric controls render read-only-ish (no port -> no parse)
 * and say so in `form/README.md`.
 */
export interface DecimalPort {
  /** Parse what the user typed. Returns `null` for empty; the port decides what is invalid. */
  parseUserInput<T extends ScaledDecimal = ScaledDecimal>(text: string, scale: number): T | null;
  /** Render for display at the given scale. */
  format(value: ScaledDecimal | null, scale: number): string;
  /**
   * Bridge to `p-inputnumber`, whose model is a JavaScript `number`. The
   * precision concern of that boundary is M2-C10's to solve - it is not
   * papered over here with a cast.
   */
  fromNumber<T extends ScaledDecimal = ScaledDecimal>(
    value: number | null,
    scale: number,
  ): T | null;
  toNumber(value: ScaledDecimal | null): number | null;
  equals(a: ScaledDecimal | null, b: ScaledDecimal | null): boolean;
}

export const DECIMAL_PORT = new InjectionToken<DecimalPort>('DECIMAL_PORT');

// --- Precision policy ------------------------------------------------------

/**
 * Server default for money decimal places.
 *
 * Confirmed: `V.SMART/V.SMART.Shared/Data/Master/Company_Module/Companydetails.cs:208`
 * - `public int DecimalPlaces { get; set; } = 2;` (re-verified 2026-08-23).
 *
 * This is the *fallback* for a client that has not yet loaded company
 * settings. It lives here, once, so that no component hardcodes it.
 */
export const SERVER_DEFAULT_DECIMAL_PLACES = 2;

export interface PrecisionPolicy {
  /** Decimal places for money. Mirrors `Companydetails.DecimalPlaces`. */
  readonly currencyScale: number;
  /** Decimal places for quantities. */
  readonly quantityScale: number;
}

export const DEFAULT_PRECISION_POLICY: PrecisionPolicy = {
  currencyScale: SERVER_DEFAULT_DECIMAL_PLACES,
  quantityScale: SERVER_DEFAULT_DECIMAL_PLACES,
};

/**
 * Injected precision. A screen overrides this with the tenant's loaded
 * `Companydetails.DecimalPlaces`; nothing reads a literal.
 */
export const PRECISION_POLICY = new InjectionToken<PrecisionPolicy>('PRECISION_POLICY', {
  providedIn: 'root',
  factory: () => DEFAULT_PRECISION_POLICY,
});

// --- Choice controls -------------------------------------------------------

/** The option shape every choice control understands. `value` is opaque to the control. */
export interface SelectOption<TValue = unknown> {
  readonly value: TValue;
  readonly label: string;
  readonly disabled?: boolean;
}

/**
 * `app-combobox`'s caller-supplied search. The control knows nothing about
 * `HttpClient` or any endpoint - see the task's API Changes section.
 */
export type ComboboxLoader<TValue = unknown> = (
  query: string,
) => Promise<readonly SelectOption<TValue>[]>;

// --- amount-or-percent -----------------------------------------------------

/**
 * The recurring `...AmtOrPer` shape. Confirmed on the document model at
 * `V.SMART/V.SMART.Shared/Data/OutSourcing/Debit_Note/DebitNote.cs:95`
 * (`DiscAmtOrPer`), `:109` (`PackingAmtOrPer`), `:117` (`InsuranceAmtOrPer`),
 * `:146` (`TCSAmtOrPer`) - all `bool`, all defaulting to `true`.
 *
 * Polarity, Confirmed at `V.SMART/V.SMART.Shared/Services/CalculationService.cs:29-31`:
 * `true` means the number is a fixed **amount**, `false` means it is a
 * **percent**. `isAmount` is that flag unnegated.
 *
 * The control captures the pair and **applies nothing**. BR-CALC-001
 * (`CalculationService.UpdateTotalsAsync`) applies the flag, server-side.
 */
export interface AmountOrPercent {
  readonly value: Money | null;
  readonly isAmount: boolean;
}

// --- The form-field contract ----------------------------------------------

/**
 * What `app-form-field` publishes to the control it wraps. It is an abstract
 * class rather than an interface so it can be a DI token without a second
 * declaration.
 *
 * There is exactly one implementation - `FormFieldComponent` - because there
 * is exactly one validation-display mechanism.
 */
export abstract class FormFieldContext {
  /** The `id` the wrapped control must put on its focusable element. */
  abstract readonly controlId: Signal<string>;
  /** The `id` of the rendered label, for controls that use `aria-labelledby`. */
  abstract readonly labelId: Signal<string>;
  /** Space-separated hint + error ids, or `null` when there is nothing to describe. */
  abstract readonly describedBy: Signal<string | null>;
  /** True when the field is showing an error. */
  abstract readonly invalid: Signal<boolean>;
  /** True when the wrapped control is required. */
  abstract readonly isRequired: Signal<boolean>;
  /**
   * Called by a control that is a *group* (radio group), for which a
   * `<label for>` has no single target: the label becomes `aria-labelledby`
   * on the group container instead.
   */
  abstract useGroupLabel(): void;
}

// --- Dates -----------------------------------------------------------------

/**
 * Date display format, in PrimeNG's own notation (`yy` = four-digit year).
 *
 * TODO: the tenant's format is not available to the client yet - no endpoint
 * exposes it and `Companydetails` carries no date-format column that this
 * task found. Until one exists the default is **ISO** (`yy-mm-dd`), which is
 * unambiguous and sorts correctly, rather than a guessed `dd/mm/yyyy`.
 * Recorded as Q-75 in `docs/kb/open-questions.md`.
 */
export const DATE_FORMAT = new InjectionToken<string>('DATE_FORMAT', {
  providedIn: 'root',
  factory: () => 'yy-mm-dd',
});
