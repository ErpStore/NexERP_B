/**
 * LEARNING — HTTP Interceptors
 * ----------------------------
 * Interceptors sit between your services and the network (like ASP.NET middleware).
 * This one attaches `Authorization: Bearer <jwt>` to every outbound request.
 * Register with `provideHttpClient(withInterceptors([authInterceptor]))`.
 */
import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);
  const token = auth.token();

  if (!token) {
    return next(req);
  }

  const cloned = req.clone({
    setHeaders: { Authorization: `Bearer ${token}` }
  });
  return next(cloned);
};
