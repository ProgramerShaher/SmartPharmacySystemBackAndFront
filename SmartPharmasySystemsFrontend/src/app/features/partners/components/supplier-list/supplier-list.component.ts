import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ConfirmationService, MessageService } from 'primeng/api';
import { SupplierService } from '../../services/supplier.service';
import { Supplier } from '../../../../core/models/supplier.interface';
import { catchError, finalize } from 'rxjs/operators';
import { of } from 'rxjs';

@Component({
  selector: 'app-supplier-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    ToastModule,
    ConfirmDialogModule,
    TagModule,
    TooltipModule,
  ],
  templateUrl: './supplier-list.component.html',
  styleUrl: './supplier-list.component.scss',
  providers: [MessageService, ConfirmationService],
})
export class SupplierListComponent implements OnInit {
  suppliers: Supplier[] = [];
  loading: boolean = true;
  searchTerm: string = '';
  today = new Date();

  constructor(
    private supplierService: SupplierService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    public router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    console.log('🚀 Supplier List Component Initialized');
    this.loadSuppliers();
  }

  loadSuppliers(): void {
    this.loading = true;
    console.log('⏳ Loading suppliers list...');
    console.log(`🔍 Search term: "${this.searchTerm}"`);

    // إنشاء query object
    const query: any = {};
    if (this.searchTerm && this.searchTerm.trim() !== '') {
      query.search = this.searchTerm.trim();
    }

    console.log('📤 Query being sent:', query);

    this.supplierService
      .getAllSuppliers(query)
      .pipe(
        catchError((error) => {
          console.error('❌ Failed to load suppliers:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: error.error?.message || 'فشل في تحميل بيانات الموردين',
          });
          this.suppliers = [];
          return of({ items: [], totalCount: 0, pageNumber: 1, pageSize: 10 });
        }),
        finalize(() => {
          this.loading = false;
          console.log(
            `✅ Suppliers list loaded: ${this.suppliers.length} suppliers found`
          );
        })
      )
      .subscribe({
        next: (result) => {
          console.log('📥 Received result:', result);
          if (result && result.items) {
            this.suppliers = result.items;
            console.log(
              `📊 Loaded ${result.items.length} suppliers out of ${result.totalCount} total`
            );
          } else {
            console.warn('⚠️ No items in result, setting empty array');
            this.suppliers = [];
          }
        }
      });
  }

  onSearch(): void {
    console.log(`🔍 Performing search with term: "${this.searchTerm}"`);
    this.loadSuppliers();
  }

  deleteSupplier(event: Event, supplier: Supplier): void {
    console.log(
      `🗑️ Delete requested for supplier: ${supplier.name} (ID: ${supplier.id})`
    );

    this.confirmationService.confirm({
      target: event.target as EventTarget,
      message: `هل أنت متأكد من حذف المورد "${supplier.name}"؟`,
      header: 'تأكيد الحذف',
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'نعم',
      rejectLabel: 'لا',
      acceptButtonStyleClass: 'p-button-danger',
      rejectButtonStyleClass: 'p-button-text',
      accept: () => {
        console.log(
          `✅ User confirmed deletion of supplier ID: ${supplier.id}`
        );
        this.supplierService.delete(supplier.id).subscribe({
          next: () => {
            console.log(`🎉 Supplier ID ${supplier.id} deleted successfully`);
            this.messageService.add({
              severity: 'success',
              summary: 'تم بنجاح',
              detail: 'تم حذف المورد بنجاح',
            });
            console.log('🔄 Reloading suppliers list after deletion');
            this.loadSuppliers();
          },
          error: (err) => {
            console.error(
              `❌ Failed to delete supplier ID ${supplier.id}:`,
              err
            );
            this.messageService.add({
              severity: 'error',
              summary: 'خطأ',
              detail: 'فشل في عملية الحذف',
            });
          },
        });
      },
      reject: () => {
        console.log('❌ User cancelled deletion');
      },
    });
  }

  viewDetails(id: number): void {
    console.log(`👁️ Viewing details for supplier ID: ${id}`);
    this.router.navigate(['detail', id], { relativeTo: this.route.parent });
  }

  editSupplier(id: number): void {
    console.log(`✏️ Editing supplier ID: ${id}`);
    this.router.navigate(['edit', id], { relativeTo: this.route.parent });
  }

  addNewSupplier(): void {
    console.log('➕ Navigating to add new supplier');
    this.router.navigate(['create'], { relativeTo: this.route.parent });
  }

  // إحصائيات إضافية
  getSuppliersWithBalance(): number {
    return this.suppliers.filter((s) => (s.balance || 0) > 0).length;
  }

  getTotalBalance(): number {
    return this.suppliers.reduce((sum, s) => sum + (s.balance || 0), 0);
  }

  getSuppliersWithInvoices(): number {
    return this.suppliers.filter(
      (s) => s.purchaseInvoices && s.purchaseInvoices.length > 0
    ).length;
  }

  getTotalPurchases(): number {
    return this.suppliers.reduce((sum, s) => {
      const invoicesTotal =
        s.purchaseInvoices?.reduce(
          (invSum, inv) => invSum + (inv.totalAmount || 0),
          0
        ) || 0;
      return sum + invoicesTotal;
    }, 0);
  }
}
