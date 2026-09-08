import { HttpInterceptorFn } from '@angular/common/http';
import { inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

/**
 * JWT Interceptor - Adds Authorization header to all HTTP requests
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
    const platformId = inject(PLATFORM_ID);
    let token = null;

    if (isPlatformBrowser(platformId)) {
        token = localStorage.getItem('auth_token');
    }

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
