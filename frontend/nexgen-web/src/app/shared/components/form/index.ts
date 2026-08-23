/**
 * The form layer (M2-C04-02). Specification: KB-051 Forms.
 *
 * Two rules govern everything exported here:
 *   1. Validation is displayed in exactly one place - `app-form-field`.
 *   2. No control implements an ERP business rule. The server stays
 *      authoritative for validation, calculation, permissions and numbering.
 */
export { FormLayoutComponent } from './form-layout.component';
export { FormSectionComponent } from './form-section.component';
export { FormFieldComponent } from './form-field.component';

export { TextInputComponent } from './text-input.component';
export { TextareaComponent } from './textarea.component';
export { NumberInputComponent } from './number-input.component';
export { CurrencyInputComponent } from './currency-input.component';
export { AmountOrPercentInputComponent } from './amount-or-percent-input.component';
export { SelectComponent } from './select.component';
export { MultiSelectComponent } from './multi-select.component';
export { ComboboxComponent } from './combobox.component';
export { DatePickerComponent } from './date-picker.component';
export { DateRangePickerComponent } from './date-range-picker.component';
export { CheckboxComponent } from './checkbox.component';
export { RadioGroupComponent } from './radio-group.component';
export { SwitchComponent } from './switch.component';
export { FileUploadComponent } from './file-upload.component';

export { applyServerErrors, clearServerErrors } from './server-validation';
export type {
  ProblemDetailsLike,
  ServerErrorResult,
  UnmatchedServerError,
} from './server-validation';

export { defaultErrorMessage, SERVER_ERROR_KEY } from './error-messages';

export {
  DATE_FORMAT,
  DECIMAL_PORT,
  DEFAULT_PRECISION_POLICY,
  FormFieldContext,
  PRECISION_POLICY,
  SERVER_DEFAULT_DECIMAL_PLACES,
} from './types';
export type {
  AmountOrPercent,
  ComboboxLoader,
  DecimalPort,
  Money,
  PrecisionPolicy,
  Qty,
  ScaledDecimal,
  SelectOption,
} from './types';
