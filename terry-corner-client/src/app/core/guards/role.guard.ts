import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { UserRole } from '../models/auth.models';

/**
 * UX-level route protection only. Every admin API endpoint must independently
 * enforce role authorization on the server — this guard just avoids flashing
 * restricted UI to the wrong user.
 */
export function roleGuard(allowedRoles: UserRole[]): CanActivateFn {
  return () => {
    const authService = inject(AuthService);
    const router = inject(Router);

    if (!authService.isAuthenticated()) {
      return router.createUrlTree(['/account/login']);
    }

    const hasAccess = allowedRoles.some((role) => authService.hasRole(role));
    return hasAccess ? true : router.createUrlTree(['/']);
  };
}
