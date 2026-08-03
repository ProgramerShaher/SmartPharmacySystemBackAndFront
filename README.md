<div align="center">
  <br />
  <img src="https://img.icons8.com/fluent/100/000000/caduceus.png" alt="Smart Pharmacy Logo" width="100" />
  <h1>⚕️ Smart Pharmacy System (نظام الصيدلية الذكي)</h1>
  <p>
    <strong>نظام تخطيط موارد المؤسسات (ERP) الشامل والحديث المصمم خصيصاً لإدارة الصيدليات وسلاسل الصيدليات.</strong>
  </p>
  <br />

  [![Backend](https://img.shields.io/badge/Backend-.NET_8-512BD4?style=for-the-badge&logo=.net&logoColor=white)](https://dotnet.microsoft.com/)
  [![Frontend](https://img.shields.io/badge/Frontend-Angular_17-DD0031?style=for-the-badge&logo=angular&logoColor=white)](https://angular.io/)
  [![Mobile](https://img.shields.io/badge/Mobile-Expo_React_Native-000020?style=for-the-badge&logo=expo&logoColor=white)](https://expo.dev/)
  [![Database](https://img.shields.io/badge/Database-SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)](https://www.microsoft.com/sql-server)
  
  <br />
</div>

## 📖 نظرة عامة شاملة (Comprehensive Overview)

**نظام الصيدلية الذكي** ليس مجرد برنامج مبيعات، بل هو منظومة متكاملة (Ecosystem) تدير كل صغيرة وكبيرة داخل المؤسسة الصيدلانية. يجمع النظام بين القوة والسرعة لضمان عدم حدوث أي أخطاء في صرف الأدوية، تتبع تواريخ الصلاحية، إدارة شؤون الموظفين، وضبط الحسابات المالية بدقة متناهية.

---

## 🎯 وحدات النظام الرئيسية (System Modules)

النظام مقسم إلى عدة وحدات (Modules) مترابطة تعمل معاً بشكل سلس:

### 📦 1. نظام إدارة المخزون (Inventory Management Module)
يُعد هذا النظام القلب النابض للصيدلية، ويدير بدقة حركة الأدوية من لحظة دخولها حتى خروجها.
* **إدارة الفروع والمخازن (Branches & Warehouses):** دعم أكثر من فرع وأكثر من مخزن (مخزن رئيسي، مخازن فرعية، ثلاجات).
* **إدارة تصنيفات الأدوية (Categories):** شجرة تصنيفات لانهائية لتنظيم الأدوية (مثال: أدوية مزمنة > سكر > أنسولين).
* **ملف الدواء الشامل (Medicine Profile):** يحتوي على الاسم العلمي، التجاري، الباركود، الشركة المصنعة، المادة الفعالة، بدائل الدواء، أسعار الشراء والبيع، والحد الأدنى للطلب.
* **نظام التشغيلات وتواريخ الانتهاء (Batches & Expiry Dates):** كل دواء يدخل المخزن يتم تسجيله برقم تشغيلة (Batch Number) وتاريخ صلاحية مستقل، مع نظام تنبيهات مبكر للأدوية التي قاربت على الانتهاء.
* **حركات المخزون (Stock Movements):** توثيق كل حركة (وارد، منصرف، تالف، مرتجع) مع ربط الحركة بالمستخدم والوقت.
* **الجرد المخزني (Stock Counting):** نظام متطور لعمليات الجرد (يومي، شهري، سنوي). يقوم النظام بجلب الأرصدة النظامية ومقارنتها بالكميات الفعلية التي يدخلها الصيدلي، ويقوم بحساب الفوارق آلياً لإنشاء تسويات جردية.

### 🛒 2. نظام المبيعات ونقاط البيع (Sales & POS Module)
مصمم للسرعة الفائقة لخدمة العملاء دون تأخير.
* **شاشة الكاشير (POS):** واجهة سريعة تدعم أجهزة قراءة الباركود للبحث السريع عن الأدوية.
* **بيع الأدوية البديلة (Substitutes):** عند عدم توفر دواء، يقترح النظام فوراً البدائل المتاحة بنفس المادة الفعالة.
* **الخصومات والضرائب (Discounts & Taxes):** تطبيق الخصومات المسموحة وحساب ضريبة القيمة المضافة (VAT) آلياً.
* **تعدد طرق الدفع:** نقدي، شبكة (بطاقة ائتمانية)، آجل، أو محافظ إلكترونية.
* **المرتجعات:** معالجة المرتجعات برقم الفاتورة الأصلي وبكل سهولة.

### 📥 3. المشتريات والموردين (Purchases & Suppliers)
* **إدارة الموردين:** ملف متكامل لكل مورد وحساباته ومديونياته.
* **أوامر الشراء (Purchase Orders):** إنشاء أوامر شراء آلياً للأدوية التي وصلت للحد الأدنى (Reorder Level).
* **استلام البضاعة (Goods Receipt):** استلام الكميات وتوليد أرقام التشغيلات (Batches) وتحديث أسعار التكلفة تلقائياً.

### 👥 4. الموارد البشرية وشؤون الموظفين (HR & Payroll Module)
نظام متكامل لإدارة فريق العمل داخل سلسلة الصيدليات.
* **الهيكل التنظيمي:** إدارة الأقسام والفروع وربط الموظفين بها.
* **ملف الموظف:** البيانات الشخصية، المسمى الوظيفي، وتاريخ التعيين.
* **الحضور والانصراف:** تسجيل حركات الحضور وتأخيرات الموظفين.
* **السلف والخصومات (Loans & Deductions):** طلبات سلف، الموافقة عليها، وجدولتها للخصم الآلي من الراتب.
* **الرواتب (Payroll):** إصدار مسيرات الرواتب بنقرة واحدة (الراتب الأساسي + البدلات - الخصومات - السلف).

### 🔐 5. نظام الصلاحيات والأمان (Security & Roles)
* **أدوار مخصصة (RBAC):** (مدير نظام، صيدلي، أمين مستودع، موظف موارد بشرية، كاشير).
* **حماية النوافذ (Guards):** لا يمكن لأي مستخدم الدخول لصفحة أو تنفيذ إجراء (حذف/تعديل) بدون الصلاحية المناسبة.

### 📊 6. لوحات التحكم والتقارير (Dashboards & Reports)
* شاشات تفاعلية تعرض: إجمالي المبيعات، الأدوية الأكثر مبيعاً، الأدوية منتهية الصلاحية، الرصيد المالي الحالي.
* تقارير قابلة للتصدير (PDF / Excel).

---

## 🛠️ البنية التحتية والتقنيات (Architecture & Technology Stack)

تم بناء النظام باستخدام أحدث التقنيات والمعماريات البرمجية لضمان الاستقرار (Stability) وقابلية التوسع (Scalability):

### ⚙️ الباك إند (Backend)
- **الإطار (Framework):** `.NET 8` (ASP.NET Core Web API).
- **المعمارية (Architecture):** `Clean Architecture` (مقسمة إلى Core, Application, Infrastructure, Web API) لضمان فصل الاهتمامات (Separation of Concerns).
- **التعامل مع البيانات (ORM):** `Entity Framework Core` مع `SQL Server`.
- **الأمان:** `JWT` (JSON Web Tokens) للمصادقة.
- **إضافات أخرى:** `AutoMapper` لربط البيانات، `FluentValidation` للتحقق من المدخلات.

### 💻 واجهة الويب (Frontend - Dashboard & POS)
- **الإطار (Framework):** `Angular 17` (باستخدام Standalone Components الحديثة).
- **مكتبة المكونات (UI Library):** `PrimeNG` لتقديم جداول، أزرار، وقوائم تفاعلية وسريعة.
- **التصميم (Styling):** `SCSS` مع نظام ألوان (Design Tokens) يمكن تغييره بسهولة لدعم الوضع الليلي (Dark Mode).
- **إدارة الحالة (State Management):** `RxJS` للتعامل مع البيانات اللحظية وإدارة الـ Observables.

### 📱 تطبيق الموبايل (Mobile App)
- **التقنية:** `React Native` مع `Expo`.
- مخصص لتسهيل عمليات الجرد السريع عبر كاميرا الجوال وقراءة الباركود مباشرة، بالإضافة إلى متابعة الإدارة للتقارير الحية.

---

## 📁 هيكلية مجلدات المشروع (Project Directory Structure)

```text
d:\MyPharmacyProject\
├── SmartPharmacySystemBackend/       # الكود المصدري للواجهة الخلفية (.NET)
│   ├── SmartPharmacySystem.Core/     # الكيانات (Entities)، الواجهات (Interfaces)
│   ├── SmartPharmacySystem.Application/# منطق العمل (Services)، الـ DTOs
│   ├── SmartPharmacySystem.Infrastructure/# قواعد البيانات (DbContext)، المستودعات (Repositories)
│   └── SmartPharmacySystem/          # المتحكمات (Controllers)، إعدادات بدء التشغيل
├── SmartPharmasySystemsFrontend/     # الكود المصدري لواجهة الويب (Angular)
│   └── src/
│       ├── app/
│       │   ├── core/                 # النماذج (Models)، المعترضات (Interceptors)، الحراس (Guards)
│       │   ├── features/             # وحدات النظام (Inventory, HR, Dashboard, Users)
│       │   └── shared/               # مكونات UI المشتركة
│       └── assets/                   # الصور، الأيقونات، ملفات التنسيق العامة
├── SmartPharmacyMobileApp/           # الكود المصدري لتطبيق الجوال (Expo)
└── graphify-out/                     # مخرجات خريطة المعرفة (Knowledge Graph) للمشروع
```

---

## 🚀 كيفية تشغيل النظام محلياً (How to Run Locally)

### 1. تشغيل الباك إند (.NET Backend)
1. افتح موجه الأوامر (Terminal) وانتقل لمجلد المشروع:
   ```bash
   cd SmartPharmacySystemBackend/SmartPharmacySystem
   ```
2. تأكد من تعديل سلسلة الاتصال (Connection String) في ملف `appsettings.json` ليتوافق مع الـ SQL Server الخاص بك.
3. قم بتحديث قاعدة البيانات (تطبيق הMigrations):
   ```bash
   dotnet ef database update
   ```
4. شغل الخادم:
   ```bash
   dotnet run
   ```
   *واجهة Swagger ستكون متاحة على: `http://localhost:5000/swagger`*

### 2. تشغيل الفرونت إند (Angular Frontend)
1. في موجه أوامر جديد، انتقل لمجلد الواجهة:
   ```bash
   cd SmartPharmasySystemsFrontend
   ```
2. ثبّت الحزم المطلوبة:
   ```bash
   npm install
   ```
3. شغل خادم التطوير:
   ```bash
   ng serve -o
   ```
   *سيفتح النظام تلقائياً على الرابط: `http://localhost:4200`*

---
<div align="center">
  <i>تم تطوير هذا النظام بأعلى معايير جودة البرمجيات لتلبية احتياجات الإدارة الصيدلانية الحديثة.</i>
</div>
