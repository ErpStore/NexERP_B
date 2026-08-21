import { inject, Injectable, InjectionToken, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

import { environment } from '../../../environments/environment';

/** The shape of the deployment-supplied runtime configuration document. */
export interface AppConfig {
  /** Base URL of the V.SMART API. No default exists anywhere in this source tree. */
  readonly apiBaseUrl: string;
}

/** Static build identity. Not configuration - it ships with the bundle. */
export const APP_INFO = {
  name: 'NexGen ERP',
  version: '0.1.0',
} as const;

export const APP_CONFIG = new InjectionToken<AppConfig>('APP_CONFIG');

@Injectable({ providedIn: 'root' })
export class AppConfigService {
  private readonly http = inject(HttpClient);
  private readonly config = signal<AppConfig | null>(null);

  /**
   * The API base URL. Throws rather than guessing: there is no fallback host in
   * this application, by design (KB-050, ADR-007:182-184).
   */
  get apiBaseUrl(): string {
    const current = this.config();
    if (current === null) {
      throw new Error('AppConfigService was read before loadAppConfig() completed.');
    }
    return current.apiBaseUrl;
  }

  async load(): Promise<void> {
    let raw: Partial<AppConfig>;
    try {
      raw = await firstValueFrom(this.http.get<Partial<AppConfig>>(environment.configUrl));
    } catch (cause) {
      throw new Error(
        `Runtime configuration could not be loaded from '${environment.configUrl}'. ` +
          'The application has no built-in API host and cannot start without it.',
        { cause },
      );
    }

    const apiBaseUrl = raw?.apiBaseUrl?.trim();
    if (!apiBaseUrl) {
      throw new Error(
        `'apiBaseUrl' is missing or blank in '${environment.configUrl}'. ` +
          'Set it to the origin of the V.SMART API. There is no default.',
      );
    }

    this.config.set({ apiBaseUrl });
  }
}

/**
 * Startup loader. Registered through `provideAppInitializer`, so a missing or
 * malformed configuration document aborts bootstrap loudly instead of letting
 * the app run against an unknown backend.
 */
export function loadAppConfig(): Promise<void> {
  return inject(AppConfigService).load();
}
