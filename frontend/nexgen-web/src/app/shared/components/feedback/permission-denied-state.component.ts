import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/**
 * "You do not have the *edit* right for the *Sales Order* screen."
 *
 * **Purely presentational.** It takes `screen` and `right` as inputs and reads
 * no service - deliberately, and asserted by an import scan in the spec. A
 * design-system component that injected the permission service would start to
 * look like a security control; it is not one. Evaluation is M2-C02's guard
 * and directive, and enforcement is the server's (ADR-004): the API answers
 * `403` with `screen` and `right` in the problem body
 * (`V.SMART.Api/Middleware/ApiProblems.cs:89-104`), which is what a caller
 * binds here.
 *
 * **No retry, and no navigation away.** Retrying does not grant a right, and
 * bouncing the user elsewhere hides which right is missing - the one fact that
 * lets them ask an administrator for the correct thing.
 */
@Component({
  selector: 'app-permission-denied-state',
  templateUrl: './permission-denied-state.component.html',
  styleUrl: './state.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PermissionDeniedStateComponent {
  /** The screen name exactly as the server names it, e.g. "Sales Order". */
  readonly screen = input.required<string>();
  /** The missing right, e.g. "edit". */
  readonly right = input.required<string>();
  readonly title = input('You do not have access to this');
}
