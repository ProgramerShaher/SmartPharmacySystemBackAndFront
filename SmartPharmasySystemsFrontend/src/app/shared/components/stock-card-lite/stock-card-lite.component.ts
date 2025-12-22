import { Component, OnInit, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { InventoryMovement } from '../../../core/models';
import { InventoryService } from '../../../features/inventory/services/inventory.service';

@Component({
  selector: 'app-stock-card-lite',
  standalone: true,
  imports: [CommonModule, TableModule],
  template: `
    <p-table
      [value]="movements"
      [loading]="loading"
      styleClass="p-datatable-sm p-datatable-striped shadow-1 border-round overflow-hidden"
    >
      <ng-template pTemplate="header">
        <tr>
          <th class="text-right">تاريخ الحركة</th>
          <th class="text-center">النوع</th>
          <th class="text-center">انتاج</th>
          <th class="text-center">استهلاك</th>
          <th class="text-center">رصيد قبل</th>
          <th class="text-center">رصيد بعد</th>
          <th class="text-right">ملاحظات</th>
        </tr>
      </ng-template>
      <ng-template pTemplate="body" let-m>
        <tr>
          <td class="text-right font-semibold">
            {{ m.movementDate | date : 'dd/MM/yyyy HH:mm' }}
          </td>
          <td class="text-center">
            <span
              class="px-2 py-1 border-round text-xs font-bold bg-gray-100"
              >{{ m.movementType }}</span
            >
          </td>
          <td class="text-center text-green-600 font-bold">
            {{ m.quantityIn > 0 ? m.quantityIn : '---' }}
          </td>
          <td class="text-center text-red-600 font-bold">
            {{ m.quantityOut > 0 ? m.quantityOut : '---' }}
          </td>
          <td class="text-center font-mono">{{ m.quantityBefore }}</td>
          <td class="text-center font-extrabold text-primary">
            {{ m.quantityAfter }}
          </td>
          <td class="text-sm text-secondary">{{ m.notes }}</td>
        </tr>
      </ng-template>
    </p-table>
  `,
})
export class StockCardLiteComponent implements OnInit {
  @Input() batchId!: number;
  movements: InventoryMovement[] = [];
  loading = true;

  constructor(private inventoryService: InventoryService) {}

  ngOnInit() {
    if (this.batchId) {
      this.inventoryService.getStockCard(this.batchId).subscribe({
        next: (data) => {
          this.movements = data;
          this.loading = false;
        },
        error: () => (this.loading = false),
      });
    }
  }
}
