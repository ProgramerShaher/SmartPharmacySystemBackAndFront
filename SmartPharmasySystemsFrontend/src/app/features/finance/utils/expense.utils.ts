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
      return '#059669'; // Emerald
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
