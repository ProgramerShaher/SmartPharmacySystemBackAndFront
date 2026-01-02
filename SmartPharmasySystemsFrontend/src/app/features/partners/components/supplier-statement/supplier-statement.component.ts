import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { SupplierService } from '../../services/supplier.service';
import { SupplierStatement, StatementItemDto } from '../../../../core/models/supplier.models';
import { MessageService } from 'primeng/api';

// PrimeNG
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { FormsModule } from '@angular/forms';

@Component({
    selector: 'app-supplier-statement',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        FormsModule,
        TableModule,
        ButtonModule,
        CardModule,
        TagModule,
        ProgressSpinnerModule,
        AutoCompleteModule
    ],
    providers: [MessageService],
    templateUrl: './supplier-statement.component.html',
    styleUrls: ['./supplier-statement.component.scss']
})
export class SupplierStatementComponent implements OnInit {
    statement = signal<SupplierStatement | null>(null);
    loading = signal(false);
    selectedSupplierId: number | null = null;

    // Search
    filteredSuppliers: any[] = [];
    searchQuery: any = null;

    today = new Date();

    constructor(
        private route: ActivatedRoute,
        private supplierService: SupplierService,
        private messageService: MessageService
    ) { }

    ngOnInit() {
        const id = this.route.snapshot.paramMap.get('id') || this.route.snapshot.queryParamMap.get('id');
        if (id) {
            this.selectedSupplierId = Number(id);
            this.loadStatement();
        }
    }

    searchSuppliers(event: any) {
        this.supplierService.getAll({ search: event.query, pageSize: 20 }).subscribe(res => {
            this.filteredSuppliers = res.items;
        });
    }

    onSupplierSelect(event: any) {
        this.selectedSupplierId = event.value.id;
        this.loadStatement();
    }

    loadStatement() {
        if (!this.selectedSupplierId) return;
        this.loading.set(true);
        this.supplierService.getStatement(this.selectedSupplierId).subscribe({
            next: (res) => {
                this.statement.set(res);
                this.loading.set(false);
            },
            error: () => {
                this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل كشف الحساب' });
                this.loading.set(false);
            }
        });
    }

    print() {
        window.print();
    }
}
