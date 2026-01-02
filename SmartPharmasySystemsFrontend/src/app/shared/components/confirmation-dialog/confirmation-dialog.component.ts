import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';

@Component({
    selector: 'app-confirmation-dialog',
    standalone: true,
    imports: [CommonModule, DialogModule, ButtonModule],
    templateUrl: './confirmation-dialog.component.html',
    styleUrls: ['./confirmation-dialog.component.scss']
})
export class ConfirmationDialogComponent {
    @Input() header = 'تأقيق العملية';
    @Input() message = 'هل أنت متأكد؟';
    @Input() subMessage = '';
    @Input() icon = 'pi pi-exclamation-triangle';
    @Input() iconColor = 'var(--primary-color)';
    @Input() confirmLabel = 'تأكيد';
    @Input() confirmIcon = 'pi pi-check';
    @Input() severity: 'success' | 'danger' | 'info' | 'warning' = 'primary' as any;

    @Output() accept = new EventEmitter<void>();
    @Output() reject = new EventEmitter<void>();

    visible = false;

    show() {
        this.visible = true;
    }

    onAccept() {
        this.accept.emit();
        this.visible = false;
    }

    onReject() {
        this.reject.emit();
        this.visible = false;
    }
}
