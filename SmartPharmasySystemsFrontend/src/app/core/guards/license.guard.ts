import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map, catchError, of } from 'rxjs';
import { LicenseService } from '../services/license.service';

/**
 * License Guard — runs BEFORE the auth guard on all protected routes.
 *
 * - If the API reports isActivated === true  → allow navigation.
 * - If the API reports isActivated === false → redirect to /lock-screen.
 * - If the API call fails (network error)    → redirect to /lock-screen for safety.
 *
 * Registration: add `licenseGuard` BEFORE `authGuard` in app.routes.ts
 * on the parent route that has canActivate.
 */
export const licenseGuard: CanActivateFn = (_route, _state) => {
  const licenseService = inject(LicenseService);
  const router         = inject(Router);

  return licenseService.getStatus().pipe(
    map(status => {
      if (status.isActivated) {
        return true;
      }
      // System is not activated — redirect to the lock screen
      return router.createUrlTree(['/lock-screen']);
    }),
    catchError(() => {
      // Network/server error — fail safely by showing the lock screen
      return of(router.createUrlTree(['/lock-screen']));
    })
  );
};
