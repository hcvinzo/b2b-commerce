'use client'

import { useRouter } from 'next/navigation'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Loader2 } from 'lucide-react'
import Image from 'next/image'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Card } from '@/components/ui/card'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
} from '@/components/ui/form'
import { StepIndicator } from '@/components/ui/StepIndicator'
import { DatePicker } from '@/components/ui/date-picker'
import { useRegistrationStore } from '@/stores/registrationStore'
import { step1Schema, Step1FormData } from '@/lib/validations/registration.schema'
import { checkEmailAvailability } from '@/lib/api'

export default function RegisterStep1Page() {
  const router = useRouter()
  const { contactPerson, setContactPerson, setCurrentStep } = useRegistrationStore()

  const form = useForm<Step1FormData>({
    resolver: zodResolver(step1Schema),
    defaultValues: {
      firstName: contactPerson.firstName || '',
      lastName: contactPerson.lastName || '',
      email: contactPerson.email || '',
      emailConfirmation: contactPerson.emailConfirmation || '',
      position: contactPerson.position || '',
      dateOfBirth: contactPerson.dateOfBirth ? new Date(contactPerson.dateOfBirth) : undefined,
      gender: contactPerson.gender || '',
      phone: contactPerson.phone || '',
      phoneExt: contactPerson.phoneExt || '',
      gsm: contactPerson.gsm || '+90',
    },
  })

  const onSubmit = async (data: Step1FormData) => {
    // Check if email is available before proceeding
    try {
      const emailCheck = await checkEmailAvailability(data.email)
      if (!emailCheck.available) {
        form.setError('email', {
          type: 'manual',
          message: 'Bu e-posta adresi zaten kayıtlı',
        })
        return
      }
    } catch (error) {
      // If email check fails, show error but don't block the user
      console.error('Email check failed:', error)
      form.setError('email', {
        type: 'manual',
        message: 'E-posta kontrolü yapılamadı. Lütfen tekrar deneyin.',
      })
      return
    }

    setContactPerson({
      ...data,
      dateOfBirth: data.dateOfBirth?.toISOString(),
    })
    setCurrentStep(2)
    router.push('/register/step-2')
  }

  const { errors } = form.formState

  return (
    <div className="max-w-5xl mx-auto">
      {/* Step Indicator */}
      <div className="mb-8">
        <StepIndicator currentStep={1} />
      </div>

      {/* Form Card */}
      <Card className="overflow-hidden p-0">
        <div className="flex flex-col lg:flex-row">
          {/* Left Side - Hero Image */}
          <div className="lg:w-1/2 relative min-h-[500px] hidden lg:block">
            <Image
              src="/images/login-hero.jpg"
              alt="B2B İş Ortaklığı"
              fill
              className="object-cover rounded-l-xl"
              priority
            />
          </div>

          {/* Right Side - Form */}
          <div className="lg:w-1/2 p-8">
            <h2 className="text-lg font-semibold text-foreground mb-6">İlgili Kişi</h2>

            <Form {...form}>
              <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
                {/* Form Error Summary */}
                {Object.keys(errors).length > 0 && (
                  <div className="mb-2 p-4 bg-destructive/10 border border-destructive/20 rounded-md">
                    <p className="text-sm font-medium text-destructive mb-2">Lütfen aşağıdaki alanları kontrol ediniz:</p>
                    <ul className="text-sm text-destructive list-disc list-inside">
                      {errors.firstName && <li>Ad gereklidir</li>}
                      {errors.lastName && <li>Soyad gereklidir</li>}
                      {errors.email && <li>{errors.email.message || 'Geçerli bir e-posta adresi giriniz'}</li>}
                      {errors.emailConfirmation && <li>{errors.emailConfirmation.message || 'E-posta adresleri eşleşmiyor'}</li>}
                      {errors.position && <li>Görev gereklidir</li>}
                      {errors.phone && <li>Telefon numarası gereklidir</li>}
                      {errors.gsm && <li>Mobil telefon numarası gereklidir</li>}
                    </ul>
                  </div>
                )}

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <FormField
                    control={form.control}
                    name="firstName"
                    render={({ field, fieldState }) => (
                      <FormItem>
                        <FormLabel className={fieldState.error ? 'text-destructive' : ''}>Adı</FormLabel>
                        <FormControl>
                          <Input placeholder="Adı" className={fieldState.error ? 'border-destructive' : ''} {...field} />
                        </FormControl>
                      </FormItem>
                    )}
                  />
                  <FormField
                    control={form.control}
                    name="lastName"
                    render={({ field, fieldState }) => (
                      <FormItem>
                        <FormLabel className={fieldState.error ? 'text-destructive' : ''}>Soyadı</FormLabel>
                        <FormControl>
                          <Input placeholder="Soyadı" className={fieldState.error ? 'border-destructive' : ''} {...field} />
                        </FormControl>
                      </FormItem>
                    )}
                  />
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <FormField
                    control={form.control}
                    name="email"
                    render={({ field, fieldState }) => (
                      <FormItem>
                        <FormLabel className={fieldState.error ? 'text-destructive' : ''}>E-Posta</FormLabel>
                        <FormControl>
                          <Input type="email" placeholder="E-Posta" className={fieldState.error ? 'border-destructive' : ''} {...field} />
                        </FormControl>
                      </FormItem>
                    )}
                  />
                  <FormField
                    control={form.control}
                    name="emailConfirmation"
                    render={({ field, fieldState }) => (
                      <FormItem>
                        <FormLabel className={fieldState.error ? 'text-destructive' : ''}>E-Posta Tekrar</FormLabel>
                        <FormControl>
                          <Input type="email" placeholder="E-Posta Tekrar" className={fieldState.error ? 'border-destructive' : ''} {...field} />
                        </FormControl>
                      </FormItem>
                    )}
                  />
                </div>

                <FormField
                  control={form.control}
                  name="position"
                  render={({ field, fieldState }) => (
                    <FormItem>
                      <FormLabel className={fieldState.error ? 'text-destructive' : ''}>Görevi</FormLabel>
                      <FormControl>
                        <Input placeholder="Görevi" className={fieldState.error ? 'border-destructive' : ''} {...field} />
                      </FormControl>
                    </FormItem>
                  )}
                />

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <FormField
                    control={form.control}
                    name="dateOfBirth"
                    render={({ field, fieldState }) => (
                      <FormItem>
                        <FormLabel className={fieldState.error ? 'text-destructive' : ''}>Doğum Tarihi</FormLabel>
                        <DatePicker
                          value={field.value}
                          onChange={field.onChange}
                          placeholder="Doğum tarihi seçiniz"
                          className={fieldState.error ? 'border-destructive' : ''}
                        />
                      </FormItem>
                    )}
                  />
                  <FormField
                    control={form.control}
                    name="gender"
                    render={({ field, fieldState }) => (
                      <FormItem>
                        <FormLabel className={fieldState.error ? 'text-destructive' : ''}>Cinsiyet</FormLabel>
                        <Select onValueChange={field.onChange} defaultValue={field.value}>
                          <SelectTrigger className={`w-full ${fieldState.error ? 'border-destructive' : ''}`}>
                            <SelectValue placeholder="Seçiniz" />
                          </SelectTrigger>
                          <SelectContent>
                            <SelectItem value="Male">Erkek</SelectItem>
                            <SelectItem value="Female">Kadın</SelectItem>
                          </SelectContent>
                        </Select>
                      </FormItem>
                    )}
                  />
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <FormField
                    control={form.control}
                    name="phone"
                    render={({ field, fieldState }) => (
                      <FormItem>
                        <FormLabel className={fieldState.error ? 'text-destructive' : ''}>İş Telefon</FormLabel>
                        <FormControl>
                          <Input type="tel" placeholder="+90 XXX XXX XX XX" className={fieldState.error ? 'border-destructive' : ''} {...field} />
                        </FormControl>
                      </FormItem>
                    )}
                  />
                  <FormField
                    control={form.control}
                    name="phoneExt"
                    render={({ field, fieldState }) => (
                      <FormItem>
                        <FormLabel className={fieldState.error ? 'text-destructive' : ''}>Dahili Numara</FormLabel>
                        <FormControl>
                          <Input placeholder="Dahili Numara" className={fieldState.error ? 'border-destructive' : ''} {...field} />
                        </FormControl>
                      </FormItem>
                    )}
                  />
                </div>

                <FormField
                  control={form.control}
                  name="gsm"
                  render={({ field, fieldState }) => (
                    <FormItem>
                      <FormLabel className={fieldState.error ? 'text-destructive' : ''}>Mobil</FormLabel>
                      <FormControl>
                        <Input type="tel" placeholder="+90 5XX XXX XX XX" className={fieldState.error ? 'border-destructive' : ''} {...field} />
                      </FormControl>
                    </FormItem>
                  )}
                />

                <div className="pt-4">
                  <Button type="submit" className="w-full" disabled={form.formState.isSubmitting}>
                    {form.formState.isSubmitting && <Loader2 className="animate-spin" />}
                    Kaydet & Devam Et
                  </Button>
                </div>
              </form>
            </Form>
          </div>
        </div>
      </Card>
    </div>
  )
}
