import { Component } from '@angular/core';
import { CardModule } from 'primeng/card';

import { APP_INFO } from '../../core/config/app-config';

/**
 * Scaffold-only landing page. It exists to prove the provider stack composes
 * and that lazy routing works; M2-C03 replaces it with the real app shell.
 */
@Component({
  selector: 'app-placeholder',
  imports: [CardModule],
  templateUrl: './placeholder.component.html',
  styleUrl: './placeholder.component.scss',
})
export class PlaceholderComponent {
  protected readonly appName = APP_INFO.name;
  protected readonly version = APP_INFO.version;
}
