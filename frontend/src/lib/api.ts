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

export { api }
