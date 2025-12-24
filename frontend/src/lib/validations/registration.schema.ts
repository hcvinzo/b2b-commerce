import { z } from 'zod'

// TC Kimlik No validation function
export function validateTcKimlikNo(tckn: string): boolean {
  if (!tckn || tckn.length !== 11 || tckn.startsWith('0')) {
    return false
  }

  // Check if all characters are digits
  if (!/^\d{11}$/.test(tckn)) {
    return false
  }

  const digits = tckn.split('').map(Number)

  const odds = digits[0] + digits[2] + digits[4] + digits[6] + digits[8]
  const evens = digits[1] + digits[3] + digits[5] + digits[7]

  // 10th digit check
  const digit10 = ((odds * 7) - evens) % 10
  if (digit10 < 0 ? digit10 + 10 : digit10 !== digits[9]) {
    return false
  }

  // 11th digit check
  const digit11 = (odds + evens + digits[9]) % 10
  if (digit11 !== digits[10]) {
    return false
  }

  return true
}

// Turkish IBAN validation function
export function validateIban(iban: string): boolean {
  if (!iban) return true // Optional field

  // Remove spaces and convert to uppercase
  const cleanIban = iban.replace(/\s/g, '').toUpperCase()

  // Turkish IBAN must be 26 characters and start with TR
  if (cleanIban.length !== 26 || !cleanIban.startsWith('TR')) {
    return false
  }

  // Check if remaining characters after TR are digits
  if (!/^TR\d{24}$/.test(cleanIban)) {
    return false
  }

  // IBAN checksum validation (mod 97)
  // Move first 4 characters to end
  const rearranged = cleanIban.slice(4) + cleanIban.slice(0, 4)

  // Convert letters to numbers (A=10, B=11, ..., Z=35)
  const numericString = rearranged.split('').map(char => {
    const code = char.charCodeAt(0)
    if (code >= 65 && code <= 90) {
      return (code - 55).toString()
    }
    return char
  }).join('')

  // Calculate mod 97 using string arithmetic (number is too large for JS)
  let remainder = 0
  for (const digit of numericString) {
    remainder = (remainder * 10 + parseInt(digit)) % 97
  }

  return remainder === 1
}

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
  tcNumber: z.string()
    .optional()
    .refine(
      (val) => !val || validateTcKimlikNo(val),
      { message: 'Geçersiz T.C. Kimlik Numarası' }
    ),
  sharePercentage: z.number().min(0).max(100).optional(),
})

// Generate year options from 1900 to current year
const currentYear = new Date().getFullYear()

export const step2Schema = z.object({
  companyTitle: z.string().min(1, 'Ünvan gereklidir'),
  taxOffice: z.string().min(1, 'Vergi dairesi gereklidir'),
  taxNumber: z.string().min(10, 'Vergi numarası en az 10 karakter olmalıdır'),
  foundedYear: z.number().min(1900, 'Geçerli bir yıl seçiniz').max(currentYear, `Kuruluş yılı ${currentYear} yılından büyük olamaz`).optional(),
  address: z.string().min(1, 'Adres gereklidir'),
  country: z.string().min(1, 'Ülke gereklidir'),
  phone: z.string().min(10, 'Geçerli bir telefon numarası giriniz'),
  website: z.string().url('Geçerli bir URL giriniz').optional().or(z.literal('')),
  authorizedPersons: z.array(authorizedPersonSchema).min(1, 'En az bir yetkili kişi girilmelidir'),
}).refine(
  (data) => {
    // Get all filled authorized persons (those with at least a name or TC number)
    const filledPersons = data.authorizedPersons.filter(p => p.fullName || p.tcNumber)
    if (filledPersons.length === 0) return true // No validation needed if no persons filled

    // Calculate total percentage
    const totalPercentage = data.authorizedPersons.reduce(
      (sum, person) => sum + (person.sharePercentage || 0),
      0
    )

    // Total must be exactly 100% if any persons are filled
    return totalPercentage === 100
  },
  {
    message: 'Yetkili/ortak pay oranları toplamı %100 olmalıdır',
    path: ['authorizedPersons'],
  }
).refine(
  (data) => {
    // Get all non-empty TC numbers
    const tcNumbers = data.authorizedPersons
      .map(p => p.tcNumber)
      .filter((tc): tc is string => !!tc && tc.length > 0)

    // Check for duplicates
    const uniqueTcNumbers = new Set(tcNumbers)
    return tcNumbers.length === uniqueTcNumbers.size
  },
  {
    message: 'Aynı T.C. Kimlik Numarası birden fazla kullanılamaz',
    path: ['authorizedPersons'],
  }
)

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
}).refine(
  (data) => {
    const total = data.customerBase.retailer +
      data.customerBase.corporate +
      data.customerBase.construction +
      data.customerBase.retail
    return total === 100
  },
  {
    message: 'Müşteri kitlesi oranları toplamı %100 olmalıdır',
    path: ['customerBase'],
  }
)

export type Step3FormData = z.infer<typeof step3Schema>

// Step 4: Banking & Documents Schema
export const bankAccountSchema = z.object({
  bankName: z.string().optional(),
  iban: z.string()
    .optional()
    .refine(
      (val) => !val || validateIban(val),
      { message: 'Geçersiz IBAN numarası. TR ile başlamalı ve 26 karakter olmalıdır.' }
    ),
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
