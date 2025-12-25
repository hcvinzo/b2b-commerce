'use client'

import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { Loader2 } from 'lucide-react'
import Image from 'next/image'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Card } from '@/components/ui/card'
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from '@/components/ui/form'
import { Footer } from '@/components/layout/Footer'
import { loginSchema, LoginFormData } from '@/lib/validations/login.schema'
import { loginUser } from '@/lib/api'

export default function LoginPage() {
  const router = useRouter()
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const form = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  })

  const onSubmit = async (data: LoginFormData) => {
    setIsLoading(true)
    setError(null)

    try {
      const response = await loginUser(data)
      localStorage.setItem('accessToken', response.accessToken)
      localStorage.setItem('refreshToken', response.refreshToken)
      router.push('/dashboard')
    } catch (err) {
      setError('Giriş başarısız. Lütfen bilgilerinizi kontrol edin.')
      console.error('Login error:', err)
    } finally {
      setIsLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-gray-50 flex flex-col items-center justify-center p-4">
      <Card className="w-full max-w-4xl flex flex-col md:flex-row overflow-hidden">
        {/* Left Side - Hero Image */}
        <div className="hidden md:block md:w-1/2 relative">
          <Image
            src="/images/login-hero.jpg"
            alt="Login Hero"
            fill
            className="object-cover"
          />
        </div>

        {/* Right Side - Form */}
        <div className="w-full md:w-1/2 p-8 md:p-12">
          {/* Logo */}
          <div className="mb-8">
            <Image
              src="/images/logo-wb.jpg"
              alt="Vesmarket Logo"
              width={180}
              height={50}
              priority
            />
          </div>

          {/* Title */}
          <h1 className="text-2xl font-bold text-gray-900 mb-2">
            Hoş geldiniz
          </h1>
          <p className="text-gray-500 mb-8">
            Sizinle iş ortağı olarak bir araya gelmekten büyük memnuniyet duyuyoruz.
          </p>

          {/* Error Message */}
          {error && (
            <div className="mb-4 p-3 bg-red-50 border border-red-200 rounded-md">
              <p className="text-sm text-red-600">{error}</p>
            </div>
          )}

          {/* Form */}
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
              <FormField
                control={form.control}
                name="businessPartnerCode"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>İş Ortağı Kodu</FormLabel>
                    <FormControl>
                      <Input placeholder="İş Ortağı Kodu" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="email"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>E-Posta</FormLabel>
                    <FormControl>
                      <Input type="email" placeholder="E-Posta" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="password"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Şifre</FormLabel>
                    <FormControl>
                      <Input type="password" placeholder="Şifre" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <div className="text-right">
                <Link
                  href="/forgot-password"
                  className="text-sm text-primary hover:text-primary-600 transition-colors"
                >
                  Şifrenizi mi unuttunuz?
                </Link>
              </div>

              <div className="flex flex-col sm:flex-row gap-4 pt-4">
                <Button type="submit" className="flex-1" disabled={isLoading}>
                  {isLoading && <Loader2 className="animate-spin" />}
                  Giriş Yap
                </Button>
                <Link href="/register/step-1" className="sm:w-32">
                  <Button type="button" variant="outline" className="w-full">
                    Kayıt Ol
                  </Button>
                </Link>
              </div>
            </form>
          </Form>
        </div>
      </Card>

      {/* Footer */}
      <div className="mt-8">
        <Footer />
      </div>
    </div>
  )
}
