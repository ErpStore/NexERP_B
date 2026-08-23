import {
  ChangeDetectionStrategy,
  Component,
  computed,
  forwardRef,
  input,
  signal,
} from '@angular/core';
import { NG_VALUE_ACCESSOR } from '@angular/forms';
import { FileUpload, type FileSelectEvent } from 'primeng/fileupload';

import { BaseFormControl } from './base-control';

/**
 * Collects `File` objects. **It performs no transport.**
 *
 * `customUpload` is on and no `url` is set, so PrimeNG never issues a
 * request. The upload endpoints are M2-B06; until a screen wires them this
 * control hands the caller `File` objects and stops. Inventing a transport
 * here would put an API contract inside a form control.
 *
 * Empty state is the drop target plus the accepted types - never blank space,
 * because "what am I allowed to attach" is the question a user actually has.
 *
 * The loading state is **caller-driven**, for the same reason: a control that
 * starts no request cannot know when one is in flight. A screen that wires the
 * M2-B06 endpoints sets `[loading]` while its own transfer runs and the control
 * renders the row - exactly as `app-select` renders a caller-supplied option
 * load. The state is present; only its trigger belongs to the caller.
 */
@Component({
  selector: 'app-file-upload',
  templateUrl: './file-upload.component.html',
  styleUrl: './control.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [FileUpload],
  providers: [
    { provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => FileUploadComponent), multi: true },
  ],
})
export class FileUploadComponent extends BaseFormControl<File[]> {
  /** Comma-separated accept list, e.g. `.pdf,.xlsx`. */
  readonly accept = input<string | undefined>(undefined);
  /** Maximum size per file, in bytes. Rejection is shown, not swallowed. */
  readonly maxFileSize = input<number | undefined>(undefined);
  readonly multiple = input(true);
  readonly chooseLabel = input('Choose files');
  /**
   * Set by the caller while it is transferring the collected files. The
   * control never sets this itself - it performs no transport.
   */
  readonly loading = input(false);

  readonly rejected = signal<string | null>(null);

  readonly files = computed(() => this.value() ?? []);

  protected override controlElementSelector = 'input[type="file"]';

  onSelect(event: FileSelectEvent): void {
    this.rejected.set(null);
    const selected = Array.from(event.files);
    this.emitValue(this.multiple() ? [...this.files(), ...selected] : selected);
    this.markTouched();
  }

  onError(): void {
    // PrimeNG rejects on accept/size before it reaches onSelect; say why.
    this.rejected.set('One or more files were rejected. Check the type and size limits.');
  }

  remove(index: number): void {
    this.emitValue(this.files().filter((_, i) => i !== index));
    this.markTouched();
  }
}
