import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * مكون بطاقة الإحصائيات القابل لإعادة الاستخدام
 * Reusable Stat Card Component with Glassmorphism Effect
 * 
 * @example
 * <app-stat-card
 *   title="رصيد الصيدلية"
 *   [value]="balance()"
 *   icon="pi-wallet"
 *   [trend]="+12.5"
 *   color="emerald"
 * ></app-stat-card>
 */
@Component({
  selector: 'app-stat-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="stat-card h-full relative overflow-hidden" [ngClass]="'stat-card-' + color">
      <!-- محتوى البطاقة / Card Content -->
      <div class="flex justify-content-between mb-3 relative z-1">
        <div class="flex-1">
          <span class="block text-500 font-medium mb-2 text-sm">{{ title }}</span>
          <div class="text-color font-bold text-3xl">{{ formattedValue }}</div>
        </div>
        <div class="icon-container" [ngClass]="'icon-bg-' + color">
          <i [class]="'pi ' + icon + ' text-2xl'"></i>
        </div>
      </div>

      <!-- مؤشر الاتجاه / Trend Indicator -->
      <div class="flex align-items-center gap-2" *ngIf="trend !== undefined">
        <span 
          class="font-medium flex align-items-center gap-1"
          [ngClass]="trend >= 0 ? 'text-green-500' : 'text-red-500'"
        >
          <i [class]="'pi text-xs ' + (trend >= 0 ? 'pi-arrow-up' : 'pi-arrow-down')"></i>
          {{ Math.abs(trend) }}%
        </span>
        <span class="text-500 text-sm">{{ subtitle }}</span>
      </div>
      <div *ngIf="trend === undefined && subtitle" class="text-500 text-sm">
        {{ subtitle }}
      </div>

      <!-- عنصر زخرفي / Decorative Element -->
      <div class="decor-circle"></div>
    </div>
  `,
  styles: [`
    .stat-card {
      padding: 1.5rem;
      border-radius: 16px;
      background: var(--surface-card);
      border: 1px solid var(--surface-border);
      box-shadow: 0 8px 32px rgba(0, 0, 0, 0.08);
      transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
      backdrop-filter: blur(10px);
    }

    .stat-card:hover {
      transform: translateY(-4px);
      box-shadow: 0 12px 48px rgba(0, 0, 0, 0.12);
    }

    /* أيقونة البطاقة / Card Icon */
    .icon-container {
      width: 3.5rem;
      height: 3.5rem;
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      flex-shrink: 0;
    }

    /* ألوان الأيقونات / Icon Colors */
    .icon-bg-emerald {
      background: linear-gradient(135deg, rgba(16, 185, 129, 0.1), rgba(16, 185, 129, 0.2));
      color: #10b981;
    }

    .icon-bg-blue {
      background: linear-gradient(135deg, rgba(14, 165, 233, 0.1), rgba(14, 165, 233, 0.2));
      color: #0ea5e9;
    }

    .icon-bg-orange {
      background: linear-gradient(135deg, rgba(249, 115, 22, 0.1), rgba(249, 115, 22, 0.2));
      color: #f97316;
    }

    .icon-bg-purple {
      background: linear-gradient(135deg, rgba(168, 85, 247, 0.1), rgba(168, 85, 247, 0.2));
      color: #a855f7;
    }

    .icon-bg-red {
      background: linear-gradient(135deg, rgba(239, 68, 68, 0.1), rgba(239, 68, 68, 0.2));
      color: #ef4444;
    }

    /* دائرة زخرفية / Decorative Circle */
    .decor-circle {
      position: absolute;
      width: 6rem;
      height: 6rem;
      border-radius: 50%;
      opacity: 0.08;
      right: -20px;
      bottom: -20px;
      transition: all 0.3s ease;
    }

    .stat-card-emerald .decor-circle {
      background: #10b981;
    }

    .stat-card-blue .decor-circle {
      background: #0ea5e9;
    }

    .stat-card-orange .decor-circle {
      background: #f97316;
    }

    .stat-card-purple .decor-circle {
      background: #a855f7;
    }

    .stat-card-red .decor-circle {
      background: #ef4444;
    }

    .stat-card:hover .decor-circle {
      transform: scale(1.2);
      opacity: 0.12;
    }

    /* تأثير الظهور التدريجي / Fade-in Animation */
    @keyframes fadeInUp {
      from {
        opacity: 0;
        transform: translateY(20px);
      }
      to {
        opacity: 1;
        transform: translateY(0);
      }
    }

    .stat-card {
      animation: fadeInUp 0.5s ease-out;
    }
  `]
})
export class StatCardComponent {
  @Input() title: string = '';
  @Input() value: number | string = 0;
  @Input() icon: string = 'pi-chart-line';
  @Input() trend?: number; // Percentage change (e.g., +12.5 or -5.3)
  @Input() subtitle: string = '';
  @Input() color: 'emerald' | 'blue' | 'orange' | 'purple' | 'red' = 'emerald';
  @Input() format: 'currency' | 'number' | 'text' = 'number';

  // Expose Math for template
  Math = Math;

  get formattedValue(): string {
    if (this.format === 'text') {
      return this.value.toString();
    }

    if (typeof this.value === 'number') {
      if (this.format === 'currency') {
        return new Intl.NumberFormat('ar-YE', {
          style: 'decimal',
          minimumFractionDigits: 0,
          maximumFractionDigits: 2
        }).format(this.value) + ' ريال';
      }
      return new Intl.NumberFormat('ar-YE').format(this.value);
    }

    return this.value.toString();
  }
}
