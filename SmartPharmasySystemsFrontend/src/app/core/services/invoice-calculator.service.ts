import { Injectable } from '@angular/core';

export interface InvoiceItem {
  quantity: number;
  unitPrice: number;
  unitCost?: number;
  discount?: number;
  tax?: number;
}

export interface InvoiceCalculation {
  subtotal: number;
  totalDiscount: number;
  totalTax: number;
  grandTotal: number;
  totalCost?: number;
  totalProfit?: number;
}

@Injectable({
  providedIn: 'root'
})
export class InvoiceCalculatorService {

  /**
   * Calculate line item total
   */
  calculateLineTotal(item: InvoiceItem): number {
    const subtotal = item.quantity * item.unitPrice;
    const discount = item.discount || 0;
    const tax = item.tax || 0;
    
    return subtotal - discount + tax;
  }

  /**
   * Calculate line item profit (for sales)
   */
  calculateLineProfit(item: InvoiceItem): number {
    if (!item.unitCost) return 0;
    
    const revenue = this.calculateLineTotal(item);
    const cost = item.quantity * item.unitCost;
    
    return revenue - cost;
  }

  /**
   * Calculate invoice totals
   */
  calculateInvoiceTotals(items: InvoiceItem[]): InvoiceCalculation {
    let subtotal = 0;
    let totalDiscount = 0;
    let totalTax = 0;
    let totalCost = 0;
    let totalProfit = 0;

    items.forEach(item => {
      const itemSubtotal = item.quantity * item.unitPrice;
      subtotal += itemSubtotal;
      totalDiscount += item.discount || 0;
      totalTax += item.tax || 0;
      
      if (item.unitCost) {
        totalCost += item.quantity * item.unitCost;
        totalProfit += this.calculateLineProfit(item);
      }
    });

    const grandTotal = subtotal - totalDiscount + totalTax;

    return {
      subtotal,
      totalDiscount,
      totalTax,
      grandTotal,
      totalCost,
      totalProfit
    };
  }

  /**
   * Calculate profit margin percentage
   */
  calculateProfitMargin(revenue: number, cost: number): number {
    if (revenue === 0) return 0;
    return ((revenue - cost) / revenue) * 100;
  }

  /**
   * Validate stock availability
   */
  validateStockAvailability(requestedQty: number, availableQty: number): {
    isValid: boolean;
    message?: string;
  } {
    if (requestedQty <= 0) {
      return { isValid: false, message: 'الكمية يجب أن تكون أكبر من صفر' };
    }

    if (requestedQty > availableQty) {
      return { 
        isValid: false, 
        message: `الكمية المتاحة فقط ${availableQty}` 
      };
    }

    return { isValid: true };
  }

  /**
   * Format currency for display
   */
  formatCurrency(amount: number, currency: string = 'ر.ي'): string {
    return `${amount.toLocaleString('ar-YE', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} ${currency}`;
  }

  /**
   * Calculate discount percentage
   */
  calculateDiscountPercentage(originalPrice: number, discountedPrice: number): number {
    if (originalPrice === 0) return 0;
    return ((originalPrice - discountedPrice) / originalPrice) * 100;
  }

  /**
   * Apply discount percentage to price
   */
  applyDiscountPercentage(price: number, discountPercent: number): number {
    return price - (price * discountPercent / 100);
  }
}
