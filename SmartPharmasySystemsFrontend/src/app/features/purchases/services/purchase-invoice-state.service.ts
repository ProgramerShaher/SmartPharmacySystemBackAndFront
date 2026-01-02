import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import { PurchaseInvoice } from '../../../core/models';

/**
 * Purchase Invoice State Management Service
 * Manages reactive state for purchase invoices
 */
@Injectable({ providedIn: 'root' })
export class PurchaseInvoiceStateService {
    
    // State Subjects
    private invoicesSubject = new BehaviorSubject<PurchaseInvoice[]>([]);
    private selectedInvoiceSubject = new BehaviorSubject<PurchaseInvoice | null>(null);
    private loadingSubject = new BehaviorSubject<boolean>(false);
    private errorSubject = new BehaviorSubject<string | null>(null);
    private supplierBalanceSubject = new BehaviorSubject<Map<number, number>>(new Map());

    // Public Observables
    public invoices$ = this.invoicesSubject.asObservable();
    public selectedInvoice$ = this.selectedInvoiceSubject.asObservable();
    public loading$ = this.loadingSubject.asObservable();
    public error$ = this.errorSubject.asObservable();
    public supplierBalance$ = this.supplierBalanceSubject.asObservable();

    /**
     * Set invoices list
     */
    setInvoices(invoices: PurchaseInvoice[]): void {
        this.invoicesSubject.next(invoices);
    }

    /**
     * Get current invoices value
     */
    getInvoices(): PurchaseInvoice[] {
        return this.invoicesSubject.value;
    }

    /**
     * Add invoice to list
     */
    addInvoice(invoice: PurchaseInvoice): void {
        const currentInvoices = this.invoicesSubject.value;
        this.invoicesSubject.next([invoice, ...currentInvoices]);
    }

    /**
     * Update invoice in list
     */
    updateInvoiceInList(updatedInvoice: PurchaseInvoice): void {
        const currentInvoices = this.invoicesSubject.value;
        const index = currentInvoices.findIndex(inv => inv.id === updatedInvoice.id);
        
        if (index !== -1) {
            currentInvoices[index] = updatedInvoice;
            this.invoicesSubject.next([...currentInvoices]);
        }

        // Update selected invoice if it's the same
        if (this.selectedInvoiceSubject.value?.id === updatedInvoice.id) {
            this.selectedInvoiceSubject.next(updatedInvoice);
        }
    }

    /**
     * Remove invoice from list
     */
    removeInvoiceFromList(id: number): void {
        const currentInvoices = this.invoicesSubject.value;
        const filtered = currentInvoices.filter(inv => inv.id !== id);
        this.invoicesSubject.next(filtered);

        // Clear selected if it was removed
        if (this.selectedInvoiceSubject.value?.id === id) {
            this.selectedInvoiceSubject.next(null);
        }
    }

    /**
     * Set selected invoice
     */
    selectInvoice(invoice: PurchaseInvoice | null): void {
        this.selectedInvoiceSubject.next(invoice);
    }

    /**
     * Get selected invoice value
     */
    getSelectedInvoice(): PurchaseInvoice | null {
        return this.selectedInvoiceSubject.value;
    }

    /**
     * Set loading state
     */
    setLoading(loading: boolean): void {
        this.loadingSubject.next(loading);
    }

    /**
     * Set error state
     */
    setError(error: string | null): void {
        this.errorSubject.next(error);
    }

    /**
     * Clear error
     */
    clearError(): void {
        this.errorSubject.next(null);
    }

    /**
     * Update supplier balance
     */
    updateSupplierBalance(supplierId: number, balance: number): void {
        const balances = this.supplierBalanceSubject.value;
        balances.set(supplierId, balance);
        this.supplierBalanceSubject.next(new Map(balances));
    }

    /**
     * Get supplier balance
     */
    getSupplierBalance(supplierId: number): number | undefined {
        return this.supplierBalanceSubject.value.get(supplierId);
    }

    /**
     * Clear all state
     */
    clearState(): void {
        this.invoicesSubject.next([]);
        this.selectedInvoiceSubject.next(null);
        this.loadingSubject.next(false);
        this.errorSubject.next(null);
        this.supplierBalanceSubject.next(new Map());
    }
}
