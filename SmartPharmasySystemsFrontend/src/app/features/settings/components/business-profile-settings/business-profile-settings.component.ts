import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { ButtonModule } from 'primeng/button';
import { InputSwitchModule } from 'primeng/inputswitch';
import { TooltipModule } from 'primeng/tooltip';
import { DialogModule } from 'primeng/dialog';
import { DropdownModule } from 'primeng/dropdown';
import { InputTextModule } from 'primeng/inputtext';
import { TagModule } from 'primeng/tag';
import {
  BusinessProfile,
  BusinessType,
  BusinessTypeMeta,
  InvoicePrintTemplate,
  UpdateBusinessProfileDto
} from '../../../../core/models/settings/business-profile.interface';
import { BusinessProfileService } from '../../../../core/services/business-profile.service';

@Component({
  selector: 'app-business-profile-settings',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    ToastModule,
    ButtonModule,
    InputSwitchModule,
    TooltipModule,
    DialogModule,
    DropdownModule,
    InputTextModule,
    TagModule
  ],
  providers: [MessageService],
  templateUrl: './business-profile-settings.component.html',
  styleUrls: ['./business-profile-settings.component.scss']
})
export class BusinessProfileSettingsComponent implements OnInit {
  private profileService = inject(BusinessProfileService);
  private messageService = inject(MessageService);
  private fb = inject(FormBuilder);

  isLoading = true;
  isSaving = false;
  isSwitching = false;

  currentProfile: BusinessProfile | null = null;
  selectedPreviewType: BusinessType = BusinessType.Pharmacy;
  previewProfile: BusinessProfile | null = null;
  allPresets: BusinessProfile[] = [];
  activitiesMeta: BusinessTypeMeta[] = [];

  // Dialog for switching confirmation
  showSwitchDialog = false;
  targetSwitchType: BusinessType | null = null;
  targetSwitchMeta: BusinessTypeMeta | null = null;

  // Form for custom profile editing
  profileForm!: FormGroup;

  printTemplates = [
    { label: 'طابعة فواتير حرارية 80 مم (Thermal)', value: InvoicePrintTemplate.Thermal80mm },
    { label: 'نموذج فاتورة رسمي A4 (Formal A4)', value: InvoicePrintTemplate.A4Formal },
    { label: 'نموذج فاتورة مبسط A4 (Simple A4)', value: InvoicePrintTemplate.A4Simple }
  ];

  ngOnInit(): void {
    this.initForm();
    this.activitiesMeta = this.profileService.getAllMeta();
    this.loadData();
  }

  private initForm(): void {
    this.profileForm = this.fb.group({
      displayName: [''],
      // بطاقة الصنف
      trackExpiryDate: [true],
      trackBatchNumber: [true],
      trackSerialNumbers: [false],
      useProductVariants: [false],
      allowDecimalQuantity: [false],
      useScaleBarcode: [false],
      hasWarranty: [false],
      hasAlternatives: [true],
      trackDimensions: [false],
      trackModelNumber: [false],
      requireBarcode: [true],
      requireCategory: [true],
      // المبيعات والسياسات
      requireCustomer: [false],
      allowSellBelowCost: [false],
      requireShiftToSell: [true],
      checkCustomerCreditLimit: [true],
      preventCreditSaleWithoutCustomer: [true],
      useFEFO: [true],
      allowMultiPayment: [true],
      allowHoldInvoice: [true],
      useCashDrawer: [true],
      defaultPrintTemplate: [InvoicePrintTemplate.Thermal80mm],
      // المشتريات
      requirePurchaseOrder: [false],
      useLandedCost: [false],
      // الضريبة
      enableVAT: [false],
      defaultVATRate: [0]
    });
  }

  loadData(): void {
    this.isLoading = true;

    // Load active profile and all presets
    this.profileService.loadActiveProfile(true).subscribe({
      next: (profile) => {
        this.currentProfile = profile;
        this.selectedPreviewType = profile.businessType;
        this.populateForm(profile);

        // Load all presets for side-by-side comparison
        this.profileService.getPresets().subscribe({
          next: (presets) => {
            this.allPresets = presets;
            this.previewProfile = presets.find(p => p.businessType === profile.businessType) || profile;
            this.isLoading = false;
          },
          error: () => {
            this.isLoading = false;
          }
        });
      },
      error: (err) => {
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل تحميل ملف النشاط التجاري من الخادم'
        });
        this.isLoading = false;
      }
    });
  }

  private populateForm(profile: BusinessProfile): void {
    this.profileForm.patchValue({
      displayName: profile.displayName,
      trackExpiryDate: profile.trackExpiryDate,
      trackBatchNumber: profile.trackBatchNumber,
      trackSerialNumbers: profile.trackSerialNumbers,
      useProductVariants: profile.useProductVariants,
      allowDecimalQuantity: profile.allowDecimalQuantity,
      useScaleBarcode: profile.useScaleBarcode,
      hasWarranty: profile.hasWarranty,
      hasAlternatives: profile.hasAlternatives,
      trackDimensions: profile.trackDimensions,
      trackModelNumber: profile.trackModelNumber,
      requireBarcode: profile.requireBarcode,
      requireCategory: profile.requireCategory,
      requireCustomer: profile.requireCustomer,
      allowSellBelowCost: profile.allowSellBelowCost,
      requireShiftToSell: profile.requireShiftToSell,
      checkCustomerCreditLimit: profile.checkCustomerCreditLimit,
      preventCreditSaleWithoutCustomer: profile.preventCreditSaleWithoutCustomer,
      useFEFO: profile.useFEFO,
      allowMultiPayment: profile.allowMultiPayment,
      allowHoldInvoice: profile.allowHoldInvoice,
      useCashDrawer: profile.useCashDrawer,
      defaultPrintTemplate: profile.defaultPrintTemplate,
      requirePurchaseOrder: profile.requirePurchaseOrder,
      useLandedCost: profile.useLandedCost,
      enableVAT: profile.enableVAT,
      defaultVATRate: profile.defaultVATRate
    });
  }

  selectActivity(type: BusinessType): void {
    this.selectedPreviewType = type;
    const preset = this.allPresets.find(p => p.businessType === type);
    if (preset) {
      this.previewProfile = preset;
    }
  }

  promptSwitch(meta: BusinessTypeMeta): void {
    if (this.currentProfile?.businessType === meta.type) {
      this.messageService.add({
        severity: 'info',
        summary: 'تنبيه',
        detail: `نشاط "${meta.name}" هو النشاط الفعال حالياً في النظام`
      });
      return;
    }

    this.targetSwitchType = meta.type;
    this.targetSwitchMeta = meta;
    this.showSwitchDialog = true;
  }

  confirmSwitch(): void {
    if (!this.targetSwitchType) return;

    this.isSwitching = true;
    this.profileService.switchBusinessType(this.targetSwitchType).subscribe({
      next: (updatedProfile) => {
        this.currentProfile = updatedProfile;
        this.selectedPreviewType = updatedProfile.businessType;
        this.populateForm(updatedProfile);
        this.showSwitchDialog = false;
        this.isSwitching = false;

        this.messageService.add({
          severity: 'success',
          summary: 'تم بنجاح',
          detail: `تم تبديل نشاط المنشأة إلى "${updatedProfile.displayName}" وتطبيق كافة سياساته وقواعده تلقائياً`
        });
      },
      error: () => {
        this.isSwitching = false;
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'حدث خطأ أثناء تبديل النشاط التجاري'
        });
      }
    });
  }

  saveCustomProfile(): void {
    if (this.profileForm.invalid) return;

    this.isSaving = true;
    const dto: UpdateBusinessProfileDto = this.profileForm.value;

    this.profileService.updateCustomProfile(dto).subscribe({
      next: (updated) => {
        this.currentProfile = updated;
        this.isSaving = false;
        this.messageService.add({
          severity: 'success',
          summary: 'تم الحفظ',
          detail: 'تم حفظ وتخصيص إعدادات وسياسات النشاط بنجاح'
        });
      },
      error: () => {
        this.isSaving = false;
        this.messageService.add({
          severity: 'error',
          summary: 'خطأ',
          detail: 'فشل حفظ الإعدادات المخصصة'
        });
      }
    });
  }

  resetToPreset(): void {
    if (!this.currentProfile) return;
    const preset = this.allPresets.find(p => p.businessType === this.currentProfile!.businessType);
    if (preset) {
      this.populateForm(preset);
      this.messageService.add({
        severity: 'info',
        summary: 'إعادة ضبط',
        detail: 'تمت استعادة الإعدادات النموذجية الافتراضية للنشاط'
      });
    }
  }

  getActiveMeta(): BusinessTypeMeta | undefined {
    if (!this.currentProfile) return undefined;
    return this.profileService.getMetaForType(this.currentProfile.businessType);
  }

  isCurrentActive(type: BusinessType): boolean {
    return this.currentProfile?.businessType === type;
  }
}
