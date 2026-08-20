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

var builder = WebApplication.CreateBuilder(args);

// -------------------- Database --------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
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
builder.Services.AddScoped<IInvoiceSequenceRepository, InvoiceSequenceRepository>();

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
builder.Services.AddScoped<IPharmacySettingsService, PharmacySettingsService>();
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
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        var context = services.GetRequiredService<ApplicationDbContext>();
        
        logger.LogInformation("جاري التحقق من وجود الجداول وتحديث قاعدة البيانات...");
        await Microsoft.EntityFrameworkCore.RelationalDatabaseFacadeExtensions.MigrateAsync(context.Database);
        logger.LogInformation("تم تحديث هيكل قاعدة البيانات بنجاح.");

        logger.LogInformation("جاري بذر البيانات الأولية للصلاحيات والأدوار...");
        await SmartPharmacySystem.Infrastructure.Data.Seeders.PermissionSeeder.SeedAsync(context);
        await SmartPharmacySystem.Infrastructure.Data.Seeders.RoleSeeder.SeedAsync(context);
        logger.LogInformation("تم بذر البيانات الأولية بنجاح.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "حدث خطأ أثناء تهجير قاعدة البيانات.");
    }
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

// Redirect root to swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseStaticFiles(); // Serve images from wwwroot/images/medicines/
// app.UseHttpsRedirection(); // Disabled for mobile testing on local network
app.UseCors("AllowAll");
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAuthentication();  // مهم جداً قبل Authorization
app.UseAuthorization();

app.MapControllers();

app.Run();
