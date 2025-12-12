# B2B E-Commerce Frontend Development Guide

## Document Purpose

This guide provides step-by-step instructions for Claude Code to create the first version of the B2B E-Commerce frontend application. It covers project setup, component development, and implementation order.

**Reference Document**: `B2B_UI_Design_Specification.md` (for detailed design specs)  
**Tech Stack**: Next.js 14+, React 18+, TypeScript, Tailwind CSS  
**Version**: 1.0  
**Date**: December 2025

---

## Table of Contents

1. [Project Setup](#project-setup)
2. [Design System Implementation](#design-system-implementation)
3. [Component Development Order](#component-development-order)
4. [Login Page Implementation](#login-page-implementation)
5. [Registration Flow Implementation](#registration-flow-implementation)
6. [Form Handling](#form-handling)
7. [API Integration](#api-integration)
8. [Testing Checklist](#testing-checklist)

---

## Project Setup

### 1. Create Next.js Project

```bash
npx create-next-app@latest b2b-ecommerce --typescript --tailwind --eslint --app --src-dir
cd b2b-ecommerce
```

### 2. Install Dependencies

```bash
# Form handling
npm install react-hook-form @hookform/resolvers zod

# HTTP client
npm install axios

# Icons
npm install lucide-react

# Class utilities
npm install clsx tailwind-merge

# Date handling (for birth date picker)
npm install date-fns

# Optional: Animation
npm install framer-motion
```

### 3. Project Structure

Create the following folder structure:

```
src/
├── app/
│   ├── (auth)/
│   │   ├── login/
│   │   │   └── page.tsx
│   │   ├── register/
│   │   │   ├── page.tsx           # Redirects to step-1
│   │   │   ├── step-1/
│   │   │   │   └── page.tsx
│   │   │   ├── step-2/
│   │   │   │   └── page.tsx
│   │   │   ├── step-3/
│   │   │   │   └── page.tsx
│   │   │   └── step-4/
│   │   │       └── page.tsx
│   │   └── layout.tsx             # Auth layout (centered, no sidebar)
│   ├── (dashboard)/
│   │   ├── layout.tsx             # Dashboard layout with sidebar
│   │   └── page.tsx               # Dashboard home
│   ├── layout.tsx                 # Root layout
│   ├── globals.css
│   └── page.tsx                   # Redirect to login or dashboard
├── components/
│   ├── ui/
│   │   ├── Button.tsx
│   │   ├── Input.tsx
│   │   ├── Select.tsx
│   │   ├── Checkbox.tsx
│   │   ├── FileUpload.tsx
│   │   ├── Card.tsx
│   │   ├── StepIndicator.tsx
│   │   └── index.ts
│   ├── forms/
│   │   ├── LoginForm.tsx
│   │   ├── ContactPersonForm.tsx      # Step 1
│   │   ├── BusinessInfoForm.tsx       # Step 2
│   │   ├── OperationalDetailsForm.tsx # Step 3
│   │   └── BankingDocumentsForm.tsx   # Step 4
│   ├── layout/
│   │   ├── AuthLayout.tsx
│   │   ├── Header.tsx
│   │   ├── Footer.tsx
│   │   └── Logo.tsx
│   └── shared/
│       └── FormSection.tsx
├── hooks/
│   ├── useRegistration.ts         # Registration state management
│   └── useAuth.ts                 # Auth state management
├── lib/
│   ├── api.ts                     # Axios instance
│   ├── utils.ts                   # Utility functions (cn, etc.)
│   └── validations/
│       ├── login.schema.ts
│       └── registration.schema.ts
├── types/
│   ├── auth.types.ts
│   └── registration.types.ts
└── stores/
    └── registrationStore.ts       # Zustand or context for multi-step form
```

---

## Design System Implementation

### 1. Tailwind Configuration

Update `tailwind.config.ts`:

```typescript
import type { Config } from 'tailwindcss'

const config: Config = {
  content: [
    './src/pages/**/*.{js,ts,jsx,tsx,mdx}',
    './src/components/**/*.{js,ts,jsx,tsx,mdx}',
    './src/app/**/*.{js,ts,jsx,tsx,mdx}',
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          DEFAULT: '#4e63a9',
          50: '#f0f2f8',
          100: '#e1e5f1',
          200: '#c3cbe3',
          300: '#a5b1d5',
          400: '#8797c7',
          500: '#4e63a9',
          600: '#3e4f87',
          700: '#2f3b65',
          800: '#1f2843',
          900: '#101422',
        },
        gray: {
          50: '#f7f7f7',
          100: '#efefef',
          200: '#d7dce6',
          300: '#bfc4ce',
          400: '#a7acb6',
          500: '#717171',
          600: '#5a5a5a',
          700: '#434343',
          800: '#2c2c2c',
          900: '#040404',
        },
      },
      fontFamily: {
        sans: ['var(--font-dm-sans)', 'system-ui', 'sans-serif'],
      },
    },
  },
  plugins: [],
}

export default config
```

### 2. Global Styles

Update `src/app/globals.css`:

```css
@import url('https://fonts.googleapis.com/css2?family=DM+Sans:wght@400;500;600;700&display=swap');

@tailwind base;
@tailwind components;
@tailwind utilities;

:root {
  --font-dm-sans: 'DM Sans', system-ui, sans-serif;
}

body {
  font-family: var(--font-dm-sans);
}

@layer components {
  .btn-primary {
    @apply bg-primary text-white px-6 py-3 rounded-md font-medium
           hover:bg-primary-600 transition-colors duration-200
           focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2
           disabled:opacity-50 disabled:cursor-not-allowed;
  }

  .btn-secondary {
    @apply bg-transparent text-primary border border-gray-200 px-6 py-3 rounded-md font-medium
           hover:bg-primary-50 transition-colors duration-200
           focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2;
  }

  .input-field {
    @apply w-full px-4 py-3 border border-gray-200 rounded-md text-gray-900
           placeholder:text-gray-500
           focus:outline-none focus:border-primary focus:ring-1 focus:ring-primary
           disabled:bg-gray-50 disabled:text-gray-500;
  }

  .input-label {
    @apply block text-sm font-medium text-gray-900 mb-1.5;
  }

  .input-error {
    @apply text-red-600 text-sm mt-1;
  }

  .form-section-title {
    @apply text-lg font-semibold text-gray-900 mb-4;
  }
}
```

### 3. Utility Functions

Create `src/lib/utils.ts`:

```typescript
import { type ClassValue, clsx } from 'clsx'
import { twMerge } from 'tailwind-merge'

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}
```

---

## Component Development Order

Implement components in this order to build up complexity:

### Phase 1: Base UI Components

1. **Button.tsx** - Primary and secondary button variants
2. **Input.tsx** - Text, email, password, tel input with label and error
3. **Select.tsx** - Dropdown select with label and error
4. **Card.tsx** - White container with shadow
5. **Logo.tsx** - Vesmarket logo component

### Phase 2: Layout Components

6. **Footer.tsx** - Copyright footer
7. **AuthLayout.tsx** - Centered layout for login/register
8. **StepIndicator.tsx** - 4-step progress indicator

### Phase 3: Form Components

9. **LoginForm.tsx** - Complete login form
10. **ContactPersonForm.tsx** - Step 1 registration
11. **Checkbox.tsx** - Checkbox component for step 3
12. **BusinessInfoForm.tsx** - Step 2 registration
13. **OperationalDetailsForm.tsx** - Step 3 registration
14. **FileUpload.tsx** - File upload component
15. **BankingDocumentsForm.tsx** - Step 4 registration

### Phase 4: Pages

16. **Login Page** - `/login`
17. **Register Step 1 Page** - `/register/step-1`
18. **Register Step 2 Page** - `/register/step-2`
19. **Register Step 3 Page** - `/register/step-3`
20. **Register Step 4 Page** - `/register/step-4`

---

## Login Page Implementation

### Button Component

```typescript
// src/components/ui/Button.tsx
import { forwardRef, ButtonHTMLAttributes } from 'react'
import { cn } from '@/lib/utils'

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary'
  size?: 'sm' | 'md' | 'lg'
  fullWidth?: boolean
  isLoading?: boolean
}

const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className, variant = 'primary', size = 'md', fullWidth, isLoading, children, disabled, ...props }, ref) => {
    const baseStyles = 'inline-flex items-center justify-center rounded-md font-medium transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-primary focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed'
    
    const variants = {
      primary: 'bg-primary text-white hover:bg-primary-600',
      secondary: 'bg-transparent text-primary border border-gray-200 hover:bg-primary-50',
    }
    
    const sizes = {
      sm: 'px-4 py-2 text-sm',
      md: 'px-6 py-3 text-sm',
      lg: 'px-8 py-4 text-base',
    }

    return (
      <button
        ref={ref}
        className={cn(
          baseStyles,
          variants[variant],
          sizes[size],
          fullWidth && 'w-full',
          className
        )}
        disabled={disabled || isLoading}
        {...props}
      >
        {isLoading ? (
          <svg className="animate-spin -ml-1 mr-2 h-4 w-4" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
          </svg>
        ) : null}
        {children}
      </button>
    )
  }
)

Button.displayName = 'Button'
export { Button }
```

### Input Component

```typescript
// src/components/ui/Input.tsx
import { forwardRef, InputHTMLAttributes } from 'react'
import { cn } from '@/lib/utils'

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string
  error?: string
  hint?: string
}

const Input = forwardRef<HTMLInputElement, InputProps>(
  ({ className, label, error, hint, id, ...props }, ref) => {
    const inputId = id || props.name

    return (
      <div className="w-full">
        {label && (
          <label htmlFor={inputId} className="input-label">
            {label}
          </label>
        )}
        <input
          ref={ref}
          id={inputId}
          className={cn(
            'input-field',
            error && 'border-red-500 focus:border-red-500 focus:ring-red-500',
            className
          )}
          {...props}
        />
        {hint && !error && (
          <p className="text-gray-500 text-sm mt-1">{hint}</p>
        )}
        {error && (
          <p className="input-error">{error}</p>
        )}
      </div>
    )
  }
)

Input.displayName = 'Input'
export { Input }
```

### Login Form Schema

```typescript
// src/lib/validations/login.schema.ts
import { z } from 'zod'

export const loginSchema = z.object({
  businessPartnerCode: z.string()
    .min(1, 'İş ortağı kodu gereklidir')
    .min(3, 'İş ortağı kodu en az 3 karakter olmalıdır'),
  email: z.string()
    .min(1, 'E-posta adresi gereklidir')
    .email('Geçerli bir e-posta adresi giriniz'),
  password: z.string()
    .min(1, 'Şifre gereklidir')
    .min(6, 'Şifre en az 6 karakter olmalıdır'),
})

export type LoginFormData = z.infer<typeof loginSchema>
```

### Login Page

```typescript
// src/app/(auth)/login/page.tsx
'use client'

import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import Link from 'next/link'
import Image from 'next/image'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { loginSchema, LoginFormData } from '@/lib/validations/login.schema'

export default function LoginPage() {
  const [isLoading, setIsLoading] = useState(false)

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  })

  const onSubmit = async (data: LoginFormData) => {
    setIsLoading(true)
    try {
      // API call here
      console.log('Login data:', data)
      // await loginUser(data)
    } catch (error) {
      console.error('Login error:', error)
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-gray-50 flex items-center justify-center p-4">
      <div className="w-full max-w-4xl bg-white rounded-lg shadow-lg overflow-hidden flex">
        {/* Left Side - Image */}
        <div className="hidden md:block w-1/2 relative">
          <Image
            src="/images/login-hero.jpg"
            alt="Business handshake"
            fill
            className="object-cover"
            priority
          />
        </div>

        {/* Right Side - Form */}
        <div className="w-full md:w-1/2 p-8 md:p-12">
          {/* Logo */}
          <div className="mb-6">
            <Link href="/" className="inline-flex items-center gap-2">
              {/* Logo SVG or Image */}
              <svg className="w-8 h-8 text-primary" viewBox="0 0 32 32" fill="currentColor">
                {/* Simplified shield logo */}
                <path d="M16 2L4 8v10c0 8 12 12 12 12s12-4 12-12V8L16 2z" />
              </svg>
              <span className="text-xl font-semibold text-gray-900">vesmarket</span>
            </Link>
          </div>

          {/* Title */}
          <h1 className="text-2xl font-bold text-gray-900 mb-2">
            Hoş geldiniz
          </h1>
          <p className="text-gray-500 mb-8">
            Sizinle iş ortağı olarak bir araya gelmekten büyük memnuniyet duyuyoruz.
          </p>

          {/* Form */}
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <Input
              label="İş Ortağı Kodu"
              placeholder="İş Ortağı Kodu"
              {...register('businessPartnerCode')}
              error={errors.businessPartnerCode?.message}
            />

            <Input
              type="email"
              label="E-Posta"
              placeholder="E-Posta"
              {...register('email')}
              error={errors.email?.message}
            />

            <Input
              type="password"
              label="Şifre"
              placeholder="Şifre"
              {...register('password')}
              error={errors.password?.message}
            />

            <div className="text-right">
              <Link 
                href="/forgot-password" 
                className="text-sm text-primary hover:text-primary-600"
              >
                Şifrenizi mi unuttunuz?
              </Link>
            </div>

            <div className="flex gap-4 pt-4">
              <Button type="submit" fullWidth isLoading={isLoading}>
                Giriş Yap
              </Button>
              <Button type="button" variant="secondary" fullWidth asChild>
                <Link href="/register/step-1">Kayıt Ol</Link>
              </Button>
            </div>
          </form>
        </div>
      </div>

      {/* Footer */}
      <footer className="absolute bottom-4 text-center w-full">
        <p className="text-sm text-gray-500">
          Copyright © 2025, Vesmarket Elektronik Hizmetler ve Ticaret A.Ş.
        </p>
      </footer>
    </div>
  )
}
```

---

## Registration Flow Implementation

### Step Indicator Component

```typescript
// src/components/ui/StepIndicator.tsx
import { cn } from '@/lib/utils'

interface StepIndicatorProps {
  currentStep: number
  totalSteps?: number
}

export function StepIndicator({ currentStep, totalSteps = 4 }: StepIndicatorProps) {
  return (
    <div className="flex items-center justify-center gap-0">
      {Array.from({ length: totalSteps }, (_, index) => {
        const step = index + 1
        const isActive = step === currentStep
        const isCompleted = step < currentStep

        return (
          <div key={step} className="flex items-center">
            {/* Step Circle */}
            <div
              className={cn(
                'w-10 h-10 rounded-full flex items-center justify-center text-sm font-medium border-2 transition-colors',
                isActive && 'bg-primary border-primary text-white',
                isCompleted && 'bg-primary border-primary text-white',
                !isActive && !isCompleted && 'bg-white border-gray-200 text-gray-500'
              )}
            >
              {step}
            </div>

            {/* Connection Line */}
            {step < totalSteps && (
              <div
                className={cn(
                  'w-16 h-0.5',
                  isCompleted ? 'bg-primary' : 'bg-gray-200'
                )}
              />
            )}
          </div>
        )
      })}
    </div>
  )
}
```

### Registration Store (State Management)

```typescript
// src/stores/registrationStore.ts
import { create } from 'zustand'
import { persist } from 'zustand/middleware'

interface ContactPerson {
  firstName: string
  lastName: string
  email: string
  alternativeEmail?: string
  position: string
  birthDate?: string
  gender?: string
  workPhone: string
  extension?: string
  mobile: string
}

interface AuthorizedPerson {
  fullName: string
  tcNumber: string
  sharePercentage: number
}

interface BusinessInfo {
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

interface RegistrationState {
  currentStep: number
  contactPerson: Partial<ContactPerson>
  businessInfo: Partial<BusinessInfo>
  operationalDetails: Record<string, any>
  bankingDocuments: Record<string, any>
  
  setCurrentStep: (step: number) => void
  setContactPerson: (data: Partial<ContactPerson>) => void
  setBusinessInfo: (data: Partial<BusinessInfo>) => void
  setOperationalDetails: (data: Record<string, any>) => void
  setBankingDocuments: (data: Record<string, any>) => void
  reset: () => void
}

const initialState = {
  currentStep: 1,
  contactPerson: {},
  businessInfo: {},
  operationalDetails: {},
  bankingDocuments: {},
}

export const useRegistrationStore = create<RegistrationState>()(
  persist(
    (set) => ({
      ...initialState,
      setCurrentStep: (step) => set({ currentStep: step }),
      setContactPerson: (data) => set((state) => ({ 
        contactPerson: { ...state.contactPerson, ...data } 
      })),
      setBusinessInfo: (data) => set((state) => ({ 
        businessInfo: { ...state.businessInfo, ...data } 
      })),
      setOperationalDetails: (data) => set((state) => ({ 
        operationalDetails: { ...state.operationalDetails, ...data } 
      })),
      setBankingDocuments: (data) => set((state) => ({ 
        bankingDocuments: { ...state.bankingDocuments, ...data } 
      })),
      reset: () => set(initialState),
    }),
    {
      name: 'registration-storage',
    }
  )
)
```

### Registration Layout

```typescript
// src/app/(auth)/register/layout.tsx
import { Footer } from '@/components/layout/Footer'

export default function RegisterLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <div className="min-h-screen bg-gray-50 flex flex-col">
      {/* Header Section */}
      <header className="py-8 text-center">
        <h1 className="text-3xl font-bold text-gray-900 mb-2">
          Merhaba
        </h1>
        <p className="text-gray-500 max-w-xl mx-auto px-4">
          Sizinle iş ortağı olarak bir araya gelmekten büyük memnuniyet duyuyoruz.
          Bu iş birliğinin karşılıklı güven ve uyum içinde uzun yıllar devam etmesini,
          birlikte birçok başarılı işlere imza atmayı gönülden diliyoruz.
        </p>
      </header>

      {/* Main Content */}
      <main className="flex-1 px-4 pb-8">
        {children}
      </main>

      {/* Footer */}
      <Footer />
    </div>
  )
}
```

### Step 1 Page (Contact Person)

```typescript
// src/app/(auth)/register/step-1/page.tsx
'use client'

import { useRouter } from 'next/navigation'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { z } from 'zod'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import { StepIndicator } from '@/components/ui/StepIndicator'
import { useRegistrationStore } from '@/stores/registrationStore'

const step1Schema = z.object({
  firstName: z.string().min(1, 'Adı gereklidir'),
  lastName: z.string().min(1, 'Soyadı gereklidir'),
  email: z.string().email('Geçerli bir e-posta adresi giriniz'),
  alternativeEmail: z.string().email().optional().or(z.literal('')),
  position: z.string().min(1, 'Görevi gereklidir'),
  birthDate: z.string().optional(),
  gender: z.string().optional(),
  workPhone: z.string().min(10, 'Geçerli bir telefon numarası giriniz'),
  extension: z.string().optional(),
  mobile: z.string().min(10, 'Geçerli bir telefon numarası giriniz'),
})

type Step1FormData = z.infer<typeof step1Schema>

export default function RegisterStep1Page() {
  const router = useRouter()
  const { contactPerson, setContactPerson, setCurrentStep } = useRegistrationStore()

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<Step1FormData>({
    resolver: zodResolver(step1Schema),
    defaultValues: contactPerson as Step1FormData,
  })

  const onSubmit = async (data: Step1FormData) => {
    setContactPerson(data)
    setCurrentStep(2)
    router.push('/register/step-2')
  }

  return (
    <div className="max-w-5xl mx-auto">
      {/* Step Indicator */}
      <div className="mb-8">
        <StepIndicator currentStep={1} />
      </div>

      {/* Form Card */}
      <div className="bg-white rounded-lg shadow-lg overflow-hidden">
        <div className="flex flex-col md:flex-row">
          {/* Left Side - Image Placeholder */}
          <div className="md:w-1/2 bg-gray-100 min-h-[400px] flex items-center justify-center">
            <span className="text-gray-400 text-lg">910 x 860px</span>
          </div>

          {/* Right Side - Form */}
          <div className="md:w-1/2 p-8">
            <h2 className="form-section-title">İlgili Kişi</h2>

            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <div className="grid grid-cols-2 gap-4">
                <Input
                  label="Adı"
                  placeholder="Adı"
                  {...register('firstName')}
                  error={errors.firstName?.message}
                />
                <Input
                  label="Soyadı"
                  placeholder="Soyadı"
                  {...register('lastName')}
                  error={errors.lastName?.message}
                />
              </div>

              <div className="grid grid-cols-2 gap-4">
                <Input
                  type="email"
                  label="E-Posta"
                  placeholder="E-Posta"
                  {...register('email')}
                  error={errors.email?.message}
                />
                <Input
                  type="email"
                  label="Alternatif E-Posta"
                  placeholder="Alternatif E-Posta"
                  {...register('alternativeEmail')}
                  error={errors.alternativeEmail?.message}
                />
              </div>

              <Input
                label="Görevi"
                placeholder="Görevi"
                {...register('position')}
                error={errors.position?.message}
              />

              <div className="grid grid-cols-2 gap-4">
                <Input
                  type="date"
                  label="Doğum Tarihi"
                  {...register('birthDate')}
                  error={errors.birthDate?.message}
                />
                <Select
                  label="Cinsiyet"
                  {...register('gender')}
                  error={errors.gender?.message}
                >
                  <option value="">Seçiniz</option>
                  <option value="male">Erkek</option>
                  <option value="female">Kadın</option>
                  <option value="prefer_not_to_say">Belirtmek İstemiyorum</option>
                </Select>
              </div>

              <div className="grid grid-cols-2 gap-4">
                <Input
                  type="tel"
                  label="İş Telefon"
                  placeholder="İş Telefon"
                  {...register('workPhone')}
                  error={errors.workPhone?.message}
                />
                <Input
                  label="Dahili Numara"
                  placeholder="Dahili Numara"
                  {...register('extension')}
                  error={errors.extension?.message}
                />
              </div>

              <Input
                type="tel"
                label="Mobil"
                placeholder="Mobil"
                {...register('mobile')}
                error={errors.mobile?.message}
              />

              <div className="pt-4">
                <Button type="submit" fullWidth isLoading={isSubmitting}>
                  Kaydet & Devam Et
                </Button>
              </div>
            </form>
          </div>
        </div>
      </div>
    </div>
  )
}
```

---

## Form Handling

### Select Component

```typescript
// src/components/ui/Select.tsx
import { forwardRef, SelectHTMLAttributes } from 'react'
import { cn } from '@/lib/utils'

interface SelectProps extends SelectHTMLAttributes<HTMLSelectElement> {
  label?: string
  error?: string
}

const Select = forwardRef<HTMLSelectElement, SelectProps>(
  ({ className, label, error, children, id, ...props }, ref) => {
    const selectId = id || props.name

    return (
      <div className="w-full">
        {label && (
          <label htmlFor={selectId} className="input-label">
            {label}
          </label>
        )}
        <select
          ref={ref}
          id={selectId}
          className={cn(
            'input-field appearance-none bg-white',
            'bg-[url("data:image/svg+xml;charset=utf-8,%3Csvg xmlns=\'http://www.w3.org/2000/svg\' fill=\'none\' viewBox=\'0 0 20 20\'%3E%3Cpath stroke=\'%236b7280\' stroke-linecap=\'round\' stroke-linejoin=\'round\' stroke-width=\'1.5\' d=\'m6 8 4 4 4-4\'/%3E%3C/svg%3E")]',
            'bg-[length:1.5rem_1.5rem] bg-[right_0.5rem_center] bg-no-repeat pr-10',
            error && 'border-red-500 focus:border-red-500 focus:ring-red-500',
            className
          )}
          {...props}
        >
          {children}
        </select>
        {error && <p className="input-error">{error}</p>}
      </div>
    )
  }
)

Select.displayName = 'Select'
export { Select }
```

### Checkbox Component

```typescript
// src/components/ui/Checkbox.tsx
import { forwardRef, InputHTMLAttributes } from 'react'
import { cn } from '@/lib/utils'

interface CheckboxProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'type'> {
  label: string
}

const Checkbox = forwardRef<HTMLInputElement, CheckboxProps>(
  ({ className, label, id, ...props }, ref) => {
    const checkboxId = id || props.name

    return (
      <label htmlFor={checkboxId} className="flex items-center gap-2 cursor-pointer">
        <input
          ref={ref}
          type="checkbox"
          id={checkboxId}
          className={cn(
            'w-4 h-4 text-primary border-gray-300 rounded',
            'focus:ring-2 focus:ring-primary focus:ring-offset-2',
            className
          )}
          {...props}
        />
        <span className="text-sm text-gray-700">{label}</span>
      </label>
    )
  }
)

Checkbox.displayName = 'Checkbox'
export { Checkbox }
```

---

## API Integration

### API Client Setup

```typescript
// src/lib/api.ts
import axios from 'axios'

const api = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5000/api',
  headers: {
    'Content-Type': 'application/json',
  },
})

// Request interceptor for auth token
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Response interceptor for token refresh
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      // Handle token refresh or redirect to login
    }
    return Promise.reject(error)
  }
)

export { api }
```

### Auth API Functions

```typescript
// src/lib/api/auth.ts
import { api } from '../api'
import { LoginFormData } from '../validations/login.schema'

export interface AuthResponse {
  accessToken: string
  refreshToken: string
  user: {
    id: number
    email: string
    firstName: string
    lastName: string
    customerId: number
    role: string
  }
}

export async function loginUser(data: LoginFormData): Promise<AuthResponse> {
  const response = await api.post<AuthResponse>('/auth/login', data)
  return response.data
}

export async function registerStep(step: number, data: any): Promise<void> {
  await api.post(`/auth/register/step-${step}`, data)
}

export async function submitRegistration(data: any): Promise<AuthResponse> {
  const response = await api.post<AuthResponse>('/auth/register/submit', data)
  return response.data
}
```

---

## Testing Checklist

### Login Page

- [ ] Form displays correctly with all fields
- [ ] Validation errors show on invalid input
- [ ] Loading state shows during submission
- [ ] Successful login redirects to dashboard
- [ ] Failed login shows error message
- [ ] "Forgot Password" link works
- [ ] "Register" button navigates to registration

### Registration Flow

- [ ] Step indicator shows correct active step
- [ ] Step 1 form validates and saves to store
- [ ] Navigation between steps works
- [ ] Form data persists when navigating back
- [ ] All field types work (text, select, checkbox, file)
- [ ] Step 4 submit sends all data to API
- [ ] Error handling for API failures

### Responsive Design

- [ ] Login page works on mobile (single column)
- [ ] Registration works on mobile
- [ ] All buttons and inputs are touch-friendly
- [ ] Text is readable on all screen sizes

### Accessibility

- [ ] All form inputs have labels
- [ ] Error messages are announced to screen readers
- [ ] Tab navigation works correctly
- [ ] Focus states are visible
- [ ] Color contrast meets WCAG standards

---

## Next Steps After Initial Implementation

1. **Dashboard Layout** - Create main dashboard with sidebar navigation
2. **Product Catalog** - Product listing and search
3. **Shopping Cart** - Cart management
4. **Order Flow** - Order creation and approval workflow
5. **Profile Management** - User profile editing
6. **Notifications** - Real-time notifications

---

**Document Version**: 1.0  
**Last Updated**: December 2025  
**Maintained By**: Development Team
