import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';

import { CurrencyFeatureService } from './currency.service';
import type { CurrencyVM } from './models';

const ROW: CurrencyVM = {
  currId: 1,
  currName: 'US Dollar',
  currSub: 'Cents',
  symbol: '$',
  isSystemDefined: false,
};

describe('CurrencyFeatureService', () => {
  let service: CurrencyFeatureService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(CurrencyFeatureService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('list() maps a DataGridWireQuery straight onto the M2-B02 wire params, unchanged', () => {
    let result: unknown;
    service
      .list({ pageNumber: 2, pageSize: 20, currName: 'dol', sort: '-createdDate' })
      .subscribe((page) => (result = page));

    const req = http.expectOne(
      (r) =>
        r.url === '/api/v1/currencies' &&
        r.params.get('pageNumber') === '2' &&
        r.params.get('pageSize') === '20' &&
        r.params.get('currName') === 'dol' &&
        r.params.get('sort') === '-createdDate',
    );
    expect(req.request.method).toBe('GET');
    req.flush({ items: [ROW], totalCount: 1, pageNumber: 2, pageSize: 20 });

    expect(result).toEqual({ items: [ROW], totalCount: 1, pageNumber: 2, pageSize: 20 });
  });

  it('list() omits a filter the caller did not set, rather than sending an empty string', () => {
    service.list({ pageNumber: 1, pageSize: 20 }).subscribe();
    const req = http.expectOne(() => true);
    expect(req.request.params.has('currName')).toBe(false);
    req.flush({ items: [], totalCount: 0, pageNumber: 1, pageSize: 20 });
  });

  it('getById() calls GET /api/v1/currencies/{id}', () => {
    service.getById(1).subscribe();
    const req = http.expectOne('/api/v1/currencies/1');
    expect(req.request.method).toBe('GET');
    req.flush(ROW);
  });

  it('create() sets saving while the request is in flight and clears it on completion', () => {
    expect(service.saving()).toBe(false);
    service.create(ROW).subscribe();
    expect(service.saving()).toBe(true);

    const req = http.expectOne('/api/v1/currencies');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(ROW);
    req.flush(ROW);

    expect(service.saving()).toBe(false);
  });

  it('create() clears saving even when the request fails', () => {
    service.create(ROW).subscribe({ error: () => undefined });
    const req = http.expectOne('/api/v1/currencies');
    req.flush({ title: 'Duplicate' }, { status: 409, statusText: 'Conflict' });

    expect(service.saving()).toBe(false);
  });

  it('update() calls PUT /api/v1/currencies/{id} with the body', () => {
    service.update(1, ROW).subscribe();
    const req = http.expectOne('/api/v1/currencies/1');
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(ROW);
    req.flush(ROW);
  });

  it('remove() calls DELETE /api/v1/currencies/{id}', () => {
    service.remove(1).subscribe();
    const req = http.expectOne('/api/v1/currencies/1');
    expect(req.request.method).toBe('DELETE');
    req.flush(null, { status: 204, statusText: 'No Content' });
  });

  it('exportOperation() calls the export endpoint with the format and the current filters, no paging', () => {
    service.exportOperation({ pageNumber: 3, pageSize: 20, currName: 'dol' }, 'xlsx').subscribe();

    const req = http.expectOne(
      (r) => r.url === '/api/v1/currencies/export' && r.params.get('format') === 'xlsx',
    );
    // The service forwards whatever DataGridQueryState hands it; grid-export.service.ts's own
    // exportQuery() is what strips paging before calling this — asserted there, not duplicated
    // here.
    req.flush(new Blob(['x']));
  });
});
