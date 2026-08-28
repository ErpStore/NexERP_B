import type { CanDeactivateFn } from '@angular/router';

import type { DocumentEditorHost } from './document-editor.component';

/**
 * The dirty-form navigation guard (M2-C08-01) - the functional replacement for
 * `UnsavedChangesModal.razor`.
 *
 * ```ts
 * { path: ':id', component: SalesOrderEditorComponent, canDeactivate: [unsavedChangesGuard] }
 * ```
 *
 * **This is a behaviour improvement, recorded as such rather than presented as
 * preservation.** The existing guard is a single global JS boolean set from any
 * input event on `document.body` (`wwwroot/js/navigationGuard.js:1-39`,
 * initialised once at `MainLayout.razor:232`), and it is read by exactly one
 * call site: `SmartBackButton.razor:64`. Navigating by the menu, the browser
 * back button or a typed URL is unguarded today, and there is no `beforeunload`
 * listener anywhere in it. A router `CanDeactivateFn` covers every in-app
 * navigation, and the editor registers `beforeunload` while dirty for the rest.
 *
 * The prompt itself belongs to the editor, not to this function: the choice is
 * Save / Discard / Stay - three outcomes - which `app-confirm-dialog` does not
 * model (INV-006's M2-C04-03 amendment), so the editor composes `app-modal`.
 *
 * The parameter is structurally typed, so this guard never imports a feature
 * component and any screen that can answer the question can use it.
 */
export const unsavedChangesGuard: CanDeactivateFn<DocumentEditorHost> = (component) =>
  component.canDeactivateDocument();
