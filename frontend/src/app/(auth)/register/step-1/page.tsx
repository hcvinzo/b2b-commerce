'use client'

import { useRouter } from 'next/navigation'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useState, useEffect } from 'react'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import { Card } from '@/components/ui/Card'
import { StepIndicator } from '@/components/ui/StepIndicator'
import { useRegistrationStore } from '@/stores/registrationStore'
import { step1Schema, Step1FormData } from '@/lib/validations/registration.schema'

// Turkey cities data
const TURKEY_CITIES = [
  { value: 'istanbul', label: 'İstanbul', districts: ['Kadıköy', 'Beşiktaş', 'Şişli', 'Üsküdar', 'Bakırköy', 'Ataşehir', 'Maltepe', 'Kartal', 'Pendik', 'Beylikdüzü', 'Esenyurt', 'Başakşehir', 'Sarıyer', 'Fatih', 'Beyoğlu'] },
  { value: 'ankara', label: 'Ankara', districts: ['Çankaya', 'Keçiören', 'Yenimahalle', 'Mamak', 'Etimesgut', 'Sincan', 'Altındağ', 'Pursaklar', 'Gölbaşı', 'Polatlı'] },
  { value: 'izmir', label: 'İzmir', districts: ['Konak', 'Karşıyaka', 'Bornova', 'Buca', 'Bayraklı', 'Çiğli', 'Gaziemir', 'Balçova', 'Narlıdere', 'Karabağlar'] },
  { value: 'bursa', label: 'Bursa', districts: ['Osmangazi', 'Nilüfer', 'Yıldırım', 'Gemlik', 'İnegöl', 'Mudanya', 'Gürsu'] },
  { value: 'antalya', label: 'Antalya', districts: ['Muratpaşa', 'Konyaaltı', 'Kepez', 'Alanya', 'Manavgat', 'Serik', 'Kaş'] },
  { value: 'adana', label: 'Adana', districts: ['Seyhan', 'Yüreğir', 'Çukurova', 'Sarıçam', 'Ceyhan', 'Kozan'] },
  { value: 'konya', label: 'Konya', districts: ['Selçuklu', 'Meram', 'Karatay', 'Ereğli', 'Akşehir', 'Beyşehir'] },
  { value: 'gaziantep', label: 'Gaziantep', districts: ['Şahinbey', 'Şehitkamil', 'Nizip', 'İslahiye', 'Oğuzeli'] },
  { value: 'mersin', label: 'Mersin', districts: ['Yenişehir', 'Mezitli', 'Akdeniz', 'Toroslar', 'Tarsus', 'Erdemli'] },
  { value: 'kayseri', label: 'Kayseri', districts: ['Melikgazi', 'Kocasinan', 'Talas', 'Develi', 'Yahyalı'] },
]

const COUNTRIES = [
  { value: 'TR', label: 'Türkiye' },
  { value: 'DE', label: 'Almanya' },
  { value: 'NL', label: 'Hollanda' },
  { value: 'GB', label: 'İngiltere' },
  { value: 'FR', label: 'Fransa' },
  { value: 'US', label: 'Amerika Birleşik Devletleri' },
]

export default function RegisterStep1Page() {
  const router = useRouter()
  const { contactPerson, setContactPerson, setCurrentStep } = useRegistrationStore()
  const [selectedCity, setSelectedCity] = useState<string>(contactPerson.city || '')
  const [districts, setDistricts] = useState<string[]>([])

  const {
    register,
    handleSubmit,
    control,
    watch,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<Step1FormData>({
    resolver: zodResolver(step1Schema),
    defaultValues: {
      ...contactPerson,
      country: contactPerson.country || 'TR',
      workPhone: contactPerson.workPhone || '+90',
      mobile: contactPerson.mobile || '+90',
    } as Step1FormData,
  })

  const watchCountry = watch('country')
  const watchCity = watch('city')

  useEffect(() => {
    if (watchCity) {
      const city = TURKEY_CITIES.find(c => c.value === watchCity)
      setDistricts(city?.districts || [])
      setSelectedCity(watchCity)
    } else {
      setDistricts([])
    }
  }, [watchCity])

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
      <Card>
        <div className="flex flex-col lg:flex-row">
          {/* Left Side - Image Placeholder */}
          <div className="lg:w-1/2 bg-gradient-to-br from-primary-100 to-primary-50 min-h-[400px] flex items-center justify-center p-8">
            <div className="text-center">
              <div className="w-24 h-24 mx-auto mb-4 bg-primary/10 rounded-full flex items-center justify-center">
                <svg className="w-12 h-12 text-primary" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
                </svg>
              </div>
              <p className="text-gray-500 text-sm">İlgili Kişi Bilgileri</p>
            </div>
          </div>

          {/* Right Side - Form */}
          <div className="lg:w-1/2 p-8">
            <h2 className="form-section-title">İlgili Kişi</h2>

            <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
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

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
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

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
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
                </Select>
              </div>

              {/* Location Selection */}
              <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                <Select
                  label="Ülke"
                  {...register('country')}
                  error={errors.country?.message}
                >
                  <option value="">Seçiniz</option>
                  {COUNTRIES.map(country => (
                    <option key={country.value} value={country.value}>{country.label}</option>
                  ))}
                </Select>
                <Select
                  label="Şehir"
                  {...register('city')}
                  error={errors.city?.message}
                  disabled={watchCountry !== 'TR'}
                >
                  <option value="">Seçiniz</option>
                  {TURKEY_CITIES.map(city => (
                    <option key={city.value} value={city.value}>{city.label}</option>
                  ))}
                </Select>
                <Select
                  label="İlçe"
                  {...register('district')}
                  error={errors.district?.message}
                  disabled={!watchCity || districts.length === 0}
                >
                  <option value="">Seçiniz</option>
                  {districts.map(district => (
                    <option key={district} value={district}>{district}</option>
                  ))}
                </Select>
              </div>

              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <Input
                  type="tel"
                  label="İş Telefon"
                  placeholder="+90 5XX XXX XX XX"
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
                placeholder="+90 5XX XXX XX XX"
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
      </Card>
    </div>
  )
}
