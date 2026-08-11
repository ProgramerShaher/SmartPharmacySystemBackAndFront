import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { TagModule } from 'primeng/tag';
import { InputNumberModule } from 'primeng/inputnumber';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { MonthlySalaryService } from '../../services/monthly-salary.service';
import { EmployeeService } from '../../services/employee.service';

@Component({
  selector: 'app-payroll',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    TableModule,
    TooltipModule,
    ToastModule,
    ConfirmDialogModule,
    DialogModule,
    DropdownModule,
    CalendarModule,
    TagModule,
    InputNumberModule,
    ProgressSpinnerModule
  ],
  templateUrl: './payroll.component.html',
  styleUrls: ['./payroll.component.scss'],
  providers: [MessageService, ConfirmationService]
})
export class PayrollComponent implements OnInit {
  records = signal<any[]>([]);
  summary = signal<any>(null);
  loading = signal<boolean>(true);
  
  filterDate = new Date();
  branches: any[] = [];
  selectedBranch: any = null;

  showForm = false;
  selectedRecord: any = null;
  formModel: any = {};
  saving = false;

  printData: any = null; // For print layout

  constructor(
    private salaryService: MonthlySalaryService,
    private employeeService: EmployeeService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.loadBranches();
  }

  loadBranches(): void {
    this.employeeService.getBranches().subscribe({
      next: (res) => {
        this.branches = res;
        if (this.branches.length > 0) {
          this.selectedBranch = this.branches[0].id;
          this.loadData();
        }
      },
      error: () => this.loading.set(false)
    });
  }

  loadData(): void {
    if (!this.selectedBranch) return;
    this.loading.set(true);
    
    const m = this.filterDate.getMonth() + 1;
    const y = this.filterDate.getFullYear();

    // 1. Get Summary
    this.salaryService.getSummary(m, y, this.selectedBranch).subscribe({
      next: (res) => this.summary.set(res),
      error: () => {}
    });

    // 2. Get Records
    this.salaryService.getByBranchMonthYear(m, y, this.selectedBranch).subscribe({
      next: (res) => {
        this.records.set(res);
        this.loading.set(false);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل مسير الرواتب' });
        this.loading.set(false);
      }
    });
  }

  payAll(): void {
    const pendingCount = this.records().filter(r => r.paymentStatus === 1).length;
    if (pendingCount === 0) {
      this.messageService.add({ severity: 'info', summary: 'ملاحظة', detail: 'لا يوجد رواتب قيد الانتظار لاعتمادها.' });
      return;
    }

    this.confirmationService.confirm({
      message: `هل أنت متأكد من ترحيل ودفع جميع الرواتب المعلقة (${pendingCount} موظف)؟`,
      header: 'تأكيد الدفع الجماعي',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.loading.set(true);
        const m = this.filterDate.getMonth() + 1;
        const y = this.filterDate.getFullYear();
        this.salaryService.payAll(m, y, this.selectedBranch).subscribe({
          next: (count) => {
            this.messageService.add({ severity: 'success', summary: 'نجاح', detail: `تم اعتماد ${count} رواتب بنجاح` });
            this.loadData();
          },
          error: () => {
            this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل الدفع الجماعي' });
            this.loading.set(false);
          }
        });
      }
    });
  }

  paySingle(id: number): void {
    this.confirmationService.confirm({
      message: 'هل أنت متأكد من اعتماد ودفع هذا الراتب؟',
      accept: () => {
        this.loading.set(true);
        this.salaryService.pay(id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم اعتماد الراتب بنجاح' });
            this.loadData();
          },
          error: () => {
            this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل اعتماد الراتب' });
            this.loading.set(false);
          }
        });
      }
    });
  }

  openEdit(record: any): void {
    this.selectedRecord = record;
    this.formModel = {
      id: record.id,
      basicSalary: record.basicSalary,
      totalAllowances: record.totalAllowances,
      totalBonuses: record.totalBonuses,
      totalDeductions: record.totalDeductions
    };
    this.showForm = true;
  }

  get netCalculated(): number {
    const b = this.formModel.basicSalary || 0;
    const a = this.formModel.totalAllowances || 0;
    const bo = this.formModel.totalBonuses || 0;
    const d = this.formModel.totalDeductions || 0;
    return b + a + bo - d;
  }

  saveEdit(): void {
    this.saving = true;
    this.salaryService.update(this.formModel).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم تحديث الراتب بنجاح' });
        this.saving = false;
        this.showForm = false;
        this.loadData();
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحديث الراتب' });
        this.saving = false;
      }
    });
  }

  printSlip(record: any): void {
    this.printData = record;
    setTimeout(() => {
      window.print();
    }, 100);
  }
}
