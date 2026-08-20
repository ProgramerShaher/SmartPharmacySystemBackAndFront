import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MessageService, ConfirmationService } from 'primeng/api';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { BadgeModule } from 'primeng/badge';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { DividerModule } from 'primeng/divider';
import { DamagedGoodsService, DamagedGoodsRecord, CreateDamagedGoodsDto } from '../../services/damaged-goods.service';
import { MedicineService } from '../../services/medicine.service';
import { InventoryService } from '../../services/inventory.service';

@Component({
  selector: 'app-damaged-goods-list',
  standalone: true,
  imports: [
    CommonModule, FormsModule, TableModule, ButtonModule, TagModule,
    ToastModule, ConfirmDialogModule, BadgeModule, TooltipModule,
    DialogModule, DropdownModule, CalendarModule, InputNumberModule,
    InputTextModule, DividerModule
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './damaged-goods-list.component.html',
  styleUrls: ['./damaged-goods-list.component.scss']
})
export class DamagedGoodsListComponent implements OnInit {
  records = signal<DamagedGoodsRecord[]>([]);
  loading = signal(false);
  showAddDialog = signal(false);
  saving = signal(false);
  activeTab = signal<'all' | 'pending' | 'approved'>('all');

  // Form
  form: CreateDamagedGoodsDto = {
    sourceWarehouseId: 0,
    medicineId: 0,
    batchNumber: '',
    expiryDate: new Date().toISOString(),
    quantity: 1,
    damageType: 0,
    disposalMethod: 0
  };

  medicines: any[] = [];
  warehouses: any[] = [];
  batches: any[] = [];

  damageTypes = [
    { label: 'تلف مادي', value: 0 },
    { label: 'انتهاء صلاحية', value: 1 },
    { label: 'تلوث', value: 2 },
    { label: 'سرقة / ضياع', value: 3 },
    { label: 'أخرى', value: 4 },
  ];

  disposalMethods = [
    { label: 'إتلاف', value: 0 },
    { label: 'إعادة للمورد', value: 1 },
    { label: 'تبرع', value: 2 },
  ];

  constructor(
    private damagedGoodsService: DamagedGoodsService,
    private medicineService: MedicineService,
    private inventoryService: InventoryService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private router: Router
  ) {}

  ngOnInit() {
    this.loadData();
    this.loadMedicines();
    this.loadWarehouses();
  }

  loadData() {
    this.loading.set(true);
    const obs = this.activeTab() === 'pending'
      ? this.damagedGoodsService.getPending()
      : this.activeTab() === 'approved'
      ? this.damagedGoodsService.getApproved()
      : this.damagedGoodsService.getAll();

    obs.subscribe({
      next: (data) => { this.records.set(data); this.loading.set(false); },
      error: () => { this.loading.set(false); this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل البيانات' }); }
    });
  }

  setTab(tab: 'all' | 'pending' | 'approved') {
    this.activeTab.set(tab);
    this.loadData();
  }

  loadMedicines() {
    this.medicineService.getAll({ pageSize: 200 }).subscribe({
      next: (res) => this.medicines = res.items.map(m => ({ label: m.name, value: m.id, id: m.id })),
      error: () => {}
    });
  }

  loadWarehouses() {
    this.inventoryService.getWarehouses().subscribe({
      next: (data: any[]) => this.warehouses = data.map(w => ({ label: w.name, value: w.id })),
      error: () => {}
    });
  }

  onMedicineSelect(medicineId: number) {
    this.form.batchNumber = '';
    if (!medicineId) return;
    this.medicineService.getFefoBatches(medicineId).subscribe({
      next: (batches) => {
        this.batches = batches.map((b: any) => ({
          label: `${b.companyBatchNumber} (ينتهي: ${b.expiryDate ? new Date(b.expiryDate).toLocaleDateString('ar') : '-'}) - متبقي: ${b.remainingQuantity}`,
          value: b.companyBatchNumber,
          expiryDate: b.expiryDate
        }));
      },
      error: () => {}
    });
  }

  onBatchSelect(batchNumber: string) {
    const batch = this.batches.find(b => b.value === batchNumber);
    if (batch?.expiryDate) this.form.expiryDate = batch.expiryDate;
  }

  openAddDialog() {
    this.form = {
      sourceWarehouseId: 0, medicineId: 0, batchNumber: '',
      expiryDate: new Date().toISOString(), quantity: 1, damageType: 0, disposalMethod: 0
    };
    this.batches = [];
    this.showAddDialog.set(true);
  }

  save() {
    if (!this.form.sourceWarehouseId || !this.form.medicineId || !this.form.batchNumber || this.form.quantity < 1) {
      this.messageService.add({ severity: 'warn', summary: 'تحقق', detail: 'يرجى تعبئة جميع الحقول المطلوبة' });
      return;
    }
    this.saving.set(true);
    this.damagedGoodsService.create(this.form).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'تم', detail: 'تم تسجيل طلب الإتلاف بنجاح (بانتظار الاعتماد)' });
        this.showAddDialog.set(false);
        this.saving.set(false);
        this.loadData();
      },
      error: (err) => {
        this.saving.set(false);
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل التسجيل' });
      }
    });
  }

  approve(record: DamagedGoodsRecord) {
    this.confirmationService.confirm({
      header: 'تأكيد اعتماد الإتلاف',
      message: `هل تريد اعتماد إذن الإتلاف "${record.damageCode}"؟\nسيتم خصم ${record.quantity} وحدة من المخزون وإنشاء قيد محاسبي بقيمة ${record.damageValue?.toFixed(2)} تلقائياً.`,
      icon: 'pi pi-exclamation-triangle',
      acceptLabel: 'نعم، اعتمد',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.damagedGoodsService.approve(record.id, 1).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'تم', detail: `تم اعتماد الإتلاف - تم خصم ${record.quantity} وحدة وإنشاء القيد المحاسبي` });
            this.loadData();
          },
          error: (err) => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل الاعتماد' })
        });
      }
    });
  }

  delete(record: DamagedGoodsRecord) {
    this.confirmationService.confirm({
      header: 'تأكيد الحذف',
      message: `هل تريد حذف إذن الإتلاف "${record.damageCode}"؟`,
      icon: 'pi pi-trash',
      acceptLabel: 'حذف',
      rejectLabel: 'إلغاء',
      acceptButtonStyleClass: 'p-button-danger',
      accept: () => {
        this.damagedGoodsService.delete(record.id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'info', summary: 'تم', detail: 'تم الحذف بنجاح' });
            this.loadData();
          },
          error: (err) => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل الحذف' })
        });
      }
    });
  }

  getStatusSeverity(status: number): string {
    switch (status) {
      case 0: return 'warning';  // PendingApproval
      case 1: return 'success';  // Approved
      case 2: return 'danger';   // Rejected
      default: return 'secondary';
    }
  }

  getStatusLabel(status: number): string {
    switch (status) {
      case 0: return 'بانتظار الاعتماد';
      case 1: return 'معتمد';
      case 2: return 'مرفوض';
      default: return 'غير معروف';
    }
  }

  getDamageTypeLabel(type: number): string {
    return this.damageTypes.find(d => d.value === type)?.label || 'غير معروف';
  }

  getDisposalMethodLabel(method: number): string {
    return this.disposalMethods.find(d => d.value === method)?.label || 'غير معروف';
  }

  getTotalDamageValue(): number {
    return this.records().reduce((sum, r) => sum + (r.damageValue || 0), 0);
  }

  getPendingCount(): number {
    return this.records().filter(r => r.status === 0).length;
  }
}
