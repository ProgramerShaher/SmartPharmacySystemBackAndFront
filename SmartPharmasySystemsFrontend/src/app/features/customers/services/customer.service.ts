import { Injectable, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { map, tap } from 'rxjs/operators';
import { environment } from '../../../../environments/environment';
import { ApiResponse, PagedResult } from '../../../core/models';
import {
    Customer,
    CustomerReceipt,
    CreateCustomerReceiptDto,
    CustomerTransaction,
    CustomerStatement,
    CustomerQueryDto
} from '../../../core/models/customer.models';

@Injectable({
    providedIn: 'root'
})
export class CustomerService {
    private readonly apiUrl = `${environment.apiUrl}/Customers`;
    private readonly receiptsUrl = `${environment.apiUrl}/CustomerReceipts`;

    // Reactive state for customers with debt
    private customersWithDebtSubject = new BehaviorSubject<Customer[]>([]);
    public customersWithDebt$ = this.customersWithDebtSubject.asObservable();

    // Signal for total debt
    public totalCustomerDebt = signal(0);

    constructor(private http: HttpClient) {
        this.refreshCustomersWithDebt();
    }

    /**
     * Get all customers
     */
    getAll(query?: CustomerQueryDto): Observable<PagedResult<Customer>> {
        let params = new HttpParams();

        if (query?.search) params = params.set('search', query.search);
        if (query?.hasDebt !== undefined) params = params.set('hasDebt', query.hasDebt.toString());
        if (query?.page) params = params.set('page', query.page.toString());
        if (query?.pageSize) params = params.set('pageSize', query.pageSize.toString());

        return this.http.get<ApiResponse<PagedResult<Customer>>>(`${this.apiUrl}`, { params })
            .pipe(map(res => res.data));
    }

    /**
     * Get customer by ID
     */
    getById(id: number): Observable<Customer> {
        return this.http.get<ApiResponse<Customer>>(`${this.apiUrl}/${id}`)
            .pipe(map(res => res.data));
    }

    /**
     * Create new customer
     */
    create(customer: Partial<Customer>): Observable<Customer> {
        return this.http.post<ApiResponse<Customer>>(`${this.apiUrl}`, customer)
            .pipe(map(res => res.data));
    }

    /**
     * Update customer
     */
    update(id: number, customer: Partial<Customer>): Observable<Customer> {
        return this.http.put<ApiResponse<Customer>>(`${this.apiUrl}/${id}`, customer)
            .pipe(map(res => res.data));
    }

    /**
     * Delete customer
     */
    delete(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${this.apiUrl}/${id}`)
            .pipe(map(res => res.data));
    }

    /**
     * Get customer statement (transactions history)
     */
    getStatement(customerId: number, dateFrom?: string, dateTo?: string): Observable<CustomerStatement> {
        let params = new HttpParams();
        if (dateFrom) params = params.set('dateFrom', dateFrom);
        if (dateTo) params = params.set('dateTo', dateTo);

        return this.http.get<ApiResponse<CustomerStatement>>(`${this.apiUrl}/${customerId}/statement`, { params })
            .pipe(map(res => res.data));
    }

    /**
     * Get customer transactions
     */
    getTransactions(customerId: number): Observable<CustomerTransaction[]> {
        return this.http.get<ApiResponse<CustomerTransaction[]>>(`${this.apiUrl}/${customerId}/transactions`)
            .pipe(map(res => res.data));
    }

    /**
     * Get all receipts
     */
    getAllReceipts(customerId?: number): Observable<CustomerReceipt[]> {
        let params = new HttpParams();
        if (customerId) params = params.set('customerId', customerId.toString());

        return this.http.get<ApiResponse<CustomerReceipt[]>>(`${this.receiptsUrl}`, { params })
            .pipe(map(res => res.data));
    }

    /**
     * Get receipt by ID
     */
    getReceiptById(id: number): Observable<CustomerReceipt> {
        return this.http.get<ApiResponse<CustomerReceipt>>(`${this.receiptsUrl}/${id}`)
            .pipe(map(res => res.data));
    }

    /**
     * Create customer receipt (payment)
     */
    createReceipt(dto: CreateCustomerReceiptDto): Observable<CustomerReceipt> {
        return this.http.post<ApiResponse<CustomerReceipt>>(`${this.receiptsUrl}/customer`, dto)
            .pipe(
                map(res => res.data),
                tap(() => {
                    // Refresh customers with debt after payment
                    this.refreshCustomersWithDebt();
                })
            );
    }

    /**
     * Delete receipt
     */
    deleteReceipt(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${this.receiptsUrl}/${id}`)
            .pipe(
                map(res => res.data),
                tap(() => this.refreshCustomersWithDebt())
            );
    }

    /**
     * Refresh customers with debt
     */
    refreshCustomersWithDebt() {
        this.getAll({ hasDebt: true, pageSize: 100 }).subscribe({
            next: (result) => {
                this.customersWithDebtSubject.next(result.items);
                const totalDebt = result.items.reduce((sum, c) => sum + c.balance, 0);
                this.totalCustomerDebt.set(totalDebt);
            }
        });
    }

    /**
     * Validate receipt amount
     */
    validateReceiptAmount(customerId: number, amount: number): Observable<{
        isValid: boolean;
        message?: string;
        customerBalance?: number;
    }> {
        return this.getById(customerId).pipe(
            map(customer => {
                if (amount <= 0) {
                    return {
                        isValid: false,
                        message: 'المبلغ يجب أن يكون أكبر من صفر',
                        customerBalance: customer.balance
                    };
                }

                if (amount > customer.balance) {
                    return {
                        isValid: false,
                        message: `المبلغ أكبر من المديونية الحالية (${customer.balance} ر.ي)`,
                        customerBalance: customer.balance
                    };
                }

                return {
                    isValid: true,
                    customerBalance: customer.balance
                };
            })
        );
    }
}
