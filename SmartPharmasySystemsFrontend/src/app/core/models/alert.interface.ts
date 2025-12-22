// Alert Enums matching backend
export enum ExpiryStatus {
  OneWeek = 'ExpiryOneWeek',
  TwoWeeks = 'ExpiryTwoWeeks',
  OneMonth = 'ExpiryOneMonth',
  TwoMonths = 'ExpiryTwoMonths'
}

export enum AlertStatus {
  Pending = 0,
  Read = 1,
  Dismissed = 2,
  Archived = 3
}

// Alert Interface
export interface Alert {
  id: number;
  batchId: number;
  batchNumber: string;
  medicineName: string;
  alertType: string;
  executionDate: string;
  expiryDate: string;
  status: number; // The API returns string "Read", "Pending" etc.
  message: string;
  isDeleted: boolean;
  createdAt: string;
}

// DTOs for API operations
export interface AlertQueryDto {
  search?: string;
  status?: number; // Filter usually takes number
  alertType?: string;
  batchNumber?: string;
  medicineName?: string;
  page?: number;
  pageSize?: number;
}

export interface CreateAlertDto {
  batchId: number;
  alertType: string;
  executionDate: string;
  message: string;
  expiryDate?: string; // Added
  status?: number; // Added
  createdAt?: string; // Added (Optional, often ignored but requested)
}

export interface UpdateAlertDto {
  id: number;
  batchId?: number;
  alertType?: string;
  executionDate?: string;
  message?: string;
  status?: number; // Changed to number to match Enum
  expiryDate?: string; // Added
  createdAt?: string; // Added
}

// Utility functions for enum handling
export class AlertUtils {
  static getExpiryStatusColor(alertType: string): string {
    if (!alertType) return '#6c757d';

    if (alertType.includes('OneWeek') || alertType.includes('SevenDays') || alertType === 'ExpiryOneWeek') {
      return '#ef4444'; // Red for 1 week (High Urgency)
    }
    if (alertType.includes('TwoWeeks') || alertType.includes('FourteenDays') || alertType === 'ExpiryTwoWeeks') {
      return '#f97316'; // Orange for 2 weeks
    }
    if (alertType.includes('OneMonth') || alertType === 'ExpiryOneMonth') {
      return '#eab308'; // Yellow for 1 month
    }
    if (alertType.includes('TwoMonths') || alertType === 'ExpiryTwoMonths') {
      return '#3b82f6'; // Blue for 2 months
    }
    return '#6c757d'; // Gray default
  }

  static getExpiryStatusLabel(alertType: string): string {
    if (!alertType) return 'غير محدد';

    if (alertType.includes('OneWeek') || alertType === 'ExpiryOneWeek') {
      return 'أسبوع واحد';
    }
    if (alertType.includes('TwoWeeks') || alertType === 'ExpiryTwoWeeks') {
      return 'أسبوعين';
    }
    if (alertType.includes('OneMonth') || alertType === 'ExpiryOneMonth') {
      return 'شهر واحد';
    }
    if (alertType.includes('TwoMonths') || alertType === 'ExpiryTwoMonths') {
      return 'شهرين';
    }

    // Fallback for API response strings if they differ
    return alertType;
  }

  static getAlertStatusLabel(status: string | number): string {
    // Handle both string name (from GET) and number (from Enum)
    const s = String(status).toLowerCase();
    if (s === 'pending' || s === '0') return 'معلق';
    if (s === 'read' || s === '1') return 'مقروء';
    if (s === 'dismissed' || s === '2') return 'مرفوض';
    if (s === 'archived' || s === '3') return 'مؤرشف';
    return String(status);
  }

  static getAlertStatusSeverity(status: string | number): "success" | "secondary" | "info" | "warning" | "danger" | "contrast" {
    const s = String(status).toLowerCase();
    if (s === 'pending' || s === '0') return 'warning';
    if (s === 'read' || s === '1') return 'success'; // Read is usually good/acknowledged
    if (s === 'dismissed' || s === '2') return 'danger';
    if (s === 'archived' || s === '3') return 'secondary';
    return 'info';
  }
}
