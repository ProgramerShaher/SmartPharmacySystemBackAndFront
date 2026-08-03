import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormsModule } from '@angular/forms';
import { StockCountService } from '../../services/stock-count.service';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { CalendarModule } from 'primeng/calendar';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import {
  StockCountHeaderDto,
  StockCountStatus,
  StockCountType,
  WarehouseDto
} from '../../../../core/models';
import { BranchService } from '../../../branches/services/branch.service';

@Component({
  selector: 'app-stock-count-list',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule, 
    ReactiveFormsModule,
    FormsModule,
    TableModule,
    ButtonModule,
    InputTextModule,
    DropdownModule,
    CalendarModule,
    TagModule,
    TooltipModule
  ],
  templateUrl: './stock-count-list.component.html',
  styleUrls: ['./stock-count-list.component.scss'],
  providers: [DatePipe]
})
export class StockCountListComponent implements OnInit {
  private readonly stockCountService = inject(StockCountService);
  private readonly branchService = inject(BranchService);
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);

  stockCounts: StockCountHeaderDto[] = [];
  warehouses: WarehouseDto[] = [];
  filterForm: FormGroup;
  isLoading = false;

  StockCountStatus = StockCountStatus;
  StockCountType = StockCountType;

  statusOptions = [
    { label: 'الكل', value: null },
    { label: 'مسودة', value: StockCountStatus.Draft },
    { label: 'قيد الجرد', value: StockCountStatus.InProgress },
    { label: 'بانتظار الاعتماد', value: StockCountStatus.PendingApproval },
    { label: 'معتمد', value: StockCountStatus.Approved },
    { label: 'مغلق', value: StockCountStatus.Closed }
  ];

  constructor() {
    this.filterForm = this.fb.group({
      warehouseId: [''],
      status: [''],
      from: [''],
      to: ['']
    });
  }

  ngOnInit(): void {
    this.loadWarehouses();
    this.loadStockCounts();
  }

  loadWarehouses(): void {
    // Assuming branchService or warehouseService has a method to get warehouses
    // To simplify, we get branches and their warehouses or call warehouse API
    // If WarehouseService exists, we could use it. Using branchService for now or a mock until service confirmed
  }

  loadStockCounts(): void {
    this.isLoading = true;
    const filters = this.filterForm.value;
    this.stockCountService.getAllHeaders(
      filters.warehouseId || undefined,
      filters.status || undefined,
      filters.from || undefined,
      filters.to || undefined
    ).subscribe({
      next: (data) => {
        this.stockCounts = data;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  onFilter(): void {
    this.loadStockCounts();
  }

  resetFilter(): void {
    this.filterForm.reset();
    this.loadStockCounts();
  }

  viewDetails(id: number): void {
    this.router.navigate(['/inventory/stock-counts', id]);
  }

  // Helpers to handle enum comparison when backend sends strings
  isStatus(item: StockCountHeaderDto, statusValue: StockCountStatus, statusString: string): boolean {
      return item.status === statusValue || (item.status as any) === statusString;
  }

  isType(item: StockCountHeaderDto, typeValue: StockCountType, typeString: string): boolean {
      return item.countType === typeValue || (item.countType as any) === typeString;
  }

  canEditOrDelete(item: StockCountHeaderDto): boolean {
      return this.isStatus(item, StockCountStatus.Draft, 'Draft') || 
             this.isStatus(item, StockCountStatus.InProgress, 'InProgress');
  }

  editStockCount(id: number): void {
      // Editing a stock count is done in the details page (modifying items)
      this.router.navigate(['/inventory/stock-counts', id]);
  }

  deleteStockCount(item: StockCountHeaderDto): void {
      if (confirm(`هل أنت متأكد من حذف أمر الجرد رقم ${item.countCode}؟`)) {
          this.stockCountService.deleteHeader(item.id).subscribe({
              next: () => {
                  this.loadStockCounts();
              },
              error: (err) => {
                  console.error('Error deleting stock count:', err);
                  alert('حدث خطأ أثناء محاولة حذف أمر الجرد.');
              }
          });
      }
  }
}
