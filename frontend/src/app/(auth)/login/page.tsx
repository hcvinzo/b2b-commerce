'use client'

import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import Link from 'next/link'
import { useRouter } from 'next/navigation'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Card } from '@/components/ui/Card'
import { Logo } from '@/components/layout/Logo'
import { Footer } from '@/components/layout/Footer'
import { loginSchema, LoginFormData } from '@/lib/validations/login.schema'
import { loginUser } from '@/lib/api'

export default function LoginPage() {
  const router = useRouter()
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<LoginFormData>({
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
        <div className="hidden md:flex md:w-1/2 bg-gradient-to-br from-primary to-primary-700 items-center justify-center p-8">
          <div className="text-center text-white">
            <div className="w-32 h-32 mx-auto mb-6 bg-white/10 rounded-full flex items-center justify-center">
              <svg className="w-16 h-16" viewBox="0 0 32 32" fill="currentColor">
                <path d="M16 2L4 7v9c0 7.732 10.039 11.871 11.627 12.432a1 1 0 00.746 0C17.961 27.871 28 23.732 28 16V7L16 2z" />
              </svg>
            </div>
            <h2 className="text-2xl font-bold mb-2">B2B İş Ortağı Portalı</h2>
            <p className="text-white/80">
              Güvenilir iş ortaklığınız için dijital çözümler
            </p>
          </div>
        </div>

        {/* Right Side - Form */}
        <div className="w-full md:w-1/2 p-8 md:p-12">
          {/* Logo */}
          <div className="mb-8">
            <Logo />
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
                className="text-sm text-primary hover:text-primary-600 transition-colors"
              >
                Şifrenizi mi unuttunuz?
              </Link>
            </div>

            <div className="flex flex-col sm:flex-row gap-4 pt-4">
              <Button type="submit" fullWidth isLoading={isLoading}>
                Giriş Yap
              </Button>
              <Link href="/register/step-1" className="w-full sm:w-auto">
                <Button type="button" variant="secondary" fullWidth>
                  Kayıt Ol
                </Button>
              </Link>
            </div>
          </form>
        </div>
      </Card>

      {/* Footer */}
      <div className="mt-8">
        <Footer />
      </div>
    </div>
  )
}
