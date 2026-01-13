import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { ApiResponse, PagedResult } from '../../../core/models';
import {
  MedicineBatchResponseDto,
  CreateMedicineBatchDto,
  UpdateMedicineBatchDto
} from '../../../core/models/medicine-batch.interface';

@Injectable({
  providedIn: 'root'
})
export class MedicineBatchService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/MedicineBatches`;

  /**
   * Get all batches with filtering
   */
  getAll(search?: string, medicineId?: number, status?: string): Observable<MedicineBatchResponseDto[]> {
    let params = new HttpParams();
    if (search) params = params.set('search', search);
    if (medicineId) params = params.set('medicineId', medicineId);
    if (status) params = params.set('status', status);

    return this.http.get<ApiResponse<MedicineBatchResponseDto[]>>(this.apiUrl, { params }).pipe(
      map(response => response.data || [])
    );
  }

  /**
   * Get batch by ID
   */
  getById(id: number): Observable<MedicineBatchResponseDto> {
    return this.http.get<ApiResponse<MedicineBatchResponseDto>>(`${this.apiUrl}/${id}`).pipe(
      map(response => response.data!)
    );
  }

  /**
   * Get batches for a specific medicine
   */
  getByMedicineId(medicineId: number): Observable<MedicineBatchResponseDto[]> {
    return this.getAll(undefined, medicineId);
  }

  /**
   * Get AVAILABLE batches for a specific medicine (FEFO order)
   * Calls /api/MedicineBatches/medicine/{id}/available
   */
  getAvailableByMedicineId(medicineId: number): Observable<MedicineBatchResponseDto[]> {
    return this.http.get<ApiResponse<MedicineBatchResponseDto[]>>(`${this.apiUrl}/medicine/${medicineId}/available`).pipe(
      map(response => response.data || [])
    );
  }

  /**
   * Create new batch
   */
  create(dto: CreateMedicineBatchDto): Observable<MedicineBatchResponseDto> {
    return this.http.post<ApiResponse<MedicineBatchResponseDto>>(this.apiUrl, dto).pipe(
      map(response => response.data!)
    );
  }

  /**
   * Update batch
   */
  update(id: number, dto: UpdateMedicineBatchDto): Observable<MedicineBatchResponseDto> {
    return this.http.put<ApiResponse<MedicineBatchResponseDto>>(`${this.apiUrl}/${id}`, dto).pipe(
      map(response => response.data!)
    );
  }

  /**
   * Delete batch
   */
  delete(id: number): Observable<void> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`).pipe(
      map(() => undefined)
    );
  }
}
