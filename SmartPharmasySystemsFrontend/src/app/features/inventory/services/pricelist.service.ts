import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';

export interface PricelistItem {
  id: number;
  pricelistId: number;
  medicineId: number;
  medicineName: string;
  fixedPrice?: number;
  discountPercentage?: number;
}

export interface Pricelist {
  id: number;
  name: string;
  description?: string;
  globalDiscountPercentage: number;
  isActive: boolean;
  createdAt: string;
  items: PricelistItem[];
  customersCount: number;
}

export interface CreatePricelistDto {
  name: string;
  description?: string;
  globalDiscountPercentage: number;
  isActive: boolean;
  items: { medicineId: number; fixedPrice?: number; discountPercentage?: number }[];
}

export interface PricelistSelectDto {
  id: number;
  name: string;
  globalDiscountPercentage: number;
}

export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class PricelistService {
  private apiUrl = `${environment.apiUrl}/Pricelist`;

  constructor(private http: HttpClient) {}

  getAll(isActive?: boolean): Observable<Pricelist[]> {
    let params = new HttpParams();
    if (isActive !== undefined) params = params.set('isActive', isActive);
    return this.http.get<ApiResponse<Pricelist[]>>(this.apiUrl, { params }).pipe(map(r => r.data));
  }

  getSelectList(): Observable<PricelistSelectDto[]> {
    return this.http.get<ApiResponse<PricelistSelectDto[]>>(`${this.apiUrl}/select`).pipe(map(r => r.data));
  }

  getById(id: number): Observable<Pricelist> {
    return this.http.get<ApiResponse<Pricelist>>(`${this.apiUrl}/${id}`).pipe(map(r => r.data));
  }

  create(dto: CreatePricelistDto): Observable<Pricelist> {
    return this.http.post<ApiResponse<Pricelist>>(this.apiUrl, dto).pipe(map(r => r.data));
  }

  update(id: number, dto: CreatePricelistDto & { id: number }): Observable<Pricelist> {
    return this.http.put<ApiResponse<Pricelist>>(`${this.apiUrl}/${id}`, dto).pipe(map(r => r.data));
  }

  delete(id: number): Observable<void> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`).pipe(map(() => void 0));
  }

  getDiscountForMedicine(pricelistId: number, medicineId: number): Observable<number> {
    return this.http.get<ApiResponse<number>>(`${this.apiUrl}/${pricelistId}/discount/${medicineId}`)
      .pipe(map(r => r.data));
  }
}
