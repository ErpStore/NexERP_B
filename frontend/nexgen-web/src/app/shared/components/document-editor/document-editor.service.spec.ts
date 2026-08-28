import { HttpClient, provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';

import { DocumentEditorStore, createHttpDocumentOperations } from './document-editor.service';
import { RecordingOperations, testSnapshot, type TestHeader, type TestLine } from './test-fixtures';

/**
 * The store and the `HttpClient` operation factory. HTTP is stubbed with
 * Angular's own `HttpTestingController` - no request-interception library is
 * added, per this task's Testing Requirements.
 */
describe('DocumentEditorStore', () => {
  it('counts every explicit refresh and re-reads through the config operation', async () => {
    const store = new DocumentEditorStore<TestHeader, TestLine>();
    const ops = new RecordingOperations();
    store.configure(ops);

    await store.load(7);
    expect(store.refreshCount()).toBe(0);

    await store.refresh();
    expect(store.refreshCount()).toBe(1);
    expect(ops.loads).toEqual([7, 7]);
    expect(store.snapshot()).toEqual(ops.snapshot);
  });

  it('records the problem body rather than throwing, and never half-applies a refused save', async () => {
    const store = new DocumentEditorStore<TestHeader, TestLine>();
    const ops = new RecordingOperations();
    ops.saveFailure = { status: 409, title: 'The server refused.' };
    store.configure(ops);
    await store.load(7);

    const result = await store.save(
      { header: { reference: 'typed' }, lines: [], linesByGrid: {} },
      7,
    );

    expect(result).toBeNull();
    expect(store.saveProblem()?.title).toBe('The server refused.');
    expect(store.snapshot()).toEqual(testSnapshot());
  });
});

describe('createHttpDocumentOperations', () => {
  let http: HttpClient;
  let controller: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpClient);
    controller = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    controller.verify();
    TestBed.resetTestingModule();
  });

  it('builds collection and item URLs from the caller-supplied base and slug only', async () => {
    const ops = createHttpDocumentOperations<TestHeader, TestLine>(
      http,
      'https://api.example.test/api/v1/',
      'test-documents',
    );

    const loaded = firstValueFrom(ops.load(7));
    controller.expectOne('https://api.example.test/api/v1/test-documents/7').flush(testSnapshot());
    expect((await loaded).id).toBe(7);

    const created = firstValueFrom(
      ops.create({ header: { reference: 'a' }, lines: [], linesByGrid: {} }),
    );
    const post = controller.expectOne('https://api.example.test/api/v1/test-documents');
    expect(post.request.method).toBe('POST');
    post.flush(testSnapshot());
    await created;

    const updated = firstValueFrom(
      ops.update(7, { header: { reference: 'a' }, lines: [], linesByGrid: {} }),
    );
    const put = controller.expectOne('https://api.example.test/api/v1/test-documents/7');
    expect(put.request.method).toBe('PUT');
    put.flush(testSnapshot());
    await updated;
  });
});
