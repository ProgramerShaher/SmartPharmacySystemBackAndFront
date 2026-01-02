import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { MedicineService } from '../../services/medicine.service';
import { MedicineDto, MedicineBatchResponseDto } from '../../../../core/models';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { DividerModule } from 'primeng/divider';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { TableModule } from 'primeng/table';
import { ChartModule } from 'primeng/chart';
import { MenuModule } from 'primeng/menu';
import { MenuItem } from 'primeng/api';

@Component({
    selector: 'app-medicine-details',
    standalone: true,
    imports: [
        CommonModule,
        RouterModule,
        ButtonModule,
        CardModule,
        TagModule,
        DividerModule,
        ProgressSpinnerModule,
        TooltipModule,
        TableModule,
        ChartModule,
        MenuModule
    ],
    templateUrl: './medicine-details.component.html',
    styleUrls: ['./medicine-details.component.scss']
})
export class MedicineDetailsComponent implements OnInit {
    medicine: MedicineDto | null = null;
    batches: MedicineBatchResponseDto[] = [];
    loading = true;

    // Quick Actions Menu
    actions: MenuItem[] = [];

    // Chart Data
    chartData: any;
    chartOptions: any;

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private medicineService: MedicineService
    ) { }

    ngOnInit() {
        this.loadData();
        this.setupActions();
    }

    private loadData(): void {
        const id = Number(this.route.snapshot.paramMap.get('id'));

        if (!id || isNaN(id)) {
            this.goBack();
            return;
        }

        this.loading = true;

        // Fetch Medicine and Batches in parallel
        this.medicineService.getById(id).subscribe({
            next: (data) => {
                this.medicine = data;
                this.checkLoadComplete();
            },
            error: (err) => {
                console.error('Error loading medicine:', err);
                this.loading = false;
            }
        });

        this.medicineService.getFEFOBatches(id).subscribe({
            next: (data) => {
                this.batches = data;
                this.checkLoadComplete();
            },
            error: (err) => {
                console.error('Error loading batches:', err);
            }
        });
    }

    private checkLoadComplete() {
        if (this.medicine) {
            this.loading = false;
            this.setupChart();
        }
    }

    setupActions() {
        this.actions = [
            { label: 'تعديل البيانات', icon: 'pi pi-pencil', command: () => this.editMedicine() },
            { label: 'طباعة باركود', icon: 'pi pi-print' },
            { separator: true },
            { label: 'حذف الدواء', icon: 'pi pi-trash', styleClass: 'text-red-500' }
        ];
    }

    setupChart() {
        // Simple stock distribution donut (Batches quantity)
        if (!this.batches.length) return;

        this.chartData = {
            labels: this.batches.map(b => b.companyBatchNumber),
            datasets: [
                {
                    data: this.batches.map(b => b.remainingQuantity),
                    backgroundColor: [
                        '#14b8a6', '#0f766e', '#2dd4bf', '#99f6e4', '#ccfbf1'
                    ]
                }
            ]
        };

        this.chartOptions = {
            plugins: {
                legend: { display: false }
            },
            cutout: '70%'
        };
    }

    goBack(): void {
        this.router.navigate(['/inventory/medicines']);
    }

    editMedicine() {
        // Here we could open the sidebar or navigate to edit page
        // For now just log
        console.log('Edit clicked');
    }

    getStatusSeverity(status: string): 'success' | 'danger' {
        return status === 'Active' ? 'success' : 'danger';
    }

    getBatchStatusSeverity(batch: MedicineBatchResponseDto): 'success' | 'danger' | 'warning' {
        if (batch.isExpired) return 'danger';
        if (batch.isExpiringSoon) return 'warning';
        return 'success';
    }

    getExpiredCount(): number {
        return this.batches.filter(b => b.isExpired).length;
    }

    getExpiringSoonCount(): number {
        return this.batches.filter(b => b.isExpiringSoon).length;
    }

    getCurrentStock(): number {
        return this.medicine?.totalQuantity || 0;
    }
}
