import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { CardModule } from 'primeng/card';
import { ButtonModule } from 'primeng/button';
import { CalendarModule } from 'primeng/calendar';
import { DropdownModule } from 'primeng/dropdown';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ReportService } from '../../../core/services/report.service';
import { EmployeePerformanceReport } from '../../../core/models/reports.interface';
import { UsersService } from '../../users/services/users.service';
import { Role, User } from '../../../core/models';

@Component({
  selector: 'app-employee-performance',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    CardModule,
    ButtonModule,
    CalendarModule,
    DropdownModule,
    TableModule,
    TagModule,
    ProgressSpinnerModule
  ],
  templateUrl: './employee-performance.component.html',
  styleUrls: ['./employee-performance.component.css']
})
export class EmployeePerformanceComponent implements OnInit {
  private readonly reportService = inject(ReportService);
  private readonly usersService = inject(UsersService);

  report = signal<EmployeePerformanceReport | null>(null);
  loading = signal(false);
  error = signal<string | null>(null);

  users = signal<User[]>([]);
  roles = signal<Role[]>([]);

  selectedEmployeeId?: number;
  selectedRoleId?: number;
  selectedOperationType = 'All';
  fromDate: Date = new Date(new Date().getFullYear(), new Date().getMonth(), 1);
  toDate: Date = new Date();

  operationTypes = [
    { label: 'الكل', value: 'All' },
    { label: 'المبيعات', value: 'Sales' },
    { label: 'المردودات', value: 'Returns' }
  ];

  employeeOptions = computed(() => [
    { label: 'كل الموظفين', value: undefined },
    ...this.users().map(user => ({ label: `${user.fullName} (${user.username})`, value: user.id }))
  ]);

  roleOptions = computed(() => [
    { label: 'كل الأدوار', value: undefined },
    ...this.roles().map(role => ({ label: role.name, value: role.id }))
  ]);

  summary = computed(() => ({
    totalSales: this.report()?.totalSales || 0,
    totalReturns: this.report()?.totalReturns || 0,
    netSales: this.report()?.netSales || 0,
    invoiceCount: this.report()?.salesInvoiceCount || 0,
    returnCount: this.report()?.salesReturnCount || 0
  }));

  ngOnInit(): void {
    this.loadFilters();
    this.loadReport();
  }

  async loadFilters(): Promise<void> {
    try {
      const [users, roles] = await Promise.all([
        this.usersService.search({ page: 1, pageSize: 500 }).toPromise(),
        this.usersService.getRoles().toPromise()
      ]);
      this.users.set(users?.items || []);
      this.roles.set(roles || []);
    } catch {
      this.users.set([]);
      this.roles.set([]);
    }
  }

  async loadReport(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const result = await this.reportService.getEmployeePerformanceReport({
        employeeId: this.selectedEmployeeId,
        roleId: this.selectedRoleId,
        fromDate: this.fromDate,
        toDate: this.toDate,
        operationType: this.selectedOperationType
      }).toPromise();
      this.report.set(result || null);
    } catch (err: any) {
      this.error.set(err?.error?.message || err?.error?.errors?.[0] || 'حدث خطأ أثناء جلب التقرير');
    } finally {
      this.loading.set(false);
    }
  }

  applyFilter(): void {
    this.loadReport();
  }

  resetFilters(): void {
    this.selectedEmployeeId = undefined;
    this.selectedRoleId = undefined;
    this.selectedOperationType = 'All';
    this.fromDate = new Date(new Date().getFullYear(), new Date().getMonth(), 1);
    this.toDate = new Date();
    this.loadReport();
  }

  formatCurrency(value: number): string {
    return value.toLocaleString('ar-SA', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }

  formatDateTime(value: string): string {
    return new Date(value).toLocaleString('ar-SA');
  }

  printReport(): void {
    window.print();
  }
}
