import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models';
import { ProductSerialNumberDto, RegisterSerialNumbersDto, WarrantyCheckResultDto } from '../models/product-serial-number.interface';

@Injectable({ providedIn: 'root' })
export class ProductSerialNumberService {
    private readonly apiUrl = `${environment.apiUrl}/ProductSerialNumbers`;

    constructor(private http: HttpClient) { }

    getInStockByMedicineId(medicineId: number): Observable<ProductSerialNumberDto[]> {
        return this.http.get<ApiResponse<ProductSerialNumberDto[]>>(`${this.apiUrl}/by-medicine/${medicineId}`).pipe(
            map(res => res.data || [])
        );
    }

    checkWarranty(serialNumber: string): Observable<WarrantyCheckResultDto> {
        return this.http.get<ApiResponse<WarrantyCheckResultDto>>(`${this.apiUrl}/warranty-check/${encodeURIComponent(serialNumber)}`).pipe(
            map(res => res.data)
        );
    }

    registerSerialNumbers(dto: RegisterSerialNumbersDto): Observable<ProductSerialNumberDto[]> {
        return this.http.post<ApiResponse<ProductSerialNumberDto[]>>(`${this.apiUrl}/register`, dto).pipe(
            map(res => res.data || [])
        );
    }
}
