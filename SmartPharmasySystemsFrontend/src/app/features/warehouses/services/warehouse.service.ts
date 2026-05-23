import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiResponse, WarehouseDto, CreateWarehouseDto, UpdateWarehouseDto, WarehouseQueryDto } from '../../../core/models';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class WarehouseService {
    private readonly apiUrl = `${environment.apiUrl}/Warehouses`;

    constructor(private http: HttpClient) { }

    getAll(query?: WarehouseQueryDto): Observable<WarehouseDto[]> {
        let params = new HttpParams();
        if (query?.branchId) params = params.set('branchId', query.branchId.toString());
        if (query?.type !== undefined) params = params.set('type', query.type.toString());
        if (query?.search) params = params.set('search', query.search);
        return this.http.get<ApiResponse<any>>(this.apiUrl, { params })
            .pipe(map(r => Array.isArray(r.data) ? r.data : []));
    }

    getById(id: number): Observable<WarehouseDto> {
        return this.http.get<ApiResponse<WarehouseDto>>(`${this.apiUrl}/${id}`)
            .pipe(map(r => r.data));
    }

    getByBranchId(branchId: number): Observable<WarehouseDto[]> {
        return this.http.get<ApiResponse<any>>(`${this.apiUrl}/branch/${branchId}`)
            .pipe(map(r => Array.isArray(r.data) ? r.data : []));
    }

    getByBranchAndType(branchId: number, type: number): Observable<WarehouseDto> {
        return this.http.get<ApiResponse<WarehouseDto>>(`${this.apiUrl}/branch/${branchId}/type/${type}`)
            .pipe(map(r => r.data));
    }

    getCount(branchId: number): Observable<number> {
        return this.http.get<ApiResponse<number>>(`${this.apiUrl}/count/${branchId}`)
            .pipe(map(r => r.data));
    }

    create(dto: CreateWarehouseDto): Observable<WarehouseDto> {
        return this.http.post<ApiResponse<WarehouseDto>>(this.apiUrl, dto)
            .pipe(map(r => r.data));
    }

    update(id: number, dto: UpdateWarehouseDto): Observable<any> {
        return this.http.put<ApiResponse<any>>(`${this.apiUrl}/${id}`, dto)
            .pipe(map(r => r.data));
    }

    delete(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`)
            .pipe(map(r => r.data));
    }
}
