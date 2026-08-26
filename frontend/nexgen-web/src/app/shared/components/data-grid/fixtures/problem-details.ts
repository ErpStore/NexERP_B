/**
 * **Test fixtures only.** Nothing here is bundled into the application.
 *
 * Every body below is the shape `V.SMART.Api` actually produces, read from the
 * middleware rather than from a plan:
 *
 *   - the correlation id is the extension member **`traceId`**
 *     (`V.SMART/V.SMART.Api/Middleware/ApiProblems.cs:43`), also returned as the
 *     `X-Correlation-Id` response header (`Middleware/CorrelationId.cs:25`);
 *   - a **409** carries the service's own sentence in **`title`**, verbatim
 *     (`ApiProblems.cs:47-53`, reached from `Middleware/ProblemResults.cs:44`);
 *   - a **403** screen-right refusal adds the extensions `screen` and `right`
 *     alongside a composed `detail` (`ApiProblems.cs:89-104`);
 *   - a **500** carries a constant title and `traceId` only, in every
 *     environment (`ApiProblems.cs:134-139`) - there is no exception message to
 *     show, and a test that expects one would be asserting a fiction.
 */

import type { GridProblemDetails } from '../data-grid-error.component';

/**
 * BR-SO-001, **byte-for-byte** as the domain team wrote it:
 * `V.SMART/V.SMART.Shared/BusinessLayer/BusinessService/SalesService/MfgPoService.cs:488`
 * (re-verified 2026-08-26). Do not reflow, retype or "tidy" this string - the
 * whole point of the test that uses it is that the bytes survive the round trip.
 */
export const BR_SO_001_DELETE_MESSAGE =
  'Cannot delete this Sales Order as a Sales DC transaction exists.';

/** BR-SO-001's sibling refusal, `MfgPoService.cs:598`. */
export const BR_SO_001_CANCEL_MESSAGE = 'Cannot cancel this Item as a Sales DC transaction exists.';

/** The 409 the export endpoint answers above its row ceiling (`CurrencyExcelController.cs:112-116`). */
export const EXPORT_ROW_LIMIT_MESSAGE =
  'The filtered set contains 12345 rows, which exceeds the export limit of 10000. Narrow the filter and export again.';

export const TEST_TRACE_ID = '00-8f1c0d2b4a6e5f70a1b2c3d4e5f60718-2a3b4c5d6e7f8091-01';

/** A business-rule refusal exactly as `ProblemResults.BusinessRuleProblem` serialises it. */
export function businessRuleProblem(
  message: string = BR_SO_001_DELETE_MESSAGE,
): GridProblemDetails {
  return {
    type: 'https://api.v-smart.local/problems/business-rule',
    title: message,
    status: 409,
    instance: '/api/v1/currencies/export',
    traceId: TEST_TRACE_ID,
  };
}

/** A screen-right refusal, `ApiProblems.ScreenRightDenied` (`:89-104`). */
export function screenRightDeniedProblem(screen = 'Currency', right = 'View'): GridProblemDetails {
  return {
    type: 'https://api.v-smart.local/problems/screen-right-denied',
    title: 'Screen right denied.',
    detail: `You do not have the '${right}' right for the '${screen}' screen.`,
    status: 403,
    instance: '/api/v1/currencies',
    screen,
    right,
    traceId: TEST_TRACE_ID,
  };
}

/** The unhandled-server-fault body, `ApiProblems.Unhandled` (`:134-139`). */
export function unhandledProblem(): GridProblemDetails {
  return {
    type: 'https://api.v-smart.local/problems/unhandled',
    title: 'An unexpected error occurred.',
    status: 500,
    instance: '/api/v1/currencies',
    traceId: TEST_TRACE_ID,
  };
}
