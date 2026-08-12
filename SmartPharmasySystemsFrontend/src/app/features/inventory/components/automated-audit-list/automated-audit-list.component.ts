import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { DialogModule } from 'primeng/dialog';
import { MessageService, ConfirmationService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { ToastModule } from 'primeng/toast';
import { TooltipModule } from 'primeng/tooltip';
import { AutomatedAuditService } from '../../services/automated-audit.service';
import { AutomatedAuditHeaderDto, GenerateAuditRequestDto } from '../../../../core/models/automated-audit.interface';
import { WarehouseService } from '../../../warehouses/services/warehouse.service';
import { WarehouseDto } from '../../../../core/models/warehouse.interface';

@Component({
  selector: 'app-automated-audit-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    TableModule,
    ButtonModule,
    DropdownModule,
    DialogModule,
    ConfirmDialogModule,
    InputTextareaModule,
    ToastModule,
    TooltipModule
  ],
  providers: [MessageService, ConfirmationService],
  templateUrl: './automated-audit-list.component.html',
  styleUrls: ['./automated-audit-list.component.scss']
})
export class AutomatedAuditListComponent implements OnInit {
  private auditService = inject(AutomatedAuditService);
  private warehouseService = inject(WarehouseService);
  private messageService = inject(MessageService);
  private confirmationService = inject(ConfirmationService);
  private router = inject(Router);

  audits: AutomatedAuditHeaderDto[] = [];
  warehouses: WarehouseDto[] = [];
  loading = false;
  totalRecords = 0;

  displayGenerateModal = false;
  generating = false;

  newAuditRequest: GenerateAuditRequestDto = {
    warehouseId: null,
    countType: 1,
    notes: ''
  };

  countTypes = [
    { label: 'جرد يومي', value: 1 },
    { label: 'جرد أسبوعي', value: 2 },
    { label: 'جرد شهري', value: 3 },
    { label: 'جرد ربع سنوي', value: 4 },
    { label: 'جرد سنوي', value: 5 },
    { label: 'جرد شامل (استثنائي)', value: 6 }
  ];

  ngOnInit() {
    this.loadWarehouses();
    this.loadAudits();
  }

  loadWarehouses() {
    this.warehouseService.getAll().subscribe({
      next: (data) => this.warehouses = data,
      error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل المستودعات' })
    });
  }

  loadAudits(event?: any) {
    this.loading = true;
    const page = event ? (event.first / event.rows) + 1 : 1;
    const pageSize = event ? event.rows : 10;

    this.auditService.getAll(page, pageSize).subscribe({
      next: (response) => {
        this.audits = response.items;
        this.totalRecords = response.totalCount;
        this.loading = false;
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل تحميل قائمة الجرد الآلي' });
        this.loading = false;
      }
    });
  }

  showGenerateModal() {
    this.newAuditRequest = { warehouseId: null, countType: 1, notes: '' };
    this.displayGenerateModal = true;
  }

  generateAudit() {
    this.generating = true;
    const request: GenerateAuditRequestDto = {
      ...this.newAuditRequest,
      warehouseId: this.newAuditRequest.warehouseId || null
    };

    this.auditService.generateAudit(request).subscribe({
      next: (audit) => {
        this.generating = false;
        this.displayGenerateModal = false;
        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم توليد الجرد الشامل بنجاح' });
        this.router.navigate(['/inventory/automated-audits', audit.id]);
      },
      error: () => {
        this.generating = false;
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'حدث خطأ أثناء عملية الجرد الآلي' });
      }
    });
  }

  deleteAudit(id: number) {
    this.confirmationService.confirm({
      message: 'هل أنت متأكد من حذف هذا الجرد الآلي بشكل نهائي؟',
      accept: () => {
        this.auditService.delete(id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم حذف الجرد بنجاح' });
            this.loadAudits();
          },
          error: () => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل الحذف' })
        });
      }
    });
  }

  getTotalShortageValue(): number {
    return this.audits.reduce((total, audit) => total + (audit.totalShortageValue || 0), 0);
  }

  getWarehouseAuditCount(): number {
    return this.audits.filter(audit => !!audit.warehouseId).length;
  }

  getGeneralAuditCount(): number {
    return this.audits.filter(audit => !audit.warehouseId).length;
  }
}
