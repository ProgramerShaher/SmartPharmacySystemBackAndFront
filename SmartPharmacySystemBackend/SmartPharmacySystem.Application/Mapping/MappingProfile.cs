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

            // Supplier Mappings
            CreateMap<CreateSupplierDto, Supplier>();
            CreateMap<UpdateSupplierDto, Supplier>();
            CreateMap<Supplier, SupplierDto>()
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src => src.Balance == 0 ? "خالص" : "مديونية"))
                .ForMember(dest => dest.StatusColor, opt => opt.MapFrom(src => src.Balance == 0 ? "#28a745" : "#fd7e14"))
                .ForMember(dest => dest.StatusIcon, opt => opt.MapFrom(src => src.Balance == 0 ? "fas fa-check" : "fas fa-hand-holding-usd"))
                .ReverseMap();

            // User Mappings
            // سيعمل التوفيق تلقائياً لأن الأسماء والأنواع أصبحت متطابقة
            CreateMap<CreateUserDto, User>();
            CreateMap<User, UserDto>().ReverseMap();

            // SupplierPayment Mappings
            CreateMap<CreateSupplierPaymentDto, SupplierPayment>();
            CreateMap<SupplierPayment, SupplierPaymentDto>()
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty))
                .ReverseMap();

            // Customer Mappings
            CreateMap<CreateCustomerDto, Customer>();
            CreateMap<UpdateCustomerDto, Customer>();
            CreateMap<Customer, CustomerDto>().ReverseMap();

            // CustomerReceipt Mappings
            CreateMap<CustomerReceipt, CustomerReceiptDto>()
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
                    (src.ExpiryDate - DateTime.Now).TotalDays <= 30 ? "#ffc107" : "#28a745"))
                .ForMember(dest => dest.StatusIcon, opt => opt.MapFrom(src =>
                    (src.ExpiryDate - DateTime.Now).TotalDays <= 7 ? "fas fa-exclamation-triangle" :
                    (src.ExpiryDate - DateTime.Now).TotalDays <= 30 ? "fas fa-exclamation-circle" : "fas fa-check"));

            // StockMovement Mappings (InventoryMovement Entity)
            CreateMap<CreateStockMovementDto, InventoryMovement>();
            CreateMap<UpdateStockMovementDto, InventoryMovement>();
            CreateMap<InventoryMovement, StockMovementDto>()
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
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.PurchaseInvoiceDetails != null ? src.PurchaseInvoiceDetails.Sum(d => d.Quantity * d.PurchasePrice) : 0))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.PurchaseInvoiceDetails)) // Map Items List
                .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.Creator != null ? src.Creator.FullName : string.Empty))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.Approver != null ? src.Approver.FullName : null))
                .ForMember(dest => dest.CancelledByName, opt => opt.MapFrom(src => src.Canceller != null ? src.Canceller.FullName : null))
                // Status Tracking
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Draft ? "مسودة" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? "مكتمل" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? "ملغى" : src.Status.ToString()))
                .ForMember(dest => dest.StatusColor, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Draft ? "#6c757d" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? "#28a745" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? "#dc3545" : "#007bff"))
                .ForMember(dest => dest.StatusIcon, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Draft ? "fas fa-file-edit" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? "fas fa-check-circle" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? "fas fa-times-circle" : "fas fa-info-circle"))
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
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.SaleInvoiceDetails))
                .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.Creator != null ? src.Creator.FullName : string.Empty))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.Approver != null ? src.Approver.FullName : null))
                .ForMember(dest => dest.CancelledByName, opt => opt.MapFrom(src => src.Canceller != null ? src.Canceller.FullName : null))
                // Status Tracking
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Draft ? "مسودة" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? "مكتمل" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? "ملغى" : src.Status.ToString()))
                .ForMember(dest => dest.StatusColor, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Draft ? "#6c757d" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? "#28a745" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? "#dc3545" : "#007bff"))
                .ForMember(dest => dest.StatusIcon, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Draft ? "fas fa-file-edit" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? "fas fa-check-circle" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? "fas fa-times-circle" : "fas fa-info-circle"))
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
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
                .ForMember(dest => dest.TotalAmount, opt => opt.Ignore());
            CreateMap<UpdatePurchaseReturnDto, PurchaseReturn>();
            CreateMap<PurchaseReturn, PurchaseReturnDto>()
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty))
                .ForMember(dest => dest.PurchaseInvoiceNumber, opt => opt.MapFrom(src => src.PurchaseInvoice != null ? src.PurchaseInvoice.SupplierInvoiceNumber : string.Empty))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.PurchaseReturnDetails != null ? src.PurchaseReturnDetails.Sum(d => d.Quantity * d.PurchasePrice) : 0))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.PurchaseReturnDetails)) // Map Items List
                .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.Creator != null ? src.Creator.FullName : string.Empty))
                .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.Approver != null ? src.Approver.FullName : null))
                .ForMember(dest => dest.CancelledByName, opt => opt.MapFrom(src => src.Canceller != null ? src.Canceller.FullName : null))
                // Status Tracking
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Draft ? "مسودة" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? "مكتمل" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? "ملغى" : src.Status.ToString()))
                .ForMember(dest => dest.StatusColor, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Draft ? "#6c757d" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? "#28a745" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? "#dc3545" : "#007bff"))
                .ForMember(dest => dest.StatusIcon, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Draft ? "fas fa-file-edit" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? "fas fa-check-circle" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? "fas fa-times-circle" : "fas fa-info-circle"))
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
                 .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.SaleInvoice != null ? src.SaleInvoice.CustomerName : string.Empty))
                 .ForMember(dest => dest.SaleInvoiceNumber, opt => opt.MapFrom(src => src.SaleInvoice != null ? src.SaleInvoice.SaleInvoiceNumber : string.Empty))
                 .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.SalesReturnDetails))
                  .ForMember(dest => dest.CreatedByName, opt => opt.MapFrom(src => src.Creator != null ? src.Creator.FullName : string.Empty))
                  .ForMember(dest => dest.ApprovedByName, opt => opt.MapFrom(src => src.Approver != null ? src.Approver.FullName : null))
                  .ForMember(dest => dest.CancelledByName, opt => opt.MapFrom(src => src.Canceller != null ? src.Canceller.FullName : null))
                // Status Tracking
                .ForMember(dest => dest.StatusName, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Draft ? "مسودة" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? "مكتمل" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? "ملغى" : src.Status.ToString()))
                .ForMember(dest => dest.StatusColor, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Draft ? "#6c757d" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? "#28a745" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? "#dc3545" : "#007bff"))
                .ForMember(dest => dest.StatusIcon, opt => opt.MapFrom(src =>
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Draft ? "fas fa-file-edit" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Approved ? "fas fa-check-circle" :
                    src.Status == SmartPharmacySystem.Core.Enums.DocumentStatus.Cancelled ? "fas fa-times-circle" : "fas fa-info-circle"))
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
        }
    }
}
