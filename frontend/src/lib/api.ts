import axios from 'axios'
import type { LoginFormData } from './validations/login.schema'
import type { AuthResponse, CustomerDocumentDto, CustomerDocumentType, DealerRegistrationDto, FileUploadResponse, RegistrationResponse } from '@/types'

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL || 'https://localhost:5001/api',
  headers: {
    'Content-Type': 'application/json',
  },
})

// Request interceptor for auth token
api.interceptors.request.use((config) => {
  if (typeof window !== 'undefined') {
    const token = localStorage.getItem('accessToken')
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
  }
  return config
})

// Response interceptor for token refresh
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      // Handle token refresh or redirect to login
      if (typeof window !== 'undefined') {
        localStorage.removeItem('accessToken')
        localStorage.removeItem('refreshToken')
        window.location.href = '/login'
      }
    }
    return Promise.reject(error)
  }
)

// Auth API functions
export async function loginUser(data: LoginFormData): Promise<AuthResponse> {
  const response = await api.post<AuthResponse>('/auth/login', data)
  return response.data
}

export async function registerDealer(data: DealerRegistrationDto): Promise<RegistrationResponse> {
  const response = await api.post<RegistrationResponse>('/auth/register', data)
  return response.data
}

export async function refreshToken(refreshToken: string): Promise<AuthResponse> {
  const response = await api.post<AuthResponse>('/auth/refresh', { refreshToken })
  return response.data
}

export async function logout(): Promise<void> {
  await api.post('/auth/logout')
}

export interface CheckEmailResponse {
  available: boolean
  message: string
}

export async function checkEmailAvailability(email: string): Promise<CheckEmailResponse> {
  const response = await api.get<CheckEmailResponse>('/auth/check-email', {
    params: { email }
  })
  return response.data
}

// Newsletter API
export interface NewsletterResponse {
  id: string
  email: string
  subscribedAt: string
  isVerified: boolean
  message: string
}

export async function subscribeNewsletter(email: string): Promise<NewsletterResponse> {
  const response = await api.post<NewsletterResponse>('/newsletter/subscribe', { email })
  return response.data
}

// Document Upload API functions

/**
 * Upload a document during registration (anonymous access)
 * Returns the uploaded file URL and metadata
 */
export async function uploadRegistrationDocument(file: File): Promise<FileUploadResponse> {
  const formData = new FormData()
  formData.append('file', file)

  const response = await api.post<FileUploadResponse>('/files/upload/registration-document', formData, {
    headers: {
      'Content-Type': 'multipart/form-data',
    },
  })
  return response.data
}

/**
 * Save document association after customer registration (anonymous access)
 */
export async function saveCustomerDocument(
  customerId: string,
  documentType: CustomerDocumentType,
  contentUrl: string,
  fileName: string,
  fileType: string,
  fileSize: number
): Promise<CustomerDocumentDto> {
  const response = await api.post<CustomerDocumentDto>(`/customers/${customerId}/documents/registration`, {
    documentType,
    contentUrl,
    fileName,
    fileType,
    fileSize,
  })
  return response.data
}

/**
 * Get all documents for a customer
 */
export async function getCustomerDocuments(customerId: string): Promise<CustomerDocumentDto[]> {
  const response = await api.get<CustomerDocumentDto[]>(`/customers/${customerId}/documents`)
  return response.data
}

/**
 * Delete a customer document
 */
export async function deleteCustomerDocument(customerId: string, documentId: string): Promise<void> {
  await api.delete(`/customers/${customerId}/documents/${documentId}`)
}

// GeoLocation API functions

export interface GeoLocationType {
  id: string
  name: string
  displayOrder: number
  locationCount: number
}

export interface GeoLocation {
  id: string
  code: string
  name: string
  geoLocationTypeId: string
  geoLocationTypeName: string
  parentId?: string
  parentName?: string
  pathName?: string
  depth: number
}

/**
 * Get all location types (Country, City, District, etc.)
 */
export async function getGeoLocationTypes(): Promise<GeoLocationType[]> {
  const response = await api.get<GeoLocationType[]>('/GeoLocationTypes')
  return response.data
}

/**
 * Get locations by type ID
 */
export async function getGeoLocationsByType(typeId: string): Promise<GeoLocation[]> {
  const response = await api.get<GeoLocation[]>(`/GeoLocations/by-type/${typeId}`)
  return response.data
}

/**
 * Get child locations by parent ID
 */
export async function getGeoLocationsByParent(parentId: string): Promise<GeoLocation[]> {
  const response = await api.get<GeoLocation[]>(`/GeoLocations/${parentId}/children`)
  return response.data
}

/**
 * Get root locations (no parent)
 */
export async function getRootGeoLocations(): Promise<GeoLocation[]> {
  const response = await api.get<GeoLocation[]>('/GeoLocations/root')
  return response.data
}

// Attribute API functions

export interface AttributeDefinition {
  id: string
  code: string
  name: string
  type: number | string
  entityType?: number | string
  parentAttributeId?: string
  isList?: boolean
  unit?: string
  isFilterable: boolean
  isRequired: boolean
  isVisibleOnProductPage: boolean
  displayOrder: number
  predefinedValues: AttributeValue[]
}

export interface AttributeValue {
  id: string
  attributeDefinitionId: string
  value: string
  displayText?: string
  displayOrder: number
}

/**
 * Get attribute definition by code
 */
export async function getAttributeByCode(code: string): Promise<AttributeDefinition> {
  const response = await api.get<AttributeDefinition>(`/AttributeDefinitions/by-code/${code}`)
  return response.data
}

/**
 * Get child attributes of a composite attribute
 */
export async function getChildAttributes(parentId: string): Promise<AttributeDefinition[]> {
  const response = await api.get<AttributeDefinition[]>(`/AttributeDefinitions/${parentId}/children`)
  return response.data
}

export { api }
