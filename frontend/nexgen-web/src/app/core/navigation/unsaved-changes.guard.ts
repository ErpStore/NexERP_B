import { DestroyRef, effect, inject, type Signal } from '@angular/core';
import type { CanDeactivateFn } from '@angular/router';

import { ConfirmDialogService } from '../../shared/components/overlay/confirm-dialog.service';

/**
 * M2-C03 — what a routed component exposes so {@link unsavedChangesGuard} and
 * {@link useBeforeUnloadGuard} can ask it, without either knowing anything about *what*
 * makes the form dirty. A `Signal<boolean>` doubles as a plain `() => boolean` for the
 * guard's point-in-time read, and as something `effect()` can react to for the
 * `beforeunload` half.
 */
export interface DirtyForm {
  readonly isDirty: Signal<boolean>;
}

/**
 * Replaces `UnsavedChangesModal.razor` — the in-app half. Blocks the router from
 * deactivating a dirty routed component until the caller either discards (via
 * M2-C04-03's confirm dialog) or cancels. A clean component is never asked.
 *
 * Covers in-app navigation and the browser back/forward buttons (both go through
 * `Router`/`Location`); it does **not** cover closing the tab or a hard reload — that is
 * {@link useBeforeUnloadGuard}'s job, because `CanDeactivateFn` never runs for either.
 */
export const unsavedChangesGuard: CanDeactivateFn<DirtyForm> = async (component) => {
  if (!component.isDirty()) {
    return true;
  }

  const confirmDialog = inject(ConfirmDialogService);
  const result = await confirmDialog.confirm({
    header: 'Discard unsaved changes?',
    message: 'You have unsaved changes on this page. Discard them and leave?',
    confirmLabel: 'Discard changes',
    cancelLabel: 'Stay on this page',
    destructive: true,
  });
  return result.confirmed;
};

/**
 * The `beforeunload` half: call once, from a dirty-form-owning component's constructor
 * (an injection context — `effect()` and `DestroyRef` both need one). The listener is
 * added and removed as `isDirty()` flips, never left registered on a clean form — a
 * `beforeunload` handler that always fires and only *sometimes* calls
 * `preventDefault()` still costs a native "leave site?" prompt suppression path in some
 * browsers, and a listener nobody needs is exactly the kind of thing that outlives its
 * component if forgotten.
 *
 * No confirm dialog here — `beforeunload`'s own browser-native prompt is the only UI a
 * page is allowed to show at this point; PrimeNG's dialog would not render before the
 * tab is already gone.
 */
export function useBeforeUnloadGuard(isDirty: Signal<boolean>): void {
  const destroyRef = inject(DestroyRef);
  let handler: ((event: BeforeUnloadEvent) => void) | null = null;

  const register = (): void => {
    if (handler) {
      return;
    }
    handler = (event: BeforeUnloadEvent) => {
      event.preventDefault();
      // Chrome (and historically most browsers) only shows the native prompt when
      // returnValue is set to a non-empty string; the string itself is never shown.
      event.returnValue = '';
    };
    window.addEventListener('beforeunload', handler);
  };
  const unregister = (): void => {
    if (!handler) {
      return;
    }
    window.removeEventListener('beforeunload', handler);
    handler = null;
  };

  effect(() => {
    if (isDirty()) {
      register();
    } else {
      unregister();
    }
  });
  destroyRef.onDestroy(unregister);
}
