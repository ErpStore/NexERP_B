import {
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  Component,
  inject,
  input,
} from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { By } from '@angular/platform-browser';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it } from 'vitest';

import { FormFieldComponent } from './form-field.component';
import { FormLayoutComponent } from './form-layout.component';
import { TextInputComponent } from './text-input.component';

/*
 * This is the test that protects M2-C07.
 *
 * The workspace is zoneless and every control is OnPush, but neither of those
 * is worth anything unless something fails when they are broken. A document
 * editor will put 200 of these controls on one screen; if a keystroke in one
 * re-checks the other 199, the screen becomes unusable at exactly the moment
 * an operator is typing fastest.
 *
 * The counter is bound in each sibling's own template, so it increments only
 * when that component's view is actually re-evaluated.
 */

const renders: number[] = [];

@Component({
  selector: 'app-probe-field',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ReactiveFormsModule, FormFieldComponent, TextInputComponent],
  template: `<span class="probe" aria-hidden="true">{{ count() }}</span>
    <app-form-field [label]="label()">
      <app-text-input [formControl]="control()" />
    </app-form-field>`,
})
class ProbeFieldComponent {
  private readonly cdr = inject(ChangeDetectorRef);

  readonly index = input.required<number>();
  readonly label = input.required<string>();
  readonly control = input.required<FormControl<string | null>>();

  /** Called from the template: counts this component's own view evaluations. */
  count(): string {
    renders[this.index()] = (renders[this.index()] ?? 0) + 1;
    return '';
  }

  /** Test-only: marks this view dirty so the next pass must re-evaluate it. */
  poke(): void {
    this.cdr.markForCheck();
  }
}

const FIELD_COUNT = 50;

/** jsdom types 20 characters through 50 OnPush controls slowly. Not a hang. */
const THIS_IS_SLOW = 60_000;

const TEMPLATE = `
  <app-form-layout [form]="form">
    @for (control of controls; track $index) {
      <app-probe-field [index]="$index" [label]="'Field ' + $index" [control]="control" />
    }
  </app-form-layout>`;

async function renderFifty() {
  const controls = Array.from({ length: FIELD_COUNT }, () => new FormControl<string | null>(null));
  const form = new FormGroup(Object.fromEntries(controls.map((c, i) => [`f${i}`, c])));
  const view = await render(TEMPLATE, {
    imports: [FormLayoutComponent, ProbeFieldComponent],
    componentProperties: { form, controls },
  });
  return { view, controls };
}

describe('change detection across a 50-control form', () => {
  beforeEach(() => {
    renders.length = 0;
  });

  it(
    're-checks no sibling control while 20 characters are typed into one',
    async () => {
      await renderFifty();

      const first = screen.getByRole('textbox', { name: 'Field 0' });
      const baseline = [...renders];
      expect(baseline).toHaveLength(FIELD_COUNT);

      await userEvent.type(first, '01234567890123456789');

      const siblingDeltas = renders
        .map((value, index) => ({ index, delta: value - (baseline[index] ?? 0) }))
        .filter(({ index }) => index !== 0)
        .filter(({ delta }) => delta !== 0);

      expect(siblingDeltas).toEqual([]);

      // 20 keystrokes across a 50-control form is slow in jsdom - and the point
      // of the test is the render count, not the clock.
    },
    THIS_IS_SLOW,
  );

  it(
    'actually typed the twenty characters, so the assertion above means something',
    async () => {
      const { controls } = await renderFifty();

      await userEvent.type(
        screen.getByRole('textbox', { name: 'Field 0' }),
        '01234567890123456789',
      );

      expect(controls[0]?.value).toBe('01234567890123456789');
    },
    THIS_IS_SLOW,
  );

  it('counts a re-check when one genuinely happens, so the probe is not inert', async () => {
    const { view } = await renderFifty();
    const probes = view.fixture.debugElement.queryAll(By.directive(ProbeFieldComponent));
    expect(probes).toHaveLength(FIELD_COUNT);

    const before = renders[7] ?? 0;

    // Marking one sibling dirty is the one thing that must move its counter.
    (probes[7]?.componentInstance as ProbeFieldComponent).poke();
    view.fixture.detectChanges();

    expect(renders[7] ?? 0).toBeGreaterThan(before);
  });
});
