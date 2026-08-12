import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { ShellComponent } from './layout/shell/shell.component';
import { LoginComponent } from './features/auth/login/login.component';
import { CurrencyListComponent } from './features/currency/currency-list/currency-list.component';
import { CurrencyFormComponent } from './features/currency/currency-form/currency-form.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'currency' },
      { path: 'currency', component: CurrencyListComponent },
      { path: 'currency/create', component: CurrencyFormComponent },
      { path: 'currency/edit/:id', component: CurrencyFormComponent }
    ]
  },
  { path: '**', redirectTo: 'currency' }
];
