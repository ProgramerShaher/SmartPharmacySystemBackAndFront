import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';

export interface DamagedGoodsRecord {
  id: number;
  damageCode: string;
  sourceWarehouseId: number;
  sourceWarehouseName?: string;
  medicineId: number;
  medicineName?: string;
  batchNumber: string;
  expiryDate: string;
  quantity: number;
  damageType: number;
  damageTypeName?: string;
  damageValue: number;
  disposalMethod: number;
  status: number;
  statusName?: string;
  approvedByUserId?: number;
  approvedByUserName?: string;
  approvedAt?: string;
  recordedByUserId: number;
  recordedByUserName?: string;
  createdAt: string;
}

export interface CreateDamagedGoodsDto {
  sourceWarehouseId: number;
  medicineId: number;
  batchNumber: string;
  expiryDate: string;
  quantity: number;
  damageType: number;
  disposalMethod: number;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class DamagedGoodsService {
  private apiUrl = `${environment.apiUrl}/DamagedGoods`;

  constructor(private http: HttpClient) {}

  getAll(filters?: {
    warehouseId?: number;
    medicineId?: number;
    damageType?: number;
    status?: number;
    from?: string;
    to?: string;
  }): Observable<DamagedGoodsRecord[]> {
    let params = new HttpParams();
    if (filters?.warehouseId) params = params.set('warehouseId', filters.warehouseId);
    if (filters?.medicineId) params = params.set('medicineId', filters.medicineId);
    if (filters?.damageType !== undefined) params = params.set('damageType', filters.damageType);
    if (filters?.status !== undefined) params = params.set('status', filters.status);
    if (filters?.from) params = params.set('from', filters.from);
    if (filters?.to) params = params.set('to', filters.to);

    return this.http.get<ApiResponse<DamagedGoodsRecord[]>>(this.apiUrl, { params })
      .pipe(map(r => r.data));
  }

  getPending(): Observable<DamagedGoodsRecord[]> {
    return this.http.get<ApiResponse<DamagedGoodsRecord[]>>(`${this.apiUrl}/pending`)
      .pipe(map(r => r.data));
  }

  getApproved(from?: string, to?: string): Observable<DamagedGoodsRecord[]> {
    let params = new HttpParams();
    if (from) params = params.set('from', from);
    if (to) params = params.set('to', to);
    return this.http.get<ApiResponse<DamagedGoodsRecord[]>>(`${this.apiUrl}/approved`, { params })
      .pipe(map(r => r.data));
  }

  getById(id: number): Observable<DamagedGoodsRecord> {
    return this.http.get<ApiResponse<DamagedGoodsRecord>>(`${this.apiUrl}/${id}`)
      .pipe(map(r => r.data));
  }

  create(dto: CreateDamagedGoodsDto): Observable<DamagedGoodsRecord> {
    return this.http.post<ApiResponse<DamagedGoodsRecord>>(this.apiUrl, dto)
      .pipe(map(r => r.data));
  }

  approve(id: number, approvedByUserId: number): Observable<DamagedGoodsRecord> {
    return this.http.put<ApiResponse<DamagedGoodsRecord>>(
      `${this.apiUrl}/${id}/approve?approvedByUserId=${approvedByUserId}`, {}
    ).pipe(map(r => r.data));
  }

  delete(id: number): Observable<void> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`).pipe(map(() => void 0));
  }
}
