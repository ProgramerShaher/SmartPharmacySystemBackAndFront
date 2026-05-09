import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ReportService } from '../../../core/services/report.service';
import { CustomerDebtsReport } from '../../../core/models/reports.interface';

@Component({
  selector: 'app-customer-debts',
  standalone: true,
  imports: [CommonModule, FormsModule, CardModule, ButtonModule, TableModule, TagModule, ProgressSpinnerModule],
  templateUrl: './customer-debts.component.html',
  styleUrls: ['./customer-debts.component.css']
})
export class CustomerDebtsComponent implements OnInit {
  protected readonly Math = Math;
  private readonly reportService = inject(ReportService);


  report = signal<CustomerDebtsReport | null>(null);
  loading = signal<boolean>(false);
  error = signal<string | null>(null);

  summary = computed(() => ({
    totalReceivable: this.report()?.totalReceivable || 0,
    debtorCount: this.report()?.debtorCount || 0,
    creditorCount: this.report()?.creditorCount || 0
  }));

  ngOnInit(): void {
    this.loadReport();
  }

  async loadReport(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);
    try {
      const result = await this.reportService.getCustomerDebtsReport().toPromise();
      this.report.set(result || null);
    } catch (err: any) {
      this.error.set(err?.error?.message || 'حدث خطأ');
    } finally {
      this.loading.set(false);
    }
  }

  formatCurrency(v: number): string { return v.toLocaleString('en-US', { minimumFractionDigits: 0, maximumFractionDigits: 0 }); }
  printReport(): void { window.print(); }
}
