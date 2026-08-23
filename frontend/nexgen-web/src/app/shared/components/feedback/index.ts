/**
 * The feedback layer (M2-C04-03). Specification: KB-051 Feedback and
 * KB-051 State patterns.
 *
 * Three rules govern everything exported here:
 *   1. `ToastService` is the **only** touchpoint for PrimeNG's own message
 *      service. Call sites never import it - see the scan in
 *      `toast.service.spec.ts`.
 *   2. The server's message is rendered **verbatim** - a 409's `title` most of
 *      all (`ApiProblems.cs:47-53`). A generic catch-all sentence is
 *      unreachable: `app-error-state`'s `message` is a required input.
 *   3. First load is a shape-matched skeleton, never a spinner on a blank page.
 */
export {
  provideToast,
  TOAST_POLITE_PASS_THROUGH,
  ToastService,
  TOAST_INFO_LIFE_MS,
  TOAST_SUCCESS_LIFE_MS,
  TOAST_WARN_LIFE_MS,
} from './toast.service';

export { InlineAlertComponent } from './inline-alert.component';
export type { AlertSeverity } from './inline-alert.component';

export { BusyOverlayComponent } from './busy-overlay.component';
export { SkeletonComponent } from './skeleton.component';
export { SkeletonTableComponent } from './skeleton-table.component';
export { SkeletonFormComponent } from './skeleton-form.component';
export { ProgressBarComponent } from './progress-bar.component';

export { EmptyStateComponent } from './empty-state.component';
export type { EmptyStateVariant } from './empty-state.component';
export { ErrorStateComponent } from './error-state.component';
export { PermissionDeniedStateComponent } from './permission-denied-state.component';
