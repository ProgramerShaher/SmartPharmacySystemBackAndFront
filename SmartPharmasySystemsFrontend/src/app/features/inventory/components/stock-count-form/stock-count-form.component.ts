import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { StockCountService } from '../../services/stock-count.service';
import { WarehouseService } from '../../../warehouses/services/warehouse.service';
import { StockCountType, CreateStockCountHeaderDto, WarehouseDto } from '../../../../core/models';
import { DropdownModule } from 'primeng/dropdown';
import { ButtonModule } from 'primeng/button';
import { InputTextareaModule } from 'primeng/inputtextarea';
import { RadioButtonModule } from 'primeng/radiobutton';

@Component({
  selector: 'app-stock-count-form',
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule, 
    FormsModule,
    RouterModule,
    DropdownModule,
    ButtonModule,
    InputTextareaModule,
    RadioButtonModule
  ],
  templateUrl: './stock-count-form.component.html',
  styleUrls: ['./stock-count-form.component.scss']
})
export class StockCountFormComponent implements OnInit {
  form: FormGroup;
  isSubmitting = false;

  warehouses: WarehouseDto[] = [];

  StockCountType = StockCountType;

  constructor(
    private fb: FormBuilder,
    private stockCountService: StockCountService,
    private warehouseService: WarehouseService,
    private router: Router
  ) {
    this.form = this.fb.group({
      warehouseId: ['', Validators.required],
      countType: [StockCountType.Annual, Validators.required],
      notes: ['']
    });
  }

  ngOnInit(): void {
    this.warehouseService.getAll().subscribe(res => {
      this.warehouses = res;
    });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const dto: CreateStockCountHeaderDto = {
        ...this.form.value,
        snapshotAt: new Date().toISOString()
    };
    
    this.stockCountService.createHeader(dto).subscribe({
      next: (res) => {
        this.isSubmitting = false;
        // Navigate to details page to add items
        this.router.navigate(['/inventory/stock-counts', res.id]);
      },
      error: () => {
        this.isSubmitting = false;
      }
    });
  }
}
