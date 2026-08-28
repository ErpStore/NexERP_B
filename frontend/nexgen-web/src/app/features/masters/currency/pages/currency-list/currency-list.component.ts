import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, computed, inject, input } from '@angular/core';
import { Router } from '@angular/router';

import type { ApiProblem } from '@/app/core/http/api-problem';
import { PermissionService } from '@/app/core/auth/permission.service';
import { HasRightDirective } from '@/app/core/auth/has-right.directive';
import { CurrencyFormDrawerComponent } from '../../components/currency-form-drawer/currency-form-drawer.component';
import { CurrencyFeatureService } from '../../currency.service';
import type { CurrencyVM } from '../../models';
import { DataGridComponent } from '@/app/shared/components/data-grid/data-grid.component';
import { DataGridToolbarComponent } from '@/app/shared/components/data-grid/data-grid-toolbar.component';
import { createDataGridQueryState } from '@/app/shared/components/data-grid/data-grid-query-state';
import type { DataGridColumn } from '@/app/shared/components/data-grid/data-grid.model';
import { EmptyStateComponent } from '@/app/shared/components/feedback/empty-state.component';
import { PermissionDeniedStateComponent } from '@/app/shared/components/feedback/permission-denied-state.component';
import { ToastService } from '@/app/shared/components/feedback/toast.service';
import { ConfirmDialogService } from '@/app/shared/components/overlay/confirm-dialog.service';
import { PageHeaderComponent } from '@/app/shared/components/page-header/page-header.component';

const CURRENCY_ROUTE = '/masters/currencies';

/** The server's `ProblemDetails`, as `core/http/error.interceptor.ts` normalised it. */
function problemFrom(error: unknown): ApiProblem {
  if (error instanceof HttpErrorResponse && error.error && typeof error.error === 'object') {
    return error.error as ApiProblem;
  }
  return {};
}

/**
 * M2-D01 — the Currency Master's list screen, and the routed component for all three
 * `currency.routes.ts` paths (see that file's own doc comment for why one component serves
 * three routes). `requireScreen('Currency', 'view')` gates the route at the authentication
 * level only (`auth.guard.ts`'s own doc comment); the missing-*right* rendering below is this
 * component's job, the same deny-by-default pattern `DashboardComponent` established for the
 * one other route that carries it today.
 */
@Component({
  selector: 'app-currency-list',
  templateUrl: './currency-list.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    PageHeaderComponent,
    EmptyStateComponent,
    PermissionDeniedStateComponent,
    DataGridComponent,
    DataGridToolbarComponent,
    HasRightDirective,
    CurrencyFormDrawerComponent,
  ],
})
export class CurrencyListComponent {
  private readonly router = inject(Router);
  private readonly currencyApi = inject(CurrencyFeatureService);
  private readonly permissions = inject(PermissionService);
  private readonly confirmDialog = inject(ConfirmDialogService);
  private readonly toast = inject(ToastService);

  /** Bound by `withComponentInputBinding()` from the `:id` route param — `new`/`''` never
   * supply one. */
  readonly id = input<string | undefined>();
  /** Bound from `currency.routes.ts`'s route `data` — `undefined` on the list route itself. */
  readonly drawerMode = input<'create' | 'edit' | undefined>();

  protected readonly hasNoRights = this.permissions.hasNoRights;
  protected readonly currencyRight = this.permissions.forScreen('Currency');

  protected readonly drawerVisible = computed(() => this.drawerMode() !== undefined);
  protected readonly editingId = computed(() => {
    const raw = this.id();
    if (raw === undefined) {
      return null;
    }
    const parsed = Number(raw);
    return Number.isInteger(parsed) ? parsed : null;
  });

  protected readonly columns: readonly DataGridColumn<CurrencyVM>[] = [
    { field: 'currName', title: 'Currency name', filter: 'text' },
    { field: 'currSub', title: 'Sub currency name', filter: 'text' },
    { field: 'symbol', title: 'Symbol' },
    { field: 'isSystemDefined', title: 'System-defined', width: '140px' },
    { field: 'createdBy', title: 'Created by', filter: 'text' },
    { field: 'createdDate', title: 'Created date', isDate: true, filter: 'date' },
  ];

  /** `filterNames` is the closed set `CurrencyQuery.cs` supports — every one of them, so the
   * URL never grows a filter the API would reject. */
  protected readonly query = createDataGridQueryState<CurrencyVM>({
    source: (wireQuery) => this.currencyApi.list(wireQuery),
    filterNames: ['currName', 'createdBy', 'fromDate', 'toDate'],
  });

  protected readonly getRowId = (row: CurrencyVM): number => row.currId ?? 0;

  protected readonly exportOperation = this.currencyApi.exportOperation;

  onNew(): void {
    void this.router.navigate([CURRENCY_ROUTE, 'new'], { queryParamsHandling: 'preserve' });
  }

  onRowActivate(row: CurrencyVM): void {
    if (row.currId === undefined) {
      return;
    }
    void this.router.navigate([CURRENCY_ROUTE, row.currId], { queryParamsHandling: 'preserve' });
  }

  /**
   * The drawer's own `visible` is a one-way binding — the route, not a local signal, is the
   * source of truth (`currencyRoutes`'s own doc comment). This only reacts to the drawer
   * closing *itself* (`Esc`, the backdrop, its close icon, or the Cancel button, all of which
   * set `visible` to `false` internally) — a successful save closes through `onSaved()`
   * instead, which navigates directly rather than waiting for this to fire.
   */
  onDrawerVisibleChange(visible: boolean): void {
    if (!visible && this.drawerVisible()) {
      void this.router.navigate([CURRENCY_ROUTE], { queryParamsHandling: 'preserve' });
    }
  }

  onSaved(): void {
    void this.router.navigate([CURRENCY_ROUTE], { queryParamsHandling: 'preserve' });
  }

  async onDelete(row: CurrencyVM): Promise<void> {
    if (row.currId === undefined) {
      return;
    }
    const { confirmed } = await this.confirmDialog.confirm({
      header: 'Delete currency',
      message: `Delete "${row.currName}"? This cannot be undone.`,
      confirmLabel: 'Delete',
      destructive: true,
    });
    if (!confirmed) {
      return;
    }
    this.currencyApi.remove(row.currId).subscribe({
      next: () => {
        this.toast.success('Currency deleted.');
        this.query.refresh();
      },
      error: (error: unknown) => {
        // The server's message, verbatim — a delete refusal (409, BR-SO-001-shaped) carries
        // the reason (e.g. "in use") in `title`, never a generic "Cannot delete" string.
        const problem = problemFrom(error);
        this.toast.error(problem.title ?? 'The currency could not be deleted.');
      },
    });
  }
}
