import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { firstValueFrom, type Observable } from 'rxjs';

import type { ApiProblem } from '@/app/core/http/api-problem';

import type {
  DocumentEditorOperations,
  DocumentId,
  DocumentPayload,
  DocumentSnapshot,
} from './document-editor.model';

/**
 * The document editor's own server state (M2-C08-01).
 *
 * **Provided by the component, never in root.** One editor instance owns one
 * document; a root-provided store would be a global cache, and KB-050
 * §Data-fetching conventions is explicit that there is no cache here: *a
 * mutation is followed by an explicit `refresh()`*, not by cache surgery.
 * {@link DocumentEditorStore.refreshCount} exists so that rule is testable.
 *
 * **No optimistic update.** `save()` never writes the outgoing payload into
 * {@link DocumentEditorStore.snapshot}; only a server response does. Anything
 * touching money, stock or document status must come back from the server
 * (BR-CALC-001, BR-STK-001).
 *
 * It parses no URL and knows no resource. Every call goes through the
 * caller-supplied {@link DocumentEditorOperations}.
 */
@Injectable()
export class DocumentEditorStore<THeader, TLine> {
  readonly #snapshot = signal<DocumentSnapshot<THeader, TLine> | null>(null);
  readonly #loading = signal(false);
  readonly #saving = signal(false);
  readonly #loadProblem = signal<ApiProblem | null>(null);
  readonly #saveProblem = signal<ApiProblem | null>(null);
  readonly #refreshCount = signal(0);

  #operations: DocumentEditorOperations<THeader, TLine> | null = null;
  #currentId: DocumentId | null = null;

  readonly snapshot = this.#snapshot.asReadonly();
  readonly loading = this.#loading.asReadonly();
  readonly saving = this.#saving.asReadonly();
  /** A failed load - the whole region renders `app-error-state` with Retry. */
  readonly loadProblem = this.#loadProblem.asReadonly();
  /** A failed save - a toast plus inline field errors; the editor is not unmounted. */
  readonly saveProblem = this.#saveProblem.asReadonly();
  /** How many explicit refreshes have happened. Read by the test that pins the no-cache rule. */
  readonly refreshCount = this.#refreshCount.asReadonly();

  readonly documentId = (): DocumentId | null => this.#currentId;

  configure(operations: DocumentEditorOperations<THeader, TLine>): void {
    this.#operations = operations;
  }

  /** Loads by id. Any failure lands in {@link loadProblem}; nothing throws out of here. */
  async load(id: DocumentId): Promise<void> {
    this.#currentId = id;
    this.#loading.set(true);
    this.#loadProblem.set(null);
    try {
      const snapshot = await firstValueFrom(this.#ops().load(id));
      this.#snapshot.set(snapshot);
    } catch (error) {
      this.#loadProblem.set(toApiProblem(error));
    } finally {
      this.#loading.set(false);
    }
  }

  /**
   * The explicit re-read KB-050 requires after a mutation. Named rather than
   * folded into `save()` so a test can count it, and so no call site is
   * tempted to hand-mutate the snapshot instead.
   */
  async refresh(): Promise<void> {
    if (this.#currentId === null) {
      return;
    }
    this.#refreshCount.update((n) => n + 1);
    await this.load(this.#currentId);
  }

  /**
   * Creates or updates. Returns the server's snapshot, or `null` when the
   * server refused - in which case {@link saveProblem} holds the problem body
   * and the caller maps it onto the form.
   */
  async save(
    payload: DocumentPayload<THeader, TLine>,
    id: DocumentId | null,
  ): Promise<DocumentSnapshot<THeader, TLine> | null> {
    this.#saving.set(true);
    this.#saveProblem.set(null);
    try {
      const request = id === null ? this.#ops().create(payload) : this.#ops().update(id, payload);
      const snapshot = await firstValueFrom(request);
      this.#snapshot.set(snapshot);
      if (snapshot.id !== undefined) {
        this.#currentId = snapshot.id;
      }
      return snapshot;
    } catch (error) {
      this.#saveProblem.set(toApiProblem(error));
      return null;
    } finally {
      this.#saving.set(false);
    }
  }

  /** Clears everything - used by Save + New, which starts a genuinely new document. */
  reset(): void {
    this.#snapshot.set(null);
    this.#loadProblem.set(null);
    this.#saveProblem.set(null);
    this.#currentId = null;
  }

  clearSaveProblem(): void {
    this.#saveProblem.set(null);
  }

  #ops(): DocumentEditorOperations<THeader, TLine> {
    if (this.#operations === null) {
      throw new Error('DocumentEditorStore was used before configure() was called.');
    }
    return this.#operations;
  }
}

/**
 * Normalises whatever `HttpClient` threw into the one `ApiProblem` shape
 * (KB-050 §Error handling). It re-parses nothing the interceptor already
 * parsed: `error.error` is the body as it arrived.
 */
export function toApiProblem(error: unknown): ApiProblem {
  if (error instanceof HttpErrorResponse) {
    const body: unknown = error.error;
    if (body !== null && typeof body === 'object') {
      return { status: error.status, ...(body as ApiProblem) };
    }
    return { status: error.status, title: error.statusText };
  }
  if (error !== null && typeof error === 'object' && 'title' in error) {
    return error as ApiProblem;
  }
  return { title: error instanceof Error ? error.message : 'Request failed.' };
}

/**
 * The typed `HttpClient` layer a feature module may build its operations from.
 *
 * It is a **factory over a caller-supplied base URL and slug**, not a service
 * with an endpoint in it: the shell still names no resource, and a module that
 * needs a different wire shape writes its own `DocumentEditorOperations`
 * instead. Nothing here is imported by the editor component.
 */
export function createHttpDocumentOperations<THeader, TLine>(
  http: HttpClient,
  baseUrl: string,
  resource: string,
): Pick<DocumentEditorOperations<THeader, TLine>, 'load' | 'create' | 'update'> {
  const collection = `${baseUrl.replace(/\/$/, '')}/${resource}`;
  return {
    load: (id: DocumentId): Observable<DocumentSnapshot<THeader, TLine>> =>
      http.get<DocumentSnapshot<THeader, TLine>>(`${collection}/${String(id)}`),
    create: (payload): Observable<DocumentSnapshot<THeader, TLine>> =>
      http.post<DocumentSnapshot<THeader, TLine>>(collection, payload),
    update: (id, payload): Observable<DocumentSnapshot<THeader, TLine>> =>
      http.put<DocumentSnapshot<THeader, TLine>>(`${collection}/${String(id)}`, payload),
  };
}

/** Convenience for a config that only needs the standard three operations. */
export function injectHttpDocumentOperations<THeader, TLine>(
  baseUrl: string,
  resource: string,
): Pick<DocumentEditorOperations<THeader, TLine>, 'load' | 'create' | 'update'> {
  return createHttpDocumentOperations<THeader, TLine>(inject(HttpClient), baseUrl, resource);
}
