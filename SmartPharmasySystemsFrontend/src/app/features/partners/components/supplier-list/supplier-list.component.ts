import { Component, OnInit, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { SupplierService } from '../../services/supplier.service';
import { Supplier } from '../../../../core/models/supplier.models';
import { MessageService, ConfirmationService } from 'primeng/api';

// PrimeNG
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { InputSwitchModule } from 'primeng/inputswitch';
import { ProgressBarModule } from 'primeng/progressbar';

@Component({
  selector: 'app-supplier-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    TagModule,
    TooltipModule,
    ConfirmDialogModule,
    ToastModule,
    InputSwitchModule,
    ProgressBarModule
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './supplier-list.component.html',
  styleUrls: ['./supplier-list.component.scss']
})
export class SupplierListComponent implements OnInit {
  suppliers = signal<Supplier[]>([]);
  loading = signal(false);

  // Calculated stats
  totalDebt = computed(() => this.suppliers().reduce((sum, s) => sum + (s.balance || 0), 0));
  highDebtCount = computed(() => this.suppliers().filter(s => (s.balance || 0) > 10000).length);

  constructor(
    private supplierService: SupplierService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) { }

  ngOnInit() {
    this.loadSuppliers();
  }

  loadSuppliers() {
    this.loading.set(true);
    this.supplierService.getAll({ pageSize: 1000 }).subscribe({
      next: (res) => {
        this.suppliers.set(res.items);
        this.loading.set(false);
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل قائمة الموردين' });
        this.loading.set(false);
      }
    });
  }

  deleteSupplier(event: Event, supplier: Supplier) {
    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: `هل أنت متأكد من حذف المورد ${supplier.name}؟`,
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'نعم، حذف',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-danger p-button-text',
      rejectButtonStyleClass: 'p-button-text',
      accept: () => {
        this.supplierService.delete(supplier.id).subscribe({
          next: () => {
                this.messageService.add({ severity: 'success', summary: 'تم الحذف', detail: 'تم حذف المورد بنجاح' });
                this.loadSuppliers();
              },
              error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في الحذف' })
            });
      }
    });
  }

  getSeverity(balance: number): 'danger' | 'warning' | 'success' | undefined {
    if (balance > 10000) return 'danger';
    if (balance > 0) return 'warning';
    return 'success';
  }

  getStatusLabel(balance: number): string {
    if (balance > 10000) return 'مديونية عالية';
    if (balance > 0) return 'مدين';
    return 'خالص';
  }

  getRandomColor(name: string): string {
    const colors = [
      'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)',
      'linear-gradient(135deg, #4facfe 0%, #00f2fe 100%)',
      'linear-gradient(135deg, #43e97b 0%, #38f9d7 100%)',
      'linear-gradient(135deg, #fa709a 0%, #fee140 100%)',
      'linear-gradient(135deg, #30cfd0 0%, #330867 100%)',
      'linear-gradient(135deg, #a8edea 0%, #fed6e3 100%)',
      'linear-gradient(135deg, #ff9a9e 0%, #fecfef 100%)',
      'linear-gradient(135deg, #ffecd2 0%, #fcb69f 100%)',
      'linear-gradient(135deg, #ff6e7f 0%, #bfe9ff 100%)'
    ];
    const index = name.charCodeAt(0) % colors.length;
    return colors[index];
  }
}
