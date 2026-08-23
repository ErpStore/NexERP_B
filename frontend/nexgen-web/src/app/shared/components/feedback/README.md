# Feedback

`ToastService` · `app-inline-alert` · `app-busy-overlay` · `app-skeleton` (+ `-table`,
`-form`) · `app-progress-bar` · `app-empty-state` · `app-error-state` ·
`app-permission-denied-state`

Built for `M2-C04-03` over **PrimeNG only** ([ADR-007](../../../../../../docs/kb/decisions/ADR-007-angular-stack.md)).
Specification: KB-051 §Feedback and §State patterns.

**No ERP business rule lives in this directory.** Nothing here performs a request, decides a
permission or computes anything. `app-error-state` and `app-inline-alert` _render_ a
normalised problem body the caller supplies.

## The state vocabulary

| State                      | Component                                                                        |
| -------------------------- | -------------------------------------------------------------------------------- |
| Loading, first time        | `app-skeleton-table` / `app-skeleton-form` — **never a spinner on a blank page** |
| Loading, refetch           | `app-progress-bar` (indeterminate), previous data left on screen                 |
| Saving                     | `app-busy-overlay` scoped to the region — saving **never blocks the page**       |
| Empty, nothing created yet | `app-empty-state` `variant="no-data"` + a create action                          |
| Empty, filtered out        | `app-empty-state` `variant="filtered"` + Clear filters                           |
| Error                      | `app-error-state` — server message, `traceId`, Retry                             |
| Permission denied          | `app-permission-denied-state` — names the missing right, no retry                |

The two empty variants are not interchangeable. Offering "New sales order" to someone whose
date filter is simply wrong wastes their time; offering "Clear filters" on a fresh tenant is
nonsense.

## Toasts

`ToastService` is the **only** file that imports PrimeNG's message service — asserted by a
scan in `toast.service.spec.ts` and by the task's own `git grep`. Call sites use
`toast.success()`, `toast.info()`, `toast.warn()`, `toast.error()`.

- success and info dismiss themselves after **4 s**; warn after 8 s;
- **error is sticky** with an explicit dismiss, because an error a screen-reader user has to
  race a timer to hear is an error they miss;
- announcement is **polite**: PrimeNG hard-codes `aria-live="assertive"` on toast items, and
  `TOAST_POLITE_PASS_THROUGH` (bound in `app.component.html`) overrides it.

**A toast is never enough on its own** for something the user must act on. It scrolls past,
it is invisible to anyone who looked away, and it is gone. Put that message next to the thing
that failed — `app-inline-alert` in a form, `app-error-state` for a whole surface — and use
the toast only as the additional, transient signal.

## The server's words

A 409 business-rule refusal carries the domain team's own sentence in the problem body's
**`title`**, not `detail` (`V.SMART/V.SMART.Api/Middleware/ApiProblems.cs:47-53`). Bind
`title` to `app-error-state`'s `message` or to `app-inline-alert`'s `message` and render it
**verbatim**: never reword, prefix, translate or truncate. Those strings are product UX.

Every problem body also carries a `traceId` extension member (`ApiProblems.cs:43`), the same
value as the `X-Correlation-Id` response header. Pass it to `app-error-state`, which renders
it and offers a copy control — support asks for it on every call.

`app-error-state`'s `message` is `input.required<string>()` **with no default**, deliberately:
a default is exactly how one generic sentence ends up shipped on 140 screens.

## Busy, not blocked

`app-busy-overlay` blocks its own region by default. `fullPage` exists for the rare operation
that genuinely owns the window — a tenant switch, a session end — and should be justified at
the call site. It announces busy-ness and never steals focus: moving focus into a spinner
strands whoever was typing. This replaces `ProcessingOverlay.razor`, which covers the whole
page.

## Measured PrimeNG 22.1.0 behaviour

- `p-message` hosts `role="alert" aria-live="polite"`; `app-inline-alert` raises that to
  `assertive` for errors only, through `[pt]`, rather than nesting a second live region.
- `p-progressbar` writes `aria-level="42%"`, which is neither allowed on `role="progressbar"`
  nor a valid value; axe rates both critical. Cleared through `[pt]`.
- `p-progressbar`'s `value` input runs through `numberAttribute`, so binding `undefined`
  renders `aria-valuenow="NaN"`. The indeterminate branch leaves the input unbound instead.

## Tokens

No colour or spacing literal appears here — everything is `var(--token)` from
`src/styles/tokens.css`, enforced by `core/theme/no-raw-colour.spec.ts` and by ESLint.
