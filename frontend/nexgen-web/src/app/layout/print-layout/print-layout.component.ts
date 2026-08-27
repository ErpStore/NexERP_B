import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

/**
 * M2-C03 — no chrome at all: for print/preview routes, where a header, sidebar or
 * breadcrumb trail would print alongside the document and waste the page. Built now, per
 * KB-050's structure, so a later print/preview route never has to unpick the authenticated
 * shell first — it attaches here from day one.
 */
@Component({
  selector: 'app-print-layout',
  imports: [RouterOutlet],
  template: `<router-outlet />`,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PrintLayoutComponent {}
