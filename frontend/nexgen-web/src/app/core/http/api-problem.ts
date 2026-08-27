/**
 * M2-C02 — the one `ApiProblem` type, per KB-050 §Error handling: *"the single place that
 * parses a problem body into a typed `ApiProblem`. Nothing else parses error bodies."*
 *
 * Not the generated `ProblemDetails`/`ValidationProblemDetails` models
 * (`core/api/generated/models/`) — this file lives outside `core/api/**`, and
 * `eslint.config.js`'s `bannedGeneratedClientImports` rule forbids importing the generated
 * client from anywhere else. This is also, deliberately, a fuller shape than either
 * generated type alone: every field ASP.NET's `ProblemDetails.Extensions` dictionary can
 * carry that KB-050's contract treats as load-bearing (`traceId`, and, for a 403
 * screen-right-denied body, `screen`/`right`) is named here as a first-class optional field,
 * not left behind an index signature.
 *
 * **A pre-existing convergence gap, disclosed rather than silently fixed.** Three earlier
 * tasks each declared their own near-identical subset of this shape before this interceptor
 * existed to normalise once: `ProblemDetailsLike`
 * (`shared/components/form/server-validation.ts`), `GridProblemDetails`
 * (`shared/components/data-grid/data-grid-error.component.ts`, which already extends
 * `ProblemDetailsLike`), and `RecordPickerProblem`
 * (`shared/components/record-picker-dialog/record-picker-dialog.model.ts`, a fresh copy).
 * `data-grid-error.component.ts`'s own doc comment names this exact convergence as Q-94
 * (`docs/kb/open-questions.md`) and says the eventual owner "should call this function or
 * replace it, not add a second one." **Not resolved by this task** — none of those three
 * files is in `M2-C02`'s Files Expected to Change, and re-pointing them is a real, separate
 * change to `data-grid`, `form` and `record-picker-dialog`, not an auth-layer one. Recorded
 * here so a future session doing that work finds this note instead of re-deriving Q-94.
 */
export interface ApiProblem {
  readonly status?: number;
  readonly title?: string;
  readonly detail?: string;
  readonly type?: string;
  readonly instance?: string;
  readonly traceId?: string;
  /** Present only on a 403 `screen-right-denied` body. */
  readonly screen?: string;
  /** Present only on a 403 `screen-right-denied` body. */
  readonly right?: string;
  /** Present only on a 400 validation-failed body — keyed by field name. */
  readonly errors?: Readonly<Record<string, readonly string[] | string>>;
}

/** `type` URIs this workspace's API actually issues (`V.SMART.Api/Middleware/ProblemTypes.cs`).
 * Branch on these, never on `title` — `title` is human prose and is not a stable contract. */
export const API_PROBLEM_TYPES = {
  validationFailed: 'validation-failed',
  unauthenticated: 'unauthenticated',
  invalidToken: 'invalid-token',
  screenRightDenied: 'screen-right-denied',
  trialExpired: 'trial-expired',
  deviceNotRecognised: 'device-not-recognised',
  platformNotAllowed: 'platform-not-allowed',
  tenantUnresolved: 'tenant-unresolved',
  businessRule: 'business-rule',
  notFound: 'not-found',
  payloadTooLarge: 'payload-too-large',
  unhandled: 'unhandled',
} as const;

/** True when `problem.type` ends with the given `ProblemTypes` suffix — the base authority
 * (`https://api.v-smart.local/problems/`) is a fixed prefix this only ever needs the tail of. */
export function isProblemType(problem: ApiProblem | null | undefined, suffix: string): boolean {
  return !!problem?.type && problem.type.endsWith(`/${suffix}`);
}
