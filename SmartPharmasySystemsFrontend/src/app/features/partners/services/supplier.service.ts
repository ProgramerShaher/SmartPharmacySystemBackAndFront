// src/app/features/partners/services/supplier.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { ApiResponse, PagedResult, Supplier, SupplierQueryDto } from '../../../core/models';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class SupplierService {
    constructor(private http: HttpClient) { }

    private logSuccess(operation: string, details?: string): void {
        console.log(`✅ ${operation} نجاح`, {
            timestamp: new Date().toISOString(),
            details
        });
    }

    private logError(operation: string, error: any): void {
        console.error(`❌ ${operation} فشل:`, {
            timestamp: new Date().toISOString(),
            error: error.message || error,
            status: error.status,
            url: error.url,
            fullError: error
        });
    }

    // --- Suppliers ---
    getAllSuppliers(query?: SupplierQueryDto): Observable<PagedResult<Supplier>> {
        console.log('🔍 Searching suppliers with query:', query);

        // إنشاء params بطريقة صحيحة
        const params: any = {};
        if (query?.search && query.search.trim() !== '') {
            params.search = query.search.trim();
        }
        if (query?.page) {
            params.page = query.page.toString();
        }
        if (query?.pageSize) {
            params.pageSize = query.pageSize.toString();
        }

        console.log('📤 Sending request to:', `${environment.apiUrl}/Suppliers`);
        console.log('📤 With params:', params);

        return this.http.get<ApiResponse<PagedResult<Supplier>>>(
            `${environment.apiUrl}/Suppliers`,
            { params }
        ).pipe(
            map(res => {
                console.log('✅ Raw API Response:', res);
                console.log('✅ Data from response:', res.data);
                console.log('✅ Items count:', res.data?.items?.length || 0);
                return res.data;
            }),
            catchError(error => {
                console.error('❌ Error in getAllSuppliers:', error);
                console.error('❌ Error status:', error.status);
                console.error('❌ Error message:', error.message);
                console.error('❌ Error details:', error.error);
                throw error;
            })
        );
    }

    // Aliases for backward compatibility
    GetAllSuppliers(query: SupplierQueryDto): Observable<PagedResult<Supplier>> {
        return this.getAllSuppliers(query);
    }

    search(query: SupplierQueryDto): Observable<PagedResult<Supplier>> {
        return this.getAllSuppliers(query);
    }

    getSupplierById(id: number): Observable<Supplier> {
        console.log(`📥 جلب بيانات المورد ${id}`);
        return this.http.get<ApiResponse<Supplier>>(
            `${environment.apiUrl}/Suppliers/${id}`
        ).pipe(map(res => res.data));
    }

    getById(id: number): Observable<Supplier> {
        return this.getSupplierById(id);
    }

    createSupplier(supplier: Partial<Supplier>): Observable<Supplier> {
        console.log('➕ إضافة مورد جديد:', supplier);

        // إعداد البيانات للإرسال - استبعاد الحقول غير المطلوبة
        const supplierToCreate = {
            name: supplier.name,
            contactPerson: supplier.contactPerson || '',
            phoneNumber: supplier.phoneNumber,
            address: supplier.address,
            email: supplier.email || '',
            notes: supplier.notes || '',
            balance: supplier.balance || 0
        };

        return this.http.post<ApiResponse<Supplier>>(
            `${environment.apiUrl}/Suppliers`,
            supplierToCreate
        ).pipe(map(res => res.data));
    }

    updateSupplier(id: number, supplier: Partial<Supplier>): Observable<Supplier> {
        console.log(`✏️ تحديث بيانات المورد ${id}:`, supplier);

        // إعداد البيانات للتحديث - يجب تضمين ID في الـ body
        const supplierToUpdate = {
            id: id, // إضافة ID في الـ body
            name: supplier.name,
            contactPerson: supplier.contactPerson || '',
            phoneNumber: supplier.phoneNumber,
            address: supplier.address,
            email: supplier.email || '',
            notes: supplier.notes || '',
            balance: supplier.balance || 0
        };

        console.log('📤 Sending update request with data:', supplierToUpdate);

        return this.http.put<ApiResponse<Supplier>>(
            `${environment.apiUrl}/Suppliers/${id}`,
            supplierToUpdate
        ).pipe(
            map(res => {
                console.log('✅ Update successful:', res);
                return res.data;
            }),
            catchError(error => {
                console.error('❌ Update failed:', error);
                throw error;
            })
        );
    }

    deleteSupplier(id: number): Observable<void> {
        console.log(`🗑️ حذف المورد ${id}`);
        return this.http.delete<ApiResponse<void>>(
            `${environment.apiUrl}/Suppliers/${id}`
        ).pipe(map(res => res.data));
    }

    // Compatibility aliases
    create(supplier: Partial<Supplier>): Observable<Supplier> {
        return this.createSupplier(supplier);
    }

    update(id: number, supplier: Partial<Supplier>): Observable<Supplier> {
        return this.updateSupplier(id, supplier);
    }

    delete(id: number): Observable<void> {
        return this.deleteSupplier(id);
    }
}
