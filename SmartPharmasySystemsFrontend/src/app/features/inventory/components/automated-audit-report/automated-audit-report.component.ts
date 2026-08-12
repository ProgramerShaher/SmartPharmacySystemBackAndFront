import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { TableModule } from 'primeng/table';
import { ChartModule } from 'primeng/chart';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { AutomatedAuditService } from '../../services/automated-audit.service';
import { AutomatedAuditHeaderDto, AuditChartDataDto } from '../../../../core/models/automated-audit.interface';

@Component({
  selector: 'app-automated-audit-report',
  standalone: true,
  imports: [
    CommonModule,
    TableModule,
    ChartModule,
    ProgressSpinnerModule,
    ButtonModule,
    ToastModule,
    TooltipModule
  ],
  providers: [MessageService],
  templateUrl: './automated-audit-report.component.html',
  styleUrls: ['./automated-audit-report.component.scss']
})
export class AutomatedAuditReportComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private auditService = inject(AutomatedAuditService);
  private messageService = inject(MessageService);
  private location = inject(Location);

  auditId!: number;
  auditData!: AutomatedAuditHeaderDto;
  loading = true;

  chartData: any;
  chartOptions: any;

  ngOnInit() {
    this.auditId = Number(this.route.snapshot.paramMap.get('id'));
    if (this.auditId) {
      this.loadAuditDetails();
      this.loadCharts();
    }
  }

  goBack() {
    this.location.back();
  }

  loadAuditDetails() {
    this.auditService.getById(this.auditId).subscribe({
      next: (data) => {
        this.auditData = data;
        this.loading = false;
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل تفاصيل التقرير' });
        this.loading = false;
      }
    });
  }

  loadCharts() {
    this.auditService.getCharts(this.auditId).subscribe({
      next: (data) => {
        this.initCharts(data);
      }
    });
  }

  initCharts(data: AuditChartDataDto) {
    const documentStyle = getComputedStyle(document.documentElement);
    const textColor = documentStyle.getPropertyValue('--text-color');
    const textColorSecondary = documentStyle.getPropertyValue('--text-color-secondary');
    const surfaceBorder = documentStyle.getPropertyValue('--surface-border');

    this.chartData = {
      labels: data.labels,
      datasets: [
        {
          label: 'المبيعات',
          backgroundColor: documentStyle.getPropertyValue('--blue-500'),
          data: data.salesValues
        },
        {
          label: 'المشتريات',
          backgroundColor: documentStyle.getPropertyValue('--green-500'),
          data: data.purchasesValues
        },
        {
          label: 'التوالف',
          backgroundColor: documentStyle.getPropertyValue('--orange-500'),
          data: data.damagesValues
        },
        {
          label: 'النواقص',
          backgroundColor: documentStyle.getPropertyValue('--red-500'),
          data: data.shortagesValues
        }
      ]
    };

    this.chartOptions = {
      maintainAspectRatio: false,
      aspectRatio: 0.8,
      plugins: {
        legend: {
          labels: { color: textColor }
        }
      },
      scales: {
        x: {
          ticks: { color: textColorSecondary, font: { weight: 500 } },
          grid: { color: surfaceBorder, drawBorder: false }
        },
        y: {
          ticks: { color: textColorSecondary },
          grid: { color: surfaceBorder, drawBorder: false }
        }
      }
    };
  }

  getItemInTotal(item: any): number {
    return (item.openingBalance || 0)
      + (item.totalPurchases || 0)
      + (item.totalTransfersIn || 0)
      + (item.totalSalesReturns || 0)
      + (item.totalAdjustments || 0);
  }

  getItemOutTotal(item: any): number {
    return (item.totalSales || 0)
      + (item.totalTransfersOut || 0)
      + (item.totalDamages || 0)
      + (item.totalPurchaseReturns || 0);
  }

  getVarianceClass(variance: number): string {
    if (variance < 0) return 'danger';
    if (variance > 0) return 'success';
    return 'neutral';
  }

  printReport() {
    window.print();
  }
}
