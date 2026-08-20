import { Component, EventEmitter, Input, OnInit, Output, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { MessagesModule } from 'primeng/messages';
import { MessageService } from 'primeng/api';
import { CheckboxModule } from 'primeng/checkbox';
import { ShiftService } from '../../../core/services/shift.service';
import { ShiftDto } from '../../../core/models';

@Component({
  selector: 'app-shift-modal',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    InputTextareaModule,
    MessagesModule,
    CheckboxModule
  ],
  templateUrl: './shift-modal.component.html',
  styleUrls: ['./shift-modal.component.scss']
})
export class ShiftModalComponent implements OnInit, OnDestroy {
  @Input() visible = false;
  @Input() mode: 'open' | 'close' = 'open';
  @Output() shiftProcessed = new EventEmitter<any>();
  @Output() visibleChange = new EventEmitter<boolean>();

  shiftForm: FormGroup;
  loading = false;
  currentShift: ShiftDto | null = null;
  loadingShift = false;

  constructor(
    private fb: FormBuilder,
    private shiftService: ShiftService,
    private messageService: MessageService
  ) {
    this.shiftForm = this.fb.group({
      cashAmount: [null, [Validators.required, Validators.min(0)]],
      notes: [''],
      transferToMainSafe: [true] // Default to transfer
    });
  }

  ngOnInit(): void {
    if (this.visible && this.mode === 'close') {
      this.loadCurrentShift();
    }
  }

  ngOnChanges(changes: any) {
    if (changes.visible && changes.visible.currentValue && this.mode === 'close') {
      if (this.shiftService.transferIntent !== null) {
        this.shiftForm.patchValue({ transferToMainSafe: this.shiftService.transferIntent });
        this.shiftService.transferIntent = null; // reset
      }
      this.loadCurrentShift();
    }
    if (changes.mode && changes.mode.currentValue === 'open') {
      this.shiftForm.reset();
    }
  }

  ngOnDestroy(): void { }

  onHide() {
    this.visibleChange.emit(false);
  }

  loadCurrentShift() {
    this.loadingShift = true;
    this.shiftService.getCurrentShift().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.currentShift = res.data;
          this.shiftForm.patchValue({
            cashAmount: res.data.expectedClosingCash || 0
          });
        }
        this.loadingShift = false;
      },
      error: () => {
        this.loadingShift = false;
      }
    });
  }

  submit(): void {
    if (this.shiftForm.invalid) return;

    this.loading = true;
    const amount = this.shiftForm.value.cashAmount;
    const notes = this.shiftForm.value.notes;

    if (this.mode === 'open') {
      this.shiftService.openShift({ openingCash: amount, notes: notes }).subscribe({
        next: (res) => {
          this.loading = false;
          if (res.success) {
            this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم فتح الوردية بنجاح' });
            this.shiftProcessed.emit(res.data);
            this.visible = false;
            this.visibleChange.emit(false);
          } else {
            this.messageService.add({ severity: 'error', summary: 'خطأ', detail: res.message || 'حدث خطأ أثناء فتح الوردية' });
          }
        },
        error: (err) => {
          this.loading = false;
          this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'حدث خطأ غير متوقع' });
        }
      });
    } else {
      const transferToMainSafe = this.shiftForm.value.transferToMainSafe;
      this.shiftService.closeShift({ actualClosingCash: amount, notes: notes, transferToMainSafe: transferToMainSafe }).subscribe({
        next: (res) => {
          this.loading = false;
          if (res.success) {
            let diffMsg = 'الرصيد مطابق تماماً';
            if (res.data.difference > 0) diffMsg = `يوجد زيادة في الدرج بمقدار ${res.data.difference}`;
            else if (res.data.difference < 0) diffMsg = `يوجد عجز في الدرج بمقدار ${Math.abs(res.data.difference)}`;

            this.messageService.add({ severity: 'success', summary: 'تم إغلاق الوردية', detail: diffMsg });
            this.shiftProcessed.emit(res.data);
            this.visible = false;
            this.visibleChange.emit(false);
          } else {
            this.messageService.add({ severity: 'error', summary: 'خطأ', detail: res.message || 'حدث خطأ أثناء إغلاق الوردية' });
          }
        },
        error: (err) => {
          this.loading = false;
          this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'حدث خطأ غير متوقع' });
        }
      });
    }
  }

  get difference(): number {
    if (!this.currentShift) return 0;
    const actual = this.shiftForm.value.cashAmount || 0;
    return actual - (this.currentShift.expectedClosingCash || 0);
  }
}
