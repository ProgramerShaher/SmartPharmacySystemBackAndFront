import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { ShiftService } from '../../../../core/services/shift.service';
import { ShiftDetailsDto, ShiftSummaryDto } from '../../../../core/models';

@Component({
  selector: 'app-shift-details',
  templateUrl: './shift-details.component.html',
  styleUrls: ['./shift-details.component.scss']
})
export class ShiftDetailsComponent implements OnInit, OnChanges {
  @Input() shiftId!: number;
  
  details: ShiftDetailsDto | null = null;
  loading: boolean = false;
  error: string = '';

  constructor(private shiftService: ShiftService) {}

  ngOnInit(): void {
    if (this.shiftId) {
      this.loadDetails();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['shiftId'] && !changes['shiftId'].firstChange) {
      this.loadDetails();
    }
  }

  loadDetails(): void {
    this.loading = true;
    this.error = '';
    this.shiftService.getDetails(this.shiftId).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.details = res.data;
        } else {
          this.error = res.message || 'فشل تحميل تفاصيل الوردية';
        }
        this.loading = false;
      },
      error: (err) => {
        this.error = 'حدث خطأ في الاتصال بالخادم';
        this.loading = false;
      }
    });
  }
}
