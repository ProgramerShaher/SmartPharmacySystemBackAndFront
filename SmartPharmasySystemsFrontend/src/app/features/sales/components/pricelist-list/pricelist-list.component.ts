import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MessageService, ConfirmationService } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { CheckboxModule } from 'primeng/checkbox';
import { DropdownModule } from 'primeng/dropdown';
import { TooltipModule } from 'primeng/tooltip';
import { DividerModule } from 'primeng/divider';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { PricelistService, Pricelist, CreatePricelistDto } from '../../../inventory/services/pricelist.service';
import { MedicineService } from '../../../inventory/services/medicine.service';

@Component({
  selector: 'app-pricelist-list',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, ButtonModule, TagModule,
    ToastModule, ConfirmDialogModule, DialogModule, InputTextModule,
    InputNumberModule, CheckboxModule, DropdownModule, TooltipModule,
    DividerModule, InputTextareaModule
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './pricelist-list.component.html',
  styleUrls: ['./pricelist-list.component.scss']
})
export class PricelistListComponent implements OnInit {
  pricelists = signal<Pricelist[]>([]);
  loading = signal(false);
  showDialog = signal(false);
  saving = signal(false);
  isEdit = signal(false);
  editId = signal<number | null>(null);

  medicines: any[] = [];

  form: CreatePricelistDto & { id?: number } = {
    name: '',
    description: '',
    globalDiscountPercentage: 0,
    isActive: true,
    items: []
  };

  newItem = { medicineId: 0, fixedPrice: undefined as number | undefined, discountPercentage: undefined as number | undefined };

  constructor(
    private pricelistService: PricelistService,
    private medicineService: MedicineService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) { }

  ngOnInit() {
    this.loadPricelists();
    this.loadMedicines();
  }

  loadPricelists() {
    this.loading.set(true);
    this.pricelistService.getAll().subscribe({
      next: (data: any) => { this.pricelists.set(data); this.loading.set(false); },
      error: () => { this.loading.set(false); }
    });
  }

  loadMedicines() {
    this.medicineService.getAll({ pageSize: 300 }).subscribe({
      next: (res: any) => this.medicines = res.items.map((m: any) => ({ label: m.name, value: m.id })),
      error: () => { }
    });
  }

  openAdd() {
    this.isEdit.set(false);
    this.editId.set(null);
    this.form = { name: '', description: '', globalDiscountPercentage: 0, isActive: true, items: [] };
    this.newItem = { medicineId: 0, fixedPrice: undefined, discountPercentage: undefined };
    this.showDialog.set(true);
  }

  openEdit(pl: Pricelist) {
    this.isEdit.set(true);
    this.editId.set(pl.id);
    this.form = {
      id: pl.id,
      name: pl.name,
      description: pl.description,
      globalDiscountPercentage: pl.globalDiscountPercentage,
      isActive: pl.isActive,
      items: pl.items.map((i: any) => ({ medicineId: i.medicineId, fixedPrice: i.fixedPrice ?? undefined, discountPercentage: i.discountPercentage ?? undefined }))
    };
    this.newItem = { medicineId: 0, fixedPrice: undefined, discountPercentage: undefined };
    this.showDialog.set(true);
  }

  addItem() {
    if (!this.newItem.medicineId) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى اختيار دواء' });
      return;
    }
    const exists = this.form.items.some((i: any) => i.medicineId === this.newItem.medicineId);
    if (exists) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'هذا الدواء موجود مسبقاً في القائمة' });
      return;
    }
    this.form.items.push({ ...this.newItem });
    this.newItem = { medicineId: 0, fixedPrice: undefined, discountPercentage: undefined };
  }

  removeItem(index: number) {
    this.form.items.splice(index, 1);
  }

  getMedicineName(id: number): string {
    return this.medicines.find(m => m.value === id)?.label || `دواء #${id}`;
  }

  save() {
    if (!this.form.name?.trim()) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يرجى إدخال اسم القائمة' });
      return;
    }
    this.saving.set(true);
    const obs = this.isEdit()
      ? this.pricelistService.update(this.editId()!, this.form as any)
      : this.pricelistService.create(this.form);

    obs.subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'تم', detail: this.isEdit() ? 'تم تحديث قائمة الأسعار' : 'تم إنشاء قائمة الأسعار بنجاح' });
        this.saving.set(false);
        this.showDialog.set(false);
        this.loadPricelists();
      },
      error: (err: any) => {
        this.saving.set(false);
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل الحفظ' });
      }
    });
  }

  delete(pl: Pricelist) {
    this.confirmationService.confirm({
      header: 'تأكيد الحذف',
      message: `هل تريد حذف قائمة الأسعار "${pl.name}"؟ ${pl.customersCount > 0 ? `(${pl.customersCount} عملاء مرتبطون)` : ''}`,
      icon: 'pi pi-trash',
      acceptLabel: 'حذف',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.pricelistService.delete(pl.id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'info', summary: 'تم', detail: 'تم الحذف بنجاح' });
            this.loadPricelists();
          },
          error: (err: any) => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل الحذف' })
        });
      }
    });
  }
}
