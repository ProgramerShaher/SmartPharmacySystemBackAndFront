import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { PurchaseInvoiceService } from '../../services/purchase-invoice.service';
import { PurchaseReturn } from '../../../../core/models';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { ToolbarModule } from 'primeng/toolbar';
import { TagModule } from 'primeng/tag';

@Component({
    selector: 'app-purchase-return-list',
    standalone: true,
    imports: [CommonModule, TableModule, ButtonModule, ToolbarModule, TagModule],
    template: `
        <div class="card p-0 shadow-3 border-round-xl overflow-hidden" dir="rtl">
            <p-toolbar styleClass="bg-orange-600 border-none p-4 text-white">
                <div class="p-toolbar-group-start">
                    <div class="flex align-items-center gap-3">
                        <i class="pi pi-replay text-4xl"></i>
                        <div>
                            <h2 class="m-0 text-2xl font-bold">مرتجعات المشتريات</h2>
                            <small class="opacity-80">إدارة مرتجعات الأدوية للموردين</small>
                        </div>
                    </div>
                </div>
            </p-toolbar>

            <div class="p-4 bg-gray-50">
                <p-table [value]="returns" [rows]="10" [paginator]="true" [loading]="loading"
                    styleClass="p-datatable-gridlines shadow-1 border-round overflow-hidden">
                    <ng-template pTemplate="header">
                        <tr>
                            <th class="text-right">رقم الفاتورة الأصلية</th>
                            <th class="text-right">تاريخ الإرجاع</th>
                            <th class="text-right">المورد</th>
                            <th class="text-center">المبلغ المسترد</th>
                            <th class="text-center">السبب</th>
                        </tr>
                    </ng-template>
                    <ng-template pTemplate="body" let-ret>
                        <tr>
                            <td class="font-bold text-indigo-700">{{ret.purchaseInvoiceNumber}}</td>
                            <td>{{ret.returnDate | date:'dd/MM/yyyy HH:mm'}}</td>
                            <td>{{ret.supplierName}}</td>
                            <td class="text-center font-bold text-red-600">
                                {{ret.totalAmount | number:'1.2-2'}} ر.ي
                            </td>
                            <td class="text-center text-sm font-italic text-600">{{ret.reason}}</td>
                        </tr>
                    </ng-template>
                </p-table>
            </div>
        </div>
    `
})
export class PurchaseReturnListComponent implements OnInit {
    returns: PurchaseReturn[] = [];
    loading = true;

    constructor(private purchaseService: PurchaseInvoiceService) { }

    ngOnInit() {
        this.loadReturns();
    }

    loadReturns() {
        this.loading = true;
        this.purchaseService.getAllReturns().subscribe(data => {
            this.returns = data;
            this.loading = false;
        });
    }
}
