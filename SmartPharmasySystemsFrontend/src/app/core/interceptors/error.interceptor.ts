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
                // Auto logout if 401 Unauthorized
                authService.logout();
                return EMPTY;
            }
            if (error.status === 403) {
                // Return error to be handled by the component or a global error handler instead of logging out
                return throwError(() => error);
            }
            return throwError(() => error);
        })
    );
};
