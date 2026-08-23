# `shared/utils/decimal` — money and quantities

Created by **M2-C10**. Every money, quantity, rate, percentage and tax value in the SPA is
parsed, held, compared, formatted and (where unavoidable) combined through this module.

## Why it exists

JavaScript's `number` is IEEE-754 binary floating point: `0.1 + 0.2 !== 0.3`. The server-side
model is C# `decimal` throughout `V.SMART.Shared/Data/**`, priced to
`Companydetails.DecimalPlaces` (default `2` —
`V.SMART/V.SMART.Shared/Data/Master/Company_Module/Companydetails.cs:208`), with an explicit
per-document `RoundOff` (`MfgInv.cs:210`, `PurchPo.cs:167`). A one-paisa client/server
disagreement is not cosmetic — it is a document that will not reconcile against the printed
invoice.

`decimal.js` is carried over from ADR-003 to
[ADR-007](../../../../../../../docs/kb/decisions/ADR-007-angular-stack.md) unchanged. A library
nobody is required to use enforces nothing, so the discipline is mechanised:

- `decimal.js` is imported in exactly one file, `decimal.ts`.
- `eslint.config.js` bans `decimal.js` imports, `parseFloat`, unary `+` coercion, `.toFixed()`
  and `Math.round/floor/ceil` outside this folder.
- `no-float-money.spec.ts` re-scans `src/**` for the same patterns plus `DecimalPipe`,
  `CurrencyPipe` and arithmetic applied to money-named identifiers, catching what an inline
  `eslint-disable` would let through.

## What this module must never contain

**No ERP calculation.** There is no `calculateLineTotal`, no `applyTax`, no `applyDiscount`,
no freight, no TCS, no round-off, no costing and no stock allocation — and there never will be.

| Rule        | Where it lives                                               | The client never does it                                                  |
| ----------- | ------------------------------------------------------------ | ------------------------------------------------------------------------- |
| BR-CALC-001 | `CalculationService.UpdateTotalsAsync(ICalculationDocument)` | Compute the total/tax/discount/freight/TCS/round-off that gets **saved**. |
| BR-STK-001  | `StockManagerService` FIFO allocation                        | Compute a stock allocation or a balance quantity.                         |

A screen may show a **provisional** preview for responsiveness. It is visually marked as
provisional and is **overwritten by the server's result before save**. If you want a total, the
answer is an API call.

## The wire boundary (INV-032)

`V.SMART.Api` configures no custom JSON serialiser, so ASP.NET Core's default
`System.Text.Json` applies and a C# `decimal` arrives as a **JSON number**.

- `fromApi(value, field)` accepts a JSON number or a JSON string and throws
  `DecimalBoundaryError` — naming the field — for `null`, `undefined`, `NaN` and `''`. It never
  substitutes zero: a missing amount rendered as `0.00` is the exact bug class this module
  exists to prevent.
- `toApi(value)` returns a JSON **number** when the value round-trips through IEEE-754 without
  losing a digit, and the exact decimal **string** when it would not. A JSON number cannot
  carry more than ~15 significant digits, while storage is `decimal(18, n)`; shipping a
  truncated number silently would be worse than a value the API rejects loudly.
  **Recorded as a finding for M2-B10 / M2-A06: the API should serialise `decimal` as a JSON
  string.** That change is not made by M2-C10.

## Precision (INV-032)

`precision.ts` is the single policy. Components never hardcode `2`.

| Constant                        | Value | Evidence                                        |
| ------------------------------- | ----- | ----------------------------------------------- |
| `SERVER_DEFAULT_DECIMAL_PLACES` | 2     | `Companydetails.cs:208` — a **display** setting |
| `STORAGE_SCALE.amount`          | 2     | `Banks.cs:36,39` `[Precision(18, 2)]`           |
| `STORAGE_SCALE.quantity`        | 3     | `StockAdd.cs:36,39` `[Precision(18, 3)]`        |
| `STORAGE_SCALE.rate`            | 4     | `StockAdd.cs:44` `[Precision(18, 4)]`           |

There is no per-item quantity precision and no per-price-list rate precision anywhere in
`V.SMART.Shared/Data` — a negative result recorded by INV-032. Note the divergence: display
defaults to 2 places while quantity and rate columns store 3 and 4.

## Rounding (INV-032)

The server rounds with `MidpointRounding.AwayFromZero`
(`V.SMART/V.SMART.Shared/Services/CalculationService.cs:103`), which is `decimal.js`'s
`ROUND_HALF_UP`. That is this module's default and it is **taken from the server**, not chosen
here. Rounding is always explicit: `round(value, places, mode)`. Nothing else rounds implicitly
except `format()`, which rounds for display only and never mutates a value.

## Using it

```ts
import { format, fromApi, money, parseUserInput } from '@/app/shared/utils/decimal';
```

In templates use the `money` pipe (`shared/pipes/money.pipe.ts`). Angular's `DecimalPipe` and
`CurrencyPipe` must **not** be used on a money value: both coerce to `number` and reintroduce
the hazard. Render with `font-variant-numeric: tabular-nums`, right-aligned (KB-051).

An **absent** amount renders as an em dash (`—`), never `0.00`. "We have no value" and "the
value is zero" are different facts.
