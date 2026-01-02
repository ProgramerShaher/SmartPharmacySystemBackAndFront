import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../../features/auth/services/auth.service';

/**
 * JWT Interceptor - Adds Authorization header to all HTTP requests
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const authService = inject(AuthService);
    const token = authService.getToken();

    // Skip adding token for auth endpoints
    if (req.url.includes('/Auth/login')) {
        return next(req);
    }

    // Add Authorization header if token exists
    if (token) {
        req = req.clone({
            setHeaders: {
                Authorization: `Bearer ${token}`
            }
    });
  }

    return next(req);
};
