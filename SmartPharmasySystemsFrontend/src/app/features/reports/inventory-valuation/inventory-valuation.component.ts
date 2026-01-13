import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

// PrimeNG Imports
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { DropdownModule } from 'primeng/dropdown';
import { InputTextModule } from 'primeng/inputtext';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

// Services & Models
import { ReportService } from '../../../core/services/report.service';
import { InventoryValuation, BatchValuation } from '../../../core/models/reports.interface';

/**
 * تقييم المخزون - Inventory Valuation Component
 * يعرض رأس المال المخزني والقيمة البيعية والربح المحتمل
 */
@Component({
  selector: 'app-inventory-valuation',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CardModule,
    ButtonModule,
    TableModule,
    TagModule,
    DropdownModule,
    InputTextModule,
    ProgressSpinnerModule
  ],
  templateUrl: './inventory-valuation.component.html',
  styleUrls: ['./inventory-valuation.component.css']
})
export class InventoryValuationComponent implements OnInit {
  private readonly reportService = inject(ReportService);

  // ===================== Signals State =====================
  valuation = signal<InventoryValuation | null>(null);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);

  // Filters
  expiryFilter: string = 'all';
  searchTerm: string = '';

  expiryOptions = [
    { label: 'الكل', value: 'all' },
    { label: 'صالح', value: 'active' },
    { label: 'قارب على الانتهاء', value: 'expiring' },
    { label: 'منتهي', value: 'expired' }
  ];

  // ===================== Computed =====================
  summary = computed(() => {
    const v = this.valuation();
    return {
      totalCapital: v?.totalCapital || 0,
      totalRetailValue: v?.totalRetailValue || 0,
      potentialProfit: v?.potentialProfit || 0,
      totalQuantity: v?.totalQuantity || 0,
      activeBatches: v?.activeBatches || 0,
      expiredBatches: v?.expiredBatches || 0,
      expiringSoonBatches: v?.expiringSoonBatches || 0
    };
  });

  // ===================== Lifecycle =====================
  ngOnInit(): void {
    this.loadValuation();
  }

  // ===================== Methods =====================
  async loadValuation(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const result = await this.reportService.getInventoryValuation({
        expiryFilter: this.expiryFilter === 'all' ? undefined : this.expiryFilter,
        search: this.searchTerm || undefined,
        page: 1,
        pageSize: 100
      }).toPromise();

      this.valuation.set(result || null);
    } catch (err: any) {
      this.error.set(err?.error?.message || 'حدث خطأ أثناء تحميل تقييم المخزون');
      console.error('Valuation load error:', err);
    } finally {
      this.loading.set(false);
    }
  }

  applyFilter(): void {
    this.loadValuation();
  }

  getExpiryStatus(batch: BatchValuation): 'success' | 'warning' | 'danger' | 'info' {
    const today = new Date();
    const expiry = new Date(batch.expiryDate);
    const daysToExpiry = Math.ceil((expiry.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));

    if (daysToExpiry < 0) return 'danger';
    if (daysToExpiry <= 30) return 'warning';
    return 'success';
  }

  getExpiryLabel(batch: BatchValuation): string {
    const today = new Date();
    const expiry = new Date(batch.expiryDate);
    const daysToExpiry = Math.ceil((expiry.getTime() - today.getTime()) / (1000 * 60 * 60 * 24));

    if (daysToExpiry < 0) return 'منتهي';
    if (daysToExpiry <= 30) return `${daysToExpiry} يوم`;
    return 'صالح';
  }

  formatCurrency(value: number): string {
    return value.toLocaleString('ar-SA', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }

  printReport(): void {
    window.print();
  }
}
