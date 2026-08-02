import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { DialogModule } from 'primeng/dialog';
import { TabViewModule } from 'primeng/tabview';
import { DropdownModule } from 'primeng/dropdown';
import { InputNumberModule } from 'primeng/inputnumber';

import { StockTransferService, StockTransferDto } from '../../services/stock-transfer.service';
import { WarehouseService } from '../../services/warehouse.service';
import { WarehouseDto } from '../../../../core/models';
import { MessageService, ConfirmationService } from 'primeng/api';
import { AuthService } from '../../../auth/services/auth.service';

@Component({
  selector: 'app-external-transfer-list',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    FormsModule,
    TableModule,
    ButtonModule,
    CardModule,
    TagModule,
    TooltipModule,
    ToastModule,
    ConfirmDialogModule,
    DialogModule,
    TabViewModule,
    DropdownModule,
    InputNumberModule
  ],
  templateUrl: './external-transfer-list.component.html',
  styleUrls: ['./external-transfer-list.component.scss'],
  providers: [MessageService, ConfirmationService]
})
export class ExternalTransferListComponent implements OnInit {
  outgoingTransfers: StockTransferDto[] = [];
  incomingTransfers: StockTransferDto[] = [];
  loading: boolean = false;
  
  displayReceiveDialog: boolean = false;
  displayDetailDialog: boolean = false;
  selectedTransfer: StockTransferDto | null = null;
  processingAction: boolean = false;
  
  warehouses: WarehouseDto[] = [];
  selectedReceiveWarehouseId: number | null = null;
  receiveItems: any[] = [];

  constructor(
    private transferService: StockTransferService,
    private warehouseService: WarehouseService,
    private messageService: MessageService,
    private confirmationService: ConfirmationService,
    private authService: AuthService
  ) {}

  get currentUserId(): number {
    return this.authService.currentUserValue?.userId ?? 1;
  }
  
  get currentUserBranchId(): number {
    return Number(this.authService.currentUserValue?.branchId ?? 0);
  }

  ngOnInit() {
    this.loadTransfers();
    this.loadWarehouses();
  }

  loadWarehouses() {
    this.warehouseService.getAll().subscribe(res => {
      // Filter warehouses for the current branch
      if (this.currentUserBranchId > 0) {
        this.warehouses = res.filter((w: any) => Number(w.branchId) === this.currentUserBranchId && w.type !== 'Damaged' && w.type !== 3);
      } else {
        this.warehouses = res.filter((w: any) => w.type !== 'Damaged' && w.type !== 3);
      }
    });
  }

  loadTransfers() {
    this.loading = true;
    this.transferService.getAll({ transferType: 5 }).subscribe({
      next: (data) => {
        const branchId = this.currentUserBranchId;
        // outgoing: source branch is current branch
        this.outgoingTransfers = data.filter(t => t.sourceBranchId === branchId || !branchId);
        // incoming: destination branch is current branch
        this.incomingTransfers = data.filter(t => t.destinationBranchId === branchId || !branchId);
        this.loading = false;
      },
      error: () => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل تحويلات الفروع' });
        this.loading = false;
      }
    });
  }

  getTransferDirectionLabel(transfer: StockTransferDto): string {
    if (transfer.sourceBranchId === this.currentUserBranchId) {
      return 'صادر';
    }
    return 'وارد';
  }

  isStatus(status: any, expected: string): boolean {
    return String(status).toLowerCase() === expected.toLowerCase() || String(status) === this.mapStatusToNumber(expected).toString();
  }
  
  mapStatusToNumber(status: string): number {
      switch (status.toLowerCase()) {
          case 'requested': return 1;
          case 'approved': return 2;
          case 'dispatched': return 3;
          case 'received': return 4;
          case 'rejected': return 5;
          default: return 0;
      }
  }

  getStatusSeverity(status: any): 'success' | 'info' | 'warning' | 'danger' | 'secondary' {
    const s = String(status).toLowerCase();
    switch (s) {
      case 'requested': case '1': return 'info';
      case 'approved': case '2': return 'success';
      case 'dispatched': case '3': return 'warning';
      case 'received': case '4': return 'success';
      case 'rejected': case '5': return 'danger';
      default: return 'info';
    }
  }

  getStatusLabel(status: any): string {
    const s = String(status).toLowerCase();
    switch (s) {
      case 'requested': case '1': return 'بانتظار الاعتماد';
      case 'approved': case '2': return 'معتمد';
      case 'dispatched': case '3': return 'قيد الشحن';
      case 'received': case '4': return 'مستلم';
      case 'rejected': case '5': return 'مرفوض';
      default: return String(status);
    }
  }

  viewDetails(transfer: StockTransferDto) {
    this.selectedTransfer = transfer;
    this.displayDetailDialog = true;
  }

  approveTransfer(transfer: StockTransferDto) {
    this.confirmationService.confirm({
      message: 'هل أنت متأكد من اعتماد طلب التحويل الخارجي؟',
      accept: () => {
        this.transferService.approve(transfer.id, this.currentUserId).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم الاعتماد بنجاح' });
            this.loadTransfers();
          },
          error: (err) => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل في الاعتماد' })
        });
      }
    });
  }

  dispatchTransfer(transfer: StockTransferDto) {
    this.confirmationService.confirm({
      message: 'هل أنت متأكد من شحن هذه البضاعة للفرع الآخر؟',
      accept: () => {
        this.transferService.dispatch(transfer.id, this.currentUserId).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم الشحن بنجاح' });
            this.loadTransfers();
          },
          error: (err) => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل في الشحن' })
        });
      }
    });
  }

  deleteTransfer(transfer: StockTransferDto) {
    this.confirmationService.confirm({
      message: 'هل أنت متأكد من إلغاء طلب التحويل نهائياً؟',
      accept: () => {
        this.transferService.delete(transfer.id).subscribe({
          next: () => {
            this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم الإلغاء بنجاح' });
            this.loadTransfers();
          },
          error: (err) => this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في الإلغاء' })
        });
      }
    });
  }

  openReceiveDialog(transfer: StockTransferDto) {
    this.selectedTransfer = transfer;
    this.selectedReceiveWarehouseId = null;
    this.receiveItems = transfer.items.map(i => ({
      ...i,
      quantityReceived: i.quantityDispatched // default suggestion
    }));
    this.displayReceiveDialog = true;
  }

  confirmReceive() {
    if (!this.selectedTransfer || !this.selectedReceiveWarehouseId) {
      this.messageService.add({ severity: 'warn', summary: 'تنبيه', detail: 'يجب اختيار مستودع الإيداع' });
      return;
    }

    this.processingAction = true;
    const payload = {
      destinationWarehouseId: this.selectedReceiveWarehouseId,
      items: this.receiveItems.map(i => ({
        stockTransferItemId: i.id,
        quantityReceived: i.quantityReceived || 0
      }))
    };

    this.transferService.receive(this.selectedTransfer.id, payload, this.currentUserId).subscribe({
      next: () => {
        this.messageService.add({ severity: 'success', summary: 'نجاح', detail: 'تم استلام البضاعة وإيداعها في المستودع بنجاح' });
        this.displayReceiveDialog = false;
        this.processingAction = false;
        this.loadTransfers();
      },
      error: (err) => {
        this.messageService.add({ severity: 'error', summary: 'خطأ', detail: err.error?.message || 'فشل في استلام البضاعة' });
        this.processingAction = false;
      }
    });
  }

  getVariance(item: any): string {
    if (item.quantityReceived == null) return '';
    const diff = item.quantityReceived - item.quantityDispatched;
    if (diff > 0) return `+${diff}`;
    return diff.toString();
  }

  getVarianceClass(item: any): string {
    if (item.quantityReceived == null) return 'text-500';
    const diff = item.quantityReceived - item.quantityDispatched;
    if (diff === 0) return 'text-green-600 bg-green-50';
    if (diff < 0) return 'text-red-600 bg-red-50';
    return 'text-orange-600 bg-orange-50';
  }
}
