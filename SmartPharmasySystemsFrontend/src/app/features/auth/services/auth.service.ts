import { Injectable, PLATFORM_ID, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { Observable, BehaviorSubject } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import {
    ApiResponse,
    LoginRequest,
    LoginResponse,
    ChangePasswordRequest,
    CurrentUserResponse
} from '../../../core/models';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
    private currentUserSubject = new BehaviorSubject<LoginResponse | null>(null);
    public currentUser$ = this.currentUserSubject.asObservable();
    private platformId = inject(PLATFORM_ID);
    private isBrowser: boolean;

    constructor(
        private http: HttpClient,
        private router: Router
    ) {
        this.isBrowser = isPlatformBrowser(this.platformId);

        // Load user from localStorage on init (only in browser)
        if (this.isBrowser) {
            const storedUser = localStorage.getItem('current_user');
            if (storedUser) {
                try {
                    this.currentUserSubject.next(JSON.parse(storedUser));
                } catch (e) {
                    console.error('Error parsing stored user:', e);
                }
            }
        }
    }

    /**
     * Login - POST /api/Auth/login
     */
    login(credentials: LoginRequest): Observable<LoginResponse> {
        return this.http.post<ApiResponse<LoginResponse>>(
            `${environment.apiUrl}/Auth/login`,
            credentials
        ).pipe(
            map(response => response.data),
            tap(loginResponse => {
                // Store token and user info
                this.setToken(loginResponse.token);
                this.setCurrentUser(loginResponse);
                this.currentUserSubject.next(loginResponse);
            })
        );
    }


    /**
     * Get current user - GET /api/Auth/me
     * Maps CurrentUserResponse to User interface
     */
    getCurrentUser(): Observable<any> {
        return this.http.get<ApiResponse<CurrentUserResponse>>(
            `${environment.apiUrl}/Auth/me`
        ).pipe(
            map(response => {
                const data = response.data;
                // Map CurrentUserResponse to User-compatible object
                return {
                    id: data.userId,
                    fullName: data.fullName || data.username,
                    username: data.username,
                    roleId: data.roleId,
                    roleName: data.role,
                    status: data.status || 1, // Default to Active
                    email: data.email,
                    phoneNumber: data.phoneNumber,
                    createdAt: data.createdAt || new Date().toISOString(),
                    isDeleted: data.isDeleted || false,
                    // Additional properties from CurrentUserResponse
                    isAdmin: data.isAdmin,
                    isPharmacist: data.isPharmacist
                };
            })
        );
    }

    /**
     * Change password - POST /api/Auth/change-password
     */
    changePassword(dto: ChangePasswordRequest): Observable<any> {
        return this.http.post<ApiResponse<any>>(
            `${environment.apiUrl}/Auth/change-password`,
            dto
        ).pipe(
            map(response => response.data)
        );
    }

    /**
     * Logout
     */
    logout(): void {
        if (this.isBrowser) {
            localStorage.removeItem('auth_token');
            localStorage.removeItem('current_user');
        }
        this.currentUserSubject.next(null);
        this.router.navigate(['/auth/login']);
    }

    /**
     * Check if user is authenticated
     */
    isAuthenticated(): boolean {
        return !!this.getToken();
    }

    /**
     * Get stored token
     */
    getToken(): string | null {
        if (this.isBrowser) {
            return localStorage.getItem('auth_token');
        }
        return null;
    }

    /**
     * Get current user value (synchronous)
     */
    get currentUserValue(): LoginResponse | null {
        return this.currentUserSubject.value;
    }

    /**
     * Check if current user is admin
     */
    isAdmin(): boolean {
        const user = this.currentUserValue;
        return user?.roleName?.toLowerCase() === 'admin';
    }

    /**
     * Check if current user is pharmacist
     */
    isPharmacist(): boolean {
        const user = this.currentUserValue;
        return user?.roleName?.toLowerCase() === 'pharmacist';
    }

    /**
     * Private: Set token in localStorage
     */
    private setToken(token: string): void {
        if (this.isBrowser) {
            localStorage.setItem('auth_token', token);
        }
    }

    /**
     * Private: Set current user in localStorage
     */
    private setCurrentUser(user: LoginResponse): void {
        if (this.isBrowser) {
            localStorage.setItem('current_user', JSON.stringify(user));
        }
    }
}
