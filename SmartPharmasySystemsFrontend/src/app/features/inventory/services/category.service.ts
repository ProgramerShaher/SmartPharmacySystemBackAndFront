import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import {
    ApiResponse,
    PagedResult,
    CategoryDto,
    CategoryQueryDto,
    CreateCategoryDto,
    UpdateCategoryDto
} from '../../../core/models';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class CategoryService {
    private readonly apiUrl = `${environment.apiUrl}/Categories`;

    constructor(private http: HttpClient) { }

    /**
     * Search categories
     * GET /api/Categories
     */
    search(query?: CategoryQueryDto): Observable<PagedResult<CategoryDto>> {
        let params = new HttpParams();
        if (query?.search) params = params.set('search', query.search);
        if (query?.page) params = params.set('page', query.page.toString());
        if (query?.pageSize) params = params.set('pageSize', query.pageSize.toString());

        return this.http.get<ApiResponse<PagedResult<CategoryDto>>>(this.apiUrl, { params })
            .pipe(
                map(response => response.data || { items: [], totalCount: 0, pageNumber: 1, pageSize: 10, totalPages: 0 })
            );
    }

    /**
     * Get category by ID
     */
    getById(id: number): Observable<CategoryDto> {
        return this.http.get<ApiResponse<CategoryDto>>(`${this.apiUrl}/${id}`)
            .pipe(map(response => response.data));
    }

    /**
     * Create category
     */
    create(dto: CreateCategoryDto): Observable<CategoryDto> {
        return this.http.post<ApiResponse<CategoryDto>>(this.apiUrl, dto)
            .pipe(map(response => response.data));
    }

    /**
     * Update category
     */
    update(id: number, dto: UpdateCategoryDto): Observable<CategoryDto> {
        return this.http.put<ApiResponse<CategoryDto>>(`${this.apiUrl}/${id}`, dto)
            .pipe(map(response => response.data));
    }

    /**
     * Delete category
     */
    delete(id: number): Observable<any> {
        return this.http.delete<ApiResponse<any>>(`${this.apiUrl}/${id}`)
            .pipe(map(response => response.data));
    }

    /**
     * Get all categories for dropdowns (helper)
     * Fetches a large page size to get all
     */
    getAllForDropdown(): Observable<CategoryDto[]> {
        return this.search({ pageSize: 1000 }).pipe(
            map(result => result.items)
        );
    }

    /**
     * Alias for getAllForDropdown to support legacy calls
     */
    getAll(): Observable<CategoryDto[]> {
        return this.getAllForDropdown();
    }
}
