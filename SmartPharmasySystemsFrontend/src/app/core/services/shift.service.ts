import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { map } from 'rxjs/operators';
import { ShiftDto, OpenShiftDto, CloseShiftDto, ShiftSummaryDto, ShiftDetailsDto, ApiResponse } from '../models';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ShiftService {
  private apiUrl = `${environment.apiUrl}/Shifts`;

  // Subject to trigger modal from anywhere
  private showModalSubject = new Subject<'open' | 'close'>();
  showModal$ = this.showModalSubject.asObservable();
  
  // Optional flag to set default behavior for shift closing modal
  public transferIntent: boolean | null = null;

  constructor(private http: HttpClient) { }

  requestShiftModal(mode: 'open' | 'close') {
    this.showModalSubject.next(mode);
  }

  getCurrentShift(): Observable<ApiResponse<ShiftDto>> {
    return this.http.get<ApiResponse<ShiftDto>>(`${this.apiUrl}/Current`);
  }

  getAllShifts(): Observable<ApiResponse<ShiftDto[]>> {
    return this.http.get<ApiResponse<ShiftDto[]>>(`${this.apiUrl}/All`);
  }

  openShift(data: OpenShiftDto): Observable<ApiResponse<ShiftDto>> {
    return this.http.post<ApiResponse<ShiftDto>>(`${this.apiUrl}/Open`, data);
  }

  closeShift(data: CloseShiftDto): Observable<ApiResponse<ShiftDto>> {
    return this.http.post<ApiResponse<ShiftDto>>(`${this.apiUrl}/Close`, data);
  }

  getSummary(id: number): Observable<ApiResponse<ShiftSummaryDto>> {
    return this.http.get<ApiResponse<ShiftSummaryDto>>(`${this.apiUrl}/${id}/Summary`);
  }

  getDetails(id: number): Observable<ApiResponse<ShiftDetailsDto>> {
    return this.http.get<ApiResponse<ShiftDetailsDto>>(`${this.apiUrl}/${id}/Details`);
  }

  getMyDrawerLedger(startDate?: string, endDate?: string): Observable<any> {
    let params: any = {};
    if (startDate) params.startDate = startDate;
    if (endDate) params.endDate = endDate;
    return this.http.get<ApiResponse<any>>(`${this.apiUrl}/my-drawer-ledger`, { params }).pipe(
        map(res => res.data)
    );
  }

  sweepUntransferredShifts(): Observable<ApiResponse<number>> {
    return this.http.post<ApiResponse<number>>(`${this.apiUrl}/SweepUntransferred`, {});
  }
}
