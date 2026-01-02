# تحديثات نظام المستخدمين - Include للأدوار

## الملخص
تم تحديث نظام المستخدمين لجلب بيانات الدور (Role) تلقائياً من جدول الأدوار عند جلب المستخدمين باستخدام `Include` في Entity Framework.

## التغييرات التي تمت

### 1. UserRepository.cs
**المسار:** `SmartPharmacySystem.Infrastructure/Repositories/UserRepository.cs`

تم إضافة `.Include(u => u.Role)` في جميع عمليات جلب المستخدمين:

- ✅ **GetByIdAsync**: تم تحديثه من `FindAsync` إلى `FirstOrDefaultAsync` مع `Include(u => u.Role)`
- ✅ **GetAllAsync**: تم إضافة `Include(u => u.Role)` قبل `ToListAsync()`
- ✅ **GetByUsernameAsync**: تم إضافة `Include(u => u.Role)` في الاستعلام
- ✅ **GetPagedAsync**: تم إضافة `Include(u => u.Role)` في بداية الـ query

```csharp
// مثال: GetByIdAsync
public async Task<User> GetByIdAsync(int id)
{
    return await _context.Users
        .Include(u => u.Role)
        .FirstOrDefaultAsync(u => u.Id == id);
}

// مثال: GetPagedAsync
var query = _context.Users
    .Include(u => u.Role)
    .AsQueryable();
```

### 2. UserDto.cs
**المسار:** `SmartPharmacySystem.Application/DTOs/User/UserDto.cs`

تم إضافة خصائص جديدة للدور:

```csharp
/// <summary>
/// معرف الدور - Role ID
/// </summary>
public int RoleId { get; set; }

/// <summary>
/// اسم الدور - Role name
/// </summary>
public string Role { get; set; } = string.Empty;

/// <summary>
/// وصف الدور - Role description
/// </summary>
public string? RoleDescription { get; set; }
```

### 3. UpdateUserDto.cs
**المسار:** `SmartPharmacySystem.Application/DTOs/User/UpdateUserDto.cs`

تم تحديث خاصية الدور من `string Role` إلى `int RoleId`:

```csharp
/// <summary>
/// معرف الدور - Role ID
/// </summary>
public int RoleId { get; set; }
```

### 4. MappingProfile.cs
**المسار:** `SmartPharmacySystem.Application/Mapping/MappingProfile.cs`

تم تحديث الـ AutoMapper mapping للمستخدمين:

```csharp
CreateMap<User, UserDto>()
    .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId))
    .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : string.Empty))
    .ForMember(dest => dest.RoleDescription, opt => opt.MapFrom(src => src.Role != null ? src.Role.Description : null))
    .ReverseMap();
```

## الفوائد

1. ✅ **جلب تلقائي للبيانات**: عند جلب أي مستخدم، سيتم جلب بيانات الدور تلقائياً
2. ✅ **تجنب N+1 Problem**: لن تحتاج لاستعلامات إضافية لجلب الدور
3. ✅ **معلومات كاملة**: يتم إرجاع RoleId و Role Name و Role Description
4. ✅ **أداء محسّن**: استعلام واحد بدلاً من استعلامات متعددة

## كيفية الاستخدام

### مثال 1: جلب مستخدم بواسطة ID
```csharp
GET /api/Users/1

Response:
{
    "id": 1,
    "username": "admin",
    "fullName": "مدير النظام",
    "roleId": 1,
    "role": "Admin",
    "roleDescription": "مدير النظام الكامل",
    "email": "admin@pharmacy.com",
    ...
}
```

### مثال 2: جلب جميع المستخدمين
```csharp
GET /api/Users

Response:
{
    "items": [
        {
            "id": 1,
            "username": "admin",
            "roleId": 1,
            "role": "Admin",
            "roleDescription": "مدير النظام الكامل",
            ...
        },
        {
            "id": 2,
            "username": "pharmacist1",
            "roleId": 2,
            "role": "Pharmacist",
            "roleDescription": "صيدلي",
            ...
        }
    ]
}
```

### مثال 3: البحث مع الفلترة
```csharp
GET /api/Users?search=admin&role=Admin&page=1&pageSize=10

Response:
{
    "items": [...],
    "totalCount": 1,
    "page": 1,
    "pageSize": 10,
    "totalPages": 1
}
```

## ملاحظات مهمة

1. **لا حاجة لاستعلامات إضافية**: الآن عند جلب المستخدمين، بيانات الدور تأتي تلقائياً
2. **التوافق مع الكود الحالي**: جميع الـ Controllers والـ Services الحالية ستعمل بدون تغيير
3. **الأداء**: استخدام `Include` أفضل من Lazy Loading في هذه الحالة

## الاختبار

يمكنك اختبار التحديثات عن طريق:

1. جلب مستخدم بواسطة ID: `GET /api/Users/{id}`
2. جلب جميع المستخدمين: `GET /api/Users`
3. البحث عن مستخدمين: `GET /api/Users?search=...`
4. الفلترة بالدور: `GET /api/Users?role=Admin`

في جميع الحالات، ستحصل على بيانات الدور كاملة (RoleId, Role Name, Role Description).
