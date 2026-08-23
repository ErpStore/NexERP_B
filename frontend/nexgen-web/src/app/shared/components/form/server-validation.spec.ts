import { FormControl, FormGroup } from '@angular/forms';
import { describe, expect, it } from 'vitest';

import { SERVER_ERROR_KEY } from './error-messages';
import { applyServerErrors, clearServerErrors, type ProblemDetailsLike } from './server-validation';

/*
 * No TestBed, no component, no DOM: `applyServerErrors` is a pure function
 * and this file proves it.
 */

function currencyForm() {
  return new FormGroup({
    currName: new FormControl<string | null>(null),
    currSymbol: new FormControl<string | null>(null),
    address: new FormGroup({
      pinCode: new FormControl<string | null>(null),
    }),
  });
}

/**
 * Shaped after the real thing: `V.SMART.Api/Middleware/ApiProblems.cs:145-155`
 * builds a `ValidationProblemDetails` from `ModelState`, so the keys are
 * PascalCase C# property names.
 */
const validationProblem: ProblemDetailsLike = {
  status: 400,
  title: 'One or more validation errors occurred.',
  errors: {
    CurrName: ['Please Enter Currency Name'],
    CurrSymbol: ['Please Enter Currency Symbol'],
  },
};

describe('applyServerErrors', () => {
  it('maps a 400 errors dictionary onto the matching typed controls', () => {
    const form = currencyForm();

    const result = applyServerErrors(form, validationProblem);

    expect(result.applied).toEqual(['currName', 'currSymbol']);
    expect(form.controls.currName.errors).toEqual({
      [SERVER_ERROR_KEY]: ['Please Enter Currency Name'],
    });
    expect(form.controls.currSymbol.invalid).toBe(true);
  });

  it('shows the server message verbatim - those strings are product UX copy', () => {
    const form = currencyForm();

    applyServerErrors(form, validationProblem);

    // CurrencyVM.cs:14 - the exact wording the Blazor app shows today.
    const errors = form.controls.currName.errors ?? {};
    expect(errors[SERVER_ERROR_KEY]).toEqual(['Please Enter Currency Name']);
  });

  it('matches PascalCase server keys to camelCase control names', () => {
    // ModelState keys are C# property names and no DictionaryKeyPolicy is
    // configured in V.SMART.Api, so the casing genuinely differs. Without the
    // normalising match every field error would fall through to the
    // form-level alert. See Q-70.
    const form = currencyForm();

    const result = applyServerErrors(form, { errors: { CURRNAME: ['nope'] } });

    expect(result.applied).toEqual(['currName']);
  });

  it('resolves a nested control path', () => {
    const form = currencyForm();

    const result = applyServerErrors(form, { errors: { 'Address.PinCode': ['Required'] } });

    expect(result.applied).toEqual(['address.pinCode']);
    expect(form.get('address.pinCode')?.invalid).toBe(true);
  });

  it('marks a control touched, or the error would never be shown', () => {
    const form = currencyForm();

    applyServerErrors(form, validationProblem);

    expect(form.controls.currName.touched).toBe(true);
  });

  it('returns an unmatched key for form-level display, with its message intact', () => {
    const form = currencyForm();

    const result = applyServerErrors(form, {
      errors: {
        CurrName: ['Please Enter Currency Name'],
        SomeFieldThisFormDoesNotHave: ['Server said no'],
      },
    });

    expect(result.applied).toEqual(['currName']);
    expect(result.unmatched).toEqual([
      { key: 'SomeFieldThisFormDoesNotHave', messages: ['Server said no'] },
    ]);
    expect(result.formLevel).toEqual(['Server said no']);
  });

  it('treats the empty ModelState key as form-level', () => {
    const form = currencyForm();

    const result = applyServerErrors(form, { errors: { '': ['The request body is invalid.'] } });

    expect(result.applied).toEqual([]);
    expect(result.formLevel).toEqual(['The request body is invalid.']);
  });

  it('surfaces a 409 business-rule message from title, verbatim', () => {
    // A 409 carries the rule message in `title` and has no errors dictionary
    // - see the BR-SO-001 comment at CurrencyController.cs:99.
    const form = currencyForm();

    const result = applyServerErrors(form, {
      status: 409,
      title: 'A currency rate for today has not been entered.',
    });

    expect(result.formLevel).toEqual(['A currency rate for today has not been entered.']);
    expect(result.applied).toEqual([]);
  });

  it('accepts a bare string as well as an array of messages', () => {
    const form = currencyForm();

    applyServerErrors(form, { errors: { CurrName: 'One message' } });

    expect(form.controls.currName.errors?.[SERVER_ERROR_KEY]).toEqual(['One message']);
  });

  it('keeps client validator errors alongside the server ones', () => {
    const form = currencyForm();
    form.controls.currName.setErrors({ maxlength: { requiredLength: 5 } });

    applyServerErrors(form, validationProblem);

    expect(Object.keys(form.controls.currName.errors ?? {})).toEqual(['maxlength', 'server']);
  });
});

describe('clearServerErrors', () => {
  it('removes only the server errors, leaving client validators alone', () => {
    const form = currencyForm();
    form.controls.currName.setErrors({ maxlength: { requiredLength: 5 } });
    applyServerErrors(form, validationProblem);

    clearServerErrors(form);

    expect(form.controls.currName.errors).toEqual({ maxlength: { requiredLength: 5 } });
    expect(form.controls.currSymbol.errors).toBeNull();
  });
});
