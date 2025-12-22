import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { SalesService } from '../../services/sales.service';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { TableModule } from 'primeng/table';
import { MessageService } from 'primeng/api';

@Component({
    selector: 'app-sale-invoice-add',
    standalone: true,
    imports: [CommonModule, ReactiveFormsModule, ButtonModule, InputTextModule, DropdownModule, TableModule],
    providers: [MessageService],
    templateUrl: './sale-invoice-add.component.html'
})
export class SaleInvoiceAddComponent {
    saleForm: FormGroup;
    paymentMethods = [
        { label: 'Cash', value: 'CASH' },
        { label: 'Card', value: 'CARD' },
        { label: 'Insurance', value: 'INSURANCE' }
    ];
    totalAmount = 0;

    constructor(
        private fb: FormBuilder,
        private salesService: SalesService,
        private router: Router,
        private messageService: MessageService
    ) {
        this.saleForm = this.fb.group({
            customerName: [''],
            paymentMethod: ['CASH', Validators.required],
            items: this.fb.array([])
        });
        this.addItem(); // Start with one item
    }

    get items() {
        return this.saleForm.get('items') as FormArray;
    }

    addItem() {
        const item = this.fb.group({
            medicineId: [null, Validators.required],
            quantity: [1, Validators.min(1)],
            unitPrice: [0, Validators.min(0)],
            totalPrice: [0]
        });
        this.items.push(item);
    }

    removeItem(index: number) {
        this.items.removeAt(index);
        this.calculateGrandTotal();
    }

    calculateTotal(index: number) {
        const item = this.items.at(index);
        const qty = item.get('quantity')?.value || 0;
        const price = item.get('unitPrice')?.value || 0;
        item.patchValue({ totalPrice: qty * price });
        this.calculateGrandTotal();
    }

    calculateGrandTotal() {
        this.totalAmount = this.items.controls.reduce((acc, curr) => acc + (curr.get('totalPrice')?.value || 0), 0);
    }

    saveSale() {
        if (this.saleForm.invalid) return;

        const saleData = {
            ...this.saleForm.value,
            totalAmount: this.totalAmount
        };

        this.salesService.createInvoice(saleData).subscribe({
            next: () => {
                this.messageService.add({ severity: 'success', summary: 'Success', detail: 'Sale completed' });
                this.router.navigate(['/sales/invoices']);
            },
            error: (e) => console.error(e)
        });
    }

    cancel() {
        this.router.navigate(['/sales/invoices']);
    }
}
