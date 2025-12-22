using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Application.Mapping;
using SmartPharmacySystem.Application.Services;
using SmartPharmacySystem.Core.Interfaces;
using SmartPharmacySystem.Infrastructure;
using SmartPharmacySystem.Infrastructure.Data;
using SmartPharmacySystem.Infrastructure.Repositories;
using SmartPharmacySystem.Middleware;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Database --------------------
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// -------------------- Authentication --------------------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

// -------------------- AutoMapper --------------------
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(typeof(MappingProfile).Assembly));


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

// Services
builder.Services.AddScoped<IMedicineService, MedicineService>();
builder.Services.AddScoped<ISupplierService, SupplierService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IExpenseService, ExpenseService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IPurchaseInvoiceService, PurchaseInvoiceService>();
builder.Services.AddScoped<IPurchaseInvoiceDetailService, PurchaseInvoiceDetailService>();
builder.Services.AddScoped<IPurchaseReturnService, PurchaseReturnService>();
builder.Services.AddScoped<IPurchaseReturnDetailService, PurchaseReturnDetailService>();
builder.Services.AddScoped<ISaleInvoiceService, SaleInvoiceService>();
builder.Services.AddScoped<ISaleInvoiceDetailService, SaleInvoiceDetailService>();
builder.Services.AddScoped<ISalesReturnService, SalesReturnService>();
builder.Services.AddScoped<ISalesReturnDetailService, SalesReturnDetailService>();
builder.Services.AddScoped<IStockMovementService, StockMovementService>();
builder.Services.AddScoped<IMedicineBatchService, MedicineBatchService>();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddScoped<IFinancialService, FinancialService>();
builder.Services.AddScoped<IInvoiceNumberGenerator, InvoiceNumberGenerator>();

// -------------------- Controllers --------------------
builder.Services.AddControllers();

// -------------------- CORS --------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
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
app.UseHttpsRedirection();
app.UseCors("AllowAngular");
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAuthentication();  // مهم جداً قبل Authorization
app.UseAuthorization();

app.MapControllers();

app.Run();
