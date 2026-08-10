import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { PermissionService } from '../services/permission.service';
import { map, take } from 'rxjs/operators';

export const permissionGuard: CanActivateFn = (route, state) => {
    const permissionService = inject(PermissionService);
    const router = inject(Router);

    const requiredPermission = route.data['permission'] as string;

    // لا توجد صلاحية مطلوبة → مسموح
    if (!requiredPermission) return true;

    // Admin يتجاوز كل الصلاحيات
    if (permissionService.isAdmin()) return true;

    // إذا تم تحميل الصلاحيات بالفعل → تحقق فوري
    if (permissionService.isLoaded) {
        if (permissionService.hasPermission(requiredPermission)) {
            return true;
        }
        return router.createUrlTree(['/unauthorized']);
    }

    // انتظر حتى يتم تحميل الصلاحيات
    return permissionService.permissionsLoaded$.pipe(
        take(1),
        map(loaded => {
            if (!loaded) return router.createUrlTree(['/unauthorized']);
            if (permissionService.hasPermission(requiredPermission)) return true;
            return router.createUrlTree(['/unauthorized']);
        })
    );
};
