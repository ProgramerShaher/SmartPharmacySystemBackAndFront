import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { InventoryMovement } from '../../../../core/models';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';

@Component({
    selector: 'app-movement-details',
    standalone: true,
    imports: [CommonModule, ButtonModule, TagModule],
    templateUrl: './movement-details.component.html',
    styleUrl: './movement-details.component.scss'
})
export class MovementDetailsComponent {
    @Input() movement: InventoryMovement | null = null;
    @Output() close = new EventEmitter<void>();
    today = new Date();

    getMovementTypeSeverity(type: string | undefined): 'success' | 'info' | 'warning' | 'danger' | 'secondary' {
        switch (type) {
            case 'IN': return 'success';
            case 'OUT': return 'info';
            case 'RETURN': return 'warning';
            case 'DAMAGE': return 'danger';
            case 'ADJUSTMENT': return 'secondary';
            default: return 'secondary';
        }
    }

    getMovementTypeLabel(type: string | undefined): string {
        switch (type) {
            case 'IN': return 'دخول (توريد)';
            case 'OUT': return 'خروج (صرف)';
            case 'RETURN': return 'مرتجع';
            case 'DAMAGE': return 'تالف';
            case 'ADJUSTMENT': return 'تعديل';
            default: return type || '';
        }
    }

    onClose() {
        this.close.emit();
    }

    isExpired(date: Date | string | undefined): boolean {
        if (!date) return false;
        return new Date(date) < this.today;
    }
}
