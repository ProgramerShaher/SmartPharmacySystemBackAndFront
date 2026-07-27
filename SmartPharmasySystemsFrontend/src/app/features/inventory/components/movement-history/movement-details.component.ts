import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { InventoryMovement } from '../../../../core/models';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { InventoryMovementService } from '../../services/inventory-movement.service';
import { ProgressSpinnerModule } from 'primeng/progressspinner';

@Component({
    selector: 'app-movement-details',
    standalone: true,
    imports: [CommonModule, ButtonModule, TagModule, ProgressSpinnerModule],
    templateUrl: './movement-details.component.html',
    styleUrl: './movement-details.component.scss'
})
export class MovementDetailsComponent implements OnInit {
    @Input() movement: InventoryMovement | null = null;
    @Output() close = new EventEmitter<void>();
    today = new Date();
    loading = false;
    isRouteMode = false;

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private movementService: InventoryMovementService
    ) {}

    ngOnInit(): void {
        const id = this.route.snapshot.paramMap.get('id');
        if (id) {
            this.isRouteMode = true;
            this.loadMovement(+id);
        }
    }

    loadMovement(id: number) {
        this.loading = true;
        this.movementService.getById(id).subscribe({
            next: (data: any) => {
                this.movement = data;
                this.loading = false;
            },
            error: () => {
                this.loading = false;
            }
        });
    }

    getMovementTypeSeverity(type: any): 'success' | 'info' | 'warning' | 'danger' | 'secondary' {
        const typeStr = type?.toString().toUpperCase();
        switch (typeStr) {
            case 'IN': return 'success';
            case 'OUT': return 'info';
            case 'RETURN': return 'warning';
            case 'DAMAGE': return 'danger';
            case 'ADJUSTMENT': return 'secondary';
            default: return 'secondary';
        }
    }

    getMovementTypeLabel(type: any): string {
        const typeStr = type?.toString().toUpperCase();
        switch (typeStr) {
            case 'IN': return 'دخول (توريد)';
            case 'OUT': return 'خروج (صرف)';
            case 'RETURN': return 'مرتجع';
            case 'DAMAGE': return 'تالف';
            case 'ADJUSTMENT': return 'تعديل';
            default: return type?.toString() || '';
        }
    }

    onClose() {
        if (this.isRouteMode) {
            this.router.navigate(['/inventory/movements']);
        } else {
            this.close.emit();
        }
    }

    isExpired(date: Date | string | undefined): boolean {
        if (!date) return false;
        return new Date(date) < this.today;
    }
}
