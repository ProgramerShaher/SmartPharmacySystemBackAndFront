import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, of, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  BusinessProfile,
  BusinessType,
  BUSINESS_TYPES_META,
  BusinessTypeMeta,
  UpdateBusinessProfileDto
} from '../models/settings/business-profile.interface';

@Injectable({
  providedIn: 'root'
})
export class BusinessProfileService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/BusinessProfile`;

  // ── Reactive Store ──────────────────────────────────────────────────────────
  private profileSubject = new BehaviorSubject<BusinessProfile | null>(null);
  readonly profile$ = this.profileSubject.asObservable();

  /**
   * القيمة اللحظية لملف النشاط الحالي (Snapshot)
   */
  get currentProfile(): BusinessProfile | null {
    return this.profileSubject.value;
  }

  // ── Quick Helpers / Selectors for UI Components ───────────────────────────
  get isPharmacy(): boolean {
    return (this.currentProfile?.businessType ?? BusinessType.Pharmacy) === BusinessType.Pharmacy;
  }

  get isSupermarket(): boolean {
    return this.currentProfile?.businessType === BusinessType.Supermarket;
  }

  get isGrocery(): boolean {
    return this.currentProfile?.businessType === BusinessType.Grocery;
  }

  get isFashion(): boolean {
    return this.currentProfile?.businessType === BusinessType.Fashion;
  }

  get isElectronics(): boolean {
    return this.currentProfile?.businessType === BusinessType.Electronics;
  }

  get isBuildingMaterials(): boolean {
    return this.currentProfile?.businessType === BusinessType.BuildingMaterials;
  }

  get isAutoParts(): boolean {
    return this.currentProfile?.businessType === BusinessType.AutoParts;
  }

  get isWholesale(): boolean {
    return this.currentProfile?.businessType === BusinessType.Wholesale;
  }

  get isFurniture(): boolean {
    return this.currentProfile?.businessType === BusinessType.Furniture;
  }

  // ── Feature Flag Selectors ────────────────────────────────────────────────
  get trackExpiryDate(): boolean {
    return this.currentProfile?.trackExpiryDate ?? true;
  }

  get trackBatchNumber(): boolean {
    return this.currentProfile?.trackBatchNumber ?? true;
  }

  get trackSerialNumbers(): boolean {
    return this.currentProfile?.trackSerialNumbers ?? false;
  }

  get useProductVariants(): boolean {
    return this.currentProfile?.useProductVariants ?? false;
  }

  get allowDecimalQuantity(): boolean {
    return this.currentProfile?.allowDecimalQuantity ?? false;
  }

  get useScaleBarcode(): boolean {
    return this.currentProfile?.useScaleBarcode ?? false;
  }

  get scaleBarcodePrefix(): string {
    return this.currentProfile?.scaleBarcodePrefix ?? '20';
  }

  get hasWarranty(): boolean {
    return this.currentProfile?.hasWarranty ?? false;
  }

  get hasAlternatives(): boolean {
    return this.currentProfile?.hasAlternatives ?? true;
  }

  get requireCustomer(): boolean {
    return this.currentProfile?.requireCustomer ?? false;
  }

  get allowHoldInvoice(): boolean {
    return this.currentProfile?.allowHoldInvoice ?? true;
  }

  get useFEFO(): boolean {
    return this.currentProfile?.useFEFO ?? true;
  }

  get allowMultiPayment(): boolean {
    return this.currentProfile?.allowMultiPayment ?? true;
  }

  get useLandedCost(): boolean {
    return this.currentProfile?.useLandedCost ?? false;
  }

  get enableVAT(): boolean {
    return this.currentProfile?.enableVAT ?? false;
  }

  get defaultVATRate(): number {
    return this.currentProfile?.defaultVATRate ?? 0;
  }

  // get useLandedCost(): boolean {
  //   return this.currentProfile?.useLandedCost ?? false;
  // }

  // ── API Operations ─────────────────────────────────────────────────────────

  /**
   * تحميل ملف النشاط التجاري الفعال وتخزينه في الـ Store
   */
  loadActiveProfile(forceRefresh = false): Observable<BusinessProfile> {
    const cached = this.profileSubject.value;
    if (!forceRefresh && cached) {
      return of(cached);
    }

    return this.http.get<BusinessProfile>(`${this.apiUrl}/current`).pipe(
      tap((profile) => this.profileSubject.next(profile))
    );
  }

  /**
   * تبديل النشاط التجاري للنظام وتطبيق قالبه المعتمد فوراً
   */
  switchBusinessType(type: BusinessType): Observable<BusinessProfile> {
    return this.http.post<BusinessProfile>(`${this.apiUrl}/switch/${type}`, {}).pipe(
      tap((profile) => this.profileSubject.next(profile))
    );
  }

  /**
   * تعديل وتخصيص إعدادات وسياسات النشاط الحالي
   */
  updateCustomProfile(dto: UpdateBusinessProfileDto): Observable<BusinessProfile> {
    return this.http.put<BusinessProfile>(`${this.apiUrl}/current`, dto).pipe(
      tap((profile) => this.profileSubject.next(profile))
    );
  }

  /**
   * جلب القوالب المسبقة لجميع الأنشطة الـ 10 للمعاينة
   */
  getPresets(): Observable<BusinessProfile[]> {
    return this.http.get<BusinessProfile[]>(`${this.apiUrl}/presets`);
  }

  /**
   * جلب معلومات وتفاصيل وهوية نشاط معين (أيقونة، وصف، لون، ميزات)
   */
  getMetaForType(type: BusinessType): BusinessTypeMeta | undefined {
    return BUSINESS_TYPES_META.find((m) => m.type === type);
  }

  /**
   * قائمة جميع بيانات الأنشطة التعريفية
   */
  getAllMeta(): BusinessTypeMeta[] {
    return BUSINESS_TYPES_META;
  }
}
