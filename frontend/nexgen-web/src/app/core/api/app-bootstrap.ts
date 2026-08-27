import { inject } from '@angular/core';

import { AppConfigService } from '../config/app-config';
import { AuthService } from '../auth/auth.service';
import { ApiConfiguration } from './generated/api-configuration';

/**
 * M2-C02 — the single app-start initializer, registered as this workspace's only
 * `provideAppInitializer` in `app.config.ts`.
 *
 * **A real gap found and fixed in passing, not just used.** `ApiConfiguration.rootUrl`
 * (`core/api/generated/api-configuration.ts`) — what every generated service call actually
 * targets — was never wired to anything. It defaulted to `''`, silently: every generated
 * client call before this task would have resolved against the current page's own origin,
 * never against the runtime-configured API host `AppConfigService` loads. Nothing had called
 * a generated service from a running route yet (`GET /api/v1/me`, consumed here, is the
 * first), so nothing had exercised the gap.
 *
 * **Why one combined initializer, not two.** Angular runs every `provideAppInitializer`
 * factory **concurrently** (`Promise.all`), not in provider-array order — registering
 * "load config" and "wire rootUrl + bootstrap auth" as two separate initializers would race,
 * and `AuthService.bootstrap()`'s first HTTP call could fire before `rootUrl` is set. This
 * function sequences all three steps with real `await`s instead.
 */
export async function bootstrapApp(): Promise<void> {
  const appConfig = inject(AppConfigService);
  const apiConfig = inject(ApiConfiguration);
  const auth = inject(AuthService);

  await appConfig.load();
  apiConfig.rootUrl = appConfig.apiBaseUrl;
  await auth.bootstrap();
}
