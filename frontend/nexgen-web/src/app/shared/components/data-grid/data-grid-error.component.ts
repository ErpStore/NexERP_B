import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

import { ErrorStateComponent } from '../feedback/error-state.component';
import { InlineAlertComponent } from '../feedback/inline-alert.component';
import { PermissionDeniedStateComponent } from '../feedback/permission-denied-state.component';
import type { ProblemDetailsLike } from '../form/server-validation';

/**
 * The RFC 7807 members this component reads, on top of the subset
 * `form/server-validation.ts:22` already declared for the form layer. The type
 * is **extended, not re-declared**, so there is exactly one description of the
 * error contract in the workspace.
 *
 * Every member below was read from the produced middleware, not from a plan:
 *   - `traceId` - `V.SMART/V.SMART.Api/Middleware/ApiProblems.cs:43`;
 *   - `screen` / `right` - `ApiProblems.cs:89-104`, the 403 screen-right body;
 *   - `title` carrying a 409's business-rule sentence verbatim -
 *     `ApiProblems.cs:47-53`.
 */
export interface GridProblemDetails extends ProblemDetailsLike {
  readonly type?: string;
  readonly instance?: string;
  /** The correlation id. Also the `X-Correlation-Id` header (`CorrelationId.cs:25`). */
  readonly traceId?: string;
  /** 403 screen-right refusals only. */
  readonly screen?: string;
  /** 403 screen-right refusals only. */
  readonly right?: string;
}

/**
 * Shown only when the failure carried **no** server message at all - a network
 * drop, a CORS refusal, an aborted request. It is never substituted for a
 * message the server did send: that substitution is the defect KB-051
 * principle 6 exists to stop.
 */
export const GRID_ERROR_FALLBACK_MESSAGE = 'The request could not be completed.';

/**
 * Turn whatever reached the grid into the problem body the states render.
 *
 * **Why this exists here, and what should remove it.** M2-C05-03's
 * specification says to consume the `ProblemDetails` the HTTP error
 * interceptor (M2-C02) already normalised and not to re-parse it. That
 * interceptor does not exist yet - `core/http/` is empty and `app.config.ts`
 * registers `withInterceptors([])` - and M2-C02 is Blocked. So the choice was
 * between rendering a raw `HttpErrorResponse` (which cannot be branched on) and
 * normalising once, here. This is the *single* normalisation point in the grid;
 * when M2-C02 lands it should call this function or replace it, **not** add a
 * second one. Recorded as Q-94 in `docs/kb/open-questions.md`.
 *
 * Nothing here rewrites, translates, prefixes or truncates a server string. The
 * only value this function invents is {@link GRID_ERROR_FALLBACK_MESSAGE}, and
 * only when the server sent no words at all.
 */
export function toGridProblem(error: unknown): GridProblemDetails | null {
  if (error === null || error === undefined) {
    return null;
  }
  if (error instanceof HttpErrorResponse) {
    const body = asProblem(error.error);
    // `status` from the body when it is there (it always is - `ApiProblems.Create`
    // sets it), and from the response otherwise, so a non-problem+json failure
    // still lands in the right branch.
    return { ...(body ?? {}), status: body?.status ?? error.status };
  }
  return asProblem(error) ?? {};
}

function asProblem(value: unknown): GridProblemDetails | null {
  return typeof value === 'object' && value !== null ? value : null;
}

/**
 * The grid's error surface: one server failure, rendered as the thing it
 * actually is.
 *
 * Three branches, because three different failures need three different
 * responses from the operator (KB-050 Error handling):
 *
 *  - **403** - the permission-denied state, **inline**, naming the missing
 *    screen right. Not a redirect: bouncing the user elsewhere hides the one
 *    fact that lets them ask an administrator for the right thing.
 *  - **409** - an inline alert carrying `title` **verbatim**. These sentences
 *    are product UX written for ERP operators
 *    (`MfgPoService.cs:488`, BR-SO-001); the component's whole obligation is
 *    fidelity of transport. No interpolation, no translation pipe - this
 *    workspace ships `@ngx-translate/core`, and a lookup that misses falls back
 *    to its key, which would replace the message with a token.
 *  - **anything else** - `app-error-state` with the message, the detail, the
 *    copyable `traceId` and Retry.
 */
@Component({
  selector: 'app-data-grid-error',
  templateUrl: './data-grid-error.component.html',
  // Layout shared with `app-data-grid-states`; the visuals belong to the primitives.
  styleUrl: './data-grid-states.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ErrorStateComponent, InlineAlertComponent, PermissionDeniedStateComponent],
})
export class DataGridErrorComponent {
  /** Whatever failed. An `HttpErrorResponse`, a parsed problem body, or neither. */
  readonly error = input<unknown>(null);
  readonly showRetry = input(true);
  readonly retry = output<void>();

  readonly problem = computed(() => toGridProblem(this.error()));
  readonly status = computed(() => this.problem()?.status ?? 0);

  /**
   * A 403 renders the permission-denied state only when the server named the
   * screen and the right. The account-gate 403s (`ApiProblems.cs:69-75`) carry
   * neither, and inventing them would put words in the server's mouth.
   */
  readonly isPermissionDenied = computed(() => {
    const problem = this.problem();
    return this.status() === 403 && !!problem?.screen && !!problem.right;
  });

  readonly isBusinessRule = computed(() => this.status() === 409);

  /** The screen name, as the server named it. Only read when {@link isPermissionDenied}. */
  readonly screen = computed(() => this.problem()?.screen ?? '');
  readonly right = computed(() => this.problem()?.right ?? '');

  /** `title`, exactly as received. Never reworded. */
  readonly message = computed(() => this.problem()?.title ?? GRID_ERROR_FALLBACK_MESSAGE);
  readonly detail = computed(() => this.problem()?.detail);
  readonly traceId = computed(() => this.problem()?.traceId);

  /**
   * A refusal interrupts; a transport failure waits for a pause (KB-051). The
   * primitives keep their own roles - overriding one would be a change request
   * against M2-C04-03 - so this only sets the urgency of the region around them.
   */
  readonly liveness = computed<'assertive' | 'polite'>(() =>
    this.status() === 403 || this.status() === 409 ? 'assertive' : 'polite',
  );
}
