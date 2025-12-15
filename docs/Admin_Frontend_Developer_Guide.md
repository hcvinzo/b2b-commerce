# Vesmarket Admin Panel - Frontend Developer Guide

> **Version**: 1.0  
> **Project**: admin  
> **Framework**: Next.js 14 (App Router)  
> **UI Library**: shadcn/ui  
> **Styling**: Tailwind CSS  
> **Language**: TypeScript

---

## Repository Structure

```
B2B-COMMERCE/
├── backend/              # .NET Core API (B2B REST API + Integration API)
├── frontend/             # Store UI (Dealer Portal - Next.js)
└── admin/                # Admin Panel (This Project - Next.js)
```

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [Project Setup](#2-project-setup)
3. [Project Structure](#3-project-structure)
4. [Design System](#4-design-system)
5. [Layout Architecture](#5-layout-architecture)
6. [Theme System (Dark/Light/System)](#6-theme-system)
7. [RTL/LTR Support](#7-rtl-ltr-support)
8. [Sidebar Navigation](#8-sidebar-navigation)
9. [Dashboard Implementation](#9-dashboard-implementation)
10. [API Integration](#10-api-integration)
11. [Authentication](#11-authentication)
12. [State Management](#12-state-management)
13. [Form Handling](#13-form-handling)
14. [Component Guidelines](#14-component-guidelines)
15. [Best Practices](#15-best-practices)

---

## 1. Project Overview

The Vesmarket Admin Panel is a comprehensive administration interface for managing the B2B e-commerce platform. It connects to the B2B REST API backend using JWT authentication and provides functionality for managing products, orders, customers, and system settings.

### Key Features

- **Responsive Layout**: Collapsible sidebar with mobile support
- **Theme Support**: Dark, light, and system preference modes
- **Internationalization**: RTL/LTR support for multi-language interfaces
- **Dashboard**: Analytics and key metrics visualization
- **CRUD Operations**: Full management capabilities for all entities

---

## 2. Project Setup

### 2.1 Create Next.js Project

Navigate to the B2B-COMMERCE directory and create the admin project:

```bash
cd B2B-COMMERCE
npx create-next-app@latest admin --typescript --tailwind --eslint --app --src-dir --import-alias "@/*"
```

When prompted:
- Would you like to use TypeScript? **Yes**
- Would you like to use ESLint? **Yes**
- Would you like to use Tailwind CSS? **Yes**
- Would you like to use `src/` directory? **Yes**
- Would you like to use App Router? **Yes**
- Would you like to customize the default import alias? **Yes** → `@/*`

### 2.2 Install shadcn/ui

```bash
cd admin
npx shadcn@latest init
```

Configuration options:
```
Which style would you like to use? › Default
Which color would you like to use as base color? › Slate
Would you like to use CSS variables for colors? › Yes
```

### 2.3 Install Required shadcn Components

```bash
# Core UI components
npx shadcn@latest add button
npx shadcn@latest add card
npx shadcn@latest add dropdown-menu
npx shadcn@latest add avatar
npx shadcn@latest add badge
npx shadcn@latest add input
npx shadcn@latest add label
npx shadcn@latest add select
npx shadcn@latest add table
npx shadcn@latest add tabs
npx shadcn@latest add toast
npx shadcn@latest add dialog
npx shadcn@latest add sheet
npx shadcn@latest add separator
npx shadcn@latest add skeleton
npx shadcn@latest add scroll-area
npx shadcn@latest add tooltip
npx shadcn@latest add collapsible
npx shadcn@latest add sidebar
npx shadcn@latest add chart
npx shadcn@latest add breadcrumb
npx shadcn@latest add command
npx shadcn@latest add popover
npx shadcn@latest add switch
npx shadcn@latest add form
npx shadcn@latest add calendar
npx shadcn@latest add date-picker
```

### 2.4 Install Additional Dependencies

```bash
# State management
npm install zustand

# Data fetching
npm install @tanstack/react-query

# Form handling
npm install react-hook-form @hookform/resolvers zod

# Icons
npm install lucide-react

# Date utilities
npm install date-fns

# Charts (if not included with shadcn chart)
npm install recharts

# Theme
npm install next-themes

# HTTP client
npm install axios

# Cookies for auth
npm install js-cookie
npm install -D @types/js-cookie
```

### 2.5 Environment Configuration

Create `.env.local` in the admin folder:

```env
# API Configuration
# Development: points to local backend
NEXT_PUBLIC_API_URL=http://localhost:5000/api/v1

# Production: update to your deployed API
# NEXT_PUBLIC_API_URL=https://api.vesmarket.com/api/v1

# App Configuration
NEXT_PUBLIC_APP_NAME=Vesmarket Admin

# Feature Flags
NEXT_PUBLIC_ENABLE_ANALYTICS=true
```

> **Note**: The backend API runs from `B2B-COMMERCE/backend/` on port 5000 by default during development.

---

## 3. Project Structure

```
B2B-COMMERCE/
├── backend/                      # .NET Core APIs
├── frontend/                     # Dealer Portal (Store UI)
└── admin/                        # Admin Panel (This Project)
    ├── src/
    │   ├── app/                  # Next.js App Router
    │   │   ├── (auth)/           # Auth routes (no layout)
    │   │   │   ├── login/
    │   │   │   │   └── page.tsx
    │   │   │   └── forgot-password/
    │   │   │       └── page.tsx
    │   │   ├── (dashboard)/      # Dashboard routes (with sidebar)
    │   │   │   ├── layout.tsx    # Dashboard layout with sidebar
    │   │   │   ├── page.tsx      # Dashboard home
    │   │   │   ├── products/
    │   │   │   │   ├── page.tsx          # Product list
    │   │   │   │   ├── [id]/
    │   │   │   │   │   └── page.tsx      # Product detail
    │   │   │   │   └── new/
    │   │   │   │       └── page.tsx      # Create product
    │   │   │   ├── orders/
    │   │   │   │   ├── page.tsx
    │   │   │   │   └── [id]/
    │   │   │   │       └── page.tsx
    │   │   │   ├── customers/
    │   │   │   │   ├── page.tsx
    │   │   │   │   └── [id]/
    │   │   │   │       └── page.tsx
    │   │   │   ├── categories/
    │   │   │   │   └── page.tsx
    │   │   │   ├── users/
    │   │   │   │   └── page.tsx
    │   │   │   └── settings/
    │   │   │       └── page.tsx
    │   │   ├── globals.css
    │   │   ├── layout.tsx        # Root layout
    │   │   └── providers.tsx     # All providers wrapper
    │   │
    │   ├── components/
    │   │   ├── ui/               # shadcn/ui components (auto-generated)
    │   │   │   ├── button.tsx
    │   │   │   ├── card.tsx
    │   │   │   ├── sidebar.tsx
    │   │   │   └── ...
    │   │   ├── layout/           # Layout components
    │   │   │   ├── app-sidebar.tsx       # Main sidebar component
    │   │   │   ├── sidebar-nav.tsx       # Navigation items
    │   │   │   ├── header.tsx            # Top header bar
    │   │   │   ├── breadcrumbs.tsx       # Breadcrumb navigation
    │   │   │   ├── user-nav.tsx          # User menu dropdown
    │   │   │   └── theme-toggle.tsx      # Theme switcher
    │   │   ├── dashboard/        # Dashboard-specific components
    │   │   │   ├── stats-cards.tsx
    │   │   │   ├── recent-orders.tsx
    │   │   │   ├── sales-chart.tsx
    │   │   │   └── top-products.tsx
    │   │   ├── forms/            # Reusable form components
    │   │   │   ├── product-form.tsx
    │   │   │   ├── customer-form.tsx
    │   │   │   └── ...
    │   │   ├── tables/           # Data table components
    │   │   │   ├── data-table.tsx
    │   │   │   ├── columns/
    │   │   │   │   ├── product-columns.tsx
    │   │   │   │   └── order-columns.tsx
    │   │   │   └── ...
    │   │   └── shared/           # Shared components
    │   │       ├── loading-spinner.tsx
    │   │       ├── error-boundary.tsx
    │   │       ├── confirm-dialog.tsx
    │   │       └── ...
    │   │
    │   ├── lib/                  # Utilities and configurations
    │   │   ├── utils.ts          # shadcn utility (cn function)
    │   │   ├── api/              # API layer
    │   │   │   ├── client.ts     # Axios instance
    │   │   │   ├── auth.ts       # Auth API calls
    │   │   │   ├── products.ts   # Product API calls
    │   │   │   ├── orders.ts     # Order API calls
    │   │   │   └── ...
    │   │   ├── validations/      # Zod schemas
    │   │   │   ├── auth.ts
    │   │   │   ├── product.ts
    │   │   │   └── ...
    │   │   └── constants.ts      # App constants
    │   │
    │   ├── hooks/                # Custom React hooks
    │   │   ├── use-auth.ts
    │   │   ├── use-sidebar.ts
    │   │   ├── use-theme.ts
    │   │   └── use-direction.ts
    │   │
    │   ├── stores/               # Zustand stores
    │   │   ├── auth-store.ts
    │   │   ├── sidebar-store.ts
    │   │   └── settings-store.ts
    │   │
    │   ├── types/                # TypeScript types
    │   │   ├── api.ts            # API response types
    │   │   ├── entities.ts       # Domain entities
    │   │   └── navigation.ts     # Navigation types
    │   │
    │   └── config/               # Configuration files
    │       ├── site.ts           # Site configuration
    │       └── navigation.ts     # Navigation config
    │
    ├── public/                   # Static assets
    ├── .env.local                # Environment variables
    ├── next.config.js
    ├── tailwind.config.ts
    ├── tsconfig.json
    └── package.json
```

---

## 4. Design System

### 4.1 Color Palette

Based on the Vesmarket brand guidelines:

```typescript
// tailwind.config.ts
import type { Config } from "tailwindcss"

const config: Config = {
  darkMode: ["class"],
  content: [
    "./src/**/*.{js,ts,jsx,tsx,mdx}",
  ],
  theme: {
    extend: {
      colors: {
        // Brand Colors
        brand: {
          primary: "#4e63a9",      // Primary blue
          dark: "#040404",          // Near black
          gray: "#717171",          // Text gray
          light: "#d7dce6",         // Light accent
          background: "#f7f7f7",    // Background
          white: "#ffffff",         // White
        },
        // shadcn colors (using CSS variables)
        border: "hsl(var(--border))",
        input: "hsl(var(--input))",
        ring: "hsl(var(--ring))",
        background: "hsl(var(--background))",
        foreground: "hsl(var(--foreground))",
        primary: {
          DEFAULT: "hsl(var(--primary))",
          foreground: "hsl(var(--primary-foreground))",
        },
        secondary: {
          DEFAULT: "hsl(var(--secondary))",
          foreground: "hsl(var(--secondary-foreground))",
        },
        destructive: {
          DEFAULT: "hsl(var(--destructive))",
          foreground: "hsl(var(--destructive-foreground))",
        },
        muted: {
          DEFAULT: "hsl(var(--muted))",
          foreground: "hsl(var(--muted-foreground))",
        },
        accent: {
          DEFAULT: "hsl(var(--accent))",
          foreground: "hsl(var(--accent-foreground))",
        },
        popover: {
          DEFAULT: "hsl(var(--popover))",
          foreground: "hsl(var(--popover-foreground))",
        },
        card: {
          DEFAULT: "hsl(var(--card))",
          foreground: "hsl(var(--card-foreground))",
        },
        sidebar: {
          DEFAULT: "hsl(var(--sidebar-background))",
          foreground: "hsl(var(--sidebar-foreground))",
          primary: "hsl(var(--sidebar-primary))",
          "primary-foreground": "hsl(var(--sidebar-primary-foreground))",
          accent: "hsl(var(--sidebar-accent))",
          "accent-foreground": "hsl(var(--sidebar-accent-foreground))",
          border: "hsl(var(--sidebar-border))",
          ring: "hsl(var(--sidebar-ring))",
        },
      },
      fontFamily: {
        sans: ["DM Sans", "system-ui", "sans-serif"],
      },
      borderRadius: {
        lg: "var(--radius)",
        md: "calc(var(--radius) - 2px)",
        sm: "calc(var(--radius) - 4px)",
      },
    },
  },
  plugins: [require("tailwindcss-animate")],
}

export default config
```

### 4.2 CSS Variables

Update `src/app/globals.css`:

```css
@tailwind base;
@tailwind components;
@tailwind utilities;

/* Import DM Sans font */
@import url('https://fonts.googleapis.com/css2?family=DM+Sans:ital,opsz,wght@0,9..40,100..1000;1,9..40,100..1000&display=swap');

@layer base {
  :root {
    /* Vesmarket Brand */
    --brand-primary: 227 48% 48%;      /* #4e63a9 */
    --brand-dark: 0 0% 2%;             /* #040404 */
    --brand-gray: 0 0% 44%;            /* #717171 */
    --brand-light: 220 23% 88%;        /* #d7dce6 */
    --brand-bg: 0 0% 97%;              /* #f7f7f7 */

    /* Light theme */
    --background: 0 0% 97%;            /* #f7f7f7 */
    --foreground: 0 0% 2%;             /* #040404 */
    --card: 0 0% 100%;
    --card-foreground: 0 0% 2%;
    --popover: 0 0% 100%;
    --popover-foreground: 0 0% 2%;
    --primary: 227 48% 48%;            /* #4e63a9 */
    --primary-foreground: 0 0% 100%;
    --secondary: 220 23% 88%;          /* #d7dce6 */
    --secondary-foreground: 0 0% 2%;
    --muted: 220 14% 96%;
    --muted-foreground: 0 0% 44%;      /* #717171 */
    --accent: 220 14% 96%;
    --accent-foreground: 0 0% 2%;
    --destructive: 0 84% 60%;
    --destructive-foreground: 0 0% 100%;
    --border: 220 13% 91%;
    --input: 220 13% 91%;
    --ring: 227 48% 48%;
    --radius: 0.5rem;

    /* Sidebar */
    --sidebar-background: 0 0% 100%;
    --sidebar-foreground: 0 0% 2%;
    --sidebar-primary: 227 48% 48%;
    --sidebar-primary-foreground: 0 0% 100%;
    --sidebar-accent: 220 14% 96%;
    --sidebar-accent-foreground: 0 0% 2%;
    --sidebar-border: 220 13% 91%;
    --sidebar-ring: 227 48% 48%;

    /* Chart colors */
    --chart-1: 227 48% 48%;
    --chart-2: 173 58% 39%;
    --chart-3: 197 37% 24%;
    --chart-4: 43 74% 66%;
    --chart-5: 27 87% 67%;
  }

  .dark {
    --background: 222 47% 11%;
    --foreground: 210 40% 98%;
    --card: 222 47% 13%;
    --card-foreground: 210 40% 98%;
    --popover: 222 47% 13%;
    --popover-foreground: 210 40% 98%;
    --primary: 227 48% 58%;
    --primary-foreground: 0 0% 100%;
    --secondary: 217 33% 17%;
    --secondary-foreground: 210 40% 98%;
    --muted: 217 33% 17%;
    --muted-foreground: 215 20% 65%;
    --accent: 217 33% 17%;
    --accent-foreground: 210 40% 98%;
    --destructive: 0 63% 50%;
    --destructive-foreground: 210 40% 98%;
    --border: 217 33% 17%;
    --input: 217 33% 17%;
    --ring: 227 48% 58%;

    /* Sidebar dark */
    --sidebar-background: 222 47% 8%;
    --sidebar-foreground: 210 40% 98%;
    --sidebar-primary: 227 48% 58%;
    --sidebar-primary-foreground: 0 0% 100%;
    --sidebar-accent: 217 33% 17%;
    --sidebar-accent-foreground: 210 40% 98%;
    --sidebar-border: 217 33% 17%;
    --sidebar-ring: 227 48% 58%;

    /* Chart colors dark */
    --chart-1: 220 70% 50%;
    --chart-2: 160 60% 45%;
    --chart-3: 30 80% 55%;
    --chart-4: 280 65% 60%;
    --chart-5: 340 75% 55%;
  }
}

@layer base {
  * {
    @apply border-border;
  }
  body {
    @apply bg-background text-foreground font-sans;
  }
}

/* RTL Support */
[dir="rtl"] {
  direction: rtl;
}

[dir="rtl"] .sidebar-icon {
  transform: scaleX(-1);
}
```

---

## 5. Layout Architecture

### 5.1 Root Layout

```typescript
// src/app/layout.tsx
import type { Metadata } from "next"
import { DM_Sans } from "next/font/google"
import "./globals.css"
import { Providers } from "./providers"

const dmSans = DM_Sans({
  subsets: ["latin"],
  variable: "--font-dm-sans",
})

export const metadata: Metadata = {
  title: {
    default: "Vesmarket Admin",
    template: "%s | Vesmarket Admin",
  },
  description: "B2B E-Commerce Administration Panel",
}

export default function RootLayout({
  children,
}: {
  children: React.ReactNode
}) {
  return (
    <html lang="en" suppressHydrationWarning>
      <body className={`${dmSans.variable} font-sans antialiased`}>
        <Providers>
          {children}
        </Providers>
      </body>
    </html>
  )
}
```

### 5.2 Providers Wrapper

```typescript
// src/app/providers.tsx
"use client"

import { ThemeProvider } from "next-themes"
import { QueryClient, QueryClientProvider } from "@tanstack/react-query"
import { ReactQueryDevtools } from "@tanstack/react-query-devtools"
import { useState } from "react"
import { DirectionProvider } from "@/hooks/use-direction"
import { Toaster } from "@/components/ui/toaster"

export function Providers({ children }: { children: React.ReactNode }) {
  const [queryClient] = useState(
    () =>
      new QueryClient({
        defaultOptions: {
          queries: {
            staleTime: 60 * 1000, // 1 minute
            refetchOnWindowFocus: false,
          },
        },
      })
  )

  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider
        attribute="class"
        defaultTheme="system"
        enableSystem
        disableTransitionOnChange
      >
        <DirectionProvider>
          {children}
          <Toaster />
        </DirectionProvider>
      </ThemeProvider>
      <ReactQueryDevtools initialIsOpen={false} />
    </QueryClientProvider>
  )
}
```

### 5.3 Dashboard Layout

```typescript
// src/app/(dashboard)/layout.tsx
import { cookies } from "next/headers"
import { SidebarInset, SidebarProvider } from "@/components/ui/sidebar"
import { AppSidebar } from "@/components/layout/app-sidebar"
import { Header } from "@/components/layout/header"

export default async function DashboardLayout({
  children,
}: {
  children: React.ReactNode
}) {
  const cookieStore = await cookies()
  const defaultOpen = cookieStore.get("sidebar:state")?.value === "true"

  return (
    <SidebarProvider defaultOpen={defaultOpen}>
      <AppSidebar />
      <SidebarInset>
        <Header />
        <main className="flex-1 overflow-auto p-4 md:p-6">
          {children}
        </main>
      </SidebarInset>
    </SidebarProvider>
  )
}
```

---

## 6. Theme System

### 6.1 Theme Toggle Component

```typescript
// src/components/layout/theme-toggle.tsx
"use client"

import * as React from "react"
import { Moon, Sun, Monitor } from "lucide-react"
import { useTheme } from "next-themes"

import { Button } from "@/components/ui/button"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"

export function ThemeToggle() {
  const { theme, setTheme } = useTheme()
  const [mounted, setMounted] = React.useState(false)

  React.useEffect(() => {
    setMounted(true)
  }, [])

  if (!mounted) {
    return (
      <Button variant="ghost" size="icon" className="h-9 w-9">
        <Sun className="h-4 w-4" />
      </Button>
    )
  }

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="icon" className="h-9 w-9">
          {theme === "dark" ? (
            <Moon className="h-4 w-4" />
          ) : theme === "light" ? (
            <Sun className="h-4 w-4" />
          ) : (
            <Monitor className="h-4 w-4" />
          )}
          <span className="sr-only">Toggle theme</span>
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuItem onClick={() => setTheme("light")}>
          <Sun className="mr-2 h-4 w-4" />
          <span>Light</span>
        </DropdownMenuItem>
        <DropdownMenuItem onClick={() => setTheme("dark")}>
          <Moon className="mr-2 h-4 w-4" />
          <span>Dark</span>
        </DropdownMenuItem>
        <DropdownMenuItem onClick={() => setTheme("system")}>
          <Monitor className="mr-2 h-4 w-4" />
          <span>System</span>
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  )
}
```

### 6.2 Using Theme in Components

```typescript
// Example: Theme-aware component
"use client"

import { useTheme } from "next-themes"

export function ThemeAwareChart() {
  const { theme, resolvedTheme } = useTheme()
  
  // resolvedTheme gives actual theme when "system" is selected
  const isDark = resolvedTheme === "dark"
  
  return (
    <div className={isDark ? "chart-dark" : "chart-light"}>
      {/* Chart content */}
    </div>
  )
}
```

---

## 7. RTL/LTR Support

### 7.1 Direction Provider

```typescript
// src/hooks/use-direction.tsx
"use client"

import React, { createContext, useContext, useState, useEffect } from "react"

type Direction = "ltr" | "rtl"

interface DirectionContextType {
  direction: Direction
  setDirection: (dir: Direction) => void
  toggleDirection: () => void
}

const DirectionContext = createContext<DirectionContextType | undefined>(undefined)

export function DirectionProvider({ children }: { children: React.ReactNode }) {
  const [direction, setDirectionState] = useState<Direction>("ltr")

  useEffect(() => {
    // Load from localStorage on mount
    const saved = localStorage.getItem("app-direction") as Direction
    if (saved) {
      setDirectionState(saved)
      document.documentElement.dir = saved
    }
  }, [])

  const setDirection = (dir: Direction) => {
    setDirectionState(dir)
    document.documentElement.dir = dir
    localStorage.setItem("app-direction", dir)
  }

  const toggleDirection = () => {
    const newDir = direction === "ltr" ? "rtl" : "ltr"
    setDirection(newDir)
  }

  return (
    <DirectionContext.Provider value={{ direction, setDirection, toggleDirection }}>
      {children}
    </DirectionContext.Provider>
  )
}

export function useDirection() {
  const context = useContext(DirectionContext)
  if (context === undefined) {
    throw new Error("useDirection must be used within a DirectionProvider")
  }
  return context
}
```

### 7.2 Direction Toggle Component

```typescript
// src/components/layout/direction-toggle.tsx
"use client"

import { Languages } from "lucide-react"
import { Button } from "@/components/ui/button"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import { useDirection } from "@/hooks/use-direction"

export function DirectionToggle() {
  const { direction, setDirection } = useDirection()

  return (
    <DropdownMenu>
      <DropdownMenuTrigger asChild>
        <Button variant="ghost" size="icon" className="h-9 w-9">
          <Languages className="h-4 w-4" />
          <span className="sr-only">Toggle direction</span>
        </Button>
      </DropdownMenuTrigger>
      <DropdownMenuContent align="end">
        <DropdownMenuItem 
          onClick={() => setDirection("ltr")}
          className={direction === "ltr" ? "bg-accent" : ""}
        >
          <span className="mr-2">🇬🇧</span>
          Left to Right (LTR)
        </DropdownMenuItem>
        <DropdownMenuItem 
          onClick={() => setDirection("rtl")}
          className={direction === "rtl" ? "bg-accent" : ""}
        >
          <span className="mr-2">🇸🇦</span>
          Right to Left (RTL)
        </DropdownMenuItem>
      </DropdownMenuContent>
    </DropdownMenu>
  )
}
```

### 7.3 RTL-Aware Utilities

```typescript
// src/lib/utils.ts
import { clsx, type ClassValue } from "clsx"
import { twMerge } from "tailwind-merge"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}

// RTL-aware spacing utilities
export function rtlAware(ltrClass: string, rtlClass: string) {
  return `${ltrClass} rtl:${rtlClass}`
}

// Common RTL transformations
export const rtl = {
  // Margins
  ml: (value: string) => `ml-${value} rtl:mr-${value} rtl:ml-0`,
  mr: (value: string) => `mr-${value} rtl:ml-${value} rtl:mr-0`,
  // Padding
  pl: (value: string) => `pl-${value} rtl:pr-${value} rtl:pl-0`,
  pr: (value: string) => `pr-${value} rtl:pl-${value} rtl:pr-0`,
  // Text alignment
  textLeft: "text-left rtl:text-right",
  textRight: "text-right rtl:text-left",
  // Positioning
  left: (value: string) => `left-${value} rtl:right-${value} rtl:left-auto`,
  right: (value: string) => `right-${value} rtl:left-${value} rtl:right-auto`,
  // Borders
  borderL: (value: string) => `border-l-${value} rtl:border-r-${value} rtl:border-l-0`,
  borderR: (value: string) => `border-r-${value} rtl:border-l-${value} rtl:border-r-0`,
  // Rounded corners
  roundedL: (value: string) => `rounded-l-${value} rtl:rounded-r-${value} rtl:rounded-l-none`,
  roundedR: (value: string) => `rounded-r-${value} rtl:rounded-l-${value} rtl:rounded-r-none`,
}
```

### 7.4 Tailwind RTL Plugin Configuration

```typescript
// tailwind.config.ts - add RTL plugin
import type { Config } from "tailwindcss"
import plugin from "tailwindcss/plugin"

const config: Config = {
  // ... other config
  plugins: [
    require("tailwindcss-animate"),
    // RTL support plugin
    plugin(function({ addVariant }) {
      addVariant('rtl', '[dir="rtl"] &')
      addVariant('ltr', '[dir="ltr"] &')
    }),
  ],
}

export default config
```

---

## 8. Sidebar Navigation

### 8.1 Navigation Configuration

```typescript
// src/config/navigation.ts
import {
  LayoutDashboard,
  Package,
  ShoppingCart,
  Users,
  FolderTree,
  Settings,
  BarChart3,
  CreditCard,
  Truck,
  FileText,
  UserCog,
  Building2,
  type LucideIcon,
} from "lucide-react"

export interface NavItem {
  title: string
  url: string
  icon: LucideIcon
  badge?: string | number
  isActive?: boolean
  items?: NavSubItem[]
}

export interface NavSubItem {
  title: string
  url: string
  badge?: string | number
}

export interface NavGroup {
  label: string
  items: NavItem[]
}

export const navigation: NavGroup[] = [
  {
    label: "Overview",
    items: [
      {
        title: "Dashboard",
        url: "/",
        icon: LayoutDashboard,
      },
      {
        title: "Analytics",
        url: "/analytics",
        icon: BarChart3,
      },
    ],
  },
  {
    label: "Catalog",
    items: [
      {
        title: "Products",
        url: "/products",
        icon: Package,
        items: [
          { title: "All Products", url: "/products" },
          { title: "Add Product", url: "/products/new" },
          { title: "Import/Export", url: "/products/import" },
        ],
      },
      {
        title: "Categories",
        url: "/categories",
        icon: FolderTree,
      },
    ],
  },
  {
    label: "Sales",
    items: [
      {
        title: "Orders",
        url: "/orders",
        icon: ShoppingCart,
        badge: 12,
        items: [
          { title: "All Orders", url: "/orders" },
          { title: "Pending Approval", url: "/orders/pending", badge: 5 },
          { title: "Processing", url: "/orders/processing" },
          { title: "Completed", url: "/orders/completed" },
        ],
      },
      {
        title: "Returns",
        url: "/returns",
        icon: Truck,
      },
      {
        title: "Invoices",
        url: "/invoices",
        icon: FileText,
      },
    ],
  },
  {
    label: "Customers",
    items: [
      {
        title: "Dealers",
        url: "/customers",
        icon: Building2,
        items: [
          { title: "All Dealers", url: "/customers" },
          { title: "Pending Approval", url: "/customers/pending" },
          { title: "Credit Management", url: "/customers/credits" },
        ],
      },
      {
        title: "Users",
        url: "/users",
        icon: Users,
      },
    ],
  },
  {
    label: "Finance",
    items: [
      {
        title: "Payments",
        url: "/payments",
        icon: CreditCard,
      },
    ],
  },
  {
    label: "System",
    items: [
      {
        title: "Admin Users",
        url: "/admin-users",
        icon: UserCog,
      },
      {
        title: "Settings",
        url: "/settings",
        icon: Settings,
      },
    ],
  },
]
```

### 8.2 App Sidebar Component

```typescript
// src/components/layout/app-sidebar.tsx
"use client"

import * as React from "react"
import { usePathname } from "next/navigation"
import Link from "next/link"
import {
  ChevronRight,
  ChevronsUpDown,
  LogOut,
  type LucideIcon,
} from "lucide-react"

import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarHeader,
  SidebarMenu,
  SidebarMenuAction,
  SidebarMenuBadge,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarMenuSub,
  SidebarMenuSubButton,
  SidebarMenuSubItem,
  SidebarRail,
  useSidebar,
} from "@/components/ui/sidebar"
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu"
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible"
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar"
import { navigation, type NavItem, type NavGroup } from "@/config/navigation"
import { useAuthStore } from "@/stores/auth-store"

// Logo Component
function SidebarLogo() {
  const { state } = useSidebar()
  
  return (
    <SidebarHeader className="border-b border-sidebar-border">
      <SidebarMenu>
        <SidebarMenuItem>
          <SidebarMenuButton size="lg" asChild>
            <Link href="/">
              <div className="flex aspect-square size-8 items-center justify-center rounded-lg bg-primary text-primary-foreground">
                <span className="font-bold text-lg">V</span>
              </div>
              <div className="flex flex-col gap-0.5 leading-none">
                <span className="font-semibold">Vesmarket</span>
                <span className="text-xs text-muted-foreground">Admin Panel</span>
              </div>
            </Link>
          </SidebarMenuButton>
        </SidebarMenuItem>
      </SidebarMenu>
    </SidebarHeader>
  )
}

// Navigation Item Component
function NavItemComponent({ item }: { item: NavItem }) {
  const pathname = usePathname()
  const isActive = pathname === item.url || pathname.startsWith(`${item.url}/`)
  const hasSubItems = item.items && item.items.length > 0

  if (hasSubItems) {
    return (
      <Collapsible asChild defaultOpen={isActive} className="group/collapsible">
        <SidebarMenuItem>
          <CollapsibleTrigger asChild>
            <SidebarMenuButton tooltip={item.title}>
              <item.icon />
              <span>{item.title}</span>
              {item.badge && (
                <SidebarMenuBadge>{item.badge}</SidebarMenuBadge>
              )}
              <ChevronRight className="ml-auto transition-transform duration-200 group-data-[state=open]/collapsible:rotate-90" />
            </SidebarMenuButton>
          </CollapsibleTrigger>
          <CollapsibleContent>
            <SidebarMenuSub>
              {item.items?.map((subItem) => (
                <SidebarMenuSubItem key={subItem.url}>
                  <SidebarMenuSubButton
                    asChild
                    isActive={pathname === subItem.url}
                  >
                    <Link href={subItem.url}>
                      <span>{subItem.title}</span>
                      {subItem.badge && (
                        <SidebarMenuBadge>{subItem.badge}</SidebarMenuBadge>
                      )}
                    </Link>
                  </SidebarMenuSubButton>
                </SidebarMenuSubItem>
              ))}
            </SidebarMenuSub>
          </CollapsibleContent>
        </SidebarMenuItem>
      </Collapsible>
    )
  }

  return (
    <SidebarMenuItem>
      <SidebarMenuButton asChild isActive={isActive} tooltip={item.title}>
        <Link href={item.url}>
          <item.icon />
          <span>{item.title}</span>
        </Link>
      </SidebarMenuButton>
      {item.badge && <SidebarMenuBadge>{item.badge}</SidebarMenuBadge>}
    </SidebarMenuItem>
  )
}

// User Menu Component
function UserMenu() {
  const { user, logout } = useAuthStore()
  
  return (
    <SidebarFooter>
      <SidebarMenu>
        <SidebarMenuItem>
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <SidebarMenuButton
                size="lg"
                className="data-[state=open]:bg-sidebar-accent data-[state=open]:text-sidebar-accent-foreground"
              >
                <Avatar className="h-8 w-8 rounded-lg">
                  <AvatarImage
                    src={user?.avatarUrl}
                    alt={user?.name || "User"}
                  />
                  <AvatarFallback className="rounded-lg">
                    {user?.name?.charAt(0) || "A"}
                  </AvatarFallback>
                </Avatar>
                <div className="grid flex-1 text-left text-sm leading-tight">
                  <span className="truncate font-semibold">
                    {user?.name || "Admin User"}
                  </span>
                  <span className="truncate text-xs text-muted-foreground">
                    {user?.email || "admin@vesmarket.com"}
                  </span>
                </div>
                <ChevronsUpDown className="ml-auto size-4" />
              </SidebarMenuButton>
            </DropdownMenuTrigger>
            <DropdownMenuContent
              className="w-[--radix-dropdown-menu-trigger-width] min-w-56 rounded-lg"
              side="bottom"
              align="end"
              sideOffset={4}
            >
              <DropdownMenuLabel className="p-0 font-normal">
                <div className="flex items-center gap-2 px-1 py-1.5 text-left text-sm">
                  <Avatar className="h-8 w-8 rounded-lg">
                    <AvatarImage src={user?.avatarUrl} />
                    <AvatarFallback className="rounded-lg">
                      {user?.name?.charAt(0) || "A"}
                    </AvatarFallback>
                  </Avatar>
                  <div className="grid flex-1 text-left text-sm leading-tight">
                    <span className="truncate font-semibold">
                      {user?.name || "Admin User"}
                    </span>
                    <span className="truncate text-xs text-muted-foreground">
                      {user?.email}
                    </span>
                  </div>
                </div>
              </DropdownMenuLabel>
              <DropdownMenuSeparator />
              <DropdownMenuItem onClick={logout}>
                <LogOut className="mr-2 h-4 w-4" />
                Log out
              </DropdownMenuItem>
            </DropdownMenuContent>
          </DropdownMenu>
        </SidebarMenuItem>
      </SidebarMenu>
    </SidebarFooter>
  )
}

// Main Sidebar Component
export function AppSidebar({ ...props }: React.ComponentProps<typeof Sidebar>) {
  return (
    <Sidebar collapsible="icon" {...props}>
      <SidebarLogo />
      <SidebarContent>
        {navigation.map((group) => (
          <SidebarGroup key={group.label}>
            <SidebarGroupLabel>{group.label}</SidebarGroupLabel>
            <SidebarGroupContent>
              <SidebarMenu>
                {group.items.map((item) => (
                  <NavItemComponent key={item.url} item={item} />
                ))}
              </SidebarMenu>
            </SidebarGroupContent>
          </SidebarGroup>
        ))}
      </SidebarContent>
      <UserMenu />
      <SidebarRail />
    </Sidebar>
  )
}
```

### 8.3 Header Component

```typescript
// src/components/layout/header.tsx
"use client"

import { SidebarTrigger } from "@/components/ui/sidebar"
import { Separator } from "@/components/ui/separator"
import { Breadcrumbs } from "@/components/layout/breadcrumbs"
import { ThemeToggle } from "@/components/layout/theme-toggle"
import { DirectionToggle } from "@/components/layout/direction-toggle"
import { UserNav } from "@/components/layout/user-nav"
import { Button } from "@/components/ui/button"
import { Bell, Search } from "lucide-react"
import { Input } from "@/components/ui/input"

export function Header() {
  return (
    <header className="flex h-16 shrink-0 items-center gap-2 border-b px-4">
      <SidebarTrigger className="-ml-1" />
      <Separator orientation="vertical" className="mr-2 h-4" />
      <Breadcrumbs />
      
      <div className="ml-auto flex items-center gap-2">
        {/* Search */}
        <div className="hidden md:flex relative">
          <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
          <Input
            type="search"
            placeholder="Search..."
            className="w-[200px] lg:w-[300px] pl-8"
          />
        </div>
        
        {/* Notifications */}
        <Button variant="ghost" size="icon" className="relative">
          <Bell className="h-4 w-4" />
          <span className="absolute -top-1 -right-1 h-4 w-4 rounded-full bg-destructive text-[10px] font-medium text-destructive-foreground flex items-center justify-center">
            3
          </span>
        </Button>
        
        {/* Direction Toggle */}
        <DirectionToggle />
        
        {/* Theme Toggle */}
        <ThemeToggle />
        
        {/* User Menu */}
        <UserNav />
      </div>
    </header>
  )
}
```

### 8.4 Breadcrumbs Component

```typescript
// src/components/layout/breadcrumbs.tsx
"use client"

import { usePathname } from "next/navigation"
import Link from "next/link"
import { ChevronRight, Home } from "lucide-react"
import {
  Breadcrumb,
  BreadcrumbItem,
  BreadcrumbLink,
  BreadcrumbList,
  BreadcrumbPage,
  BreadcrumbSeparator,
} from "@/components/ui/breadcrumb"

// Map of path segments to readable names
const pathNames: Record<string, string> = {
  products: "Products",
  orders: "Orders",
  customers: "Customers",
  categories: "Categories",
  users: "Users",
  settings: "Settings",
  analytics: "Analytics",
  payments: "Payments",
  returns: "Returns",
  invoices: "Invoices",
  new: "Create New",
  edit: "Edit",
  pending: "Pending",
  processing: "Processing",
  completed: "Completed",
  credits: "Credit Management",
  import: "Import/Export",
}

export function Breadcrumbs() {
  const pathname = usePathname()
  const segments = pathname.split("/").filter(Boolean)
  
  if (segments.length === 0) {
    return null
  }

  return (
    <Breadcrumb>
      <BreadcrumbList>
        <BreadcrumbItem>
          <BreadcrumbLink asChild>
            <Link href="/">
              <Home className="h-4 w-4" />
            </Link>
          </BreadcrumbLink>
        </BreadcrumbItem>
        
        {segments.map((segment, index) => {
          const href = `/${segments.slice(0, index + 1).join("/")}`
          const isLast = index === segments.length - 1
          const name = pathNames[segment] || segment
          
          // Check if segment is an ID (number or UUID)
          const isId = /^[0-9]+$/.test(segment) || 
                       /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(segment)
          
          return (
            <BreadcrumbItem key={href}>
              <BreadcrumbSeparator>
                <ChevronRight className="h-4 w-4" />
              </BreadcrumbSeparator>
              {isLast ? (
                <BreadcrumbPage>
                  {isId ? `#${segment.slice(0, 8)}...` : name}
                </BreadcrumbPage>
              ) : (
                <BreadcrumbLink asChild>
                  <Link href={href}>
                    {isId ? `#${segment.slice(0, 8)}...` : name}
                  </Link>
                </BreadcrumbLink>
              )}
            </BreadcrumbItem>
          )
        })}
      </BreadcrumbList>
    </Breadcrumb>
  )
}
```

---

## 9. Dashboard Implementation

### 9.1 Dashboard Page

```typescript
// src/app/(dashboard)/page.tsx
import { Suspense } from "react"
import { StatsCards } from "@/components/dashboard/stats-cards"
import { RecentOrders } from "@/components/dashboard/recent-orders"
import { SalesChart } from "@/components/dashboard/sales-chart"
import { TopProducts } from "@/components/dashboard/top-products"
import { Skeleton } from "@/components/ui/skeleton"

export default function DashboardPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Dashboard</h1>
        <p className="text-muted-foreground">
          Welcome back! Here&apos;s an overview of your business.
        </p>
      </div>

      {/* Stats Cards */}
      <Suspense fallback={<StatsCardsSkeleton />}>
        <StatsCards />
      </Suspense>

      {/* Charts Row */}
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-7">
        <Suspense fallback={<ChartSkeleton />}>
          <SalesChart className="col-span-4" />
        </Suspense>
        <Suspense fallback={<ChartSkeleton />}>
          <TopProducts className="col-span-3" />
        </Suspense>
      </div>

      {/* Recent Orders */}
      <Suspense fallback={<TableSkeleton />}>
        <RecentOrders />
      </Suspense>
    </div>
  )
}

function StatsCardsSkeleton() {
  return (
    <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
      {[...Array(4)].map((_, i) => (
        <Skeleton key={i} className="h-32" />
      ))}
    </div>
  )
}

function ChartSkeleton() {
  return <Skeleton className="h-[350px]" />
}

function TableSkeleton() {
  return <Skeleton className="h-[400px]" />
}
```

### 9.2 Stats Cards Component

```typescript
// src/components/dashboard/stats-cards.tsx
"use client"

import { useQuery } from "@tanstack/react-query"
import {
  DollarSign,
  ShoppingCart,
  Users,
  Package,
  TrendingUp,
  TrendingDown,
} from "lucide-react"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { cn } from "@/lib/utils"
import { getDashboardStats } from "@/lib/api/dashboard"

interface StatCardProps {
  title: string
  value: string
  change: number
  icon: React.ReactNode
}

function StatCard({ title, value, change, icon }: StatCardProps) {
  const isPositive = change >= 0
  
  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
        <CardTitle className="text-sm font-medium">{title}</CardTitle>
        <div className="h-8 w-8 rounded-md bg-primary/10 flex items-center justify-center text-primary">
          {icon}
        </div>
      </CardHeader>
      <CardContent>
        <div className="text-2xl font-bold">{value}</div>
        <div className="flex items-center text-xs text-muted-foreground">
          {isPositive ? (
            <TrendingUp className="mr-1 h-3 w-3 text-green-500" />
          ) : (
            <TrendingDown className="mr-1 h-3 w-3 text-red-500" />
          )}
          <span className={cn(isPositive ? "text-green-500" : "text-red-500")}>
            {isPositive ? "+" : ""}{change}%
          </span>
          <span className="ml-1">from last month</span>
        </div>
      </CardContent>
    </Card>
  )
}

export function StatsCards() {
  const { data: stats, isLoading } = useQuery({
    queryKey: ["dashboard-stats"],
    queryFn: getDashboardStats,
  })

  if (isLoading) {
    return (
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {[...Array(4)].map((_, i) => (
          <Card key={i}>
            <CardContent className="h-32 animate-pulse bg-muted" />
          </Card>
        ))}
      </div>
    )
  }

  const cards = [
    {
      title: "Total Revenue",
      value: `$${stats?.totalRevenue?.toLocaleString() || "0"}`,
      change: stats?.revenueChange || 0,
      icon: <DollarSign className="h-4 w-4" />,
    },
    {
      title: "Orders",
      value: stats?.totalOrders?.toLocaleString() || "0",
      change: stats?.ordersChange || 0,
      icon: <ShoppingCart className="h-4 w-4" />,
    },
    {
      title: "Customers",
      value: stats?.totalCustomers?.toLocaleString() || "0",
      change: stats?.customersChange || 0,
      icon: <Users className="h-4 w-4" />,
    },
    {
      title: "Products",
      value: stats?.activeProducts?.toLocaleString() || "0",
      change: stats?.productsChange || 0,
      icon: <Package className="h-4 w-4" />,
    },
  ]

  return (
    <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
      {cards.map((card) => (
        <StatCard key={card.title} {...card} />
      ))}
    </div>
  )
}
```

### 9.3 Sales Chart Component

```typescript
// src/components/dashboard/sales-chart.tsx
"use client"

import { useQuery } from "@tanstack/react-query"
import {
  Area,
  AreaChart,
  ResponsiveContainer,
  XAxis,
  YAxis,
  Tooltip,
  CartesianGrid,
} from "recharts"
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import {
  ChartConfig,
  ChartContainer,
  ChartTooltip,
  ChartTooltipContent,
} from "@/components/ui/chart"
import { getSalesData } from "@/lib/api/dashboard"
import { cn } from "@/lib/utils"

const chartConfig = {
  sales: {
    label: "Sales",
    color: "hsl(var(--chart-1))",
  },
  orders: {
    label: "Orders",
    color: "hsl(var(--chart-2))",
  },
} satisfies ChartConfig

interface SalesChartProps {
  className?: string
}

export function SalesChart({ className }: SalesChartProps) {
  const { data: salesData, isLoading } = useQuery({
    queryKey: ["sales-chart"],
    queryFn: getSalesData,
  })

  return (
    <Card className={cn(className)}>
      <CardHeader>
        <CardTitle>Sales Overview</CardTitle>
        <CardDescription>
          Monthly sales and orders for the current year
        </CardDescription>
      </CardHeader>
      <CardContent className="pb-4">
        {isLoading ? (
          <div className="h-[300px] animate-pulse bg-muted rounded" />
        ) : (
          <ChartContainer config={chartConfig} className="h-[300px]">
            <AreaChart
              data={salesData}
              margin={{ top: 10, right: 30, left: 0, bottom: 0 }}
            >
              <CartesianGrid strokeDasharray="3 3" className="stroke-muted" />
              <XAxis
                dataKey="month"
                tickLine={false}
                axisLine={false}
                tickMargin={8}
                className="text-xs"
              />
              <YAxis
                tickLine={false}
                axisLine={false}
                tickMargin={8}
                tickFormatter={(value) => `$${value / 1000}k`}
                className="text-xs"
              />
              <ChartTooltip content={<ChartTooltipContent />} />
              <Area
                type="monotone"
                dataKey="sales"
                stackId="1"
                stroke="var(--color-sales)"
                fill="var(--color-sales)"
                fillOpacity={0.2}
              />
              <Area
                type="monotone"
                dataKey="orders"
                stackId="2"
                stroke="var(--color-orders)"
                fill="var(--color-orders)"
                fillOpacity={0.2}
              />
            </AreaChart>
          </ChartContainer>
        )}
      </CardContent>
    </Card>
  )
}
```

### 9.4 Recent Orders Component

```typescript
// src/components/dashboard/recent-orders.tsx
"use client"

import { useQuery } from "@tanstack/react-query"
import Link from "next/link"
import { formatDistanceToNow } from "date-fns"
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table"
import { Badge } from "@/components/ui/badge"
import { Button } from "@/components/ui/button"
import { ArrowRight } from "lucide-react"
import { getRecentOrders } from "@/lib/api/orders"

const statusColors: Record<string, string> = {
  pending: "bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300",
  processing: "bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300",
  shipped: "bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-300",
  delivered: "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300",
  cancelled: "bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300",
}

export function RecentOrders() {
  const { data: orders, isLoading } = useQuery({
    queryKey: ["recent-orders"],
    queryFn: () => getRecentOrders(5),
  })

  return (
    <Card>
      <CardHeader className="flex flex-row items-center justify-between">
        <div>
          <CardTitle>Recent Orders</CardTitle>
          <CardDescription>
            Latest orders from your customers
          </CardDescription>
        </div>
        <Button variant="outline" size="sm" asChild>
          <Link href="/orders">
            View All
            <ArrowRight className="ml-2 h-4 w-4" />
          </Link>
        </Button>
      </CardHeader>
      <CardContent>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Order ID</TableHead>
              <TableHead>Customer</TableHead>
              <TableHead>Status</TableHead>
              <TableHead className="text-right">Amount</TableHead>
              <TableHead>Date</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {isLoading ? (
              [...Array(5)].map((_, i) => (
                <TableRow key={i}>
                  <TableCell colSpan={5}>
                    <div className="h-8 animate-pulse bg-muted rounded" />
                  </TableCell>
                </TableRow>
              ))
            ) : (
              orders?.map((order) => (
                <TableRow key={order.id}>
                  <TableCell className="font-medium">
                    <Link
                      href={`/orders/${order.id}`}
                      className="hover:underline text-primary"
                    >
                      #{order.orderNumber}
                    </Link>
                  </TableCell>
                  <TableCell>{order.customerName}</TableCell>
                  <TableCell>
                    <Badge
                      variant="secondary"
                      className={statusColors[order.status] || ""}
                    >
                      {order.status}
                    </Badge>
                  </TableCell>
                  <TableCell className="text-right">
                    ${order.total.toLocaleString()}
                  </TableCell>
                  <TableCell className="text-muted-foreground">
                    {formatDistanceToNow(new Date(order.createdAt), {
                      addSuffix: true,
                    })}
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </CardContent>
    </Card>
  )
}
```

---

## 10. API Integration

### 10.1 Axios Client Setup

```typescript
// src/lib/api/client.ts
import axios, { AxiosError, InternalAxiosRequestConfig } from "axios"
import Cookies from "js-cookie"

const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000/api/v1"

export const apiClient = axios.create({
  baseURL: API_URL,
  headers: {
    "Content-Type": "application/json",
  },
})

// Request interceptor - add auth token
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = Cookies.get("accessToken")
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    return config
  },
  (error) => Promise.reject(error)
)

// Response interceptor - handle token refresh
apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & {
      _retry?: boolean
    }

    // If 401 and not already retrying
    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true

      try {
        const refreshToken = Cookies.get("refreshToken")
        if (refreshToken) {
          const response = await axios.post(`${API_URL}/auth/refresh`, {
            refreshToken,
          })

          const { accessToken, refreshToken: newRefreshToken } = response.data

          Cookies.set("accessToken", accessToken, { secure: true, sameSite: "strict" })
          Cookies.set("refreshToken", newRefreshToken, { secure: true, sameSite: "strict" })

          originalRequest.headers.Authorization = `Bearer ${accessToken}`
          return apiClient(originalRequest)
        }
      } catch (refreshError) {
        // Refresh failed - redirect to login
        Cookies.remove("accessToken")
        Cookies.remove("refreshToken")
        window.location.href = "/login"
        return Promise.reject(refreshError)
      }
    }

    return Promise.reject(error)
  }
)

// Generic API response type
export interface ApiResponse<T> {
  data: T
  message?: string
  success: boolean
}

// Paginated response type
export interface PaginatedResponse<T> {
  items: T[]
  pageNumber: number
  pageSize: number
  totalCount: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}
```

### 10.2 API Service Example (Products)

```typescript
// src/lib/api/products.ts
import { apiClient, PaginatedResponse, ApiResponse } from "./client"
import { Product, ProductCreateDto, ProductUpdateDto, ProductFilters } from "@/types/entities"

const PRODUCTS_BASE = "/products"

export async function getProducts(
  filters: ProductFilters
): Promise<PaginatedResponse<Product>> {
  const params = new URLSearchParams()
  
  if (filters.page) params.append("pageNumber", filters.page.toString())
  if (filters.pageSize) params.append("pageSize", filters.pageSize.toString())
  if (filters.search) params.append("search", filters.search)
  if (filters.categoryId) params.append("categoryId", filters.categoryId.toString())
  if (filters.brandId) params.append("brandId", filters.brandId.toString())
  if (filters.isActive !== undefined) params.append("isActive", filters.isActive.toString())
  if (filters.sortBy) params.append("sortBy", filters.sortBy)
  if (filters.sortOrder) params.append("sortOrder", filters.sortOrder)

  const response = await apiClient.get<PaginatedResponse<Product>>(
    `${PRODUCTS_BASE}?${params.toString()}`
  )
  return response.data
}

export async function getProduct(id: string): Promise<Product> {
  const response = await apiClient.get<Product>(`${PRODUCTS_BASE}/${id}`)
  return response.data
}

export async function createProduct(data: ProductCreateDto): Promise<Product> {
  const response = await apiClient.post<Product>(PRODUCTS_BASE, data)
  return response.data
}

export async function updateProduct(
  id: string,
  data: ProductUpdateDto
): Promise<Product> {
  const response = await apiClient.put<Product>(`${PRODUCTS_BASE}/${id}`, data)
  return response.data
}

export async function deleteProduct(id: string): Promise<void> {
  await apiClient.delete(`${PRODUCTS_BASE}/${id}`)
}

export async function toggleProductStatus(id: string): Promise<Product> {
  const response = await apiClient.patch<Product>(
    `${PRODUCTS_BASE}/${id}/toggle-status`
  )
  return response.data
}
```

### 10.3 React Query Hooks

```typescript
// src/hooks/use-products.ts
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query"
import { useToast } from "@/hooks/use-toast"
import {
  getProducts,
  getProduct,
  createProduct,
  updateProduct,
  deleteProduct,
} from "@/lib/api/products"
import { ProductFilters, ProductCreateDto, ProductUpdateDto } from "@/types/entities"

export const productKeys = {
  all: ["products"] as const,
  lists: () => [...productKeys.all, "list"] as const,
  list: (filters: ProductFilters) => [...productKeys.lists(), filters] as const,
  details: () => [...productKeys.all, "detail"] as const,
  detail: (id: string) => [...productKeys.details(), id] as const,
}

export function useProducts(filters: ProductFilters) {
  return useQuery({
    queryKey: productKeys.list(filters),
    queryFn: () => getProducts(filters),
  })
}

export function useProduct(id: string) {
  return useQuery({
    queryKey: productKeys.detail(id),
    queryFn: () => getProduct(id),
    enabled: !!id,
  })
}

export function useCreateProduct() {
  const queryClient = useQueryClient()
  const { toast } = useToast()

  return useMutation({
    mutationFn: (data: ProductCreateDto) => createProduct(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: productKeys.lists() })
      toast({
        title: "Product created",
        description: "The product has been created successfully.",
      })
    },
    onError: (error: Error) => {
      toast({
        title: "Error",
        description: error.message || "Failed to create product.",
        variant: "destructive",
      })
    },
  })
}

export function useUpdateProduct() {
  const queryClient = useQueryClient()
  const { toast } = useToast()

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: ProductUpdateDto }) =>
      updateProduct(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: productKeys.lists() })
      queryClient.invalidateQueries({ queryKey: productKeys.detail(id) })
      toast({
        title: "Product updated",
        description: "The product has been updated successfully.",
      })
    },
    onError: (error: Error) => {
      toast({
        title: "Error",
        description: error.message || "Failed to update product.",
        variant: "destructive",
      })
    },
  })
}

export function useDeleteProduct() {
  const queryClient = useQueryClient()
  const { toast } = useToast()

  return useMutation({
    mutationFn: (id: string) => deleteProduct(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: productKeys.lists() })
      toast({
        title: "Product deleted",
        description: "The product has been deleted successfully.",
      })
    },
    onError: (error: Error) => {
      toast({
        title: "Error",
        description: error.message || "Failed to delete product.",
        variant: "destructive",
      })
    },
  })
}
```

---

## 11. Authentication

### 11.1 Auth Store (Zustand)

```typescript
// src/stores/auth-store.ts
import { create } from "zustand"
import { persist } from "zustand/middleware"
import Cookies from "js-cookie"
import { apiClient } from "@/lib/api/client"

interface User {
  id: string
  email: string
  name: string
  role: string
  avatarUrl?: string
}

interface AuthState {
  user: User | null
  isAuthenticated: boolean
  isLoading: boolean
  login: (email: string, password: string) => Promise<void>
  logout: () => void
  checkAuth: () => Promise<void>
  setUser: (user: User) => void
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      isAuthenticated: false,
      isLoading: true,

      login: async (email: string, password: string) => {
        set({ isLoading: true })
        try {
          const response = await apiClient.post("/auth/login", { email, password })
          const { accessToken, refreshToken, user } = response.data

          Cookies.set("accessToken", accessToken, { secure: true, sameSite: "strict" })
          Cookies.set("refreshToken", refreshToken, { secure: true, sameSite: "strict" })

          set({ user, isAuthenticated: true, isLoading: false })
        } catch (error) {
          set({ isLoading: false })
          throw error
        }
      },

      logout: () => {
        Cookies.remove("accessToken")
        Cookies.remove("refreshToken")
        set({ user: null, isAuthenticated: false })
        window.location.href = "/login"
      },

      checkAuth: async () => {
        const token = Cookies.get("accessToken")
        if (!token) {
          set({ isAuthenticated: false, isLoading: false })
          return
        }

        try {
          const response = await apiClient.get("/auth/me")
          set({ user: response.data, isAuthenticated: true, isLoading: false })
        } catch {
          set({ user: null, isAuthenticated: false, isLoading: false })
        }
      },

      setUser: (user) => set({ user }),
    }),
    {
      name: "auth-storage",
      partialize: (state) => ({ user: state.user }),
    }
  )
)
```

### 11.2 Auth Guard Component

```typescript
// src/components/auth/auth-guard.tsx
"use client"

import { useEffect } from "react"
import { useRouter } from "next/navigation"
import { useAuthStore } from "@/stores/auth-store"
import { Skeleton } from "@/components/ui/skeleton"

interface AuthGuardProps {
  children: React.ReactNode
  requiredRole?: string
}

export function AuthGuard({ children, requiredRole }: AuthGuardProps) {
  const router = useRouter()
  const { isAuthenticated, isLoading, user, checkAuth } = useAuthStore()

  useEffect(() => {
    checkAuth()
  }, [checkAuth])

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.push("/login")
    }

    if (!isLoading && isAuthenticated && requiredRole && user?.role !== requiredRole) {
      router.push("/")
    }
  }, [isLoading, isAuthenticated, router, requiredRole, user])

  if (isLoading) {
    return (
      <div className="flex h-screen items-center justify-center">
        <div className="space-y-4 w-full max-w-md">
          <Skeleton className="h-12 w-full" />
          <Skeleton className="h-4 w-3/4" />
          <Skeleton className="h-4 w-1/2" />
        </div>
      </div>
    )
  }

  if (!isAuthenticated) {
    return null
  }

  return <>{children}</>
}
```

### 11.3 Login Page

```typescript
// src/app/(auth)/login/page.tsx
"use client"

import { useState } from "react"
import { useRouter } from "next/navigation"
import Link from "next/link"
import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { z } from "zod"
import { Loader2 } from "lucide-react"

import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle,
} from "@/components/ui/card"
import { useAuthStore } from "@/stores/auth-store"
import { useToast } from "@/hooks/use-toast"

const loginSchema = z.object({
  email: z.string().email("Invalid email address"),
  password: z.string().min(6, "Password must be at least 6 characters"),
})

type LoginFormData = z.infer<typeof loginSchema>

export default function LoginPage() {
  const router = useRouter()
  const { login } = useAuthStore()
  const { toast } = useToast()
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
      await login(data.email, data.password)
      router.push("/")
    } catch (error: any) {
      toast({
        title: "Login failed",
        description: error.response?.data?.message || "Invalid credentials",
        variant: "destructive",
      })
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-background p-4">
      <Card className="w-full max-w-md">
        <CardHeader className="space-y-1 text-center">
          <div className="flex justify-center mb-4">
            <div className="h-12 w-12 rounded-lg bg-primary flex items-center justify-center">
              <span className="text-2xl font-bold text-primary-foreground">V</span>
            </div>
          </div>
          <CardTitle className="text-2xl font-bold">Welcome back</CardTitle>
          <CardDescription>
            Sign in to your admin account
          </CardDescription>
        </CardHeader>
        <form onSubmit={handleSubmit(onSubmit)}>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <Label htmlFor="email">Email</Label>
              <Input
                id="email"
                type="email"
                placeholder="admin@vesmarket.com"
                {...register("email")}
                disabled={isLoading}
              />
              {errors.email && (
                <p className="text-sm text-destructive">{errors.email.message}</p>
              )}
            </div>
            <div className="space-y-2">
              <div className="flex items-center justify-between">
                <Label htmlFor="password">Password</Label>
                <Link
                  href="/forgot-password"
                  className="text-sm text-primary hover:underline"
                >
                  Forgot password?
                </Link>
              </div>
              <Input
                id="password"
                type="password"
                {...register("password")}
                disabled={isLoading}
              />
              {errors.password && (
                <p className="text-sm text-destructive">{errors.password.message}</p>
              )}
            </div>
          </CardContent>
          <CardFooter>
            <Button type="submit" className="w-full" disabled={isLoading}>
              {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Sign in
            </Button>
          </CardFooter>
        </form>
      </Card>
    </div>
  )
}
```

---

## 12. State Management

### 12.1 Settings Store

```typescript
// src/stores/settings-store.ts
import { create } from "zustand"
import { persist } from "zustand/middleware"

interface SettingsState {
  // Sidebar
  sidebarCollapsed: boolean
  setSidebarCollapsed: (collapsed: boolean) => void
  toggleSidebar: () => void
  
  // Locale
  locale: string
  setLocale: (locale: string) => void
  
  // Table settings
  defaultPageSize: number
  setDefaultPageSize: (size: number) => void
  
  // Notifications
  enableNotifications: boolean
  setEnableNotifications: (enabled: boolean) => void
}

export const useSettingsStore = create<SettingsState>()(
  persist(
    (set) => ({
      // Sidebar
      sidebarCollapsed: false,
      setSidebarCollapsed: (collapsed) => set({ sidebarCollapsed: collapsed }),
      toggleSidebar: () => set((state) => ({ sidebarCollapsed: !state.sidebarCollapsed })),
      
      // Locale
      locale: "en",
      setLocale: (locale) => set({ locale }),
      
      // Table settings
      defaultPageSize: 10,
      setDefaultPageSize: (size) => set({ defaultPageSize: size }),
      
      // Notifications
      enableNotifications: true,
      setEnableNotifications: (enabled) => set({ enableNotifications: enabled }),
    }),
    {
      name: "settings-storage",
    }
  )
)
```

---

## 13. Form Handling

### 13.1 Validation Schemas

```typescript
// src/lib/validations/product.ts
import { z } from "zod"

export const productSchema = z.object({
  name: z.string().min(2, "Name must be at least 2 characters"),
  code: z.string().min(1, "Product code is required"),
  description: z.string().optional(),
  categoryId: z.string().min(1, "Category is required"),
  brandId: z.string().optional(),
  listPrice: z.number().min(0, "Price must be positive"),
  dealerPrice: z.number().min(0, "Dealer price must be positive").optional(),
  stockQuantity: z.number().int().min(0, "Stock must be a positive integer"),
  isActive: z.boolean().default(true),
  images: z.array(z.string()).optional(),
  attributes: z.record(z.string(), z.any()).optional(),
})

export type ProductFormData = z.infer<typeof productSchema>
```

### 13.2 Reusable Form Component

```typescript
// src/components/forms/product-form.tsx
"use client"

import { useForm } from "react-hook-form"
import { zodResolver } from "@hookform/resolvers/zod"
import { Loader2 } from "lucide-react"

import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Textarea } from "@/components/ui/textarea"
import { Switch } from "@/components/ui/switch"
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { productSchema, type ProductFormData } from "@/lib/validations/product"
import { useCategories } from "@/hooks/use-categories"
import { useBrands } from "@/hooks/use-brands"

interface ProductFormProps {
  defaultValues?: Partial<ProductFormData>
  onSubmit: (data: ProductFormData) => Promise<void>
  isLoading?: boolean
}

export function ProductForm({ defaultValues, onSubmit, isLoading }: ProductFormProps) {
  const { data: categories } = useCategories()
  const { data: brands } = useBrands()

  const form = useForm<ProductFormData>({
    resolver: zodResolver(productSchema),
    defaultValues: {
      name: "",
      code: "",
      description: "",
      categoryId: "",
      brandId: "",
      listPrice: 0,
      dealerPrice: 0,
      stockQuantity: 0,
      isActive: true,
      ...defaultValues,
    },
  })

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <div className="grid gap-6 md:grid-cols-2">
          <FormField
            control={form.control}
            name="name"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Product Name</FormLabel>
                <FormControl>
                  <Input placeholder="Enter product name" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="code"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Product Code</FormLabel>
                <FormControl>
                  <Input placeholder="SKU-001" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="categoryId"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Category</FormLabel>
                <Select onValueChange={field.onChange} defaultValue={field.value}>
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder="Select a category" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    {categories?.map((category) => (
                      <SelectItem key={category.id} value={category.id}>
                        {category.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="brandId"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Brand</FormLabel>
                <Select onValueChange={field.onChange} defaultValue={field.value}>
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder="Select a brand" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    {brands?.map((brand) => (
                      <SelectItem key={brand.id} value={brand.id}>
                        {brand.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="listPrice"
            render={({ field }) => (
              <FormItem>
                <FormLabel>List Price</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    step="0.01"
                    placeholder="0.00"
                    {...field}
                    onChange={(e) => field.onChange(parseFloat(e.target.value))}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="dealerPrice"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Dealer Price</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    step="0.01"
                    placeholder="0.00"
                    {...field}
                    onChange={(e) => field.onChange(parseFloat(e.target.value))}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="stockQuantity"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Stock Quantity</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    placeholder="0"
                    {...field}
                    onChange={(e) => field.onChange(parseInt(e.target.value))}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="isActive"
            render={({ field }) => (
              <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                <div className="space-y-0.5">
                  <FormLabel className="text-base">Active</FormLabel>
                  <FormDescription>
                    Make this product visible in the catalog
                  </FormDescription>
                </div>
                <FormControl>
                  <Switch
                    checked={field.value}
                    onCheckedChange={field.onChange}
                  />
                </FormControl>
              </FormItem>
            )}
          />
        </div>

        <FormField
          control={form.control}
          name="description"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Description</FormLabel>
              <FormControl>
                <Textarea
                  placeholder="Enter product description"
                  className="min-h-[100px]"
                  {...field}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="flex justify-end gap-4">
          <Button type="button" variant="outline">
            Cancel
          </Button>
          <Button type="submit" disabled={isLoading}>
            {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Save Product
          </Button>
        </div>
      </form>
    </Form>
  )
}
```

---

## 14. Component Guidelines

### 14.1 Loading States

```typescript
// src/components/shared/loading-spinner.tsx
import { Loader2 } from "lucide-react"
import { cn } from "@/lib/utils"

interface LoadingSpinnerProps {
  size?: "sm" | "md" | "lg"
  className?: string
}

const sizeClasses = {
  sm: "h-4 w-4",
  md: "h-6 w-6",
  lg: "h-8 w-8",
}

export function LoadingSpinner({ size = "md", className }: LoadingSpinnerProps) {
  return (
    <Loader2 className={cn("animate-spin", sizeClasses[size], className)} />
  )
}

// Full page loading
export function PageLoader() {
  return (
    <div className="flex h-[50vh] items-center justify-center">
      <LoadingSpinner size="lg" />
    </div>
  )
}
```

### 14.2 Confirm Dialog

```typescript
// src/components/shared/confirm-dialog.tsx
"use client"

import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog"
import { Button } from "@/components/ui/button"
import { Loader2 } from "lucide-react"

interface ConfirmDialogProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  title: string
  description: string
  confirmText?: string
  cancelText?: string
  onConfirm: () => void | Promise<void>
  isLoading?: boolean
  variant?: "default" | "destructive"
}

export function ConfirmDialog({
  open,
  onOpenChange,
  title,
  description,
  confirmText = "Confirm",
  cancelText = "Cancel",
  onConfirm,
  isLoading,
  variant = "default",
}: ConfirmDialogProps) {
  return (
    <AlertDialog open={open} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>{title}</AlertDialogTitle>
          <AlertDialogDescription>{description}</AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel disabled={isLoading}>{cancelText}</AlertDialogCancel>
          <Button
            variant={variant}
            onClick={onConfirm}
            disabled={isLoading}
          >
            {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            {confirmText}
          </Button>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}
```

### 14.3 Empty State

```typescript
// src/components/shared/empty-state.tsx
import { LucideIcon } from "lucide-react"
import { cn } from "@/lib/utils"

interface EmptyStateProps {
  icon: LucideIcon
  title: string
  description: string
  action?: React.ReactNode
  className?: string
}

export function EmptyState({
  icon: Icon,
  title,
  description,
  action,
  className,
}: EmptyStateProps) {
  return (
    <div className={cn("flex flex-col items-center justify-center py-12", className)}>
      <div className="rounded-full bg-muted p-4 mb-4">
        <Icon className="h-8 w-8 text-muted-foreground" />
      </div>
      <h3 className="text-lg font-semibold mb-1">{title}</h3>
      <p className="text-muted-foreground text-center max-w-sm mb-4">
        {description}
      </p>
      {action}
    </div>
  )
}
```

---

## 15. Best Practices

### 15.1 File Naming Conventions

- **Components**: `kebab-case.tsx` (e.g., `product-form.tsx`)
- **Hooks**: `use-[name].ts` (e.g., `use-products.ts`)
- **Stores**: `[name]-store.ts` (e.g., `auth-store.ts`)
- **Types**: `[name].ts` in types folder (e.g., `entities.ts`)
- **Utilities**: `kebab-case.ts` (e.g., `format-date.ts`)

### 15.2 Import Order

```typescript
// 1. React/Next.js
import { useState, useEffect } from "react"
import { useRouter } from "next/navigation"

// 2. External libraries
import { useQuery } from "@tanstack/react-query"
import { z } from "zod"

// 3. Internal components
import { Button } from "@/components/ui/button"
import { ProductForm } from "@/components/forms/product-form"

// 4. Hooks
import { useProducts } from "@/hooks/use-products"

// 5. Utils/Types
import { cn } from "@/lib/utils"
import type { Product } from "@/types/entities"
```

### 15.3 TypeScript Guidelines

```typescript
// Always define prop interfaces
interface ComponentProps {
  title: string
  isLoading?: boolean
  onSubmit: (data: FormData) => Promise<void>
}

// Use type for unions/primitives
type Status = "pending" | "processing" | "completed"

// Use interface for objects
interface User {
  id: string
  name: string
  email: string
}

// Avoid `any` - use `unknown` if type is truly unknown
function handleError(error: unknown) {
  if (error instanceof Error) {
    console.error(error.message)
  }
}
```

### 15.4 Error Handling Pattern

```typescript
// src/lib/api/error-handler.ts
import { AxiosError } from "axios"

interface ApiError {
  message: string
  code?: string
  details?: Record<string, string[]>
}

export function handleApiError(error: unknown): ApiError {
  if (error instanceof AxiosError) {
    const data = error.response?.data
    return {
      message: data?.message || error.message || "An error occurred",
      code: data?.code,
      details: data?.errors,
    }
  }
  
  if (error instanceof Error) {
    return { message: error.message }
  }
  
  return { message: "An unexpected error occurred" }
}
```

### 15.5 Performance Tips

1. **Use React.memo for expensive components**
```typescript
export const ExpensiveList = React.memo(function ExpensiveList({ items }: Props) {
  return items.map(item => <Item key={item.id} {...item} />)
})
```

2. **Lazy load routes**
```typescript
// Next.js automatically code-splits by route
// For components:
const HeavyChart = dynamic(() => import("@/components/charts/heavy-chart"), {
  loading: () => <Skeleton className="h-[300px]" />,
})
```

3. **Debounce search inputs**
```typescript
import { useDebouncedCallback } from "use-debounce"

const debouncedSearch = useDebouncedCallback((value: string) => {
  setFilters(prev => ({ ...prev, search: value }))
}, 300)
```

4. **Use image optimization**
```typescript
import Image from "next/image"

<Image
  src={product.imageUrl}
  alt={product.name}
  width={200}
  height={200}
  placeholder="blur"
  blurDataURL="data:image/jpeg;base64,..."
/>
```

---

## Quick Reference Commands

```bash
# Navigate to admin project
cd B2B-COMMERCE/admin

# Development
npm run dev              # Start dev server (default: http://localhost:3000)
npm run build            # Build for production
npm run start            # Start production server
npm run lint             # Run ESLint
npm run type-check       # Run TypeScript check

# Add shadcn components
npx shadcn@latest add [component-name]

# Generate types from API (if using OpenAPI)
npx openapi-typescript http://localhost:5000/swagger/v1/swagger.json -o src/types/api.d.ts

# Run with backend (from B2B-COMMERCE root)
# Terminal 1: Start backend
cd backend && dotnet run

# Terminal 2: Start admin frontend
cd admin && npm run dev
```

---

## Resources

- [Next.js Documentation](https://nextjs.org/docs)
- [shadcn/ui Components](https://ui.shadcn.com/docs/components)
- [Tailwind CSS](https://tailwindcss.com/docs)
- [React Query](https://tanstack.com/query/latest)
- [Zustand](https://docs.pmnd.rs/zustand/getting-started/introduction)
- [React Hook Form](https://react-hook-form.com/)
- [Zod](https://zod.dev/)

---

*Last updated: December 2025*
