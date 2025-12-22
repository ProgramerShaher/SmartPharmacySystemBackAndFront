using AutoMapper;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Application.DTOs.Categories;
using SmartPharmacySystem.Application.DTOs.Medicine;
using SmartPharmacySystem.Application.DTOs.Suppliers;
using SmartPharmacySystem.Application.DTOs.User;
using SmartPharmacySystem.Application.DTOs.Expense;
using SmartPharmacySystem.Application.DTOs.MedicineBatch;
using SmartPharmacySystem.Application.DTOs.StockMovement;
using SmartPharmacySystem.Application.DTOs.Alerts;
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
            CreateMap<Supplier, SupplierDto>().ReverseMap();

            // User Mappings
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>()
                .ForMember(dest => dest.Password, opt => opt.Ignore());
            CreateMap<User, UserDto>().ReverseMap();

            // Expense Mappings
            CreateMap<CreateExpenseDto, Expense>();
            CreateMap<UpdateExpenseDto, Expense>();
            CreateMap<Expense, ExpenseDto>().ReverseMap();

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
                .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUser != null ? src.CreatedByUser.FullName : null));

            // StockMovement Mappings (InventoryMovement Entity)
            CreateMap<CreateStockMovementDto, InventoryMovement>();
            CreateMap<UpdateStockMovementDto, InventoryMovement>();
            CreateMap<InventoryMovement, StockMovementDto>()
                 .ForMember(dest => dest.MedicineId, opt => opt.MapFrom(src => src.MedicineId))
                 .ReverseMap();

            // PurchaseInvoice Mappings
            CreateMap<CreatePurchaseInvoiceDto, PurchaseInvoice>();
            CreateMap<UpdatePurchaseInvoiceDto, PurchaseInvoice>();
            CreateMap<PurchaseInvoice, PurchaseInvoiceDto>()
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.PurchaseInvoiceDetails != null ? src.PurchaseInvoiceDetails.Sum(d => d.Quantity * d.PurchasePrice) : 0)) // Calculate dynamically as requested
                .ReverseMap();

            // PurchaseInvoiceDetail Mappings
            CreateMap<CreatePurchaseInvoiceDetailDto, PurchaseInvoiceDetail>();
            CreateMap<UpdatePurchaseInvoiceDetailDto, PurchaseInvoiceDetail>();
            CreateMap<PurchaseInvoiceDetail, PurchaseInvoiceDetailDto>()
                .ForMember(dest => dest.PurchaseInvoiceNumber, opt => opt.MapFrom(src => src.PurchaseInvoice != null ? src.PurchaseInvoice.SupplierInvoiceNumber : string.Empty))
                .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Medicine != null ? src.Medicine.Name : string.Empty))
                .ForMember(dest => dest.CompanyBatchNumber, opt => opt.MapFrom(src => src.Batch != null ? src.Batch.CompanyBatchNumber : string.Empty))
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Quantity * src.PurchasePrice))
                .ReverseMap();

            // SaleInvoice Mappings
            CreateMap<CreateSaleInvoiceDto, SaleInvoice>()
                .ForMember(dest => dest.InvoiceDate, opt => opt.MapFrom(src => src.SaleInvoiceDate));
            CreateMap<UpdateSaleInvoiceDto, SaleInvoice>();
            CreateMap<SaleInvoice, SaleInvoiceDto>()
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.SaleInvoiceDetails != null ? src.SaleInvoiceDetails.Sum(d => d.Quantity * d.SalePrice) : 0))
                .ForMember(dest => dest.TotalCost, opt => opt.MapFrom(src => src.SaleInvoiceDetails != null ? src.SaleInvoiceDetails.Sum(d => d.Quantity * d.UnitCost) : 0))
                .ForMember(dest => dest.TotalProfit, opt => opt.MapFrom(src => src.SaleInvoiceDetails != null ? src.SaleInvoiceDetails.Sum(d => (d.Quantity * d.SalePrice) - (d.Quantity * d.UnitCost)) : 0))
                .ReverseMap();

            // SaleInvoiceDetail Mappings
            CreateMap<CreateSaleInvoiceDetailDto, SaleInvoiceDetail>()
                .ForMember(dest => dest.UnitCost, opt => opt.MapFrom(src => src.UnitCost)); // Ensure UnitCost is mapped if passed
            CreateMap<UpdateSaleInvoiceDetailDto, SaleInvoiceDetail>();
            CreateMap<SaleInvoiceDetail, SaleInvoiceDetailDto>()
                .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Medicine != null ? src.Medicine.Name : string.Empty))
                .ForMember(dest => dest.CompanyBatchNumber, opt => opt.MapFrom(src => src.Batch != null ? src.Batch.CompanyBatchNumber : string.Empty))
                .ForMember(dest => dest.SaleInvoiceDate, opt => opt.MapFrom(src => (src.SaleInvoice != null && src.SaleInvoice.InvoiceDate != default) ? src.SaleInvoice.InvoiceDate : (src.SaleInvoice != null ? src.SaleInvoice.CreatedAt : DateTime.Now)))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.SaleInvoice != null ? src.SaleInvoice.CustomerName : string.Empty))
                .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.SaleInvoice != null ? src.SaleInvoice.PaymentMethod : string.Empty))
                .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.SaleInvoice != null ? src.SaleInvoice.CreatedBy : 0))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.SaleInvoice != null ? src.SaleInvoice.CreatedAt : DateTime.Now))
                .ForMember(dest => dest.TotalLineAmount, opt => opt.MapFrom(src => src.Quantity * src.SalePrice))
                .ForMember(dest => dest.TotalCost, opt => opt.MapFrom(src => src.Quantity * src.UnitCost))
                .ForMember(dest => dest.Profit, opt => opt.MapFrom(src => (src.Quantity * src.SalePrice) - (src.Quantity * src.UnitCost)))
                // Parent Invoice Totals (Dynamic)
                .ForMember(dest => dest.InvoiceTotalAmount, opt => opt.MapFrom(src => src.SaleInvoice != null && src.SaleInvoice.SaleInvoiceDetails != null
                    ? src.SaleInvoice.SaleInvoiceDetails.Sum(d => d.Quantity * d.SalePrice) : 0))
                .ForMember(dest => dest.InvoiceTotalCost, opt => opt.MapFrom(src => src.SaleInvoice != null && src.SaleInvoice.SaleInvoiceDetails != null
                    ? src.SaleInvoice.SaleInvoiceDetails.Sum(d => d.Quantity * d.UnitCost) : 0))
                .ForMember(dest => dest.InvoiceTotalProfit, opt => opt.MapFrom(src => src.SaleInvoice != null && src.SaleInvoice.SaleInvoiceDetails != null
                    ? src.SaleInvoice.SaleInvoiceDetails.Sum(d => (d.Quantity * d.SalePrice) - (d.Quantity * d.UnitCost)) : 0))
                // Batch Info
                .ForMember(dest => dest.BatchRemainingQuantity, opt => opt.MapFrom(src => src.Batch != null ? src.Batch.RemainingQuantity : 0))
                .ForMember(dest => dest.BatchSoldQuantity, opt => opt.MapFrom(src => src.Batch != null ? src.Batch.Quantity - src.Batch.RemainingQuantity : 0))
                .ForMember(dest => dest.BatchExpiryDate, opt => opt.MapFrom(src => src.Batch != null ? (DateTime?)src.Batch.ExpiryDate : null))
                .ReverseMap();

            // PurchaseReturn Mappings
            CreateMap<CreatePurchaseReturnDto, PurchaseReturn>();
            CreateMap<UpdatePurchaseReturnDto, PurchaseReturn>();
            CreateMap<PurchaseReturn, PurchaseReturnDto>()
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty))
                .ForMember(dest => dest.PurchaseInvoiceNumber, opt => opt.MapFrom(src => src.PurchaseInvoice != null ? src.PurchaseInvoice.SupplierInvoiceNumber : string.Empty))
                .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.PurchaseReturnDetails != null ? src.PurchaseReturnDetails.Sum(d => d.Quantity * d.PurchasePrice) : 0))
                .ReverseMap();

            // PurchaseReturnDetail Mappings
            CreateMap<CreatePurchaseReturnDetailDto, PurchaseReturnDetail>();
            CreateMap<UpdatePurchaseReturnDetailDto, PurchaseReturnDetail>();
            CreateMap<PurchaseReturnDetail, PurchaseReturnDetailDto>()
                .ForMember(dest => dest.PurchaseReturnNumber, opt => opt.MapFrom(src => src.PurchaseReturn != null ? src.PurchaseReturn.Id.ToString() : string.Empty))
                .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Medicine != null ? src.Medicine.Name : string.Empty))
                .ForMember(dest => dest.CompanyBatchNumber, opt => opt.MapFrom(src => src.Batch != null ? src.Batch.CompanyBatchNumber : string.Empty))
                .ForMember(dest => dest.TotalReturn, opt => opt.MapFrom(src => src.Quantity * src.PurchasePrice))
                .ReverseMap();

            // SalesReturn Mappings
            CreateMap<CreateSalesReturnDto, SalesReturn>();
            CreateMap<UpdateSalesReturnDto, SalesReturn>();
            CreateMap<SalesReturn, SalesReturnDto>()
                 .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.SaleInvoice != null ? src.SaleInvoice.CustomerName : string.Empty))
                 .ForMember(dest => dest.SaleInvoiceNumber, opt => opt.MapFrom(src => src.SaleInvoice != null ? src.SaleInvoice.Id.ToString() : string.Empty))
                 .ForMember(dest => dest.TotalAmount, opt => opt.MapFrom(src => src.SalesReturnDetails != null ? src.SalesReturnDetails.Sum(d => d.Quantity * d.SalePrice) : 0))
                 .ReverseMap();

            // SalesReturnDetail Mappings
            CreateMap<CreateSalesReturnDetailDto, SalesReturnDetail>();
            CreateMap<UpdateSalesReturnDetailDto, SalesReturnDetail>();
            CreateMap<SalesReturnDetail, SalesReturnDetailDto>()
                .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Medicine != null ? src.Medicine.Name : string.Empty))
                .ForMember(dest => dest.CompanyBatchNumber, opt => opt.MapFrom(src => src.Batch != null ? src.Batch.CompanyBatchNumber : string.Empty))
                .ForMember(dest => dest.TotalReturn, opt => opt.MapFrom(src => src.Quantity * src.SalePrice))
                .ReverseMap();

            // Alert Mappings
            CreateMap<CreateAlertDto, Alert>();
            CreateMap<UpdateAlertDto, Alert>();
            CreateMap<Alert, AlertDto>()
                .ForMember(dest => dest.BatchNumber, opt => opt.MapFrom(src => src.Batch != null ? src.Batch.CompanyBatchNumber : string.Empty))
                .ForMember(dest => dest.MedicineName, opt => opt.MapFrom(src => src.Batch != null && src.Batch.Medicine != null ? src.Batch.Medicine.Name : string.Empty))
                .ForMember(dest => dest.ExpiryDate, opt => opt.MapFrom(src => src.Batch != null ? (DateTime?)src.Batch.ExpiryDate : null))
                .ForMember(dest => dest.Message, opt => opt.MapFrom(src => AlertMessageHelper.GenerateMessage(src.AlertType, src.Batch, "ar")))
                .ReverseMap();

            // Financial Mappings
            CreateMap<PharmacyAccount, PharmacyAccountDto>();
            CreateMap<FinancialTransaction, FinancialTransactionDto>();
            CreateMap<FinancialInvoice, FinancialInvoiceDto>();
        }
    }
}
