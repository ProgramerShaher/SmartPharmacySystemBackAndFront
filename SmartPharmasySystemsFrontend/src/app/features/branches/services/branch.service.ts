import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiResponse, BranchDto, CreateBranchDto, UpdateBranchDto, BranchQueryDto } from '../../../core/models';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class BranchService {
    private readonly apiUrl = `${environment.apiUrl}/Branches`;

    constructor(private http: HttpClient) { }

    getAll(query?: BranchQueryDto): Observable<BranchDto[]> {
        let params = new HttpParams();
        if (query?.search) params = params.set('search', query.search);
        if (query?.isActive !== undefined) params = params.set('isActive', query.isActive.toString());
        if (query?.branchType !== undefined) params = params.set('type', query.branchType.toString());

        return this.http.get<ApiResponse<any>>(this.apiUrl, { params })
            .pipe(map(r => Array.isArray(r.data) ? r.data : []));
    }

    getById(id: number): Observable<BranchDto> {
        return this.http.get<ApiResponse<BranchDto>>(`${this.apiUrl}/${id}`)
            .pipe(map(r => r.data));
    }

    getByCode(code: string): Observable<BranchDto> {
        return this.http.get<ApiResponse<BranchDto>>(`${this.apiUrl}/code/${code}`)
            .pipe(map(r => r.data));
    }

    getActive(): Observable<BranchDto[]> {
        return this.http.get<ApiResponse<any>>(`${this.apiUrl}/active`)
            .pipe(map(r => Array.isArray(r.data) ? r.data : []));
    }

    create(dto: CreateBranchDto): Observable<BranchDto> {
        return this.http.post<ApiResponse<BranchDto>>(this.apiUrl, dto)
            .pipe(map(r => r.data));
    }

    update(dto: UpdateBranchDto): Observable<any> {
        return this.http.put<ApiResponse<any>>(`${this.apiUrl}/${dto.id}`, dto)
            .pipe(map(r => r.data));
    }

    delete(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`)
            .pipe(map(r => r.data));
    }

}
