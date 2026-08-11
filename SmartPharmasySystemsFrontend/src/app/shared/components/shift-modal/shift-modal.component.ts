import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { MessageService } from 'primeng/api';
import { ShiftService } from '../../../core/services/shift.service';

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
    InputTextareaModule
  ],
  templateUrl: './shift-modal.component.html',
  styleUrls: ['./shift-modal.component.scss']
})
export class ShiftModalComponent implements OnInit {
  @Input() visible = false;
  @Input() mode: 'open' | 'close' = 'open';
  @Output() shiftProcessed = new EventEmitter<any>();

  shiftForm: FormGroup;
  loading = false;

  constructor(
    private fb: FormBuilder,
    private shiftService: ShiftService,
    private messageService: MessageService
  ) {
    this.shiftForm = this.fb.group({
      cashAmount: [null, [Validators.required, Validators.min(0)]],
      notes: ['']
    });
  }

  ngOnInit(): void {
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
            this.messageService.add({ severity: 'success', summary: 'تم بنجاح', detail: 'تم فتح الوردية بنجاح' });
            this.shiftProcessed.emit(res.data);
            this.visible = false;
          } else {
            this.messageService.add({ severity: 'error', summary: 'خطأ', detail: res.message || 'حدث خطأ أثناء فتح الوردية' });
          }
        },
        error: (err) => {
          this.loading = false;
          this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'حدث خطأ غير متوقع' });
        }
      });
    } else {
      this.shiftService.closeShift({ actualClosingCash: amount, notes: notes }).subscribe({
        next: (res) => {
          this.loading = false;
          if (res.success) {
            let diffMsg = 'الرصيد مطابق';
            if (res.data.difference > 0) diffMsg = `يوجد زيادة بمقدار ${res.data.difference}`;
            else if (res.data.difference < 0) diffMsg = `يوجد عجز بمقدار ${Math.abs(res.data.difference)}`;

            this.messageService.add({ severity: 'success', summary: 'تم إغلاق الوردية', detail: diffMsg });
            this.shiftProcessed.emit(res.data);
            this.visible = false;
          } else {
            this.messageService.add({ severity: 'error', summary: 'خطأ', detail: res.message || 'حدث خطأ أثناء إغلاق الوردية' });
          }
        },
        error: (err) => {
          this.loading = false;
          this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'حدث خطأ غير متوقع' });
        }
      });
    }
  }
}
