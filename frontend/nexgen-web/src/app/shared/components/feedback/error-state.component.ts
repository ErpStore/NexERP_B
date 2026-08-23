import { ChangeDetectionStrategy, Component, input, output, signal } from '@angular/core';

/**
 * The failure panel: the server's message **verbatim**, the correlation id,
 * and Retry.
 *
 * `message` is `input.required<string>()` **with no default, deliberately**.
 * A default is how one generic sentence gets shipped on 140 screens: the argument is
 * forgotten, the fallback fills in, and support is left with a screenshot that
 * says nothing. Omitting it is a compile error, and `error-state.component.spec.ts`
 * proves that with `@ts-expect-error`.
 *
 * What to bind, from the shipped contract (`V.SMART.Api/Middleware/ApiProblems.cs`):
 *   - `message` <- the problem's **`title`**, not `detail`. A 409 business-rule
 *     refusal carries the service's own sentence in `title` (`:47-53`), so
 *     binding `detail` alone would render an empty panel for exactly the case
 *     the operator most needs to read.
 *   - `detail` <- the problem's `detail` when there is one; it is a secondary
 *     line, never a substitute.
 *   - `traceId` <- the `traceId` extension member every body carries (`:43`),
 *     also returned in the `X-Correlation-Id` header.
 *
 * Retry invokes a caller-supplied callback. This component knows nothing about
 * endpoints, `HttpClient` or what failed.
 */
@Component({
  selector: 'app-error-state',
  templateUrl: './error-state.component.html',
  styleUrl: './state.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ErrorStateComponent {
  readonly message = input.required<string>();
  readonly detail = input<string | undefined>(undefined);
  /** The `traceId` extension member. Rendered and copyable when present. */
  readonly traceId = input<string | undefined>(undefined);
  readonly title = input('Something went wrong');
  readonly showRetry = input(true);
  readonly retry = output<void>();

  private readonly copied = signal(false);
  readonly copyState = this.copied.asReadonly();

  /**
   * Support asks for the correlation id on every call, and retyping a GUID
   * from a screen is a poor use of an operator's minute.
   */
  copyTraceId(): void {
    void this.copy();
  }

  private async copy(): Promise<void> {
    const id = this.traceId();
    if (!id) {
      return;
    }
    try {
      await navigator.clipboard.writeText(id);
      this.copied.set(true);
    } catch {
      // No clipboard permission, or an insecure context. The id stays on
      // screen and selectable, which is the fallback that always works.
      this.copied.set(false);
    }
  }
}
