import { ChangeDetectionStrategy, Component, computed, input, output } from '@angular/core';

let nextFooterId = 0;

/**
 * The dialog's footer: what is selected, and the three things that can be done
 * about it.
 *
 * Two deliberate corrections to `DetailsModal.razor:89-93`:
 *
 *  - The button labelled **"Print"** there does not print. It calls
 *    `ExcelExportService.ExportPendingListToExcel` and downloads an `.xlsx`
 *    (`:241-244`). It is labelled **Export** here. A mislabel, corrected - not
 *    a feature change.
 *  - **Update is always enabled** there (`:90`), and the only guard in
 *    `ConfirmSelection` is unreachable: it tests the result of `.ToList()` for
 *    null and the `catch` immediately rethrows (`:156-168`). The genuinely
 *    reachable case - an empty selection - is not handled at all. Here the
 *    confirm button is disabled while nothing is selected, and says why. The
 *    Blazor defect is **recorded, not fixed**: `DetailsModal.razor` keeps
 *    serving its 33 pages unchanged until each is migrated.
 */
@Component({
  selector: 'app-record-picker-footer',
  templateUrl: './record-picker-footer.component.html',
  // The dialog's stylesheet, shared rather than duplicated: the footer's
  // classes are part of the same visual block, and emulated encapsulation
  // would not otherwise reach a second component's view.
  styleUrl: './record-picker-dialog.component.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RecordPickerFooterComponent {
  readonly count = input(0);
  readonly confirmLabel = input('Add selected');
  readonly cancelLabel = input('Close');
  readonly exportLabel = input('Export');
  /** No export request supplied means no export control at all. */
  readonly canExport = input(false);
  readonly exporting = input(false);

  readonly confirmed = output<void>();
  readonly cancelled = output<void>();
  readonly exported = output<void>();

  readonly hintId = `app-record-picker-hint-${nextFooterId++}`;

  readonly disabled = computed(() => this.count() === 0);

  /**
   * Announced through `aria-live="polite"`, so the running total is heard as it
   * changes rather than discovered by hunting for it.
   */
  readonly summary = computed(() => {
    const count = this.count();
    return count === 1 ? '1 selected' : `${count} selected`;
  });
}
