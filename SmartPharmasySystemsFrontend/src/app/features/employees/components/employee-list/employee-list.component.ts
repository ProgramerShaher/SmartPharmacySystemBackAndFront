import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogModule } from 'primeng/dialog';
import * as XLSX from 'xlsx';
import { EmployeeService } from '../../services/employee.service';
import { AttendanceService } from '../../services/attendance.service';
import { MonthlySalaryService } from '../../services/monthly-salary.service';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { EmployeeFormComponent } from '../employee-form/employee-form.component';

@Component({
  selector: 'app-employee-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    ButtonModule,
    TableModule,
    TooltipModule,
    ToastModule,
    ConfirmDialogModule,
    DialogModule,
    ProgressSpinnerModule,
    EmployeeFormComponent
  ],
  templateUrl: './employee-list.component.html',
  styleUrls: ['./employee-list.component.scss'],
  providers: [MessageService, ConfirmationService]
})
export class EmployeeListComponent implements OnInit {
  dashboardData = signal<any>(null);
  loading = signal<boolean>(true);

  // A local map to track attendance status per employee ID (true = Present, false = Absent)
  attendanceState = new Map<number, boolean>();

  // Calendar Dialog State
  showAttendanceDialog = false;
  selectedEmployeeForAttendance: any = null;
  calendarDays: { day: number, date: Date, isAbsent: boolean }[] = [];
  calendarLoading = false;
  absentDaysCount = 0;
  presentDaysCount = 0;

  // Salary Details Dialog State
  showSalaryDetailsDialog = false;
  selectedEmployeeForDetails: any = null;
  salaryDetails: any = null;
  detailsLoading = false;

  exporting = false;
  printData: any[] = [];

  // Form State
  showForm = false;
  selectedEmployee: any = null;
  editMode = false;

  constructor(
    private employeeService: EmployeeService,
    private attendanceService: AttendanceService,
    private monthlySalaryService: MonthlySalaryService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.loading.set(true);
    this.employeeService.getBranchDashboard().subscribe({
      next: (res) => {
        this.dashboardData.set(res);
        // Default everyone to present
        if (res && res.departments) {
          res.departments.forEach((dept: any) => {
            if (dept.employees) {
              dept.employees.forEach((emp: any) => {
                this.attendanceState.set(emp.id, true);
              });
            }
          });
        }
        this.loading.set(false);
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل بيانات الموظفين' });
        this.loading.set(false);
      }
    });
  }

  isEmployeePresent(employeeId: number): boolean {
    return this.attendanceState.get(employeeId) ?? true;
  }

  toggleAttendance(employee: any): void {
    const isPresent = this.isEmployeePresent(employee.id);
    
    if (isPresent) {
      // Trying to mark as Absent (requires confirmation because of salary deduction)
      this.confirmationService.confirm({
        message: `هل أنت متأكد من تسجيل الموظف (${employee.fullName}) غائباً اليوم؟ سيتم خصم راتب يوم من راتبه الأساسي آلياً.`,
        header: 'تأكيد تسجيل الغياب',
        icon: 'pi pi-exclamation-triangle',
        accept: () => {
          this.markAbsent(employee.id);
        }
      });
    } else {
      // Trying to mark as Present again - for simplicity, we tell them it can't be undone easily from here without DB modification, or we just allow visual toggle. 
      // To properly undo, we'd need an undo API. For now, show info message.
      this.messageService.add({ 
        severity: 'info', 
        summary: 'ملاحظة', 
        detail: 'لإلغاء الغياب المسجل، يرجى التوجه إلى شاشة تفاصيل الموظف ومراجعة الخصومات.' 
      });
    }
  }

  private markAbsent(employeeId: number): void {
    this.loading.set(true);
    this.attendanceService.markAbsent(employeeId).subscribe({
      next: () => {
        this.attendanceState.set(employeeId, false);
        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم تسجيل الغياب وخصم الراتب بنجاح' });
        this.loading.set(false);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تسجيل الغياب' });
        this.loading.set(false);
      }
    });
  }

  // --- Employee Form Logic ---

  openAddForm(): void {
    this.selectedEmployee = null;
    this.editMode = false;
    this.showForm = true;
  }

  openEditForm(employee: any): void {
    this.selectedEmployee = employee;
    this.editMode = true;
    this.showForm = true;
  }

  onFormSaved(): void {
    this.showForm = false;
    this.loadData();
  }

  // --- Monthly Attendance Calendar Logic ---

  openAttendanceDialog(employee: any): void {
    this.selectedEmployeeForAttendance = employee;
    this.showAttendanceDialog = true;
    this.loadCalendarData(employee.id);
  }

  loadCalendarData(employeeId: number): void {
    this.calendarLoading = true;
    const today = new Date();
    const year = today.getFullYear();
    const month = today.getMonth();
    
    // Generate days for the current month
    const daysInMonth = new Date(year, month + 1, 0).getDate();
    this.calendarDays = [];
    for (let i = 1; i <= daysInMonth; i++) {
      this.calendarDays.push({
        day: i,
        date: new Date(year, month, i),
        isAbsent: false // default to present
      });
    }

    // Fetch records for this month
    const firstDay = new Date(year, month, 1);
    const lastDay = new Date(year, month + 1, 0);

    this.attendanceService.getByEmployee(employeeId, firstDay, lastDay).subscribe({
      next: (records: any[]) => {
        // Mark absent days
        records.forEach(record => {
          if (record.attendanceStatus === 'Absent' || record.attendanceStatus === 2) { // 'Absent' or 2
            const recordDate = new Date(record.checkIn);
            const dayObj = this.calendarDays.find(d => d.day === recordDate.getDate());
            if (dayObj) {
              dayObj.isAbsent = true;
            }
          }
        });
        
        this.updateCalendarStats();
        this.calendarLoading = false;
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل سجلات الحضور' });
        this.calendarLoading = false;
      }
    });
  }

  updateCalendarStats(): void {
    this.absentDaysCount = this.calendarDays.filter(d => d.isAbsent).length;
    this.presentDaysCount = this.calendarDays.length - this.absentDaysCount;
  }

  isFutureDate(date: Date): boolean {
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return date.getTime() > today.getTime();
  }

  toggleDayAttendance(dayObj: any): void {
    const isCurrentlyAbsent = dayObj.isAbsent;
    
    if (!isCurrentlyAbsent) {
      // Trying to mark as Absent
      this.confirmationService.confirm({
        message: `هل أنت متأكد من تسجيل الموظف غائباً في يوم ${dayObj.day}؟ سيتم خصم راتب يوم من راتبه الأساسي آلياً.`,
        header: 'تأكيد تسجيل الغياب',
        icon: 'pi pi-exclamation-triangle',
        accept: () => {
          this.calendarLoading = true;
          this.attendanceService.markAbsent(this.selectedEmployeeForAttendance.id, dayObj.date).subscribe({
            next: () => {
              dayObj.isAbsent = true;
              this.updateCalendarStats();
              this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم تسجيل الغياب والخصم بنجاح' });
              this.calendarLoading = false;
              
              // Also update today's state if it was today
              if (dayObj.date.toDateString() === new Date().toDateString()) {
                this.attendanceState.set(this.selectedEmployeeForAttendance.id, false);
              }
            },
            error: () => {
              this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تسجيل الغياب' });
              this.calendarLoading = false;
            }
          });
        }
      });
    } else {
      // Trying to mark as Present (Undo absence)
      this.confirmationService.confirm({
        message: `هل أنت متأكد من تعديل حالة يوم ${dayObj.day} إلى حاضر؟ سيتم التراجع عن خصم الغياب آلياً.`,
        header: 'تأكيد تعديل الحالة',
        icon: 'pi pi-info-circle',
        accept: () => {
          this.calendarLoading = true;
          this.attendanceService.markPresent(this.selectedEmployeeForAttendance.id, dayObj.date).subscribe({
            next: () => {
              dayObj.isAbsent = false;
              this.updateCalendarStats();
              this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم تعديل الحالة إلى حاضر وإلغاء الخصم بنجاح' });
              this.calendarLoading = false;
              
              // Also update today's state if it was today
              if (dayObj.date.toDateString() === new Date().toDateString()) {
                this.attendanceState.set(this.selectedEmployeeForAttendance.id, true);
              }
            },
            error: () => {
              this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تعديل الحالة' });
              this.calendarLoading = false;
            }
          });
        }
      });
    }
  }

  // --- Salary Details Dialog Logic ---

  openSalaryDetails(employee: any): void {
    this.selectedEmployeeForDetails = employee;
    this.showSalaryDetailsDialog = true;
    this.detailsLoading = true;
    this.salaryDetails = null;

    const today = new Date();
    this.monthlySalaryService.getByEmployeeMonthYear(employee.id, today.getMonth() + 1, today.getFullYear()).subscribe({
      next: (res) => {
        if (res) {
          this.salaryDetails = res;
        } else {
          this.salaryDetails = {
            basicSalary: employee.basicSalary || 0,
            totalAllowances: 0,
            totalBonuses: 0,
            totalDeductions: 0,
            netSalary: employee.basicSalary || 0,
            deductions: []
          };
        }
        this.detailsLoading = false;
      },
      error: () => {
        // If not found, it means no deductions were made this month yet.
        this.salaryDetails = {
          basicSalary: employee.basicSalary,
          totalAllowances: 0,
          totalBonuses: 0,
          totalDeductions: 0,
          netSalary: employee.basicSalary,
          deductions: []
        };
        this.detailsLoading = false;
      }
    });
  }

  // --- Export Logic ---

  exportExcel(): void {
    const dashboard = this.dashboardData();
    if (!dashboard) return;
    
    this.exporting = true;
    const today = new Date();
    this.monthlySalaryService.getByBranchMonthYear(today.getMonth() + 1, today.getFullYear(), dashboard.branchId).subscribe({
      next: (salaries: any[]) => {
        this.exporting = false;
        
        // Map to Excel format
        const excelData = salaries.map((s, index) => ({
          '#': index + 1,
          'الكود': s.employeeCode,
          'الاسم': s.employeeName,
          'الراتب الأساسي': s.basicSalary,
          'إجمالي الخصومات': s.totalDeductions,
          'صافي الراتب': s.netSalary
        }));

        const ws: XLSX.WorkSheet = XLSX.utils.json_to_sheet(excelData);
        // RTL
        ws['!dir'] = 'rtl';
        const wb: XLSX.WorkBook = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(wb, ws, 'رواتب الموظفين');
        
        const fileName = `رواتب_شهر_${today.getMonth() + 1}_${today.getFullYear()}.xlsx`;
        XLSX.writeFile(wb, fileName);
      },
      error: () => {
        this.exporting = false;
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تصدير الرواتب' });
      }
    });
  }

  exportPdf(): void {
    const dashboard = this.dashboardData();
    if (!dashboard) return;
    
    this.exporting = true;
    const today = new Date();
    this.monthlySalaryService.getByBranchMonthYear(today.getMonth() + 1, today.getFullYear(), dashboard.branchId).subscribe({
      next: (salaries: any[]) => {
        this.exporting = false;
        this.printData = salaries;
        // Wait for Angular to render the table in DOM before printing
        setTimeout(() => {
          window.print();
        }, 300);
      },
      error: () => {
        this.exporting = false;
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تجهيز الطباعة' });
      }
    });
  }
}
