import { Directive, effect, inject, input, TemplateRef, ViewContainerRef } from '@angular/core';

import type { Right } from './auth.models';
import { PermissionService } from './permission.service';

/**
 * M2-C02 — `*appHasRight="'Sales Order'; right: 'create'"`. Renders its content only when
 * the right is present; an absent screen key renders nothing (deny-by-default, matching
 * `RightsHelper.cs`).
 *
 * **RENDERING ONLY — NOT A SECURITY CONTROL.** Hiding a button with this directive is a UX
 * affordance; the server re-checks the caller's `UserRight` rows on every request
 * regardless (ADR-004 §3). See `permission.service.ts`'s class doc comment for the full
 * rationale — repeated here because a directive is exactly the kind of thing a future
 * consumer reaches for without reading the service it wraps first.
 */
@Directive({ selector: '[appHasRight]' })
export class HasRightDirective {
  readonly #permissions = inject(PermissionService);
  readonly #templateRef = inject(TemplateRef<unknown>);
  readonly #viewContainerRef = inject(ViewContainerRef);

  /** The screen name, exactly as `Screens.ScreenName` — ordinal, case-sensitive. */
  readonly appHasRight = input.required<string>();
  readonly appHasRightRight = input<Right>('view');

  #hasView = false;

  constructor() {
    effect(() => {
      const granted = this.#permissions.has(this.appHasRight(), this.appHasRightRight());
      if (granted && !this.#hasView) {
        this.#viewContainerRef.createEmbeddedView(this.#templateRef);
        this.#hasView = true;
      } else if (!granted && this.#hasView) {
        this.#viewContainerRef.clear();
        this.#hasView = false;
      }
    });
  }
}
