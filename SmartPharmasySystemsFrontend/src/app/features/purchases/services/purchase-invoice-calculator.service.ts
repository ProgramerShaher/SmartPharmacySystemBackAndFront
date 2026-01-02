import { Injectable } from '@angular/core';
import { CreatePurchaseInvoiceDetailDto, PurchaseInvoiceDetail } from '../../../core/models';

/**
 * Purchase Invoice Calculator Service
 * The "Brain" - Handles all calculations and validations
 * 100% Accurate Financial Logic
 */
@Injectable({ providedIn: 'root' })
export class PurchaseInvoiceCalculatorService {

    /**
     * Calculate row total: Quantity * Purchase Price
     * @param quantity Purchased quantity
     * @param purchasePrice Price per unit
     * @returns Total amount for the row
     */
    calculateRowTotal(quantity: number, purchasePrice: number): number {
        return quantity * purchasePrice;
    }

    /**
     * Calculate True Unit Cost (after bonus)
     * Formula: (Quantity * PurchasePrice) / (Quantity + BonusQuantity)
     * @param quantity Purchased quantity
     * @param bonusQuantity Free bonus quantity
     * @param purchasePrice Price per unit
     * @returns True unit cost after bonus
     */
    calculateTrueUnitCost(quantity: number, bonusQuantity: number, purchasePrice: number): number {
        const totalQuantity = quantity + bonusQuantity;
        if (totalQuantity === 0) {
            return 0;
        }
        const totalCost = quantity * purchasePrice;
        return totalCost / totalQuantity;
    }

    /**
     * Calculate invoice grand total
     * Sum of all row totals
     * @param items Array of invoice detail items
     * @returns Grand total amount
     */
    calculateInvoiceGrandTotal(items: CreatePurchaseInvoiceDetailDto[] | PurchaseInvoiceDetail[]): number {
        return items.reduce((sum, item) => {
            const rowTotal = this.calculateRowTotal(item.quantity, item.purchasePrice);
            return sum + rowTotal;
        }, 0);
    }

    /**
     * Validate that Sale Price >= Purchase Price
     * Business Rule: Cannot sell at a loss
     * @param salePrice Sale price per unit
     * @param purchasePrice Purchase price per unit
     * @returns Validation result
     */
    validateSalePrice(salePrice: number, purchasePrice: number): { valid: boolean; message?: string } {
        if (salePrice < purchasePrice) {
            return {
                valid: false,
                message: 'سعر البيع يجب أن يكون أكبر من أو يساوي سعر الشراء'
            };
        }
        return { valid: true };
    }

    /**
     * Validate expiry date
     * Must be a future date
     * @param expiryDate Expiry date string (ISO format)
     * @returns Validation result
     */
    validateExpiryDate(expiryDate: string): { valid: boolean; message?: string } {
        const expiry = new Date(expiryDate);
        const today = new Date();
        today.setHours(0, 0, 0, 0);

        if (expiry <= today) {
            return {
                valid: false,
                message: 'تاريخ الانتهاء يجب أن يكون في المستقبل'
            };
        }
        return { valid: true };
    }

    /**
     * Calculate days until expiry
     * @param expiryDate Expiry date string (ISO format)
     * @returns Number of days until expiry (negative if expired)
     */
    calculateDaysUntilExpiry(expiryDate: string): number {
        const expiry = new Date(expiryDate);
        const today = new Date();
        today.setHours(0, 0, 0, 0);
        expiry.setHours(0, 0, 0, 0);

        const diffTime = expiry.getTime() - today.getTime();
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
        return diffDays;
    }

    /**
     * Get expiry status based on days until expiry
     * @param daysUntilExpiry Days until expiry
     * @returns Status string
     */
    getExpiryStatus(daysUntilExpiry: number): string {
        if (daysUntilExpiry < 0) {
            return 'منتهي الصلاحية';
        } else if (daysUntilExpiry <= 30) {
            return 'قريب الانتهاء';
        } else {
            return 'صالح';
        }
    }

    /**
     * Get expiry status color
     * @param daysUntilExpiry Days until expiry
     * @returns Color class
     */
    getExpiryStatusColor(daysUntilExpiry: number): string {
        if (daysUntilExpiry < 0) {
            return 'danger'; // Red
        } else if (daysUntilExpiry <= 30) {
            return 'warning'; // Amber
        } else {
            return 'success'; // Green
        }
    }

    /**
     * Validate quantity
     * Must be greater than 0
     * @param quantity Quantity value
     * @returns Validation result
     */
    validateQuantity(quantity: number): { valid: boolean; message?: string } {
        if (quantity <= 0) {
            return {
                valid: false,
                message: 'الكمية يجب أن تكون أكبر من صفر'
            };
        }
        return { valid: true };
    }

    /**
     * Validate purchase price
     * Must be greater than 0
     * @param price Purchase price
     * @returns Validation result
     */
    validatePurchasePrice(price: number): { valid: boolean; message?: string } {
        if (price <= 0) {
            return {
                valid: false,
                message: 'سعر الشراء يجب أن يكون أكبر من صفر'
            };
        }
        return { valid: true };
    }

    /**
     * Validate entire invoice detail item
     * Runs all validations
     * @param item Invoice detail item
     * @returns Validation result with all errors
     */
    validateInvoiceItem(item: CreatePurchaseInvoiceDetailDto): { valid: boolean; errors: string[] } {
        const errors: string[] = [];

        const qtyValidation = this.validateQuantity(item.quantity);
        if (!qtyValidation.valid) {
            errors.push(qtyValidation.message!);
        }

        const priceValidation = this.validatePurchasePrice(item.purchasePrice);
        if (!priceValidation.valid) {
            errors.push(priceValidation.message!);
        }

        const salePriceValidation = this.validateSalePrice(item.salePrice, item.purchasePrice);
        if (!salePriceValidation.valid) {
            errors.push(salePriceValidation.message!);
        }

        const expiryValidation = this.validateExpiryDate(item.expiryDate);
        if (!expiryValidation.valid) {
            errors.push(expiryValidation.message!);
        }

        return {
            valid: errors.length === 0,
            errors
        };
    }

    /**
     * Format currency for display
     * @param amount Amount to format
     * @returns Formatted string
     */
    formatCurrency(amount: number): string {
        return new Intl.NumberFormat('ar-YE', {
            style: 'currency',
            currency: 'YER',
            minimumFractionDigits: 2,
            maximumFractionDigits: 2
        }).format(amount);
    }

    /**
     * Round to 2 decimal places
     * @param value Value to round
     * @returns Rounded value
     */
    round(value: number): number {
        return Math.round(value * 100) / 100;
    }
}
