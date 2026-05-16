export interface PharmacySettings {
  id: number;
  pharmacyName: string;
  logoUrl?: string;
  address?: string;
  phoneNumber?: string;
  mobileNumber?: string;
  email?: string;
  taxNumber?: string;
  commercialRegister?: string;
  healthMinistryLicense?: string;
  website?: string;
  baseCurrency: string;
  invoiceWelcomeMessage?: string;
}

export interface UpdatePharmacySettingsDto {
  pharmacyName: string;
  address?: string;
  phoneNumber?: string;
  mobileNumber?: string;
  email?: string;
  taxNumber?: string;
  commercialRegister?: string;
  healthMinistryLicense?: string;
  website?: string;
  baseCurrency: string;
  invoiceWelcomeMessage?: string;
}
