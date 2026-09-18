using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using SmartPharmacySystem.Application.DTOs.Quotations;
using SmartPharmacySystem.Application.DTOs.SalesInvoiceDetails;
using SmartPharmacySystem.Application.DTOs.SalesInvoices;
using SmartPharmacySystem.Application.DTOs.Shared;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

/// <summary>
/// تطبيق خدمة عروض الأسعار وقوائم الأسعار التقديرية وتحويلها إلى فواتير مبيعات
/// </summary>
public class QuotationService : IQuotationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ISaleInvoiceService _saleInvoiceService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<QuotationService> _logger;

    public QuotationService(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ISaleInvoiceService saleInvoiceService,
        ICurrentUserService currentUserService,
        ILogger<QuotationService> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _saleInvoiceService = saleInvoiceService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<QuotationDto?> GetByIdAsync(int id)
    {
        var quotation = await _unitOfWork.Quotations.GetByIdWithDetailsAsync(id);
        if (quotation == null) return null;

        var dto = _mapper.Map<QuotationDto>(quotation);
        return dto;
    }

    public async Task<QuotationPrintDto?> GetPrintDataAsync(int id)
    {
        var quotation = await _unitOfWork.Quotations.GetByIdWithDetailsAsync(id);
        if (quotation == null) return null;

        var settings = await _unitOfWork.PharmacySettings.GetSettingsAsync();

        var printDto = new QuotationPrintDto
        {
            Id = quotation.Id,
            QuotationNumber = quotation.QuotationNumber,
            QuotationDate = quotation.QuotationDate,
            ExpiryDate = quotation.ExpiryDate,
            StatusName = quotation.Status switch
            {
                QuotationStatus.Draft => "مسودة",
                QuotationStatus.Sent => "مرسل للعميل",
                QuotationStatus.Accepted => "مقبول",
                QuotationStatus.Rejected => "مرفوض",
                QuotationStatus.Expired => "منتهي الصلاحية",
                QuotationStatus.ConvertedToInvoice => "محول لفاتورة مبيعات",
                _ => quotation.Status.ToString()
            },
            CompanyName = settings?.PharmacyName ?? "المؤسسة التجارية",
            CompanyTaxNumber = settings?.TaxNumber,
            CompanyCommercialRegister = settings?.CommercialRegister,
            CompanyAddress = settings?.Address,
            CompanyPhone = settings?.PhoneNumber,
            CompanyEmail = settings?.Email,
            CompanyLogoUrl = settings?.LogoUrl,
            BranchName = quotation.Branch?.Name ?? string.Empty,

            CustomerName = quotation.Customer?.Name ?? quotation.CustomerName,
            CustomerPhone = quotation.Customer?.PhoneNumber ?? quotation.CustomerPhone,
            CustomerEmail = quotation.Customer?.Email ?? quotation.CustomerEmail,
            CustomerTaxNumber = null,
            CustomerAddress = quotation.Customer?.Address,

            Subtotal = quotation.Subtotal,
            TaxRate = quotation.TaxRate,
            TaxAmount = quotation.TaxAmount,
            IsTaxInclusive = quotation.IsTaxInclusive,
            TotalDiscount = quotation.TotalDiscount,
            TotalAmount = quotation.TotalAmount,

            Notes = quotation.Notes,
            TermsAndConditions = quotation.TermsAndConditions,

            Details = quotation.QuotationDetails.Select(d => new QuotationDetailDto
            {
                Id = d.Id,
                QuotationId = d.QuotationId,
                MedicineId = d.MedicineId,
                MedicineName = d.Medicine != null ? d.Medicine.Name : string.Empty,
                MedicineCode = d.Medicine?.InternalCode,
                Barcode = d.Medicine?.DefaultBarcode,
                SaleUnitId = d.SaleUnitId,
                UnitName = d.SaleUnit?.Name,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                DiscountPercentage = d.DiscountPercentage,
                DiscountAmount = d.DiscountAmount,
                TaxRate = d.TaxRate,
                TaxAmount = d.TaxAmount,
                Subtotal = d.Subtotal,
                Total = d.Total,
                Notes = d.Notes
            }).ToList()
        };

        return printDto;
    }

    public async Task<PagedResult<QuotationDto>> GetPagedAsync(QuotationFilterDto filter)
    {
        var (items, totalCount) = await _unitOfWork.Quotations.GetPagedAsync(
            filter.Search,
            filter.BranchId,
            filter.CustomerId,
            filter.Status,
            filter.DateFrom,
            filter.DateTo,
            filter.Page,
            filter.PageSize);

        var dtos = _mapper.Map<IEnumerable<QuotationDto>>(items);
        return new PagedResult<QuotationDto>(dtos, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<QuotationDto> CreateAsync(CreateQuotationDto dto, int userId)
    {
        if (dto.Details == null || !dto.Details.Any())
            throw new InvalidOperationException("يجب إضافة صنف واحد على الأقل في عرض السعر.");

        int branchId = dto.BranchId ?? _currentUserService.GetCurrentBranchId() ?? 1;
        string quotationNumber = await _unitOfWork.Quotations.GetNextQuotationNumberAsync(branchId);

        var quotation = new Quotation
        {
            BranchId = branchId,
            QuotationNumber = quotationNumber,
            QuotationDate = dto.QuotationDate.Kind == DateTimeKind.Utc ? dto.QuotationDate : dto.QuotationDate.ToUniversalTime(),
            ExpiryDate = dto.ExpiryDate.HasValue ? (dto.ExpiryDate.Value.Kind == DateTimeKind.Utc ? dto.ExpiryDate.Value : dto.ExpiryDate.Value.ToUniversalTime()) : null,
            CustomerId = dto.CustomerId,
            CustomerName = dto.CustomerName,
            CustomerPhone = dto.CustomerPhone,
            CustomerEmail = dto.CustomerEmail,
            Status = QuotationStatus.Draft,
            Notes = dto.Notes,
            TermsAndConditions = dto.TermsAndConditions,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        CalculateQuotationFinancials(quotation, dto.Details, dto.TotalDiscount, dto.TaxRate, dto.IsTaxInclusive);

        await _unitOfWork.Quotations.AddAsync(quotation);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("تم إنشاء عرض سعر جديد رقم {QuotationNumber} للمنشأة بنجاح.", quotationNumber);

        var created = await _unitOfWork.Quotations.GetByIdWithDetailsAsync(quotation.Id);
        return _mapper.Map<QuotationDto>(created);
    }

    public async Task<QuotationDto> UpdateAsync(int id, UpdateQuotationDto dto, int userId)
    {
        var quotation = await _unitOfWork.Quotations.GetByIdWithDetailsAsync(id)
            ?? throw new KeyNotFoundException($"عرض السعر برقم {id} غير موجود");

        if (quotation.Status == QuotationStatus.ConvertedToInvoice)
            throw new InvalidOperationException("لا يمكن تعديل عرض سعر تم تحويله مسبقاً إلى فاتورة مبيعات.");

        if (dto.Details == null || !dto.Details.Any())
            throw new InvalidOperationException("يجب إضافة صنف واحد على الأقل في عرض السعر.");

        quotation.ExpiryDate = dto.ExpiryDate.HasValue ? (dto.ExpiryDate.Value.Kind == DateTimeKind.Utc ? dto.ExpiryDate.Value : dto.ExpiryDate.Value.ToUniversalTime()) : null;
        quotation.CustomerId = dto.CustomerId;
        quotation.CustomerName = dto.CustomerName;
        quotation.CustomerPhone = dto.CustomerPhone;
        quotation.CustomerEmail = dto.CustomerEmail;
        quotation.Notes = dto.Notes;
        quotation.TermsAndConditions = dto.TermsAndConditions;
        quotation.UpdatedAt = DateTime.UtcNow;

        if (dto.Status.HasValue)
        {
            quotation.Status = dto.Status.Value;
        }

        CalculateQuotationFinancials(quotation, dto.Details, dto.TotalDiscount, dto.TaxRate, dto.IsTaxInclusive);

        await _unitOfWork.Quotations.UpdateAsync(quotation);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("تم تحديث عرض السعر رقم {QuotationNumber} بنجاح.", quotation.QuotationNumber);

        var updated = await _unitOfWork.Quotations.GetByIdWithDetailsAsync(quotation.Id);
        return _mapper.Map<QuotationDto>(updated);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var quotation = await _unitOfWork.Quotations.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"عرض السعر برقم {id} غير موجود");

        if (quotation.Status == QuotationStatus.ConvertedToInvoice)
            throw new InvalidOperationException("لا يمكن حذف عرض سعر تم تحويله إلى فاتورة مبيعات.");

        await _unitOfWork.Quotations.DeleteAsync(id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("تم حذف عرض السعر رقم {QuotationNumber}.", quotation.QuotationNumber);
        return true;
    }

    public async Task<QuotationDto> ChangeStatusAsync(int id, QuotationStatus newStatus, int userId)
    {
        var quotation = await _unitOfWork.Quotations.GetByIdWithDetailsAsync(id)
            ?? throw new KeyNotFoundException($"عرض السعر برقم {id} غير موجود");

        if (quotation.Status == QuotationStatus.ConvertedToInvoice)
            throw new InvalidOperationException("لا يمكن تعديل حالة عرض سعر تم تحويله إلى فاتورة مبيعات.");

        quotation.Status = newStatus;
        quotation.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Quotations.UpdateAsync(quotation);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("تم تغيير حالة عرض السعر رقم {QuotationNumber} إلى {Status}.", quotation.QuotationNumber, newStatus);
        return _mapper.Map<QuotationDto>(quotation);
    }

    public async Task<SaleInvoiceDto> ConvertToSaleInvoiceAsync(int id, ConvertQuotationToInvoiceDto dto, int userId)
    {
        var quotation = await _unitOfWork.Quotations.GetByIdWithDetailsAsync(id)
            ?? throw new KeyNotFoundException($"عرض السعر برقم {id} غير موجود");

        if (quotation.Status == QuotationStatus.ConvertedToInvoice)
            throw new InvalidOperationException($"عرض السعر تم تحويله مسبقاً إلى فاتورة مبيعات برقم {quotation.ConvertedSaleInvoiceId}");

        if (quotation.Status == QuotationStatus.Rejected)
            throw new InvalidOperationException("لا يمكن تحويل عرض سعر مرفوض إلى فاتورة مبيعات.");

        if (quotation.Status == QuotationStatus.Expired || (quotation.ExpiryDate.HasValue && quotation.ExpiryDate.Value.Date < DateTime.UtcNow.Date))
            throw new InvalidOperationException("عرض السعر منتهي الصلاحية ولا يمكن تحويله مباشرة. يرجى تجديد العرض أولاً.");

        if (!quotation.QuotationDetails.Any())
            throw new InvalidOperationException("عرض السعر لا يحتوي على أي بنود صالحة للتحويل.");

        // إعداد كائن طلب إنشاء فاتورة مبيعات رسمية بكامل مواصفات ومعايير النظام
        var createSaleInvoiceDto = new CreateSaleInvoiceDto
        {
            InvoiceDate = DateTime.Now,
            PaymentMethod = dto.PaymentMethod,
            CustomerId = quotation.CustomerId,
            CustomerName = quotation.Customer?.Name ?? quotation.CustomerName ?? "عميل عرض أسعار",
            PaidAmount = dto.PaidAmount,
            TaxRate = quotation.TaxRate,
            IsTaxInclusive = quotation.IsTaxInclusive,
            TotalDiscount = quotation.TotalDiscount,
            Payments = dto.Payments,
            Details = quotation.QuotationDetails.Select(d => new CreateSaleInvoiceDetailDto
            {
                MedicineId = d.MedicineId,
                SaleUnitId = d.SaleUnitId,
                Quantity = d.Quantity,
                SalePrice = d.UnitPrice,
                DiscountPercentage = d.DiscountPercentage,
                TaxRate = d.TaxRate
            }).ToList()
        };

        // استدعاء خدمة المبيعات الرسمية لتنفيذ كافة العمليات (خصم المستودع FEFO، السندات المحاسبية، ZATCA، وردية الكاشير)
        var createdInvoice = await _saleInvoiceService.CreateAsync(createSaleInvoiceDto, userId);

        // ربط عرض السعر بالفاتورة الناتجة وتحديث حالته
        quotation.Status = QuotationStatus.ConvertedToInvoice;
        quotation.ConvertedSaleInvoiceId = createdInvoice.Id;
        quotation.ConvertedAt = DateTime.UtcNow;
        quotation.ConvertedBy = userId;

        await _unitOfWork.Quotations.UpdateAsync(quotation);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("تم تحويل عرض السعر رقم {QuotationNumber} بنجاح إلى فاتورة مبيعات رقم {InvoiceNumber}",
            quotation.QuotationNumber, createdInvoice.SaleInvoiceNumber);

        return createdInvoice;
    }

    private static void CalculateQuotationFinancials(
        Quotation quotation,
        IEnumerable<CreateQuotationDetailDto> details,
        decimal headerDiscount,
        decimal headerTaxRate,
        bool isTaxInclusive)
    {
        quotation.QuotationDetails.Clear();
        decimal subtotalSum = 0;
        decimal taxSum = 0;
        decimal detailsDiscountSum = 0;

        foreach (var d in details)
        {
            var lineGross = d.Quantity * d.UnitPrice;
            var lineDiscount = d.DiscountAmount > 0
                ? d.DiscountAmount
                : (d.DiscountPercentage > 0 ? (lineGross * d.DiscountPercentage / 100m) : 0);

            var lineAfterDiscount = Math.Max(0, lineGross - lineDiscount);
            var effectiveTaxRate = d.TaxRate > 0 ? d.TaxRate : headerTaxRate;

            decimal lineTax;
            decimal lineSubtotal;
            decimal lineTotal;

            if (isTaxInclusive)
            {
                lineTotal = lineAfterDiscount;
                lineSubtotal = effectiveTaxRate > 0 ? (lineTotal / (1m + (effectiveTaxRate / 100m))) : lineTotal;
                lineTax = lineTotal - lineSubtotal;
            }
            else
            {
                lineSubtotal = lineAfterDiscount;
                lineTax = effectiveTaxRate > 0 ? (lineSubtotal * (effectiveTaxRate / 100m)) : 0;
                lineTotal = lineSubtotal + lineTax;
            }

            detailsDiscountSum += lineDiscount;
            subtotalSum += lineSubtotal;
            taxSum += lineTax;

            quotation.QuotationDetails.Add(new QuotationDetail
            {
                MedicineId = d.MedicineId,
                SaleUnitId = d.SaleUnitId,
                Quantity = d.Quantity,
                UnitPrice = d.UnitPrice,
                DiscountPercentage = d.DiscountPercentage,
                DiscountAmount = Math.Round(lineDiscount, 2),
                TaxRate = effectiveTaxRate,
                TaxAmount = Math.Round(lineTax, 2),
                Subtotal = Math.Round(lineSubtotal, 2),
                Total = Math.Round(lineTotal, 2),
                Notes = d.Notes,
                CreatedAt = DateTime.UtcNow
            });
        }

        quotation.IsTaxInclusive = isTaxInclusive;
        quotation.TaxRate = headerTaxRate;
        quotation.Subtotal = Math.Round(subtotalSum, 2);
        quotation.TaxAmount = Math.Round(taxSum, 2);
        quotation.TotalDiscount = Math.Round(headerDiscount + detailsDiscountSum, 2);

        if (isTaxInclusive)
        {
            quotation.TotalAmount = Math.Round(Math.Max(0, (subtotalSum + taxSum) - headerDiscount), 2);
        }
        else
        {
            var netSubtotal = Math.Max(0, subtotalSum - headerDiscount);
            var netTax = headerTaxRate > 0 ? (netSubtotal * (headerTaxRate / 100m)) : taxSum;
            quotation.TotalAmount = Math.Round(netSubtotal + netTax, 2);
        }
    }
}
