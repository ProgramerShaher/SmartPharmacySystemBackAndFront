import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiResponse } from '../../../core/models';
import { environment } from '../../../../environments/environment';

export interface DamagedGoodsRecordDto {
    id: number;
    damageCode: string;
    sourceWarehouseId: number;
    warehouseName: string;
    branchName: string;
    medicineId: number;
    medicineName: string;
    batchNumber: string;
    expiryDate: string;
    quantity: number;
    damageValue: number;
    damageType: string | number; // enum DamageType
    damageTypeName: string;
    disposalMethod: string | number;
    disposalMethodName: string;
    status: string | number; // enum RecordStatus
    statusName: string;
    statusColor: string;
    reason?: string;
    recordedByUserId: number;
    recordedByName: string;
    recordedAt: string;
    approvedByUserId?: number;
    approvedByName?: string;
    approvedAt?: string;
}

export interface CreateDamagedGoodsRecordDto {
    sourceWarehouseId: number;
    medicineId: number;
    batchNumber: string;
    expiryDate: string;
    quantity: number;
    damageType: number;
    reason?: string;
}

@Injectable({ providedIn: 'root' })
export class DamagedGoodsService {
    private readonly apiUrl = `${environment.apiUrl}/DamagedGoods`;

    constructor(private http: HttpClient) { }

    getAll(paramsObj?: {
        warehouseId?: number;
        medicineId?: number;
        damageType?: string | number;
        status?: string | number;
        from?: string;
        to?: string;
    }): Observable<DamagedGoodsRecordDto[]> {
        let params = new HttpParams();
        if (paramsObj?.warehouseId) params = params.set('warehouseId', paramsObj.warehouseId.toString());
        if (paramsObj?.medicineId) params = params.set('medicineId', paramsObj.medicineId.toString());
        if (paramsObj?.damageType !== undefined) params = params.set('damageType', paramsObj.damageType.toString());
        if (paramsObj?.status !== undefined) params = params.set('status', paramsObj.status.toString());
        if (paramsObj?.from) params = params.set('from', paramsObj.from);
        if (paramsObj?.to) params = params.set('to', paramsObj.to);

        return this.http.get<ApiResponse<DamagedGoodsRecordDto[]>>(this.apiUrl, { params })
            .pipe(map(r => r.data || []));
    }

    getById(id: number): Observable<DamagedGoodsRecordDto> {
        return this.http.get<ApiResponse<DamagedGoodsRecordDto>>(`${this.apiUrl}/${id}`)
            .pipe(map(r => r.data));
    }

    getByCode(code: string): Observable<DamagedGoodsRecordDto> {
        return this.http.get<ApiResponse<DamagedGoodsRecordDto>>(`${this.apiUrl}/code/${code}`)
            .pipe(map(r => r.data));
    }

    getPending(): Observable<DamagedGoodsRecordDto[]> {
        return this.http.get<ApiResponse<DamagedGoodsRecordDto[]>>(`${this.apiUrl}/pending`)
            .pipe(map(r => r.data || []));
    }

    getApproved(from?: string, to?: string): Observable<DamagedGoodsRecordDto[]> {
        let params = new HttpParams();
        if (from) params = params.set('from', from);
        if (to) params = params.set('to', to);
        return this.http.get<ApiResponse<DamagedGoodsRecordDto[]>>(`${this.apiUrl}/approved`, { params })
            .pipe(map(r => r.data || []));
    }

    create(dto: CreateDamagedGoodsRecordDto): Observable<DamagedGoodsRecordDto> {
        return this.http.post<ApiResponse<DamagedGoodsRecordDto>>(this.apiUrl, dto)
            .pipe(map(r => r.data));
    }

    update(dto: DamagedGoodsRecordDto): Observable<any> {
        return this.http.put<ApiResponse<any>>(this.apiUrl, dto)
            .pipe(map(r => r.data));
    }

    approve(id: number, approvedByUserId: number): Observable<DamagedGoodsRecordDto> {
        return this.http.put<ApiResponse<DamagedGoodsRecordDto>>(`${this.apiUrl}/${id}/approve?approvedByUserId=${approvedByUserId}`, {})
            .pipe(map(r => r.data));
    }

    delete(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`)
            .pipe(map(r => r.data));
    }
}
