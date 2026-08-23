/**
 * The overlay layer (M2-C04-03). Specification: KB-051 Overlays.
 *
 * Three rules govern everything exported here:
 *   1. **PrimeNG only** (ADR-007). Every surface is `p-dialog`, `p-drawer`,
 *      `p-confirmdialog`, `p-popover`, `[pTooltip]` or `p-contextmenu`.
 *   2. **No ERP business rule lives here.** `app-confirm-dialog` can require a
 *      reason (the BR-SO-003 *capability*); whether a reason is required, and
 *      whether the action is legal at all, is the server's decision.
 *   3. Open/closed state belongs to the calling screen. There is no global
 *      overlay store.
 */
export { ModalComponent } from './modal.component';
export type { ModalSize } from './modal.component';

export {
  DrawerComponent,
  DRAWER_DEFAULT_WIDTH,
  DRAWER_MAX_WIDTH,
  DRAWER_MIN_WIDTH,
  DRAWER_RESIZE_STEP,
} from './drawer.component';

export { ConfirmDialogComponent } from './confirm-dialog.component';
export { ConfirmDialogService, provideConfirmDialog } from './confirm-dialog.service';
export type { ConfirmRequest, ConfirmResult } from './confirm-dialog.service';

export { PopoverComponent } from './popover.component';
export { TooltipDirective } from './tooltip.directive';
export { ContextMenuComponent } from './context-menu.component';

export { focusFirstElementIn, OverlayFocusKeeper } from './overlay-focus';
