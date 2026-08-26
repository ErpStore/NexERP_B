import {
  ChangeDetectionStrategy,
  Component,
  ElementRef,
  computed,
  inject,
  input,
  viewChild,
} from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { MenuModule } from 'primeng/menu';
import type { MenuItem } from 'primeng/api';
import type { Menu } from 'primeng/menu';

import { DataGridErrorComponent } from './data-grid-error.component';
import {
  GRID_EXPORT_XLSX,
  GridExportService,
  type GridExportOperation,
} from './grid-export.service';
import type { DataGridState } from './data-grid.model';

/** A format the toolbar offers, and the words shown for it. */
export interface GridExportFormat {
  readonly format: string;
  readonly label: string;
}

/**
 * The only format offered by default, because it is the only one the server
 * produces (`CurrencyExcelController.cs:48`). See {@link GRID_EXPORT_XLSX}.
 */
export const GRID_EXPORT_DEFAULT_FORMATS: readonly GridExportFormat[] = [
  { format: GRID_EXPORT_XLSX, label: 'Excel' },
];

/**
 * The grid's toolbar action: **export the current view**.
 *
 * It mounts into `DataGrid`'s `#toolbar` slot, which it shares with M2-C05-02's
 * column-preferences control. Export is treated as a *secondary* action and
 * Clear filters as the primary one, per KB-051 Do not - "never hide a primary
 * action in an overflow menu".
 *
 * The request carries the grid's current **sort and filters** and no paging, so
 * what the user exports is what they are looking at. An unfiltered export of a
 * filtered view is a data-integrity bug, not a UX wrinkle.
 *
 * A failure is rendered by `app-data-grid-error`, which means a 409 from the
 * server's 10,000-row ceiling (`CurrencyExcelController.cs:58`, `:112-116`)
 * reaches the user as the server's own sentence - naming the row count and what
 * to do - rather than as a generic toast.
 */
@Component({
  selector: 'app-data-grid-toolbar',
  templateUrl: './data-grid-toolbar.component.html',
  styleUrl: './data-grid-states.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [ButtonModule, MenuModule, DataGridErrorComponent],
  providers: [GridExportService],
})
export class DataGridToolbarComponent {
  /** Per-toolbar, never shared: two grids must not disable each other's button. */
  readonly exporter = inject(GridExportService);

  /** The grid's current query state. Bind `DataGridQueryState.state()`. */
  readonly state = input.required<DataGridState>();
  /** One call to the server's export endpoint, over the generated client. */
  readonly exportOperation = input.required<GridExportOperation>();
  readonly formats = input<readonly GridExportFormat[]>(GRID_EXPORT_DEFAULT_FORMATS);
  /** Filename without an extension, used when the server did not supply one. */
  readonly fallbackBaseName = input('export');
  readonly label = input('Export');

  protected readonly trigger = viewChild<ElementRef<HTMLButtonElement>>('exportTrigger');
  protected readonly menu = viewChild<Menu>('exportMenu');

  readonly singleFormat = computed(() => this.formats().length <= 1);

  /** What the button says while a download is being prepared. */
  readonly busyLabel = computed(() => `${this.label()} in progress`);

  readonly menuItems = computed<MenuItem[]>(() =>
    this.formats().map((entry) => ({
      label: entry.label,
      command: () => this.run(entry.format),
    })),
  );

  onTriggerClick(event: Event): void {
    if (this.singleFormat()) {
      this.run(this.formats()[0]?.format ?? GRID_EXPORT_XLSX);
      return;
    }
    this.menu()?.toggle(event);
  }

  run(format: string): void {
    this.exporter.exportAs({
      operation: this.exportOperation(),
      state: this.state(),
      format,
      fallbackBaseName: this.fallbackBaseName(),
    });
    // Focus returns to the control that opened the menu, not to <body>.
    this.trigger()?.nativeElement.focus();
  }
}
