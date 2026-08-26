import { HttpClient, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import {
  GRID_EXPORT_XLSX,
  GridExportService,
  contentDispositionFilename,
  exportQuery,
  fallbackFilename,
  type GridExportOperation,
} from './grid-export.service';
import { defaultDataGridState, type DataGridState } from './data-grid.model';
import { BR_SO_001_DELETE_MESSAGE, EXPORT_ROW_LIMIT_MESSAGE } from './fixtures/problem-details';

/** The one export endpoint that exists (`CurrencyExcelController.cs:84`). */
const EXPORT_URL = '/api/v1/currencies/export';

/** `CurrencyExcelController.cs:51`. */
const XLSX_CONTENT_TYPE = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';

/** A filtered, sorted view - what a user is looking at when they click Export. */
const FILTERED_STATE: DataGridState = {
  ...defaultDataGridState(),
  page: 4,
  pageSize: 50,
  sort: [{ field: 'createdDate', direction: 'desc' }],
  filters: { currName: 'acme', createdBy: 'jo' },
};

/**
 * The operation a caller writes over the generated client. This one goes
 * through the real `HttpClient` so `HttpTestingController` sees the request -
 * the same shape as `exportCurrencies$Response`, which returns the **full**
 * response because the filename lives in a header.
 */
function operationOver(http: HttpClient): GridExportOperation {
  return (query, format) =>
    http.get(EXPORT_URL, {
      params: { ...query, format },
      observe: 'response',
      responseType: 'blob',
    });
}

describe('GridExportService', () => {
  let http: HttpTestingController;
  let service: GridExportService;
  let created: string[];
  let revoked: string[];
  let clicked: HTMLAnchorElement[];

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), GridExportService],
    });
    http = TestBed.inject(HttpTestingController);
    service = TestBed.inject(GridExportService);

    created = [];
    revoked = [];
    clicked = [];
    // jsdom implements neither, and a real anchor click would try to navigate.
    URL.createObjectURL = vi.fn((): string => {
      const url = `blob:test/${created.length}`;
      created.push(url);
      return url;
    });
    URL.revokeObjectURL = vi.fn((url: string) => {
      revoked.push(url);
    });
    vi.spyOn(HTMLAnchorElement.prototype, 'click').mockImplementation(function (
      this: HTMLAnchorElement,
    ) {
      clicked.push(this);
    });
  });

  afterEach(() => {
    http.verify();
    vi.restoreAllMocks();
  });

  function run(state: DataGridState = FILTERED_STATE, format = GRID_EXPORT_XLSX) {
    service.exportAs({
      operation: operationOver(TestBed.inject(HttpClient)),
      state,
      format,
      fallbackBaseName: 'currencies',
    });
    return http.expectOne((candidate) => candidate.url === EXPORT_URL);
  }

  function workbook(): Blob {
    return new Blob(['workbook'], { type: XLSX_CONTENT_TYPE });
  }

  /** Test 11. An unfiltered export of a filtered view is a data-integrity bug. */
  it('issues exactly one request carrying the current sort and filters', () => {
    const request = run();

    expect(request.request.params.get('sort')).toBe('-createdDate');
    expect(request.request.params.get('currName')).toBe('acme');
    expect(request.request.params.get('createdBy')).toBe('jo');
    expect(request.request.params.get('format')).toBe(GRID_EXPORT_XLSX);

    request.flush(workbook());
    http.verify();
  });

  /** An export is the whole filtered set, not the page on screen. */
  it('sends no paging parameters', () => {
    const request = run();

    expect(request.request.params.get('pageNumber')).toBeNull();
    expect(request.request.params.get('pageSize')).toBeNull();

    request.flush(workbook());
  });

  /** Test 12, first half. */
  it('saves under the Content-Disposition filename when the server supplies one', () => {
    const request = run();

    request.flush(workbook(), {
      headers: { 'Content-Disposition': 'attachment; filename="currencies-20260826-101500.xlsx"' },
    });

    expect(clicked).toHaveLength(1);
    expect(clicked[0]?.download).toBe('currencies-20260826-101500.xlsx');
  });

  /**
   * Test 12, second half - and the path a real browser actually takes today:
   * `V.SMART.Api/Program.cs:165-171` exposes no headers, so
   * `Content-Disposition` reads as null cross-origin. The fallback is
   * deterministic on purpose; a timestamp here would be a guess at the server's
   * own name.
   */
  it('falls back to a deterministic name when the header is unreadable', () => {
    const request = run();

    request.flush(workbook());

    expect(clicked[0]?.download).toBe('currencies.xlsx');
  });

  /** Test 13. */
  it('revokes the object URL after the download', () => {
    const request = run();

    request.flush(workbook(), {
      headers: { 'Content-Disposition': 'attachment; filename="c.xlsx"' },
    });

    expect(created).toHaveLength(1);
    expect(revoked).toEqual(created);
  });

  it('clears the busy flag when the export completes', () => {
    const request = run();
    expect(service.exporting()).toBe(true);

    request.flush(workbook());

    expect(service.exporting()).toBe(false);
  });

  it('refuses a second export while one is in flight rather than downloading twice', () => {
    const request = run();
    service.exportAs({
      operation: operationOver(TestBed.inject(HttpClient)),
      state: FILTERED_STATE,
      fallbackBaseName: 'currencies',
    });

    http.expectNone((candidate) => candidate.url === EXPORT_URL);
    request.flush(workbook());
  });

  /**
   * A `responseType: 'blob'` request receives its **error** body as a Blob too -
   * XHR honours the requested type for a 4xx exactly as it does for a 200. This
   * is what the server actually sends back through that channel.
   */
  function problemBlob(problem: Record<string, unknown>): Blob {
    return new Blob([JSON.stringify(problem)], { type: 'application/problem+json' });
  }

  /**
   * Test 14. The Blazor list shows `"Error while exporting MfgPo!"`
   * (`DetailsModal.razor:246-251`); the 409 the server answers above its
   * 10,000-row ceiling says how many rows there are and what to do about it.
   */
  it('surfaces the server message on failure, not a generic one', async () => {
    const request = run();

    request.flush(
      problemBlob({
        type: 'https://api.v-smart.local/problems/business-rule',
        title: EXPORT_ROW_LIMIT_MESSAGE,
        status: 409,
        traceId: 'trace-1',
      }),
      { status: 409, statusText: 'Conflict' },
    );
    await vi.waitFor(() => expect(service.error()).not.toBeNull());

    expect(service.error()?.status).toBe(409);
    expect(service.error()?.title).toBe(EXPORT_ROW_LIMIT_MESSAGE);
    expect(service.exporting()).toBe(false);
    expect(clicked).toHaveLength(0);
  });

  it('keeps a business-rule refusal byte-for-byte', async () => {
    const request = run();

    request.flush(problemBlob({ title: BR_SO_001_DELETE_MESSAGE, status: 409 }), {
      status: 409,
      statusText: 'Conflict',
    });
    await vi.waitFor(() => expect(service.error()).not.toBeNull());

    expect(service.error()?.title).toBe(BR_SO_001_DELETE_MESSAGE);
  });

  it('falls back to the transport status when the error body is unreadable', async () => {
    const request = run();

    request.flush(new Blob(['<html>gateway</html>'], { type: 'text/html' }), {
      status: 502,
      statusText: 'Bad Gateway',
    });
    await vi.waitFor(() => expect(service.error()).not.toBeNull());

    expect(service.error()?.status).toBe(502);
    expect(service.error()?.title).toBeUndefined();
  });

  it('clears a previous failure when a new export starts', async () => {
    const failed = run();
    failed.flush(problemBlob({ title: 'nope', status: 409 }), {
      status: 409,
      statusText: 'Conflict',
    });
    await vi.waitFor(() => expect(service.error()).not.toBeNull());

    const second = run();
    expect(service.error()).toBeNull();
    second.flush(workbook());
  });
});

describe('exportQuery', () => {
  it('keeps sort and filters and drops paging', () => {
    expect(exportQuery(FILTERED_STATE)).toEqual({
      sort: '-createdDate',
      currName: 'acme',
      createdBy: 'jo',
    });
  });
});

describe('contentDispositionFilename', () => {
  it('reads the quoted form', () => {
    expect(contentDispositionFilename('attachment; filename="currencies.xlsx"')).toBe(
      'currencies.xlsx',
    );
  });

  it('prefers the RFC 5987 extended form and decodes it', () => {
    const header = `attachment; filename="fallback.xlsx"; filename*=UTF-8''devis%20d%27achat.xlsx`;

    expect(contentDispositionFilename(header)).toBe(`devis d'achat.xlsx`);
  });

  it('reads an unquoted name', () => {
    expect(contentDispositionFilename('attachment; filename=currencies.xlsx')).toBe(
      'currencies.xlsx',
    );
  });

  it('never lets a server-supplied name become a path', () => {
    expect(contentDispositionFilename('attachment; filename="../../etc/passwd"')).toBe(
      '.._.._etc_passwd',
    );
  });

  it('answers null when there is no header - the cross-origin case today', () => {
    expect(contentDispositionFilename(null)).toBeNull();
    expect(contentDispositionFilename('attachment')).toBeNull();
  });
});

describe('fallbackFilename', () => {
  it('is deterministic, so the same view saves under the same name twice', () => {
    expect(fallbackFilename('currencies', 'xlsx')).toBe('currencies.xlsx');
    expect(fallbackFilename('currencies', 'xlsx')).toBe(fallbackFilename('currencies', 'xlsx'));
  });
});
