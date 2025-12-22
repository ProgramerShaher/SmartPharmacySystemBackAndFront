import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { ApiResponse, AuthResponse, User, UserCreateDto } from '../../../core/models';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AuthService {
    constructor(private http: HttpClient) { }

    login(credentials: { username: string, password: string }): Observable<AuthResponse> {
        return this.http.post<ApiResponse<AuthResponse>>(`${environment.apiUrl}/auth/login`, credentials).pipe(map(res => res.data));
    }

    register(user: UserCreateDto): Observable<void> {
        return this.http.post<ApiResponse<void>>(`${environment.apiUrl}/auth/register`, user).pipe(map(res => res.data));
    }

    logout(): void {
        localStorage.removeItem('token');
    }

    refreshToken(): Observable<AuthResponse> {
        return this.http.post<ApiResponse<AuthResponse>>(`${environment.apiUrl}/auth/refresh-token`, {}).pipe(map(res => res.data));
    }

    isAuthenticated(): boolean {
        return !!this.getToken();
    }

    getToken(): string | null {
        return localStorage.getItem('token');
    }

    // Keeping existing methods if they are useful helpers not conflicting
    requestPasswordReset(email: string): Observable<void> {
        return this.http.post<ApiResponse<void>>(`${environment.apiUrl}/auth/request-password-reset`, { email }).pipe(map(res => res.data));
    }

    resetPassword(token: string, newPassword: string): Observable<void> {
        return this.http.post<ApiResponse<void>>(`${environment.apiUrl}/auth/reset-password`, { token, newPassword }).pipe(map(res => res.data));
    }

    getCurrentUser(): Observable<User> {
        return this.http.get<ApiResponse<User>>(`${environment.apiUrl}/auth/me`).pipe(map(res => res.data));
    }

    changePassword(oldPassword: string, newPassword: string): Observable<void> {
        return this.http.post<ApiResponse<void>>(`${environment.apiUrl}/auth/change-password`, { oldPassword, newPassword }).pipe(map(res => res.data));
    }
}
