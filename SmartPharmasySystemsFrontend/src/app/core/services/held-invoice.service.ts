import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, map, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models';
import { HeldInvoiceDto, CreateHeldInvoiceDto } from '../models/held-invoice.models';

@Injectable({ providedIn: 'root' })
export class HeldInvoiceService {
    private readonly apiUrl = `${environment.apiUrl}/HeldInvoices`;

    private heldCountSubject = new BehaviorSubject<number>(0);
    readonly heldCount$ = this.heldCountSubject.asObservable();

    constructor(private http: HttpClient) { }

    getAll(): Observable<HeldInvoiceDto[]> {
        return this.http.get<ApiResponse<HeldInvoiceDto[]>>(this.apiUrl).pipe(
            map(res => res.data || []),
            tap(list => this.heldCountSubject.next(list.length))
        );
    }

    getById(id: number): Observable<HeldInvoiceDto> {
        return this.http.get<ApiResponse<HeldInvoiceDto>>(`${this.apiUrl}/${id}`).pipe(
            map(res => res.data)
        );
    }

    hold(dto: CreateHeldInvoiceDto): Observable<HeldInvoiceDto> {
        return this.http.post<ApiResponse<HeldInvoiceDto>>(this.apiUrl, dto).pipe(
            map(res => res.data),
            tap(() => this.refreshCount().subscribe())
        );
    }

    resumeAndDelete(id: number): Observable<boolean> {
        return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}`).pipe(
            map(res => res.data),
            tap(() => this.refreshCount().subscribe())
        );
    }

    refreshCount(): Observable<HeldInvoiceDto[]> {
        return this.getAll();
    }
}
