import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { ToastService } from '../services/toast.service';
import { AuthService } from '../services/auth.service';

const FALLBACK_MESSAGE = 'Something went wrong. Please try again.';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toast = inject(ToastService);
  const router = inject(Router);
  const authService = inject(AuthService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401) {
        authService.logout();
        router.navigate(['/account/login']);
        toast.error('Your session has expired. Please sign in again.');
      } else if (error.status === 403) {
        toast.error("You don't have permission to do that.");
      } else if (error.status === 0) {
        toast.error("Can't reach the server. Check your connection and try again.");
      } else {
        const problemDetail = (error.error?.detail as string) ?? (error.error?.title as string);
        toast.error(problemDetail ?? FALLBACK_MESSAGE);
      }
      return throwError(() => error);
    }),
  );
};
