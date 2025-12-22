import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';

@Component({
    selector: 'app-confirmation-dialog',
    standalone: true,
    imports: [CommonModule, DialogModule, ButtonModule],
    template: `
        <p-dialog [(visible)]="visible" [header]="header" [modal]="true" 
            [style]="{width: '400px'}" [closable]="false" [draggable]="false" [resizable]="false">
            <div class="flex flex-column align-items-center p-4 text-center">
                <i [class]="icon" [style.color]="iconColor" class="text-6xl mb-4"></i>
                <p class="text-xl font-bold m-0 mb-2">{{message}}</p>
                <p class="text-secondary m-0">{{subMessage}}</p>
            </div>
            <ng-template pTemplate="footer">
                <div class="flex justify-content-center gap-2 pb-3">
                    <p-button label="تراجع" icon="pi pi-times" severity="secondary" text (onClick)="onReject()"></p-button>
                    <p-button [label]="confirmLabel" [icon]="confirmIcon" [severity]="severity" 
                        class="shadow-2" (onClick)="onAccept()"></p-button>
                </div>
            </ng-template>
        </p-dialog>
    `
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
