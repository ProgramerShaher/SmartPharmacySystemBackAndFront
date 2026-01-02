import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { ApiResponse, Category, InventoryMovement, Medicine, MedicineBatch, PagedResult, CategoryQueryDto, MedicineQueryDto, StockMovementQueryDto } from '../../../core/models';
import { environment } from '../../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class InventoryService {
    constructor(private http: HttpClient) { }

    // --- Category Service Spec ---
    getAllCategories(query?: CategoryQueryDto): Observable<PagedResult<Category>> {
        return this.http.get<ApiResponse<PagedResult<Category>>>(`${environment.apiUrl}/Categories`, { params: query as any }).pipe(map(res => res.data));
    }
    getCategoryById(id: number): Observable<Category> {
        return this.http.get<ApiResponse<Category>>(`${environment.apiUrl}/Categories/${id}`).pipe(map(res => res.data));
    }
    createCategory(category: Partial<Category>): Observable<Category> {
        return this.http.post<ApiResponse<Category>>(`${environment.apiUrl}/Categories`, category).pipe(map(res => res.data));
    }
    updateCategory(id: number, category: Partial<Category>): Observable<Category> {
        return this.http.put<ApiResponse<Category>>(`${environment.apiUrl}/Categories/${id}`, category).pipe(map(res => res.data));
    }
    deleteCategory(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${environment.apiUrl}/Categories/${id}`).pipe(map(res => res.data));
    }
    getCategoryStats(id: number): Observable<any> {
        return this.http.get<ApiResponse<any>>(`${environment.apiUrl}/Categories/${id}/stats`).pipe(map(res => res.data));
    }

    // --- Medicine Service Spec ---
    searchMedicines(query: MedicineQueryDto): Observable<PagedResult<Medicine>> {
        return this.http.get<ApiResponse<PagedResult<Medicine>>>(`${environment.apiUrl}/Medicines`, { params: query as any }).pipe(map(res => res.data));
    }
    // Aliases as requested
    getAllMedicines(query?: any): Observable<PagedResult<Medicine>> { return this.searchMedicines(query || {}); }
    getMedicineById(id: number): Observable<Medicine> {
        return this.http.get<ApiResponse<Medicine>>(`${environment.apiUrl}/Medicines/${id}`).pipe(map(res => res.data));
    }
    createMedicine(medicine: Partial<Medicine>): Observable<Medicine> {
        return this.http.post<ApiResponse<Medicine>>(`${environment.apiUrl}/Medicines`, medicine).pipe(map(res => res.data));
    }
    updateMedicine(id: number, medicine: Partial<Medicine>): Observable<Medicine> {
        return this.http.put<ApiResponse<Medicine>>(`${environment.apiUrl}/Medicines/${id}`, medicine).pipe(map(res => res.data));
    }
    deleteMedicine(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${environment.apiUrl}/Medicines/${id}`).pipe(map(res => res.data));
    }

    // --- MedicineBatch Service Spec ---
    getAllBatches(filter?: string): Observable<MedicineBatch[]> {
        return this.http.get<ApiResponse<MedicineBatch[]>>(`${environment.apiUrl}/MedicineBatches`, { params: filter ? { filter } : {} }).pipe(map(res => res.data));
    }
    getBatchById(id: number): Observable<MedicineBatch> {
        return this.http.get<ApiResponse<MedicineBatch>>(`${environment.apiUrl}/MedicineBatches/${id}`).pipe(map(res => res.data));
    }
    createBatch(batch: Partial<MedicineBatch>): Observable<MedicineBatch> {
        return this.http.post<ApiResponse<MedicineBatch>>(`${environment.apiUrl}/MedicineBatches`, batch).pipe(map(res => res.data));
    }
    updateBatch(id: number, batch: Partial<MedicineBatch>): Observable<MedicineBatch> {
        return this.http.put<ApiResponse<MedicineBatch>>(`${environment.apiUrl}/MedicineBatches/${id}`, batch).pipe(map(res => res.data));
    }
    deleteBatch(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${environment.apiUrl}/MedicineBatches/${id}`).pipe(map(res => res.data));
    }

    getBatchByBarcode(barcode: string): Observable<MedicineBatch> {
        return this.http.get<ApiResponse<MedicineBatch>>(`${environment.apiUrl}/MedicineBatches/barcode/${barcode}`).pipe(map(res => res.data));
    }
    getBatchesByMedicineId(medicineId: number): Observable<MedicineBatch[]> {
        return this.http.get<ApiResponse<MedicineBatch[]>>(`${environment.apiUrl}/MedicineBatches/medicine/${medicineId}`).pipe(map(res => res.data));
    }

    getAvailableBatches(): Observable<MedicineBatch[]> {
        return this.http.get<ApiResponse<MedicineBatch[]>>(`${environment.apiUrl}/MedicineBatches/available`).pipe(map(res => res.data));
    }
    getAvailableBatchesByMedicineId(medicineId: number): Observable<MedicineBatch[]> {
        return this.http.get<ApiResponse<MedicineBatch[]>>(`${environment.apiUrl}/MedicineBatches/available/medicine/${medicineId}`).pipe(map(res => res.data));
    }
    getExpiringBatches(days?: number): Observable<MedicineBatch[]> {
        return this.http.get<ApiResponse<MedicineBatch[]>>(`${environment.apiUrl}/MedicineBatches/expiring`, { params: days ? { days: days.toString() } : {} }).pipe(map(res => res.data));
    }
    getExpiredBatches(): Observable<MedicineBatch[]> {
        return this.http.get<ApiResponse<MedicineBatch[]>>(`${environment.apiUrl}/MedicineBatches/expired`).pipe(map(res => res.data));
    }
    getBatchesByStatus(status: string): Observable<MedicineBatch[]> {
        return this.http.get<ApiResponse<MedicineBatch[]>>(`${environment.apiUrl}/MedicineBatches/status/${status}`).pipe(map(res => res.data));
    }

    getTotalQuantity(medicineId: number): Observable<number> {
        return this.http.get<ApiResponse<number>>(`${environment.apiUrl}/MedicineBatches/total-quantity/${medicineId}`).pipe(map(res => res.data));
    }

    // Batch Operations
    validateBatchForSale(batchId: number, quantity: number): Observable<any> {
        return this.http.post<ApiResponse<any>>(`${environment.apiUrl}/MedicineBatches/${batchId}/validate`, { quantity }).pipe(map(res => res.data));
    }
    updateExpiredBatches(): Observable<number> {
        return this.http.post<ApiResponse<number>>(`${environment.apiUrl}/MedicineBatches/update-expired`, {}).pipe(map(res => res.data));
    }

    // --- StockMovement Service Spec ---
    getAllMovements(query?: StockMovementQueryDto): Observable<PagedResult<InventoryMovement>> {
        return this.http.get<ApiResponse<PagedResult<InventoryMovement>>>(`${environment.apiUrl}/StockMovements`, { params: query as any }).pipe(map(res => res.data));
    }
    getMovementById(id: number): Observable<InventoryMovement> {
        return this.http.get<ApiResponse<InventoryMovement>>(`${environment.apiUrl}/StockMovements/${id}`).pipe(map(res => res.data));
    }
    createMovement(movement: Partial<InventoryMovement>): Observable<InventoryMovement> {
        return this.http.post<ApiResponse<InventoryMovement>>(`${environment.apiUrl}/StockMovements`, movement).pipe(map(res => res.data));
    }
    updateMovement(id: number, movement: Partial<InventoryMovement>): Observable<InventoryMovement> {
        return this.http.put<ApiResponse<InventoryMovement>>(`${environment.apiUrl}/StockMovements/${id}`, movement).pipe(map(res => res.data));
    }
    deleteMovement(id: number): Observable<void> {
        return this.http.delete<ApiResponse<void>>(`${environment.apiUrl}/StockMovements/${id}`).pipe(map(res => res.data));
    }

    getStockCard(batchId: number): Observable<InventoryMovement[]> {
        return this.http.get<ApiResponse<InventoryMovement[]>>(`${environment.apiUrl}/StockMovements/stock-card/${batchId}`).pipe(map(res => res.data));
    }

    /**
     * Create manual stock movement (matches backend DTO)
     * @param movement - Manual movement DTO matching backend structure
     */
    createManualMovement(movement: {
        medicineId: number;
        batchId: number;
        quantity: number;
        type: number; // 5=Adjustment, 6=Damage, 7=Expiry
        reason: string;
        approvedBy: number;
    }): Observable<InventoryMovement> {
        return this.http.post<ApiResponse<InventoryMovement>>(`${environment.apiUrl}/Movements/manual`, movement).pipe(map(res => res.data));
    }
}
