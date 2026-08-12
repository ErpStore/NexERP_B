import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MessageService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { Toast } from 'primeng/toast';
import { firstValueFrom } from 'rxjs';
import { CurrencyVm } from '../models/currency.model';
import { CurrencyService } from '../currency.service';

/**
 * LEARNING — Reactive Forms mirror C# DataAnnotations
 * Validators.required / pattern ≈ [Required] / [RegularExpression] on CurrencyVM.
 * `signal` tracks loading and edit mode; the FormGroup owns field values.
 */
@Component({
  selector: 'app-currency-form',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, Card, InputText, Select, Button, Toast],
  providers: [MessageService],
  templateUrl: './currency-form.component.html',
  styleUrl: './currency-form.component.scss'
})
export class CurrencyFormComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly currencyService = inject(CurrencyService);
  private readonly messages = inject(MessageService);

  readonly loading = signal(false);
  readonly saving = signal(false);
  readonly isEdit = signal(false);
  readonly currId = signal(0);

  readonly symbolOptions = [
    { label: '₹', value: '₹' },
    { label: '$', value: '$' },
    { label: '€', value: '€' },
    { label: '£', value: '£' },
    { label: '¥', value: '¥' },
    { label: '₩', value: '₩' },
    { label: '₿', value: '₿' },
    { label: '₽', value: '₽' }
  ];

  readonly form = this.fb.nonNullable.group({
    currName: ['', [Validators.required, Validators.maxLength(100)]],
    currSub: ['', [Validators.required, Validators.maxLength(100)]],
    symbol: ['₹', [Validators.required, Validators.pattern(/[$€₹¥£₩₿₽]/)]]
  });

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      const id = Number(idParam);
      this.currId.set(id);
      this.isEdit.set(true);
      void this.load(id);
    }
  }

  async load(id: number): Promise<void> {
    this.loading.set(true);
    try {
      const vm = await firstValueFrom(this.currencyService.getById(id));
      this.form.patchValue({
        currName: vm.currName ?? '',
        currSub: vm.currSub ?? '',
        symbol: vm.symbol ?? '₹'
      });

      if (vm.isSystemDefined) {
        this.form.disable();
        this.messages.add({
          severity: 'warn',
          summary: 'System currency',
          detail: 'System-defined currencies cannot be modified.'
        });
      }
    } catch (err: unknown) {
      const detail = this.extractError(err) ?? 'Failed to load currency.';
      this.messages.add({ severity: 'error', summary: 'Load failed', detail });
    } finally {
      this.loading.set(false);
    }
  }

  async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    const raw = this.form.getRawValue();
    const payload: CurrencyVm = {
      currId: this.currId(),
      currName: raw.currName,
      currSub: raw.currSub,
      symbol: raw.symbol,
      isSystemDefined: false,
      createdBy: null,
      createdDate: null,
      modifiedBy: null,
      modifiedDate: null
    };

    try {
      if (this.isEdit()) {
        await firstValueFrom(this.currencyService.update(this.currId(), payload));
        this.messages.add({ severity: 'success', summary: 'Updated', detail: 'Currency updated.' });
      } else {
        await firstValueFrom(this.currencyService.create(payload));
        this.messages.add({ severity: 'success', summary: 'Created', detail: 'Currency created.' });
      }
      await this.router.navigateByUrl('/currency');
    } catch (err: unknown) {
      const detail = this.extractError(err) ?? 'Save failed.';
      this.messages.add({ severity: 'error', summary: 'Save failed', detail });
    } finally {
      this.saving.set(false);
    }
  }

  private extractError(err: unknown): string | null {
    if (typeof err === 'object' && err !== null && 'error' in err) {
      const body = (err as { error?: { message?: string } }).error;
      return body?.message ?? null;
    }
    return null;
  }
}
