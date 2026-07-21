# الدليل المرجعي الشامل للنظام الذكي لإدارة الصيدليات
## Smart Pharmacy ERP - Master Architecture Plan (v3.0 Final)

> هذا المستند هو المرجع الوحيد والمتكامل لتصميم النظام بالكامل، يشمل: الفروع، المخازن، الموردين، التحويلات، التالف، الجرد، الموظفين، والمالية.

---

## القسم الأول: الفروع والأكواد الموحدة

### 1.1 جدول الفروع (`Branch`)
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `Id` | int PK | المعرف الداخلي |
| `BranchCode` | string UNIQUE | كود الفرع الموحد في كل التقارير (مثال: `10001` للرئيسي، `34233` لفرع أ) |
| `Name` | string | اسم الفرع |
| `Location` | string | العنوان |
| `BranchType` | enum | `Main` أو `Sub` |
| `IsActive` | bool | نشط/موقوف |

> **ملاحظة هامة:** الفرع الرئيسي (BranchType = Main) يملك نفس قدرات الفروع الأخرى (البيع للجمهور، صندوق نقدي، كاشير)، **لكنه يمتلك إضافةً صلاحيات التوزيع والتوريد المركزي** التي لا تمتلكها الفروع الأخرى. لا يوجد فرق في بنية قاعدة البيانات، الفرق فقط في الصلاحيات.

### 1.2 الكود الموحد للدواء (Unified Medicine SKU)
كل دواء له كود موحد واحد يُستخدم في كافة الفروع والتقارير مما يسهّل فلترة "كم بيع هذا الدواء عبر جميع الفروع".

---

## القسم الثاني: هيكل المخازن الشجري

### 2.1 أنواع المخازن
```csharp
public enum WarehouseType
{
    Main    = 1,  // مخزن الفرع الرئيسي (يبيع ويوزع)
    Branch  = 2,  // مخزن فرع عادي (يبيع فقط)
    Damaged = 3   // مخزن التالف والمنتهي (لا يبيع، يُتلف أو يُرجع للمورد)
}
```

### 2.2 جدول المخازن (`Warehouse`)
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `Id` | int PK | |
| `BranchId` | int FK | الفرع المالك لهذا المخزن |
| `Type` | WarehouseType | نوع المخزن |
| `Name` | string | اسم المخزن |

### 2.3 جدول رصيد المخزون (`InventoryStock`)
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `Id` | int PK | |
| `WarehouseId` | int FK | المخزن |
| `MedicineId` | int FK | الدواء |
| `BatchNumber` | string | رقم التشغيلة |
| `ExpiryDate` | DateTime | تاريخ انتهاء الصلاحية |
| `Quantity` | int | الكمية الفعلية الحالية |
| `ReorderLevel` | int | **حد الأمان الخاص بهذا الدواء في هذا المخزن** (مختلف لكل دواء) |
| `ReorderQuantity` | int | الكمية المقترحة عند الطلب التلقائي |

---

## القسم الثالث: الشراء المركزي والتوريد التلقائي

### 3.1 دورة الشراء المركزي
1. يستقبل الفرع الرئيسي البضاعة من المورد → تدخل لمخزنه.
2. أي فرع يحتاج أدوية يطلبها من الفرع الرئيسي عبر سند تحويل.
3. الفروع لا تشتري من الموردين مباشرة (في الوضع المركزي).

### 3.2 محرك التوريد التلقائي (Auto-Replenishment Engine)
**الآلية:** بعد كل عملية بيع، يتحقق النظام:
```
إذا (InventoryStock.Quantity <= InventoryStock.ReorderLevel):
    ← أنشئ StockTransfer تلقائياً
    ← Status = AutoRequested
    ← Source = مخزن الفرع الرئيسي
    ← Destination = مخزن الفرع الحالي
    ← Quantity = ReorderQuantity
    ← أرسل إشعار للمدير العام
```

---

## القسم الرابع: التحويلات بين المخازن وقاعدة الاستلام/التسليم

### 4.1 جدول سندات التحويل (`StockTransfer`)
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `Id` | int PK | |
| `TransferCode` | string | رقم السند الفريد |
| `SourceWarehouseId` | int FK | المخزن المُرسِل |
| `DestinationWarehouseId` | int FK | المخزن المستقبِل |
| `Status` | enum | الحالة التفصيلية (انظر 4.2) |
| `TransferType` | enum | `Manual / AutoRequested / BranchRequest` |
| `RequestedByUserId` | int FK | من أنشأ الطلب |
| `DispatchedByUserId` | int FK | أمين المخزن الذي صرف |
| `DispatchedAt` | DateTime | وقت الإرسال |
| `ReceivedByUserId` | int FK | موظف الفرع المستلم |
| `ReceivedAt` | DateTime | وقت تأكيد الاستلام |
| `Notes` | string | |

### 4.2 حالات سند التحويل
```csharp
public enum TransferStatus
{
    AutoRequested = 0,  // طلب تلقائي بانتظار مراجعة المدير
    Requested     = 1,  // طلب يدوي من الفرع
    Approved      = 2,  // اعتمد من الإدارة
    Dispatched    = 3,  // خرجت البضاعة من المخزن المرسل
    InTransit     = 4,  // البضاعة في الطريق (لا تُحسب في أي مخزن)
    Received      = 5,  // تأكيد الاستلام من الفرع
    Cancelled     = 6   // ملغى
}
```

### 4.3 بنود سند التحويل (`StockTransferItem`)
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `StockTransferId` | int FK | |
| `MedicineId` | int FK | |
| `BatchNumber` | string | |
| `ExpiryDate` | DateTime | |
| `QuantityRequested` | int | المطلوب |
| `QuantityDispatched` | int | المشحون فعلياً |
| `QuantityReceived` | int | المستلم فعلياً بعد العد |

### 4.4 قاعدة الاستلام والتسليم الصارمة
- ✅ **الصرف:** لا تخرج بضاعة بدون سند صرف معتمد → تُخصم من المرسِل فوراً → حالة `InTransit`
- ✅ **الاستلام:** لا تدخل الكمية لرصيد الفرع قبل ضغط زر "تأكيد الاستلام"
- ✅ **الفوارق:** إذا `QuantityReceived < QuantityDispatched` → يُسجَّل عجز → يُرسَل إشعار للإدارة

---

## القسم الخامس: خدمة تسجيل البضاعة التالفة وانتهاء الصلاحية

### 5.1 جدول سجل التوالف (`DamagedGoodsRecord`)
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `Id` | int PK | |
| `DamageCode` | string | رقم إذن الإتلاف |
| `SourceWarehouseId` | int FK | المخزن الذي خرجت منه البضاعة التالفة |
| `MedicineId` | int FK | |
| `BatchNumber` | string | |
| `ExpiryDate` | DateTime | |
| `Quantity` | int | الكمية التالفة |
| `DamageType` | enum | `Expired / PhysicalDamage / ManufacturingDefect` |
| `DamageValue` | decimal | القيمة المالية للتالف (بسعر الشراء) |
| `DisposalMethod` | enum | `Destroyed / ReturnedToSupplier / Donated` |
| `ApprovedByUserId` | int FK | المدير الذي اعتمد إذن الإتلاف |
| `RecordedByUserId` | int FK | الموظف المسجل |
| `RecordedAt` | DateTime | |

### 5.2 آلية تسجيل التالف
```
1. الموظف يسجل الدواء التالف في شاشة "التوالف"
2. يختار السبب (تلف جسدي / انتهاء صلاحية)
3. يرسل الطلب للمدير بحالة "بانتظار الاعتماد"
4. المدير يراجع ويعتمد
5. عند الاعتماد:
   - تُخصم الكمية من InventoryStock للمخزن المصدر
   - تنتقل الكمية إلى مخزن التالف (WarehouseType = Damaged)
   - يُسجَّل قيد محاسبي: "خسارة تالف" في الحسابات
```

### 5.3 محرك تنبيهات انتهاء الصلاحية (Expiry Alert Engine)
يعمل يومياً في الخلفية (Background Service) ويتحقق من جميع التشغيلات:
- 🟡 **قبل 6 أشهر:** تنبيه أصفر للمدير "الدواء X ينتهي بعد 180 يوم، الكمية: 90 كرتون - يُنصح بعمل عرض تخفيض"
- 🟠 **قبل 3 أشهر:** تنبيه برتقالي "تنبيه حرج - ينصح بإرجاع المورد أو التخفيض الفوري"
- 🔴 **قبل شهر:** قفل تلقائي للبيع + طلب نقل لمخزن التالف تلقائياً

### 5.4 تقرير التوالف والعجز (Damage & Loss Report)
يوضح للإدارة في أي وقت:
- إجمالي قيمة التوالف لكل فرع (شهرياً/سنوياً)
- نسبة التالف إلى المشتريات (Loss Ratio)
- أكثر الأدوية تلفاً
- حالات الفوارق في التحويلات (عجز الشحن)

---

## القسم السادس: الجرد المخزني (Physical Stock Count / Inventory Audit)

### 6.1 أنواع الجرد
```csharp
public enum StockCountType
{
    Daily    = 1, // جرد يومي لصنف بعينه
    Monthly  = 2, // جرد شهري
    BiAnnual = 3, // جرد نصف سنوي
    Annual   = 4  // جرد سنوي
}
```

### 6.2 جدول رأس الجرد (`StockCountHeader`)
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `Id` | int PK | |
| `CountCode` | string | رقم أمر الجرد |
| `WarehouseId` | int FK | المخزن المجرود |
| `CountType` | StockCountType | |
| `StartedAt` | DateTime | بداية الجرد |
| `FinishedAt` | DateTime | نهاية الجرد |
| `Status` | enum | `Draft / InProgress / PendingApproval / Approved / Closed` |
| `ApprovedByUserId` | int FK | |
| `Notes` | string | |

### 6.3 جدول تفاصيل الجرد (`StockCountItem`)
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `StockCountHeaderId` | int FK | |
| `MedicineId` | int FK | |
| `BatchNumber` | string | |
| `ExpiryDate` | DateTime | |
| `SystemQuantity` | int | الكمية المتوقعة حسب النظام |
| `PhysicalQuantity` | int | الكمية الفعلية المعدودة |
| `Variance` | int (محسوب) | `PhysicalQuantity - SystemQuantity` |
| `VarianceValue` | decimal | قيمة الفارق المالي |
| `VarianceReason` | string | سبب الفارق |

### 6.4 دورة عمل الجرد
```
1. المدير يفتح "أمر جرد جديد" ويختار المخزن ونوع الجرد
2. النظام يُجمّد العمليات (اختياري) أو يُحدد وقت الجرد
3. الموظفون يدخلون الكميات الفعلية المعدودة لكل صنف وتشغيلة
4. النظام يحسب تلقائياً: الكمية المتوقعة، الفارق، القيمة المالية للفارق
5. يرسل تقرير للإدارة بحالة "بانتظار الاعتماد"
6. المدير يراجع الفوارق ويعتمد التسوية
7. عند الاعتماد: يتم تعديل أرصدة المخزون لتطابق الواقع الفعلي
8. يُسجَّل قيد محاسبي: "عجز جرد" أو "فائض جرد"
```

### 6.5 تقرير الجرد الشامل (Audit Report)
يُظهر لكل صنف في كل مخزن:
- الرصيد الافتتاحي (أول الفترة)
- إجمالي الواردات (مشتريات + تحويلات واردة)
- إجمالي الصادرات (مبيعات + تحويلات صادرة + توالف)
- **الرصيد النظري المتوقع**
- الرصيد الفعلي المعدود
- **الفارق والقيمة المالية للفارق**

---

## القسم السابع: إدارة الموظفين والرواتب (HR & Payroll Module)

### 7.1 جدول الموظفين (`Employee`)
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `Id` | int PK | |
| `EmployeeCode` | string UNIQUE | رقم الموظف |
| `FullName` | string | |
| `NationalId` | string | رقم الهوية |
| `BranchId` | int FK | الفرع التابع له |
| `DepartmentId` | int FK | القسم (صيدلاني، محاسب، مخازن...) |
| `JobTitle` | string | المسمى الوظيفي |
| `HireDate` | DateTime | تاريخ التوظيف |
| `BasicSalary` | decimal | الراتب الأساسي |
| `IsActive` | bool | |

### 7.2 جدول الأقسام (`Department`)
| الحقل | الوصف |
|:---|:---|
| `Id` | |
| `BranchId` | الفرع التابع له القسم |
| `Name` | مثال: "الصيدلة"، "المخازن"، "المحاسبة"، "التوصيل" |

### 7.3 جدول النوبات والحضور (`Attendance`)
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `Id` | int PK | |
| `EmployeeId` | int FK | |
| `BranchId` | int FK | قد يكون الموظف مُعارَاً لفرع مؤقتاً |
| `CheckIn` | DateTime | وقت الحضور |
| `CheckOut` | DateTime | وقت الانصراف |
| `WorkedHours` | decimal | محسوبة تلقائياً |
| `Shift` | enum | `Morning / Evening / Night` |
| `AttendanceStatus` | enum | `Present / Absent / Late / Excused` |

### 7.4 جدول الرواتب الشهرية (`MonthlySalary`)
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `Id` | int PK | |
| `EmployeeId` | int FK | |
| `BranchId` | int FK | الفرع الذي يُحمَّل عليه الراتب (مهم للتقارير المالية الفرعية) |
| `Month` | int | |
| `Year` | int | |
| `BasicSalary` | decimal | الراتب الأساسي |
| `Allowances` | decimal | بدلات (سكن، مواصلات) |
| `Bonuses` | decimal | حوافز وعمولات |
| `Deductions` | decimal | خصومات (غياب، تأخير، سلفة) |
| `NetSalary` | decimal | الراتب الصافي |
| `PaymentStatus` | enum | `Pending / Paid` |
| `PaidAt` | DateTime | |

### 7.5 جدول السلف والقروض (`EmployeeLoan`)
| الحقل | الوصف |
|:---|:---|
| `EmployeeId` | |
| `Amount` | المبلغ |
| `MonthlyDeduction` | القسط الشهري المخصوم تلقائياً من الراتب |
| `RemainingAmount` | المتبقي |

---

## القسم الثامن: الحسابات المالية الموحدة

### 8.1 حساب العملاء الموحد (`CustomerLedger`)
| الحقل | الوصف |
|:---|:---|
| `CustomerId` | معرف العميل الموحد عبر جميع الفروع |
| `BranchId` | الفرع الذي جرت فيه العملية (شراء أو سداد) |
| `TransactionType` | `Invoice` (دين) أو `Receipt` (سداد) |
| `Debit` | المبيعات الآجلة (تزيد الدين) |
| `Credit` | السداد (تقلل الدين) |
| `BalanceAfter` | الرصيد الحالي للعميل |

**السداد العابر للفروع:** العميل يشتري بالدين من فرع أ ويسدد في فرع ب ← يقبل النظام السداد ← يُسجل قيد تسوية داخلية بين الفرعين.

### 8.2 تسوية ما بين الفروع (`InterBranchSettlement`)
| الحقل | الوصف |
|:---|:---|
| `FromBranchId` | الفرع المدين (استلم سداد لصالح فرع آخر) |
| `ToBranchId` | الفرع الدائن |
| `Amount` | المبلغ |
| `SettlementDate` | تاريخ التسوية |
| `Status` | `Pending / Settled` |

---

## القسم التاسع: إغلاق اليومية وتقرير الفرع اليومي (EOD Report)

### 9.1 جدول إغلاق اليومية (`DailyClosing`)
| الحقل | الوصف |
|:---|:---|
| `BranchId` | |
| `ClosingDate` | التاريخ |
| `Status` | `Draft / PendingApproval / Approved` |
| `TotalCashSales` | إجمالي المبيعات النقدية |
| `TotalCreditSales` | إجمالي المبيعات الآجلة |
| `TotalCardSales` | إجمالي المبيعات بالبطاقة |
| `TotalCollections` | إجمالي الديون المحصلة نقداً |
| `TotalExpenses` | إجمالي المصروفات النقدية |
| `ExpectedCash` | `= CashSales + Collections - Expenses` |
| `ActualCash` | الكاش الفعلي في الصندوق (يُدخله الكاشير) |
| `CashVariance` | `= ActualCash - ExpectedCash` |
| `SubmittedByUserId` | الكاشير أو مدير الفرع |
| `ApprovedByUserId` | الإدارة العامة |
| `ApprovedAt` | |

### 9.2 دورة اعتماد اليومية
```
1. نهاية اليوم: الكاشير يُدخل الكاش الفعلي في الصندوق
2. النظام يحسب الكاش المتوقع ويظهر الفارق
3. مدير الفرع يراجع ويرسل للإدارة العامة بحالة "PendingApproval"
4. الإدارة العامة في الفرع الرئيسي ترى يومية جميع الفروع
5. بعد مراجعة أي فوارق، تعتمد الإدارة → حالة "Approved"
6. تُغلق المحاسبة وتُنقل الأرقام للسجلات الشهرية
```

---

## القسم العاشر: بطاقة الصنف الكاملة (Complete Stock Card)

لكل دواء في أي فرع، يمكن رؤية سجل حركته الكامل:

| التاريخ | كود الفرع | نوع الحركة | السند | +/- | الرصيد |
|:---|:---|:---|:---|:---|:---|
| 2026-05-01 | `10001` | توريد (مشتريات) | فاتورة #901 | +500 | 500 |
| 2026-05-03 | `10001` | صرف تحويل | سند تحويل #12 | -50 | 450 |
| 2026-05-03 | `34233` | استلام تحويل | سند تحويل #12 | +50 | 50 |
| 2026-05-04 | `34233` | مبيعات | فاتورة #4001 | -46 | 4 ← أقل من حد الأمان! |
| 2026-05-04 | `34233` | طلب تلقائي | سند تحويل #13 Auto | مطلوب 50 | - |
| 2026-05-10 | `34233` | تالف (انتهاء صلاحية) | إذن إتلاف #7 | -2 | 2 |
| 2026-05-30 | `34233` | جرد فعلي (فائض) | أمر جرد #J01 | +1 | 3 |

---

## القسم الحادي عشر: الصلاحيات والأدوار (Roles & Permissions)

| الدور | الصلاحيات |
|:---|:---|
| `SuperAdmin` | كل شيء - الإدارة العامة |
| `MainBranchManager` | كل شيء في الفرع الرئيسي + اعتماد يوميات الفروع + إدارة التوزيع |
| `BranchManager` | إدارة كاملة لفرعه فقط |
| `Pharmacist` | البيع + إدخال مبيعات + مشاهدة مخزون فرعه |
| `StoreKeeper` | استلام + تحويل + جرد المخزن |
| `Cashier` | إغلاق اليومية + الصندوق |
| `Accountant` | التقارير المالية + الرواتب |

---

## القسم الثاني عشر: خطة التنفيذ البرمجي

### المرحلة الأولى - قاعدة البيانات والـ Backend
1. `Branch` + `Warehouse` + `InventoryStock` (مع `ReorderLevel` لكل دواء)
2. `StockTransfer` + `StockTransferItem`
3. `DamagedGoodsRecord`
4. `StockCountHeader` + `StockCountItem`
5. `Employee` + `Department` + `Attendance` + `MonthlySalary`
6. `CustomerLedger` + `InterBranchSettlement`
7. `DailyClosing`

### المرحلة الثانية - خدمات الخلفية (Background Services)
- `ExpiryAlertService` → يعمل يومياً ليلاً
- `AutoReplenishmentService` → يعمل بعد كل عملية بيع
- `DailyClosingReminderService` → يُرسل تذكير بنهاية اليوم

### المرحلة الثالثة - الواجهة الأمامية (Angular)
- لوحة تحكم الفرع (مبيعات اليوم، تنبيهات المخزون، طلبات التحويل)
- شاشة إدارة المخزون + التحويلات
- شاشة التوالف وإذن الإتلاف
- شاشة الجرد المخزني
- شاشة الموظفين والرواتب
- شاشة إغلاق اليومية
- لوحة تحكم الإدارة العامة (مقارنة الفروع)
