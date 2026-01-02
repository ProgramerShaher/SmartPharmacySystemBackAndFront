import { Component, OnInit, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TableModule } from 'primeng/table';
import { InventoryMovement } from '../../../core/models';
import { InventoryService } from '../../../features/inventory/services/inventory.service';

@Component({
  selector: 'app-stock-card-lite',
  standalone: true,
  imports: [CommonModule, TableModule],
  templateUrl: './stock-card-lite.component.html',
  styleUrls: ['./stock-card-lite.component.scss']
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
