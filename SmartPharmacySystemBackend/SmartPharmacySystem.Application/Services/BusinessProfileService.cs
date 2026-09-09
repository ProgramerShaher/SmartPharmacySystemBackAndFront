using AutoMapper;
using SmartPharmacySystem.Application.DTOs.Settings;
using SmartPharmacySystem.Application.Interfaces;
using SmartPharmacySystem.Core.Entities;
using SmartPharmacySystem.Core.Enums;
using SmartPharmacySystem.Core.Interfaces;

namespace SmartPharmacySystem.Application.Services;

/// <summary>
/// خدمة إدارة ملفات الأنماط والأنشطة التجارية
/// </summary>
public class BusinessProfileService : IBusinessProfileService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public BusinessProfileService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<BusinessProfileDto> GetActiveProfileAsync()
    {
        var activeProfile = await _unitOfWork.BusinessProfiles.GetActiveProfileAsync();

        if (activeProfile == null)
        {
            // فحص إعدادات الصيدلية لمعرفة النشاط المحدد مسبقاً، والافتراضي صيدلية
            var settings = await _unitOfWork.PharmacySettings.GetSettingsAsync();
            var businessType = settings?.BusinessType ?? BusinessType.Pharmacy;

            activeProfile = CreatePresetProfileEntity(businessType);
            activeProfile.IsActive = true;

            await _unitOfWork.BusinessProfiles.AddAsync(activeProfile);
            await _unitOfWork.SaveChangesAsync();

            // ربط الإعدادات بالملف المنشأ
            if (settings != null)
            {
                settings.BusinessProfileId = activeProfile.Id;
                settings.BusinessType = businessType;
                await _unitOfWork.PharmacySettings.UpdateAsync(settings);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        return _mapper.Map<BusinessProfileDto>(activeProfile);
    }

    public async Task<BusinessProfileDto> SwitchBusinessTypeAsync(BusinessType newType)
    {
        // 1. استرجاع جميع الملفات لإلغاء تنشيطها
        var allProfiles = await _unitOfWork.BusinessProfiles.GetAllProfilesAsync();
        foreach (var p in allProfiles)
        {
            p.IsActive = false;
            await _unitOfWork.BusinessProfiles.UpdateAsync(p);
        }

        // 2. البحث عن ملف موجود لنفس النشاط، أو إنشاء ملف جديد من القالب
        var targetProfile = await _unitOfWork.BusinessProfiles.GetByTypeAsync(newType);
        if (targetProfile == null)
        {
            targetProfile = CreatePresetProfileEntity(newType);
            targetProfile.IsActive = true;
            await _unitOfWork.BusinessProfiles.AddAsync(targetProfile);
        }
        else
        {
            targetProfile.IsActive = true;
            await _unitOfWork.BusinessProfiles.UpdateAsync(targetProfile);
        }

        await _unitOfWork.SaveChangesAsync();

        // 3. تحديث الإعدادات العامة للمنشأة
        var settings = await _unitOfWork.PharmacySettings.GetSettingsAsync();
        if (settings != null)
        {
            settings.BusinessType = newType;
            settings.BusinessProfileId = targetProfile.Id;
            await _unitOfWork.PharmacySettings.UpdateAsync(settings);
            await _unitOfWork.SaveChangesAsync();
        }

        return _mapper.Map<BusinessProfileDto>(targetProfile);
    }

    public async Task<BusinessProfileDto> UpdateCustomProfileAsync(UpdateBusinessProfileDto dto)
    {
        var activeProfile = await _unitOfWork.BusinessProfiles.GetActiveProfileAsync();
        if (activeProfile == null)
        {
            activeProfile = CreatePresetProfileEntity(BusinessType.Pharmacy);
            await _unitOfWork.BusinessProfiles.AddAsync(activeProfile);
        }

        _mapper.Map(dto, activeProfile);
        activeProfile.IsCustom = true;
        activeProfile.IsActive = true;

        await _unitOfWork.BusinessProfiles.UpdateAsync(activeProfile);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<BusinessProfileDto>(activeProfile);
    }

    public IEnumerable<BusinessProfileDto> GetAllPresetProfiles()
    {
        var types = Enum.GetValues<BusinessType>();
        var list = new List<BusinessProfileDto>();

        foreach (var type in types)
        {
            var entity = CreatePresetProfileEntity(type);
            list.Add(_mapper.Map<BusinessProfileDto>(entity));
        }

        return list;
    }

    public BusinessProfileDto GetPresetForType(BusinessType type)
    {
        var entity = CreatePresetProfileEntity(type);
        return _mapper.Map<BusinessProfileDto>(entity);
    }

    /// <summary>
    /// مُولّد القوالب الافتراضية لكل نشاط تجاري بناءً على مصفوفة المتطلبات المعتمدة في PROJECT_MASTER.md
    /// </summary>
    public static BusinessProfile CreatePresetProfileEntity(BusinessType type)
    {
        return type switch
        {
            BusinessType.Pharmacy => new BusinessProfile
            {
                BusinessType = BusinessType.Pharmacy,
                DisplayName = "صيدلية ومستلزمات طبية",
                TrackExpiryDate = true,
                TrackBatchNumber = true,
                TrackSerialNumbers = false,
                UseProductVariants = false,
                AllowDecimalQuantity = false,
                UseScaleBarcode = false,
                HasWarranty = false,
                HasAlternatives = true,
                TrackDimensions = false,
                TrackModelNumber = false,
                RequireBarcode = true,
                RequireCategory = true,
                RequireCustomer = false,
                AllowSellBelowCost = false,
                RequireShiftToSell = true,
                CheckCustomerCreditLimit = true,
                PreventCreditSaleWithoutCustomer = true,
                UseFEFO = true,
                AllowMultiPayment = true,
                AllowHoldInvoice = true,
                UseCashDrawer = true,
                DefaultPrintTemplate = InvoicePrintTemplate.Thermal80mm,
                RequirePurchaseOrder = false,
                UseLandedCost = false,
                EnableVAT = false,
                DefaultVATRate = 0m
            },

            BusinessType.Supermarket => new BusinessProfile
            {
                BusinessType = BusinessType.Supermarket,
                DisplayName = "سوبرماركت وهايبرماركت",
                TrackExpiryDate = true,
                TrackBatchNumber = true,
                TrackSerialNumbers = false,
                UseProductVariants = false,
                AllowDecimalQuantity = true,
                UseScaleBarcode = true,
                HasWarranty = false,
                HasAlternatives = false,
                TrackDimensions = false,
                TrackModelNumber = false,
                RequireBarcode = true,
                RequireCategory = true,
                RequireCustomer = false,
                AllowSellBelowCost = false,
                RequireShiftToSell = true,
                CheckCustomerCreditLimit = false,
                PreventCreditSaleWithoutCustomer = false,
                UseFEFO = true,
                AllowMultiPayment = true,
                AllowHoldInvoice = true,
                UseCashDrawer = true,
                DefaultPrintTemplate = InvoicePrintTemplate.Thermal80mm,
                RequirePurchaseOrder = false,
                UseLandedCost = false,
                EnableVAT = false,
                DefaultVATRate = 0m
            },

            BusinessType.Grocery => new BusinessProfile
            {
                BusinessType = BusinessType.Grocery,
                DisplayName = "بقالة وميني ماركت",
                TrackExpiryDate = true,
                TrackBatchNumber = false,
                TrackSerialNumbers = false,
                UseProductVariants = false,
                AllowDecimalQuantity = true,
                UseScaleBarcode = true,
                HasWarranty = false,
                HasAlternatives = false,
                TrackDimensions = false,
                TrackModelNumber = false,
                RequireBarcode = true,
                RequireCategory = true,
                RequireCustomer = false,
                AllowSellBelowCost = false,
                RequireShiftToSell = true,
                CheckCustomerCreditLimit = false,
                PreventCreditSaleWithoutCustomer = false,
                UseFEFO = true,
                AllowMultiPayment = true,
                AllowHoldInvoice = true,
                UseCashDrawer = true,
                DefaultPrintTemplate = InvoicePrintTemplate.Thermal80mm,
                RequirePurchaseOrder = false,
                UseLandedCost = false,
                EnableVAT = false,
                DefaultVATRate = 0m
            },

            BusinessType.Fashion => new BusinessProfile
            {
                BusinessType = BusinessType.Fashion,
                DisplayName = "ملابس وأحذية وأقمشة",
                TrackExpiryDate = false,
                TrackBatchNumber = false,
                TrackSerialNumbers = false,
                UseProductVariants = true,
                AllowDecimalQuantity = false,
                UseScaleBarcode = false,
                HasWarranty = false,
                HasAlternatives = false,
                TrackDimensions = false,
                TrackModelNumber = false,
                RequireBarcode = true,
                RequireCategory = true,
                RequireCustomer = false,
                AllowSellBelowCost = false,
                RequireShiftToSell = true,
                CheckCustomerCreditLimit = true,
                PreventCreditSaleWithoutCustomer = true,
                UseFEFO = false,
                AllowMultiPayment = true,
                AllowHoldInvoice = true,
                UseCashDrawer = true,
                DefaultPrintTemplate = InvoicePrintTemplate.Thermal80mm,
                RequirePurchaseOrder = false,
                UseLandedCost = false,
                EnableVAT = false,
                DefaultVATRate = 0m
            },

            BusinessType.Electronics => new BusinessProfile
            {
                BusinessType = BusinessType.Electronics,
                DisplayName = "أجهزة كهربائية وإلكترونيات وجوالات",
                TrackExpiryDate = false,
                TrackBatchNumber = false,
                TrackSerialNumbers = true,
                UseProductVariants = false,
                AllowDecimalQuantity = false,
                UseScaleBarcode = false,
                HasWarranty = true,
                HasAlternatives = false,
                TrackDimensions = false,
                TrackModelNumber = true,
                RequireBarcode = true,
                RequireCategory = true,
                RequireCustomer = true,
                AllowSellBelowCost = false,
                RequireShiftToSell = false,
                CheckCustomerCreditLimit = true,
                PreventCreditSaleWithoutCustomer = true,
                UseFEFO = false,
                AllowMultiPayment = true,
                AllowHoldInvoice = false,
                UseCashDrawer = false,
                DefaultPrintTemplate = InvoicePrintTemplate.A4Formal,
                RequirePurchaseOrder = true,
                UseLandedCost = true,
                EnableVAT = true,
                DefaultVATRate = 15m
            },

            BusinessType.BuildingMaterials => new BusinessProfile
            {
                BusinessType = BusinessType.BuildingMaterials,
                DisplayName = "مواد بناء وسيراميك وحديد ودهانات",
                TrackExpiryDate = false,
                TrackBatchNumber = false,
                TrackSerialNumbers = false,
                UseProductVariants = false,
                AllowDecimalQuantity = true,
                UseScaleBarcode = false,
                HasWarranty = false,
                HasAlternatives = false,
                TrackDimensions = true,
                TrackModelNumber = false,
                RequireBarcode = false,
                RequireCategory = true,
                RequireCustomer = true,
                AllowSellBelowCost = false,
                RequireShiftToSell = false,
                CheckCustomerCreditLimit = true,
                PreventCreditSaleWithoutCustomer = true,
                UseFEFO = false,
                AllowMultiPayment = true,
                AllowHoldInvoice = false,
                UseCashDrawer = false,
                DefaultPrintTemplate = InvoicePrintTemplate.A4Formal,
                RequirePurchaseOrder = true,
                UseLandedCost = true,
                EnableVAT = true,
                DefaultVATRate = 15m
            },

            BusinessType.AutoParts => new BusinessProfile
            {
                BusinessType = BusinessType.AutoParts,
                DisplayName = "قطع غيار سيارات ومعدات",
                TrackExpiryDate = false,
                TrackBatchNumber = false,
                TrackSerialNumbers = false,
                UseProductVariants = false,
                AllowDecimalQuantity = false,
                UseScaleBarcode = false,
                HasWarranty = true,
                HasAlternatives = true,
                TrackDimensions = false,
                TrackModelNumber = true,
                RequireBarcode = false,
                RequireCategory = true,
                RequireCustomer = true,
                AllowSellBelowCost = false,
                RequireShiftToSell = false,
                CheckCustomerCreditLimit = true,
                PreventCreditSaleWithoutCustomer = true,
                UseFEFO = false,
                AllowMultiPayment = true,
                AllowHoldInvoice = false,
                UseCashDrawer = false,
                DefaultPrintTemplate = InvoicePrintTemplate.A4Formal,
                RequirePurchaseOrder = true,
                UseLandedCost = true,
                EnableVAT = true,
                DefaultVATRate = 15m
            },

            BusinessType.Wholesale => new BusinessProfile
            {
                BusinessType = BusinessType.Wholesale,
                DisplayName = "تجارة جملة وتوزيع",
                TrackExpiryDate = true,
                TrackBatchNumber = true,
                TrackSerialNumbers = false,
                UseProductVariants = false,
                AllowDecimalQuantity = true,
                UseScaleBarcode = false,
                HasWarranty = false,
                HasAlternatives = false,
                TrackDimensions = false,
                TrackModelNumber = false,
                RequireBarcode = false,
                RequireCategory = true,
                RequireCustomer = true,
                AllowSellBelowCost = false,
                RequireShiftToSell = false,
                CheckCustomerCreditLimit = true,
                PreventCreditSaleWithoutCustomer = true,
                UseFEFO = false,
                AllowMultiPayment = true,
                AllowHoldInvoice = false,
                UseCashDrawer = false,
                DefaultPrintTemplate = InvoicePrintTemplate.A4Formal,
                RequirePurchaseOrder = true,
                UseLandedCost = true,
                EnableVAT = false,
                DefaultVATRate = 0m
            },

            BusinessType.Furniture => new BusinessProfile
            {
                BusinessType = BusinessType.Furniture,
                DisplayName = "أثاث ومفروشات وديكور",
                TrackExpiryDate = false,
                TrackBatchNumber = false,
                TrackSerialNumbers = false,
                UseProductVariants = true,
                AllowDecimalQuantity = false,
                UseScaleBarcode = false,
                HasWarranty = true,
                HasAlternatives = false,
                TrackDimensions = true,
                TrackModelNumber = true,
                RequireBarcode = false,
                RequireCategory = true,
                RequireCustomer = true,
                AllowSellBelowCost = false,
                RequireShiftToSell = false,
                CheckCustomerCreditLimit = true,
                PreventCreditSaleWithoutCustomer = true,
                UseFEFO = false,
                AllowMultiPayment = true,
                AllowHoldInvoice = false,
                UseCashDrawer = false,
                DefaultPrintTemplate = InvoicePrintTemplate.A4Formal,
                RequirePurchaseOrder = true,
                UseLandedCost = false,
                EnableVAT = false,
                DefaultVATRate = 0m
            },

            _ => new BusinessProfile
            {
                BusinessType = BusinessType.GeneralTrade,
                DisplayName = "تجارة عامة ومبيعات",
                TrackExpiryDate = false,
                TrackBatchNumber = false,
                TrackSerialNumbers = false,
                UseProductVariants = false,
                AllowDecimalQuantity = true,
                UseScaleBarcode = false,
                HasWarranty = false,
                HasAlternatives = false,
                TrackDimensions = false,
                TrackModelNumber = false,
                RequireBarcode = false,
                RequireCategory = true,
                RequireCustomer = false,
                AllowSellBelowCost = false,
                RequireShiftToSell = true,
                CheckCustomerCreditLimit = true,
                PreventCreditSaleWithoutCustomer = true,
                UseFEFO = false,
                AllowMultiPayment = true,
                AllowHoldInvoice = true,
                UseCashDrawer = true,
                DefaultPrintTemplate = InvoicePrintTemplate.Thermal80mm,
                RequirePurchaseOrder = false,
                UseLandedCost = false,
                EnableVAT = false,
                DefaultVATRate = 0m
            }
        };
    }
}
