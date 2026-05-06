import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { InventoryMovementService } from '../../services/inventory-movement.service';
import {
  StockMovementDto,
  StockMovementQueryDto,
  PagedResult,
  StockMovementType,
  ReferenceType,
  StockMovementSummary
} from '../../../../core/models';

import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { TagModule } from 'primeng/tag';
import { ToolbarModule } from 'primeng/toolbar';
import { CardModule } from 'primeng/card';
import { ChartModule } from 'primeng/chart';
import { TooltipModule } from 'primeng/tooltip';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';

@Component({
  selector: 'app-movement-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    DropdownModule,
    CalendarModule,
    TagModule,
    ToolbarModule,
    CardModule,
    ChartModule,
    TooltipModule,
    ToastModule
  ],
  providers: [MessageService],
  templateUrl: './movement-list.component.html',
  styleUrls: ['./movement-list.component.scss']
})
export class MovementListComponent implements OnInit {
  movements = signal<StockMovementDto[]>([]);
  loading = signal(false);
  totalRecords = signal(0);
  showFilters = signal(false);

  totalStockValue = signal(0);
  nearExpiryCount = signal(0);
  lowStockCount = signal(0);
  todayMovements = signal(0);

  movementTrendData: any = { labels: [], datasets: [] };
  movementTrendOptions: any;
  categoryDistributionData: any = { labels: [], datasets: [] };
  categoryDistributionOptions: any;

  query: StockMovementQueryDto = {
    page: 1,
    pageSize: 15
  };

  startDate: Date | null = null;
  endDate: Date | null = null;

  movementTypes = [
    { label: 'توريد', value: StockMovementType.Purchase },
    { label: 'بيع', value: StockMovementType.Sale },
    { label: 'مردود مشتريات', value: StockMovementType.PurchaseReturn },
    { label: 'مردود مبيعات', value: StockMovementType.SalesReturn },
    { label: 'تعديل مخزون', value: StockMovementType.Adjustment },
    { label: 'تالف', value: StockMovementType.Damage },
    { label: 'منتهي الصلاحية', value: StockMovementType.Expiry }
  ];

  constructor(
    private movementService: InventoryMovementService,
    private router: Router,
    private messageService: MessageService
  ) {
    this.initCharts();
  }

  ngOnInit() {
    this.loadMovements();
    this.loadKPIStats();
  }

  initCharts() {
    this.movementTrendOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: 'top',
          labels: { font: { family: 'Cairo, sans-serif', size: 10 }, usePointStyle: true, padding: 10 }
        },
        tooltip: {
          backgroundColor: 'rgba(15, 23, 42, 0.9)',
          titleFont: { family: 'Cairo, sans-serif', size: 12 },
          bodyFont: { family: 'Cairo, sans-serif', size: 11 },
          padding: 10,
          cornerRadius: 8
        }
      },
      scales: {
        y: { beginAtZero: true, ticks: { font: { family: 'Cairo, sans-serif', size: 10 } } },
        x: { ticks: { font: { family: 'Cairo, sans-serif', size: 9 } } }
      }
    };

    this.categoryDistributionOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: 'bottom',
          labels: { font: { family: 'Cairo, sans-serif', size: 10 }, padding: 8, usePointStyle: true }
        },
        tooltip: {
          backgroundColor: 'rgba(15, 23, 42, 0.9)',
          titleFont: { family: 'Cairo, sans-serif' },
          bodyFont: { family: 'Cairo, sans-serif' },
          padding: 10,
          cornerRadius: 8
        }
      },
      cutout: '62%'
    };
  }

  loadMovements() {
    this.loading.set(true);

    if (this.startDate) {
      this.query.startDate = this.toDateOnly(this.startDate);
    } else {
      delete this.query.startDate;
    }

    if (this.endDate) {
      this.query.endDate = this.toDateOnly(this.endDate);
    } else {
      delete this.query.endDate;
    }

    this.movementService.getAll(this.query).subscribe({
      next: (result: PagedResult<StockMovementDto>) => {
        this.movements.set(result.items || []);
        this.totalRecords.set(result.totalCount || 0);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error loading movements:', error);
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل حركات المخزون من قاعدة البيانات' });
        this.loading.set(false);
      }
    });
  }

  loadKPIStats() {
    this.movementService.getSummary().subscribe({
      next: (summary: StockMovementSummary) => {
        this.totalStockValue.set(summary.totalStockValue || 0);
        this.nearExpiryCount.set(summary.nearExpiryCount || 0);
        this.lowStockCount.set(summary.lowStockCount || 0);
        this.todayMovements.set(summary.todayMovements || 0);
        this.updateTrendChart(summary.last30DaysTrend || []);
        this.updateCategoryChart(summary.categoryDistribution || []);
      },
      error: (error) => console.error('Error loading movement summary:', error)
    });
  }

  onPageChange(event: any) {
    this.query.page = (event.first / event.rows) + 1;
    this.query.pageSize = event.rows;
    this.loadMovements();
  }

  onDateChange() {
    this.query.page = 1;
    this.loadMovements();
  }

  toggleFilters() {
    this.showFilters.update(v => !v);
  }

  viewDetails(movement: StockMovementDto) {
    this.router.navigate(['/inventory/movements', movement.id]);
  }

  exportPDF() {
    this.messageService.add({ severity: 'info', summary: 'تصدير PDF', detail: 'سيتم تجهيز التصدير لاحقًا' });
  }

  exportExcel() {
    this.messageService.add({ severity: 'info', summary: 'تصدير Excel', detail: 'سيتم تجهيز التصدير لاحقًا' });
  }

  isAddition(type: StockMovementType): boolean {
    return type === StockMovementType.Purchase ||
      type === StockMovementType.SalesReturn ||
      type === StockMovementType.Adjustment;
  }

  getQuantityClass(movement: StockMovementDto): string {
    return this.isAddition(movement.movementType) ? 'movement-qty movement-qty-in' : 'movement-qty movement-qty-out';
  }

  getTypeClass(type: StockMovementType): string {
    switch (type) {
      case StockMovementType.Purchase:
      case StockMovementType.SalesReturn:
        return 'type-pill type-green';
      case StockMovementType.Sale:
      case StockMovementType.PurchaseReturn:
        return 'type-pill type-blue';
      case StockMovementType.Adjustment:
        return 'type-pill type-amber';
      case StockMovementType.Damage:
      case StockMovementType.Expiry:
        return 'type-pill type-red';
      default:
        return 'type-pill type-gray';
    }
  }

  getMovementTypeLabel(type: StockMovementType): string {
    return this.movementService.getMovementTypeLabel(type);
  }

  getMovementTypeSeverity(type: StockMovementType): 'success' | 'info' | 'warning' | 'danger' | 'secondary' {
    return this.movementService.getMovementTypeSeverity(type);
  }

  getReferenceTypeLabel(type: ReferenceType): string {
    return this.movementService.getReferenceTypeLabel(type);
  }

  private updateTrendChart(trend: any[]) {
    this.movementTrendData = {
      labels: trend.map(item => new Date(item.date).toLocaleDateString('ar-EG', { day: '2-digit', month: '2-digit' })),
      datasets: [
        {
          label: 'الإضافات',
          data: trend.map(item => item.additions),
          borderColor: '#10b981',
          backgroundColor: 'rgba(16, 185, 129, 0.12)',
          tension: 0.35,
          fill: true
        },
        {
          label: 'الخصومات',
          data: trend.map(item => item.deductions),
          borderColor: '#ef4444',
          backgroundColor: 'rgba(239, 68, 68, 0.10)',
          tension: 0.35,
          fill: true
        }
      ]
    };
  }

  private updateCategoryChart(categories: any[]) {
    const colors = ['#10b981', '#3b82f6', '#f59e0b', '#8b5cf6', '#ec4899', '#14b8a6', '#f97316', '#64748b'];
    this.categoryDistributionData = {
      labels: categories.map(item => item.categoryName || 'بدون تصنيف'),
      datasets: [{
        data: categories.map(item => item.quantity),
        backgroundColor: colors,
        hoverBackgroundColor: colors
      }]
    };
  }

  private toDateOnly(date: Date): string {
    const year = date.getFullYear();
    const month = `${date.getMonth() + 1}`.padStart(2, '0');
    const day = `${date.getDate()}`.padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
