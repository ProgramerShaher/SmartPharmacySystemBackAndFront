import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, map, tap } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface PermissionGroup {
    module: string;
    moduleAr: string;
    permissions: any[];
}

@Injectable({ providedIn: 'root' })
export class PermissionService {
    private http = inject(HttpClient);

    // الصلاحيات المحملة
    private permissionsSubject = new BehaviorSubject<string[]>([]);
    public permissions$ = this.permissionsSubject.asObservable();

    // هل تم تحميل الصلاحيات؟
    private loadedSubject = new BehaviorSubject<boolean>(false);
    public permissionsLoaded$ = this.loadedSubject.asObservable();

    // Role الحالي للمستخدم
    private currentRoleSubject = new BehaviorSubject<string>('');
    public currentRole$ = this.currentRoleSubject.asObservable();

    /**
     * تحميل صلاحيات المستخدم الحالي من الـ API
     */
    loadMyPermissions(): Observable<string[]> {
        return this.http.get<{ data: { permissions: string[], role: string }, success: boolean }>(
            `${environment.apiUrl}/Users/me/permissions`
        ).pipe(
            map(res => {
                // دعم كلا التنسيقين: { data: string[] } أو { data: { permissions, role } }
                if (Array.isArray(res.data)) {
                    return res.data as unknown as string[];
                }
                return (res.data as any)?.permissions || res.data as unknown as string[] || [];
            }),
            tap(permissions => {
                this.permissionsSubject.next(permissions);
                this.loadedSubject.next(true);
            })
        );
    }

    /**
     * الحصول على صلاحيات المستخدم مجمعة حسب الوحدة
     */
    getAllPermissionsGrouped(): Observable<PermissionGroup[]> {
        return this.http.get<{ data: PermissionGroup[] }>(`${environment.apiUrl}/Permissions`).pipe(
            map(res => res.data || [])
        );
    }

    /**
     * هل يملك المستخدم صلاحية معينة؟
     * Admin يتجاوز جميع الفحوصات
     */
    hasPermission(permissionCode: string): boolean {
        if (!this.loadedSubject.value) return false;
        if (!permissionCode) return true;
        if (this.isAdmin()) return true;
        return this.permissionsSubject.value.includes(permissionCode);
    }

    /**
     * هل يملك المستخدم أي من الصلاحيات المعطاة؟
     */
    hasAnyPermission(permissionCodes: string[]): boolean {
        if (!permissionCodes || permissionCodes.length === 0) return true;
        if (this.isAdmin()) return true;
        if (!this.loadedSubject.value) return false;
        return permissionCodes.some(code => this.permissionsSubject.value.includes(code));
    }

    /**
     * هل يملك المستخدم جميع الصلاحيات المعطاة؟
     */
    hasAllPermissions(permissionCodes: string[]): boolean {
        if (!permissionCodes || permissionCodes.length === 0) return true;
        if (this.isAdmin()) return true;
        if (!this.loadedSubject.value) return false;
        return permissionCodes.every(code => this.permissionsSubject.value.includes(code));
    }

    /**
     * هل المستخدم الحالي أدمن؟
     * يتحقق من التوكن المخزن في localStorage
     */
    isAdmin(): boolean {
        try {
            const token = localStorage.getItem('auth_token');
            if (!token) return false;
            const payload = JSON.parse(atob(token.split('.')[1]));
            const role = payload?.role || payload?.Role || payload?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || '';
            return role.toLowerCase() === 'admin';
        } catch {
            return false;
        }
    }

    /**
     * مسح الصلاحيات (عند تسجيل الخروج)
     */
    clearPermissions(): void {
        this.permissionsSubject.next([]);
        this.loadedSubject.next(false);
        this.currentRoleSubject.next('');
    }

    /**
     * الصلاحيات الحالية (قيمة فورية)
     */
    get currentPermissions(): string[] {
        return this.permissionsSubject.value;
    }

    /**
     * هل تم تحميل الصلاحيات؟ (قيمة فورية)
     */
    get isLoaded(): boolean {
        return this.loadedSubject.value;
    }
}
