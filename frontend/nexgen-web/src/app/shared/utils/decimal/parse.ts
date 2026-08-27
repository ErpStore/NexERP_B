import { InvalidDecimalError, type Money, type Qty } from './decimal';
import { money, qty } from './money';
import { getDefaultDecimalPlaces } from './precision';

/**
 * Why a raw string did not become a value. The caller renders these; they are
 * never turned into a silent zero.
 */
export type ParseFailure = 'not-a-number' | 'too-many-decimals' | 'not-finite' | 'too-many-signs';

/**
 * A discriminated parse result. `parseUserInput` never throws and never yields
 * `0` for input it did not understand.
 *
 * - `value`      - a complete, valid number.
 * - `empty`      - nothing typed yet. The field's value is *absent*, not zero.
 * - `incomplete` - a legal prefix mid-typing (`-`, `.`, `1.`). Not an error;
 *                  the control must not scold the user for still typing.
 * - `error`      - understood as an attempt at a number and rejected.
 */
export type ParseResult<T> =
  | { readonly kind: 'value'; readonly value: T; readonly raw: string }
  | { readonly kind: 'empty'; readonly raw: string }
  | { readonly kind: 'incomplete'; readonly raw: string }
  | { readonly kind: 'error'; readonly reason: ParseFailure; readonly raw: string };

export interface ParseOptions {
  /** Maximum decimal places accepted. Defaults to the precision policy. */
  places?: number;
  /** BCP-47 tag whose group/decimal separators are honoured. */
  locale?: string;
}

interface Separators {
  group: string;
  decimal: string;
}

function separatorsFor(locale: string | undefined): Separators {
  const parts = new Intl.NumberFormat(locale).formatToParts(12345.6);
  const group = parts.find((part) => part.type === 'group')?.value ?? ',';
  const decimal = parts.find((part) => part.type === 'decimal')?.value ?? '.';
  return { group, decimal };
}

/** Strip everything that is presentational: spaces, NBSP and group separators. */
function normalise(raw: string, separators: Separators): string {
  let text = raw.replace(/\s/g, '');
  text = text.replaceAll(separators.group, '');
  if (separators.decimal !== '.') {
    text = text.replaceAll(separators.decimal, '.');
  }
  // Unicode minus / en dash typed or pasted from a document.
  return text.replace(/^[−–]/, '-');
}

const COMPLETE = /^-?\d+(\.\d+)?$/;
const INCOMPLETE = /^-?(\d*\.?|\d+\.)$/;

/**
 * Parse what a user typed into a money field.
 *
 * Handles: empty input, a lone `-`, a lone `.`, a trailing separator mid-typing,
 * locale group separators, a locale comma decimal separator, and more decimals
 * than the field allows.
 *
 * This is what a Reactive Forms `ValidatorFn` for a money field consumes;
 * `Validators.pattern` over a numeric string is not a substitute, because it
 * cannot tell "still typing" from "wrong".
 */
export function parseUserInput(raw: string, options: ParseOptions = {}): ParseResult<Money> {
  return parseWith(raw, options, money);
}

/** {@link parseUserInput} for quantity fields. */
export function parseUserInputAsQty(raw: string, options: ParseOptions = {}): ParseResult<Qty> {
  return parseWith(raw, options, qty);
}

function parseWith<T extends Money | Qty>(
  raw: string,
  options: ParseOptions,
  construct: (input: string) => T,
): ParseResult<T> {
  const places = options.places ?? getDefaultDecimalPlaces();
  const text = normalise(raw, separatorsFor(options.locale));

  if (text === '') {
    return { kind: 'empty', raw };
  }
  if (!COMPLETE.test(text)) {
    if (INCOMPLETE.test(text)) {
      return { kind: 'incomplete', raw };
    }
    const signs = text.match(/[-+]/g)?.length ?? 0;
    return { kind: 'error', reason: signs > 1 ? 'too-many-signs' : 'not-a-number', raw };
  }

  const decimals = text.includes('.') ? text.length - text.indexOf('.') - 1 : 0;
  if (decimals > places) {
    return { kind: 'error', reason: 'too-many-decimals', raw };
  }

  try {
    return { kind: 'value', value: construct(text), raw };
  } catch (error) {
    // Belt and braces: this function must never throw, whatever the cause.
    return {
      kind: 'error',
      reason: error instanceof InvalidDecimalError ? 'not-finite' : 'not-a-number',
      raw,
    };
  }
}
