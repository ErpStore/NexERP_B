import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { ConfirmDialog } from 'primeng/confirmdialog';
import { InputText } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { TableModule } from 'primeng/table';
import { Tag } from 'primeng/tag';
import { Toast } from 'primeng/toast';
import { firstValueFrom } from 'rxjs';
import { CurrencyVm } from '../models/currency.model';
import { CurrencyService } from '../currency.service';

/**
 * LEARNING — Signals for list UI state
 * `rows`, `totalCount`, `loading`, `error` are signals. Templates call them as functions: `loading()`.
 * HttpClient still returns Observables — we convert with firstValueFrom then `.set()` the signals.
 */
@Component({
  selector: 'app-currency-list',
  standalone: true,
  imports: [
    DatePipe,
    FormsModule,
    RouterLink,
    Card,
    TableModule,
    Button,
    InputText,
    Select,
    Tag,
    Toast,
    ConfirmDialog
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './currency-list.component.html',
  styleUrl: './currency-list.component.scss'
})
export class CurrencyListComponent implements OnInit {
  private readonly currencyService = inject(CurrencyService);
  private readonly messages = inject(MessageService);
  private readonly confirm = inject(ConfirmationService);

  readonly rows = signal<CurrencyVm[]>([]);
  readonly totalCount = signal(0);
  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  pageNumber = 1;
  pageSize = 10;
  currName = '';

  readonly pageSizeOptions = [
    { label: '10 per page', value: 10 },
    { label: '20 per page', value: 20 },
    { label: '50 per page', value: 50 }
  ];

  ngOnInit(): void {
    void this.load();
  }

  async load(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const result = await firstValueFrom(
        this.currencyService.search({
          pageNumber: this.pageNumber,
          pageSize: this.pageSize,
          currName: this.currName.trim() || undefined
        })
      );
      this.rows.set(result.items);
      this.totalCount.set(result.totalCount);
    } catch (err: unknown) {
      const detail = this.extractError(err) ?? 'Failed to load currencies.';
      this.error.set(detail);
      this.messages.add({ severity: 'error', summary: 'Load failed', detail });
    } finally {
      this.loading.set(false);
    }
  }

  onPageSizeChange(): void {
    this.pageNumber = 1;
    void this.load();
  }

  search(): void {
    this.pageNumber = 1;
    void this.load();
  }

  refresh(): void {
    void this.load();
  }

  prevPage(): void {
    if (this.pageNumber <= 1) return;
    this.pageNumber -= 1;
    void this.load();
  }

  nextPage(): void {
    const maxPage = Math.max(1, Math.ceil(this.totalCount() / this.pageSize));
    if (this.pageNumber >= maxPage) return;
    this.pageNumber += 1;
    void this.load();
  }

  confirmDelete(row: CurrencyVm): void {
    this.confirm.confirm({
      message: `Delete currency "${row.currName}"?`,
      header: 'Confirm delete',
      icon: 'pi pi-exclamation-triangle',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => void this.delete(row.currId)
    });
  }

  private async delete(id: number): Promise<void> {
    try {
      await firstValueFrom(this.currencyService.delete(id));
      this.messages.add({ severity: 'success', summary: 'Deleted', detail: 'Currency deleted.' });
      await this.load();
    } catch (err: unknown) {
      const detail = this.extractError(err) ?? 'Delete failed.';
      this.messages.add({ severity: 'error', summary: 'Delete blocked', detail });
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
