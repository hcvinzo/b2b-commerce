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
  FormMessage,
} from '@/components/ui/form'
import { StepIndicator } from '@/components/ui/StepIndicator'
import { DatePicker } from '@/components/ui/date-picker'
import { useRegistrationStore } from '@/stores/registrationStore'
import { step1Schema, Step1FormData } from '@/lib/validations/registration.schema'

export default function RegisterStep1Page() {
  const router = useRouter()
  const { contactPerson, setContactPerson, setCurrentStep } = useRegistrationStore()

  const form = useForm<Step1FormData>({
    resolver: zodResolver(step1Schema),
    defaultValues: {
      firstName: contactPerson.firstName || '',
      lastName: contactPerson.lastName || '',
      email: contactPerson.email || '',
      alternativeEmail: contactPerson.alternativeEmail || '',
      position: contactPerson.position || '',
      birthDate: contactPerson.birthDate ? new Date(contactPerson.birthDate) : undefined,
      gender: contactPerson.gender || '',
      workPhone: contactPerson.workPhone || '+90',
      extension: contactPerson.extension || '',
      mobile: contactPerson.mobile || '+90',
    },
  })

  const onSubmit = async (data: Step1FormData) => {
    setContactPerson({
      ...data,
      birthDate: data.birthDate?.toISOString(),
    })
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
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <FormField
                    control={form.control}
                    name="firstName"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Adı</FormLabel>
                        <FormControl>
                          <Input placeholder="Adı" {...field} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                  <FormField
                    control={form.control}
                    name="lastName"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Soyadı</FormLabel>
                        <FormControl>
                          <Input placeholder="Soyadı" {...field} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
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
                    name="alternativeEmail"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Alternatif E-Posta</FormLabel>
                        <FormControl>
                          <Input type="email" placeholder="Alternatif E-Posta" {...field} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>

                <FormField
                  control={form.control}
                  name="position"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Görevi</FormLabel>
                      <FormControl>
                        <Input placeholder="Görevi" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <FormField
                    control={form.control}
                    name="birthDate"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Doğum Tarihi</FormLabel>
                        <FormControl>
                          <DatePicker
                            value={field.value}
                            onChange={field.onChange}
                            placeholder="Doğum tarihi seçiniz"
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                  <FormField
                    control={form.control}
                    name="gender"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Cinsiyet</FormLabel>
                        <Select onValueChange={field.onChange} defaultValue={field.value}>
                          <FormControl>
                            <SelectTrigger className="w-full">
                              <SelectValue placeholder="Seçiniz" />
                            </SelectTrigger>
                          </FormControl>
                          <SelectContent>
                            <SelectItem value="male">Erkek</SelectItem>
                            <SelectItem value="female">Kadın</SelectItem>
                          </SelectContent>
                        </Select>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>

                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <FormField
                    control={form.control}
                    name="workPhone"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>İş Telefon</FormLabel>
                        <FormControl>
                          <Input type="tel" placeholder="+90 5XX XXX XX XX" {...field} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                  <FormField
                    control={form.control}
                    name="extension"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Dahili Numara</FormLabel>
                        <FormControl>
                          <Input placeholder="Dahili Numara" {...field} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>

                <FormField
                  control={form.control}
                  name="mobile"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Mobil</FormLabel>
                      <FormControl>
                        <Input type="tel" placeholder="+90 5XX XXX XX XX" {...field} />
                      </FormControl>
                      <FormMessage />
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
