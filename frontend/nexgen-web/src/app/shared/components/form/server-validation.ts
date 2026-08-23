import type { AbstractControl, FormGroup } from '@angular/forms';

import { SERVER_ERROR_KEY } from './error-messages';

/*
 * Server validation errors -> form controls.
 *
 * A pure function, not a service: it holds no state, injects nothing, and is
 * testable without a TestBed. The server stays authoritative for validation;
 * the client mirror exists for UX only, so whatever the server says wins and
 * is shown **verbatim** - those strings are product UX copy.
 *
 * Contract read from source (V.SMART.Api/Middleware/ApiProblems.cs:145-155):
 * a 400 is a `ValidationProblemDetails` built from `ModelState`, with
 * `type = ProblemTypes.ValidationFailed`, a fixed `title`, an `errors`
 * dictionary and a `traceId` extension. A 409 carries its business-rule
 * message in `title` instead, with no `errors` - which is why `title` is
 * surfaced as a form-level message when there is nothing to key it to.
 */

/** The subset of RFC 7807 this function needs. Not a generated type; the generated one is M2-B10's. */
export interface ProblemDetailsLike {
  readonly status?: number;
  readonly title?: string;
  readonly detail?: string;
  readonly errors?: Readonly<Record<string, readonly string[] | string>>;
}

export interface UnmatchedServerError {
  /** The key exactly as the server sent it. */
  readonly key: string;
  readonly messages: readonly string[];
}

export interface ServerErrorResult {
  /** Control paths that received an error, in the form's own naming. */
  readonly applied: readonly string[];
  /** Keys with no matching control - the caller decides what to do with them. */
  readonly unmatched: readonly UnmatchedServerError[];
  /** Messages for the form-level alert, verbatim and in server order. */
  readonly formLevel: readonly string[];
}

/**
 * Keys the server uses for "this is about the request, not a field".
 * `ModelState` uses the empty key; `$` prefixes a body-level binding failure.
 */
function isFormLevelKey(key: string): boolean {
  return key === '' || key === '$' || key.toLowerCase() === 'request';
}

function toMessages(value: readonly string[] | string): readonly string[] {
  return typeof value === 'string' ? [value] : value;
}

/**
 * Find the control a server key names.
 *
 * The keys come from `ModelState`, so they are **PascalCase C# property
 * names** (`CurrName`), while Angular control names are conventionally
 * camelCase - no `DictionaryKeyPolicy` is configured in `V.SMART.Api`, and
 * `JsonSerializerDefaults.Web` does not rewrite dictionary keys. The match is
 * therefore: exact path first, then a case-insensitive walk. Without this,
 * every field error would silently fall through to the form-level alert.
 * Recorded as Q-72 in `docs/kb/open-questions.md` - it is Inferred from the
 * absence of a naming policy, not from an observed 400 body.
 */
function findControl(
  form: FormGroup,
  key: string,
): { path: string; control: AbstractControl } | null {
  const path = key.replace(/\[(\d+)\]/g, '.$1');
  const exact = form.get(path);
  if (exact) {
    return { path, control: exact };
  }
  const wanted = path.toLowerCase();
  for (const candidate of controlPaths(form)) {
    if (candidate.toLowerCase() === wanted) {
      const control = form.get(candidate);
      if (control) {
        return { path: candidate, control };
      }
    }
  }
  return null;
}

/** Every addressable control path in the group, depth-first. */
function controlPaths(group: FormGroup, prefix = ''): string[] {
  const paths: string[] = [];
  for (const [name, control] of Object.entries(group.controls)) {
    const path = prefix ? `${prefix}.${name}` : name;
    paths.push(path);
    const nested = control as Partial<FormGroup>;
    if (nested.controls && !Array.isArray(nested.controls)) {
      paths.push(...controlPaths(control as FormGroup, path));
    }
  }
  return paths;
}

/**
 * Apply a `ProblemDetails` to a form. Returns what matched, what did not, and
 * what belongs in the form-level alert.
 *
 * Controls that receive an error are marked touched, because an error the
 * user never triggered is otherwise invisible under the touched/dirty rule in
 * `app-form-field`.
 */
export function applyServerErrors(form: FormGroup, problem: ProblemDetailsLike): ServerErrorResult {
  const applied: string[] = [];
  const unmatched: UnmatchedServerError[] = [];
  const formLevel: string[] = [];

  for (const [key, raw] of Object.entries(problem.errors ?? {})) {
    const messages = toMessages(raw);
    if (messages.length === 0) {
      continue;
    }
    if (isFormLevelKey(key)) {
      formLevel.push(...messages);
      continue;
    }
    const match = findControl(form, key);
    if (!match) {
      unmatched.push({ key, messages });
      formLevel.push(...messages);
      continue;
    }
    match.control.setErrors({ ...(match.control.errors ?? {}), [SERVER_ERROR_KEY]: messages });
    match.control.markAsTouched();
    applied.push(match.path);
  }

  // A 409 has no `errors` dictionary: the business-rule message is the title,
  // and it is shown as written.
  if (!problem.errors && problem.title) {
    formLevel.push(problem.title);
  }

  return { applied, unmatched, formLevel };
}

/**
 * Clear every server-set error, leaving client validators alone. Call it
 * before a resubmit, or the previous attempt's errors linger.
 */
export function clearServerErrors(form: FormGroup): void {
  for (const path of controlPaths(form)) {
    const control = form.get(path);
    const errors = control?.errors;
    if (!control || !errors || !(SERVER_ERROR_KEY in errors)) {
      continue;
    }
    const remaining = Object.fromEntries(
      Object.entries(errors).filter(([key]) => key !== SERVER_ERROR_KEY),
    );
    control.setErrors(Object.keys(remaining).length > 0 ? remaining : null);
  }
}
