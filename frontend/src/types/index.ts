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
  city: string
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

// Complete Registration Data
export interface DealerRegistration {
  contactPerson: ContactPerson
  businessInfo: BusinessInfo
  operationalDetails: OperationalDetails
  bankingDocuments: BankingDocuments
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
