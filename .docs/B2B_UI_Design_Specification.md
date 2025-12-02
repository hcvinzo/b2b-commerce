# B2B E-Commerce Platform - UI Design Specification

## Document Purpose

This document provides complete UI design specifications for the B2B E-Commerce Platform frontend, extracted from design mockups. It covers login, registration flows, and establishes the design system (colors, typography, components) for consistent implementation.

**Target**: Next.js Frontend (React + TypeScript + Tailwind CSS)  
**Version**: 1.0  
**Date**: December 2025

---

## Table of Contents

1. [Design System](#design-system)
2. [Login Page](#login-page)
3. [Registration Flow](#registration-flow)
4. [Form Components](#form-components)
5. [Implementation Guidelines](#implementation-guidelines)

---

## Design System

### Brand

**Brand Name**: Vesmarket  
**Logo**: Shield-like icon with "vesmarket" text  
**Tagline**: B2B E-Commerce Platform for Dealer Partners

### Color Palette

| Color Name | Hex Code | Usage |
|------------|----------|-------|
| Primary Blue | `#4e63a9` | Primary buttons, links, active states, brand accent |
| Black | `#040404` | Headings, primary text |
| Gray | `#717171` | Secondary text, placeholders, labels |
| Light Blue/Gray | `#d7dce6` | Borders, input outlines, inactive states |
| Light Gray | `#f7f7f7` | Page backgrounds |
| White | `#ffffff` | Card backgrounds, input backgrounds |

### Tailwind CSS Color Configuration

```javascript
// tailwind.config.js
module.exports = {
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
        }
      }
    }
  }
}
```

### Typography

**Font Family**: DM Sans (Google Fonts)

```css
@import url('https://fonts.googleapis.com/css2?family=DM+Sans:wght@400;500;600;700&display=swap');
```

#### Type Scale

| Element | Size | Weight | Color |
|---------|------|--------|-------|
| Page Title | 28-32px | 700 (Bold) | `#040404` |
| Section Title | 18-20px | 600 (Semibold) | `#040404` |
| Subtitle/Description | 14-16px | 400 (Regular) | `#717171` |
| Form Label | 14px | 500 (Medium) | `#040404` |
| Input Text | 14px | 400 (Regular) | `#040404` |
| Input Placeholder | 14px | 400 (Regular) | `#717171` |
| Button Text | 14px | 500 (Medium) | White or `#4e63a9` |
| Small Text/Caption | 12px | 400 (Regular) | `#717171` |

### Tailwind Typography Configuration

```javascript
// tailwind.config.js
module.exports = {
  theme: {
    extend: {
      fontFamily: {
        sans: ['DM Sans', 'system-ui', 'sans-serif'],
      },
    }
  }
}
```

---

## Login Page

### Layout Structure

```
┌─────────────────────────────────────────────────────────────────┐
│                     Page Background (#f7f7f7)                   │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                    Centered Card (White)                  │  │
│  │  ┌─────────────────────┬───────────────────────────────┐  │  │
│  │  │                     │                               │  │  │
│  │  │   Hero Image        │   Form Section                │  │  │
│  │  │   (Handshake with   │                               │  │  │
│  │  │    digital overlay) │   - Logo                      │  │  │
│  │  │                     │   - Welcome Title             │  │  │
│  │  │   Width: ~50%       │   - Description               │  │  │
│  │  │                     │   - Business Partner Code     │  │  │
│  │  │                     │   - Email                     │  │  │
│  │  │                     │   - Password                  │  │  │
│  │  │                     │   - Forgot Password Link      │  │  │
│  │  │                     │   - Login Button (Primary)    │  │  │
│  │  │                     │   - Register Button (Outline) │  │  │
│  │  │                     │                               │  │  │
│  │  └─────────────────────┴───────────────────────────────┘  │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                 │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  Copyright © 2025, Vesmarket Elektronik Hizmetler ...     │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### Login Form Fields

| Field | Type | Placeholder (Turkish) | Placeholder (English) | Required |
|-------|------|----------------------|----------------------|----------|
| Business Partner Code | text | İş Ortağı Kodu | Business Partner Code | Yes |
| Email | email | E-Posta | Email | Yes |
| Password | password | Şifre | Password | Yes |

### Login Actions

| Action | Type | Label (Turkish) | Label (English) | Style |
|--------|------|-----------------|-----------------|-------|
| Login | button | Giriş Yap | Login | Primary (filled blue) |
| Register | button/link | Kayıt Ol | Register | Secondary (outlined) |
| Forgot Password | link | Şifrenizi mi unuttunuz? | Forgot your password? | Text link |

### Login Page Component Structure

```typescript
// pages/login/page.tsx or app/login/page.tsx
interface LoginFormData {
  businessPartnerCode: string;
  email: string;
  password: string;
}

// Components needed:
// - LoginPage (container)
// - LoginCard (white card with image + form)
// - HeroImage (left side image)
// - LoginForm (right side form)
// - Logo
// - FormInput (reusable)
// - PrimaryButton
// - OutlineButton
// - TextLink
// - Footer
```

---

## Registration Flow

### Overview

4-step wizard registration flow for dealer customer onboarding:

| Step | Title (Turkish) | Title (English) | Description |
|------|-----------------|-----------------|-------------|
| 1 | İlgili Kişi | Contact Person | Primary contact information |
| 2 | İşletme Bilgileri | Business Information | Company details and authorized persons |
| 3 | Operasyonel Bilgiler | Operational Details | Business metrics, product categories, working conditions |
| 4 | Banka & Belgeler | Banking & Documents | Bank accounts, collaterals, required documents |

### Registration Layout Structure

```
┌─────────────────────────────────────────────────────────────────┐
│                     Page Background (#f7f7f7)                   │
│                                                                 │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                    Header Section                         │  │
│  │         "Merhaba" (Welcome Title - Centered)              │  │
│  │         Welcome message text (Centered, Gray)             │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                 │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                  Step Indicator                           │  │
│  │     ●────●────●────●                                      │  │
│  │     1    2    3    4                                      │  │
│  │  (Active step: filled, others: outlined)                  │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                 │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │                   Form Card (White)                       │  │
│  │  ┌─────────────────────┬───────────────────────────────┐  │  │
│  │  │   Left Section      │   Right Section               │  │  │
│  │  │   (Image or Form)   │   (Form fields)               │  │  │
│  │  │                     │                               │  │  │
│  │  └─────────────────────┴───────────────────────────────┘  │  │
│  │                                                           │  │
│  │                    [Save & Continue Button]               │  │
│  └───────────────────────────────────────────────────────────┘  │
│                                                                 │
│  ┌───────────────────────────────────────────────────────────┐  │
│  │  Copyright © 2025, Vesmarket Elektronik Hizmetler ...     │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

### Step 1: Contact Person (İlgili Kişi)

#### Layout
- Left: Placeholder image (910 x 860px) or decorative image
- Right: Form fields

#### Form Fields

| Field | Type | Label (TR) | Label (EN) | Required | Width |
|-------|------|------------|------------|----------|-------|
| firstName | text | Adı | First Name | Yes | 50% |
| lastName | text | Soyadı | Last Name | Yes | 50% |
| email | email | E-Posta | Email | Yes | 50% |
| alternativeEmail | email | Alternatif E-Posta | Alternative Email | No | 50% |
| position | text | Görevi | Position/Role | Yes | 100% |
| birthDate | date | Doğum Tarihi | Birth Date | No | 50% |
| gender | select | Cinsiyet | Gender | No | 50% |
| workPhone | tel | İş Telefon | Work Phone | Yes | 50% |
| extension | text | Dahili Numara | Extension | No | 50% |
| mobile | tel | Mobil | Mobile | Yes | 100% |

#### Gender Options
- Erkek / Male
- Kadın / Female
- Belirtmek İstemiyorum / Prefer not to say

### Step 2: Business Information (İşletme Bilgileri)

#### Layout
- Left section: Business information + Communication
- Right section: Authorized persons & Partners

#### Left Section - Business Information (İşletme Bilgileri)

| Field | Type | Label (TR) | Label (EN) | Required | Width |
|-------|------|------------|------------|----------|-------|
| companyTitle | text | Ünvan | Company Title | Yes | 100% |
| taxOffice | text | Vergi Dairesi | Tax Office | Yes | 33% |
| taxNumber | text | Vergi Numarası | Tax Number | Yes | 33% |
| foundedYear | number | Kuruluş Yılı | Founded Year | No | 33% |

#### Left Section - Communication (İletişim)

| Field | Type | Label (TR) | Label (EN) | Required | Width |
|-------|------|------------|------------|----------|-------|
| address | textarea | Adres | Address | Yes | 100% |
| city | text | Şehir | City | Yes | 33% |
| phone | tel | Telefon | Phone | Yes | 33% |
| website | url | İnternet Sayfası | Website | No | 33% |

*Note: Multiple address rows supported (2 rows visible in design)*

#### Right Section - Authorized Persons & Partners (Yetkililer & Ortaklar)

Repeating row structure (6 rows in design):

| Field | Type | Label (TR) | Label (EN) | Required | Width |
|-------|------|------------|------------|----------|-------|
| fullName | text | Adı Soyadı | Full Name | Yes* | 40% |
| tcNumber | text | T.C. Kimlik Numarası | Turkish ID Number | Yes* | 35% |
| sharePercentage | number | Pay Oranı | Share Percentage | Yes* | 25% |

*At least one authorized person required

### Step 3: Operational Details (Operasyonel Bilgiler)

#### Layout
- Left section: Business metrics, customer base, product categories
- Right section: Current business partners, requested working conditions

#### Left Section - Business Metrics

**Personnel Count (Personel Sayısı)**

| Field | Type | Label (TR) | Label (EN) | Options |
|-------|------|------------|------------|---------|
| employeeCount | select | Personel Sayısı | Personnel Count | Dropdown (1-10, 11-50, 51-200, 200+) |

**Business Structure (İşletme Yapısı)**

| Field | Type | Label (TR) | Label (EN) | Options |
|-------|------|------------|------------|---------|
| businessStructure | select | İşletme Yapısı | Business Structure | Dropdown |

**Revenue Information (Ciro Bilgisi)**

| Field | Type | Label (TR) | Label (EN) | Required |
|-------|------|------------|------------|----------|
| revenueYear | number | Değiştiğinde Yıl | Year of Last Change | No |
| targetRevenue | number | Bu Yıl Hedeflenen | This Year Target | No |

**Customer Base (Müşteri Kitlesi)** - Percentages

| Field | Type | Label (TR) | Label (EN) |
|-------|------|------------|------------|
| retailerPercent | number | Satıcı & Kurgu | Retailer & Setup |
| corporatePercent | number | Kurumsal | Corporate |
| constructionPercent | number | Boya | Construction/Paint |
| retailPercent | number | Perakende | Retail |

*Note: Total should equal 100%*

**Product Categories (Satışını Gerçekleştirdiğiniz Ürün Kategorileri)**

Checkbox grid:

| Category (TR) | Category (EN) |
|---------------|---------------|
| Bilgisayarlar | Computers |
| İş Makineleri | Industrial Machines |
| Sunucular | Servers |
| Yazılımlar | Software |
| Network | Networking |
| Güvenlik | Security |
| Depolama | Storage |
| Ağ Ürünleri | Network Products |
| Yazıcı Çözümleri | Printer Solutions |
| Aksesuarlar | Accessories |
| Diğer | Other |

#### Right Section - Business Relationships

**Current Business Partners (Çalışmakta Olduğunuz Şirketler & Çalışma Koşulları)**

Repeating row structure (4 rows):

| Field | Type | Label (TR) | Label (EN) | Width |
|-------|------|------------|------------|-------|
| companyName | text | Şirket Adı | Company Name | 40% |
| workingCondition | select | Seçiniz | Select Condition | 30% |
| creditLimit | number | Limit Tutarı (USD) | Limit Amount (USD) | 30% |

**Requested Working Conditions (Talep Ettiğiniz Çalışma Koşulları)**

Checkbox group:

| Option (TR) | Option (EN) |
|-------------|-------------|
| Nakit & Kredi Kartı | Cash & Credit Card |
| Açık Hesap & Vadeli | Open Account & Deferred |
| Çek | Check |

### Step 4: Banking & Documents (Banka & Belgeler)

#### Layout
- Left section: Bank accounts
- Right section: Collaterals + Documents

#### Left Section - Bank Accounts (Banka Hesap Bilgileri)

Repeating row structure (6 rows):

| Field | Type | Label (TR) | Label (EN) | Width |
|-------|------|------------|------------|-------|
| bankName | text/select | Banka Adı | Bank Name | 45% |
| iban | text | IBAN | IBAN | 55% |

*IBAN Format: TR + 2 check digits + 5 bank code + 1 reserve + 16 account number*

#### Right Section - Collaterals (Teminatlar)

Repeating row structure (3 rows):

| Field | Type | Label (TR) | Label (EN) | Width |
|-------|------|------------|------------|-------|
| guaranteeType | select | Teminat Türü | Guarantee Type | 40% |
| amount | number | Tutar | Amount | 30% |
| currency | select | Para Birimi | Currency | 30% |

Guarantee Types:
- Nakit / Cash
- Teminat Mektubu / Letter of Guarantee
- İpotek / Mortgage
- Kefalet / Surety

Currency Options:
- TRY, USD, EUR

#### Right Section - Documents (Evrak & Belgeler)

File upload fields (2 columns × 3 rows):

| Document (TR) | Document (EN) | Type |
|---------------|---------------|------|
| Vergi Levhası (Son) | Tax Certificate (Latest) | PDF/Image |
| İmza Sirküleri | Signature Circular | PDF/Image |
| Sicil Gazetesi (Son) | Trade Registry (Latest) | PDF/Image |
| İş Ortağı Sözleşmesi | Partnership Agreement | PDF |
| Yetkili Kimlik Fotokopisi | Authorized ID Copy | PDF/Image |
| Yetkili İkamet Belgesi | Authorized Residence Document | PDF/Image |

---

## Form Components

### Input Field Component

```
┌─────────────────────────────────────┐
│ Label Text                          │
├─────────────────────────────────────┤
│ [                     Placeholder ] │
└─────────────────────────────────────┘
```

**States:**
- Default: Border `#d7dce6`, Background `#ffffff`
- Focus: Border `#4e63a9`, Shadow subtle blue glow
- Error: Border `#dc2626`, Error message below
- Disabled: Background `#f7f7f7`, Text `#717171`

### Button Components

**Primary Button (Filled)**
```
┌─────────────────────────────────────┐
│           Button Text               │
└─────────────────────────────────────┘
Background: #4e63a9
Text: #ffffff
Border-radius: 6px
Padding: 12px 24px
Hover: Background #3e4f87
```

**Secondary Button (Outline)**
```
┌─────────────────────────────────────┐
│           Button Text               │
└─────────────────────────────────────┘
Background: transparent
Border: 1px solid #d7dce6
Text: #4e63a9
Border-radius: 6px
Padding: 12px 24px
Hover: Background #f0f2f8
```

### Step Indicator Component

```
   ●─────●─────●─────●
   1     2     3     4

Active step: Filled circle (#4e63a9)
Completed step: Filled circle (#4e63a9)
Upcoming step: Outlined circle (#d7dce6)
Connection line: #d7dce6
```

### File Upload Component

```
┌─────────────────────────────────────┐
│ Document Name                       │
├─────────────────────────────────────┤
│  [📁 Upload or drag file here    ] │
│  or                                 │
│  [Browse Files]                     │
└─────────────────────────────────────┘
```

---

## Implementation Guidelines

### Component Library Recommendations

Use the following structure for reusable components:

```
src/
├── components/
│   ├── ui/
│   │   ├── Button/
│   │   │   ├── Button.tsx
│   │   │   ├── Button.styles.ts
│   │   │   └── index.ts
│   │   ├── Input/
│   │   ├── Select/
│   │   ├── Checkbox/
│   │   ├── FileUpload/
│   │   ├── StepIndicator/
│   │   └── Card/
│   ├── forms/
│   │   ├── LoginForm/
│   │   └── RegistrationForm/
│   └── layout/
│       ├── Header/
│       ├── Footer/
│       └── PageLayout/
├── app/ (or pages/)
│   ├── login/
│   ├── register/
│   │   ├── step-1/
│   │   ├── step-2/
│   │   ├── step-3/
│   │   └── step-4/
│   └── ...
└── styles/
    ├── globals.css
    └── variables.css
```

### Form Validation

Implement client-side validation using:
- React Hook Form or Formik
- Zod or Yup for schema validation

### API Integration

```typescript
// API endpoints for registration
POST /api/auth/register/step-1  // Contact person
POST /api/auth/register/step-2  // Business info
POST /api/auth/register/step-3  // Operational details
POST /api/auth/register/step-4  // Banking & documents

// Or single endpoint with step parameter
POST /api/auth/register
Body: { step: 1-4, data: {...} }

// Login endpoint
POST /api/auth/login
Body: { businessPartnerCode, email, password }
```

### Responsive Design

- Mobile first approach
- Breakpoints:
  - Mobile: < 640px (single column)
  - Tablet: 640px - 1024px (flexible columns)
  - Desktop: > 1024px (full layout as designed)

### Accessibility Requirements

- All form fields must have proper labels
- Color contrast ratio minimum 4.5:1
- Keyboard navigation support
- Screen reader friendly error messages
- Focus indicators visible

---

## TypeScript Interfaces

### Registration Data Types

```typescript
// Step 1: Contact Person
interface ContactPerson {
  firstName: string;
  lastName: string;
  email: string;
  alternativeEmail?: string;
  position: string;
  birthDate?: Date;
  gender?: 'male' | 'female' | 'prefer_not_to_say';
  workPhone: string;
  extension?: string;
  mobile: string;
}

// Step 2: Business Information
interface AuthorizedPerson {
  fullName: string;
  tcNumber: string;
  sharePercentage: number;
}

interface Address {
  street: string;
  city: string;
  phone: string;
  website?: string;
}

interface BusinessInfo {
  companyTitle: string;
  taxOffice: string;
  taxNumber: string;
  foundedYear?: number;
  addresses: Address[];
  authorizedPersons: AuthorizedPerson[];
}

// Step 3: Operational Details
interface BusinessPartner {
  companyName: string;
  workingCondition: string;
  creditLimit: number;
}

interface OperationalDetails {
  employeeCount: string;
  businessStructure: string;
  revenueYear?: number;
  targetRevenue?: number;
  customerBase: {
    retailer: number;
    corporate: number;
    construction: number;
    retail: number;
  };
  productCategories: string[];
  currentPartners: BusinessPartner[];
  requestedConditions: string[];
}

// Step 4: Banking & Documents
interface BankAccount {
  bankName: string;
  iban: string;
}

interface Collateral {
  type: string;
  amount: number;
  currency: 'TRY' | 'USD' | 'EUR';
}

interface BankingDocuments {
  bankAccounts: BankAccount[];
  collaterals: Collateral[];
  documents: {
    taxCertificate?: File;
    signatureCircular?: File;
    tradeRegistry?: File;
    partnershipAgreement?: File;
    authorizedIdCopy?: File;
    authorizedResidenceDocument?: File;
  };
}

// Complete Registration
interface DealerRegistration {
  contactPerson: ContactPerson;
  businessInfo: BusinessInfo;
  operationalDetails: OperationalDetails;
  bankingDocuments: BankingDocuments;
}
```

---

## Footer

### Copyright Text

```
Copyright © 2025, Vesmarket Elektronik Hizmetler ve Ticaret A.Ş.
```

---

**Document Version**: 1.0  
**Last Updated**: December 2025  
**Maintained By**: Frontend Development Team
