import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';

import { financialYearFor, recentFinancialYears } from '../../../core/navigation/financial-year';
import { FinancialYearService } from '../../../core/navigation/financial-year.service';
import { SelectComponent } from '../form/select.component';
import type { SelectOption } from '../form/types';

const YEARS_OFFERED = 5;

/**
 * M2-C03 — the header's financial-year control. Self-contained, like `app-theme-toggle`:
 * it reads and writes `FinancialYearService` directly rather than taking props, since that
 * service (like `ThemeService`) is device/session presentation state, not the auth layer
 * this directory may not import from. See `financial-year.ts`'s file header for what
 * selecting a past year does today — nothing downstream reads it yet.
 */
@Component({
  selector: 'app-financial-year-selector',
  imports: [SelectComponent, ReactiveFormsModule],
  templateUrl: './financial-year-selector.component.html',
  styleUrl: './financial-year-selector.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FinancialYearSelectorComponent {
  private readonly fy = inject(FinancialYearService);

  protected readonly options = computed<SelectOption<string>[]>(() =>
    recentFinancialYears(financialYearFor(new Date()), YEARS_OFFERED).map((year) => ({
      value: year.label,
      label: year.label,
    })),
  );

  protected readonly control = new FormControl<string>(this.fy.selected().label, {
    nonNullable: true,
  });

  constructor() {
    this.control.valueChanges.subscribe((label) => {
      const year = recentFinancialYears(financialYearFor(new Date()), YEARS_OFFERED).find(
        (candidate) => candidate.label === label,
      );
      if (year) {
        this.fy.select(year);
      }
    });
  }
}
