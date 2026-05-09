import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { InventoryService } from '../../../features/inventory/services/inventory.service';
import { StockCardDto } from '../../../core/models';

@Component({
  selector: 'app-stock-card-lite',
  standalone: true,
  imports: [CommonModule, TableModule],
  templateUrl: './stock-card-lite.component.html',
  styleUrls: ['./stock-card-lite.component.scss']
})
export class StockCardLiteComponent implements OnInit {
  @Input() batchId!: number;
  movements: StockCardDto[] = [];
  loading = true;

  constructor(private inventoryService: InventoryService) { }

  ngOnInit() {
    if (this.batchId) {
      // First get batch to find medicineId
      this.inventoryService.getBatchById(this.batchId).subscribe({
        next: (batch: any) => {
          if (batch && batch.medicineId) {
            this.inventoryService.getStockCard(batch.medicineId, this.batchId).subscribe({
              next: (data: any) => {
                this.movements = data;
                this.loading = false;
              },
              error: () => (this.loading = false)
            });
          } else {
            this.loading = false;
          }
        },
        error: () => (this.loading = false)
      });
    }
  }

  getMovementTypeLabel(type: any): string {
    const typeStr = String(type).toLowerCase();
    if (typeStr.includes('purchase')) return 'شراء / توريد';
    if (typeStr.includes('sale')) return 'بيع مباشر';
    if (typeStr.includes('return')) return 'مرتجع';
    if (typeStr.includes('adjustment')) return 'تعديل جردي';
    if (typeStr.includes('damage')) return 'تالف';
    if (typeStr.includes('opening')) return 'رصيد أول';
    if (typeStr.includes('stockin') || typeStr === 'in') return 'إضافة مخزنية';
    if (typeStr.includes('stockout') || typeStr === 'out') return 'صرف مخزني';
    return type || 'غير معروف';
  }
}
