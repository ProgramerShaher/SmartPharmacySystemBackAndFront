import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin, catchError, of } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.interface';
import { SaleInvoice } from '../models/sale-invoice.interface';
import { SettingsService } from './settings.service';
import { PharmacySettings } from '../models/settings/pharmacy-settings.interface';

@Injectable({
    providedIn: 'root'
})
export class PrintService {
    private apiUrl = `${environment.apiUrl}/Print`;
    private salesInvoiceUrl = `${environment.apiUrl}/SalesInvoices`;

    constructor(
        private http: HttpClient,
        private settingsService: SettingsService
    ) { }

    /**
     * Print a customer-facing thermal/standard receipt for a sales invoice.
     */
    printInvoice(invoiceId: number): void {
        const printWindow = window.open('', '_blank', 'width=840,height=920');

        if (!printWindow) {
            console.error('Unable to open print window. The browser may have blocked the popup.');
            return;
        }

        printWindow.document.write(this.buildLoadingDocument());
        printWindow.document.close();

        forkJoin({
            invoiceRes: this.http.get<ApiResponse<SaleInvoice>>(`${this.salesInvoiceUrl}/${invoiceId}`),
            settings: this.settingsService.getSettings().pipe(catchError(() => of(null)))
        }).subscribe({
            next: ({ invoiceRes, settings }) => {
                if (!invoiceRes.data) {
                    this.writePrintError(printWindow, 'تعذر العثور على بيانات الفاتورة.');
                    return;
                }

                this.writeInvoiceDocument(printWindow, invoiceRes.data, settings);
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

    private writeInvoiceDocument(printWindow: Window, invoice: SaleInvoice, settings?: PharmacySettings | null): void {
        printWindow.document.open();
        printWindow.document.write(this.buildInvoiceDocument(invoice, settings));
        printWindow.document.close();
        printWindow.focus();

        setTimeout(() => {
            printWindow.print();
        }, 450);
    }

    private writePrintError(printWindow: Window, message: string): void {
        printWindow.document.open();
        printWindow.document.write(`
            <!doctype html>
            <html lang="ar" dir="rtl">
            <head>
                <meta charset="utf-8">
                <title>خطأ في الطباعة</title>
                <link rel="preconnect" href="https://fonts.googleapis.com">
                <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
                <link href="https://fonts.googleapis.com/css2?family=Cairo:wght@600;700&display=swap" rel="stylesheet">
                <style>
                    body { font-family: 'Cairo', system-ui, sans-serif; padding: 32px; text-align: center; background: #f8fafc; color: #0f172a; }
                    .error-card { max-width: 420px; margin: 40px auto; padding: 24px; background: #ffffff; border-radius: 12px; border: 1px solid #fecdd3; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05); }
                    h1 { font-size: 18px; color: #e11d48; margin-top: 0; }
                    p { color: #475569; font-size: 14px; }
                </style>
            </head>
            <body>
                <div class="error-card">
                    <h1>تعذر تجهيز الفاتورة</h1>
                    <p>${this.escapeHtml(message)}</p>
                </div>
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
                <title>تجهيز الفاتورة...</title>
                <link rel="preconnect" href="https://fonts.googleapis.com">
                <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
                <link href="https://fonts.googleapis.com/css2?family=Cairo:wght@600;700&display=swap" rel="stylesheet">
                <style>
                    body { font-family: 'Cairo', sans-serif; display: flex; align-items: center; justify-content: center; height: 100vh; margin: 0; background: #f8fafc; color: #0f172a; }
                    .loader { text-align: center; }
                    .spinner { width: 42px; height: 42px; border: 4px solid #cbd5e1; border-top-color: #0284c7; border-radius: 50%; animation: spin 0.8s linear infinite; margin: 0 auto 16px; }
                    @keyframes spin { to { transform: rotate(360deg); } }
                </style>
            </head>
            <body>
                <div class="loader">
                    <div class="spinner"></div>
                    <div>جاري تحميل وتنسيق فاتورة المبيعات للطباعة...</div>
                </div>
            </body>
            </html>
        `;
    }

    private buildInvoiceDocument(invoice: SaleInvoice, settings?: PharmacySettings | null): string {
        const items = invoice.items ?? [];
        const invoiceDate = this.formatDate(invoice.invoiceDate);
        const createdAt = this.formatDate(invoice.createdAt || invoice.invoiceDate);
        const customerName = invoice.customerName || 'عميل نقدي';
        const paymentMethod = this.formatPaymentMethod(invoice.paymentMethod);
        const totalAmount = this.toNumber(invoice.totalAmount);
        const currency = settings?.baseCurrency || 'ر.ي';

        // Build Pharmacy Header Info
        const pharmacyName = settings?.pharmacyName || 'الصيدلية الذكية';
        const phone = settings?.phoneNumber || settings?.mobileNumber || '';
        const address = settings?.address || '';
        const commercialRegister = settings?.commercialRegister || '';
        const taxNumber = settings?.taxNumber || '';
        const welcomeMsg = settings?.invoiceWelcomeMessage || 'شكراً لزيارتكم - نتمنى لكم دوام الصحة والعافية';

        // Resolve logo URL
        let logoHtml = '';
        if (settings?.logoUrl) {
            let logoSrc = settings.logoUrl;
            if (!logoSrc.startsWith('http') && !logoSrc.startsWith('data:')) {
                const baseUrl = environment.apiUrl.replace(/\/api\/?$/, '');
                logoSrc = `${baseUrl}${logoSrc.startsWith('/') ? '' : '/'}${logoSrc}`;
            }
            logoHtml = `
                <img src="${logoSrc}" alt="Logo" class="pharmacy-logo" onerror="this.style.display='none'; this.nextElementSibling.style.display='flex';" />
                <div class="logo-fallback" style="display:none;">
                    <svg viewBox="0 0 24 24" width="28" height="28" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2v20M2 12h20M7 7l10 10M17 7L7 17"/></svg>
                </div>
            `;
        } else {
            logoHtml = `
                <div class="logo-fallback">
                    <svg viewBox="0 0 24 24" width="28" height="28" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 2v20M2 12h20M7 7l10 10M17 7L7 17"/></svg>
                </div>
            `;
        }

        let totalQtyCount = 0;
        const rows = items.map((item, index) => {
            const quantity = this.toNumber(item.quantity);
            totalQtyCount += quantity;
            const price = this.toNumber(item.salePrice);
            const lineTotal = this.toNumber(item.totalLineAmount || (quantity * price));

            return `
                <tr>
                    <td class="col-idx">${index + 1}</td>
                    <td class="col-item">
                        <div class="item-title">${this.escapeHtml(item.medicineName || 'صنف غير محدد')}</div>
                        ${item.companyBatchNumber ? `<div class="item-sub">رقم الدفعة: <span>${this.escapeHtml(item.companyBatchNumber)}</span></div>` : ''}
                    </td>
                    <td class="col-num num-cell">${this.formatNumber(quantity, 0)}</td>
                    <td class="col-num num-cell">${this.formatNumber(price, 2)}</td>
                    <td class="col-num num-cell font-bold">${this.formatNumber(lineTotal, 2)}</td>
                </tr>
            `;
        }).join('');

        return `
            <!doctype html>
            <html lang="ar" dir="rtl">
            <head>
                <meta charset="utf-8">
                <title>فاتورة مبيعات #${this.escapeHtml(invoice.saleInvoiceNumber || String(invoice.id))}</title>
                <link rel="preconnect" href="https://fonts.googleapis.com">
                <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
                <link href="https://fonts.googleapis.com/css2?family=Cairo:wght@400;600;700;800;900&display=swap" rel="stylesheet">
                <style>
                    @page {
                        size: auto;
                        margin: 5mm;
                    }

                    * {
                        box-sizing: border-box;
                        -webkit-print-color-adjust: exact !important;
                        print-color-adjust: exact !important;
                    }

                    body {
                        margin: 0;
                        padding: 12px;
                        background: #ffffff;
                        color: #0f172a;
                        font-family: 'Cairo', system-ui, -apple-system, sans-serif;
                        font-size: 12px;
                        line-height: 1.5;
                    }

                    .invoice-wrapper {
                        max-width: 760px;
                        margin: 0 auto;
                        background: #ffffff;
                        border: 1px solid #e2e8f0;
                        border-radius: 12px;
                        padding: 24px;
                        box-shadow: 0 4px 12px rgba(0,0,0,0.03);
                    }

                    @media print {
                        body {
                            padding: 0;
                        }
                        .invoice-wrapper {
                            border: none;
                            box-shadow: none;
                            padding: 4px;
                            max-width: 100%;
                        }
                    }

                    /* Header Section */
                    .header-container {
                        display: flex;
                        align-items: center;
                        justify-content: space-between;
                        padding-bottom: 16px;
                        border-bottom: 2.5px solid #0284c7;
                        margin-bottom: 16px;
                        gap: 16px;
                    }

                    .brand-box {
                        display: flex;
                        align-items: center;
                        gap: 14px;
                    }

                    .pharmacy-logo {
                        max-height: 64px;
                        max-width: 120px;
                        object-fit: contain;
                        border-radius: 6px;
                    }

                    .logo-fallback {
                        width: 52px;
                        height: 52px;
                        border-radius: 12px;
                        background: #e0f2fe;
                        color: #0284c7;
                        display: flex;
                        align-items: center;
                        justify-content: center;
                    }

                    .pharmacy-info h1 {
                        margin: 0 0 4px 0;
                        font-size: 20px;
                        font-weight: 800;
                        color: #0369a1;
                        line-height: 1.2;
                    }

                    .pharmacy-info p {
                        margin: 0;
                        font-size: 11px;
                        color: #475569;
                    }

                    .invoice-badge-box {
                        text-align: left;
                    }

                    .invoice-title {
                        display: inline-block;
                        background: #0284c7;
                        color: #ffffff;
                        padding: 5px 18px;
                        border-radius: 20px;
                        font-weight: 800;
                        font-size: 14px;
                        letter-spacing: 0.5px;
                    }

                    .invoice-no {
                        margin-top: 6px;
                        font-size: 15px;
                        font-weight: 800;
                        color: #0f172a;
                        direction: ltr;
                        text-align: left;
                    }

                    /* Metadata Grid */
                    .meta-grid {
                        display: grid;
                        grid-template-columns: repeat(2, 1fr);
                        gap: 10px 24px;
                        background: #f8fafc;
                        border: 1px solid #e2e8f0;
                        border-radius: 10px;
                        padding: 14px 18px;
                        margin-bottom: 18px;
                    }

                    .meta-item {
                        display: flex;
                        align-items: center;
                        gap: 8px;
                        font-size: 12px;
                    }

                    .meta-label {
                        color: #64748b;
                        font-weight: 600;
                        white-space: nowrap;
                    }

                    .meta-value {
                        color: #0f172a;
                        font-weight: 700;
                    }

                    .num-value {
                        direction: ltr;
                        display: inline-block;
                    }

                    /* Items Table */
                    .table-container {
                        margin-bottom: 20px;
                        border: 1px solid #cbd5e1;
                        border-radius: 8px;
                        overflow: hidden;
                    }

                    table {
                        width: 100%;
                        border-collapse: collapse;
                        text-align: right;
                    }

                    thead {
                        background: #f1f5f9;
                        border-bottom: 2px solid #cbd5e1;
                    }

                    th {
                        padding: 10px 12px;
                        font-weight: 800;
                        color: #334155;
                        font-size: 12px;
                    }

                    td {
                        padding: 10px 12px;
                        border-bottom: 1px solid #e2e8f0;
                        vertical-align: middle;
                        font-size: 12px;
                    }

                    tbody tr:nth-child(even) {
                        background-color: #f8fafc;
                    }

                    tbody tr:last-child td {
                        border-bottom: none;
                    }

                    .col-idx {
                        width: 36px;
                        text-align: center;
                        color: #94a3b8;
                        font-weight: 700;
                    }

                    .col-item {
                        min-width: 180px;
                    }

                    .item-title {
                        font-weight: 700;
                        color: #0f172a;
                    }

                    .item-sub {
                        font-size: 10px;
                        color: #64748b;
                        margin-top: 2px;
                    }

                    .col-num {
                        text-align: left;
                        width: 90px;
                    }

                    .num-cell {
                        direction: ltr;
                        font-weight: 600;
                        font-family: 'Cairo', monospace, sans-serif;
                    }

                    .font-bold {
                        font-weight: 800;
                        color: #0f172a;
                    }

                    /* Summary Box */
                    .summary-wrapper {
                        display: flex;
                        justify-content: space-between;
                        align-items: flex-start;
                        gap: 20px;
                        margin-bottom: 20px;
                    }

                    .summary-notes {
                        flex: 1;
                        background: #f8fafc;
                        border: 1px dashed #cbd5e1;
                        border-radius: 8px;
                        padding: 12px 14px;
                        font-size: 11px;
                        color: #475569;
                    }

                    .summary-notes-title {
                        font-weight: 700;
                        color: #1e293b;
                        margin-bottom: 4px;
                    }

                    .summary-box {
                        width: 280px;
                        background: #ffffff;
                        border: 1px solid #cbd5e1;
                        border-radius: 10px;
                        padding: 12px 16px;
                    }

                    .summary-row {
                        display: flex;
                        align-items: center;
                        gap: 8px;
                        padding: 6px 0;
                        border-bottom: 1px dashed #e2e8f0;
                        font-size: 12px;
                    }

                    .summary-row:last-child {
                        border-bottom: none;
                    }

                    .summary-label {
                        color: #64748b;
                        font-weight: 600;
                        white-space: nowrap;
                    }

                    .summary-val {
                        font-weight: 700;
                        color: #0f172a;
                        display: inline-flex;
                        align-items: center;
                        gap: 4px;
                    }

                    .net-total-box {
                        background: #0284c7;
                        color: #ffffff;
                        margin-top: 8px;
                        padding: 10px 14px;
                        border-radius: 8px;
                        display: flex;
                        align-items: center;
                        gap: 10px;
                    }

                    .net-total-label {
                        font-size: 14px;
                        font-weight: 800;
                        white-space: nowrap;
                    }

                    .net-total-val {
                        font-size: 16px;
                        font-weight: 900;
                        display: inline-flex;
                        align-items: center;
                        gap: 6px;
                    }

                    /* Footer */
                    .footer-section {
                        border-top: 1px solid #e2e8f0;
                        padding-top: 16px;
                        text-align: center;
                    }

                    .barcode-container {
                        margin-bottom: 10px;
                        display: flex;
                        flex-direction: column;
                        align-items: center;
                    }

                    .barcode-lines {
                        font-family: 'Courier New', Courier, monospace;
                        font-weight: bold;
                        font-size: 18px;
                        letter-spacing: 4px;
                        color: #0f172a;
                        direction: ltr;
                    }

                    .barcode-subtext {
                        font-size: 10px;
                        color: #64748b;
                        letter-spacing: 1px;
                        direction: ltr;
                    }

                    .welcome-msg {
                        font-size: 12px;
                        font-weight: 700;
                        color: #0369a1;
                        margin-bottom: 4px;
                    }

                    .policy-msg {
                        font-size: 10px;
                        color: #64748b;
                    }
                </style>
            </head>
            <body>
                <main class="invoice-wrapper">
                    <!-- Header -->
                    <header class="header-container">
                        <div class="brand-box">
                            ${logoHtml}
                            <div class="pharmacy-info">
                                <h1>${this.escapeHtml(pharmacyName)}</h1>
                                ${address ? `<p>${this.escapeHtml(address)}</p>` : ''}
                                ${phone ? `<p>هاتف: <span class="num-value">${this.escapeHtml(phone)}</span></p>` : ''}
                                ${commercialRegister || taxNumber ? `
                                    <p>
                                        ${commercialRegister ? `س.ت: <span class="num-value">${this.escapeHtml(commercialRegister)}</span>` : ''}
                                        ${taxNumber ? ` | الرقم الضريبي: <span class="num-value">${this.escapeHtml(taxNumber)}</span>` : ''}
                                    </p>
                                ` : ''}
                            </div>
                        </div>

                        <div class="invoice-badge-box">
                            <div class="invoice-title">فاتورة مبيعات</div>
                            <div class="invoice-no">#${this.escapeHtml(invoice.saleInvoiceNumber || String(invoice.id))}</div>
                        </div>
                    </header>

                    <!-- Metadata Grid -->
                    <section class="meta-grid">
                        <div class="meta-item">
                            <span class="meta-label">تاريخ الفاتورة:</span>
                            <span class="meta-value num-value">${invoiceDate}</span>
                        </div>
                        <div class="meta-item">
                            <span class="meta-label">طريقة الدفع:</span>
                            <span class="meta-value">${paymentMethod}</span>
                        </div>
                        <div class="meta-item">
                            <span class="meta-label">اسم العميل:</span>
                            <span class="meta-value">${this.escapeHtml(customerName)}</span>
                        </div>
                        <div class="meta-item">
                            <span class="meta-label">المحاسب / البائع:</span>
                            <span class="meta-value">${this.escapeHtml(invoice.createdByName || 'النظام')}</span>
                        </div>
                    </section>

                    <!-- Items Table -->
                    <section class="table-container">
                        <table>
                            <thead>
                                <tr>
                                    <th class="col-idx">#</th>
                                    <th class="col-item">اسم الصنف والبيانات</th>
                                    <th class="col-num">الكمية</th>
                                    <th class="col-num">السعر</th>
                                    <th class="col-num">الإجمالي</th>
                                </tr>
                            </thead>
                            <tbody>
                                ${rows || '<tr><td colspan="5" style="text-align:center;color:#94a3b8;padding:20px;">لا توجد أصناف في هذه الفاتورة</td></tr>'}
                            </tbody>
                        </table>
                    </section>

                    <!-- Summary & Totals -->
                    <section class="summary-wrapper">
                        <div class="summary-notes">
                            <div class="summary-notes-title">ملاحظات الفاتورة:</div>
                            <div>${invoice.notes ? this.escapeHtml(invoice.notes) : 'لا توجد ملاحظات إضافية.'}</div>
                        </div>

                        <div class="summary-box">
                            <div class="summary-row">
                                <span class="summary-label">عدد الأصناف:</span>
                                <span class="summary-val"><span dir="ltr">${items.length}</span></span>
                            </div>
                            <div class="summary-row">
                                <span class="summary-label">إجمالي القطع:</span>
                                <span class="summary-val"><span dir="ltr">${this.formatNumber(totalQtyCount, 0)}</span></span>
                            </div>
                            <div class="summary-row">
                                <span class="summary-label">الإجمالي الفرعي:</span>
                                <span class="summary-val"><span dir="ltr">${this.formatNumber(totalAmount, 2)}</span> <span>${this.escapeHtml(currency)}</span></span>
                            </div>
                            <div class="summary-row">
                                <span class="summary-label">الخصم:</span>
                                <span class="summary-val"><span dir="ltr">0.00</span> <span>${this.escapeHtml(currency)}</span></span>
                            </div>
                            
                            <div class="net-total-box">
                                <span class="net-total-label">الصافي الإجمالي:</span>
                                <span class="net-total-val"><span dir="ltr">${this.formatNumber(totalAmount, 2)}</span> <span>${this.escapeHtml(currency)}</span></span>
                            </div>
                        </div>
                    </section>

                    <!-- Footer -->
                    <footer class="footer-section">
                        <div class="barcode-container">
                            <div class="barcode-lines">*${this.escapeHtml(invoice.saleInvoiceNumber || String(invoice.id))}*</div>
                            <div class="barcode-subtext">${this.escapeHtml(invoice.saleInvoiceNumber || String(invoice.id))}</div>
                        </div>
                        <div class="welcome-msg">${this.escapeHtml(welcomeMsg)}</div>
                        <div class="policy-msg">البضاعة المباعة ترجع أو تستبدل خلال 3 أيام بشرط وجود أصل الفاتورة وأن يكون الدواء بحالته الأصلية.</div>
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

        return date.toLocaleString('en-US', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit',
            hour12: true
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
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }
}

