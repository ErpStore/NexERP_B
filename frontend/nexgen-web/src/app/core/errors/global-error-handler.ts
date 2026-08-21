import { ErrorHandler, Injectable } from '@angular/core';

/**
 * Global handler for unhandled *client-side* exceptions (KB-050 "Error
 * handling"). Server problem bodies are a different thing entirely and are
 * parsed by the HTTP error interceptor that M2-C02 adds - never presented here
 * as if they were client faults.
 *
 * Deliberately unstyled and minimal: M2-C04-03 replaces this surface with the
 * `ErrorState` primitive.
 */
@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  handleError(error: unknown): void {
    console.error('[NexGen ERP] Unhandled client error', error);

    const host = document.querySelector('app-root');
    if (host === null || host.querySelector('[data-global-error]') !== null) {
      return;
    }

    const panel = document.createElement('div');
    panel.setAttribute('data-global-error', '');
    panel.setAttribute('role', 'alert');
    panel.textContent = 'Something went wrong. Reload the page and try again.';
    host.prepend(panel);
  }
}
