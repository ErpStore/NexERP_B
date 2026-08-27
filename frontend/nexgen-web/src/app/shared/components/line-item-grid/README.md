# `LineItemGrid` — the keyboard-first editable grid

**Task:** `M2-C07`. **Specification:** [KB-051 § Data display](../../../../../../../docs/kb/frontend-new/design-system.md#data-display).
**Stack decision:** [ADR-007](../../../../../../../docs/kb/decisions/ADR-007-angular-stack.md)
§ Key rationales, Addendum (Q-83) — PrimeNG Table is the default; AG Grid is the pre-approved
fallback if a screen ever needs it, and that call belongs to the screen, not to this component.

It is an editing surface, not a calculator: cells, a keyboard model, row lifecycle operations,
validation display. It computes nothing about the document — see `line-item-grid.model.ts`'s
own doc comment and `line-item-grid.no-business-logic.spec.ts`, which enforces the boundary
rather than trusting a comment to hold it.

## The event contract

Derived from the _interaction shapes_ — not the logic — of `MfgPOUpsert.razor`'s line-entry
methods (re-verified 2026-08-27; absolute line numbers in
[KB-015](../../../../../../../docs/kb/architecture/frontend-architecture-existing.md#what-is-actually-inside-a-code-block)).
Each domain event carries a `respond(patch)` callback the grid awaits; each lifecycle event is
informational, because the grid has already applied it locally through `LineItemForm`.

| `rowEvent.type`      | Kind      | Triggered by                                                                                             | Existing Blazor evidence                                                                                                    | Server round trip?                                                           |
| -------------------- | --------- | -------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------- |
| `item-selected`      | Domain    | An `'typeahead'`/`'select'` cell column configured with `onEditCommitted: () => 'item-selected'` commits | `OnItemChanged`, `MfgPOUpsert.razor:2483` — resolves last price, HSN, UOM, labour cost, WO number                           | Yes — the caller resolves the full row and calls `respond(patch)`            |
| `quantity-changed`   | Domain    | A quantity column commits                                                                                | `OnQtyChange`/`UpdateQuantities`, `:2569`/`:2631` — clamps against already-transacted and quote-balance quantities          | Yes                                                                          |
| `rate-changed`       | Domain    | A rate/price column commits                                                                              | Implied by the same cascade as `item-selected` — a manually-typed rate still needs the server's tax/rounding context        | Yes                                                                          |
| `row-cancel-toggled` | Domain    | A cancel-flag column commits                                                                             | `OnItemCancelChanged`, `:3113` — refuses if the row is unsaved, the document is cancelled, or quantities have already moved | Yes                                                                          |
| `row-added`          | Lifecycle | `Enter` on a valid row, or the toolbar's **Add line**                                                    | `AddRow`, `:2706`, guarded by `ValidateLastRowAsync`, `:2686`                                                               | No — already applied                                                         |
| `row-duplicated`     | Lifecycle | `Ctrl+D`                                                                                                 | New capability — no Blazor equivalent                                                                                       | No                                                                           |
| `row-removed`        | Lifecycle | `Delete`, confirmed first if the row holds data                                                          | `DeleteAndResequenceAsync`, called `:3468`                                                                                  | No — the grid does not call the server itself; the caller's own listener may |
| `rows-reordered`     | Lifecycle | `Alt+↑`/`Alt+↓`                                                                                          | New capability — no Blazor equivalent                                                                                       | No                                                                           |
| `lines-pulled`       | Lifecycle | The caller calls the public `pullLines()` method after `RecordPickerDialog` (or any picker) resolves     | `DetailsModal.razor` today; `RecordPickerDialog` is M2-C06's replacement                                                    | No                                                                           |

**Not a `rowEvent` at all — a miss, recorded rather than assumed:** "a row being validated
before it may be committed" (one of the interaction shapes this task's Investigation
Requirements named). Validation reaches the grid through the `rowErrors` **input**, keyed by
`rowId`, not through an outbound event — the grid renders what it is given; it never asks
permission to accept a value.

**The `Slno` renumbering question:** answered in
[KB-015](../../../../../../../docs/kb/architecture/frontend-architecture-existing.md#what-is-actually-inside-a-code-block) —
persisted, but never a cross-document reference, so a caller may safely trigger renumbering
from `row-added`/`-duplicated`/`-removed`/`rows-reordered`. This component does not do it
itself: it is generic over `TLine` and does not assume a `SlNo`-shaped field exists.

## The `DECIMAL_PORT` gap

`decimal-cell.component.ts` and `integer-cell.component.ts` do **not** wrap
`app-currency-input`/`app-number-input` (M2-C04-02), even though instruction 8 of this task's
spec says to wrap an M2-C04-02 control. `shared/components/form/types.ts`'s `DECIMAL_PORT`
injection token has no real implementation anywhere in the app — only a test-only
`fake-decimal-port.ts` exists — so those two controls render read-only-ish without one
(`form/README.md`). Wiring the real port is a cross-cutting fix outside this task's scope
(every numeric control app-wide, not just this grid). These two cells wrap `app-text-input`
instead and do the decimal-safe parse/format themselves, directly against
`shared/utils/decimal` — the one place outside its own directory that module already licenses
an import from.

## Known gaps (2026-08-27)

See `docs/kb/execution/tasks/M2-C07.md` § Close-out for the full, current list. In short: the
200-row typing-**latency** millisecond figure needs a live browser session and was not
obtained; the keyboard jump-to-next-invalid-row shortcut was not built; clipboard paste and the
<768px responsive rendering are built but have no dedicated spec of their own.
