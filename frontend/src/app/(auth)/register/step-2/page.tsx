'use client'

import { useRouter } from 'next/navigation'
import { useForm, useFieldArray } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Card } from '@/components/ui/Card'
import { StepIndicator } from '@/components/ui/StepIndicator'
import { useRegistrationStore } from '@/stores/registrationStore'
import { step2Schema, Step2FormData } from '@/lib/validations/registration.schema'

export default function RegisterStep2Page() {
  const router = useRouter()
  const { businessInfo, setBusinessInfo, setCurrentStep } = useRegistrationStore()

  const {
    register,
    handleSubmit,
    control,
    formState: { errors, isSubmitting },
  } = useForm<Step2FormData>({
    resolver: zodResolver(step2Schema),
    defaultValues: {
      ...businessInfo,
      authorizedPersons: businessInfo.authorizedPersons || [
        { fullName: '', tcNumber: '', sharePercentage: 0 },
        { fullName: '', tcNumber: '', sharePercentage: 0 },
        { fullName: '', tcNumber: '', sharePercentage: 0 },
        { fullName: '', tcNumber: '', sharePercentage: 0 },
        { fullName: '', tcNumber: '', sharePercentage: 0 },
        { fullName: '', tcNumber: '', sharePercentage: 0 },
      ],
    } as Step2FormData,
  })

  const { fields } = useFieldArray({
    control,
    name: 'authorizedPersons',
  })

  const onSubmit = async (data: Step2FormData) => {
    setBusinessInfo(data)
    setCurrentStep(3)
    router.push('/register/step-3')
  }

  return (
    <div className="max-w-6xl mx-auto">
      {/* Step Indicator */}
      <div className="mb-8">
        <StepIndicator currentStep={2} />
      </div>

      {/* Form Card */}
      <Card className="p-8">
        <form onSubmit={handleSubmit(onSubmit)}>
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
            {/* Left Section - Business Info */}
            <div>
              <h2 className="form-section-title">İşletme Bilgileri</h2>

              <div className="space-y-4">
                <Input
                  label="Ünvan"
                  placeholder="Şirket Ünvanı"
                  {...register('companyTitle')}
                  error={errors.companyTitle?.message}
                />

                <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                  <Input
                    label="Vergi Dairesi"
                    placeholder="Vergi Dairesi"
                    {...register('taxOffice')}
                    error={errors.taxOffice?.message}
                  />
                  <Input
                    label="Vergi Numarası"
                    placeholder="Vergi Numarası"
                    {...register('taxNumber')}
                    error={errors.taxNumber?.message}
                  />
                  <div className="space-y-1">
                    <label className="input-label">Kuruluş Yılı</label>
                    <select
                      className="input-field"
                      {...register('foundedYear', { valueAsNumber: true })}
                    >
                      <option value="">Yıl Seçiniz</option>
                      {Array.from(
                        { length: new Date().getFullYear() - 1900 + 1 },
                        (_, i) => new Date().getFullYear() - i
                      ).map((year) => (
                        <option key={year} value={year}>
                          {year}
                        </option>
                      ))}
                    </select>
                    {errors.foundedYear?.message && (
                      <p className="input-error">{errors.foundedYear.message}</p>
                    )}
                  </div>
                </div>

                <h3 className="form-section-title mt-8">İletişim</h3>

                <Input
                  label="Adres"
                  placeholder="Adres"
                  {...register('address')}
                  error={errors.address?.message}
                />

                <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                  <Input
                    label="Ülke"
                    placeholder="Ülke"
                    {...register('country')}
                    error={errors.country?.message}
                  />
                  <Input
                    type="tel"
                    label="Telefon"
                    placeholder="Telefon"
                    {...register('phone')}
                    error={errors.phone?.message}
                  />
                  <Input
                    label="İnternet Sayfası"
                    placeholder="https://"
                    {...register('website')}
                    error={errors.website?.message}
                  />
                </div>
              </div>
            </div>

            {/* Right Section - Authorized Persons */}
            <div>
              <h2 className="form-section-title">Yetkililer & Ortaklar</h2>

              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="border-b border-gray-200">
                      <th className="text-left py-2 px-2 text-sm font-medium text-gray-700">Adı Soyadı</th>
                      <th className="text-left py-2 px-2 text-sm font-medium text-gray-700">T.C. Kimlik No</th>
                      <th className="text-left py-2 px-2 text-sm font-medium text-gray-700">Pay Oranı (%)</th>
                    </tr>
                  </thead>
                  <tbody>
                    {fields.map((field, index) => (
                      <tr key={field.id} className="border-b border-gray-100">
                        <td className="py-2 px-2">
                          <input
                            type="text"
                            placeholder="Adı Soyadı"
                            className="input-field text-sm"
                            {...register(`authorizedPersons.${index}.fullName`)}
                          />
                        </td>
                        <td className="py-2 px-2">
                          <div>
                            <input
                              type="text"
                              placeholder="T.C. Kimlik No"
                              className={`input-field text-sm ${errors.authorizedPersons?.[index]?.tcNumber ? 'border-red-500' : ''}`}
                              maxLength={11}
                              {...register(`authorizedPersons.${index}.tcNumber`)}
                            />
                            {errors.authorizedPersons?.[index]?.tcNumber && (
                              <p className="text-xs text-red-500 mt-1">
                                {errors.authorizedPersons[index]?.tcNumber?.message}
                              </p>
                            )}
                          </div>
                        </td>
                        <td className="py-2 px-2">
                          <input
                            type="number"
                            placeholder="%"
                            className="input-field text-sm w-20"
                            min={0}
                            max={100}
                            {...register(`authorizedPersons.${index}.sharePercentage`, { valueAsNumber: true })}
                          />
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              {errors.authorizedPersons?.message && (
                <p className="input-error mt-2">{errors.authorizedPersons.message}</p>
              )}
              {errors.authorizedPersons?.root?.message && (
                <p className="input-error mt-2">{errors.authorizedPersons.root.message}</p>
              )}
            </div>
          </div>

          <div className="flex justify-between mt-8 pt-6 border-t border-gray-200">
            <Button
              type="button"
              variant="secondary"
              onClick={() => router.push('/register/step-1')}
            >
              Geri
            </Button>
            <Button type="submit" isLoading={isSubmitting}>
              Kaydet & Devam Et
            </Button>
          </div>
        </form>
      </Card>
    </div>
  )
}
