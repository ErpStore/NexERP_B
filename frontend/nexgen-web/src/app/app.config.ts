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

    provideTranslateService({
      lang: 'en',
      fallbackLang: 'en',
      loader: provideTranslateLoader(InMemoryTranslateLoader),
    }),
  ],
};
