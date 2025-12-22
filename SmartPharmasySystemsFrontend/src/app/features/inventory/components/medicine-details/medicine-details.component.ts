import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { InventoryService } from '../../services/inventory.service';
import { Medicine } from '../../../../core/models';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';

@Component({
    selector: 'app-medicine-details',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        ButtonModule,
        CardModule,
        TagModule,
        DividerModule,
        ProgressSpinnerModule,
        TooltipModule
    ],
    templateUrl: './medicine-details.component.html',
    styleUrls: ['./medicine-details.component.scss'] // إضافة ملف SCSS
})
export class MedicineDetailsComponent implements OnInit {
    medicine: Medicine | null = null;
    loading = true;

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private inventoryService: InventoryService
    ) { }

    ngOnInit() {
        this.loadMedicineDetails();
    }

    /**
     * تحميل تفاصيل الدواء
     */
    private loadMedicineDetails(): void {
        const id = Number(this.route.snapshot.paramMap.get('id'));

        if (!id || isNaN(id)) {
            this.navigateToMedicinesList();
            return;
        }

        this.loading = true;
        this.inventoryService.getMedicineById(id).subscribe({
            next: (data) => {
                this.medicine = data;
                this.loading = false;
            },
            error: (err) => {
                console.error('Error loading medicine details:', err);
                this.loading = false;
                this.navigateToMedicinesList();
            }
        });
    }

    /**
     * الحصول على حالة المخزون كنص
     */
    getStockStatus(): string {
        const stock = this.getCurrentStock();
        return stock > 0 ? 'متوفر' : 'نفذ';
    }

    /**
     * الحصول على مستوى الشدة لحالة المخزون
     */
    getStockSeverity(): 'success' | 'danger' {
        return this.getCurrentStock() > 0 ? 'success' : 'danger';
    }

    /**
     * حساب المخزون الحالي
     */
    getCurrentStock(): number {
        if (!this.medicine) return 0;
        return this.medicine.stock ?? this.medicine.totalQuantity ?? 0;
    }

    /**
     * التحقق من وجود مخزون منخفض
     */
    isLowStock(): boolean {
        const currentStock = this.getCurrentStock();
        const minAlert = this.medicine?.minAlertQuantity ?? 0;
        return currentStock > 0 && currentStock <= minAlert;
    }

    /**
     * العودة إلى قائمة الأدوية
     */
    goBack(): void {
        this.router.navigate(['/inventory/medicines']);
    }

    /**
     * التنقل إلى قائمة الأدوية
     */
    private navigateToMedicinesList(): void {
        this.router.navigate(['/inventory/medicines']);
    }
}