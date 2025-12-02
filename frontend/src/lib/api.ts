import axios from 'axios'
import type { LoginFormData } from './validations/login.schema'
import type { AuthResponse, DealerRegistration } from '@/types'

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000/api',
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

export async function registerDealer(data: DealerRegistration): Promise<AuthResponse> {
  const response = await api.post<AuthResponse>('/auth/register', data)
  return response.data
}

export async function refreshToken(refreshToken: string): Promise<AuthResponse> {
  const response = await api.post<AuthResponse>('/auth/refresh', { refreshToken })
  return response.data
}

export async function logout(): Promise<void> {
  await api.post('/auth/logout')
}

export { api }
