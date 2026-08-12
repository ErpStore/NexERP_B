import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MessageService } from 'primeng/api';
import { Button } from 'primeng/button';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Password } from 'primeng/password';
import { Toast } from 'primeng/toast';
import { AuthService } from '../../../core/auth/auth.service';

/**
 * LEARNING — Standalone components
 * No NgModule. This component lists what it needs in `imports`.
 * LEARNING — Reactive Forms
 * FormBuilder creates a FormGroup with validators (like DataAnnotations on CurrencyVM).
 */
@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, Card, InputText, Password, Button, Toast],
  providers: [MessageService],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly messages = inject(MessageService);

  readonly loading = signal(false);
  readonly error = signal<string | null>(null);

  readonly form = this.fb.nonNullable.group({
    username: ['', Validators.required],
    password: ['', Validators.required]
  });

  ngOnInit(): void {
    if (this.auth.isAuthenticated()) {
      void this.router.navigateByUrl('/currency');
    }
  }

  async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);
    this.error.set(null);

    try {
      const { username, password } = this.form.getRawValue();
      await this.auth.login(username, password);
      this.messages.add({ severity: 'success', summary: 'Welcome', detail: 'Signed in successfully' });
      await this.router.navigateByUrl('/currency');
    } catch (err: unknown) {
      const detail = this.extractError(err) ?? 'Login failed. Check credentials and API availability.';
      this.error.set(detail);
      this.messages.add({ severity: 'error', summary: 'Login failed', detail });
    } finally {
      this.loading.set(false);
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
