// Contact Person (Step 1) - Maps to CustomerContact
export interface ContactPerson {
  firstName: string // CustomerContact.FirstName
  lastName: string // CustomerContact.LastName
  email: string // CustomerContact.Email
  emailConfirmation: string // For validation - must match email
  position?: string // CustomerContact.Position
  dateOfBirth?: string // CustomerContact.DateOfBirth
  gender?: string // CustomerContact.Gender (Male/Female)
  phone?: string // CustomerContact.Phone (İş Telefonu)
  phoneExt?: string // CustomerContact.PhoneExt (Dahili Numara)
  gsm: string // CustomerContact.Gsm (Mobil)
}

// Authorized Person / Shareholder (Yetkililer & Ortaklar)
export interface AuthorizedPerson {
  ad_soyad?: string // Full name
  kimlik_no?: string // TC Kimlik No (11 digits)
  ortaklik_payi?: number // Share percentage (0-100)
}

// Business Information (Step 2)
export interface BusinessInfo {
  // İşletme Bilgileri section
  title: string // Customer.Title (Ünvan)
  taxOffice?: string // Customer.TaxOffice (Vergi Dairesi)
  taxNo?: string // Customer.TaxNo (Vergi Numarası)
  establishmentYear?: number // Customer.EstablishmentYear (Kuruluş Yılı)
  website?: string // Customer.Website (İnternet Sayfası)
  // İletişim section - CustomerAddress
  address: string // CustomerAddress.Address
  geoLocationId?: string // CustomerAddress.GeoLocationId
  geoLocationPathName?: string // For display (e.g., "Türkiye/İstanbul/Kadıköy")
  postalCode?: string // CustomerAddress.PostalCode (Posta Kodu)
  addressPhone?: string // CustomerAddress.Phone (Telefon)
  addressPhoneExt?: string // CustomerAddress.PhoneExt (Dahili)
  addressGsm?: string // CustomerAddress.Gsm (Mobil)
  // Yetkililer & Ortaklar - CustomerAttributes
  authorizedPersons: AuthorizedPerson[]
}

// Composite Attribute Value (generic for dynamic attributes)
export interface CompositeAttributeValue {
  [key: string]: string | number | undefined
}

// Operational Details (Step 3) - Using dynamic attributes from API
export interface OperationalDetails {
  // Single select attributes
  calisanSayisi: string // calisan_sayisi attribute
  isletmeYapisi: string // isletme_yapisi attribute
  // Composite attributes (single value)
  ciro?: CompositeAttributeValue[] // ciro composite attribute
  musteriKitlesi?: CompositeAttributeValue[] // musteri_kitlesi composite attribute
  // Composite attributes (multi value)
  isOrtaklari?: CompositeAttributeValue[] // is_ortaklari composite attribute
  // Multi select attributes
  satilanUrunKategorileri: string[] // satilan_urun_kategorileri attribute
  calismaKosullari: string[] // calisma_kosullari attribute
}

// Bank Account
export interface BankAccount {
  bankName?: string
  iban?: string
}

// Collateral
export interface Collateral {
  type?: string
  amount?: number
  currency?: 'TRY' | 'USD' | 'EUR'
}

// Banking & Documents (Step 4)
export interface BankingDocuments {
  bankAccounts: BankAccount[]
  collaterals: Collateral[]
  documents: {
    taxCertificate?: File
    signatureCircular?: File
    tradeRegistry?: File
    partnershipAgreement?: File
    authorizedIdCopy?: File
    authorizedResidenceDocument?: File
  }
}

// Complete Registration Data (for form state)
export interface DealerRegistration {
  contactPerson: ContactPerson
  businessInfo: BusinessInfo
  operationalDetails: OperationalDetails
  bankingDocuments: BankingDocuments
}

// DTO for backend registration API (matches RegisterCommand)
export interface DealerRegistrationDto {
  // Company info
  title: string
  taxOffice?: string
  taxNo?: string
  establishmentYear?: number
  website?: string
  // Primary contact info
  contactFirstName: string
  contactLastName: string
  contactEmail: string
  contactPosition?: string
  contactDateOfBirth?: string  // ISO date string
  contactGender?: string  // Male, Female, Unknown
  contactPhone?: string
  contactPhoneExt?: string
  contactGsm?: string
  // Primary address info
  addressTitle?: string
  address?: string
  geoLocationId?: string
  postalCode?: string
  addressPhone?: string
  addressPhoneExt?: string
  addressGsm?: string
  // User account
  email: string
  password: string
  passwordConfirmation: string
  acceptTerms: boolean
  acceptKvkk: boolean
  // Document URLs (JSON string)
  documentUrls?: string
  // Customer attributes (collected during registration)
  attributes?: UpsertCustomerAttributesByDefinitionDto[]
}

// Registration response from backend
export interface RegistrationResponse {
  id: string
  companyName: string
  email: string
  isApproved: boolean
}

// Auth Response
export interface AuthResponse {
  accessToken: string
  refreshToken: string
  user: {
    id: string
    email: string
    firstName: string
    lastName: string
    customerId?: string
    role: string
  }
}

// Customer Document Types
export type CustomerDocumentType =
  | 'TaxCertificate'
  | 'SignatureCircular'
  | 'TradeRegistry'
  | 'PartnershipAgreement'
  | 'AuthorizedIdCopy'
  | 'AuthorizedResidenceDocument'

// Customer Document DTO
export interface CustomerDocumentDto {
  id: string
  customerId: string
  documentType: CustomerDocumentType
  documentTypeName: string
  fileName: string
  fileType: string
  contentUrl: string
  fileSize: number
  createdAt: string
  updatedAt?: string
}

// File upload response
export interface FileUploadResponse {
  url: string
  fileName: string
  contentType: string
  size: number
}

// Document upload state for registration
export interface DocumentUploadState {
  file: File | null
  uploadedUrl: string | null
  fileName: string | null
  fileType: string | null
  fileSize: number | null
  isUploading: boolean
  error: string | null
}

// Customer Attribute DTOs (matches backend UpsertCustomerAttributesByDefinitionDto)
export interface UpsertCustomerAttributesByDefinitionDto {
  attributeDefinitionId: string  // UUID of the attribute definition
  items: CustomerAttributeItemDto[]
}

export interface CustomerAttributeItemDto {
  id?: string  // Optional ID for updating existing
  value: string  // JSON string containing the attribute data
}
