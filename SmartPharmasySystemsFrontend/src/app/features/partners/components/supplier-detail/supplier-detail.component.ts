import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { ChipModule } from 'primeng/chip';
import { DividerModule } from 'primeng/divider';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { TableModule } from 'primeng/table';
import { TabViewModule } from 'primeng/tabview';
import { ConfirmationService, MessageService } from 'primeng/api';
import { SupplierService } from '../../services/supplier.service';
import { Supplier } from '../../../../core/models/supplier.models';
import { DocumentStatus } from '../../../../core/models/stock-movement.enums';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-supplier-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ButtonModule,
    CardModule,
    TagModule,
    ChipModule,
    DividerModule,
    ConfirmDialogModule,
    ToastModule,
    TooltipModule,
    TableModule,
    TabViewModule,
  ],
  templateUrl: './supplier-detail.component.html',
  styleUrl: './supplier-detail.component.scss',
  providers: [MessageService, ConfirmationService],
})
export class SupplierDetailComponent implements OnInit {
  supplier?: Supplier;
  loading: boolean = true;
  showInvoices: boolean = false;
  showReturns: boolean = false;

  constructor(
    private supplierService: SupplierService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit(): void {
    console.log('🚀 Supplier Detail Component Initialized');
    this.route.params.subscribe((params) => {
      if (params['id']) {
        const supplierId = +params['id'];
        console.log(`📋 Loading details for supplier ID: ${supplierId}`);
        this.loadSupplier(supplierId);
      }
    });
  }

  loadSupplier(id: number): void {
    this.loading = true;
    console.log(`⏳ Fetching supplier details for ID: ${id}`);

    this.supplierService
      .getById(id)
      .pipe(
        finalize(() => {
          this.loading = false;
          console.log(`✅ Finished loading supplier details for ID: ${id}`);
        })
      )
      .subscribe({
        next: (supplier) => {
          this.supplier = supplier;
          console.log(`📊 Supplier details loaded:`, supplier);
          console.log(
            `📋 Purchase Invoices: ${supplier.purchaseInvoices?.length || 0}`
          );
          console.log(
            `📋 Purchase Returns: ${supplier.purchaseReturns?.length || 0}`
          );
        },
        error: (error) => {
          console.error(
            `❌ Failed to load supplier details for ID ${id}:`,
            error
          );
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: 'فشل في تحميل بيانات المورد',
          });
        },
      });
  }

  getTotalPurchases(): number {
    if (!this.supplier?.purchaseInvoices) return 0;
    return this.supplier.purchaseInvoices.reduce(
      (sum, invoice) => sum + (invoice.totalAmount || 0),
      0
    );
  }

  getTotalReturns(): number {
    if (!this.supplier?.purchaseReturns) return 0;
    return this.supplier.purchaseReturns.reduce(
      (sum, ret) => sum + (ret.totalAmount || 0),
      0
    );
  }

  getTotalTransactions(): number {
    return this.getTotalPurchases() + this.getTotalReturns();
  }

  toggleInvoices(): void {
    this.showInvoices = !this.showInvoices;
    console.log(
      `📋 ${this.showInvoices ? 'Showing' : 'Hiding'} purchase invoices`
    );
  }

  toggleReturns(): void {
    this.showReturns = !this.showReturns;
    console.log(
      `📋 ${this.showReturns ? 'Showing' : 'Hiding'} purchase returns`
    );
  }

  editSupplier(): void {
    if (this.supplier) {
      console.log(`✏️ Navigating to edit supplier ID: ${this.supplier.id}`);
      this.router.navigate(['/partners/suppliers/edit', this.supplier.id]);
    }
  }

  deleteSupplier(event: Event): void {
    if (!this.supplier) {
      console.warn('⚠️ Attempted to delete undefined supplier');
      return;
    }

    console.log(
      `🗑️ Delete confirmation requested for supplier: ${this.supplier.name}`
    );

    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: `هل أنت متأكد من حذف المورد "${this.supplier.name}"؟ سيتم حذف كافة البيانات المرتبطة به بما في ذلك فواتير الشراء والمرتجعات.`,
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'نعم، احذف',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-text',
      accept: () => {
        console.log(
          `✅ User confirmed deletion of supplier ID: ${this.supplier!.id}`
        );
        this.supplierService.delete(this.supplier!.id).subscribe({
          next: () => {
            console.log(
              `🎉 Supplier ID ${this.supplier!.id} deleted successfully`
            );
            this.messageService.add({
              severity: 'success',
              summary: 'تم الحذف',
              detail: 'تم حذف المورد بنجاح',
            });
            setTimeout(() => {
              console.log('🔄 Redirecting to suppliers list after deletion');
              this.router.navigate(['/partners/suppliers']);
            }, 1000);
          },
          error: (err) => {
            console.error(
              `❌ Failed to delete supplier ID ${this.supplier!.id}:`,
              err
            );
            this.messageService.add({
              severity: 'error',
              summary: 'خطأ',
              detail: 'فشل في حذف المورد',
            });
          },
        });
      },
      reject: () => {
        console.log('❌ User cancelled deletion');
      },
    });
  }

  goBack(): void {
    console.log('↩️ Going back to suppliers list');
    this.router.navigate(['/partners/suppliers']);
  }

  getRandomColor(name: string): string {
    if (!name) return 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)';
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

  getInvoiceStatusSeverity(status: DocumentStatus): "success" | "secondary" | "info" | "warning" | "danger" | "contrast" | undefined {
    switch (status) {
      case DocumentStatus.Approved: return 'success';
      case DocumentStatus.Draft: return 'warning';
      case DocumentStatus.Cancelled: return 'danger';
      default: return 'info';
    }
  }

  getInvoiceStatusLabel(status: DocumentStatus): string {
    switch (status) {
      case DocumentStatus.Approved: return 'معتمدة';
      case DocumentStatus.Draft: return 'مسودة';
      case DocumentStatus.Cancelled: return 'ملغاة';
      default: return 'غير معروف';
    }
  }
}
