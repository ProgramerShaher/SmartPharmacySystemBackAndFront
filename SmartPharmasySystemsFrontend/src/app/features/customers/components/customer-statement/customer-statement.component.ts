import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CustomerService } from '../../services/customer.service';
import { CustomerStatement, CustomerTransaction } from '../../../../core/models/customer.models';
import { MessageService } from 'primeng/api';

// PrimeNG
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CalendarModule } from 'primeng/calendar';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ToastModule } from 'primeng/toast';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-customer-statement',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    TableModule,
    ButtonModule,
    CalendarModule,
    CardModule,
    TagModule,
    TooltipModule,
    ToastModule
  ],
  providers: [MessageService],
  templateUrl: './customer-statement.component.html',
  styleUrls: ['./customer-statement.component.scss']
})
export class CustomerStatementComponent implements OnInit {
  statement = signal<CustomerStatement | null>(null);
  loading = signal(false);
  customerId: number | null = null;

  dateFrom: Date | null = null;
  dateTo: Date | null = null;
  today: Date = new Date();

  constructor(
    private route: ActivatedRoute,
    private customerService: CustomerService,
    private messageService: MessageService
  ) { }

  ngOnInit() {
    this.customerId = Number(this.route.snapshot.paramMap.get('id'));
    if (this.customerId) {
      this.loadStatement();
    }
  }

  loadStatement() {
    if (!this.customerId) return;
    this.loading.set(true);

    // Format dates for API if present
    const from = this.dateFrom ? this.dateFrom.toISOString() : undefined;
    const to = this.dateTo ? this.dateTo.toISOString() : undefined;

    this.customerService.getStatement(this.customerId, from, to).subscribe({
      next: (res) => {
        this.statement.set(res);
        this.loading.set(false);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل كشف الحساب' });
        this.loading.set(false);
      }
    });
  }

  printStatement() {
    window.print();
  }

  getTransactionSeverity(type: string): string {
    switch (type) {
      case 'SaleInvoice': return 'info';
      case 'Receipt': return 'success';
      case 'Return': return 'warning';
      default: return 'secondary';
    }
  }

  getTransactionLabel(type: string): string {
    switch (type) {
      case 'SaleInvoice': return 'فاتورة مبيعات';
      case 'Receipt': return 'سند قبض';
      case 'Return': return 'مرتجع مبيعات';
      default: return type;
    }
  }
}
