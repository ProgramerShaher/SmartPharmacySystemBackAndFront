import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { InventoryMovementService } from '../../services/inventory-movement.service';
import {
  StockMovementDto,
  StockMovementQueryDto,
  PagedResult,
  StockMovementType,
  ReferenceType
} from '../../../../core/models';

// PrimeNG
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

  // KPI Stats
  totalStockValue = signal(0);
  nearExpiryCount = signal(0);
  lowStockCount = signal(0);
  todayMovements = signal(0);

  // Charts Data
  movementTrendData: any;
  movementTrendOptions: any;
  categoryDistributionData: any;
  categoryDistributionOptions: any;

  query: StockMovementQueryDto = {
    page: 1,
    pageSize: 25
  };

  startDate: Date | null = null;
  endDate: Date | null = null;

  movementTypes = [
    { label: 'توريد', value: StockMovementType.Purchase },
    { label: 'بيع', value: StockMovementType.Sale },
    { label: 'مرتجع مشتريات', value: StockMovementType.PurchaseReturn },
    { label: 'مرتجع مبيعات', value: StockMovementType.SalesReturn },
    { label: 'تعديل', value: StockMovementType.Adjustment },
    { label: 'تلف', value: StockMovementType.Damage },
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
    // Movement Trend Chart (Last 30 days)
    this.movementTrendData = {
      labels: this.getLast30Days(),
      datasets: [
        {
          label: 'التوريد',
          data: this.generateMockTrendData(30, 10, 50),
          borderColor: '#10b981',
          backgroundColor: 'rgba(16, 185, 129, 0.1)',
          tension: 0.4,
          fill: true
        },
        {
          label: 'الصرف',
          data: this.generateMockTrendData(30, 5, 40),
          borderColor: '#ef4444',
          backgroundColor: 'rgba(239, 68, 68, 0.1)',
          tension: 0.4,
          fill: true
        }
      ]
    };

    this.movementTrendOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: 'top',
          labels: {
            font: { family: 'Cairo, sans-serif', size: 12 },
            usePointStyle: true,
            padding: 15
          }
        },
        tooltip: {
          backgroundColor: 'rgba(0, 0, 0, 0.8)',
          titleFont: { family: 'Cairo, sans-serif', size: 14 },
          bodyFont: { family: 'Cairo, sans-serif', size: 12 },
          padding: 12,
          cornerRadius: 8
        }
      },
      scales: {
        y: {
          beginAtZero: true,
          ticks: { font: { family: 'Cairo, sans-serif' } }
        },
        x: {
          ticks: { font: { family: 'Cairo, sans-serif', size: 10 } }
        }
      }
    };

    // Category Distribution Chart
    this.categoryDistributionData = {
      labels: ['أدوية', 'مستحضرات', 'مكملات', 'أخرى'],
      datasets: [{
        data: [45, 25, 20, 10],
        backgroundColor: ['#10b981', '#3b82f6', '#f59e0b', '#8b5cf6'],
        hoverBackgroundColor: ['#059669', '#2563eb', '#d97706', '#7c3aed']
      }]
    };

    this.categoryDistributionOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          position: 'bottom',
          labels: {
            font: { family: 'Cairo, sans-serif', size: 11 },
            padding: 10,
            usePointStyle: true
          }
        },
        tooltip: {
          backgroundColor: 'rgba(0, 0, 0, 0.8)',
          titleFont: { family: 'Cairo, sans-serif' },
          bodyFont: { family: 'Cairo, sans-serif' },
          padding: 10,
          cornerRadius: 8,
          callbacks: {
            label: (context: any) => {
              const label = context.label || '';
              const value = context.parsed || 0;
              return `${label}: ${value}%`;
            }
          }
        }
      },
      cutout: '65%'
    };
  }

  loadMovements() {
    this.loading.set(true);

    if (this.startDate) {
      this.query.startDate = this.startDate.toISOString();
    } else {
      delete this.query.startDate;
    }

    if (this.endDate) {
      this.query.endDate = this.endDate.toISOString();
    } else {
      delete this.query.endDate;
    }

    this.movementService.getAll(this.query).subscribe({
      next: (result: PagedResult<StockMovementDto>) => {
        this.movements.set(result.items);
        this.totalRecords.set(result.totalCount);
        this.loading.set(false);
        this.calculateTodayMovements(result.items);
      },
      error: (error) => {
        console.error('Error loading movements:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل تحميل حركات المخزون'
        });
        this.loading.set(false);
      }
    });
  }

  loadKPIStats() {
    // Mock data - replace with real API calls
    this.totalStockValue.set(2450000);
    this.nearExpiryCount.set(12);
    this.lowStockCount.set(8);
  }

  calculateTodayMovements(movements: StockMovementDto[]) {
    const today = new Date().toDateString();
    const count = movements.filter(m =>
      new Date(m.date).toDateString() === today
    ).length;
    this.todayMovements.set(count);
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
    this.messageService.add({
      severity: 'info',
      summary: 'تصدير PDF',
      detail: 'جاري تجهيز الملف...'
    });
    // Implement PDF export
  }

  exportExcel() {
    this.messageService.add({
      severity: 'info',
      summary: 'تصدير Excel',
      detail: 'جاري تجهيز الملف...'
    });
    // Implement Excel export
  }

  isAddition(type: StockMovementType): boolean {
    return type === StockMovementType.Purchase ||
      type === StockMovementType.SalesReturn;
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

  // Helper methods
  private getLast30Days(): string[] {
    const days = [];
    for (let i = 29; i >= 0; i--) {
      const date = new Date();
      date.setDate(date.getDate() - i);
      days.push(date.toLocaleDateString('ar-EG', { day: '2-digit', month: '2-digit' }));
    }
    return days;
  }

  private generateMockTrendData(count: number, min: number, max: number): number[] {
    return Array.from({ length: count }, () =>
      Math.floor(Math.random() * (max - min + 1)) + min
    );
  }
}
