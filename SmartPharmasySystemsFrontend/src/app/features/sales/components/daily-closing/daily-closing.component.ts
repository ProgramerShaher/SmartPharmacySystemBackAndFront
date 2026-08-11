import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-daily-closing',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="p-4 bg-white border-round shadow-1 text-center">
      <i class="pi pi-lock text-6xl text-primary mb-3"></i>
      <h2>إغلاق الورديات (Daily Closing)</h2>
      <p class="text-600 mt-2">جاري العمل على برمجة شاشة إغلاق الورديات. قريباً جداً ستكون جاهزة! 🚀</p>
    </div>
  `
})
export class DailyClosingComponent {}
