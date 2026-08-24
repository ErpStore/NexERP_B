# Overlays

`app-modal` · `app-drawer` · `app-confirm-dialog` · `app-popover` · `[appTooltip]` ·
`app-context-menu`

Built for `M2-C04-03` over **PrimeNG only** ([ADR-007](../../../../../../docs/kb/decisions/ADR-007-angular-stack.md)).
Specification: KB-051 §Overlays.

**No ERP business rule lives in this directory.** `app-confirm-dialog` can _collect_ a
mandatory reason — the capability BR-SO-003 needs — but it never decides that one is
required, never checks downstream transactions (`IsPoTransactionsMatchedAsync`,
`CanSalesOrderItemCancelCheckAsync`) and never reverts a quantity. Those live in
`V.SMART.Shared` and stay there. A screen passes `reasonRequired`; the server decides
whether the cancellation is legal.

## Modal or drawer?

> KB-051 §Do not — "Use modals for anything that needs the list behind it — use a drawer."

| Situation                                                                 | Use                |
| ------------------------------------------------------------------------- | ------------------ |
| A question that must be answered before anything else                     | `app-modal`        |
| A short form whose context is the page you came from                      | `app-modal`        |
| Record detail an operator reconciles **against the list it came from**    | `app-drawer`       |
| Anything the user will compare, item by item, with what is behind it      | `app-drawer`       |
| A small secondary surface anchored to a control (column chooser, filters) | `app-popover`      |
| Row or record actions                                                     | `app-context-menu` |

If you cannot decide, ask whether the operator will look back at the list while the overlay
is open. If yes, it is a drawer.

## Keyboard model

| Component            | Model                                                                                                                                                                                                                                                                                                                                     |
| -------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `app-modal`          | Focus moves to the first control on open, is trapped, `Esc` closes, focus returns to the exact invoking element, background scroll is locked while it is open.                                                                                                                                                                            |
| `app-drawer`         | The same, plus the resize handle: `←` widens, `→` narrows, `Shift` doubles the step, `Home`/`End` jump to the limits. Width is remembered per `persistKey` in local storage.                                                                                                                                                              |
| `app-confirm-dialog` | The modal contract — focus in on open, trapped, focus back on the exact invoking element on close, background scroll locked — plus: `Esc`, the backdrop and the close icon all map to **cancel**, never to confirm. Confirm is disabled until a required reason is non-empty after trim. The `destructive` variant changes emphasis only. |
| `app-popover`        | The trigger is your own button, so `Enter`/`Space` already open it. `Esc` closes and restores focus.                                                                                                                                                                                                                                      |
| `[appTooltip]`       | Opens on **focus** as well as hover; `Esc` dismisses.                                                                                                                                                                                                                                                                                     |
| `app-context-menu`   | A visible trigger button, plus `Shift+F10`. Arrow keys move, `Esc` closes and restores focus.                                                                                                                                                                                                                                             |

**A tooltip must never be the only place a piece of information exists.** It is unreachable
on touch, absent from print, and gone the moment focus moves. If the operator needs it to
finish the task, put it in `app-form-field`'s `hint` or in the body copy.

## Usage

```ts
// Confirmation, with the BR-SO-003 reason capability.
private readonly confirm = inject(ConfirmDialogService);

async cancelLine(): Promise<void> {
  const { confirmed, reason } = await this.confirm.confirm({
    header: 'Cancel line',
    message: 'Cancel line 3 of SO-0001?',
    reasonRequired: true,     // the *screen* mirrors a server rule; it does not own it
    destructive: true,
  });
  if (confirmed) {
    // POST the cancellation, reason included. The server decides.
  }
}
```

```html
<button type="button" (click)="detailOpen = true">Open SO-0001</button>
<app-drawer title="Sales order SO-0001" persistKey="sales-order" [(visible)]="detailOpen">
  <!-- record detail; the list is still behind it -->
</app-drawer>
```

Open/closed state belongs to the calling screen. There is **no global overlay store**: across
~140 screens a registry is how modal state stops being traceable to the code that opened it.

## Measured PrimeNG 22.1.0 behaviour

Recorded because each of these was found by test, not assumed, and each would otherwise be
rediscovered:

- **`Drawer` and `Esc`.** `Drawer.onKeyDown` answers `Escape` with `hide(false)`, which tears
  down the modality but never clears `visible`, so the drawer stays on screen; the
  document-level fallback keys off the deprecated `event.which`; and
  `unbindDocumentEscapeListener()` calls itself. `app-drawer` sets `[closeOnEscape]="false"`
  and implements `Esc` itself.
- **`Drawer` and `(onHide)`.** It emits only from `close()` — the close icon and the mask. A
  programmatic `visible = false` never emits, so `app-drawer` drives close from the signal.
- **`Drawer` role.** The root is hard-coded `role="complementary"`; `app-drawer` sets
  `role="dialog"`, `aria-modal` and `aria-label` on it after show.
- **Focus on open.** PrimeNG decides what is focusable partly from layout, and declines to
  move focus where there is none (jsdom, for one). `overlay-focus.ts` moves focus in if
  PrimeNG has not.
- **`p-confirmdialog` never moves focus in at all.** It hard-codes `[focusOnShow]="false"` on
  the `p-dialog` it renders and depends on `pAutoFocus` sitting on _its own_ accept/reject
  buttons; a custom `#footer` replaces those, so nothing takes focus — and PrimeNG's focus
  trap cannot help, because a trap only holds focus that is already inside. It also exposes
  no `(onShow)`, and `p-dialog` **moves** its wrapper to `document.body`
  (`appendContainer()`) as the enter transition starts, which blurs anything focused before
  the move. `app-confirm-dialog` therefore focuses from `afterEveryRender`, which runs again
  after the move; `focusFirstElementIn` is a no-op once focus is inside. Its `[focusTrap]`
  input is **not** forwarded to the inner dialog either — the trap that works is the inner
  `p-dialog`'s own default, which is why the trap is asserted by test rather than assumed.
- **Accessible names.** The dialog and drawer close icons have no name unless
  `closeAriaLabel` / `ariaCloseLabel` is set; `p-confirmdialog` does **not** forward
  `closeAriaLabel` to the dialog it renders, so the name goes through `[pt]`.
- **`p-contextmenu` ARIA.** Every `role="menuitem"` carries `aria-level`, `aria-setsize` and
  `aria-posinset`, which ARIA allows on a treeitem and not a menuitem; axe rates it critical.
  Cleared through `[pt]`.

## Deliberate departure from `BsModal.razor`

`V.SMART/V.SMART.Shared/Components/BsModal.razor:76-93` lets Confirm be pressed with an empty
reason and answers with a toastr warning — "Please enter a valid reason before confirming." —
then returns without confirming. `app-confirm-dialog` **disables** Confirm instead, as the
task specifies. Same outcome; the state is visible before the click rather than announced
after it. The trade is that a screen-reader user meets a disabled button rather than a
message, so the reason field is marked required and carries its own error text.

## Tokens

No colour or spacing literal appears here — everything is `var(--token)` from
`src/styles/tokens.css`, enforced by `core/theme/no-raw-colour.spec.ts` and by ESLint. Two
gaps in the token layer are **reported, not filled** (M2-C04-01 owns it): there is no
z-index/stacking token, so overlays rely on PrimeNG's own layering; and the theme preset has
no `mask` key, so the dialog and drawer backdrop keeps Aura's default.
