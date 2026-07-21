# 🗺️ خطة التنفيذ التفصيلية - Smart Pharmacy ERP
## Implementation Workflow - Backend → Frontend

> كل خطوة مستقلة وقابلة للاختبار قبل الانتقال للخطوة التالية.  
> الترقيم: `[Phase].[Module].[Step]`

---

## ═══════════════════════════════════
## 🔵 الجزء الأول: BACKEND (.NET 9)
## ═══════════════════════════════════

---

## 📦 المرحلة B1: الكيانات وقاعدة البيانات (Core Entities)

### B1.1 - طبقة Core (Domain Entities)
- [ ] `Branch` — الفروع (Id, BranchCode, Name, BranchType, IsActive)
- [ ] `Warehouse` — المخازن (Id, BranchId, Type: Main/Branch/Damaged, Name)
- [ ] `InventoryStock` — رصيد المخزون (WarehouseId, MedicineId, BatchNumber, ExpiryDate, Quantity, **ReorderLevel**, **ReorderQuantity**)
- [ ] `StockTransfer` + `StockTransferItem` — سندات التحويل وبنودها
- [ ] `DamagedGoodsRecord` — سجل التوالف والمنتهية الصلاحية
- [ ] `StockCountHeader` + `StockCountItem` — رأس وتفاصيل الجرد المخزني
- [ ] `Employee` — الموظف (EmployeeCode, BranchId, DepartmentId, BasicSalary)
- [ ] `Department` — الأقسام مرتبطة بالفروع
- [ ] `Attendance` — الحضور والانصراف
- [ ] `MonthlySalary` — الرواتب الشهرية
- [ ] `EmployeeLoan` — السلف والقروض
- [ ] `CustomerLedger` — حساب العميل الموحد
- [ ] `InterBranchSettlement` — تسوية ما بين الفروع
- [ ] `DailyClosing` — إغلاق اليومية

### B1.2 - تحديث ApplicationDbContext
- [ ] إضافة DbSets لجميع الكيانات الجديدة
- [ ] إعداد العلاقات (Relationships) في `OnModelCreating`
- [ ] تطبيق Global Query Filters لتصفية البيانات حسب الفرع

### B1.3 - Migration وقاعدة البيانات
- [ ] إنشاء Migration جديدة: `Add_MultiBranch_Inventory_HR`
- [ ] تشغيل Migration وإنشاء الجداول
- [ ] إضافة Seed Data: فرع رئيسي + فرع تجريبي + مخزن لكل منهما

---

## 🔧 المرحلة B2: واجهات الخدمات (IServices - Application Layer)

- [ ] `IBranchService` — CRUD الفروع + تفعيل/تعطيل
- [ ] `IWarehouseService` — CRUD المخازن
- [ ] `IInventoryService` — استعلام الأرصدة + بطاقة الصنف
- [ ] `IStockTransferService` — إنشاء/اعتماد/رفض/شحن/استلام سندات التحويل
- [ ] `IDamagedGoodsService` — تسجيل التالف + اعتماد إذن الإتلاف
- [ ] `IStockCountService` — إنشاء + تسجيل نتائج + اعتماد الجرد
- [ ] `IEmployeeService` — CRUD الموظفين + تعيين الفرع
- [ ] `IAttendanceService` — تسجيل الحضور والانصراف
- [ ] `IPayrollService` — إنشاء + اعتماد + صرف الرواتب الشهرية
- [ ] `ICustomerLedgerService` — استعلام + سداد عابر للفروع
- [ ] `IDailyClosingService` — إنشاء + إرسال + اعتماد اليومية

---

## ⚙️ المرحلة B3: تطبيق الخدمات (Service Implementations)

### B3.1 - StockTransferService (الأهم)
- [ ] `CreateRequest()` — إنشاء طلب تحويل يدوي
- [ ] `Approve()` — اعتماد الطلب من المدير
- [ ] `Dispatch()` — الصرف من المستودع (خصم من المرسِل فوراً، حالة InTransit)
- [ ] `ConfirmReceipt()` — تأكيد الاستلام بالكمية الفعلية (إضافة لمخزن الفرع)
- [ ] `HandleVariance()` — تسجيل عجز الشحن وإشعار الإدارة

### B3.2 - DamagedGoodsService
- [ ] `RegisterDamage()` — تسجيل التالف (بحالة PendingApproval)
- [ ] `Approve()` — الاعتماد (خصم من المخزن الأصلي، نقل لمخزن التالف)
- [ ] تسجيل قيد محاسبي: "خسارة توالف"

### B3.3 - CustomerLedgerService
- [ ] `GetCustomerBalance()` — رصيد العميل الموحد عبر الفروع
- [ ] `ReceivePayment(customerId, branchId, amount)` — السداد في أي فرع
- [ ] `CreateInterBranchSettlement()` — تسوية تلقائية بين الفروع

### B3.4 - DailyClosingService
- [ ] `GenerateDraftClosing(branchId, date)` — حساب أرقام اليومية تلقائياً
- [ ] `SubmitForApproval()` — إرسال لإدارة الفرع الرئيسي
- [ ] `Approve()` — اعتماد وقفل المحاسبة

---

## 🔄 المرحلة B4: الخدمات الخلفية (Background Services)

### B4.1 - ExpiryAlertService (يعمل يومياً 00:01)
```
لكل تشغيلة في InventoryStock:
  إذا ExpiryDate - اليوم <= 30 يوم:
    قفل البيع + طلب نقل لمخزن التالف
  إذا <= 90 يوم:
    إشعار برتقالي للمدير
  إذا <= 180 يوم:
    إشعار أصفر
```

### B4.2 - AutoReplenishmentService (يعمل بعد كل عملية بيع)
```
لكل مخزن فرع بعد البيع:
  إذا Quantity <= ReorderLevel:
    إنشاء StockTransfer تلقائي (AutoRequested)
    إشعار للمدير العام
```

### B4.3 - DailyClosingReminderService (يعمل يومياً 20:00)
```
لكل فرع لم يُغلق يوميته:
  إرسال إشعار لمدير الفرع
```

---

## 🌐 المرحلة B5: الـ Controllers (API Endpoints)

### B5.1 - BranchController
- [ ] `GET /api/branches` — قائمة الفروع
- [ ] `POST /api/branches` — إنشاء فرع
- [ ] `PUT /api/branches/{id}` — تعديل
- [ ] `DELETE /api/branches/{id}` — حذف

### B5.2 - InventoryController
- [ ] `GET /api/inventory/stock?warehouseId=&medicineId=` — الرصيد الحالي
- [ ] `GET /api/inventory/stock-card?medicineId=&branchId=` — بطاقة الصنف

### B5.3 - StockTransferController
- [ ] `POST /api/stock-transfers` — إنشاء طلب
- [ ] `PUT /api/stock-transfers/{id}/approve` — اعتماد
- [ ] `PUT /api/stock-transfers/{id}/dispatch` — صرف
- [ ] `PUT /api/stock-transfers/{id}/receive` — تأكيد استلام
- [ ] `GET /api/stock-transfers?status=&branchId=` — استعلام

### B5.4 - DamagedGoodsController
- [ ] `POST /api/damaged-goods` — تسجيل تالف
- [ ] `PUT /api/damaged-goods/{id}/approve` — اعتماد
- [ ] `GET /api/damaged-goods/report?branchId=&from=&to=` — تقرير

### B5.5 - StockCountController
- [ ] `POST /api/stock-counts` — إنشاء أمر جرد
- [ ] `POST /api/stock-counts/{id}/items` — إدخال كميات الجرد الفعلية
- [ ] `PUT /api/stock-counts/{id}/submit` — إرسال للاعتماد
- [ ] `PUT /api/stock-counts/{id}/approve` — اعتماد + تسوية الأرصدة

### B5.6 - EmployeeController
- [ ] `GET /api/employees?branchId=` — قائمة الموظفين بالفرع
- [ ] `POST /api/employees` — إضافة موظف
- [ ] `PUT /api/employees/{id}/transfer-branch` — نقل موظف لفرع آخر

### B5.7 - PayrollController
- [ ] `POST /api/payroll/generate?month=&year=` — توليد رواتب شهر
- [ ] `PUT /api/payroll/{id}/approve` — اعتماد
- [ ] `GET /api/payroll/report?branchId=&month=` — تقرير الرواتب

### B5.8 - CustomerLedgerController
- [ ] `GET /api/customers/{id}/balance` — رصيد العميل الموحد
- [ ] `POST /api/customers/{id}/receive-payment` — استقبال سداد من أي فرع

### B5.9 - DailyClosingController
- [ ] `POST /api/daily-closing/generate?branchId=&date=` — توليد مسودة
- [ ] `PUT /api/daily-closing/{id}/submit` — إرسال للاعتماد
- [ ] `PUT /api/daily-closing/{id}/approve` — اعتماد
- [ ] `GET /api/daily-closing/summary?date=` — ملخص كل الفروع ليوم واحد

---

## ═══════════════════════════════════
## 🟢 الجزء الثاني: FRONTEND (Angular)
## ═══════════════════════════════════

---

## 🎨 المرحلة F1: البنية التحتية للواجهة

### F1.1 - Services & Models
- [ ] إنشاء models/interfaces لكل كيان
- [ ] `BranchService` — API calls للفروع
- [ ] `InventoryService` — API calls للمخزون
- [ ] `StockTransferService` — API calls للتحويلات
- [ ] `DamagedGoodsService` — API calls للتوالف
- [ ] `StockCountService` — API calls للجرد
- [ ] `EmployeeService` — API calls للموظفين
- [ ] `PayrollService` — API calls للرواتب
- [ ] `CustomerLedgerService` — API calls للحسابات
- [ ] `DailyClosingService` — API calls لإغلاق اليومية

### F1.2 - Auth & Branch Context
- [ ] حفظ `BranchId` و`BranchCode` في JWT + LocalStorage عند تسجيل الدخول
- [ ] إضافة HTTP Interceptor يُرسل `X-Branch-Id` header مع كل طلب
- [ ] `BranchContextService` يُتيح لأي مكوّن معرفة الفرع الحالي

---

## 🖥️ المرحلة F2: وحدة الفروع والمخازن

### F2.1 - شاشة إدارة الفروع (SuperAdmin فقط)
- [ ] جدول الفروع مع كود الفرع ونوعه وحالته
- [ ] نموذج إنشاء/تعديل فرع
- [ ] مؤشرات سريعة: عدد الموظفين، المبيعات اليوم، تنبيهات المخزون

### F2.2 - شاشة المخزون الحالي
- [ ] فلترة بالمخزن + الدواء + التشغيلة + تاريخ الصلاحية
- [ ] مؤشر ملون: 🔴 أقل من حد الأمان / 🟡 قريب / 🟢 آمن
- [ ] زر "عرض بطاقة الصنف" لكل دواء

### F2.3 - بطاقة الصنف (Stock Card)
- [ ] جدول تاريخي بكل حركات الدواء (توريد، تحويل، بيع، تالف، جرد)
- [ ] فلترة بالفرع + التاريخ + نوع الحركة
- [ ] رصيد تراكمي مُحسَّب

---

## 🔄 المرحلة F3: وحدة التحويلات المخزنية

### F3.1 - شاشة طلبات التحويل
- [ ] جدول السندات مع الحالة ملونة (Requested/InTransit/Received)
- [ ] فلترة بالفرع المصدر/الوجهة + الحالة + التاريخ
- [ ] زر "اعتماد" (للمدير العام) + زر "صرف" (لأمين المخزن)

### F3.2 - شاشة إنشاء طلب تحويل
- [ ] اختيار المخزن المصدر والوجهة
- [ ] إضافة بنود (دواء + تشغيلة + كمية)
- [ ] حساب التوفر الفعلي قبل الإرسال

### F3.3 - شاشة تأكيد الاستلام
- [ ] عرض البنود المشحونة
- [ ] إدخال الكمية المستلمة فعلياً لكل بند
- [ ] تمييز الفروقات بالأحمر + زر "تأكيد الاستلام"

---

## ☠️ المرحلة F4: وحدة التالف وانتهاء الصلاحية

### F4.1 - شاشة تسجيل التالف
- [ ] اختيار الدواء + التشغيلة + الكمية + سبب التلف
- [ ] حساب القيمة المالية تلقائياً (بسعر الشراء)
- [ ] إرسال لاعتماد المدير

### F4.2 - شاشة قائمة التوالف
- [ ] جدول كل سجلات التلف مع الحالة (بانتظار/معتمد)
- [ ] تقرير: إجمالي قيمة التوالف (شهري/سنوي) لكل فرع
- [ ] مقارنة نسبة التالف بين الفروع

### F4.3 - لوحة تنبيهات الصلاحية
- [ ] قائمة الأدوية قريبة الانتهاء مرتبة حسب الأخطر
- [ ] بطاقات ملونة 🔴🟠🟡 حسب الأشهر المتبقية
- [ ] زر "سجّل للإتلاف" + زر "اطلب إرجاع للمورد"

---

## 📋 المرحلة F5: وحدة الجرد المخزني

### F5.1 - شاشة إنشاء أمر الجرد
- [ ] اختيار المخزن + نوع الجرد + الفترة
- [ ] تصدير قائمة الجرد كـ Excel/PDF

### F5.2 - شاشة إدخال نتائج الجرد
- [ ] عرض الكمية النظامية المتوقعة لكل صنف
- [ ] حقل لإدخال الكمية الفعلية المعدودة
- [ ] حساب الفارق تلقائياً ملوناً بالأحمر/الأخضر

### F5.3 - تقرير الجرد الشامل
- [ ] رصيد افتتاحي + واردات + صادرات + رصيد نظامي + رصيد فعلي + فارق + قيمة الفارق
- [ ] جدول مالي قابل للطباعة

---

## 👥 المرحلة F6: وحدة الموظفين والرواتب (HR)

### F6.1 - شاشة الموظفين
- [ ] جدول بكل الموظفين مع الفرع والقسم والمسمى الوظيفي
- [ ] فلترة بالفرع + القسم + الحالة
- [ ] بطاقة الموظف: بيانات، راتب، سلف، إجازات

### F6.2 - شاشة الحضور والغياب
- [ ] تسجيل حضور/غياب يومي لكل موظف
- [ ] تقرير شهري: أيام الحضور/الغياب/التأخير لكل موظف

### F6.3 - شاشة توليد الرواتب
- [ ] اختيار الشهر والسنة وإنشاء كشف الرواتب تلقائياً
- [ ] عرض: الأساسي + البدلات + الحوافز - الخصومات - أقساط السلف = الصافي
- [ ] زر اعتماد وطباعة قسائم الرواتب

---

## 💰 المرحلة F7: وحدة الحسابات والعملاء

### F7.1 - شاشة حساب العميل الموحد
- [ ] بحث بالاسم أو رقم الهاتف
- [ ] عرض كل الفواتير والمدفوعات من جميع الفروع
- [ ] الرصيد الإجمالي المستحق

### F7.2 - شاشة استقبال السداد العابر
- [ ] اختيار العميل + إدخال المبلغ
- [ ] النظام يُوزع السداد على الأقدم فاتورةً أولاً
- [ ] طباعة إيصال السداد مع الفرع الذي تم فيه

---

## 📊 المرحلة F8: لوحات التحكم والتقارير

### F8.1 - لوحة تحكم الفرع (Branch Dashboard)
- [ ] مبيعات اليوم (كاش / دين / بطاقة)
- [ ] تنبيهات المخزون (أقل من حد الأمان / قارب الانتهاء)
- [ ] طلبات تحويل بانتظار الاستلام
- [ ] إشعارات التوالف بانتظار الاعتماد

### F8.2 - لوحة تحكم الإدارة العامة (Multi-Branch Dashboard)
- [ ] مقارنة مبيعات جميع الفروع في شريط واحد
- [ ] أكثر الأدوية مبيعاً عبر الفروع
- [ ] فروع بها تنبيهات مخزون حرجة
- [ ] ملخص يوميات الفروع بانتظار الاعتماد

### F8.3 - شاشة إغلاق اليومية
- [ ] حقل "الكاش الفعلي بالصندوق"
- [ ] مقارنة تلقائية بالكاش المتوقع + إظهار الفارق
- [ ] زر "إرسال للاعتماد"

---

## ✅ ترتيب التنفيذ المقترح (أولوية التسليم)

```
الأسبوع 1-2:  B1 (Entities + Migration + Seed Data)
الأسبوع 3:    B2 + B3 (IServices + Implementations) - الأساسيات
الأسبوع 4:    B5.1 → B5.5 (Core API Controllers)
الأسبوع 5:    B4 (Background Services)
الأسبوع 6:    B5.6 → B5.9 (HR + Payroll + Closing APIs)
الأسبوع 7:    F1 + F1.2 (Frontend Infrastructure + Auth)
الأسبوع 8:    F2 + F3 (Inventory + Transfers Screens)
الأسبوع 9:    F4 + F5 (Damaged + Stock Count)
الأسبوع 10:   F6 + F7 (HR + Customer Ledger)
الأسبوع 11:   F8 (Dashboards + EOD Report)
الأسبوع 12:   Testing + Bug Fixes + Final Review
```
