# الدليل المرجعي المُصحَّح - الجزء الأول (v4.0)
## Smart Pharmacy ERP — Sections 1 to 6
> ✅ مُصحَّح بناءً على مراجعة نقدية — جميع الأخطاء العشرة مُعالَجة

---

## القسم 1: الفروع

### جدول `Branch`
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `Id` | int PK | |
| `BranchCode` | string UNIQUE | كود الفرع الموحد في جميع التقارير |
| `Name` | string | |
| `Location` | string | |
| `BranchType` | enum `Main / Sub` | Main = يبيع + يوزع + يشتري مركزياً |
| `IsActive` | bool | |

> **تصحيح:** الفرع الرئيسي (Main) يبيع للجمهور ويملك صندوقاً نقدياً تماماً كالفروع الأخرى. الفرق الوحيد: **الصلاحيات** (توزيع، شراء مركزي، اعتماد يوميات). لا فرق في بنية قاعدة البيانات.

---

## القسم 2: المخازن

### 2.1 أنواع المخازن
```csharp
public enum WarehouseType
{
    Main    = 1,  // مخزن الفرع الرئيسي
    Branch  = 2,  // مخزن فرع عادي
    Damaged = 3   // مخزن التالف (لكل فرع مخزن تالف خاص به)
}
```

### 2.2 جدول `Warehouse`
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `Id` | int PK | |
| `BranchId` | int FK | الفرع المالك |
| `Type` | WarehouseType | |
| `Name` | string | |

> كل فرع يملك مخزنين على الأقل: واحد عادي (Main/Branch) + واحد للتالف (Damaged).

### 2.3 جدول `InventoryStock` (كميات التشغيلات)
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `Id` | int PK | |
| `WarehouseId` | int FK | |
| `MedicineId` | int FK | |
| `BatchNumber` | string | رقم التشغيلة |
| `ExpiryDate` | DateTime | تاريخ الانتهاء |
| `Quantity` | int | الكمية الفعلية لهذه التشغيلة |

> **✅ تصحيح الخطأ 1:** `ReorderLevel` و`ReorderQuantity` نُقلا إلى جدول منفصل (2.4). لا يجب أن يكونا في جدول التشغيلات.

### 2.4 ✅ جدول جديد: `MedicineWarehouseConfig` (إعدادات الدواء بالمخزن)
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `WarehouseId` | int FK (PK) | |
| `MedicineId` | int FK (PK) | |
| `ReorderLevel` | int | حد الأمان الخاص بهذا الدواء في هذا المخزن |
| `ReorderQuantity` | int | الكمية المطلوبة عند الطلب التلقائي |

> **PK مركب:** `(WarehouseId, MedicineId)` — إعداد واحد لكل دواء في كل مخزن، مستقل تماماً عن التشغيلات.

---

## القسم 3: الشراء المركزي والتوريد التلقائي

### 3.1 دورة الشراء المركزي
1. يستقبل الفرع الرئيسي البضاعة من المورد → تدخل مخزنه في `InventoryStock`.
2. الفروع تطلب من الفرع الرئيسي عبر سند تحويل — لا تشتري من الموردين مباشرة.

### 3.2 محرك التوريد التلقائي (Auto-Replenishment)
**✅ تصحيح الخطأ 6 — منع الطلبات المكررة:**
```
بعد كل عملية بيع في فرع معين لدواء معين:

1. احسب إجمالي الكمية لهذا الدواء في هذا المخزن:
   SUM(Quantity) من InventoryStock WHERE WarehouseId=X AND MedicineId=Y

2. جلب ReorderLevel من MedicineWarehouseConfig WHERE WarehouseId=X AND MedicineId=Y

3. إذا (إجمالي الكمية <= ReorderLevel):
   تحقق أولاً: هل يوجد StockTransfer بحالة (AutoRequested/Requested/Approved/Dispatched)
   لنفس (SourceWarehouse=الرئيسي, DestinationWarehouse=X, MedicineId=Y)؟
   
   - إذا نعم: لا تفعل شيئاً (الطلب موجود)
   - إذا لا:  أنشئ StockTransfer جديد بحالة AutoRequested + أرسل إشعاراً للمدير
```

---

## القسم 4: التحويلات بين المخازن

### 4.1 جدول `StockTransfer`
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `Id` | int PK | |
| `TransferCode` | string UNIQUE | رقم السند |
| `SourceWarehouseId` | int FK | |
| `DestinationWarehouseId` | int FK | |
| `Status` | TransferStatus | |
| `TransferType` | enum `Manual/AutoRequested/BranchRequest` | |
| `RequestedByUserId` | int FK | |
| `ApprovedByUserId` | int FK Nullable | ✅ مُضاف |
| `ApprovedAt` | DateTime Nullable | ✅ مُضاف |
| `DispatchedByUserId` | int FK Nullable | |
| `DispatchedAt` | DateTime Nullable | |
| `ReceivedByUserId` | int FK Nullable | |
| `ReceivedAt` | DateTime Nullable | |
| `Notes` | string | |

### 4.2 حالات سند التحويل (مُصحَّحة)
```csharp
public enum TransferStatus
{
    AutoRequested = 0,  // طلب تلقائي بانتظار مراجعة المدير
    Requested     = 1,  // طلب يدوي من الفرع
    Approved      = 2,  // اعتمده المدير (ApprovedByUserId مُسجَّل)
    Dispatched    = 3,  // خرجت البضاعة + خُصمت من المرسِل + هي الآن "في الطريق"
    Received      = 5,  // تأكيد الاستلام من الفرع (أُضيفت لمخزن الفرع)
    Cancelled     = 6
}
```

> **✅ تصحيح الخطأ 3:** حُذفت حالة `InTransit` المنفصلة. `Dispatched` تعني بالتعريف أن البضاعة خرجت وهي في الطريق. لا يوجد حدث يفصل بين "خرجت" و"في الطريق".

### 4.3 جدول `StockTransferItem`
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `Id` | int PK | |
| `StockTransferId` | int FK | |
| `MedicineId` | int FK | |
| `BatchNumber` | string | |
| `ExpiryDate` | DateTime | |
| `QuantityRequested` | int | |
| `QuantityDispatched` | int | الكمية التي خرجت فعلاً |
| `QuantityReceived` | int Nullable | الكمية المستلمة بعد العد |

### 4.4 قواعد الاستلام والتسليم
- **الصرف:** السند معتمد (Approved) + أمين المخزن يضغط "صرف" → يُخصم من المرسِل → حالة `Dispatched`
- **الاستلام:** موظف الفرع يُدخل الكمية الفعلية ويضغط "تأكيد" → تُضاف لمخزنه → حالة `Received`
- **العجز:** `QuantityReceived < QuantityDispatched` → يُسجَّل تلقائياً + إشعار للإدارة للتحقيق

---

## القسم 5: التالف وانتهاء الصلاحية

### 5.1 جدول `DamagedGoodsRecord`
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `Id` | int PK | |
| `DamageCode` | string | رقم إذن الإتلاف |
| `SourceWarehouseId` | int FK | المخزن الأصلي |
| `MedicineId` | int FK | |
| `BatchNumber` | string | |
| `ExpiryDate` | DateTime | |
| `Quantity` | int | |
| `DamageType` | enum `Expired/PhysicalDamage/ManufacturingDefect` | |
| `DamageValue` | decimal | بسعر الشراء |
| `DisposalMethod` | enum `Destroyed/ReturnedToSupplier/Donated` | |
| `Status` | enum `PendingApproval/Approved/Rejected` | |
| `ApprovedByUserId` | int FK Nullable | |
| `ApprovedAt` | DateTime Nullable | |
| `RecordedByUserId` | int FK | |
| `RecordedAt` | DateTime | |

### 5.2 آلية تسجيل التالف
```
1. الموظف يسجل التالف → Status = PendingApproval
2. المدير يراجع ويعتمد
3. عند الاعتماد فقط:
   - تُخصم الكمية من InventoryStock (المخزن الأصلي)
   - تُضاف لمخزن التالف الخاص بالفرع (WarehouseType=Damaged)
   - قيد محاسبي: "خسارة توالف"
```

### 5.3 محرك تنبيهات الصلاحية
| المرحلة | الإجراء |
|:---|:---|
| قبل 6 أشهر | 🟡 إشعار أصفر: "يُنصح بعمل عرض تخفيض" |
| قبل 3 أشهر | 🟠 إشعار برتقالي: "يُنصح بإرجاع المورد أو التخفيض" |
| قبل شهر | 🔴 إشعار أحمر + **اقتراح** لمدير لإنشاء إذن إتلاف — **لا قفل تلقائي** |

> **✅ تصحيح الخطأ 8:** القفل التلقائي للبيع والنقل التلقائي لمخزن التالف **حُذفا**. الدواء صالح للبيع قانونياً حتى تاريخ انتهائه. القرار يبقى بيد المدير.

---

## القسم 6: الجرد المخزني

### 6.1 أنواع الجرد
```csharp
public enum StockCountType { Daily=1, Monthly=2, BiAnnual=3, Annual=4 }
```

### 6.2 جدول `StockCountHeader`
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `Id` | int PK | |
| `CountCode` | string | رقم أمر الجرد |
| `WarehouseId` | int FK | مخزن واحد محدد |
| `CountType` | StockCountType | |
| `SnapshotAt` | DateTime | **وقت الجرد الفعلي** (تُقارن الكميات به لا بوقت الإدخال) |
| `StartedAt` | DateTime | |
| `FinishedAt` | DateTime Nullable | |
| `Status` | enum `Draft/InProgress/PendingApproval/Approved/Closed` | |
| `ApprovedByUserId` | int FK Nullable | |
| `Notes` | string | |

### 6.3 جدول `StockCountItem`
| الحقل | النوع | الوصف |
|:---|:---|:---|
| `StockCountHeaderId` | int FK (PK) | |
| `MedicineId` | int FK (PK) | |
| `BatchNumber` | string (PK) | |
| `ExpiryDate` | DateTime | |
| `SystemQuantity` | int | لقطة الكمية النظامية عند `SnapshotAt` |
| `PhysicalQuantity` | int Nullable | الكمية الفعلية المعدودة |
| `Variance` | int محسوب | `PhysicalQuantity - SystemQuantity` |
| `VarianceValue` | decimal محسوب | قيمة الفارق (بسعر الشراء) |
| `VarianceReason` | string Nullable | |

### 6.4 دورة الجرد
```
1. المدير يفتح أمر جرد → يحدد المخزن + نوع الجرد + SnapshotAt
2. النظام يأخذ لقطة من InventoryStock عند SnapshotAt ويملأ SystemQuantity
3. الموظفون يعدّون الكميات الفعلية ويُدخلونها (PhysicalQuantity)
4. النظام يحسب الفارق والقيمة المالية تلقائياً
5. يُرسل للمدير بحالة PendingApproval
6. عند الاعتماد: تُعدَّل أرصدة InventoryStock + قيد محاسبي (عجز/فائض جرد)
```

> **ملاحظة:** لا يُجمَّد النظام أثناء الجرد — يُستخدم `SnapshotAt` كمرجع زمني ثابت للمقارنة. أي حركات بعد هذا الوقت تُحسب للجرد القادم.
