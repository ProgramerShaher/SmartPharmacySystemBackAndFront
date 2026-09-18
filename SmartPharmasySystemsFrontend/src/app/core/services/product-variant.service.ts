import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models';
import { ProductVariantDto, CreateProductVariantDto, UpdateProductVariantDto } from '../models/product-variant.interface';

@Injectable({ providedIn: 'root' })
export class ProductVariantService {
    private readonly apiUrl = `${environment.apiUrl}/ProductVariants`;

    constructor(private http: HttpClient) { }

    getByMedicineId(medicineId: number): Observable<ProductVariantDto[]> {
        return this.http.get<ApiResponse<ProductVariantDto[]>>(`${this.apiUrl}/by-medicine/${medicineId}`).pipe(
            map(res => res.data || [])
        );
    }

    getById(id: number): Observable<ProductVariantDto> {
        return this.http.get<ApiResponse<ProductVariantDto>>(`${this.apiUrl}/${id}`).pipe(
            map(res => res.data)
        );
    }

    getByCode(code: string): Observable<ProductVariantDto> {
        return this.http.get<ApiResponse<ProductVariantDto>>(`${this.apiUrl}/by-code/${encodeURIComponent(code)}`).pipe(
            map(res => res.data)
        );
    }

    create(dto: CreateProductVariantDto): Observable<ProductVariantDto> {
        return this.http.post<ApiResponse<ProductVariantDto>>(this.apiUrl, dto).pipe(
            map(res => res.data)
        );
    }

    update(id: number, dto: UpdateProductVariantDto): Observable<ProductVariantDto> {
        return this.http.put<ApiResponse<ProductVariantDto>>(`${this.apiUrl}/${id}`, dto).pipe(
            map(res => res.data)
        );
    }

    delete(id: number): Observable<boolean> {
        return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`).pipe(
            map(res => res.data)
        );
    }
}
