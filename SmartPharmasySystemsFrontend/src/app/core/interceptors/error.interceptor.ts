import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { throwError, EMPTY } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../../features/auth/services/auth.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
    const authService = inject(AuthService);

    return next(req).pipe(
        catchError((error: HttpErrorResponse) => {
            if (error.status === 401) {
                // Do not intercept or swallow 401 for login endpoints - let the login component handle the error message
                const isAuthEndpoint = req.url.toLowerCase().includes('/auth/login') || req.url.toLowerCase().includes('/login');
                if (isAuthEndpoint) {
                    return throwError(() => error);
                }

                // Auto logout if 401 Unauthorized for authenticated session requests
                authService.logout();
                return EMPTY;
            }
            return throwError(() => error);
        })
    );
};
