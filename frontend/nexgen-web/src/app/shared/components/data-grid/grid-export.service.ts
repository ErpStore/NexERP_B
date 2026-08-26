import { HttpErrorResponse, type HttpResponse } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import type { Observable } from 'rxjs';

import { toGridProblem, type GridProblemDetails } from './data-grid-error.component';
import {
  WIRE_PAGE_NUMBER_PARAM,
  WIRE_PAGE_SIZE_PARAM,
  toWireQuery,
  type DataGridWireQuery,
} from './data-grid-query.adapter';
import type { DataGridState } from './data-grid.model';

/**
 * The one format the server actually produces.
 *
 * `CurrencyExcelController.cs:48` declares `xlsx` as the only accepted value and
 * `:100-104` answers a 400 `ValidationProblemDetails` for anything else. A CSV
 * entry in the toolbar would therefore be a button that always fails, so the
 * default offers Excel alone. Recorded as Q-95 in `docs/kb/open-questions.md`:
 * adding CSV is a change request against the export endpoint, not a client
 * change - ADR-005 forbids building the file here.
 */
export const GRID_EXPORT_XLSX = 'xlsx';

/** The extension a saved file gets when the server did not name one. */
const FORMAT_EXTENSIONS: Readonly<Record<string, string>> = { xlsx: 'xlsx', csv: 'csv' };

/**
 * One call to a server export endpoint. The caller writes it over the generated
 * OpenAPI client - e.g. `CurrencyService.exportCurrencies$Response({...})` - so
 * this directory still names no endpoint.
 *
 * It must return the **full** `HttpResponse`, not the body: the filename lives
 * in `Content-Disposition`, and `exportCurrencies()` (body-only) throws it away.
 */
export type GridExportOperation = (
  query: DataGridWireQuery,
  format: string,
) => Observable<HttpResponse<Blob>>;

export interface GridExportRequest {
  readonly operation: GridExportOperation;
  /** The **current** query state. Its sort and filters go on the wire; paging does not. */
  readonly state: DataGridState;
  readonly format?: string;
  /**
   * The filename used when the server did not supply one, without an extension -
   * e.g. `currencies`. Deterministic on purpose: a timestamp here would make the
   * fallback untestable and the file harder to find twice.
   */
  readonly fallbackBaseName?: string;
}

/**
 * Fetches an exported file from the server and saves it.
 *
 * **The client never builds the file.** ADR-005 put Excel export behind server
 * endpoints and `ExcelExportService`
 * (`V.SMART/V.SMART.Shared/Services/ExcelExportService.cs:24`, `:113`) is what
 * produces the bytes; this service asks for them and hands them to the browser.
 * `no-client-file-generation.spec.ts` is the standing guard on that.
 *
 * **Paging is deliberately not sent.** An export is the whole filtered set, not
 * the page on screen - `CurrencyExcelController.cs:80-82` documents that it
 * ignores `pageNumber`/`pageSize`, and sending them would only invite a future
 * endpoint to honour them and silently export 20 rows.
 *
 * **The failure is the server's, verbatim.** The Blazor list shows a generic
 * `"Error while exporting MfgPo!"` toast (`DetailsModal.razor:246-251`); a 409
 * from the 10,000-row ceiling (`CurrencyExcelController.cs:58`, `:112-116`) says
 * exactly how many rows there are and what to do about it, which is information
 * no generic string can reconstruct.
 *
 * Not `providedIn: 'root'`: `exporting` is per-grid state, and two grids sharing
 * one busy flag would disable each other's toolbar.
 */
@Injectable()
export class GridExportService {
  readonly #exporting = signal(false);
  readonly #error = signal<GridProblemDetails | null>(null);

  /** True while a request is in flight. Bind the toolbar control's `disabled` to it. */
  readonly exporting = this.#exporting.asReadonly();
  /** The server's problem body from the last failed export, or `null`. */
  readonly error = this.#error.asReadonly();

  clearError(): void {
    this.#error.set(null);
  }

  /**
   * Issues **exactly one** request carrying the current sort and filters, then
   * saves the response. Concurrent calls are refused rather than queued: two
   * downloads of the same list is never what the second click meant.
   */
  exportAs(request: GridExportRequest): void {
    if (this.#exporting()) {
      return;
    }
    const format = request.format ?? GRID_EXPORT_XLSX;
    this.#exporting.set(true);
    this.#error.set(null);

    request.operation(exportQuery(request.state), format).subscribe({
      next: (response) => {
        this.#save(response, fallbackFilename(request.fallbackBaseName ?? 'export', format));
      },
      error: (error: unknown) => {
        this.#exporting.set(false);
        void this.#reportFailure(error);
      },
      complete: () => this.#exporting.set(false),
    });
  }

  /**
   * Reads the failure the browser actually delivers.
   *
   * The request asks for `responseType: 'blob'`, so an error body arrives as a
   * **Blob**, not as parsed JSON - XHR honours the requested response type for
   * a 4xx exactly as it does for a 200. Without the read below, the server's
   * 409 sentence (`CurrencyExcelController.cs:112-116`) would reach the user as
   * `[object Blob]`, which is the generic-message failure this task exists to
   * remove. The parse is deliberately forgiving: an unreadable body falls back
   * to the transport status rather than throwing inside an error handler.
   */
  async #reportFailure(error: unknown): Promise<void> {
    if (error instanceof HttpErrorResponse && error.error instanceof Blob) {
      const body = await readProblemBlob(error.error);
      this.#error.set({ ...(body ?? {}), status: body?.status ?? error.status });
      return;
    }
    this.#error.set(toGridProblem(error));
  }

  #save(response: HttpResponse<Blob>, fallback: string): void {
    const body = response.body;
    if (!body) {
      return;
    }
    const name =
      contentDispositionFilename(response.headers.get('Content-Disposition')) ?? fallback;
    const url = URL.createObjectURL(body);
    try {
      const anchor = document.createElement('a');
      anchor.href = url;
      anchor.download = name;
      anchor.rel = 'noopener';
      anchor.style.display = 'none';
      document.body.appendChild(anchor);
      anchor.click();
      anchor.remove();
    } finally {
      // Revoked in the same turn the click started the download in, so the blob
      // cannot outlive the export. The navigation is already committed by then.
      URL.revokeObjectURL(url);
    }
  }
}

/**
 * The wire query an export is made with: the current sort and filters, and no
 * paging.
 */
export function exportQuery(state: DataGridState): DataGridWireQuery {
  const query: Record<string, string | number> = { ...toWireQuery(state) };
  delete query[WIRE_PAGE_NUMBER_PARAM];
  delete query[WIRE_PAGE_SIZE_PARAM];
  return query;
}

/** `export-2026-08-26.xlsx` is a guess; `currencies.xlsx` is not. */
export function fallbackFilename(baseName: string, format: string): string {
  return `${baseName}.${FORMAT_EXTENSIONS[format] ?? format}`;
}

/**
 * Reads the filename out of `Content-Disposition`, RFC 5987 form first.
 *
 * **Known gap, stated rather than papered over.** `Content-Disposition` is not a
 * CORS-safelisted response header, and `V.SMART/V.SMART.Api/Program.cs:165-171`
 * configures no `WithExposedHeaders`, so on the cross-origin call the SPA
 * actually makes this header reads as `null` in a browser and the fallback is
 * the only path taken. That is a gap against M2-B06's CORS configuration, logged
 * as Q-96 in `docs/kb/open-questions.md` and R-79 in the technical-debt
 * register - not something to work around with a guessed server filename.
 */
export function contentDispositionFilename(header: string | null): string | null {
  if (!header) {
    return null;
  }
  const extended = /filename\*\s*=\s*[^']*'[^']*'([^;]+)/i.exec(header);
  const quoted = /filename\s*=\s*"([^"]*)"/i.exec(header);
  const bare = /filename\s*=\s*([^;]+)/i.exec(header);
  const raw = extended?.[1] ?? quoted?.[1] ?? bare?.[1];
  if (raw === undefined) {
    return null;
  }
  return safeFilename(extended ? decodeURIComponent(raw.trim()) : raw.trim());
}

/** Parses a `problem+json` body that arrived as a Blob. Never throws. */
async function readProblemBlob(blob: Blob): Promise<GridProblemDetails | null> {
  try {
    const text = await blob.text();
    const parsed: unknown = text === '' ? null : JSON.parse(text);
    return typeof parsed === 'object' && parsed !== null ? parsed : null;
  } catch {
    return null;
  }
}

/** POSIX and Windows path separators. */
const PATH_SEPARATORS: readonly string[] = ['/', String.fromCharCode(92)];

/** A server-supplied name never becomes a path. */
function safeFilename(name: string): string | null {
  // Written without a literal backslash escape in a character class, so the
  // separator set survives any tool that rewrites escapes. One pass each.
  const cleaned = PATH_SEPARATORS.reduce(
    (value, separator) => value.split(separator).join('_'),
    name,
  ).trim();
  return cleaned === '' ? null : cleaned;
}
