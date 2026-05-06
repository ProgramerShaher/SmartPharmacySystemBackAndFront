import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.interface';
import { SaleInvoice } from '../models/sale-invoice.interface';

@Injectable({
    providedIn: 'root'
})
export class PrintService {
    private apiUrl = `${environment.apiUrl}/Print`;
    private salesInvoiceUrl = `${environment.apiUrl}/SalesInvoices`;

    constructor(private http: HttpClient) { }

    /**
     * Print a customer-facing thermal receipt (80mm) for a sales invoice.
     */
    printInvoice(invoiceId: number): void {
        const printWindow = window.open('', '_blank', 'width=420,height=720');

        if (!printWindow) {
            console.error('Unable to open print window. The browser may have blocked the popup.');
            return;
        }

        printWindow.document.write(this.buildLoadingDocument());
        printWindow.document.close();

        this.http.get<ApiResponse<SaleInvoice>>(`${this.salesInvoiceUrl}/${invoiceId}`).subscribe({
            next: (response) => {
                if (!response.data) {
                    this.writePrintError(printWindow, 'تعذر العثور على بيانات الفاتورة.');
                    return;
                }

                this.writeInvoiceDocument(printWindow, response.data);
            },
            error: () => {
                this.writePrintError(printWindow, 'فشل تحميل بيانات الفاتورة للطباعة.');
            }
        });
    }

    /**
     * Print a receipt for a sales return.
     * Endpoints: POST /api/Print/return/{returnId}
     */
    printReturn(returnId: number): Observable<void> {
        return this.http.post<void>(`${this.apiUrl}/return/${returnId}`, {});
    }

    private writeInvoiceDocument(printWindow: Window, invoice: SaleInvoice): void {
        printWindow.document.open();
        printWindow.document.write(this.buildInvoiceDocument(invoice));
        printWindow.document.close();
        printWindow.focus();

        setTimeout(() => {
            printWindow.print();
        }, 350);
    }

    private writePrintError(printWindow: Window, message: string): void {
        printWindow.document.open();
        printWindow.document.write(`
            <!doctype html>
            <html lang="ar" dir="rtl">
            <head>
                <meta charset="utf-8">
                <title>خطأ في الطباعة</title>
                <style>
                    body { font-family: Arial, sans-serif; padding: 24px; text-align: center; }
                    h1 { font-size: 20px; color: #b91c1c; }
                    p { color: #374151; }
                </style>
            </head>
            <body>
                <h1>تعذر تجهيز الفاتورة</h1>
                <p>${this.escapeHtml(message)}</p>
            </body>
            </html>
        `);
        printWindow.document.close();
    }

    private buildLoadingDocument(): string {
        return `
            <!doctype html>
            <html lang="ar" dir="rtl">
            <head>
                <meta charset="utf-8">
                <title>تجهيز الفاتورة</title>
                <style>
                    body { font-family: Arial, sans-serif; padding: 24px; text-align: center; color: #111827; }
                </style>
            </head>
            <body>جاري تجهيز الفاتورة للطباعة...</body>
            </html>
        `;
    }

    private buildInvoiceDocument(invoice: SaleInvoice): string {
        const items = invoice.items ?? [];
        const invoiceDate = this.formatDate(invoice.invoiceDate);
        const createdAt = this.formatDate(invoice.createdAt || invoice.invoiceDate);
        const customerName = invoice.customerName || 'عميل نقدي';
        const paymentMethod = this.formatPaymentMethod(invoice.paymentMethod);
        const totalAmount = this.toNumber(invoice.totalAmount);
        const rows = items.map((item, index) => {
            const quantity = this.toNumber(item.quantity);
            const price = this.toNumber(item.salePrice);
            const lineTotal = this.toNumber(item.totalLineAmount || quantity * price);

            return `
                <tr>
                    <td class="index">${index + 1}</td>
                    <td>
                        <div class="item-name">${this.escapeHtml(item.medicineName || 'صنف غير محدد')}</div>
                        <div class="item-meta">دفعة: ${this.escapeHtml(item.companyBatchNumber || '-')}</div>
                    </td>
                    <td class="num">${this.formatNumber(quantity, 0)}</td>
                    <td class="num">${this.formatNumber(price, 2)}</td>
                    <td class="num">${this.formatNumber(lineTotal, 2)}</td>
                </tr>
            `;
        }).join('');

        return `
            <!doctype html>
            <html lang="ar" dir="rtl">
            <head>
                <meta charset="utf-8">
                <title>فاتورة بيع ${this.escapeHtml(invoice.saleInvoiceNumber || String(invoice.id))}</title>
                <style>
                    @page {
                        size: 80mm auto;
                        margin: 4mm;
                    }

                    * {
                        box-sizing: border-box;
                    }

                    body {
                        margin: 0;
                        background: #ffffff;
                        color: #111827;
                        font-family: "Cairo", "Tahoma", "Arial", sans-serif;
                        font-size: 11px;
                        line-height: 1.45;
                    }

                    .receipt {
                        width: 72mm;
                        margin: 0 auto;
                    }

                    .header {
                        text-align: center;
                        padding-bottom: 8px;
                        border-bottom: 1px dashed #9ca3af;
                    }

                    .brand {
                        font-size: 18px;
                        font-weight: 800;
                        color: #047857;
                    }

                    .subtitle,
                    .contact,
                    .muted {
                        color: #4b5563;
                    }

                    .title {
                        display: inline-block;
                        margin: 8px 0 4px;
                        padding: 3px 10px;
                        border: 1px solid #111827;
                        border-radius: 999px;
                        font-weight: 800;
                    }

                    .barcode {
                        margin-top: 3px;
                        font-family: "Courier New", monospace;
                        letter-spacing: 1px;
                        direction: ltr;
                    }

                    .section {
                        padding: 7px 0;
                        border-bottom: 1px dashed #d1d5db;
                    }

                    .pair {
                        display: flex;
                        justify-content: space-between;
                        gap: 8px;
                        margin: 2px 0;
                    }

                    .label {
                        color: #4b5563;
                        white-space: nowrap;
                    }

                    .value {
                        font-weight: 700;
                        text-align: left;
                    }

                    table {
                        width: 100%;
                        border-collapse: collapse;
                    }

                    th {
                        padding: 5px 2px;
                        border-bottom: 1px solid #111827;
                        font-size: 10px;
                        color: #374151;
                    }

                    td {
                        padding: 5px 2px;
                        border-bottom: 1px dotted #d1d5db;
                        vertical-align: top;
                    }

                    .index {
                        width: 14px;
                        color: #6b7280;
                    }

                    .item-name {
                        font-weight: 700;
                    }

                    .item-meta {
                        color: #6b7280;
                        font-size: 9px;
                    }

                    .num {
                        text-align: left;
                        direction: ltr;
                        font-variant-numeric: tabular-nums;
                        white-space: nowrap;
                    }

                    .totals {
                        margin-top: 6px;
                    }

                    .total-row {
                        display: flex;
                        justify-content: space-between;
                        align-items: center;
                        padding: 4px 0;
                    }

                    .grand-total {
                        margin-top: 4px;
                        padding: 8px;
                        border: 2px solid #111827;
                        border-radius: 6px;
                        font-size: 15px;
                        font-weight: 900;
                    }

                    .notes {
                        margin-top: 7px;
                        padding: 6px;
                        border: 1px dashed #9ca3af;
                        border-radius: 5px;
                    }

                    .footer {
                        padding-top: 8px;
                        text-align: center;
                    }

                    .thanks {
                        font-weight: 800;
                        color: #047857;
                    }

                    @media print {
                        body {
                            width: 80mm;
                        }
                    }
                </style>
            </head>
            <body>
                <main class="receipt">
                    <header class="header">
                        <div class="brand">الصيدلية الذكية</div>
                        <div class="subtitle">نظام إدارة صيدليات متكامل</div>
                        <div class="contact">هاتف: +967 777 123 456</div>
                        <div class="title">فاتورة مبيعات</div>
                        <div class="barcode">*${this.escapeHtml(invoice.saleInvoiceNumber || String(invoice.id))}*</div>
                    </header>

                    <section class="section">
                        <div class="pair">
                            <span class="label">رقم الفاتورة</span>
                            <span class="value">${this.escapeHtml(invoice.saleInvoiceNumber || String(invoice.id))}</span>
                        </div>
                        <div class="pair">
                            <span class="label">تاريخ البيع</span>
                            <span class="value">${invoiceDate}</span>
                        </div>
                        <div class="pair">
                            <span class="label">وقت التجهيز</span>
                            <span class="value">${createdAt}</span>
                        </div>
                    </section>

                    <section class="section">
                        <div class="pair">
                            <span class="label">العميل</span>
                            <span class="value">${this.escapeHtml(customerName)}</span>
                        </div>
                        <div class="pair">
                            <span class="label">طريقة الدفع</span>
                            <span class="value">${paymentMethod}</span>
                        </div>
                        <div class="pair">
                            <span class="label">المحاسب</span>
                            <span class="value">${this.escapeHtml(invoice.createdByName || 'النظام')}</span>
                        </div>
                    </section>

                    <section class="section">
                        <table>
                            <thead>
                                <tr>
                                    <th>#</th>
                                    <th>الصنف</th>
                                    <th class="num">الكمية</th>
                                    <th class="num">السعر</th>
                                    <th class="num">الإجمالي</th>
                                </tr>
                            </thead>
                            <tbody>
                                ${rows || '<tr><td colspan="5" style="text-align:center;color:#6b7280;">لا توجد أصناف</td></tr>'}
                            </tbody>
                        </table>
                    </section>

                    <section class="totals">
                        <div class="total-row">
                            <span>عدد الأصناف</span>
                            <strong>${items.length}</strong>
                        </div>
                        <div class="total-row">
                            <span>الإجمالي الفرعي</span>
                            <strong>${this.formatNumber(totalAmount, 2)} ر.ي</strong>
                        </div>
                        <div class="total-row">
                            <span>الضريبة</span>
                            <strong>0.00 ر.ي</strong>
                        </div>
                        <div class="total-row grand-total">
                            <span>الصافي</span>
                            <span>${this.formatNumber(totalAmount, 2)} ر.ي</span>
                        </div>
                    </section>

                    ${invoice.notes ? `
                        <section class="notes">
                            <strong>ملاحظات:</strong>
                            <div>${this.escapeHtml(invoice.notes)}</div>
                        </section>
                    ` : ''}

                    <footer class="footer">
                        <div class="thanks">شكراً لزيارتكم</div>
                        <div class="muted">يرجى الاحتفاظ بالفاتورة لعمليات المرتجع أو الاستبدال.</div>
                    </footer>
                </main>
            </body>
            </html>
        `;
    }

    private formatPaymentMethod(paymentMethod: string | number): string {
        if (paymentMethod === 1 || paymentMethod === '1' || paymentMethod === 'Cash') {
            return 'نقدي';
        }

        if (paymentMethod === 2 || paymentMethod === '2' || paymentMethod === 'Credit') {
            return 'آجل';
        }

        return this.escapeHtml(String(paymentMethod || '-'));
    }

    private formatDate(value?: string): string {
        if (!value) return '-';

        const date = new Date(value);
        if (Number.isNaN(date.getTime())) return this.escapeHtml(value);

        return date.toLocaleString('ar-YE', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    private formatNumber(value: number, digits: number): string {
        return this.toNumber(value).toLocaleString('en-US', {
            minimumFractionDigits: digits,
            maximumFractionDigits: digits
        });
    }

    private toNumber(value: number | string | null | undefined): number {
        const parsed = Number(value);
        return Number.isFinite(parsed) ? parsed : 0;
    }

    private escapeHtml(value: string): string {
        return value
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }
}
