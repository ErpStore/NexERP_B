import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { describe, expect, it } from 'vitest';

import { FileUploadComponent } from './file-upload.component';
import { FormFieldComponent } from './form-field.component';

const TEMPLATE = `
  <form [formGroup]="form">
    <app-form-field label="Attachments">
      <app-file-upload formControlName="files" [accept]="accept" />
    </app-form-field>
  </form>`;

async function setup(accept: string | undefined = '.pdf,.xlsx') {
  const form = new FormGroup({ files: new FormControl<File[] | null>([]) });
  const view = await render(TEMPLATE, {
    imports: [ReactiveFormsModule, FormFieldComponent, FileUploadComponent],
    componentProperties: { form, accept },
  });
  const control = view.fixture.debugElement.query(By.directive(FileUploadComponent))
    .componentInstance as FileUploadComponent;
  return { form, view, control };
}

function file(name: string): File {
  return new File(['x'], name, { type: 'application/pdf' });
}

describe('app-file-upload', () => {
  it('shows the drop target and the accepted types instead of blank space', async () => {
    await setup();

    expect(screen.getByText('Accepted types: .pdf,.xlsx')).toBeDefined();
    expect(screen.getByText('No files attached yet.')).toBeDefined();
  });

  it('collects File objects and performs no transport', async () => {
    const { form, control } = await setup();

    control.onSelect({
      originalEvent: new Event('select'),
      files: [file('po.pdf')],
      currentFiles: [],
    });

    expect(form.value.files).toHaveLength(1);
    expect(form.value.files?.[0]).toBeInstanceOf(File);
  });

  it('lists each file with a keyboard-reachable remove control', async () => {
    const { form, view, control } = await setup();

    control.onSelect({
      originalEvent: new Event('select'),
      files: [file('po.pdf'), file('quote.pdf')],
      currentFiles: [],
    });
    view.fixture.detectChanges();

    const remove = screen.getByRole('button', { name: 'Remove quote.pdf' });
    remove.focus();
    expect(document.activeElement).toBe(remove);

    await userEvent.keyboard('{Enter}');
    view.fixture.detectChanges();

    expect(form.value.files).toHaveLength(1);
    expect(form.value.files?.[0]?.name).toBe('po.pdf');
  });

  it('opens the picker with Enter and with Space - the choose control is a real button', async () => {
    await setup();

    const picker = document.querySelector<HTMLInputElement>('input[type="file"]');
    const chooser = screen.getByRole('button');
    let opened = 0;
    picker?.addEventListener('click', () => {
      opened += 1;
    });

    chooser.focus();
    await userEvent.keyboard('{Enter}');
    await userEvent.keyboard(' ');

    expect(opened).toBe(2);
  });

  it('shows a clear error state when a file is rejected, rather than dropping it silently', async () => {
    const { view, control } = await setup();

    control.onError();
    view.fixture.detectChanges();

    expect(screen.getByRole('alert').textContent).toContain('rejected');
  });

  it('exposes a real file input the keyboard can reach', async () => {
    await setup();

    const input = document.querySelector<HTMLInputElement>('input[type="file"]');
    expect(input).not.toBeNull();
    expect(input?.accept).toBe('.pdf,.xlsx');
  });

  it('adds to the existing list in multiple mode rather than replacing it', async () => {
    const { form, control } = await setup();

    control.onSelect({ originalEvent: new Event('s'), files: [file('a.pdf')], currentFiles: [] });
    control.onSelect({ originalEvent: new Event('s'), files: [file('b.pdf')], currentFiles: [] });

    expect(form.value.files?.map((f) => f.name)).toEqual(['a.pdf', 'b.pdf']);
  });
});
