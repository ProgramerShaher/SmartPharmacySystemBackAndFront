import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map, catchError } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { ApiResponse, PagedResult } from '../../../core/models';
import { 
  AutomatedAuditHeaderDto, 
  GenerateAuditRequestDto, 
  AuditChartDataDto 
} from '../../../core/models/automated-audit.interface';

@Injectable({
  providedIn: 'root'
})
export class AutomatedAuditService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${environment.apiUrl}/AutomatedAudits`;

  getAll(page = 1, pageSize = 10, warehouseId?: number): Observable<PagedResult<AutomatedAuditHeaderDto>> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (warehouseId) {
      params = params.set('warehouseId', warehouseId.toString());
    }

    return this.http.get<ApiResponse<PagedResult<AutomatedAuditHeaderDto>>>(this.apiUrl, { params }).pipe(
      map(response => response.data!)
    );
  }

  getById(id: number): Observable<AutomatedAuditHeaderDto> {
    return this.http.get<ApiResponse<AutomatedAuditHeaderDto>>(`${this.apiUrl}/${id}`).pipe(
      map(response => response.data!)
    );
  }

  generateAudit(request: GenerateAuditRequestDto): Observable<AutomatedAuditHeaderDto> {
    return this.http.post<ApiResponse<AutomatedAuditHeaderDto>>(`${this.apiUrl}/generate`, request).pipe(
      map(response => response.data!)
    );
  }

  getCharts(id: number): Observable<AuditChartDataDto> {
    return this.http.get<ApiResponse<AuditChartDataDto>>(`${this.apiUrl}/${id}/charts`).pipe(
      map(response => response.data!)
    );
  }

  delete(id: number): Observable<void> {
    return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`).pipe(
      map(() => undefined)
    );
  }
}
