import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface RoleDto {
    id: number;
    name: string;
    description?: string;
    nameAr?: string;
    color?: string;
    isSystemRole: boolean;
    isActive: boolean;
    createdAt?: string;
    permissionIds?: number[];
}

@Injectable({ providedIn: 'root' })
export class RolesService {
    private http = inject(HttpClient);
    private apiUrl = `${environment.apiUrl}/Roles`;

    getAll(): Observable<{ data: RoleDto[] }> {
        return this.http.get<{ data: RoleDto[] }>(this.apiUrl);
    }

    getById(id: number): Observable<{ data: RoleDto }> {
        return this.http.get<{ data: RoleDto }>(`${this.apiUrl}/${id}`);
    }

    create(dto: any): Observable<{ data: RoleDto }> {
        return this.http.post<{ data: RoleDto }>(this.apiUrl, dto);
    }

    update(id: number, dto: any): Observable<any> {
        return this.http.put(`${this.apiUrl}/${id}`, dto);
    }

    delete(id: number): Observable<any> {
        return this.http.delete(`${this.apiUrl}/${id}`);
    }

    updatePermissions(id: number, permissionIds: number[]): Observable<any> {
        return this.http.put(`${this.apiUrl}/${id}/permissions`, permissionIds);
    }

    getPermissions(id: number): Observable<{ data: number[] }> {
        return this.http.get<{ data: number[] }>(`${this.apiUrl}/${id}/permissions`);
    }
}
