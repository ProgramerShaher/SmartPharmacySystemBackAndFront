import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { forkJoin } from 'rxjs';
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
    const branchId = this.currentUserBranchId;

    if (branchId > 0) {
      // Use the dedicated branch endpoint — server filters source + destination correctly
      forkJoin({
        push: this.transferService.getByBranch(branchId, 5), // External Push (type=5)
        pull: this.transferService.getByBranch(branchId, 3)  // BranchRequest/Pull (type=3)
      }).subscribe({
        next: (res) => {
          const allExternal = [...res.push, ...res.pull];
          // Deduplicate by id (a transfer could appear in both if branchId matches both src and dst)
          const seen = new Set<number>();
          const unique = allExternal.filter(t => { if (seen.has(t.id)) return false; seen.add(t.id); return true; });

          this.outgoingTransfers = unique.filter(t => t.sourceBranchId === branchId);
          this.incomingTransfers = unique.filter(t => t.destinationBranchId === branchId);
          this.loading = false;
        },
        error: () => {
          this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل تحويلات الفروع' });
          this.loading = false;
        }
      });
    } else {
      // Admin without branch — show all external transfers
      forkJoin({
        push: this.transferService.getAll({ transferType: 5 }),
        pull: this.transferService.getAll({ transferType: 3 })
      }).subscribe({
        next: (res) => {
          const allExternal = [...res.push, ...res.pull];
          this.outgoingTransfers = allExternal;
          this.incomingTransfers = allExternal;
          this.loading = false;
        },
        error: () => {
          this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'فشل في تحميل التحويلات' });
          this.loading = false;
        }
      });
    }
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
      case 'dispatched': case '3': return 'في الطريق';
      case 'received': case '4': return 'مستلم';
      case 'rejected': case '5': return 'مرفوض';
      default: return String(status);
    }
  }

  viewDetails(transfer: StockTransferDto) {
    this.selectedTransfer = transfer;
    this.displayDetailDialog = true;
  }

  isExternalPush(tt: any): boolean {
    return String(tt).toLowerCase() === 'external' || Number(tt) === 5;
  }

  isBranchRequest(tt: any): boolean {
    return String(tt).toLowerCase() === 'branchrequest' || Number(tt) === 3;
  }

  canApprove(transfer: StockTransferDto): boolean {
    if (!this.isStatus(transfer.status, 'Requested')) return false;
    const branchId = this.currentUserBranchId;
    // Admin (no branch) can approve anything pending
    if (!branchId) return true;

    // External Push (type=5): the DESTINATION branch approves
    if (this.isExternalPush(transfer.transferType)) {
      return transfer.destinationBranchId === branchId;
    }
    // BranchRequest Pull (type=3): the SOURCE branch approves
    if (this.isBranchRequest(transfer.transferType)) {
      return transfer.sourceBranchId === branchId;
    }
    return false;
  }

  canDispatch(transfer: StockTransferDto): boolean {
    if (!this.isStatus(transfer.status, 'Approved')) return false;
    const branchId = this.currentUserBranchId;
    // Admin can dispatch anything
    if (!branchId) return true;
    // Only the SOURCE branch ships
    return transfer.sourceBranchId === branchId;
  }

  canReceive(transfer: StockTransferDto): boolean {
    if (!this.isStatus(transfer.status, 'Dispatched')) return false;
    const branchId = this.currentUserBranchId;
    // Admin can receive anything
    if (!branchId) return true;
    // Only the DESTINATION branch receives
    return transfer.destinationBranchId === branchId;
  }

  canDelete(transfer: StockTransferDto): boolean {
    // Can cancel only if Requested AND the current user is the requester
    return this.isStatus(transfer.status, 'Requested') && transfer.requestedByUserId === this.currentUserId;
  }

  getTransferTypeLabel(transferType: any): string {
    switch (Number(transferType)) {
      case 3: return 'طلب (Pull)';
      case 4: return 'داخلي';
      case 5: return 'إرسال (Push)';
      default: return String(transferType);
    }
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
      this.messageService.add({ severity: 'error', summary: 'خطأ', detail: 'يرجى اختيار مستودع الاستلام' });
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
