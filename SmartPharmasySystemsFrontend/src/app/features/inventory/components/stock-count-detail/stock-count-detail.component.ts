import { Component, OnInit, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators, FormsModule } from '@angular/forms';
import { StockCountService } from '../../services/stock-count.service';
import { MedicineService } from '../../services/medicine.service';
import { 
  StockCountHeaderDto, 
  StockCountItemDto, 
  StockCountStatus,
  StockCountType,
  Medicine
} from '../../../../core/models';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { DropdownModule } from 'primeng/dropdown';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';

@Component({
  selector: 'app-stock-count-detail',
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
    TagModule,
    TooltipModule
  ],
  templateUrl: './stock-count-detail.component.html',
  styleUrls: ['./stock-count-detail.component.scss'],
  providers: [DatePipe]
})
export class StockCountDetailComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly stockCountService = inject(StockCountService);
  private readonly medicineService = inject(MedicineService);
  private readonly fb = inject(FormBuilder);

  headerId!: number;
  header!: StockCountHeaderDto;
  items: StockCountItemDto[] = [];
  
  isLoading = true;
  isAddingItem = false;
  
  StockCountStatus = StockCountStatus;
  StockCountType = StockCountType;

  // Add Item Form
  addItemForm: FormGroup;
  medicines: Medicine[] = [];

  constructor() {
    this.addItemForm = this.fb.group({
      medicineId: ['', Validators.required],
      batchNumber: ['', Validators.required],
      physicalQuantity: [0, [Validators.required, Validators.min(0)]],
      varianceReason: ['']
    });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.headerId = +id;
      this.loadData();
    } else {
      this.router.navigate(['/inventory/stock-counts']);
    }
  }

  loadData(): void {
    this.isLoading = true;
    this.stockCountService.getHeaderById(this.headerId).subscribe({
      next: (headerData) => {
        this.header = headerData;
        this.loadItems();
      },
      error: () => {
        this.isLoading = false;
        // Handle error (show toast etc)
      }
    });
  }

  loadItems(): void {
    this.stockCountService.getItems(this.headerId).subscribe({
      next: (itemsData) => {
        // We will bind ngModel directly to these items for inline editing of physicalQuantity
        this.items = itemsData;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  loadMedicines(): void {
    if (this.medicines.length === 0) {
      this.medicineService.getAll().subscribe({
        next: (res) => {
          this.medicines = res.items;
        }
      });
    }
  }

  toggleAddItem(): void {
    this.isAddingItem = !this.isAddingItem;
    if (this.isAddingItem) {
      this.loadMedicines();
      this.addItemForm.reset({ physicalQuantity: 0 });
    }
  }

  onAddItemSubmit(): void {
    if (this.addItemForm.invalid) return;

    const dto = this.addItemForm.value;
    this.stockCountService.addItem(this.headerId, dto).subscribe({
      next: () => {
        this.toggleAddItem();
        this.loadItems();
      }
    });
  }

  saveItem(item: StockCountItemDto): void {
    if (item.physicalQuantity === null || item.physicalQuantity === undefined) return;
    
    this.stockCountService.updateItem(this.headerId, {
      medicineId: item.medicineId,
      batchNumber: item.batchNumber,
      physicalQuantity: item.physicalQuantity,
      varianceReason: item.varianceReason || undefined
    }).subscribe({
      next: () => {
        // Reload to get updated variance/value from backend
        this.loadItems();
      }
    });
  }

  deleteItem(item: StockCountItemDto): void {
    if (confirm('هل أنت متأكد من حذف هذا الصنف من الجرد؟')) {
      this.stockCountService.deleteItem(this.headerId, item.medicineId, item.batchNumber).subscribe({
        next: () => {
          this.loadItems();
        }
      });
    }
  }

  submitForApproval(): void {
    if (confirm('هل أنت متأكد من إرسال هذا الجرد للاعتماد؟ لا يمكن تعديل الكميات بعد ذلك.')) {
      this.stockCountService.submitForApproval(this.headerId).subscribe({
        next: (res) => {
          this.header = res;
        }
      });
    }
  }

  approve(): void {
    // In a real app, you get the current user ID from AuthService
    const currentUserId = 1; 
    
    if (confirm('هل أنت متأكد من اعتماد وتسوية هذا الجرد؟ سيتم تعديل أرصدة المخزون ولن يمكن التراجع.')) {
      this.stockCountService.approve(this.headerId, currentUserId).subscribe({
        next: (res) => {
          this.header = res;
        }
      });
    }
  }

  canEdit(): boolean {
    if (!this.header) return false;
    const status = this.header.status as any;
    return status === StockCountStatus.Draft || status === StockCountStatus.InProgress
        || status === 'Draft' || status === 'InProgress';
  }

  isDraft(): boolean {
    const s = this.header?.status as any;
    return s === StockCountStatus.Draft || s === 'Draft';
  }

  isInProgress(): boolean {
    const s = this.header?.status as any;
    return s === StockCountStatus.InProgress || s === 'InProgress';
  }

  isPendingApproval(): boolean {
    const s = this.header?.status as any;
    return s === StockCountStatus.PendingApproval || s === 'PendingApproval';
  }

  isApproved(): boolean {
    const s = this.header?.status as any;
    return s === StockCountStatus.Approved || s === 'Approved';
  }

  isClosed(): boolean {
    const s = this.header?.status as any;
    return s === StockCountStatus.Closed || s === 'Closed';
  }
}
