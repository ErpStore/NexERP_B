import type { ValidationErrors } from '@angular/forms';

/*
 * The one place a validation error becomes a sentence.
 *
 * These cover the *shapes* Angular's built-in validators produce - the client
 * mirror of the server's DataAnnotations, for UX only. They are not ERP rules
 * and they are not a validator schema for any real entity: the shapes
 * themselves are generated from OpenAPI by M2-B10.
 *
 * A message that came from the server (`server`) is shown **verbatim**: those
 * strings are product UX copy, e.g.
 * `V.SMART/V.SMART.Shared/ViewModels/MasterViewModel/AccountsViewModel/CurrencyVM.cs:14`
 * "Please Enter Currency Name".
 */

/** Message for the `server` error key set by {@link applyServerErrors}. */
export const SERVER_ERROR_KEY = 'server';

interface LengthError {
  readonly requiredLength: number;
}
interface RangeError {
  readonly min?: number;
  readonly max?: number;
}

function isLengthError(value: unknown): value is LengthError {
  return typeof value === 'object' && value !== null && 'requiredLength' in value;
}

function isRangeError(value: unknown): value is RangeError {
  return typeof value === 'object' && value !== null && ('min' in value || 'max' in value);
}

/**
 * The first error on a control, as a sentence. `null` when there is nothing
 * to say.
 *
 * `overrides` lets a screen replace a message without introducing a second
 * error-rendering mechanism.
 */
export function defaultErrorMessage(
  errors: ValidationErrors | null,
  overrides?: Readonly<Record<string, string>>,
): string | null {
  if (!errors) {
    return null;
  }
  for (const [key, detail] of Object.entries(errors)) {
    const override = overrides?.[key];
    if (override !== undefined) {
      return override;
    }
    const message = messageFor(key, detail);
    if (message !== null) {
      return message;
    }
  }
  return null;
}

function messageFor(key: string, detail: unknown): string | null {
  switch (key) {
    // Shown verbatim - it is the server's own words.
    case SERVER_ERROR_KEY:
      return Array.isArray(detail) ? detail.map(String).join(' ') : String(detail);
    case 'required':
      return 'This field is required.';
    case 'email':
      return 'Enter a valid email address.';
    case 'pattern':
      return 'The value is not in the expected format.';
    case 'minlength':
      return isLengthError(detail)
        ? `Enter at least ${detail.requiredLength} characters.`
        : 'The value is too short.';
    case 'maxlength':
      return isLengthError(detail)
        ? `Enter no more than ${detail.requiredLength} characters.`
        : 'The value is too long.';
    case 'min':
      return isRangeError(detail) && detail.min !== undefined
        ? `The value must be at least ${detail.min}.`
        : 'The value is too small.';
    case 'max':
      return isRangeError(detail) && detail.max !== undefined
        ? `The value must be at most ${detail.max}.`
        : 'The value is too large.';
    default:
      // An unknown key with a string payload is treated as a ready-made
      // message; anything else falls through to a generic sentence rather
      // than rendering "[object Object]" at a user.
      return typeof detail === 'string' ? detail : 'The value is not valid.';
  }
}
