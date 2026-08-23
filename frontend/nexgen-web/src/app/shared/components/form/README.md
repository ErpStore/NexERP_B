# The form layer

Built by **M2-C04-02** against [KB-051 §Forms](../../../../../../docs/kb/frontend-new/design-system.md#forms)
and [ADR-007](../../../../../../docs/kb/decisions/ADR-007-angular-stack.md): standalone Angular
components, `ChangeDetectionStrategy.OnPush`, typed Reactive Forms, PrimeNG 22 underneath.

Import from `@/app/shared/components` (the barrel), never from a file path.

## Three rules

1. **One validation display.** Every control renders inside `app-form-field`, and
   `app-form-field` is the only place a field error is drawn. If you find yourself writing
   error markup next to a control, you are building a second mechanism — don't.
2. **No business logic in a control.** A control captures, formats and displays. It applies no
   party cascade, no duplicate-line check, no quantity balance, no tax rule and no
   `…AmtOrPer` calculation. The server stays authoritative for validation, calculation,
   permissions and document numbering.
3. **No permission input.** Gating is M2-C02's `has-right` directive, applied by the screen. A
   `requiredRight` input on a control would scatter permission logic across ~140 screens and
   invite the belief that the client enforces it.

Client validators mirror `DataAnnotations` **for UX only**. Cross-field and cross-row rules
(`ValidateRowAsync`, `IsItemAlreadySelected`, quantity-balance checks) are _not_ expressible as
field validators; they are extracted server-side by each wave's `-03` step. The validator shapes
themselves are generated from OpenAPI by **M2-B10** — nothing here hand-writes a schema for a
real ERP entity.

## Composition

```html
<form [formGroup]="form" (ngSubmit)="layout.onSubmit()">
  <app-form-layout #layout [form]="form" mode="create" [formErrors]="formErrors()">
    <app-form-section title="Header">
      <app-form-field label="Document number" hint="Trailing spaces are trimmed.">
        <app-text-input formControlName="docNo" />
      </app-form-field>
    </app-form-section>

    <ng-container formActions>
      <button type="submit" [disabled]="submitting()">Save</button>
    </ng-container>
  </app-form-layout>
</form>
```

**The screen owns the `<form [formGroup]>` element.** Angular resolves a projected
`formControlName` through the _declaration_ injector tree, so a `FormGroupDirective` living
inside `app-form-layout` would be invisible to projected fields and every one of them would
throw `NG01050`. The typed group is handed in as an input instead.

## Layout

| Component          | What it owns                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| ------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `app-form-layout`  | The responsive grid — **3 columns at ≥1440, 2 at 1024–1439, 1 at ≤1023**, from M2-C04-01's `BREAKPOINTS`, no new breakpoint values. The loading **skeleton** shaped like the final field layout (never a spinner on a blank page), the form-level error alert (server message verbatim), the sticky footer slot (`[formActions]`), `dirty` for the M2-C03/M2-C08 `CanDeactivateFn` guard, and **autofocus of the first field in `mode="create"`**. It does _not_ implement the unsaved-changes guard. |
| `app-form-section` | A titled group with an optional description, optionally collapsible; the heading stays in the a11y tree either way.                                                                                                                                                                                                                                                                                                                                                                                   |
| `app-form-field`   | Label **above** the field, optional hint, required marker `*` **and** `aria-required`, error inline below with an icon (never colour-only), `aria-describedby` composed from hint + error ids, `aria-invalid`, and a permanent `aria-live="polite"` region so the error is announced. An error shows once the control is **touched or dirty** — shouting "required" at an untouched create form is noise.                                                                                             |

## Controls

| Control                       | PrimeNG                                | Value type                                    | Notes                                                                    |
| ----------------------------- | -------------------------------------- | --------------------------------------------- | ------------------------------------------------------------------------ |
| `app-text-input`              | `[pInputText]`                         | `string`                                      | Trims **on commit (blur)**, not while typing.                            |
| `app-textarea`                | `[pTextarea]`                          | `string`                                      | Same trim behaviour; `rows` input.                                       |
| `app-number-input`            | `p-inputnumber`                        | `Qty`                                         | Right-aligned tabular figures. Scale from the injected precision policy. |
| `app-currency-input`          | `p-inputnumber`                        | `Money`                                       | As above, currency scale.                                                |
| `app-amount-or-percent-input` | `p-inputnumber` + `p-selectbutton`     | `{ value: Money \| null; isAmount: boolean }` | Toggling the mode does **not** convert the number.                       |
| `app-select`                  | `p-select`                             | one option value                              | Loading / empty / error rows inside the dropdown.                        |
| `app-multi-select`            | `p-multiselect`                        | array of option values                        | As above; chips display, and `Backspace` removes the most recent chip.   |
| `app-combobox`                | `p-autocomplete`                       | one option value                              | Debounced async search through a caller-supplied loader.                 |
| `app-date-picker`             | `p-datepicker`                         | `Date`                                        | Typed entry always available.                                            |
| `app-date-range-picker`       | `p-datepicker` `selectionMode="range"` | `[Date, Date]`                                | As above.                                                                |
| `app-checkbox`                | `p-checkbox`                           | `boolean`                                     | A boolean **saved on submit**.                                           |
| `app-radio-group`             | `p-radiobutton`                        | one option value                              | The label becomes `aria-labelledby` on the group.                        |
| `app-switch`                  | `p-toggleswitch`                       | `boolean`                                     | A toggle with **immediate effect**.                                      |
| `app-file-upload`             | `p-fileupload`                         | `File[]`                                      | `customUpload`, no transport.                                            |

### Switch versus checkbox

`app-switch` is for a toggle that takes effect the moment it is flipped. A field saved when the
form is submitted uses `app-checkbox`. Say it once here, or the two become interchangeable
across ~140 screens and neither means anything.

### The triad: loading, empty, error

`app-select`, `app-multi-select` and `app-combobox` render all three **inside the dropdown** —
a blocking overlay on a typeahead steals the keyboard. `app-combobox` additionally holds the
previous list while refetching, and its error row carries a **Retry**. `app-file-upload` with no
files shows its drop target and the accepted types, not blank space.

## Keyboard model

Each row says what the control does **and where that is proved**. Where a key is not asserted
the row says so, with the reason, instead of implying coverage.

| Control                             | Keys                                                                                                                                                                                                                                                                    | Asserted by                                                            |
| ----------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------- |
| text, textarea                      | Native. Tab order follows visual order; trim happens on commit.                                                                                                                                                                                                         | `text-input.component.spec.ts`, `textarea.component.spec.ts`           |
| select                              | `ArrowDown` opens; arrows move; `Home`/`End` jump to the first/last option; `Enter` selects; `Esc` closes and keeps the previous value.                                                                                                                                 | `select.component.spec.ts`                                             |
| multi-select                        | As select, and `Enter` toggles each option with the panel staying open; `Backspace` removes the most recent chip, and is ignored while the filter box holds text.                                                                                                       | `multi-select.component.spec.ts`                                       |
| combobox                            | Arrows move through the results, `Enter` selects the highlighted one, `Esc` closes. `Home`/`End` move the **caret**, not the list — this one is a text entry, and PrimeNG's `AutoComplete` treats them that way. `aria-activedescendant` tracks the highlighted option. | `combobox.component.spec.ts`                                           |
| radio group                         | `Tab` enters and leaves the group as one stop; `Space` selects the focused option. **Arrow movement between options is not asserted** — see below.                                                                                                                      | `radio-group.component.spec.ts`                                        |
| checkbox, switch                    | `Space` toggles.                                                                                                                                                                                                                                                        | `checkbox.component.spec.ts`, `switch.component.spec.ts`               |
| date pickers                        | Typing is always available and focusing the field opens the calendar. **The calendar grid keys — arrows by day, `PageUp`/`PageDown` by month, `Esc` to close — are not asserted**; see below. **The calendar is never the only entry path** — typists are faster.       | `date-picker.component.spec.ts`, `date-range-picker.component.spec.ts` |
| file upload                         | The choose control is a real button: `Enter` and `Space` both open the picker, and every file row's Remove is focusable.                                                                                                                                                | `file-upload.component.spec.ts`                                        |
| number, currency, amount-or-percent | Native numeric entry plus PrimeNG's spinner keys; the mode toggle is arrow-navigable. **Masked entry is not asserted** — `p-inputnumber` is a masked input and `userEvent.type` cannot drive it under jsdom (stated in `number-input.component.spec.ts:12-20`).         | `number-input.component.spec.ts` for the value contract                |

### What the keyboard pass at review owns, and why

Three behaviours cannot be asserted in this test environment. Each is recorded here so that the
manual pass knows exactly what it is carrying, and so that no reader mistakes silence for
coverage.

1. **Radio-group arrow movement.** Native radios get roving focus from the user agent. jsdom
   does not implement it and `userEvent` does not synthesise it, so there is nothing to assert
   against.
2. **The date-picker calendar grid.** PrimeNG 22.1.0 reads the legacy `event.which` /
   `event.keyCode` in `DatePicker.onDateCellKeydown` and `DatePicker.onInputKeydown`, while
   `@testing-library/user-event` v14 dispatches keydown with `which === 0` and `keyCode === 0`
   under jsdom (probed 2026-08-23). Every handler therefore falls through whatever key is sent,
   so a test written against them would assert the harness rather than the control.
3. **Masked numeric typing** in `p-inputnumber` — the reason is in
   `number-input.component.spec.ts:12-20`: the mask rebuilds its value from key events and
   selection ranges that jsdom does not implement faithfully.

`readonly` is **not** `disabled`: a readonly control keeps its value selectable, copyable and in
the tab order.

## Money — what is deliberately missing

`app-number-input`, `app-currency-input` and `app-amount-or-percent-input` hold `Money`/`Qty`,
which are **opaque branded types**: a component physically cannot do arithmetic on one. All
parsing and formatting goes through the injected `DECIMAL_PORT`.

> **TODO(M2-C10).** M2-C10 owns the real decimal module and has not merged. `DECIMAL_PORT` is
> declared here and provided by nobody in the application; without a provider the three numeric
> controls cannot parse, and they say so rather than guessing. `fake-decimal-port.ts` is a **test
> fixture** — it is not exported from `index.ts` and is provided nowhere in application code.
> A `parseFloat` in a control is exactly the defect M2-C10 exists to prevent and would stay
> invisible until an invoice failed to reconcile.

Decimal places come from the injected `PRECISION_POLICY`, whose fallback constant is traceable to
`Companydetails.DecimalPlaces` (`V.SMART/V.SMART.Shared/Data/Master/Company_Module/Companydetails.cs:208`,
default `2`, re-verified 2026-08-23). No component hardcodes it.

`AmountOrPercent`'s polarity — `true` means a fixed **amount**, `false` a **percent** — is
Confirmed at `V.SMART/V.SMART.Shared/Services/CalculationService.cs:29-31`, matching the
`…AmtOrPer` flags at `V.SMART/V.SMART.Shared/Data/OutSourcing/Debit_Note/DebitNote.cs:95,109,117,146`.
How the pair projects onto the server's _two separate_ properties is **Q-74**, an API contract
question, not a control question.

## Server errors

```ts
const result = applyServerErrors(this.form, problem);
this.formErrors.set(result.formLevel);
```

`applyServerErrors` is a pure function — no state, testable without a TestBed. It maps a 400
`ProblemDetails` `errors` dictionary onto matching typed controls as a `server` error key,
marks them touched (an error the user never triggered is otherwise invisible), and returns
whatever matched no control for the form-level alert **with the server's message verbatim**:
those strings are product UX. A 409's business-rule message goes straight into `formErrors`.

Key matching is exact-path first, then case-insensitive — `ModelState` keys are PascalCase C#
property names while control names are camelCase. That is **Inferred** from the absence of any
`DictionaryKeyPolicy` in `V.SMART.Api`, not observed on the wire: **Q-72**.

## Known gaps, recorded rather than hidden

| Gap                                                                          | Where                                           |
| ---------------------------------------------------------------------------- | ----------------------------------------------- |
| The decimal module is not implemented.                                       | TODO(M2-C10) in `types.ts`, `numeric-base.ts`   |
| The form-level alert is a minimal placeholder, not the shared `InlineAlert`. | TODO(M2-C04-03) in `form-layout.component.html` |
| Date format defaults to ISO; no endpoint exposes the tenant's.               | Q-75, `DATE_FORMAT` in `types.ts`               |
| `TrimmedInputText`'s whitespace-only quirk is reproduced, not fixed.         | Q-73, `text-input.component.ts`                 |
| `ProblemDetails` key casing is inferred, not observed.                       | Q-72, `server-validation.ts`                    |

## Testing

`*.spec.ts` only — `tsconfig.spec.json` includes exactly `src/**/*.d.ts` and `src/**/*.spec.ts`,
so a differently named test file is silently not run.

- `render-count.spec.ts` is the test that protects M2-C07: typing 20 characters into one control
  inside a 50-control layout re-checks **zero** siblings, counted by a probe bound in each
  sibling's own template. Two negative controls sit beside it, so the probe cannot pass while
  inert.
- `a11y.spec.ts` runs `axe` over every control in both themes. jsdom applies no stylesheet, so
  `color-contrast` cannot run there — contrast is covered by computation in
  `src/app/core/theme/contrast.spec.ts` (M2-C04-01). The repository-wide axe-in-CI pass is M5-09.
- `p-inputnumber` is a masked input that rebuilds its value from key events and selection
  ranges, neither faithfully implemented in jsdom. The numeric specs therefore drive the model
  change PrimeNG emits and assert keyboard _reachability_ separately; real masked typing is
  covered by the keyboard pass required at review.
