import { z } from 'zod'

// Step 1: Contact Person Schema
export const step1Schema = z.object({
  firstName: z.string().min(1, 'Adı gereklidir'),
  lastName: z.string().min(1, 'Soyadı gereklidir'),
  email: z.string().min(1, 'E-posta adresi gereklidir').email('Geçerli bir e-posta adresi giriniz'),
  alternativeEmail: z.string().email('Geçerli bir e-posta adresi giriniz').optional().or(z.literal('')),
  position: z.string().min(1, 'Görevi gereklidir'),
  birthDate: z.string().optional(),
  gender: z.string().optional(),
  country: z.string().min(1, 'Ülke seçiniz'),
  city: z.string().optional(),
  district: z.string().optional(),
  workPhone: z.string().min(10, 'Geçerli bir telefon numarası giriniz'),
  extension: z.string().optional(),
  mobile: z.string().min(10, 'Geçerli bir telefon numarası giriniz'),
})

export type Step1FormData = z.infer<typeof step1Schema>

// Step 2: Business Information Schema
export const authorizedPersonSchema = z.object({
  fullName: z.string().optional(),
  tcNumber: z.string().optional(),
  sharePercentage: z.number().min(0).max(100).optional(),
})

export const step2Schema = z.object({
  companyTitle: z.string().min(1, 'Ünvan gereklidir'),
  taxOffice: z.string().min(1, 'Vergi dairesi gereklidir'),
  taxNumber: z.string().min(10, 'Vergi numarası en az 10 karakter olmalıdır'),
  foundedYear: z.number().min(1900).max(new Date().getFullYear()).optional(),
  address: z.string().min(1, 'Adres gereklidir'),
  city: z.string().min(1, 'Şehir gereklidir'),
  phone: z.string().min(10, 'Geçerli bir telefon numarası giriniz'),
  website: z.string().url('Geçerli bir URL giriniz').optional().or(z.literal('')),
  authorizedPersons: z.array(authorizedPersonSchema).min(1, 'En az bir yetkili kişi girilmelidir'),
})

export type Step2FormData = z.infer<typeof step2Schema>

// Step 3: Operational Details Schema
export const businessPartnerSchema = z.object({
  companyName: z.string().optional(),
  workingCondition: z.string().optional(),
  creditLimit: z.number().optional(),
})

export const step3Schema = z.object({
  employeeCount: z.string().min(1, 'Personel sayısı seçiniz'),
  businessStructure: z.string().min(1, 'İşletme yapısı seçiniz'),
  revenueYear: z.number().optional(),
  targetRevenue: z.number().optional(),
  customerBase: z.object({
    retailer: z.number().min(0).max(100),
    corporate: z.number().min(0).max(100),
    construction: z.number().min(0).max(100),
    retail: z.number().min(0).max(100),
  }),
  productCategories: z.array(z.string()).min(1, 'En az bir kategori seçiniz'),
  currentPartners: z.array(businessPartnerSchema).optional(),
  requestedConditions: z.array(z.string()).min(1, 'En az bir çalışma koşulu seçiniz'),
})

export type Step3FormData = z.infer<typeof step3Schema>

// Step 4: Banking & Documents Schema
export const bankAccountSchema = z.object({
  bankName: z.string().optional(),
  iban: z.string().optional(),
})

export const collateralSchema = z.object({
  type: z.string().optional(),
  amount: z.number().optional(),
  currency: z.enum(['TRY', 'USD', 'EUR']).optional(),
})

export const step4Schema = z.object({
  bankAccounts: z.array(bankAccountSchema).min(1, 'En az bir banka hesabı girilmelidir'),
  collaterals: z.array(collateralSchema).optional(),
  documents: z.object({
    taxCertificate: z.any().optional(),
    signatureCircular: z.any().optional(),
    tradeRegistry: z.any().optional(),
    partnershipAgreement: z.any().optional(),
    authorizedIdCopy: z.any().optional(),
    authorizedResidenceDocument: z.any().optional(),
  }),
})

export type Step4FormData = z.infer<typeof step4Schema>
