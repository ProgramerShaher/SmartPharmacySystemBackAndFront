import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { throwError, EMPTY } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../../features/auth/services/auth.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
    const authService = inject(AuthService);

    return next(req).pipe(
        catchError((error: HttpErrorResponse) => {
            if (error.status === 401 || error.status === 403) {
                // Auto logout if 401 Unauthorized or 403 Forbidden
                authService.logout();
                return EMPTY;
            }
            return throwError(() => error);
        })
    );
};
