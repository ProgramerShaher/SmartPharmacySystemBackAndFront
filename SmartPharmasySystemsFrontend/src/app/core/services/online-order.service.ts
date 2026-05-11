import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface OnlineOrder {
  id: number;
  orderNumber: string;
  orderDate: string;
  customerName: string;
  customerPhone: string;
  totalAmount: number;
  status: string;
  statusCode: number;
  deliveryAddress: string;
  customerNotes?: string;
  orderItems: any[];
}

@Injectable({
  providedIn: 'root'
})
export class OnlineOrderService {
  private apiUrl = `${environment.apiUrl}/online-orders`;

  constructor(private http: HttpClient) { }

  getAllOrders(status?: number): Observable<any> {
    const url = status ? `${this.apiUrl}?status=${status}` : this.apiUrl;
    return this.http.get<any>(url);
  }

  getOrderById(id: number): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  updateOrderStatus(id: number, status: number): Observable<any> {
    return this.http.patch<any>(`${this.apiUrl}/${id}/status`, { status });
  }
}
