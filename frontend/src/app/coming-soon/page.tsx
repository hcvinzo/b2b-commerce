'use client'

import { useState } from 'react'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Card } from '@/components/ui/Card'
import Image from 'next/image'
import { Footer } from '@/components/layout/Footer'
import { newsletterSchema, NewsletterFormData } from '@/lib/validations/newsletter.schema'
import { subscribeNewsletter } from '@/lib/api'

export default function ComingSoonPage() {
  const [isLoading, setIsLoading] = useState(false)
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null)

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<NewsletterFormData>({
    resolver: zodResolver(newsletterSchema),
  })

  const onSubmit = async (data: NewsletterFormData) => {
    setIsLoading(true)
    setMessage(null)

    try {
      const response = await subscribeNewsletter(data.email)
      setMessage({ type: 'success', text: response.message })
      reset()
    } catch (err) {
      setMessage({
        type: 'error',
        text: 'Bir hata oluştu. Lütfen daha sonra tekrar deneyin.'
      })
      console.error('Newsletter subscription error:', err)
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
            alt="Coming Soon"
            fill
            className="object-cover"
          />
        </div>

        {/* Right Side - Content */}
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
            Yapım Aşamasında
          </h1>
          <p className="text-gray-500 mb-6">
            B2B e-ticaret platformumuz çok yakında hizmetinizde olacak.
            Lansman haberlerinden ilk siz haberdar olmak için e-posta adresinizi bırakın.
          </p>

          {/* Message */}
          {message && (
            <div className={`mb-4 p-3 rounded-md ${
              message.type === 'success'
                ? 'bg-green-50 border border-green-200'
                : 'bg-red-50 border border-red-200'
            }`}>
              <p className={`text-sm ${
                message.type === 'success' ? 'text-green-600' : 'text-red-600'
              }`}>
                {message.text}
              </p>
            </div>
          )}

          {/* Newsletter Form */}
          <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
            <Input
              type="email"
              label="E-Posta Adresiniz"
              placeholder="ornek@sirket.com"
              {...register('email')}
              error={errors.email?.message}
            />

            <Button type="submit" fullWidth isLoading={isLoading}>
              Beni Haberdar Et
            </Button>
          </form>

          {/* Additional Info */}
          <div className="mt-8 pt-6 border-t border-gray-200">
            <p className="text-sm text-gray-400 text-center">
              E-posta adresiniz sadece lansman bildirimleri için kullanılacaktır.
            </p>
          </div>
        </div>
      </Card>

      {/* Footer */}
      <div className="mt-8">
        <Footer />
      </div>
    </div>
  )
}
