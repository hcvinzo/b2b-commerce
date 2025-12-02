'use client'

import { useRouter } from 'next/navigation'
import { useForm, useFieldArray, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Button } from '@/components/ui/Button'
import { Input } from '@/components/ui/Input'
import { Select } from '@/components/ui/Select'
import { Checkbox } from '@/components/ui/Checkbox'
import { Card } from '@/components/ui/Card'
import { StepIndicator } from '@/components/ui/StepIndicator'
import { useRegistrationStore } from '@/stores/registrationStore'
import { step3Schema, Step3FormData } from '@/lib/validations/registration.schema'

const PRODUCT_CATEGORIES = [
  { value: 'computers', label: 'Bilgisayarlar' },
  { value: 'industrial_machines', label: 'İş Makineleri' },
  { value: 'servers', label: 'Sunucular' },
  { value: 'software', label: 'Yazılımlar' },
  { value: 'network', label: 'Network' },
  { value: 'security', label: 'Güvenlik' },
  { value: 'storage', label: 'Depolama' },
  { value: 'network_products', label: 'Ağ Ürünleri' },
  { value: 'printers', label: 'Yazıcı Çözümleri' },
  { value: 'accessories', label: 'Aksesuarlar' },
  { value: 'other', label: 'Diğer' },
]

const WORKING_CONDITIONS = [
  { value: 'cash_credit', label: 'Nakit & Kredi Kartı' },
  { value: 'open_account', label: 'Açık Hesap & Vadeli' },
  { value: 'check', label: 'Çek' },
]

export default function RegisterStep3Page() {
  const router = useRouter()
  const { operationalDetails, setOperationalDetails, setCurrentStep } = useRegistrationStore()

  const {
    register,
    handleSubmit,
    control,
    watch,
    formState: { errors, isSubmitting },
  } = useForm<Step3FormData>({
    resolver: zodResolver(step3Schema),
    defaultValues: {
      employeeCount: operationalDetails.employeeCount || '',
      businessStructure: operationalDetails.businessStructure || '',
      revenueYear: operationalDetails.revenueYear,
      targetRevenue: operationalDetails.targetRevenue,
      customerBase: operationalDetails.customerBase || {
        retailer: 0,
        corporate: 0,
        construction: 0,
        retail: 0,
      },
      productCategories: operationalDetails.productCategories || [],
      currentPartners: operationalDetails.currentPartners || [
        { companyName: '', workingCondition: '', creditLimit: 0 },
        { companyName: '', workingCondition: '', creditLimit: 0 },
        { companyName: '', workingCondition: '', creditLimit: 0 },
        { companyName: '', workingCondition: '', creditLimit: 0 },
      ],
      requestedConditions: operationalDetails.requestedConditions || [],
    },
  })

  const { fields: partnerFields } = useFieldArray({
    control,
    name: 'currentPartners',
  })

  const selectedCategories = watch('productCategories')
  const selectedConditions = watch('requestedConditions')

  const onSubmit = async (data: Step3FormData) => {
    setOperationalDetails(data)
    setCurrentStep(4)
    router.push('/register/step-4')
  }

  return (
    <div className="max-w-6xl mx-auto">
      {/* Step Indicator */}
      <div className="mb-8">
        <StepIndicator currentStep={3} />
      </div>

      {/* Form Card */}
      <Card className="p-8">
        <form onSubmit={handleSubmit(onSubmit)}>
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
            {/* Left Section */}
            <div className="space-y-6">
              {/* Personnel & Structure */}
              <div>
                <h2 className="form-section-title">İşletme Yapısı</h2>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <Select
                    label="Personel Sayısı"
                    {...register('employeeCount')}
                    error={errors.employeeCount?.message}
                  >
                    <option value="">Seçiniz</option>
                    <option value="1-9">1-9</option>
                    <option value="10-19">10-19</option>
                    <option value="20-29">20-29</option>
                    <option value="30-39">30-39</option>
                    <option value="40-49">40-49</option>
                    <option value="50+">50+</option>
                  </Select>
                  <Select
                    label="İşletme Yapısı"
                    {...register('businessStructure')}
                    error={errors.businessStructure?.message}
                  >
                    <option value="">Seçiniz</option>
                    <option value="ofis">Ofis/Büro</option>
                    <option value="magaza">Mağaza</option>
                    <option value="home_ofis">Home Ofis</option>
                  </Select>
                </div>
              </div>

              {/* Revenue Info */}
              <div>
                <h3 className="form-section-title">Ciro Bilgisi</h3>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <Input
                    type="number"
                    label="Geçen Yıl Cirosu"
                    placeholder="TL"
                    {...register('revenueYear', { valueAsNumber: true })}
                    error={errors.revenueYear?.message}
                  />
                  <Input
                    type="number"
                    label="Bu Yıl Hedeflenen"
                    placeholder="TL"
                    {...register('targetRevenue', { valueAsNumber: true })}
                    error={errors.targetRevenue?.message}
                  />
                </div>
              </div>

              {/* Customer Base */}
              <div>
                <h3 className="form-section-title">Müşteri Kitlesi (%)</h3>
                <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
                  <Input
                    type="number"
                    label="Satıcı & Kurgu"
                    placeholder="%"
                    min={0}
                    max={100}
                    {...register('customerBase.retailer', { valueAsNumber: true })}
                  />
                  <Input
                    type="number"
                    label="Kurumsal"
                    placeholder="%"
                    min={0}
                    max={100}
                    {...register('customerBase.corporate', { valueAsNumber: true })}
                  />
                  <Input
                    type="number"
                    label="Boya"
                    placeholder="%"
                    min={0}
                    max={100}
                    {...register('customerBase.construction', { valueAsNumber: true })}
                  />
                  <Input
                    type="number"
                    label="Perakende"
                    placeholder="%"
                    min={0}
                    max={100}
                    {...register('customerBase.retail', { valueAsNumber: true })}
                  />
                </div>
              </div>

              {/* Product Categories */}
              <div>
                <h3 className="form-section-title">Satışını Gerçekleştirdiğiniz Ürün Kategorileri</h3>
                <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
                  {PRODUCT_CATEGORIES.map((category) => (
                    <Controller
                      key={category.value}
                      name="productCategories"
                      control={control}
                      render={({ field }) => (
                        <Checkbox
                          label={category.label}
                          checked={field.value?.includes(category.value)}
                          onChange={(e) => {
                            const newValue = e.target.checked
                              ? [...(field.value || []), category.value]
                              : field.value?.filter((v: string) => v !== category.value) || []
                            field.onChange(newValue)
                          }}
                        />
                      )}
                    />
                  ))}
                </div>
                {errors.productCategories && (
                  <p className="input-error mt-2">{errors.productCategories.message}</p>
                )}
              </div>
            </div>

            {/* Right Section */}
            <div className="space-y-6">
              {/* Current Partners */}
              <div>
                <h2 className="form-section-title">Çalışmakta Olduğunuz Şirketler & Çalışma Koşulları</h2>
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead>
                      <tr className="border-b border-gray-200">
                        <th className="text-left py-2 px-2 text-sm font-medium text-gray-700">Şirket Adı</th>
                        <th className="text-left py-2 px-2 text-sm font-medium text-gray-700">Çalışma Koşulu</th>
                        <th className="text-left py-2 px-2 text-sm font-medium text-gray-700">Limit (USD)</th>
                      </tr>
                    </thead>
                    <tbody>
                      {partnerFields.map((field, index) => (
                        <tr key={field.id} className="border-b border-gray-100">
                          <td className="py-2 px-2">
                            <input
                              type="text"
                              placeholder="Şirket Adı"
                              className="input-field text-sm"
                              {...register(`currentPartners.${index}.companyName`)}
                            />
                          </td>
                          <td className="py-2 px-2">
                            <select
                              className="input-field text-sm"
                              {...register(`currentPartners.${index}.workingCondition`)}
                            >
                              <option value="">Seçiniz</option>
                              <option value="nakit_kredi">Nakit/Kredi Kartı</option>
                              <option value="acik_hesap">Açık Hesap</option>
                              <option value="cek">Çek</option>
                            </select>
                          </td>
                          <td className="py-2 px-2">
                            <input
                              type="number"
                              placeholder="USD"
                              className="input-field text-sm w-28"
                              {...register(`currentPartners.${index}.creditLimit`, { valueAsNumber: true })}
                            />
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>

              {/* Requested Working Conditions */}
              <div>
                <h3 className="form-section-title">Talep Ettiğiniz Çalışma Koşulları</h3>
                <div className="space-y-3">
                  {WORKING_CONDITIONS.map((condition) => (
                    <Controller
                      key={condition.value}
                      name="requestedConditions"
                      control={control}
                      render={({ field }) => (
                        <Checkbox
                          label={condition.label}
                          checked={field.value?.includes(condition.value)}
                          onChange={(e) => {
                            const newValue = e.target.checked
                              ? [...(field.value || []), condition.value]
                              : field.value?.filter((v: string) => v !== condition.value) || []
                            field.onChange(newValue)
                          }}
                        />
                      )}
                    />
                  ))}
                </div>
                {errors.requestedConditions && (
                  <p className="input-error mt-2">{errors.requestedConditions.message}</p>
                )}
              </div>
            </div>
          </div>

          <div className="flex justify-between mt-8 pt-6 border-t border-gray-200">
            <Button
              type="button"
              variant="secondary"
              onClick={() => router.push('/register/step-2')}
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
