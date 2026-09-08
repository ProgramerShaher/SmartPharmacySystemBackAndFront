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
import { MedicineBatchResponseDto } from '../../../core/models/medicine-batch.interface';

import { of } from 'rxjs';
import { tap } from 'rxjs/operators';

@Injectable({
    providedIn: 'root'
})
export class MedicineService {
    private readonly http = inject(HttpClient);
    private readonly apiUrl = `${environment.apiUrl}/Medicines`;

    // Local In-Memory Fast Lookup Index
    private lookupCache: Medicine[] | null = null;
    private barcodeMap = new Map<string, Medicine>();
    private isPreloading = false;

    /**
     * Preload or fetch compact medicine lookup dataset
     */
    loadLookupIndex(forceRefresh = false): Observable<Medicine[]> {
        if (!forceRefresh && this.lookupCache) {
            return of(this.lookupCache);
        }

        return this.http.get<ApiResponse<Medicine[]>>(`${this.apiUrl}/lookup`).pipe(
            map(res => res.data || []),
            tap(items => {
                this.lookupCache = items;
                this.barcodeMap.clear();
                items.forEach(med => {
                    const code = med.defaultBarcode || (med as any).barcode;
                    if (code) {
                        this.barcodeMap.set(code.trim().toLowerCase(), med);
                    }
                    if (med.internalCode) {
                        this.barcodeMap.set(med.internalCode.trim().toLowerCase(), med);
                    }
                    if (med.medicineUnits && med.medicineUnits.length > 0) {
                        med.medicineUnits.forEach(unit => {
                            if (unit.barcode) {
                                this.barcodeMap.set(unit.barcode.trim().toLowerCase(), med);
                            }
                        });
                    }
                });
            })
        );
    }

    /**
     * Fast O(1) local barcode lookup
     */
    getByBarcodeLocal(barcode: string): Medicine | undefined {
        if (!barcode) return undefined;
        return this.barcodeMap.get(barcode.trim().toLowerCase());
    }

    /**
     * Ultra-fast local search with intelligent ranking (< 5 ms)
     */
    searchLocal(query: string, maxResults = 20): Medicine[] {
        if (!this.lookupCache) return [];

        const q = (query || '').trim().toLowerCase();
        if (!q) {
            return this.lookupCache.slice(0, maxResults);
        }

        // 1. Exact Barcode / Code Match
        const exactBarcodeMatch = this.getByBarcodeLocal(q);

        const exactNameMatches: Medicine[] = [];
        const startsWithMatches: Medicine[] = [];
        const containsMatches: Medicine[] = [];

        for (const item of this.lookupCache) {
            if (exactBarcodeMatch && item.id === exactBarcodeMatch.id) continue;

            const name = (item.name || '').toLowerCase();
            const sciName = (item.scientificName || '').toLowerCase();

            if (name === q) {
                exactNameMatches.push(item);
            } else if (name.startsWith(q)) {
                startsWithMatches.push(item);
            } else if (name.includes(q) || sciName.includes(q)) {
                containsMatches.push(item);
            }
        }

        const results: Medicine[] = [];
        if (exactBarcodeMatch) results.push(exactBarcodeMatch);
        results.push(...exactNameMatches, ...startsWithMatches, ...containsMatches);

        return results.slice(0, maxResults);
    }

    /**
     * Invalidate local lookup cache after mutations
     */
    clearLookupCache(): void {
        this.lookupCache = null;
        this.barcodeMap.clear();
    }


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
     * Get available batches for FEFO (First Expire First Out)
     */
    getFefoBatches(id: number): Observable<MedicineBatchResponseDto[]> {
        return this.http.get<ApiResponse<MedicineBatchResponseDto[]>>(`${this.apiUrl}/${id}/fefo-batches`).pipe(
            map(response => response.data || [])
        );
    }

  /**
   * Create new medicine
   */
    create(dto: CreateMedicineDto): Observable<Medicine> {
        return this.http.post<ApiResponse<Medicine>>(this.apiUrl, dto).pipe(
            map(response => response.data!),
            tap(() => this.clearLookupCache())
        );
    }

  /**
   * Update existing medicine
   */
    update(id: number, dto: UpdateMedicineDto): Observable<Medicine> {
        return this.http.put<ApiResponse<Medicine>>(`${this.apiUrl}/${id}`, dto).pipe(
            map(response => response.data!),
            tap(() => this.clearLookupCache())
        );
    }

  /**
   * Delete medicine
   */
    delete(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`).pipe(
            map(() => undefined),
            tap(() => this.clearLookupCache())
        );
    }

    /**
     * Delete multiple medicines
     */
    deleteBulk(ids: number[]): Observable<ApiResponse<any>> {
        return this.http.request<ApiResponse<any>>('delete', `${this.apiUrl}/bulk`, { body: ids }).pipe(
            tap(() => this.clearLookupCache())
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

    /**
     * Download Excel template for importing medicines
     */
    downloadTemplate(): void {
        const url = `${this.apiUrl}/export-template`;
        window.open(url, '_blank');
    }

    /**
     * Import medicines from Excel file
     */
    importMedicines(file: File): Observable<any> {
        const formData = new FormData();
        formData.append('file', file);
        return this.http.post<ApiResponse<any>>(`${this.apiUrl}/import`, formData).pipe(
            map(response => response.data)
        );
    }
}
