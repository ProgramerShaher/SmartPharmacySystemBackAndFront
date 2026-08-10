import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { SupplierService } from '../../services/supplier.service';
import { SupplierStatement, StatementItemDto } from '../../../../core/models/supplier.models';
import { MessageService } from 'primeng/api';
import { SettingsService } from '../../../../core/services/settings.service';
import { PharmacySettings } from '../../../../core/models/settings/pharmacy-settings.interface';
import { AuthService } from '../../../auth/services/auth.service';

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

    pharmacySettings = signal<PharmacySettings>({
        id: 0,
        pharmacyName: 'الصيدلية الذكية',
        taxNumber: '',
        phoneNumber: '',
        address: 'اليمن',
        baseCurrency: 'ر.ي'
    });

    branchName = signal<string>('الفرع الرئيسي');

    // Search
    filteredSuppliers: any[] = [];
    searchQuery: any = null;

    today = new Date();

    constructor(
        private route: ActivatedRoute,
        private supplierService: SupplierService,
        private messageService: MessageService,
        private settingsService: SettingsService,
        private authService: AuthService
    ) { }

    ngOnInit() {
        // Load Pharmacy Settings
        this.settingsService.getSettings().subscribe({
            next: (settings) => {
                if (settings) {
                    this.pharmacySettings.set(settings);
                }
            }
        });

        // Load Branch Name from Current User
        const user = this.authService.currentUserValue;
        if (user) {
            const bName = user.branchName || (user.username === 'admin' ? 'الفرع الرئيسي' : 'الفرع ' + (user.branchId || 'الرئيسي'));
            this.branchName.set(bName);
        }

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

    /** إجمالي فواتير الشراء الآجلة (دين علينا للمورد) */
    getCreditTotal(): number {
        return this.statement()?.transactions
            ?.filter(t => t.type === 'فاتورة شراء')
            ?.reduce((s, t) => s + (t.credit || 0), 0) ?? 0;
    }

    /** إجمالي سندات الصرف المدفوعة */
    getPaymentTotal(): number {
        return this.statement()?.transactions
            ?.filter(t => t.type === 'سند صرف')
            ?.reduce((s, t) => s + (t.debit || 0), 0) ?? 0;
    }

    /** إجمالي المرتجعات (دين على المورد لنا) */
    getReturnTotal(): number {
        return this.statement()?.transactions
            ?.filter(t => t.type === 'مرتجع شراء')
            ?.reduce((s, t) => s + (t.debit || 0), 0) ?? 0;
    }

    get Math() {
        return Math;
    }
}
