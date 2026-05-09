import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { CalendarModule } from 'primeng/calendar';
import { TableModule } from 'primeng/table';
import { ChartModule } from 'primeng/chart';
import { TagModule } from 'primeng/tag';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ReportService } from '../../../core/services/report.service';
import { DailySalesReport } from '../../../core/models/reports.interface';

@Component({
  selector: 'app-daily-sales',
  standalone: true,
  imports: [CommonModule, FormsModule, CardModule, ButtonModule, CalendarModule, TableModule, ChartModule, TagModule, ProgressSpinnerModule],
  templateUrl: './daily-sales.component.html',
  styleUrls: ['./daily-sales.component.css']
})
export class DailySalesComponent implements OnInit {
  private readonly reportService = inject(ReportService);

  report = signal<DailySalesReport | null>(null);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);
  selectedDate: Date = new Date();

  chartData = signal<any>(null);
  chartOptions = { responsive: true, maintainAspectRatio: false };

  summary = computed(() => ({
    totalSales: this.report()?.totalSales || 0,
    grossProfit: this.report()?.grossProfit || 0,
    invoiceCount: this.report()?.invoiceCount || 0,
    itemsSold: this.report()?.itemsSold || 0,
    profitMargin: this.report()?.profitMargin || 0
  }));

  ngOnInit(): void {
    this.loadReport();
  }

  async loadReport(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);
    try {
      const result = await this.reportService.getDailySalesReport(this.selectedDate).toPromise();
      this.report.set(result || null);
      this.updateChart(result?.salesByHour || []);
    } catch (err: any) {
      this.error.set(err?.error?.message || 'حدث خطأ');
    } finally {
      this.loading.set(false);
    }
  }

  updateChart(hourlyData: any[]): void {
    if (!hourlyData.length) return;
    this.chartData.set({
      labels: hourlyData.map(h => `${h.hour}:00`),
      datasets: [{
        label: 'المبيعات',
        data: hourlyData.map(h => h.amount),
        backgroundColor: 'rgba(59, 130, 246, 0.5)',
        borderColor: 'rgb(59, 130, 246)',
        borderWidth: 2
      }]
    });
  }

  applyFilter(): void { this.loadReport(); }
  formatCurrency(v: number): string { return v.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 }); }
  printReport(): void { window.print(); }
}
