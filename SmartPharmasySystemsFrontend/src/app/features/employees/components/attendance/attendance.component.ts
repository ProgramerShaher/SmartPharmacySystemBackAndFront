import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TooltipModule } from 'primeng/tooltip';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { TagModule } from 'primeng/tag';
import { AttendanceService } from '../../services/attendance.service';
import { EmployeeService } from '../../services/employee.service';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ShiftType, AttendanceStatus } from '../../../../core/models';

@Component({
  selector: 'app-attendance',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    TableModule,
    TooltipModule,
    ToastModule,
    ConfirmDialogModule,
    DropdownModule,
    CalendarModule,
    TagModule,
    ProgressSpinnerModule
  ],
  templateUrl: './attendance.component.html',
  styleUrls: ['./attendance.component.scss'],
  providers: [MessageService, ConfirmationService]
})
export class AttendanceComponent implements OnInit {
  records = signal<any[]>([]);
  loading = signal<boolean>(true);
  
  filterDate = new Date();
  branches: any[] = [];
  selectedBranch: any = null;

  stats = signal({ total: 0, present: 0, absent: 0 });

  ShiftTypeEnum = ShiftType;
  AttendanceStatusEnum = AttendanceStatus;

  constructor(
    private attendanceService: AttendanceService,
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
    
    // Fetch branch dashboard (to get all employees) and today's attendances in parallel or sequence
    this.employeeService.getBranchDashboard(this.selectedBranch).subscribe({
      next: (dash: any) => {
        let emps: any[] = [];
        dash.departments.forEach((d: any) => {
            if (d.employees) {
                emps = emps.concat(d.employees);
            }
        });
        
        // Fetch actual attendances for today
        this.attendanceService.getByBranch(this.selectedBranch, this.filterDate, this.filterDate).subscribe({
          next: (attendances) => {
             const selectedDay = this.startOfDay(this.filterDate);
             const mapped = emps
              .filter(emp => this.isEmployeeActiveOnDate(emp, selectedDay))
              .map(emp => {
                const record = attendances.find((a: any) => a.employeeId === emp.id);
                return {
                    employeeId: emp.id,
                    employeeCode: emp.employeeCode,
                    fullName: emp.fullName,
                    departmentName: emp.departmentName,
                    shift: emp.shift || ShiftType.Morning,
                    attendanceStatus: record ? record.attendanceStatus : AttendanceStatus.Absent, // default absent if no record
                    checkIn: record ? record.checkIn : null,
                    checkOut: record ? record.checkOut : null,
                    workedHours: record ? record.workedHours : null,
                    id: record ? record.id : null // actual DB ID for attendance
                };
             });
             this.records.set(mapped);
             this.updateStats();
             this.loading.set(false);
          },
          error: () => {
             this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل سجل الحضور' });
             this.loading.set(false);
          }
        });
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل بيانات الموظفين' });
        this.loading.set(false);
      }
    });
  }

  updateStats(): void {
    const list = this.records();
    this.stats.set({
      total: list.length,
      present: list.filter(r => r.attendanceStatus === AttendanceStatus.Present).length,
      absent: list.filter(r => r.attendanceStatus === AttendanceStatus.Absent).length
    });
  }

  markPresent(emp: any): void {
    this.loading.set(true);
    this.attendanceService.markPresent(emp.employeeId, this.filterDate).subscribe({
      next: (res) => {
        emp.attendanceStatus = AttendanceStatus.Present;
        emp.id = res.id;
        emp.checkIn = res.checkIn;
        this.updateStats();
        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم تسجيل الحضور' });
        this.loading.set(false);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل العملية' });
        this.loading.set(false);
      }
    });
  }

  markAbsent(emp: any): void {
    this.confirmationService.confirm({
      message: 'هل أنت متأكد من تسجيل غياب هذا الموظف؟',
      accept: () => {
        this.loading.set(true);
        this.attendanceService.markAbsent(emp.employeeId, this.filterDate).subscribe({
          next: (res) => {
            emp.attendanceStatus = AttendanceStatus.Absent;
            emp.id = res.id;
            this.updateStats();
            this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم تسجيل الغياب' });
            this.loading.set(false);
          },
          error: () => {
            this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل العملية' });
            this.loading.set(false);
          }
        });
      }
    });
  }

  markCheckout(emp: any): void {
    if (!emp.id) return;
    this.loading.set(true);
    this.attendanceService.checkOut(emp.id, new Date()).subscribe({
      next: (res) => {
        emp.checkOut = res.checkOut;
        emp.workedHours = res.workedHours;
        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم تسجيل الانصراف' });
        this.loading.set(false);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل العملية' });
        this.loading.set(false);
      }
    });
  }

  getShiftName(shift: ShiftType): string {
    switch (shift) {
      case ShiftType.Morning: return 'صباحي';
      case ShiftType.Evening: return 'مسائي';
      case ShiftType.Night: return 'ليلي';
      default: return 'غير محدد';
    }
  }

  getStatusName(status: AttendanceStatus): string {
    switch (status) {
      case AttendanceStatus.Present: return 'حاضر';
      case AttendanceStatus.Absent: return 'غائب';
      case AttendanceStatus.Late: return 'متأخر';
      case AttendanceStatus.OnLeave: return 'إجازة';
      default: return 'غير محدد';
    }
  }

  getStatusSeverity(status: AttendanceStatus): 'success' | 'danger' | 'warning' | 'info' {
    switch (status) {
      case AttendanceStatus.Present: return 'success';
      case AttendanceStatus.Absent: return 'danger';
      case AttendanceStatus.Late: return 'warning';
      case AttendanceStatus.OnLeave: return 'info';
      default: return 'info';
    }
  }

  private isEmployeeActiveOnDate(emp: any, selectedDay: Date): boolean {
    if (!emp.hireDate) return true;

    const hireDate = this.startOfDay(new Date(emp.hireDate));
    if (selectedDay < hireDate) return false;

    if (emp.terminationDate) {
      const terminationDate = this.startOfDay(new Date(emp.terminationDate));
      if (selectedDay > terminationDate) return false;
    }

    return true;
  }

  private startOfDay(date: Date): Date {
    return new Date(date.getFullYear(), date.getMonth(), date.getDate());
  }
}
