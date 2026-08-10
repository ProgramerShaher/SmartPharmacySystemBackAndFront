import { ApplicationConfig } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideClientHydration } from '@angular/platform-browser';
import { ConfirmationService, MessageService } from 'primeng/api';

import { routes } from './app.routes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';

import { provideAnimations } from '@angular/platform-browser/animations';
import { provideToastr } from 'ngx-toastr';
import { APP_INITIALIZER } from '@angular/core';
import { AuthService } from './features/auth/services/auth.service';
import { PermissionService } from './core/services/permission.service';
import { firstValueFrom, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

export function initializeApp(authService: AuthService, permissionService: PermissionService) {
  return () => {
    if (authService.isAuthenticated()) {
      return firstValueFrom(permissionService.loadMyPermissions().pipe(
        catchError(() => of([]))
      ));
    }
    return Promise.resolve(true);
  };
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideClientHydration(),
    provideAnimations(),
    provideToastr({
      timeOut: 3000,
      positionClass: 'toast-top-right',
      preventDuplicates: true,
      progressBar: true
    }),
    provideHttpClient(
      withFetch(),
      withInterceptors([authInterceptor, errorInterceptor])
    ),
    MessageService,
    ConfirmationService,
    {
      provide: APP_INITIALIZER,
      useFactory: initializeApp,
      deps: [AuthService, PermissionService],
      multi: true
    }
  ]
};
