import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { ApiResponse, PagedResult } from '../../../core/models';
import { 
    Medicine, 
    CreateMedicineDto,
    UpdateMedicineDto, 
    MedicineQueryDto,
    MedicineDetailsDto
} from '../../../core/models/medicine.interface';

@Injectable({
    providedIn: 'root'
})
export class MedicineService {
    private readonly http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}/Medicines`;

  /**
   * Get all medicines with advanced filtering
   */
    getAll(query?: MedicineQueryDto): Observable<PagedResult<Medicine>> {
        let params = new HttpParams();

      if (query) {
          if (query.search) params = params.set('search', query.search);
          if (query.categoryId) params = params.set('categoryId', query.categoryId);
          if (query.manufacturer) params = params.set('manufacturer', query.manufacturer);
          if (query.status) params = params.set('status', query.status);
          if (query.page) params = params.set('page', query.page);
          if (query.pageSize) params = params.set('pageSize', query.pageSize);
          if (query.sortBy) params = params.set('sortBy', query.sortBy);
          if (query.sortDescending !== undefined) params = params.set('sortDescending', query.sortDescending);
      }

      return this.http.get<ApiResponse<PagedResult<Medicine>>>(this.apiUrl, { params }).pipe(
          map(response => response.data!)
      );
  }

  /**
   * Get medicine by ID
   */
    getById(id: number): Observable<Medicine> {
        return this.http.get<ApiResponse<Medicine>>(`${this.apiUrl}/${id}`).pipe(
            map(response => response.data!)
        );
    }

    /**
     * Get full medicine details with batches
     */
    getDetails(id: number): Observable<MedicineDetailsDto> {
        return this.http.get<ApiResponse<MedicineDetailsDto>>(`${this.apiUrl}/${id}/details`).pipe(
            map(response => response.data!)
        );
    }

  /**
   * Create new medicine
   */
    create(dto: CreateMedicineDto): Observable<Medicine> {
        return this.http.post<ApiResponse<Medicine>>(this.apiUrl, dto).pipe(
            map(response => response.data!)
        );
    }

  /**
   * Update existing medicine
   */
    update(id: number, dto: UpdateMedicineDto): Observable<Medicine> {
        return this.http.put<ApiResponse<Medicine>>(`${this.apiUrl}/${id}`, dto).pipe(
            map(response => response.data!)
        );
    }

  /**
   * Delete medicine
   */
    delete(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`).pipe(
            map(() => undefined)
        );
    }

  /**
   * Get distinct manufacturers for dropdown
   */
    getManufacturers(): Observable<string[]> {
        return this.http.get<ApiResponse<string[]>>(`${this.apiUrl}/manufacturers`).pipe(
            map(response => response.data || [])
        );
    }
}
