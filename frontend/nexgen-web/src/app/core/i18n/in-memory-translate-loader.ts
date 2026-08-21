import { Injectable } from '@angular/core';
import { TranslateLoader, TranslationObject } from '@ngx-translate/core';
import { Observable, of } from 'rxjs';

import en from '../../../i18n/en.json';

const BUNDLES: Record<string, TranslationObject> = { en };

/**
 * Runtime-switchable translation source (ADR-007: runtime i18n, not Angular's
 * compile-time i18n). Only `en` exists today; when a second locale lands this
 * becomes an HTTP loader over `src/i18n/*.json` without touching callers.
 */
@Injectable({ providedIn: 'root' })
export class InMemoryTranslateLoader implements TranslateLoader {
  getTranslation(lang: string): Observable<TranslationObject> {
    return of(BUNDLES[lang] ?? BUNDLES['en'] ?? {});
  }
}
