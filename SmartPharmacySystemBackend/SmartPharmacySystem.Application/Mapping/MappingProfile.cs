using AutoMapper;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Application.DTOs.Customers;
using SmartPharmacySystem.Application.DTOs.Alerts;
using SmartPharmacySystem.Application.DTOs.Categories;
using SmartPharmacySystem.Application.DTOs.Medicine;
using SmartPharmacySystem.Application.DTOs.Suppliers;
using SmartPharmacySystem.Application.DTOs.User;
using SmartPharmacySystem.Application.DTOs.Expense;
using SmartPharmacySystem.Application.DTOs.MedicineBatch;
using SmartPharmacySystem.Application.DTOs.PurchaseInvoiceDetails;
using SmartPharmacySystem.Application.DTOs.SalesInvoices;
using SmartPharmacySystem.Application.DTOs.SalesInvoiceDetails;
using SmartPharmacySystem.Application.DTOs.PurchaseReturns;
using SmartPharmacySystem.Application.DTOs.PurchaseReturnDetails;
using SmartPharmacySystem.Application.DTOs.SalesReturns;
using SmartPharmacySystem.Application.DTOs.SalesReturnDetails;
using SmartPharmacySystem.Application.DTOs.CreatePurchaseInvoice;
using SmartPharmacySystem.Application.Helpers;
using SmartPharmacySystem.Application.DTOs.Financial;
using SmartPharmacySystem.Application.DTOs.Role;
using SmartPharmacySystem.Application.DTOs.StockMovement;
using SmartPharmacySystem.Application.DTOs.SupplierPayments;
using SmartPharmacySystem.Application.DTOs.Settings;
using SmartPharmacySystem.Application.DTOs.Branches;
using SmartPharmacySystem.Application.DTOs.Warehouses;
using SmartPharmacySystem.Application.DTOs.InventoryStocks;
using SmartPharmacySystem.Application.DTOs.MedicineWarehouseConfigs;
using SmartPharmacySystem.Application.DTOs.StockTransfers;
using SmartPharmacySystem.Application.DTOs.DamagedGoods;
using SmartPharmacySystem.Application.DTOs.MonthlySalaries;
using SmartPharmacySystem.Application.DTOs.ExpenseCategories;
using SmartPharmacySystem.Application.DTOs.PriceOverrides;
using SmartPharmacySystem.Application.DTOs.InvoiceSequences;
using SmartPharmacySystem.Application.DTOs.StockCounts;
using SmartPharmacySystem.Application.DTOs.Departments;
using SmartPharmacySystem.Application.DTOs.Employees;
using SmartPharmacySystem.Application.DTOs.Attendances;
using SmartPharmacySystem.Application.DTOs.EmployeeLoans;
using SmartPharmacySystem.Application.DTOs.CustomerLedgers;
using SmartPharmacySystem.Application.DTOs.InterBranchSettlements;
using SmartPharmacySystem.Application.DTOs.DailyClosings;
using SmartPharmacySystem.Application.DTOs.Notifications;
using PriceOverrideDto = SmartPharmacySystem.Application.DTOs.Customers.PriceOverrideDto;
using CreateExpenseCategoryDto = SmartPharmacySystem.Application.DTOs.Expense.CreateExpenseCategoryDto;
using ExpenseCategoryDto = SmartPharmacySystem.Application.DTOs.Expense.ExpenseCategoryDto;

namespace SmartPharmacySystem.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Category Mappings
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>();
            CreateMap<Category, CategoryDto>().ReverseMap();

            // Medicine Mappings
            CreateMap<CreateMedicineDto, Medicine>();
            CreateMap<UpdateMedicineDto, Medicine>();
            CreateMap<Medicine, MedicineDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
                .ReverseMap();
            CreateMap<Medicine, MedicineDetailsDto>()
                .IncludeBase<Medicine, MedicineDto>();

            // Supplier Mappings
            CreateMap<CreateSupplierDto, Supplier>();
            CreateMap<UpdateSupplierDto, Supplier>();
            CreateMap<Supplier, SupplierDto>()
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Balance == 0 ? "خالص" : "دائن"))
                .ForMember(dest => dest.StatusColor, opt => opt.MapFrom(src => src.Balance == 0 ? "#22c55e" : "#f97316"))
                .ForMember(dest => dest.StatusIcon, opt => opt.MapFrom(src => src.Balance == 0 ? "pi pi-check-circle" : "pi pi-exclamation-circle"))
                .ForMember(dest => dest.PurchaseInvoices, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseReturns, opt => opt.Ignore());
            // Explicit reverse (SupplierDto -> Supplier) - ignore computed and nav props
            CreateMap<SupplierDto, Supplier>()
                .ForMember(dest => dest.PurchaseInvoices, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseReturns, opt => opt.Ignore())
                .ForMember(dest => dest.Account, opt => opt.Ignore());

            // User Mappings
            CreateMap<CreateUserDto, User>();
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.RoleId, opt => opt.MapFrom(src => src.RoleId))
                .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.Role != null ? src.Role.Name : string.Empty))
                .ForMember(dest => dest.RoleDescription, opt => opt.MapFrom(src => src.Role != null ? src.Role.Description : null))
                .ReverseMap();

            // SupplierPayment Mappings
            CreateMap<CreateSupplierPaymentDto, SupplierPayment>();
            CreateMap<SupplierPayment, SupplierPaymentDto>()
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty))
                .ReverseMap();

            // Customer Mappings
            CreateMap<CreateCustomerDto, Customer>();
            CreateMap<UpdateCustomerDto, Customer>();
            CreateMap<Customer, CustomerDto>().ReverseMap();

            // CustomerReceipt Mappings
            CreateMap<CustomerReceipt, CustomerReceiptDto>()
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : string.Empty))
                .ReverseMap();
            CreateMap<CreateCustomerReceiptDto, CustomerReceipt>();

            // Price Override Mappings
            CreateMap<PriceOverride, PriceOverrideDto>()
                .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Medicine != null ? src.Medicine.Name : string.Empty))
                .ForMember(dest => dest.BatchNumber, opt => opt.MapFrom(src => src.Batch != null ? src.Batch.CompanyBatchNumber : string.Empty))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
                .ReverseMap();
            // Expense Category Mappings
            CreateMap<CreateExpenseCategoryDto, ExpenseCategory>();
            CreateMap<ExpenseCategory, ExpenseCategoryDto>().ReverseMap();

            // Expense Mappings
            CreateMap<CreateExpenseDto, Expense>();
            CreateMap<UpdateExpenseDto, Expense>();
            CreateMap<Expense, ExpenseDto>()
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty))
                .ForMember(dest => dest.AccountName, opt => opt.MapFrom(src => src.Account != null ? src.Account.Name : string.Empty))
                .ReverseMap();

            // MedicineBatch Mappings (Legacy)
            CreateMap<CreateMedicineBatchDto, MedicineBatch>();
            CreateMap<UpdateMedicineBatchDto, MedicineBatch>();
            CreateMap<MedicineBatch, MedicineBatchDto>().ReverseMap();

            // MedicineBatch Mappings (New)
            CreateMap<MedicineBatchCreateDto, MedicineBatch>()
                .ForMember(dest => dest.RemainingQuantity, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => "Active"));
            CreateMap<MedicineBatchUpdateDto, MedicineBatch>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<MedicineBatch, MedicineBatchResponseDto>()
                .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Medicine != null ? src.Medicine.Name : string.Empty))
                .ForMember(dest => dest.SoldQuantity, opt => opt.MapFrom(src => src.Quantity - src.RemainingQuantity))
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUser != null ? src.CreatedByUser.FullName : null))
                // Inventory Status Logic
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src =>
                    (src.ExpiryDate - DateTime.Now).TotalDays <= 7 ? "منتهي الصلاحية" :
                    (src.ExpiryDate - DateTime.Now).TotalDays <= 30 ? "قريب الانتهاء" : "صالح"))
                .ForMember(dest => dest.StatusColor, opt => opt.MapFrom(src =>
                    (src.ExpiryDate - DateTime.Now).TotalDays <= 7 ? "#8B0000" :
                    (src.ExpiryDate - DateTime.Now).TotalDays <= 30 ? "#ffc107" : "success"))
                .ForMember(dest => dest.StatusIcon, opt => opt.MapFrom(src =>
                    (src.ExpiryDate - DateTime.Now).TotalDays <= 7 ? "fas fa-exclamation-triangle" :
                    (src.ExpiryDate - DateTime.Now).TotalDays <= 30 ? "fas fa-exclamation-circle" : "fas fa-check"));

            // StockMovement Mappings (InventoryMovement Entity)
            CreateMap<CreateStockMovementDto, InventoryMovement>();
            CreateMap<UpdateStockMovementDto, InventoryMovement>();
            CreateMap<InventoryMovement, StockMovementDto>()
      .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
      .ForMember(dest => dest.FinancialDescription, opt => opt.MapFrom(src =>
          src.Notes.Contains("[FIN_DESC]")
          ? src.Notes.Split("[FIN_DESC]", StringSplitOptions.None)[1]
          : null))
      .ForMember(dest => dest.Notes, opt => opt.MapFrom(src =>
          src.Notes.Contains("[FIN_DESC]")
          ? src.Notes.Split("[FIN_DESC]", StringSplitOptions.None)[0]
          : src.Notes))
      .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Medicine != null ? src.Medicine.Name : string.Empty))
      .ForMember(dest => dest.BatchNumber, opt => opt.MapFrom(src => src.Batch != null ? src.Batch.CompanyBatchNumber : string.Empty));
            // PurchaseInvoice Mappings
            CreateMap<CreatePurchaseInvoiceDto, PurchaseInvoice>()
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());
            CreateMap<UpdatePurchaseInvoiceDto, PurchaseInvoice>();
            CreateMap<PurchaseInvoice, PurchaseInvoiceDto>()
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty))
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.PurchaseInvoiceDetails)) // Map Items List
                .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.Creator != null ? src.Creator.FullName : string.Empty))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.Approver != null ? src.Approver.FullName : null))
                .ForMember(dest => dest.CancelledByName, opt => opt.MapFrom(src => src.Canceller != null ? src.Canceller.FullName : null))
                // Status Tracking
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Draft ? "مسودة" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? "معتمدة" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? "ملغاة" : src.Status.ToString()))
                .ForMember(dest => dest.StatusColor, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Draft ? "warning" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? "success" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? "danger" : "info"))
                .ForMember(dest => dest.StatusIcon, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Draft ? "pi pi-pencil" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? "pi pi-check" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? "pi pi-times" : "pi pi-info-circle"))
                // Action Tracking
                .ForMember(dest => dest.ActionByName, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? (src.Canceller != null ? src.Canceller.FullName : "System") :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? (src.Approver != null ? src.Approver.FullName : "System") :
                    (src.Creator != null ? src.Creator.FullName : "System")))
                .ForMember(dest => dest.ActionDate, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? (src.CancelledAt ?? src.CreatedAt) :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? (src.ApprovedAt ?? src.CreatedAt) :
                    src.CreatedAt))
                .ReverseMap();

            // PurchaseInvoiceDetail Mappings
            CreateMap<CreatePurchaseInvoiceDetailDto, PurchaseInvoiceDetail>();
            CreateMap<UpdatePurchaseInvoiceDetailDto, PurchaseInvoiceDetail>();
            CreateMap<PurchaseInvoiceDetail, PurchaseInvoiceDetailDto>()
                .ForMember(dest => dest.PurchaseInvoiceNumber, opt => opt.MapFrom(src => src.PurchaseInvoice != null ? src.PurchaseInvoice.SupplierInvoiceNumber : string.Empty))
                .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Medicine != null ? src.Medicine.Name : string.Empty))
                .ForMember(dest => dest.CompanyBatchNumber, opt => opt.MapFrom(src => src.Batch != null ? src.Batch.CompanyBatchNumber : string.Empty))
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Quantity * src.PurchasePrice))
                .ForMember(dest => dest.SalePrice, opt => opt.MapFrom(src => src.SalePrice))
                // Expiry Logic
                .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.Batch != null ? (DateTime?)src.Batch.ExpiryDate : null))
                .ForMember(dest => dest.DaysUntilExpiry, opt => opt.MapFrom(src => src.Batch != null ? (src.Batch.ExpiryDate - DateTime.Now).Days : 0))
                .ForMember(dest => dest.CanSell, opt => opt.MapFrom(src => src.Batch != null && (src.Batch.ExpiryDate - DateTime.Now).Days > 0))
                .ForMember(dest => dest.ExpiryStatus, opt => opt.MapFrom(src =>
                    src.Batch == null ? "غير معروف" :
                    (src.Batch.ExpiryDate - DateTime.Now).Days <= 0 ? "منتهي الصلاحية" :
                    (src.Batch.ExpiryDate - DateTime.Now).Days <= 90 ? "قريب الانتهاء" :
                    "صالح"
                ))
                .ReverseMap();

            // SaleInvoice Mappings
            CreateMap<CreateSaleInvoiceDto, SaleInvoice>()
                .ForMember(dest => dest.InvoiceDate, opt => opt.MapFrom(src => src.InvoiceDate != default ? src.InvoiceDate : DateTime.Now))
                .ForMember(dest => dest.SaleInvoiceDetails, opt => opt.MapFrom(src => src.Details))
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.TotalCost, opt => opt.Ignore())
                .ForMember(dest => dest.TotalProfit, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId));

            CreateMap<UpdateSaleInvoiceDto, SaleInvoice>()
                .ForMember(dest => dest.SaleInvoiceDetails, opt => opt.MapFrom(src => src.Details))
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustomerId));

            CreateMap<SaleInvoice, SaleInvoiceDto>()
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.SaleInvoiceDetails))
                .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.Creator != null ? src.Creator.FullName : string.Empty))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.Approver != null ? src.Approver.FullName : null))
                .ForMember(dest => dest.CancelledByName, opt => opt.MapFrom(src => src.Canceller != null ? src.Canceller.FullName : null))
                // Status Tracking
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Draft ? "مسودة" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? "معتمدة" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? "ملغاة" : src.Status.ToString()))
                .ForMember(dest => dest.StatusColor, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Draft ? "warning" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? "success" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? "danger" : "info"))
                .ForMember(dest => dest.StatusIcon, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Draft ? "pi pi-pencil" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? "pi pi-check" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? "pi pi-times" : "pi pi-info-circle"))
                // Action Tracking
                .ForMember(dest => dest.ActionByName, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? (src.Canceller != null ? src.Canceller.FullName : "System") :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? (src.Approver != null ? src.Approver.FullName : "System") :
                    (src.Creator != null ? src.Creator.FullName : "System")))
                .ForMember(dest => dest.ActionDate, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? (src.CancelledAt ?? src.CreatedAt) :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? (src.ApprovedAt ?? src.CreatedAt) :
                    src.CreatedAt))
                .ReverseMap()
                .ForMember(dest => dest.SaleInvoiceDetails, opt => opt.Ignore())
                .ForMember(dest => dest.SaleInvoiceDetails, opt => opt.Ignore());

            // SaleInvoiceDetail Mappings
            CreateMap<CreateSaleInvoiceDetailDto, SaleInvoiceDetail>()
                .ForMember(dest => dest.SaleInvoice, opt => opt.Ignore())
                .ForMember(dest => dest.Medicine, opt => opt.Ignore())
                .ForMember(dest => dest.Batch, opt => opt.Ignore())
                .ForMember(dest => dest.UnitCost, opt => opt.Ignore())
                .ForMember(dest => dest.TotalLineAmount, opt => opt.Ignore())
                .ForMember(dest => dest.TotalCost, opt => opt.Ignore())
                .ForMember(dest => dest.Profit, opt => opt.Ignore());

            CreateMap<UpdateSaleInvoiceDetailDto, SaleInvoiceDetail>()
                .ForMember(dest => dest.SaleInvoice, opt => opt.Ignore())
                .ForMember(dest => dest.Medicine, opt => opt.Ignore())
                .ForMember(dest => dest.Batch, opt => opt.Ignore())
                .ForMember(dest => dest.UnitCost, opt => opt.Ignore())
                .ForMember(dest => dest.TotalLineAmount, opt => opt.Ignore())
                .ForMember(dest => dest.TotalCost, opt => opt.Ignore())
                .ForMember(dest => dest.Profit, opt => opt.Ignore());

            CreateMap<SaleInvoiceDetail, SaleInvoiceDetailDto>()
                .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Medicine != null ? src.Medicine.Name : string.Empty))
                .ForMember(dest => dest.CompanyBatchNumber, opt => opt.MapFrom(src => src.Batch != null ? src.Batch.CompanyBatchNumber : string.Empty))
                .ForMember(dest => dest.SaleInvoiceDate, opt => opt.MapFrom(src => (src.SaleInvoice != null && src.SaleInvoice.InvoiceDate != default) ? src.SaleInvoice.InvoiceDate : (src.SaleInvoice != null ? src.SaleInvoice.CreatedAt : DateTime.Now)))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.SaleInvoice != null ? src.SaleInvoice.CustomerName : string.Empty))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.SaleInvoice != null ? src.SaleInvoice.PaymentMethod.ToString() : string.Empty))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.SaleInvoice != null ? src.SaleInvoice.CreatedBy : 0))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.SaleInvoice != null ? src.SaleInvoice.CreatedAt : DateTime.Now))
                .ForMember(dest => dest.TotalLineAmount, opt => opt.MapFrom(src => src.TotalLineAmount))
                .ForMember(dest => dest.TotalCost, opt => opt.MapFrom(src => src.TotalCost))
                .ForMember(dest => dest.Profit, opt => opt.MapFrom(src => src.Profit))
                // Parent Invoice Totals (Dynamic)
                .ForMember(dest => dest.InvoiceTotalAmount, opt => opt.MapFrom(src => src.SaleInvoice != null ? src.SaleInvoice.TotalAmount : 0))
                .ForMember(dest => dest.InvoiceTotalCost, opt => opt.MapFrom(src => src.SaleInvoice != null ? src.SaleInvoice.TotalCost : 0))
                .ForMember(dest => dest.InvoiceTotalProfit, opt => opt.MapFrom(src => src.SaleInvoice != null ? src.SaleInvoice.TotalProfit : 0))
                // Batch Info
                .ForMember(dest => dest.BatchRemainingQuantity, opt => opt.MapFrom(src => src.Batch != null ? src.Batch.RemainingQuantity : 0))
                .ForMember(dest => dest.BatchSoldQuantity, opt => opt.MapFrom(src => src.Batch != null ? src.Batch.SoldQuantity : 0))
                .ForMember(dest => dest.BatchExpiryDate, opt => opt.MapFrom(src => src.Batch != null ? (DateTime?)src.Batch.ExpiryDate : null))
                .ReverseMap()
                .ForMember(dest => dest.SaleInvoice, opt => opt.Ignore())
                .ForMember(dest => dest.Medicine, opt => opt.Ignore())
                .ForMember(dest => dest.Batch, opt => opt.Ignore());

            // PurchaseReturn Mappings
            CreateMap<CreatePurchaseReturnDto, PurchaseReturn>()
                .ForMember(dest => dest.PurchaseReturnDetails, opt => opt.MapFrom(src => src.Details))  // ✅ Map Details to PurchaseReturnDetails
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore());

            CreateMap<UpdatePurchaseReturnDto, PurchaseReturn>();
            CreateMap<PurchaseReturn, PurchaseReturnDto>()
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty))
                .ForMember(dest => dest.PurchaseInvoiceNumber, opt => opt.MapFrom(src => src.PurchaseInvoice != null ? src.PurchaseInvoice.SupplierInvoiceNumber : string.Empty))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.TotalAmount))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.PurchaseReturnDetails)) // Map Items List
                .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.Creator != null ? src.Creator.FullName : string.Empty))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.Approver != null ? src.Approver.FullName : null))
                .ForMember(dest => dest.CancelledByName, opt => opt.MapFrom(src => src.Canceller != null ? src.Canceller.FullName : null))
                // Status Tracking
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Draft ? "مسودة" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? "معتمدة" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? "ملغاة" : src.Status.ToString()))
                .ForMember(dest => dest.StatusColor, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Draft ? "warning" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? "success" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? "danger" : "info"))
                .ForMember(dest => dest.StatusIcon, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Draft ? "pi pi-pencil" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? "pi pi-check" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? "pi pi-times" : "pi pi-info-circle"))
                // Action Tracking
                .ForMember(dest => dest.ActionByName, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? (src.Canceller != null ? src.Canceller.FullName : "System") :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? (src.Approver != null ? src.Approver.FullName : "System") :
                    (src.Creator != null ? src.Creator.FullName : "System")))
                .ForMember(dest => dest.ActionDate, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? (src.CancelledAt ?? src.CreatedAt) :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? (src.ApprovedAt ?? src.CreatedAt) :
                    src.CreatedAt))
                .ReverseMap();

            // PurchaseReturnDetail Mappings
            CreateMap<CreatePurchaseReturnDetailDto, PurchaseReturnDetail>()
                .ForMember(dest => dest.TotalReturn, opt => opt.Ignore());
            CreateMap<UpdatePurchaseReturnDetailDto, PurchaseReturnDetail>();
            CreateMap<PurchaseReturnDetail, PurchaseReturnDetailDto>()
                .ForMember(dest => dest.PurchaseReturnNumber, opt => opt.MapFrom(src => src.PurchaseReturn != null ? src.PurchaseReturn.Id.ToString() : string.Empty))
                .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Medicine != null ? src.Medicine.Name : string.Empty))
                .ForMember(dest => dest.CompanyBatchNumber, opt => opt.MapFrom(src => src.Batch != null ? src.Batch.CompanyBatchNumber : string.Empty))
                .ForMember(dest => dest.TotalReturn, opt => opt.MapFrom(src => src.Quantity * src.PurchasePrice))
                // Expiry Logic
                .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.Batch != null ? (DateTime?)src.Batch.ExpiryDate : null))
                .ForMember(dest => dest.DaysUntilExpiry, opt => opt.MapFrom(src => src.Batch != null ? (src.Batch.ExpiryDate - DateTime.Now).Days : 0))
                .ForMember(dest => dest.CanSell, opt => opt.MapFrom(src => src.Batch != null && (src.Batch.ExpiryDate - DateTime.Now).Days > 0))
                .ForMember(dest => dest.ExpiryStatus, opt => opt.MapFrom(src =>
                    src.Batch == null ? "غير معروف" :
                    (src.Batch.ExpiryDate - DateTime.Now).Days <= 0 ? "منتهي الصلاحية" :
                    (src.Batch.ExpiryDate - DateTime.Now).Days <= 90 ? "قريب الانتهاء" :
                    "صالح"
                ))
                .ReverseMap();

            // SalesReturn Mappings
            CreateMap<CreateSalesReturnDto, SalesReturn>()
                .ForMember(dest => dest.SalesReturnDetails, opt => opt.MapFrom(src => src.Details))
                .ForMember(dest => dest.SaleInvoice, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore())
                .ForMember(dest => dest.TotalCost, opt => opt.Ignore())
                .ForMember(dest => dest.TotalProfit, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

            CreateMap<UpdateSalesReturnDto, SalesReturn>()
                .ForMember(dest => dest.SalesReturnDetails, opt => opt.MapFrom(src => src.Details))
                .ForMember(dest => dest.SaleInvoice, opt => opt.Ignore());

            CreateMap<SalesReturn, SalesReturnDto>()
                 .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
                 .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.SaleInvoice != null ? src.SaleInvoice.CustomerName : string.Empty))
                 .ForMember(dest => dest.SaleInvoiceNumber, opt => opt.MapFrom(src => src.SaleInvoice != null ? src.SaleInvoice.SaleInvoiceNumber : string.Empty))
                 .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.SalesReturnDetails))
                  .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.Creator != null ? src.Creator.FullName : string.Empty))
                  .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.Approver != null ? src.Approver.FullName : null))
                  .ForMember(dest => dest.CancelledByName, opt => opt.MapFrom(src => src.Canceller != null ? src.Canceller.FullName : null))
                // Status Tracking
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Draft ? "مسودة" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? "معتمدة" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? "ملغاة" : src.Status.ToString()))
                .ForMember(dest => dest.StatusColor, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Draft ? "warning" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? "success" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? "danger" : "info"))
                .ForMember(dest => dest.StatusIcon, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Draft ? "pi pi-pencil" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? "pi pi-check" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? "pi pi-times" : "pi pi-info-circle"))
                // Action Tracking
                .ForMember(dest => dest.ActionByName, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? (src.Canceller != null ? src.Canceller.FullName : "System") :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? (src.Approver != null ? src.Approver.FullName : "System") :
                    (src.Creator != null ? src.Creator.FullName : "System")))
                .ForMember(dest => dest.ActionDate, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? (src.CancelledAt ?? src.CreatedAt) :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? (src.ApprovedAt ?? src.CreatedAt) :
                    src.CreatedAt))
                  .ReverseMap()
                 .ForMember(dest => dest.SalesReturnDetails, opt => opt.Ignore())
                 .ForMember(dest => dest.SaleInvoice, opt => opt.Ignore());

            // SalesReturnDetail Mappings
            CreateMap<CreateSalesReturnDetailDto, SalesReturnDetail>()
                .ForMember(dest => dest.SalesReturn, opt => opt.Ignore())
                .ForMember(dest => dest.Medicine, opt => opt.Ignore())
                .ForMember(dest => dest.Batch, opt => opt.Ignore())
                .ForMember(dest => dest.UnitCost, opt => opt.Ignore())
                .ForMember(dest => dest.TotalLineAmount, opt => opt.Ignore())
                .ForMember(dest => dest.TotalCost, opt => opt.Ignore())
                .ForMember(dest => dest.Profit, opt => opt.Ignore());

            CreateMap<UpdateSalesReturnDetailDto, SalesReturnDetail>()
                .ForMember(dest => dest.SalesReturn, opt => opt.Ignore())
                .ForMember(dest => dest.Medicine, opt => opt.Ignore())
                .ForMember(dest => dest.Batch, opt => opt.Ignore());

            CreateMap<SalesReturnDetail, SalesReturnDetailDto>()
                .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Medicine != null ? src.Medicine.Name : string.Empty))
                .ForMember(dest => dest.CompanyBatchNumber, opt => opt.MapFrom(src => src.Batch != null ? src.Batch.CompanyBatchNumber : string.Empty))
                .ForMember(dest => dest.TotalLineAmount, opt => opt.MapFrom(src => src.TotalLineAmount))
                .ForMember(dest => dest.TotalCost, opt => opt.MapFrom(src => src.TotalCost))
                .ForMember(dest => dest.Profit, opt => opt.MapFrom(src => src.Profit))
                .ForMember(dest => dest.TotalReturn, opt => opt.MapFrom(src => src.TotalLineAmount))
                .ReverseMap()
                .ForMember(dest => dest.SalesReturn, opt => opt.Ignore())
                .ForMember(dest => dest.Medicine, opt => opt.Ignore())
                .ForMember(dest => dest.Batch, opt => opt.Ignore());

            // Alert Mappings
            CreateMap<CreateAlertDto, Alert>();
            CreateMap<UpdateAlertDto, Alert>();
            CreateMap<Alert, AlertDto>()
                .ForMember(dest => dest.BatchNumber, opt => opt.MapFrom(src => src.Batch != null ? src.Batch.CompanyBatchNumber : string.Empty))
                .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Batch != null && src.Batch.Medicine != null ? src.Batch.Medicine.Name : string.Empty))
                .ReverseMap();

            // Financial Mappings
            CreateMap<PharmacyAccount, PharmacyAccountDto>();
            CreateMap<FinancialTransaction, FinancialTransactionDto>();

            // Role Mappings
            CreateMap<CreateRoleDto, Core.Entities.Role>();
            CreateMap<UpdateRoleDto, Core.Entities.Role>();
            CreateMap<Core.Entities.Role, RoleDto>();

            // ==================== Accounting Mappings ====================

            // Account Mappings
            CreateMap<CreateAccountDto, Account>();
            CreateMap<UpdateAccountDto, Account>();
            CreateMap<Account, AccountDto>()
                .ForMember(dest => dest.ParentName, opt => opt.MapFrom(src => src.Parent != null ? src.Parent.Name : string.Empty))
                .ReverseMap();

            // JournalEntry Mappings
            CreateMap<CreateJournalEntryDto, JournalEntry>()
                .ForMember(dest => dest.Lines, opt => opt.MapFrom(src => src.Lines));

            // DTO -> Entity: تجاهل خصائص التنقل لمنع إنشاء كيانات فارغة
            CreateMap<JournalEntryDto, JournalEntry>()
                .ForMember(dest => dest.Lines, opt => opt.MapFrom(src => src.Lines));

            CreateMap<JournalEntry, JournalEntryDto>()
                .ForMember(dest => dest.Lines, opt => opt.MapFrom(src => src.Lines));

            // JournalEntryLine Mappings
            CreateMap<CreateJournalEntryLineDto, JournalEntryLine>()
                .ForMember(dest => dest.Account, opt => opt.Ignore());

            // DTO -> Entity: تجاهل خاصية Account لمنع إنشاء حسابات فارغة
            CreateMap<JournalEntryLineDto, JournalEntryLine>()
                .ForMember(dest => dest.Account, opt => opt.Ignore())
                .ForMember(dest => dest.JournalEntry, opt => opt.Ignore());

            CreateMap<JournalEntryLine, JournalEntryLineDto>()
                .ForMember(dest => dest.AccountName, opt => opt.MapFrom(src => src.Account != null ? src.Account.Name : string.Empty))
                .ForMember(dest => dest.AccountCode, opt => opt.MapFrom(src => src.Account != null ? src.Account.Code : string.Empty));

            // Cheque Mappings
            CreateMap<CreateChequeDto, Cheque>();
            CreateMap<Cheque, ChequeDto>()
                .ForMember(dest => dest.BankAccountName, opt => opt.MapFrom(src => src.BankAccount != null ? src.BankAccount.Name : string.Empty))
                .ReverseMap();

            // Pharmacy Settings Mappings
            CreateMap<PharmacySettings, PharmacySettingsDto>().ReverseMap();
            CreateMap<UpdatePharmacySettingsDto, PharmacySettings>();

            // ==================== Multi-Branch & Inventory Mappings ====================

            // Branch Mappings
            CreateMap<CreateBranchDto, Branch>();
            CreateMap<UpdateBranchDto, Branch>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Branch, BranchDto>()
                .ForMember(dest => dest.BranchTypeName, opt => opt.MapFrom(src =>
                    src.BranchType == Core.Enums.BranchType.Main ? "فرع رئيسي" : "فرع فرعي"))
                .ForMember(dest => dest.WarehouseCount, opt => opt.MapFrom(src => src.Warehouses != null ? src.Warehouses.Count : 0))
                .ForMember(dest => dest.EmployeeCount, opt => opt.MapFrom(src => src.Employees != null ? src.Employees.Count : 0));

            // Warehouse Mappings
            CreateMap<CreateWarehouseDto, Warehouse>();
            CreateMap<UpdateWarehouseDto, Warehouse>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Warehouse, WarehouseDto>()
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
                .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src =>
                    src.Type == Core.Enums.WarehouseType.Main ? "مخزن رئيسي" :
                    src.Type == Core.Enums.WarehouseType.Branch ? "مخزن فرع" : "مخزن تالف"));

            // InventoryStock Mappings
            CreateMap<CreateInventoryStockDto, InventoryStock>();
            CreateMap<UpdateInventoryStockDto, InventoryStock>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<InventoryStock, InventoryStockDto>()
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
                .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Medicine != null ? src.Medicine.Name : string.Empty))
                .ForMember(dest => dest.MedicineBarcode, opt => opt.MapFrom(src => src.Medicine != null ? src.Medicine.DefaultBarcode : string.Empty))
                .ForMember(dest => dest.DaysUntilExpiry, opt => opt.MapFrom(src => (src.ExpiryDate - DateTime.Now).Days))
                .ForMember(dest => dest.ExpiryStatus, opt => opt.MapFrom(src =>
                    (src.ExpiryDate - DateTime.Now).Days <= 0 ? "منتهي" :
                    (src.ExpiryDate - DateTime.Now).Days <= 30 ? "قريب الانتهاء" :
                    (src.ExpiryDate - DateTime.Now).Days <= 90 ? "ينتهي قريباً" : "صالح"))
                .ForMember(dest => dest.ExpiryStatusColor, opt => opt.MapFrom(src =>
                    (src.ExpiryDate - DateTime.Now).Days <= 0 ? "#ef4444" :
                    (src.ExpiryDate - DateTime.Now).Days <= 30 ? "#f59e0b" :
                    (src.ExpiryDate - DateTime.Now).Days <= 90 ? "#eab308" : "#22c55e"));

            // MedicineWarehouseConfig Mappings
            CreateMap<CreateMedicineWarehouseConfigDto, MedicineWarehouseConfig>();
            CreateMap<UpdateMedicineWarehouseConfigDto, MedicineWarehouseConfig>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<MedicineWarehouseConfig, MedicineWarehouseConfigDto>()
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
                .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Medicine != null ? src.Medicine.Name : string.Empty))
                .ForMember(dest => dest.CurrentStock, opt => opt.Ignore())
                .ForMember(dest => dest.IsBelowReorderLevel, opt => opt.Ignore());

            // StockTransfer Mappings
            CreateMap<CreateStockTransferDto, StockTransfer>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items))
                .ForMember(dest => dest.TransferCode, opt => opt.Ignore())
                .ForMember(dest => dest.RequestedByUserId, opt => opt.Ignore());
            CreateMap<StockTransfer, StockTransferDto>()
                .ForMember(dest => dest.SourceWarehouseName, opt => opt.MapFrom(src => src.SourceWarehouse != null ? src.SourceWarehouse.Name : string.Empty))
                .ForMember(dest => dest.SourceBranchName, opt => opt.MapFrom(src => src.SourceWarehouse != null && src.SourceWarehouse.Branch != null ? src.SourceWarehouse.Branch.Name : string.Empty))
                .ForMember(dest => dest.DestinationWarehouseName, opt => opt.MapFrom(src => src.DestinationWarehouse != null ? src.DestinationWarehouse.Name : string.Empty))
                .ForMember(dest => dest.DestinationBranchName, opt => opt.MapFrom(src => src.DestinationWarehouse != null && src.DestinationWarehouse.Branch != null ? src.DestinationWarehouse.Branch.Name : string.Empty))
                .ForMember(dest => dest.RequestedByName, opt => opt.MapFrom(src => src.RequestedByUser != null ? src.RequestedByUser.FullName : string.Empty))
                .ForMember(dest => dest.RequestedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.ApprovedByUser != null ? src.ApprovedByUser.FullName : null))
                .ForMember(dest => dest.DispatchedByName, opt => opt.MapFrom(src => src.DispatchedByUser != null ? src.DispatchedByUser.FullName : null))
                .ForMember(dest => dest.ReceivedByName, opt => opt.MapFrom(src => src.ReceivedByUser != null ? src.ReceivedByUser.FullName : null))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src =>
                    src.Status == Core.Enums.TransferStatus.AutoRequested ? "طلب تلقائي" :
                    src.Status == Core.Enums.TransferStatus.Requested ? "قيد الانتظار" :
                    src.Status == Core.Enums.TransferStatus.Approved ? "معتمد" :
                    src.Status == Core.Enums.TransferStatus.Dispatched ? "تم الصرف" :
                    src.Status == Core.Enums.TransferStatus.Received ? "تم الاستلام" :
                    src.Status == Core.Enums.TransferStatus.Cancelled ? "ملغي" : src.Status.ToString()))
                .ForMember(dest => dest.StatusColor, opt => opt.MapFrom(src =>
                    src.Status == Core.Enums.TransferStatus.AutoRequested ? "warning" :
                    src.Status == Core.Enums.TransferStatus.Requested ? "info" :
                    src.Status == Core.Enums.TransferStatus.Approved ? "primary" :
                    src.Status == Core.Enums.TransferStatus.Dispatched ? "warning" :
                    src.Status == Core.Enums.TransferStatus.Received ? "success" :
                    src.Status == Core.Enums.TransferStatus.Cancelled ? "danger" : "secondary"))
                .ForMember(dest => dest.TransferTypeName, opt => opt.MapFrom(src =>
                    src.TransferType == Core.Enums.TransferType.Manual ? "يدوي" :
                    src.TransferType == Core.Enums.TransferType.AutoRequested ? "تلقائي" : "طلب فرع"))
                .ForMember(dest => dest.ItemsCount, opt => opt.MapFrom(src => src.Items != null ? src.Items.Count() : 0))
                .ForMember(dest => dest.TotalQuantityRequested, opt => opt.MapFrom(src => src.Items != null ? src.Items.Sum(i => i.QuantityRequested) : 0))
                .ForMember(dest => dest.TotalQuantityReceived, opt => opt.MapFrom(src => src.Items != null ? src.Items.Where(i => i.QuantityReceived.HasValue).Sum(i => i.QuantityReceived!.Value) : 0));

            // StockTransferItem Mappings
            CreateMap<CreateStockTransferItemDto, StockTransferItem>();
            CreateMap<StockTransferItem, StockTransferItemDto>()
                .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Medicine != null ? src.Medicine.Name : string.Empty))
                .ForMember(dest => dest.MedicineBarcode, opt => opt.MapFrom(src => src.Medicine != null ? src.Medicine.DefaultBarcode : string.Empty))
                .ForMember(dest => dest.Variance, opt => opt.MapFrom(src => (src.QuantityReceived ?? 0) - src.QuantityDispatched));

            // DamagedGoodsRecord Mappings
            CreateMap<CreateDamagedGoodsRecordDto, DamagedGoodsRecord>()
                .ForMember(dest => dest.DamageCode, opt => opt.Ignore())
                .ForMember(dest => dest.DamageValue, opt => opt.Ignore())
                .ForMember(dest => dest.RecordedByUserId, opt => opt.Ignore());
            CreateMap<DamagedGoodsRecord, DamagedGoodsRecordDto>()
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.SourceWarehouse != null ? src.SourceWarehouse.Name : string.Empty))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.SourceWarehouse != null && src.SourceWarehouse.Branch != null ? src.SourceWarehouse.Branch.Name : string.Empty))
                .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Medicine != null ? src.Medicine.Name : string.Empty))
                .ForMember(dest => dest.RecordedByName, opt => opt.MapFrom(src => src.RecordedByUser != null ? src.RecordedByUser.FullName : string.Empty))
                .ForMember(dest => dest.RecordedAt, opt => opt.MapFrom(src => src.CreatedAt))
                .ForMember(dest => dest.DamageTypeName, opt => opt.MapFrom(src =>
                    src.DamageType == Core.Enums.DamageType.Expired ? "انتهاء صلاحية" :
                    src.DamageType == Core.Enums.DamageType.PhysicalDamage ? "تلف فيزيائي" : "عيب تصنيع"))
                .ForMember(dest => dest.DisposalMethodName, opt => opt.MapFrom(src =>
                    src.DisposalMethod == Core.Enums.DisposalMethod.Destroyed ? "إتلاف" :
                    src.DisposalMethod == Core.Enums.DisposalMethod.ReturnedToSupplier ? "إرجاع للمورد" : "تبرع"))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src =>
                    src.Status == Core.Enums.RecordStatus.PendingApproval ? "بانتظار الاعتماد" :
                    src.Status == Core.Enums.RecordStatus.Approved ? "معتمد" : "مرفوض"))
                .ForMember(dest => dest.StatusColor, opt => opt.MapFrom(src =>
                    src.Status == Core.Enums.RecordStatus.PendingApproval ? "warning" :
                    src.Status == Core.Enums.RecordStatus.Approved ? "success" : "danger"))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.ApprovedByUser != null ? src.ApprovedByUser.FullName : null))
                .ForMember(dest => dest.RecordedByName, opt => opt.MapFrom(src => src.RecordedByUser != null ? src.RecordedByUser.FullName : string.Empty));

            // StockCountHeader Mappings
            CreateMap<CreateStockCountHeaderDto, StockCountHeader>()
                .ForMember(dest => dest.CountCode, opt => opt.Ignore())
                .ForMember(dest => dest.StartedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ApprovedByUserId, opt => opt.Ignore());
            CreateMap<StockCountHeader, StockCountHeaderDto>()
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Warehouse != null && src.Warehouse.Branch != null ? src.Warehouse.Branch.Name : string.Empty))
                .ForMember(dest => dest.CountTypeName, opt => opt.MapFrom(src =>
                    src.CountType == Core.Enums.StockCountType.Daily ? "يومي" :
                    src.CountType == Core.Enums.StockCountType.Monthly ? "شهري" :
                    src.CountType == Core.Enums.StockCountType.BiAnnual ? "نصف سنوي" : "سنوي"))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src =>
                    src.Status == Core.Enums.StockCountStatus.Draft ? "مسودة" :
                    src.Status == Core.Enums.StockCountStatus.InProgress ? "قيد التنفيذ" :
                    src.Status == Core.Enums.StockCountStatus.PendingApproval ? "بانتظار الاعتماد" :
                    src.Status == Core.Enums.StockCountStatus.Approved ? "معتمد" : "مغلق"))
                .ForMember(dest => dest.StatusColor, opt => opt.MapFrom(src =>
                    src.Status == Core.Enums.StockCountStatus.Draft ? "secondary" :
                    src.Status == Core.Enums.StockCountStatus.InProgress ? "info" :
                    src.Status == Core.Enums.StockCountStatus.PendingApproval ? "warning" :
                    src.Status == Core.Enums.StockCountStatus.Approved ? "success" : "dark"))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.ApprovedByUser != null ? src.ApprovedByUser.FullName : null))
                .ForMember(dest => dest.ItemsCount, opt => opt.MapFrom(src => src.Items != null ? src.Items.Count() : 0))
                .ForMember(dest => dest.TotalVarianceItems, opt => opt.MapFrom(src => src.Items != null ? src.Items.Count(i => (i.PhysicalQuantity ?? 0) != i.SystemQuantity) : 0))
                .ForMember(dest => dest.TotalVarianceValue, opt => opt.Ignore());

            // StockCountItem Mappings
            CreateMap<UpdateStockCountItemDto, StockCountItem>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<StockCountItem, StockCountItemDto>()
                .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Medicine != null ? src.Medicine.Name : string.Empty))
                .ForMember(dest => dest.MedicineBarcode, opt => opt.MapFrom(src => src.Medicine != null ? src.Medicine.DefaultBarcode : string.Empty))
                .ForMember(dest => dest.Variance, opt => opt.MapFrom(src => (src.PhysicalQuantity ?? 0) - src.SystemQuantity))
                .ForMember(dest => dest.VarianceValue, opt => opt.MapFrom(src => ((src.PhysicalQuantity ?? 0) - src.SystemQuantity) * src.PurchasePrice));

            // ==================== HR & Payroll Mappings ====================

            // Department Mappings
            CreateMap<CreateDepartmentDto, Department>();
            CreateMap<UpdateDepartmentDto, Department>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Department, DepartmentDto>()
                .ForMember(dest => dest.EmployeeCount, opt => opt.MapFrom(src => src.Employees != null ? src.Employees.Count : 0));

            // Employee Mappings
            CreateMap<CreateEmployeeDto, Employee>();
            CreateMap<UpdateEmployeeDto, Employee>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Employee, EmployeeDto>()
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : string.Empty))
                .ForMember(dest => dest.TotalLoans, opt => opt.Ignore())
                .ForMember(dest => dest.RemainingLoans, opt => opt.Ignore());

            // Attendance Mappings
            CreateMap<CreateAttendanceDto, Attendance>();
            CreateMap<UpdateAttendanceDto, Attendance>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<Attendance, AttendanceDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.FullName : string.Empty))
                .ForMember(dest => dest.WorkingBranchName, opt => opt.MapFrom(src => src.WorkingBranch != null ? src.WorkingBranch.Name : string.Empty))
                .ForMember(dest => dest.ShiftName, opt => opt.MapFrom(src =>
                    src.Shift == Core.Enums.ShiftType.Morning ? "صباحي" :
                    src.Shift == Core.Enums.ShiftType.Evening ? "مسائي" : "ليلي"))
                .ForMember(dest => dest.AttendanceStatusName, opt => opt.MapFrom(src =>
                    src.AttendanceStatus == Core.Enums.AttendanceStatus.Present ? "حاضر" :
                    src.AttendanceStatus == Core.Enums.AttendanceStatus.Absent ? "غائب" :
                    src.AttendanceStatus == Core.Enums.AttendanceStatus.Late ? "متأخر" : "بعذر"));

            // MonthlySalary Mappings
            CreateMap<MonthlySalary, MonthlySalaryDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.FullName : string.Empty))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
                .ForMember(dest => dest.PaymentStatusName, opt => opt.MapFrom(src =>
                    src.PaymentStatus == Core.Enums.PaymentStatus.Pending ? "قيد الانتظار" : "تم الصرف"))
                .ForMember(dest => dest.Deductions, opt => opt.MapFrom(src => src.Deductions));

            // SalaryDeductionItem Mappings
            CreateMap<SalaryDeductionItem, SalaryDeductionItemDto>()
                .ForMember(dest => dest.DeductionTypeName, opt => opt.MapFrom(src =>
                    src.DeductionType == Core.Enums.DeductionType.Absence ? "غياب" :
                    src.DeductionType == Core.Enums.DeductionType.LateArrival ? "تأخير" :
                    src.DeductionType == Core.Enums.DeductionType.LoanInstalment ? "قسط سلفة" :
                    src.DeductionType == Core.Enums.DeductionType.Insurance ? "تأمين" :
                    src.DeductionType == Core.Enums.DeductionType.Tax ? "ضريبة" : "أخرى"));

            // EmployeeLoan Mappings
            CreateMap<CreateEmployeeLoanDto, EmployeeLoan>();
            CreateMap<UpdateEmployeeLoanDto, EmployeeLoan>()
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
            CreateMap<EmployeeLoan, EmployeeLoanDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.FullName : string.Empty))
                .ForMember(dest => dest.InstalmentsPaid, opt => opt.Ignore())
                .ForMember(dest => dest.TotalInstalments, opt => opt.Ignore());

            // ==================== Customer & Finance Mappings ====================

            // CustomerLedger Mappings
            CreateMap<CreateCustomerLedgerDto, CustomerLedger>()
                .ForMember(dest => dest.Debit, opt => opt.MapFrom(src =>
                    src.TransactionType == Core.Enums.CustomerTransactionType.Invoice ? src.Amount : 0))
                .ForMember(dest => dest.Credit, opt => opt.MapFrom(src =>
                    src.TransactionType == Core.Enums.CustomerTransactionType.Receipt || src.TransactionType == Core.Enums.CustomerTransactionType.Return ? src.Amount : 0));
            CreateMap<CustomerLedger, CustomerLedgerDto>()
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : string.Empty))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
                .ForMember(dest => dest.TransactionTypeName, opt => opt.MapFrom(src =>
                    src.TransactionType == Core.Enums.CustomerTransactionType.Invoice ? "فاتورة" :
                    src.TransactionType == Core.Enums.CustomerTransactionType.Receipt ? "سداد" : "مرتجع"))
                .ForMember(dest => dest.RunningBalance, opt => opt.Ignore());

            // InterBranchSettlement Mappings
            CreateMap<CreateInterBranchSettlementDto, InterBranchSettlement>();
            CreateMap<InterBranchSettlement, InterBranchSettlementDto>()
                .ForMember(dest => dest.FromBranchName, opt => opt.MapFrom(src => src.FromBranch != null ? src.FromBranch.Name : string.Empty))
                .ForMember(dest => dest.ToBranchName, opt => opt.MapFrom(src => src.ToBranch != null ? src.ToBranch.Name : string.Empty))
                .ForMember(dest => dest.SettledByUserName, opt => opt.MapFrom(src => src.SettledByUser != null ? src.SettledByUser.FullName : null))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src =>
                    src.Status == Core.Enums.SettlementStatus.Pending ? "قيد الانتظار" : "تمت التسوية"))
                .ForMember(dest => dest.StatusColor, opt => opt.MapFrom(src =>
                    src.Status == Core.Enums.SettlementStatus.Pending ? "warning" : "success"));

            // DailyClosing Mappings
            CreateMap<CreateDailyClosingDto, DailyClosing>()
                .ForMember(dest => dest.BranchId, opt => opt.Ignore())
                .ForMember(dest => dest.TotalCashSales, opt => opt.Ignore())
                .ForMember(dest => dest.TotalCreditSales, opt => opt.Ignore())
                .ForMember(dest => dest.TotalCardSales, opt => opt.Ignore())
                .ForMember(dest => dest.TotalCollections, opt => opt.Ignore())
                .ForMember(dest => dest.TotalCashReturns, opt => opt.Ignore())
                .ForMember(dest => dest.TotalExpenses, opt => opt.Ignore())
                .ForMember(dest => dest.SubmittedByUserId, opt => opt.Ignore());
            CreateMap<DailyClosing, DailyClosingDto>()
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty))
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src =>
                    src.Status == Core.Enums.ClosingStatus.Draft ? "مسودة" :
                    src.Status == Core.Enums.ClosingStatus.PendingApproval ? "بانتظار الاعتماد" : "معتمد"))
                .ForMember(dest => dest.StatusColor, opt => opt.MapFrom(src =>
                    src.Status == Core.Enums.ClosingStatus.Draft ? "secondary" :
                    src.Status == Core.Enums.ClosingStatus.PendingApproval ? "warning" : "success"))
                .ForMember(dest => dest.SubmittedByName, opt => opt.MapFrom(src => src.SubmittedByUser != null ? src.SubmittedByUser.FullName : string.Empty))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.ApprovedByUser != null ? src.ApprovedByUser.FullName : null));

            // ==================== Notification Mappings ====================

            // Notification Mappings
            CreateMap<CreateNotificationDto, Notification>();
            CreateMap<Notification, NotificationDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : null))
                .ForMember(dest => dest.TypeName, opt => opt.MapFrom(src =>
                    src.Type == Core.Enums.NotificationType.LowStock ? "نقص مخزون" :
                    src.Type == Core.Enums.NotificationType.Expiry ? "انتهاء صلاحية" :
                    src.Type == Core.Enums.NotificationType.TransferRequest ? "طلب تحويل" :
                    src.Type == Core.Enums.NotificationType.DamageApproval ? "اعتماد تالف" :
                    src.Type == Core.Enums.NotificationType.EODPending ? "إغلاق يومي" : "فارق نقدي"))
                .ForMember(dest => dest.TypeIcon, opt => opt.MapFrom(src =>
                    src.Type == Core.Enums.NotificationType.LowStock ? "pi pi-exclamation-triangle" :
                    src.Type == Core.Enums.NotificationType.Expiry ? "pi pi-clock" :
                    src.Type == Core.Enums.NotificationType.TransferRequest ? "pi pi-send" :
                    src.Type == Core.Enums.NotificationType.DamageApproval ? "pi pi-check-circle" :
                    src.Type == Core.Enums.NotificationType.EODPending ? "pi pi-calendar" : "pi pi-dollar"))
                .ForMember(dest => dest.TimeAgo, opt => opt.Ignore());

            // ==================== New Service Mappings ====================

            // MonthlySalary Mappings
            CreateMap<CreateMonthlySalaryDto, MonthlySalary>();
            CreateMap<UpdateMonthlySalaryDto, MonthlySalary>();
            CreateMap<MonthlySalary, MonthlySalaryDto>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.FullName : string.Empty))
                .ForMember(dest => dest.EmployeeCode, opt => opt.MapFrom(src => src.Employee != null ? src.Employee.EmployeeCode : string.Empty))
                .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch != null ? src.Branch.Name : string.Empty));

          
            // InvoiceSequence Mappings
            CreateMap<InvoiceNumberSequence, InvoiceSequenceDto>()
                .ForMember(dest => dest.NextNumber, opt => opt.Ignore());
        }
    }
}
