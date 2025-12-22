import { inject } from '@angular/core';
import { Router, CanActivateFn } from '@angular/router';

export const authGuard: CanActivateFn = (route, state) => {
    const router = inject(Router);

    if (typeof localStorage !== 'undefined') {
        const token = localStorage.getItem('token');
        if (token) {
            return true;
        }
    }

    return router.createUrlTree(['/auth/login']);
};
