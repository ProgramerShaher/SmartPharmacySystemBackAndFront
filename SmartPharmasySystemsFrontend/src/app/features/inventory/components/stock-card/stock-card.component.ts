import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { InventoryMovementService } from '../../services/inventory-movement.service';
import { StockCardDto } from '../../../../core/models';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { TagModule } from 'primeng/tag';
import { ToolbarModule } from 'primeng/toolbar';
import { MessageService } from 'primeng/api';

@Component({
  selector: 'app-stock-card',
  standalone: true,
  imports: [
    CommonModule,
    TableModule,
    ButtonModule,
    CardModule,
    TagModule,
    ToolbarModule
  ],
  templateUrl: './stock-card.component.html',
  styleUrls: ['./stock-card.component.scss']
})
export class StockCardComponent implements OnInit {
  stockCard: StockCardDto[] = [];
  loading = false;
  medicineId: number | null = null;
  batchId: number | null = null;
  currentBalance = 0;

  constructor(
    private movementService: InventoryMovementService,
    private route: ActivatedRoute,
    private router: Router,
    private messageService: MessageService
  ) { }

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.medicineId = params['medicineId'] ? +params['medicineId'] : null;
      this.batchId = params['batchId'] ? +params['batchId'] : null;

      if (this.medicineId) {
        this.loadStockCard();
        this.loadBalance();
      }
    });
  }

  loadStockCard() {
    if (!this.medicineId) return;

    this.loading = true;
    this.movementService.getStockCard(this.medicineId, this.batchId || undefined).subscribe({
      next: (data) => {
        this.stockCard = data;
        this.loading = false;
      },
      error: (error) => {
        console.error('Error loading stock card:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل تحميل كرت الصنف'
        });
        this.loading = false;
      }
    });
  }

  loadBalance() {
    if (!this.medicineId) return;

    this.movementService.getCurrentBalance(this.medicineId, this.batchId || undefined).subscribe({
      next: (balance) => {
        this.currentBalance = balance;
      },
      error: (error) => {
        console.error('Error loading balance:', error);
      }
    });
  }

  getTotalIn(): number {
    return this.stockCard.reduce((sum, card) => sum + (card.quantityChange > 0 ? card.quantityChange : 0), 0);
  }

  getTotalOut(): number {
    return this.stockCard.reduce((sum, card) => sum + (card.quantityChange < 0 ? Math.abs(card.quantityChange) : 0), 0);
  }

  getMovementTypeSeverity(type: any): 'success' | 'info' | 'warning' | 'danger' | 'secondary' {
    // Basic mapping for StockCardDto movement types
    const typeStr = String(type).toLowerCase();
    if (typeStr.includes('purchase') || typeStr.includes('in')) return 'success';
    if (typeStr.includes('sale') || typeStr.includes('out') || typeStr.includes('damage')) return 'danger';
    if (typeStr.includes('return')) return 'info';
    if (typeStr.includes('adjustment')) return 'warning';
    return 'secondary';
  }

  goBack() {
    this.router.navigate(['/inventory/movements']);
  }
}
