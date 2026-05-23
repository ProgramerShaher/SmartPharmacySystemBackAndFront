import { PaymentType } from '../../../core/models';

/**
 * Get Arabic label for payment method
 */
export function getPaymentMethodLabel(method: PaymentType): string {
  switch (method) {
    case PaymentType.Cash:
      return 'نقدي';
    case PaymentType.Credit:
      return 'آجل';
    default:
      return 'غير محدد';
  }
}

/**
 * Get icon for payment method
 */
export function getPaymentMethodIcon(method: PaymentType): string {
  switch (method) {
    case PaymentType.Cash:
      return 'pi-wallet';
    case PaymentType.Credit:
      return 'pi-credit-card';
    default:
      return 'pi-question-circle';
  }
}

/**
 * Get color for payment method
 */
export function getPaymentMethodColor(method: PaymentType): string {
  switch (method) {
    case PaymentType.Cash:
      return getComputedStyle(document.documentElement).getPropertyValue('--primary-600').trim() || '#059669'; // Emerald
    case PaymentType.Credit:
      return '#D97706'; // Amber
    default:
      return '#6B7280'; // Gray
  }
}

/**
 * Get severity for payment method tag
 */
export function getPaymentMethodSeverity(method: PaymentType): 'success' | 'warning' | 'info' | 'danger' {
  switch (method) {
    case PaymentType.Cash:
      return 'success';
    case PaymentType.Credit:
      return 'warning';
    default:
      return 'info';
  }
}

/**
 * Get Arabic label for paid status
 */
export function getPaidStatusLabel(isPaid: boolean): string {
  return isPaid ? 'مدفوع' : 'غير مدفوع';
}

/**
 * Get severity for paid status
 */
export function getPaidStatusSeverity(isPaid: boolean): 'success' | 'warning' {
  return isPaid ? 'success' : 'warning';
}

/**
 * Get icon for paid status
 */
export function getPaidStatusIcon(isPaid: boolean): string {
  return isPaid ? 'pi-check-circle' : 'pi-clock';
}

/**
 * Format expense amount with currency
 */
export function formatExpenseAmount(amount: number): string {
  return `${amount.toLocaleString('ar-YE', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} ريال`;
}

/**
 * Format expense amount number ONLY (without currency label)
 */
export function formatExpenseAmountNumber(amount: number): string {
  return amount.toLocaleString('ar-YE', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

/**
 * Format date to Arabic locale
 */
export function formatDate(date: string | Date): string {
  const d = typeof date === 'string' ? new Date(date) : date;
  return d.toLocaleDateString('ar-YE', {
    year: 'numeric',
    month: 'long',
    day: 'numeric'
  });
}

/**
 * Format date and time to Arabic locale
 */
export function formatDateTime(date: string | Date): string {
  const d = typeof date === 'string' ? new Date(date) : date;
  return d.toLocaleString('ar-YE', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  });
}

/**
 * Convert number to Arabic words (Tafneet)
 */
export function convertNumberToArabicWords(amount: number): string {
  if (amount === 0) return 'صفر';

  const ones = ['', 'واحد', 'اثنان', 'ثلاثة', 'أربعة', 'خمسة', 'ستة', 'سبعة', 'ثمانية', 'تسعة'];
  const teens = ['عشرة', 'أحد عشر', 'اثنا عشر', 'ثلاثة عشر', 'أربعة عشر', 'خمسة عشر', 'ستة عشر', 'سبعة عشر', 'ثمانية عشر', 'تسعة عشر'];
  const tens = ['', 'عشرة', 'عشرون', 'ثلاثون', 'أربعون', 'خمسون', 'ستون', 'سبعون', 'ثمانون', 'تسعون'];
  const hundreds = ['', 'مائة', 'مائتان', 'ثلاثمائة', 'أربعمائة', 'خمسمائة', 'ستمائة', 'سبعمائة', 'ثمانمائة', 'تسعمائة'];

  const convertGroup = (num: number): string => {
    let parts: string[] = [];

    const h = Math.floor(num / 100);
    const t = Math.floor((num % 100) / 10);
    const o = num % 10;

    if (h > 0) {
      parts.push(hundreds[h]);
    }

    if (t > 0 || o > 0) {
      if (t === 1) {
        parts.push(teens[o]);
      } else {
        if (o > 0) {
          if (o === 1 && t > 0) {
            parts.push('واحد');
          } else if (o === 2 && t > 0) {
            parts.push('اثنان');
          } else {
            parts.push(ones[o]);
          }
        }
        if (t > 0) {
          parts.push(tens[t]);
        }
      }
    }

    return parts.filter(p => p !== '').join(' و');
  };

  let num = Math.floor(amount);
  let words = '';

  const millions = Math.floor(num / 1000000);
  const thousands = Math.floor((num % 1000000) / 1000);
  const remainder = num % 1000;

  if (millions > 0) {
    if (millions === 1) {
      words += 'مليون';
    } else if (millions === 2) {
      words += 'مليونان';
    } else if (millions >= 3 && millions <= 10) {
      words += convertGroup(millions) + ' ملايين';
    } else {
      words += convertGroup(millions) + ' مليون';
    }
  }

  if (thousands > 0) {
    if (words !== '') words += ' و';
    
    if (thousands === 1) {
      words += 'ألف';
    } else if (thousands === 2) {
      words += 'ألفان';
    } else if (thousands >= 3 && thousands <= 10) {
      words += convertGroup(thousands) + ' آلاف';
    } else {
      words += convertGroup(thousands) + ' ألف';
    }
  }

  if (remainder > 0) {
    if (words !== '') words += ' و';
    words += convertGroup(remainder);
  }

  // Handle decimals (fils/cents) if any
  const decimal = Math.round((amount - num) * 100);
  if (decimal > 0) {
    words += ` و ${convertGroup(decimal)} فلس`;
  }

  return words;
}

