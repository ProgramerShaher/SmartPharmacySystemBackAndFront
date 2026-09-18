import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { ApiResponse } from '../../../core/models/api-response.interface';
import {
  Quotation,
  CreateQuotationDto,
  QuotationStatus,
  ConvertQuotationToInvoiceDto,
  QuotationPrintData
} from '../models/quotation.interface';

@Injectable({
  providedIn: 'root'
})
export class QuotationService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/Quotations`;

  getPaged(query?: any): Observable<{ items: Quotation[]; totalCount: number; page: number; pageSize: number }> {
    let params = new HttpParams();
    if (query) {
      if (query.search) params = params.set('search', query.search);
      if (query.branchId) params = params.set('branchId', query.branchId.toString());
      if (query.customerId) params = params.set('customerId', query.customerId.toString());
      if (query.status !== undefined && query.status !== null && query.status !== '') params = params.set('status', query.status.toString());
      if (query.dateFrom) params = params.set('dateFrom', query.dateFrom);
      if (query.dateTo) params = params.set('dateTo', query.dateTo);
      if (query.page) params = params.set('page', query.page.toString());
      if (query.pageSize) params = params.set('pageSize', query.pageSize.toString());
    }

    return this.http.get<ApiResponse<any>>(this.baseUrl, { params }).pipe(
      map(response => {
        if (response.data && response.data.items) {
          return {
            items: response.data.items,
            totalCount: response.data.totalCount,
            page: response.data.page,
            pageSize: response.data.pageSize
          };
        }
        return {
          items: Array.isArray(response.data) ? response.data : [],
          totalCount: Array.isArray(response.data) ? response.data.length : 0,
          page: 1,
          pageSize: 20
        };
      })
    );
  }

  getById(id: number): Observable<Quotation> {
    return this.http.get<ApiResponse<Quotation>>(`${this.baseUrl}/${id}`).pipe(
      map(res => res.data)
    );
  }

  getPrintData(id: number): Observable<QuotationPrintData> {
    return this.http.get<ApiResponse<QuotationPrintData>>(`${this.baseUrl}/${id}/print`).pipe(
      map(res => res.data)
    );
  }

  create(dto: CreateQuotationDto): Observable<Quotation> {
    return this.http.post<ApiResponse<Quotation>>(this.baseUrl, dto).pipe(
      map(res => res.data)
    );
  }

  update(id: number, dto: Partial<CreateQuotationDto> & { status?: QuotationStatus }): Observable<Quotation> {
    return this.http.put<ApiResponse<Quotation>>(`${this.baseUrl}/${id}`, dto).pipe(
      map(res => res.data)
    );
  }

  delete(id: number): Observable<boolean> {
    return this.http.delete<ApiResponse<boolean>>(`${this.baseUrl}/${id}`).pipe(
      map(res => res.data)
    );
  }

  changeStatus(id: number, status: QuotationStatus): Observable<Quotation> {
    const params = new HttpParams().set('status', status.toString());
    return this.http.patch<ApiResponse<Quotation>>(`${this.baseUrl}/${id}/status`, null, { params }).pipe(
      map(res => res.data)
    );
  }

  convertToInvoice(id: number, dto: ConvertQuotationToInvoiceDto): Observable<any> {
    return this.http.post<ApiResponse<any>>(`${this.baseUrl}/${id}/convert-to-invoice`, dto).pipe(
      map(res => res.data)
    );
  }
}
