import { Component, OnInit } from '@angular/core';
import { ShiftService } from '../../../../core/services/shift.service';
import { ShiftDto, OpenShiftDto, CloseShiftDto } from '../../../../core/models';
import { MessageService, ConfirmationService } from 'primeng/api';

@Component({
  selector: 'app-shift-list',
  templateUrl: './shift-list.component.html',
  styleUrls: ['./shift-list.component.scss'],
  providers: [MessageService, ConfirmationService]
})
export class ShiftListComponent implements OnInit {
  shifts: ShiftDto[] = [];
  loading: boolean = false;

  // Modals
  displayOpenModal: boolean = false;
  displayCloseModal: boolean = false;
  displayDetailsModal: boolean = false;

  selectedShift: ShiftDto | null = null;

  openData: OpenShiftDto = { openingCash: 0 };
  closeData: CloseShiftDto = { actualClosingCash: 0 };

  constructor(
    private shiftService: ShiftService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) { }

  ngOnInit(): void {
    this.loadShifts();
  }

  loadShifts(): void {
    this.loading = true;
    this.shiftService.getAllShifts().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.shifts = res.data;
        }
        this.loading = false;
      },
      error: (err) => {
        this.loading = false;
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'حدث خطأ أثناء تحميل الورديات' });
      }
    });
  }

  getShiftStatusLabel(status: string): string {
    switch (status) {
      case 'Open': return 'مفتوحة';
      case 'Closed': return 'مغلقة';
      default: return status;
    }
  }

  getShiftStatusSeverity(status: string): 'success' | 'info' | 'warning' | 'danger' | 'secondary' | 'contrast' {
    switch (status) {
      case 'Open': return 'success';
      case 'Closed': return 'secondary';
      default: return 'info';
    }
  }

  openNewShift(): void {
    this.openData = { openingCash: 0 };
    this.displayOpenModal = true;
  }

  confirmOpenShift(): void {
    this.shiftService.openShift(this.openData).subscribe({
      next: (res) => {
        if (res.success) {
          this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم فتح الوردية بنجاح' });
          this.displayOpenModal = false;
          this.loadShifts();
        } else {
          this.messageService.add({ severity: 'error', summary: 'خطأ', detail: res.message });
        }
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'حدث خطأ أثناء فتح الوردية' });
      }
    });
  }

  closeCurrentShift(shift: ShiftDto): void {
    this.selectedShift = shift;
    this.closeData = { actualClosingCash: shift.expectedClosingCash || 0, transferToMainSafe: false };
    this.displayCloseModal = true;
  }

  confirmCloseShift(): void {
    this.shiftService.closeShift(this.closeData).subscribe({
      next: (res) => {
        if (res.success) {
          this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم إغلاق الوردية بنجاح' });
          this.displayCloseModal = false;
          this.loadShifts();
        } else {
          this.messageService.add({ severity: 'error', summary: 'خطأ', detail: res.message });
        }
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'حدث خطأ أثناء إغلاق الوردية' });
      }
    });
  }

  viewDetails(shift: ShiftDto): void {
    this.selectedShift = shift;
    this.displayDetailsModal = true;
  }
}
