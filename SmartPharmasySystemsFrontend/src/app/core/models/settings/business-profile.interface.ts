export enum BusinessType {
  Pharmacy = 1,
  Supermarket = 2,
  Grocery = 3,
  Fashion = 4,
  Electronics = 5,
  BuildingMaterials = 6,
  AutoParts = 7,
  Wholesale = 8,
  Furniture = 9,
  GeneralTrade = 10
}

export enum InvoicePrintTemplate {
  Thermal80mm = 1,
  A4Formal = 2,
  A4Simple = 3
}

export interface BusinessProfile {
  id: number;
  businessType: BusinessType;
  businessTypeName: string;
  displayName: string;
  isActive: boolean;
  isCustom: boolean;

  // ─── إعدادات بطاقة الصنف ─────────────────────────
  trackExpiryDate: boolean;
  trackBatchNumber: boolean;
  trackSerialNumbers: boolean;
  useProductVariants: boolean;
  allowDecimalQuantity: boolean;
  useScaleBarcode: boolean;
  hasWarranty: boolean;
  hasAlternatives: boolean;
  trackDimensions: boolean;
  trackModelNumber: boolean;
  requireBarcode: boolean;
  requireCategory: boolean;

  // ─── إعدادات وسياسات المبيعات ────────────────────
  requireCustomer: boolean;
  allowSellBelowCost: boolean;
  requireShiftToSell: boolean;
  checkCustomerCreditLimit: boolean;
  preventCreditSaleWithoutCustomer: boolean;
  useFEFO: boolean;
  allowMultiPayment: boolean;
  allowHoldInvoice: boolean;
  useCashDrawer: boolean;
  defaultPrintTemplate: InvoicePrintTemplate;
  defaultPrintTemplateName: string;

  // ─── إعدادات المشتريات ───────────────────────────
  requirePurchaseOrder: boolean;
  useLandedCost: boolean;

  // ─── إعدادات الضريبة ─────────────────────────────
  enableVAT: boolean;
  defaultVATRate: number;
  scaleBarcodePrefix?: string;
}

export interface UpdateBusinessProfileDto {
  displayName: string;
  trackExpiryDate: boolean;
  trackBatchNumber: boolean;
  trackSerialNumbers: boolean;
  useProductVariants: boolean;
  allowDecimalQuantity: boolean;
  useScaleBarcode: boolean;
  hasWarranty: boolean;
  hasAlternatives: boolean;
  trackDimensions: boolean;
  trackModelNumber: boolean;
  requireBarcode: boolean;
  requireCategory: boolean;
  requireCustomer: boolean;
  allowSellBelowCost: boolean;
  requireShiftToSell: boolean;
  checkCustomerCreditLimit: boolean;
  preventCreditSaleWithoutCustomer: boolean;
  useFEFO: boolean;
  allowMultiPayment: boolean;
  allowHoldInvoice: boolean;
  useCashDrawer: boolean;
  defaultPrintTemplate: InvoicePrintTemplate;
  requirePurchaseOrder: boolean;
  useLandedCost: boolean;
  enableVAT: boolean;
  defaultVATRate: number;
  scaleBarcodePrefix?: string;
}

export interface BusinessTypeMeta {
  type: BusinessType;
  name: string;
  icon: string;
  color: string;
  description: string;
  keyFeatures: string[];
}

export const BUSINESS_TYPES_META: BusinessTypeMeta[] = [
  {
    type: BusinessType.Pharmacy,
    name: 'صيدلية ومستلزمات طبية',
    icon: 'pi pi-heart-fill',
    color: '#10b981',
    description: 'تتبع تاريخ الصلاحية، أرقام التشغيلات، صرف FEFO، والبدائل الدوائية',
    keyFeatures: ['صلاحية إلزامية', 'تشغيلة/دفعة', 'صرف FEFO', 'بدائل دوائية']
  },
  {
    type: BusinessType.Supermarket,
    name: 'سوبرماركت وهايبرماركت',
    icon: 'pi pi-shopping-cart',
    color: '#3b82f6',
    description: 'كميات عشرية بالميزان، باركود إلكتروني، تعليق الفاتورة، زبون طيار',
    keyFeatures: ['أوزان وكسور (كجم)', 'باركود الميزان', 'تعليق الفواتير (Hold)', 'كاشير سريع']
  },
  {
    type: BusinessType.Grocery,
    name: 'بقالة وميني ماركت',
    icon: 'pi pi-shopping-bag',
    color: '#06b6d4',
    description: 'كاشير سريع، مبيعات مباشرة، تتبع صلاحية مبسط وكسور أوزان',
    keyFeatures: ['تتبع صلاحية', 'كميات عشرية', 'كاشير سريع', 'درج نقود']
  },
  {
    type: BusinessType.Fashion,
    name: 'ملابس وأحذية وأقمشة',
    icon: 'pi pi-tag',
    color: '#ec4899',
    description: 'مصفوفة المقاسات والألوان (Variants)، إدارة الماركات والمواسم',
    keyFeatures: ['مصفوفة مقاسات × ألوان', 'مواسم وتصفيات', 'طباعة باركود مقاسات', 'ماركات تجارية']
  },
  {
    type: BusinessType.Electronics,
    name: 'أجهزة وإلكترونيات وجوالات',
    icon: 'pi pi-mobile',
    color: '#8b5cf6',
    description: 'تتبع السيريال والـ IMEI لكل جهاز، شهادات وضمانات البيع',
    keyFeatures: ['سيريال / IMEI', 'تتبع الضمان', 'رقم الموديل', 'فاتورة A4 رسمية']
  },
  {
    type: BusinessType.BuildingMaterials,
    name: 'مواد بناء وسيراميك وحديد',
    icon: 'pi pi-box',
    color: '#f59e0b',
    description: 'كميات عشرية (متر/طن/كيس)، أبعاد المنتجات، مصاريف شحن وتخليص',
    keyFeatures: ['كميات عشرية وأطوال', 'أبعاد وأحجام', 'تكاليف شحن Landed Cost', 'أمر توريد A4']
  },
  {
    type: BusinessType.AutoParts,
    name: 'قطع غيار سيارات ومعدات',
    icon: 'pi pi-cog',
    color: '#64748b',
    description: 'رقم القطعة OEM، السيارات المتوافقة، بدائل القطع وضمان الجودة',
    keyFeatures: ['رقم القطعة OEM', 'موديلات متوافقة', 'بدائل القطع', 'تتبع الضمان']
  },
  {
    type: BusinessType.Wholesale,
    name: 'تجارة جملة وتوزيع',
    icon: 'pi pi-truck',
    color: '#ea580c',
    description: 'أوامر الشراء PO، فحص سقف الائتمان، كميات مجانية (بونص)',
    keyFeatures: ['أوامر شراء إلزامية', 'سقف ائتمان العميل', 'كميات بونص', 'أسعار جملة متعددة']
  },
  {
    type: BusinessType.Furniture,
    name: 'أثاث ومفروشات وديكور',
    icon: 'pi pi-home',
    color: '#84cc16',
    description: 'أبعاد ومقاسات الغرف، خيارات الأقمشة والألوان، فترات الضمان والتسليم',
    keyFeatures: ['أبعاد ومقاسات', 'أقمشة وألوان', 'فترة ضمان', 'تسليم وشحن']
  },
  {
    type: BusinessType.GeneralTrade,
    name: 'تجارة عامة ومبيعات',
    icon: 'pi pi-briefcase',
    color: '#6366f1',
    description: 'نمط متوازن ومرن يناسب أي نشاط تجاري بيع وشراء وتوريد',
    keyFeatures: ['إعدادات مرنة', 'نقدي وآجل', 'دفع متعدد', 'طباعة حراري/A4']
  }
];
