import {
  ApplicationConfig,
  ErrorHandler,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
} from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideTranslateLoader, provideTranslateService } from '@ngx-translate/core';
import { providePrimeNG } from 'primeng/config';

import { routes } from './app.routes';
import { bootstrapApp } from './core/api/app-bootstrap';
import { authInterceptor } from './core/auth/auth.interceptor';
import { GlobalErrorHandler } from './core/errors/global-error-handler';
import { correlationInterceptor } from './core/http/correlation.interceptor';
import { errorInterceptor } from './core/http/error.interceptor';
import { tenantInterceptor } from './core/http/tenant.interceptor';
import { InMemoryTranslateLoader } from './core/i18n/in-memory-translate-loader';
import { NexGenThemeOptions } from './core/theme/theme.preset';
import { provideConfirmDialog } from './shared/components/overlay/confirm-dialog.service';
import { provideToast } from './shared/components/feedback/toast.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    { provide: ErrorHandler, useClass: GlobalErrorHandler },

    provideRouter(routes, withComponentInputBinding()),

    // M2-C02 — order matters and is deliberate: correlation and tenant headers must be on
    // the request before auth decides whether to retry it; auth's single-flight refresh
    // must run before error normalises whatever comes back (success or the final failure).
    provideHttpClient(
      withInterceptors([
        correlationInterceptor,
        tenantInterceptor,
        authInterceptor,
        errorInterceptor,
      ]),
    ),

    // Runtime configuration must be resolved before the app renders; a missing API base URL
    // aborts bootstrap rather than defaulting to someone's laptop. M2-C02 extended this to
    // also wire ApiConfiguration.rootUrl (a real, pre-existing gap — see bootstrap.ts's doc
    // comment) and to attempt the silent auth bootstrap, all three sequenced in one
    // initializer because Angular runs separate provideAppInitializer factories
    // concurrently, not in this array's order.
    provideAppInitializer(bootstrapApp),

    // PrimeNG themed from src/styles/tokens.css: every preset value is a
    // var(--token) reference, and darkModeSelector is the same
    // [data-theme="dark"] attribute ThemeService writes (M2-C04-01, Q-67).
    providePrimeNG({ theme: NexGenThemeOptions }),

    // One toast host and one confirm-dialog host for the whole application
    // (both live in app.component.html). Screens reach the toast layer
    // through shared/components/feedback/toast.service.ts, which is the only
    // file that imports PrimeNG's message service, and confirmation through
    // shared/components/overlay/confirm-dialog.service.ts (M2-C04-03).
    provideToast(),
    provideConfirmDialog(),

    provideTranslateService({
      lang: 'en',
      fallbackLang: 'en',
      loader: provideTranslateLoader(InMemoryTranslateLoader),
    }),
  ],
};
