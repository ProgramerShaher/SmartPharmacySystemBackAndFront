import { Component, OnInit, signal, computed, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { InputNumberModule } from 'primeng/inputnumber';
import { TableModule } from 'primeng/table';
import { AutoCompleteModule } from 'primeng/autocomplete';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { ToastModule } from 'primeng/toast';
import { DividerModule } from 'primeng/divider';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { CheckboxModule } from 'primeng/checkbox';
import { CardModule } from 'primeng/card';
import { QuotationService } from '../../services/quotation.service';
import { MedicineService } from '../../../inventory/services/medicine.service';
import { CustomerService } from '../../../customers/services/customer.service';
import { CreateQuotationDto, CreateQuotationDetailDto, Quotation } from '../../models/quotation.interface';
import { Medicine } from '../../../../core/models/medicine.interface';
import { Customer } from '../../../../core/models/customer.models';

interface QuotationLineItem {
  medicineId: number;
  medicineName: string;
  barcode?: string;
  saleUnitId?: number;
  unitName?: string;
  quantity: number;
  unitPrice: number;
  discountPercentage: number;
  discountAmount: number;
  taxRate: number;
  subtotal: number;
  taxAmount: number;
  total: number;
  notes?: string;
}

@Component({
  selector: 'app-quotation-create',
  standalone: true,
  imports: [
    CommonModule, FormsModule, RouterModule, ButtonModule, InputTextModule,
    InputNumberModule, TableModule, AutoCompleteModule, DropdownModule,
    CalendarModule, ToastModule, DividerModule, InputTextareaModule,
    CheckboxModule, CardModule
  ],
  providers: [MessageService],
  templateUrl: './quotation-create.component.html',
  styleUrls: ['./quotation-create.component.scss']
})
export class QuotationCreateComponent implements OnInit {
  private quotationService = inject(QuotationService);
  private medicineService = inject(MedicineService);
  private customerService = inject(CustomerService);
  private messageService = inject(MessageService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  isEditMode = signal<boolean>(false);
  quotationId = signal<number | null>(null);
  loading = signal<boolean>(false);
  saving = signal<boolean>(false);

  // Form Header State
  quotationNumber = signal<string>('');
  quotationDate = new Date();
  expiryDate: Date | null = new Date(Date.now() + 15 * 24 * 60 * 60 * 1000); // 15 days default
  selectedCustomer: Customer | null = null;
  customerName: string = '';
  customerPhone: string = '';
  customerEmail: string = '';
  isTaxInclusive: boolean = true;
  headerTaxRate: number = 15;
  headerDiscount: number = 0;
  notes: string = '';
  termsAndConditions: string = '1. الأسعار الموضحة أعلاه سارية حتى تاريخ انتهاء صلاحية العرض.\n2. التوريد يتم فور استلام أمر الشراء المعتمد.\n3. الدفع نقداً أو بموجب شيك مقبول الدفع عند التسليم.\n4. الضمان يشمل عيوب التصنيع لمدة سنتين للأجهزة والمعدات.';

  // Line Items
  items = signal<QuotationLineItem[]>([]);

  // Medicine Search
  filteredMedicines: Medicine[] = [];
  selectedMedicine: Medicine | null = null;
  searchQuery: string = '';

  // Customer List
  customers: Customer[] = [];

  // Computed Totals
  subtotal = computed(() => {
    return this.items().reduce((sum, item) => sum + item.subtotal, 0);
  });

  taxAmount = computed(() => {
    return this.items().reduce((sum, item) => sum + item.taxAmount, 0);
  });

  totalDiscount = computed(() => {
    const lineDiscounts = this.items().reduce((sum, item) => sum + item.discountAmount, 0);
    return lineDiscounts + (this.headerDiscount || 0);
  });

  netTotal = computed(() => {
    if (this.isTaxInclusive) {
      const sum = this.items().reduce((s, it) => s + it.total, 0);
      return Math.max(0, sum - (this.headerDiscount || 0));
    } else {
      const netSub = Math.max(0, this.subtotal() - (this.headerDiscount || 0));
      const tax = (netSub * this.headerTaxRate) / 100;
      return netSub + tax;
    }
  });

  ngOnInit(): void {
    this.loadLookups();

    const id = this.route.snapshot.params['id'];
    if (id) {
      this.isEditMode.set(true);
      this.quotationId.set(+id);
      this.loadQuotationForEdit(+id);
    }
  }

  loadLookups(): void {
    this.customerService.loadLookupIndex().subscribe({
      next: (list) => { this.customers = list; },
      error: () => {}
    });
  }

  loadQuotationForEdit(id: number): void {
    this.loading.set(true);
    this.quotationService.getById(id).subscribe({
      next: (q) => {
        this.quotationNumber.set(q.quotationNumber);
        this.quotationDate = new Date(q.quotationDate);
        this.expiryDate = q.expiryDate ? new Date(q.expiryDate) : null;
        this.customerName = q.customerName || '';
        this.customerPhone = q.customerPhone || '';
        this.customerEmail = q.customerEmail || '';
        this.headerTaxRate = q.taxRate;
        this.isTaxInclusive = q.isTaxInclusive;
        this.headerDiscount = q.totalDiscount;
        this.notes = q.notes || '';
        this.termsAndConditions = q.termsAndConditions || '';

        if (q.customerId) {
          this.selectedCustomer = this.customers.find(c => c.id === q.customerId) || null;
        }

        const lines: QuotationLineItem[] = q.details.map(d => ({
          medicineId: d.medicineId,
          medicineName: d.medicineName,
          barcode: d.barcode,
          saleUnitId: d.saleUnitId,
          unitName: d.unitName,
          quantity: d.quantity,
          unitPrice: d.unitPrice,
          discountPercentage: d.discountPercentage,
          discountAmount: d.discountAmount,
          taxRate: d.taxRate,
          subtotal: d.subtotal,
          taxAmount: d.taxAmount,
          total: d.total,
          notes: d.notes
        }));

        this.items.set(lines);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل تحميل بيانات عرض السعر للتعديل'
        });
      }
    });
  }

  onCustomerSelect(event: any): void {
    const cust = event.value;
    if (cust) {
      this.customerName = cust.name;
      this.customerPhone = cust.phoneNumber || '';
      this.customerEmail = cust.email || '';
    }
  }

  searchMedicines(event: any): void {
    const query = event.query.toLowerCase();
    this.medicineService.loadLookupIndex().subscribe((medicines) => {
      this.filteredMedicines = medicines.filter(m =>
        m.name?.toLowerCase().includes(query) ||
        m.defaultBarcode?.toLowerCase().includes(query) ||
        m.internalCode?.toLowerCase().includes(query)
      ).slice(0, 15);
    });
  }

  onMedicineSelect(event: any): void {
    const med: Medicine = event.value;
    if (!med) return;

    const existingIndex = this.items().findIndex(i => i.medicineId === med.id);
    if (existingIndex > -1) {
      const updated = [...this.items()];
      updated[existingIndex].quantity += 1;
      this.recalculateLine(updated[existingIndex]);
      this.items.set(updated);
    } else {
      const price = med.defaultPurchasePrice ? (med.defaultPurchasePrice * 1.25) : 10;
      const newLine: QuotationLineItem = {
        medicineId: med.id,
        medicineName: med.name,
        barcode: med.defaultBarcode,
        quantity: 1,
        unitPrice: Math.round(price * 100) / 100,
        discountPercentage: 0,
        discountAmount: 0,
        taxRate: this.headerTaxRate,
        subtotal: 0,
        taxAmount: 0,
        total: 0
      };
      this.recalculateLine(newLine);
      this.items.update(list => [...list, newLine]);
    }

    this.selectedMedicine = null;
    this.searchQuery = '';
  }

  recalculateLine(line: QuotationLineItem): void {
    const gross = line.quantity * line.unitPrice;
    if (line.discountPercentage > 0) {
      line.discountAmount = Math.round((gross * line.discountPercentage / 100) * 100) / 100;
    }
    const afterDiscount = Math.max(0, gross - line.discountAmount);
    const rate = line.taxRate || this.headerTaxRate;

    if (this.isTaxInclusive) {
      line.total = afterDiscount;
      line.subtotal = rate > 0 ? (line.total / (1 + rate / 100)) : line.total;
      line.taxAmount = line.total - line.subtotal;
    } else {
      line.subtotal = afterDiscount;
      line.taxAmount = rate > 0 ? (line.subtotal * (rate / 100)) : 0;
      line.total = line.subtotal + line.taxAmount;
    }

    line.subtotal = Math.round(line.subtotal * 100) / 100;
    line.taxAmount = Math.round(line.taxAmount * 100) / 100;
    line.total = Math.round(line.total * 100) / 100;
  }

  onLineChange(line: QuotationLineItem): void {
    this.recalculateLine(line);
    this.items.update(list => [...list]);
  }

  removeItem(index: number): void {
    this.items.update(list => list.filter((_, i) => i !== index));
  }

  recalculateAll(): void {
    const updated = this.items().map(item => {
      item.taxRate = this.headerTaxRate;
      this.recalculateLine(item);
      return item;
    });
    this.items.set(updated);
  }

  saveQuotation(): void {
    if (!this.items().length) {
      this.messageService.add({
        severity: 'warn',
        summary: 'تنبيه',
        detail: 'يجب إضافة صنف واحد على الأقل في عرض السعر'
      });
      return;
    }

    if (!this.customerName.trim() && !this.selectedCustomer) {
      this.messageService.add({
        severity: 'warn',
        summary: 'تنبيه',
        detail: 'يرجى إدخال اسم العميل أو اختيار عميل مسجل'
      });
      return;
    }

    this.saving.set(true);

    const detailDtos: CreateQuotationDetailDto[] = this.items().map(it => ({
      medicineId: it.medicineId,
      saleUnitId: it.saleUnitId,
      quantity: it.quantity,
      unitPrice: it.unitPrice,
      discountPercentage: it.discountPercentage,
      discountAmount: it.discountAmount,
      taxRate: it.taxRate,
      notes: it.notes
    }));

    const dto: CreateQuotationDto = {
      quotationDate: this.quotationDate.toISOString(),
      expiryDate: this.expiryDate ? this.expiryDate.toISOString() : undefined,
      customerId: this.selectedCustomer?.id,
      customerName: this.selectedCustomer?.name || this.customerName,
      customerPhone: this.customerPhone,
      customerEmail: this.customerEmail,
      taxRate: this.headerTaxRate,
      isTaxInclusive: this.isTaxInclusive,
      totalDiscount: this.headerDiscount,
      notes: this.notes,
      termsAndConditions: this.termsAndConditions,
      details: detailDtos
    };

    if (this.isEditMode() && this.quotationId()) {
      this.quotationService.update(this.quotationId()!, dto).subscribe({
        next: (res) => {
          this.saving.set(false);
          this.messageService.add({
            severity: 'success',
            summary: 'نجاح التحديث',
            detail: 'تم تحديث عرض السعر بنجاح'
          });
          this.router.navigate(['/sales/quotations', res.id]);
        },
        error: (err) => {
          this.saving.set(false);
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: err.error?.message || 'فشل تحديث عرض السعر'
          });
        }
      });
    } else {
      this.quotationService.create(dto).subscribe({
        next: (res) => {
          this.saving.set(false);
          this.messageService.add({
            severity: 'success',
            summary: 'تم الإنشاء',
            detail: `تم إنشاء عرض السعر برقم ${res.quotationNumber} بنجاح`
          });
          this.router.navigate(['/sales/quotations', res.id]);
        },
        error: (err) => {
          this.saving.set(false);
          this.messageService.add({
            severity: 'error',
            summary: 'خطأ',
            detail: err.error?.message || 'فشل حفظ عرض السعر'
          });
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/sales/quotations']);
  }
}
