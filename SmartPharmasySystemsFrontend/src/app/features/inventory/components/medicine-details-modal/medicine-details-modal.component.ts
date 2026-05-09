import { Component, input, model, signal, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { TableModule } from 'primeng/table';
import { TagModule } from 'primeng/tag';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { TooltipModule } from 'primeng/tooltip';
import { MedicineService } from '../../services/medicine.service';
import { MedicineDetailsDto } from '../../../../core/models/medicine.interface';

@Component({
  selector: 'app-medicine-details-modal',
  standalone: true,
  imports: [
    CommonModule, 
    DialogModule, 
    ButtonModule, 
    TableModule, 
    TagModule, 
    ProgressSpinnerModule, 
    TooltipModule
  ],
  templateUrl: './medicine-details-modal.component.html',
  styleUrl: './medicine-details-modal.component.scss'
})
export class MedicineDetailsModalComponent {
  visible = model.required<boolean>();
  medicineId = input<number | null>(null);

  medicineDetails = signal<MedicineDetailsDto | null>(null);
  loading = signal(false);
  errorOccurred = signal(false);
  errorMessage = signal('');

  constructor(private medicineService: MedicineService) {
    effect(() => {
      if (this.visible() && this.medicineId()) {
        this.loadDetails();
      } else if (!this.visible()) {
        this.medicineDetails.set(null);
      }
    }, { allowSignalWrites: true });
  }

  loadDetails() {
    const id = this.medicineId();
    if (!id) return;
    
    this.loading.set(true);
    this.errorOccurred.set(false);
    this.errorMessage.set('');

    this.medicineService.getDetails(id).subscribe({
      next: (details) => {
        this.medicineDetails.set(details);
        this.loading.set(false);
      },
      error: (err) => {
        console.error('Error loading details:', err);
        this.loading.set(false);
        this.errorOccurred.set(true);
        this.errorMessage.set(err.error?.message || 'تعذر الاتصال بالخادم. يرجى التأكد من تشغيل الـ Backend.');
      }
    });
  }

  getStockSeverity(): 'success' | 'warning' | 'danger' | 'info' {
    const details = this.medicineDetails();
    if (!details) return 'info';
    
    const stock = details.totalStock || 0;
    if (stock <= 0) return 'danger';
    if (stock <= details.reorderLevel) return 'warning';
    return 'success';
  }

  isLowStock(): boolean {
    const details = this.medicineDetails();
    return !!details && (details.totalStock || 0) <= details.reorderLevel;
  }

  close() {
    this.visible.set(false);
  }
}
