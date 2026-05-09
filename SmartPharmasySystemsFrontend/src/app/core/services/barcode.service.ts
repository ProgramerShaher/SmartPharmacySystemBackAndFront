import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.interface';
import { BarcodeQuery, BarcodeResult } from '../models/barcode.interface';

@Injectable({
  providedIn: 'root'
})
export class BarcodeService {
  private apiUrl = `${environment.apiUrl}/Barcode`;

  constructor(private http: HttpClient) { }

  /**
   * Processes a barcode for a specific transaction
   * @param query The barcode and transaction type
   * @returns Observable of the barcode result
   */
  processBarcode(query: BarcodeQuery): Observable<ApiResponse<BarcodeResult>> {
    return this.http.post<ApiResponse<BarcodeResult>>(`${this.apiUrl}/process`, query);
  }
}
