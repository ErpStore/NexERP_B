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
import { loadAppConfig } from './core/config/app-config';
import { GlobalErrorHandler } from './core/errors/global-error-handler';
import { InMemoryTranslateLoader } from './core/i18n/in-memory-translate-loader';
import { NexGenThemeOptions } from './core/theme/theme.preset';
import { provideConfirmDialog } from './shared/components/overlay/confirm-dialog.service';
import { provideToast } from './shared/components/feedback/toast.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    { provide: ErrorHandler, useClass: GlobalErrorHandler },

    provideRouter(routes, withComponentInputBinding()),

    // The interceptor array is intentionally empty: auth, tenant, correlation
    // and error interceptors are M2-C02's, not this task's.
    provideHttpClient(withInterceptors([])),

    // Runtime configuration must be resolved before the app renders; a missing
    // API base URL aborts bootstrap rather than defaulting to someone's laptop.
    provideAppInitializer(loadAppConfig),

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
