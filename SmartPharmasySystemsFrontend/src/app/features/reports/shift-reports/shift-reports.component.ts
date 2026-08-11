import { Component, OnInit } from '@angular/core';
import { CommonModule, DecimalPipe, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ShiftService } from '../../../core/services/shift.service';
import { ShiftDto, ShiftDetailsDto } from '../../../core/models';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { InputTextModule } from 'primeng/inputtext';
import { DialogModule } from 'primeng/dialog';
import { CardModule } from 'primeng/card';

@Component({
  selector: 'app-shift-reports',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ToastModule,
    DecimalPipe,
    DatePipe,
    ButtonModule,
    ProgressSpinnerModule,
    InputTextModule,
    DialogModule,
    CardModule
  ],
  providers: [MessageService],
  templateUrl: './shift-reports.component.html',
  styleUrls: ['./shift-reports.component.scss']
})
export class ShiftReportsComponent implements OnInit {
  shifts: ShiftDto[] = [];
  filteredShifts: ShiftDto[] = [];
  isLoading = true;
  searchTerm = '';

  showDetailsDialog = false;
  loadingDetails = false;
  shiftDetails: ShiftDetailsDto | null = null;

  constructor(
    private shiftService: ShiftService,
    private messageService: MessageService
  ) { }

  ngOnInit() {
    this.loadShifts();
  }

  loadShifts() {
    this.isLoading = true;
    this.shiftService.getAllShifts().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.shifts = res.data;
          this.filteredShifts = [...this.shifts];
        } else {
          this.showError('تعذر جلب بيانات الورديات');
        }
        this.isLoading = false;
      },
      error: () => {
        this.showError('حدث خطأ في الاتصال بالخادم');
        this.isLoading = false;
      }
    });
  }

  filterShifts() {
    if (!this.searchTerm) {
      this.filteredShifts = [...this.shifts];
      return;
    }

    const term = this.searchTerm.toLowerCase();
    this.filteredShifts = this.shifts.filter(s =>
      (s.userName && s.userName.toLowerCase().includes(term)) ||
      (s.status && s.status.toLowerCase().includes(term))
    );
  }

  getStatusClass(status: string): string {
    return status === 'Open' ? 'badge-success' : 'badge-secondary';
  }

  getStatusLabel(status: string): string {
    return status === 'Open' ? 'مفتوحة' : 'مغلقة';
  }

  showError(msg: string) {
    this.messageService.add({ severity: 'error', summary: 'خطأ', detail: msg });
  }

  refresh() {
    this.loadShifts();
  }

  viewDetails(id: number) {
    this.showDetailsDialog = true;
    this.loadingDetails = true;
    this.shiftService.getDetails(id).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.shiftDetails = res.data;
        } else {
          this.showError('تعذر جلب تفاصيل الوردية');
          this.showDetailsDialog = false;
        }
        this.loadingDetails = false;
      },
      error: () => {
        this.showError('حدث خطأ أثناء جلب التفاصيل');
        this.loadingDetails = false;
        this.showDetailsDialog = false;
      }
    });
  }
}
