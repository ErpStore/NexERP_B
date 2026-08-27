import { TestBed } from '@angular/core/testing';
import { render, screen } from '@testing-library/angular';
import userEvent from '@testing-library/user-event';
import { beforeAll, describe, expect, it } from 'vitest';

import { financialYearFor } from '../../../core/navigation/financial-year';
import { FinancialYearService } from '../../../core/navigation/financial-year.service';
import { installMatchMedia } from '../form/jsdom-overlay-support';
import { FinancialYearSelectorComponent } from './financial-year-selector.component';

describe('app-financial-year-selector', () => {
  // jsdom has no window.matchMedia at all, and PrimeNG's Overlay (which app-select's
  // underlying p-select uses) reads it even to render the closed control's selected label —
  // see jsdom-overlay-support.ts's own doc comment.
  beforeAll(installMatchMedia);

  it('shows the real current financial year as the initial value', async () => {
    const { fixture } = await render(FinancialYearSelectorComponent);
    // PrimeNG's p-select resolves its closed-state label from `options` on a tick after
    // the initial synchronous render — same reason every state-changing assertion in
    // select.component.spec.ts calls detectChanges() again before reading the DOM.
    fixture.detectChanges();
    await fixture.whenStable();

    const current = financialYearFor(new Date());
    expect(screen.getByRole('combobox', { name: 'Financial year' }).textContent).toContain(
      current.label,
    );
  });

  it('selecting a different year updates FinancialYearService', async () => {
    await render(FinancialYearSelectorComponent);
    const service = TestBed.inject(FinancialYearService);
    const current = financialYearFor(new Date());
    const previousLabel = `${current.startYear - 1}-${String(current.startYear).slice(-2)}`;

    await userEvent.click(screen.getByRole('combobox', { name: 'Financial year' }));
    await userEvent.click(await screen.findByText(previousLabel));

    expect(service.selected().label).toBe(previousLabel);
  });
});
