# الدليل المرجعي المُصحَّح - الجزء الثاني (v4.0)
## Smart Pharmacy ERP — Sections 7 to 12 + الوحدات المفقودة

---

## القسم 7: الموظفين والرواتب (HR & Payroll)

### 7.1 جدول `Department` (مُصحَّح — مركزي عالمي)
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `Id` | int PK | |
| `Name` | string | مثال: "الصيدلة"، "المحاسبة"، "المخازن"، "التوصيل" |

> **✅ تصحيح الخطأ 5:** حُذف `BranchId` من جدول الأقسام. الأقسام مركزية عالمية. الموظف نفسه هو من يرتبط بالفرع والقسم معاً.

### 7.2 جدول `Employee`
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `Id` | int PK | |
| `EmployeeCode` | string UNIQUE | رقم الموظف |
| `FullName` | string | |
| `NationalId` | string | |
| `BranchId` | int FK | الفرع الأساسي للموظف |
| `DepartmentId` | int FK | القسم (من الجدول المركزي) |
| `JobTitle` | string | |
| `HireDate` | DateTime | |
| `TerminationDate` | DateTime Nullable | ✅ مُضاف — تاريخ إنهاء الخدمة |
| `BasicSalary` | decimal | |
| `IsActive` | bool | |

### 7.3 جدول `Attendance`
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `Id` | int PK | |
| `EmployeeId` | int FK | |
| `WorkingBranchId` | int FK | الفرع الفعلي للحضور (قد يختلف عن فرع الموظف الأساسي عند الإعارة) |
| `CheckIn` | DateTime | |
| `CheckOut` | DateTime Nullable | |
| `WorkedHours` | decimal محسوب | |
| `Shift` | enum `Morning/Evening/Night` | |
| `AttendanceStatus` | enum `Present/Absent/Late/Excused` | |

### 7.4 جدول `MonthlySalary` (رأس الراتب)
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `Id` | int PK | |
| `EmployeeId` | int FK | |
| `BranchId` | int FK | الفرع الذي يُحمَّل عليه الراتب |
| `Month` | int | |
| `Year` | int | |
| `BasicSalary` | decimal | |
| `TotalAllowances` | decimal | مجموع البدلات |
| `TotalBonuses` | decimal | مجموع الحوافز |
| `TotalDeductions` | decimal | مجموع الخصومات (محسوب من التفاصيل) |
| `NetSalary` | decimal | `BasicSalary + Allowances + Bonuses - Deductions` |
| `PaymentStatus` | enum `Pending/Paid` | |
| `PaidAt` | DateTime Nullable | |

### 7.5 ✅ جدول جديد: `SalaryDeductionItem` (تفاصيل الخصومات)
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `Id` | int PK | |
| `MonthlySalaryId` | int FK | |
| `DeductionType` | enum `Absence/LateArrival/LoanInstalment/Insurance/Tax/Other` | |
| `Amount` | decimal | |
| `Description` | string | |

> **✅ تصحيح:** لا يكفي حقل `Deductions` واحد. كل خصم يجب أن يكون سجلاً منفصلاً بنوعه ومبلغه لأغراض التدقيق والتقارير.

### 7.6 جدول `EmployeeLoan` (السلف)
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `Id` | int PK | |
| `EmployeeId` | int FK | |
| `Amount` | decimal | إجمالي السلفة |
| `MonthlyInstalment` | decimal | القسط الشهري |
| `RemainingAmount` | decimal | المتبقي للسداد |
| `StartMonth` | int | |
| `StartYear` | int | |
| `IsFullyPaid` | bool | |

---

## القسم 8: الحسابات المالية الموحدة

### 8.1 جدول `CustomerLedger` (مُصحَّح)
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `Id` | int PK | |
| `CustomerId` | int FK | |
| `BranchId` | int FK | الفرع الذي جرت فيه العملية |
| `TransactionDate` | DateTime | |
| `TransactionType` | enum `Invoice/Receipt/Return` | |
| `ReferenceId` | int | معرف الفاتورة أو سند القبض |
| `Debit` | decimal | مبيعات آجلة (تزيد الدين) |
| `Credit` | decimal | سداد أو مرتجع (تُقلل الدين) |

> **✅ تصحيح الخطأ 7:** حُذف حقل `BalanceAfter`. رصيد العميل يُحسب دائماً في الاستعلام:
> ```sql
> SELECT SUM(Debit) - SUM(Credit) AS Balance
> FROM CustomerLedger WHERE CustomerId = X
> ```
> هذا يضمن دقة الرصيد حتى لو أُدرجت سجلات بتواريخ سابقة.

### 8.2 جدول `InterBranchSettlement` (تسوية بين الفروع)
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `Id` | int PK | |
| `FromBranchId` | int FK | الفرع الذي استلم سداداً لصالح فرع آخر |
| `ToBranchId` | int FK | الفرع الدائن الأصلي |
| `Amount` | decimal | |
| `SettlementDate` | DateTime | |
| `Status` | enum `Pending/Settled` | |
| `SettledByUserId` | int FK Nullable | من أجرى التسوية |

---

## القسم 9: إغلاق اليومية (EOD)

### 9.1 جدول `DailyClosing` (مُصحَّح)
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `Id` | int PK | |
| `BranchId` | int FK | |
| `ClosingDate` | Date | |
| `Status` | enum `Draft/PendingApproval/Approved` | |
| `OpeningCash` | decimal | ✅ مُضاف — رصيد الصندوق افتتاح اليوم |
| `TotalCashSales` | decimal | |
| `TotalCreditSales` | decimal | |
| `TotalCardSales` | decimal | |
| `TotalCollections` | decimal | ديون محصلة نقداً |
| `TotalCashReturns` | decimal | ✅ مُضاف — مرتجعات نقدية لعملاء |
| `TotalExpenses` | decimal | مصروفات نقدية خرجت من الصندوق |
| `ExpectedCash` | decimal محسوب | ✅ المعادلة المصحَّحة (انظر 9.2) |
| `ActualCash` | decimal | الكاش الفعلي في الصندوق (يُدخله الكاشير) |
| `CashVariance` | decimal محسوب | `ActualCash - ExpectedCash` |
| `SubmittedByUserId` | int FK | |
| `ApprovedByUserId` | int FK Nullable | |
| `ApprovedAt` | DateTime Nullable | |

### 9.2 المعادلة المصحَّحة لتوقع الكاش
```
ExpectedCash = OpeningCash
             + TotalCashSales
             + TotalCollections
             - TotalCashReturns
             - TotalExpenses
```
> **✅ تصحيح الخطأ 2:** أُضيف `OpeningCash` و`TotalCashReturns` للمعادلة.

---

## القسم 10: بطاقة الصنف (مُصحَّحة)

> **✅ تصحيح الخطأ 10:** البطاقة تُعرض **لمخزن واحد فقط** مع رصيد تراكمي خاص به. لا تخلط بين أرصدة مخازن مختلفة.

**مثال — بطاقة باندول في مخزن الفرع أ (34233):**

| التاريخ | نوع الحركة | السند | وارد | صادر | الرصيد |
|:---|:---|:---|:---|:---|:---|
| 2026-05-03 | استلام تحويل | سند #12 | 50 | — | 50 |
| 2026-05-04 | مبيعات | فاتورة #4001 | — | 46 | 4 |
| 2026-05-04 | طلب تحويل تلقائي | سند #13 Auto | — | — | 4 (بانتظار) |
| 2026-05-06 | استلام تحويل | سند #13 | 50 | — | 54 |
| 2026-05-10 | تالف (انتهاء صلاحية) | إذن إتلاف #7 | — | 2 | 52 |
| 2026-05-30 | جرد (فائض) | أمر جرد #J01 | 1 | — | 53 |

---

## القسم 11: الصلاحيات والأدوار

| الدور | الصلاحيات |
|:---|:---|
| `SuperAdmin` | كل شيء في جميع الفروع |
| `MainBranchManager` | إدارة الفرع الرئيسي + اعتماد يوميات الفروع + التوزيع المركزي |
| `BranchManager` | إدارة كاملة لفرعه فقط |
| `Pharmacist` | البيع + المرتجعات + مشاهدة مخزون فرعه |
| `StoreKeeper` | استلام + صرف تحويلات + تسجيل تالف + جرد |
| `Cashier` | فتح/إغلاق اليومية + استقبال سداد |
| `Accountant` | تقارير مالية + رواتب + حسابات العملاء |

> **توضيح:** `SuperAdmin` و`MainBranchManager` دوران مختلفان. SuperAdmin هو مالك النظام التقني (يُنشئ فروعاً، يُدير مستخدمين). MainBranchManager هو المدير التشغيلي للفرع الرئيسي.

---

## القسم 12: ✅ الوحدات المفقودة (مُضافة)

### 12.1 مرتجعات العملاء (`SalesReturn`)
| الحقل | الوصف |
|:---|:---|
| `Id` | |
| `OriginalInvoiceId` | الفاتورة الأصلية |
| `BranchId` | الفرع الذي استلم المرتجع |
| `ReturnDate` | |
| `ReturnReason` | enum `CustomerRequest/Damaged/WrongItem` |
| `Status` | enum `PendingApproval/Approved/Rejected` |

**بنود المرتجع (`SalesReturnItem`):** MedicineId, BatchNumber, Quantity, UnitPrice, ReturnToWarehouse (هل يعود للمخزن العادي أم مخزن التالف؟)

**عند الاعتماد:**
- إذا الدواء سليم → يُعاد لمخزن الفرع العادي
- إذا الدواء تالف/منتهي → يُنقل لمخزن التالف
- يُسجَّل قيد في `CustomerLedger` (Credit)

### 12.2 مرتجعات الموردين (`SupplierReturn`)
| الحقل | الوصف |
|:---|:---|
| `Id` | |
| `SupplierId` | |
| `SourceWarehouseId` | المخزن الذي تخرج منه |
| `ReturnDate` | |
| `Reason` | enum `Expired/Damaged/QualityIssue` |
| `Status` | enum `PendingApproval/Approved/Shipped` |

**بنود المرتجع للمورد (`SupplierReturnItem`):** MedicineId, BatchNumber, Quantity, PurchasePrice

**عند الاعتماد:** تُخصم الكمية من `InventoryStock` + قيد محاسبي: "مرتجع مورد"

### 12.3 جدول الإشعارات (`Notification`) ✅
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `Id` | int PK | |
| `UserId` | int FK | المستلِم |
| `BranchId` | int FK Nullable | إذا كانت الإشعار خاصاً بفرع |
| `Type` | enum | `LowStock/Expiry/TransferRequest/DamageApproval/EODPending/CashVariance` |
| `Title` | string | |
| `Body` | string | |
| `ReferenceId` | int Nullable | معرف السجل المرتبط |
| `ReferenceType` | string | مثال: "StockTransfer", "DamagedGoodsRecord" |
| `IsRead` | bool | |
| `CreatedAt` | DateTime | |
