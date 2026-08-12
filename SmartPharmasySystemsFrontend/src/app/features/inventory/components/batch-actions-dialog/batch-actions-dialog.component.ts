import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { MedicineBatch } from '../../../../core/models';

@Component({
    selector: 'app-batch-actions-dialog',
    standalone: true,
    imports: [CommonModule, DialogModule, ButtonModule],
    template: `
        <p-dialog [(visible)]="visible" [modal]="true" header="إجراءات الدفعة" (onHide)="onClose.emit()">
            <div class="p-4 flex flex-column gap-3">
                <p>إجراءات الدفعة قيد التطوير...</p>
                <div class="flex justify-content-end gap-2">
                    <button pButton label="إغلاق" class="p-button-secondary" (click)="visible = false; onClose.emit()"></button>
                </div>
            </div>
        </p-dialog>
    `
})
export class BatchActionsDialogComponent {
    @Input() visible = false;
    @Output() visibleChange = new EventEmitter<boolean>();
    @Input() batch!: MedicineBatch;
    @Output() onClose = new EventEmitter<void>();
    @Output() onSuccess = new EventEmitter<void>();
}
