// Contact Person (Step 1)
export interface ContactPerson {
  firstName: string
  lastName: string
  email: string
  alternativeEmail?: string
  position: string
  birthDate?: string
  gender?: string
  country?: string
  city?: string
  district?: string
  workPhone: string
  extension?: string
  mobile: string
}

// Authorized Person
export interface AuthorizedPerson {
  fullName?: string
  tcNumber?: string
  sharePercentage?: number
}

// Business Information (Step 2)
export interface BusinessInfo {
  companyTitle: string
  taxOffice: string
  taxNumber: string
  foundedYear?: number
  address: string
  country: string
  phone: string
  website?: string
  authorizedPersons: AuthorizedPerson[]
}

// Business Partner
export interface BusinessPartner {
  companyName?: string
  workingCondition?: string
  creditLimit?: number
}

// Operational Details (Step 3)
export interface OperationalDetails {
  employeeCount: string
  businessStructure: string
  revenueYear?: number
  targetRevenue?: number
  customerBase: {
    retailer: number
    corporate: number
    construction: number
    retail: number
  }
  productCategories: string[]
  currentPartners: BusinessPartner[]
  requestedConditions: string[]
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
  // Required company info
  companyName: string
  taxNumber: string
  taxOffice: string
  email: string
  phone: string
  contactPersonName: string
  contactPersonTitle: string
  // Optional company info
  tradeName?: string
  mersisNo?: string
  identityNo?: string
  tradeRegistryNo?: string
  mobilePhone?: string
  fax?: string
  website?: string
  // Optional financial info
  creditLimit?: number
  currency?: string
  type?: string
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
