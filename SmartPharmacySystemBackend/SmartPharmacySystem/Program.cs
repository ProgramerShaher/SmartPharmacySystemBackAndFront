using SmartPharmacySystem.Application.IServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Interfaces.Data;
using SmartPharmacySystem.Application.Mapping;
using SmartPharmacySystem.Application.Services;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure;
using SmartPharmacySystem.Infrastructure.Data;
using SmartPharmacySystem.Infrastructure.Repositories;
using SmartPharmacySystem.Infrastructure.Workers;
using SmartPharmacySystem.Infrastructure.Hubs;
using SmartPharmacySystem.Middleware;
using SmartPharmacySystem.Infrastructure.Services;

// تحديد مسار النظام ليكون نفس مسار ملف الـ exe بدلاً من مسار الويندوز الافتراضي
var options = new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
};

var builder = WebApplication.CreateBuilder(options);
builder.Host.UseWindowsService(); // السطر الذي أضفناه سابقاً

// -------------------- Database (Auto-Discovery) --------------------
// يكتشف SQL Server تلقائياً على أي جهاز بدون أي إعداد يدوي
var connectionString = DetectSqlServerConnection(
    builder.Configuration.GetConnectionString("DefaultConnection") ?? "",
    builder.Environment
);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString,
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
        }));
builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

// -------------------- JWT Authentication --------------------
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey not configured");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero // Remove delay of token expiration
    };
});

// -------------------- AutoMapper --------------------
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfile)));

// -------------------- Caching --------------------
builder.Services.AddMemoryCache();



// -------------------- Dependency Injection --------------------
// Repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IMedicineRepository, MedicineRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IPurchaseInvoiceRepository, PurchaseInvoiceRepository>();
builder.Services.AddScoped<IPurchaseInvoiceDetailRepository, PurchaseInvoiceDetailRepository>();
builder.Services.AddScoped<IPurchaseReturnRepository, PurchaseReturnRepository>();
builder.Services.AddScoped<IPurchaseReturnDetailRepository, PurchaseReturnDetailRepository>();
builder.Services.AddScoped<ISaleInvoiceRepository, SaleInvoiceRepository>();
builder.Services.AddScoped<ISaleInvoiceDetailRepository, SaleInvoiceDetailRepository>();
builder.Services.AddScoped<ISalesReturnRepository, SalesReturnRepository>();
builder.Services.AddScoped<ISalesReturnDetailRepository, SalesReturnDetailRepository>();
builder.Services.AddScoped<IInventoryMovementRepository, InventoryMovementRepository>();
builder.Services.AddScoped<IMedicineBatchRepository, MedicineBatchRepository>();
builder.Services.AddScoped<IAlertRepository, AlertRepository>();
builder.Services.AddScoped<IFinancialRepository, FinancialRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ISupplierPaymentRepository, SupplierPaymentRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerReceiptRepository, CustomerReceiptRepository>();
builder.Services.AddScoped<IPriceOverrideRepository, PriceOverrideRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IJournalEntryRepository, JournalEntryRepository>();
builder.Services.AddScoped<IChequeRepository, ChequeRepository>();
builder.Services.AddScoped<IBranchRepository, BranchRepository>();
builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IAttendanceRepository, AttendanceRepository>();
builder.Services.AddScoped<IMonthlySalaryRepository, MonthlySalaryRepository>();
builder.Services.AddScoped<IEmployeeLoanRepository, EmployeeLoanRepository>();
builder.Services.AddScoped<ICustomerLedgerRepository, CustomerLedgerRepository>();
builder.Services.AddScoped<IInterBranchSettlementRepository, InterBranchSettlementRepository>();
builder.Services.AddScoped<IDailyClosingRepository, DailyClosingRepository>();
builder.Services.AddScoped<IExpenseCategoryRepository, ExpenseCategoryRepository>();
builder.Services.AddScoped<IInventoryStockRepository, InventoryStockRepository>();
builder.Services.AddScoped<IStockCountRepository, StockCountRepository>();
builder.Services.AddScoped<IDamagedGoodsRepository, DamagedGoodsRepository>();
builder.Services.AddScoped<IStockTransferRepository, StockTransferRepository>();
builder.Services.AddScoped<IStockTransferItemRepository, StockTransferItemRepository>();
builder.Services.AddScoped<IMedicineWarehouseConfigRepository, MedicineWarehouseConfigRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<IPharmacySettingsRepository, PharmacySettingsRepository>();
builder.Services.AddScoped<IBusinessProfileRepository, BusinessProfileRepository>();
builder.Services.AddScoped<IInvoiceSequenceRepository, InvoiceSequenceRepository>();
builder.Services.AddScoped<IBackupConfigRepository, BackupConfigRepository>();
builder.Services.AddScoped<IBackupHistoryRepository, BackupHistoryRepository>();


// Services
builder.Services.AddScoped<IMedicineService, MedicineService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ICurrentUserService, SmartPharmacySystem.Services.CurrentUserService>();

// Authorization Infrastructure
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationPolicyProvider, SmartPharmacySystem.Authorization.PermissionPolicyProvider>();
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler, SmartPharmacySystem.Authorization.PermissionAuthorizationHandler>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IPurchaseInvoiceService, PurchaseInvoiceService>();
builder.Services.AddScoped<IPurchaseReturnService, PurchaseReturnService>();
builder.Services.AddScoped<ISaleInvoiceService, SaleInvoiceService>();
builder.Services.AddScoped<ISaleInvoiceDetailService, SaleInvoiceDetailService>();
builder.Services.AddScoped<ISalesReturnService, SalesReturnService>();
builder.Services.AddScoped<ISalesReturnDetailService, SalesReturnDetailService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IStockMovementService, StockMovementService>();
builder.Services.AddScoped<IMedicineBatchService, MedicineBatchService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IFinancialService, FinancialService>();
builder.Services.AddScoped<IInvoiceNumberGenerator, InvoiceNumberGenerator>();
builder.Services.AddScoped<IBarcodeResolverService, BarcodeResolverService>();
builder.Services.AddScoped<IBarcodeService, BarcodeService>();
builder.Services.AddScoped<ISupplierPaymentService, SupplierPaymentService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<ICustomerReceiptService, CustomerReceiptService>();
builder.Services.AddScoped<IReportService, ReportService>(); // Central Reporting Engine
builder.Services.AddScoped<IMasterDashboardService, MasterDashboardService>(); // Master Dashboard - Single Source of Truth
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IJournalEntryService, JournalEntryService>();
builder.Services.AddScoped<IChequeService, ChequeService>();

// Backup System Services
builder.Services.AddScoped<IEncryptionService, EncryptionService>();
builder.Services.AddScoped<IKeyManagementService, KeyManagementService>();
builder.Services.AddScoped<IGoogleDriveService, GoogleDriveService>();
builder.Services.AddScoped<IBackupStateService, BackupStateService>();
builder.Services.AddScoped<IBackupRetentionService, BackupRetentionService>();
builder.Services.AddScoped<IBackupService, BackupService>();
builder.Services.AddScoped<IPharmacySettingsService, PharmacySettingsService>();
builder.Services.AddScoped<IBusinessProfileService, BusinessProfileService>();
builder.Services.AddHttpClient<IWhatsAppNotificationService, WhatsAppNotificationService>();

// -------------------- HR & Payroll Services --------------------
builder.Services.AddScoped<IBranchService, BranchService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IShiftService, ShiftService>();
builder.Services.AddScoped<IMonthlySalaryService, MonthlySalaryService>();
builder.Services.AddScoped<IEmployeeLoanService, EmployeeLoanService>();

// -------------------- Financial & Settlement Services --------------------
builder.Services.AddScoped<ICustomerLedgerService, CustomerLedgerService>();
builder.Services.AddScoped<IInterBranchSettlementService, InterBranchSettlementService>();
builder.Services.AddScoped<IDailyClosingService, DailyClosingService>();
builder.Services.AddScoped<IExpenseCategoryService, ExpenseCategoryService>();
builder.Services.AddScoped<IClosingValidationService, ClosingValidationService>();

// -------------------- Inventory & Warehouse Services --------------------
builder.Services.AddScoped<IInventoryStockService, InventoryStockService>();
builder.Services.AddScoped<IStockCountService, StockCountService>();
builder.Services.AddScoped<IAutomatedAuditService, AutomatedAuditService>();
builder.Services.AddScoped<IDamagedGoodsService, DamagedGoodsService>();
builder.Services.AddScoped<IStockTransferService, StockTransferService>();
builder.Services.AddScoped<IMedicineWarehouseConfigService, MedicineWarehouseConfigService>();

// -------------------- Additional Services --------------------
builder.Services.AddScoped<IPriceOverrideService, PriceOverrideService>();
builder.Services.AddScoped<IInvoiceSequenceService, InvoiceSequenceService>();
builder.Services.AddScoped<IPricelistService, PricelistService>();
builder.Services.AddScoped<IPricelistRepository, PricelistRepository>();

// -------------------- Mobile App Services --------------------
builder.Services.AddScoped<IOnlineOrderService, OnlineOrderService>();
builder.Services.AddScoped<IMobileAuthService, MobileAuthService>();

// -------------------- Licensing --------------------
// Singleton: hardware fingerprinting is stateless; no benefit from per-request instantiation.
builder.Services.AddSingleton<ILicenseService, LicenseService>();


// -------------------- HttpContextAccessor --------------------
builder.Services.AddHttpContextAccessor();

// -------------------- Controllers --------------------
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddSignalR();
builder.Services.AddHostedService<ExpiryCheckWorker>();
builder.Services.AddHostedService<StockCountWorker>();
builder.Services.AddHostedService<BackupStartupTask>();
builder.Services.AddHostedService<BackupSchedulerWorker>();

// -------------------- CORS --------------------
builder.Services.AddCors(options =>
{
    // Global CORS Policy for Development
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// -------------------- Swagger with JWT --------------------
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Clinic API",
        Version = "v1",
        Description = "API for Managing Clinics"
    });

    // إضافة زر Authorize للـ JWT في Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "ضع التوكن هنا بدون كلمة Bearer. مثال: 12345abcde",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });

});

// -------------------- Build APP --------------------
var app = builder.Build();

// -------------------- Database Initialization & Seeding --------------------
try
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<ApplicationDbContext>();

    const int maxRetries = 3;
    const int delaySeconds = 2;

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            logger.LogInformation("🔄 محاولة الاتصال بقاعدة البيانات... ({Attempt}/{Max})", attempt, maxRetries);

            await context.Database.CanConnectAsync();
            await context.Database.MigrateAsync();
            await SmartPharmacySystem.Infrastructure.Data.Seeders.PermissionSeeder.SeedAsync(context);
            await SmartPharmacySystem.Infrastructure.Data.Seeders.RoleSeeder.SeedAsync(context);
            logger.LogInformation("✅ تم الاتصال بـ SQL Server وبذر البيانات بنجاح.");
            break;
        }
        catch (Exception ex)
        {
            logger.LogError("❌ فشلت محاولة قاعدة البيانات {Attempt}/{Max}: {Message}", attempt, maxRetries, ex.Message);

            if (attempt == maxRetries)
            {
                logger.LogCritical(ex, "🚫 تعذّر الاتصال بـ SQL Server أو تطبيق Migrations بعد {Max} محاولات.", maxRetries);
            }
            else
            {
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds));
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"⚠️ تنبيه قاعدة البيانات: {ex.Message}");
}

// -------------------- Middlewares --------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DocumentTitle = "Clinic API Documentation";
        c.RoutePrefix = "swagger";
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Clinic API v1");

        // تحسينات واجهة
        c.InjectStylesheet("/swagger/custom-swagger.css");
        c.DisplayRequestDuration();
        c.DefaultModelsExpandDepth(-1);
        c.EnableFilter();
    });

    // إضافة ReDoc
    app.UseReDoc(c =>
    {
        c.SpecUrl("/swagger/v1/swagger.json");
        c.DocumentTitle = "Clinic API – ReDoc";
        c.RoutePrefix = "redoc";
    });
}

app.MapHub<NotificationHub>("/notificationHub");

// Redirect root to swagger only in development
if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Redirect("/swagger"));
}

app.UseDefaultFiles(); // ليبحث عن ملف index.html الخاص بـ Angular عند فتح الصفحة الرئيسية
app.UseStaticFiles(); // Serve static files & Angular assets from wwwroot
// app.UseHttpsRedirection(); // Disabled for mobile testing on local network
app.UseCors("AllowAll");
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAuthentication();  // مهم جداً قبل Authorization
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("index.html"); // ليقوم Angular بإدارة التنقل بين الصفحات بدون أخطاء
app.Run();

// ═════════════════════════════════════════════════════════════════════════════
// Auto-Discovery: يكتشف SQL Server تلقائياً على أي جهاز
// ═════════════════════════════════════════════════════════════════════════════
static string DetectSqlServerConnection(string configuredConnection, IWebHostEnvironment env)
{
    const string dbName = "PharmacyDB";

    // 1. جرّب الـ Connection String المحفوظ في appsettings.json أولاً
    if (!string.IsNullOrWhiteSpace(configuredConnection) && TestConnection(configuredConnection))
        return configuredConnection;

    // 2. ابحث عن SQL Server instances نشطة عبر Windows Services
    var candidates = GetSqlServerInstances();

    foreach (var server in candidates)
    {
        // 1. فحص الاتصال بالسيرفر نفسه (باستخدام قاعدة master الافتراضية)
        var testCs = $"Server={server};Database=master;Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=3";
        
        if (TestConnection(testCs))
        {
            // 2. إذا نجح الاتصال بالسيرفر، نبني نص الاتصال النهائي الخاص بتطبيقنا
            var finalCs = $"Server={server};Database={dbName};Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";
            
            // احفظ الـ Connection String الناجح في appsettings.json للمرات القادمة
            SaveConnectionString(finalCs, env);
            return finalCs;
        }
    }

    // 3. إذا لم يُعثر على أي اتصال → ارجع الـ appsettings بحيث رسالة الخطأ تكون واضحة
    return configuredConnection;
}

static bool TestConnection(string connectionString)
{
    try
    {
        using var conn = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
        conn.Open();
        return true;
    }
    catch { return false; }
}

static List<string> GetSqlServerInstances()
{
    var servers = new List<string>();

    try
    {
        // اقرأ الـ Services لاكتشاف جميع instances
        var services = System.ServiceProcess.ServiceController.GetServices();
        foreach (var svc in services)
        {
            if (!svc.DisplayName.StartsWith("SQL Server (", StringComparison.OrdinalIgnoreCase))
                continue;

            // استخرج اسم الـ Instance من "SQL Server (INSTANCENAME)"
            var start    = svc.DisplayName.IndexOf('(') + 1;
            var end      = svc.DisplayName.IndexOf(')');
            var instance = svc.DisplayName[start..end];

            if (instance.Equals("MSSQLSERVER", StringComparison.OrdinalIgnoreCase))
                servers.Add(".");           // Default instance
            else
                servers.Add($".\\{instance}");  // Named instance
        }
    }
    catch { /* ignore — نكمل بالـ fallbacks */ }

    // أضف خيارات احتياطية شائعة
    servers.AddRange(new[] { ".", ".\\SQLEXPRESS", ".\\MSSQLSERVER", "localhost", "localhost\\SQLEXPRESS" });

    return servers.Distinct().ToList();
}

static void SaveConnectionString(string connectionString, IWebHostEnvironment env)
{
    try
    {
        var appSettingsPath = Path.Combine(env.ContentRootPath, "appsettings.json");
        if (!File.Exists(appSettingsPath)) return;

        var json = File.ReadAllText(appSettingsPath);
        // استبدل قيمة DefaultConnection
        var escaped = connectionString.Replace("\\", "\\\\");
        json = System.Text.RegularExpressions.Regex.Replace(
            json,
            @"""DefaultConnection""\s*:\s*""[^""]*""",
            $@"""DefaultConnection"": ""{escaped}"""
        );
        File.WriteAllText(appSettingsPath, json);
    }
    catch { /* لا نوقف التطبيق إذا فشل الحفظ */ }
}
