import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

export interface KpiCardData {
  title: string;
  value: number;
  icon: string;
  gradient: 'green' | 'blue' | 'orange' | 'red' | 'purple';
  comparison?: {
    period: string;
    change: number; // percentage
  };
  sparklineData?: number[];
  route?: string;
}

@Component({
  selector: 'app-kpi-card',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div 
      class="kpi-card" 
      [ngClass]="'gradient-' + data.gradient"
      [class.clickable]="data.route"
      (click)="handleClick()">
      
      <!-- Header -->
      <div class="card-header">
        <div class="icon-wrapper">
          <i [class]="'pi ' + data.icon"></i>
        </div>
        <span class="card-title">{{ data.title }}</span>
      </div>

      <!-- Main Value -->
      <div class="card-value">
        {{ data.value | number:'1.0-0' }}
        <span class="currency">IQD</span>
      </div>

      <!-- Comparison -->
      <div class="card-comparison" *ngIf="data.comparison">
        <span 
          class="change-indicator"
          [class.positive]="data.comparison.change > 0"
          [class.negative]="data.comparison.change < 0">
          <i [class]="data.comparison.change > 0 ? 'pi pi-arrow-up' : 'pi pi-arrow-down'"></i>
          {{ data.comparison.change | number:'1.1-1' }}%
        </span>
        <span class="comparison-period">vs {{ data.comparison.period }}</span>
      </div>

      <!-- Sparkline Mini Chart -->
      <div class="sparkline-container" *ngIf="data.sparklineData && data.sparklineData.length > 0">
        <svg class="sparkline" viewBox="0 0 100 30" preserveAspectRatio="none">
          <polyline
            [attr.points]="generateSparklinePoints()"
            fill="none"
            stroke="currentColor"
            stroke-width="2"
            stroke-linecap="round"
            stroke-linejoin="round"
          />
        </svg>
      </div>

      <!-- Navigation Hint -->
      <div class="nav-hint" *ngIf="data.route">
        <i class="pi pi-arrow-left"></i>
        <span>عرض التفاصيل</span>
      </div>
    </div>
  `,
  styles: [`
    .kpi-card {
      position: relative;
      padding: 1.5rem;
      border-radius: 16px;
      background: var(--surface-card);
      border: 1px solid var(--surface-border);
      overflow: hidden;
      transition: all 0.3s cubic-bezier(0.4, 0, 0.2, 1);
      height: 100%;
      display: flex;
      flex-direction: column;
      gap: 0.75rem;
    }

    .kpi-card.clickable {
      cursor: pointer;
    }

    .kpi-card.clickable:hover {
      transform: translateY(-4px);
      box-shadow: 0 12px 24px rgba(0, 0, 0, 0.15);
      border-color: rgba(var(--primary-rgb), 0.3);
    }

    /* Gradient Backgrounds */
    .kpi-card::before {
      content: '';
      position: absolute;
      top: 0;
      right: 0;
      width: 100%;
      height: 100%;
      opacity: 0.05;
      z-index: 0;
      transition: opacity 0.3s ease;
    }

    .kpi-card.clickable:hover::before {
      opacity: 0.1;
    }

    .gradient-green::before {
      background: linear-gradient(135deg, #10b981, #059669);
    }

    .gradient-blue::before {
      background: linear-gradient(135deg, #3b82f6, #1d4ed8);
    }

    .gradient-orange::before {
      background: linear-gradient(135deg, #f59e0b, #d97706);
    }

    .gradient-red::before {
      background: linear-gradient(135deg, #ef4444, #dc2626);
    }

    .gradient-purple::before {
      background: linear-gradient(135deg, #8b5cf6, #7c3aed);
    }

    /* Header */
    .card-header {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      position: relative;
      z-index: 1;
    }

    .icon-wrapper {
      width: 40px;
      height: 40px;
      border-radius: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.25rem;
      position: relative;
    }

    .gradient-green .icon-wrapper {
      background: linear-gradient(135deg, #10b981, #059669);
      color: white;
      box-shadow: 0 4px 12px rgba(16, 185, 129, 0.3);
    }

    .gradient-blue .icon-wrapper {
      background: linear-gradient(135deg, #3b82f6, #1d4ed8);
      color: white;
      box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
    }

    .gradient-orange .icon-wrapper {
      background: linear-gradient(135deg, #f59e0b, #d97706);
      color: white;
      box-shadow: 0 4px 12px rgba(245, 158, 11, 0.3);
    }

    .gradient-red .icon-wrapper {
      background: linear-gradient(135deg, #ef4444, #dc2626);
      color: white;
      box-shadow: 0 4px 12px rgba(239, 68, 68, 0.3);
    }

    .gradient-purple .icon-wrapper {
      background: linear-gradient(135deg, #8b5cf6, #7c3aed);
      color: white;
      box-shadow: 0 4px 12px rgba(139, 92, 246, 0.3);
    }

    .card-title {
      font-size: 0.875rem;
      font-weight: 600;
      color: var(--text-color-secondary);
      position: relative;
      z-index: 1;
    }

    /* Main Value */
    .card-value {
      font-size: 2rem;
      font-weight: 800;
      color: var(--text-color);
      line-height: 1;
      position: relative;
      z-index: 1;
      direction: ltr;
      text-align: left;
    }

    [dir="rtl"] .card-value {
      text-align: right;
    }

    .card-value .currency {
      font-size: 0.875rem;
      font-weight: 600;
      color: var(--text-color-secondary);
      margin-right: 0.5rem;
    }

    /* Comparison */
    .card-comparison {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.875rem;
      position: relative;
      z-index: 1;
    }

    .change-indicator {
      display: flex;
      align-items: center;
      gap: 0.25rem;
      font-weight: 700;
      padding: 0.25rem 0.5rem;
      border-radius: 6px;
    }

    .change-indicator.positive {
      color: #059669;
      background: rgba(16, 185, 129, 0.1);
    }

    .change-indicator.negative {
      color: #dc2626;
      background: rgba(239, 68, 68, 0.1);
    }

    .change-indicator i {
      font-size: 0.75rem;
    }

    .comparison-period {
      color: var(--text-color-secondary);
      font-size: 0.8125rem;
    }

    /* Sparkline */
    .sparkline-container {
      height: 30px;
      margin-top: auto;
      position: relative;
      z-index: 1;
    }

    .sparkline {
      width: 100%;
      height: 100%;
      opacity: 0.6;
    }

    .gradient-green .sparkline {
      color: #10b981;
    }

    .gradient-blue .sparkline {
      color: #3b82f6;
    }

    .gradient-orange .sparkline {
      color: #f59e0b;
    }

    .gradient-red .sparkline {
      color: #ef4444;
    }

    .gradient-purple .sparkline {
      color: #8b5cf6;
    }

    /* Navigation Hint */
    .nav-hint {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.8125rem;
      color: var(--primary-color);
      font-weight: 600;
      opacity: 0;
      transition: opacity 0.3s ease;
      position: relative;
      z-index: 1;
    }

    .kpi-card.clickable:hover .nav-hint {
      opacity: 1;
    }

    .nav-hint i {
      font-size: 0.75rem;
    }

    /* Dark Mode */
    [data-theme="dark"] .kpi-card {
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
    }

    [data-theme="dark"] .kpi-card.clickable:hover {
      box-shadow: 0 12px 24px rgba(0, 0, 0, 0.5);
    }

    /* RTL */
    [dir="rtl"] .nav-hint {
      flex-direction: row-reverse;
    }

    [dir="rtl"] .nav-hint i {
      transform: scaleX(-1);
    }

    /* Responsive */
    @media (max-width: 768px) {
      .kpi-card {
        padding: 1.25rem;
      }

      .card-value {
        font-size: 1.75rem;
      }

      .icon-wrapper {
        width: 36px;
        height: 36px;
        font-size: 1.125rem;
      }
    }
  `]
})
export class KpiCardComponent {
  @Input() data!: KpiCardData;
  @Output() cardClick = new EventEmitter<string>();

  constructor(private router: Router) {}

  handleClick(): void {
    if (this.data.route) {
      this.cardClick.emit(this.data.route);
      this.router.navigate([this.data.route]);
    }
  }

  generateSparklinePoints(): string {
    if (!this.data.sparklineData || this.data.sparklineData.length === 0) {
      return '';
    }

    const data = this.data.sparklineData;
    const max = Math.max(...data);
    const min = Math.min(...data);
    const range = max - min || 1;

    return data.map((value, index) => {
      const x = (index / (data.length - 1)) * 100;
      const y = 30 - ((value - min) / range) * 30;
      return `${x},${y}`;
    }).join(' ');
  }
}
