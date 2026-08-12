import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { DropdownModule } from 'primeng/dropdown';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ChartModule } from 'primeng/chart';
import { TableModule } from 'primeng/table';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { InputTextModule } from 'primeng/inputtext';
import { InventoryMovementService } from '../../services/inventory-movement.service';
import { MedicineService } from '../../services/medicine.service';

@Component({
  selector: 'app-inventory-flow-analysis',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ReactiveFormsModule,
    DropdownModule,
    ButtonModule,
    ProgressSpinnerModule,
    ChartModule,
    TableModule,
    ToastModule,
    InputTextModule
  ],
  providers: [MessageService],
  templateUrl: './inventory-flow-analysis.component.html',
  styleUrls: ['./inventory-flow-analysis.component.scss']
})
export class InventoryFlowAnalysisComponent implements OnInit {
  private readonly movementService = inject(InventoryMovementService);
  private readonly medicineService = inject(MedicineService);
  private readonly fb = inject(FormBuilder);
  private readonly messageService = inject(MessageService);

  searchForm!: FormGroup;
  medicines: any[] = [];
  reportData: any = null;
  loading = false;
  chartData: any;
  chartOptions: any;

  ngOnInit(): void {
    this.initForm();
    this.loadMedicines();
    this.initChartOptions();
  }

  initForm(): void {
    this.searchForm = this.fb.group({
      medicineId: [null, Validators.required],
      batchNumber: ['']
    });
  }

  loadMedicines(): void {
    this.medicineService.getAll({ pageSize: 10000 }).subscribe({
      next: (data: any) => this.medicines = data.items,
      error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل الأدوية' })
    });
  }

  generateReport(): void {
    if (this.searchForm.invalid) return;

    this.loading = true;
    const { medicineId, batchNumber } = this.searchForm.value;

    this.movementService.getInventoryFlowReport(medicineId, batchNumber).subscribe({
      next: (data: any) => {
        this.reportData = data;
        this.setupChart(data);
        this.loading = false;
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل تقرير التدفق المخزني' });
        this.loading = false;
      }
    });
  }

  setupChart(data: any): void {
    this.chartData = {
      labels: ['رصيد افتتاحي', 'مشتريات', 'مبيعات', 'تحويلات واردة', 'تحويلات صادرة', 'تالف', 'تسويات'],
      datasets: [
        {
          label: 'التدفق المخزني',
          backgroundColor: ['#3b82f6', '#10b981', '#ef4444', '#f59e0b', '#6366f1', '#ec4899', '#8b5cf6'],
          data: [
            data.openingBalance,
            data.totalPurchases,
            data.totalSales,
            data.totalTransfersIn,
            data.totalTransfersOut,
            data.totalDamages,
            data.totalAdjustments
          ]
        }
      ]
    };
  }

  initChartOptions(): void {
    this.chartOptions = {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: {
          display: false
        }
      },
      scales: {
        y: {
          beginAtZero: true
        }
      }
    };
  }
}
