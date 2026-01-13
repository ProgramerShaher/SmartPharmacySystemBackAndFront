import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { CalendarModule } from 'primeng/calendar';
import { TableModule } from 'primeng/table';
import { InputNumberModule } from 'primeng/inputnumber';
import { TagModule } from 'primeng/tag';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ReportService } from '../../../core/services/report.service';
import { BestSellingMedicinesReport } from '../../../core/models/reports.interface';

@Component({
  selector: 'app-best-selling',
  standalone: true,
  imports: [CommonModule, FormsModule, CardModule, ButtonModule, CalendarModule, TableModule, InputNumberModule, TagModule, ProgressSpinnerModule],
  templateUrl: './best-selling.component.html',
  styleUrls: ['./best-selling.component.css']
})
export class BestSellingComponent implements OnInit {
  private readonly reportService = inject(ReportService);

  report = signal<BestSellingMedicinesReport | null>(null);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);
  
  fromDate: Date = new Date(new Date().getFullYear(), new Date().getMonth(), 1);
  toDate: Date = new Date();
  topCount: number = 10;

  summary = computed(() => ({
    totalSold: this.report()?.totalMedicinesSold || 0,
    totalRevenue: this.report()?.totalRevenue || 0
  }));

  ngOnInit(): void {
    this.loadReport();
  }

  async loadReport(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);
    try {
      const result = await this.reportService.getBestSellingMedicines(this.fromDate, this.toDate, this.topCount).toPromise();
      this.report.set(result || null);
    } catch (err: any) {
      this.error.set(err?.error?.message || 'حدث خطأ');
    } finally {
      this.loading.set(false);
    }
  }

  applyFilter(): void { this.loadReport(); }
  formatCurrency(v: number): string { return v.toLocaleString('ar-SA', { minimumFractionDigits: 2 }); }
  printReport(): void { window.print(); }
}
