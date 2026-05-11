import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { OnlineOrderService, OnlineOrder } from '../../core/services/online-order.service';
import { ToastrService } from 'ngx-toastr';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-online-orders',
  standalone: true,
  imports: [CommonModule, TableModule, ButtonModule],
  templateUrl: './online-orders.component.html',
  styleUrls: []
})
export class OnlineOrdersComponent implements OnInit {
  orders: OnlineOrder[] = [];
  loading = true;

  constructor(
    private orderService: OnlineOrderService,
    private toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.loading = true;
    this.orderService.getAllOrders().subscribe({
      next: (res) => {
        this.orders = res.data;
        this.loading = false;
      },
      error: (err) => {
        this.toastr.error('خطأ في جلب الطلبات');
        this.loading = false;
      }
    });
  }

  updateStatus(order: OnlineOrder, status: number): void {
    this.orderService.updateOrderStatus(order.id, status).subscribe({
      next: () => {
        this.toastr.success('تم تحديث حالة الطلب');
        this.loadOrders();
      },
      error: () => this.toastr.error('خطأ في التحديث')
    });
  }

  getStatusClass(code: number): string {
    switch (code) {
      case 1: return 'badge-warning'; // Pending
      case 2: return 'badge-info';    // Preparing
      case 3: return 'badge-primary'; // Out
      case 4: return 'badge-success'; // Delivered
      case 5:
      case 6: return 'badge-danger';  // Rejected
      default: return 'badge-secondary';
    }
  }
}
