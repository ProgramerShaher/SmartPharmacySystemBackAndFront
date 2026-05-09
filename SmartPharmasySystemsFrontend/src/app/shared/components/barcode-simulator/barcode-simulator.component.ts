import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DialogModule } from 'primeng/dialog';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { TransactionType } from '../../../core/models/barcode.interface';

@Component({
  selector: 'app-barcode-simulator',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    DropdownModule
  ],
  templateUrl: './barcode-simulator.component.html',
  styleUrls: ['./barcode-simulator.component.scss']
})
export class BarcodeSimulatorComponent {
  @Input() visible = false;
  @Input() currentTransactionType: TransactionType = TransactionType.Sale;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() onScan = new EventEmitter<string>();

  barcodeValue = '';

  transactionTypes = [
    { label: 'بيع', value: TransactionType.Sale },
    { label: 'شراء', value: TransactionType.Purchase },
    { label: 'مرتجع', value: TransactionType.Return }
  ];

  simulateScan() {
    if (this.barcodeValue.trim()) {
      this.onScan.emit(this.barcodeValue.trim());
      this.barcodeValue = '';
      this.close();
    }
  }

  close() {
    this.visible = false;
    this.visibleChange.emit(false);
  }
}
