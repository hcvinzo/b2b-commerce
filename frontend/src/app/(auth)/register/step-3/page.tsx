'use client'

import { useRouter } from 'next/navigation'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useState, useEffect } from 'react'
import { Loader2 } from 'lucide-react'

import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Input } from '@/components/ui/input'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import {
  Form,
  FormField,
  FormItem,
  FormMessage,
} from '@/components/ui/form'
import { StepIndicator } from '@/components/ui/StepIndicator'
import { SingleSelectAttribute } from '@/components/ui/single-select-attribute'
import { MultiSelectAttribute } from '@/components/ui/multi-select-attribute'
import {
  CompositeAttributeInput,
  type CompositeAttributeField,
  type CompositeAttributeValue,
} from '@/components/ui/composite-attribute-input'
import { useRegistrationStore } from '@/stores/registrationStore'
import { step3Schema, Step3FormData } from '@/lib/validations/registration.schema'
import { getAttributeByCode, getChildAttributes } from '@/lib/api'

// Map attribute type to field type for composite attributes
function mapAttributeTypeToFieldType(type: number | string, isList?: boolean): 'text' | 'number' | 'tc_kimlik' | 'select' {
  const typeValue = typeof type === 'string' ? type.toLowerCase() : type
  // Type 2 = Select, Type 3 = MultiSelect, or isList indicates a select
  if (typeValue === 2 || typeValue === 3 || typeValue === 'select' || typeValue === 'multiselect' || isList) return 'select'
  if (typeValue === 1 || typeValue === 'number') return 'number'
  if (typeValue === 'tc_kimlik') return 'tc_kimlik'
  return 'text'
}

export default function RegisterStep3Page() {
  const router = useRouter()
  const { operationalDetails, setOperationalDetails, setCurrentStep } = useRegistrationStore()

  // State for composite attributes
  const [ciroFields, setCiroFields] = useState<CompositeAttributeField[]>([])
  const [ciroTitle, setCiroTitle] = useState<string>('Ciro Bilgisi')
  const [musteriKitlesiFields, setMusteriKitlesiFields] = useState<CompositeAttributeField[]>([])
  const [musteriKitlesiTitle, setMusteriKitlesiTitle] = useState<string>('Müşteri Kitlesi (%)')
  const [isOrtaklariFields, setIsOrtaklariFields] = useState<CompositeAttributeField[]>([])
  const [isOrtaklariTitle, setIsOrtaklariTitle] = useState<string>('Çalışmakta Olduğunuz Şirketler & Çalışma Koşulları')
  const [isLoadingAttributes, setIsLoadingAttributes] = useState(true)

  // Composite attribute values (managed separately from form)
  const [ciroValues, setCiroValues] = useState<CompositeAttributeValue[]>(() => {
    const existing = operationalDetails.ciro
    if (existing && existing.length > 0) return existing as CompositeAttributeValue[]
    return [{}]
  })

  const [musteriKitlesiValues, setMusteriKitlesiValues] = useState<CompositeAttributeValue[]>(() => {
    const existing = operationalDetails.musteriKitlesi
    if (existing && existing.length > 0) return existing as CompositeAttributeValue[]
    return [{}]
  })

  const [isOrtaklariValues, setIsOrtaklariValues] = useState<CompositeAttributeValue[]>(() => {
    const existing = operationalDetails.isOrtaklari
    if (existing && existing.length > 0) return existing as CompositeAttributeValue[]
    return [{}, {}, {}, {}]
  })

  // Load composite attribute definitions
  useEffect(() => {
    async function loadCompositeAttributes() {
      try {
        // Load Ciro attribute
        const ciroAttr = await getAttributeByCode('ciro')
        setCiroTitle(ciroAttr.name)
        const ciroChildren = await getChildAttributes(ciroAttr.id)
        setCiroFields(
          ciroChildren
            .sort((a, b) => a.displayOrder - b.displayOrder)
            .map((attr) => ({
              code: attr.code,
              name: attr.name,
              type: mapAttributeTypeToFieldType(attr.type, attr.isList),
              placeholder: attr.name,
              required: attr.isRequired,
              options: attr.predefinedValues?.map((v) => ({
                value: v.value,
                label: v.displayText || v.value,
              })),
            }))
        )

        // Load Müşteri Kitlesi attribute
        const musteriAttr = await getAttributeByCode('musteri_kitlesi')
        setMusteriKitlesiTitle(musteriAttr.name)
        const musteriChildren = await getChildAttributes(musteriAttr.id)
        setMusteriKitlesiFields(
          musteriChildren
            .sort((a, b) => a.displayOrder - b.displayOrder)
            .map((attr) => ({
              code: attr.code,
              name: attr.name,
              type: 'number' as const,
              placeholder: '%',
              required: attr.isRequired,
              min: 0,
              max: 100,
            }))
        )

        // Load İş Ortakları attribute
        const isOrtaklariAttr = await getAttributeByCode('is_ortaklari')
        setIsOrtaklariTitle(isOrtaklariAttr.name)
        const isOrtaklariChildren = await getChildAttributes(isOrtaklariAttr.id)
        setIsOrtaklariFields(
          isOrtaklariChildren
            .sort((a, b) => a.displayOrder - b.displayOrder)
            .map((attr) => ({
              code: attr.code,
              name: attr.name,
              type: mapAttributeTypeToFieldType(attr.type, attr.isList),
              placeholder: attr.name,
              required: attr.isRequired,
              options: attr.predefinedValues?.map((v) => ({
                value: v.value,
                label: v.displayText || v.value,
              })),
            }))
        )
      } catch (error) {
        console.error('Failed to load composite attributes:', error)
        // Set fallback fields
        setCiroFields([
          { code: 'gecen_yil_cirosu', name: 'Geçen Yıl Cirosu', type: 'number', placeholder: 'Tutar' },
          { code: 'bu_yil_hedeflenen', name: 'Bu Yıl Hedeflenen', type: 'number', placeholder: 'Tutar' },
          { code: 'para_birimi', name: 'Para Birimi', type: 'text', placeholder: 'TRY' },
        ])
        setMusteriKitlesiFields([
          { code: 'devlet_kamu', name: 'Devlet & Kamu', type: 'number', placeholder: '%', min: 0, max: 100 },
          { code: 'kurumsal', name: 'Kurumsal', type: 'number', placeholder: '%', min: 0, max: 100 },
          { code: 'bayi', name: 'Bayi', type: 'number', placeholder: '%', min: 0, max: 100 },
          { code: 'perakende', name: 'Perakende', type: 'number', placeholder: '%', min: 0, max: 100 },
        ])
        setIsOrtaklariFields([
          { code: 'sirket_adi', name: 'Şirket Adı', type: 'text', placeholder: 'Şirket Adı' },
          { code: 'calisma_kosulu', name: 'Çalışma Koşulu', type: 'text', placeholder: 'Seçiniz' },
          { code: 'limit_usd', name: 'Limit (USD)', type: 'number', placeholder: '0' },
        ])
      } finally {
        setIsLoadingAttributes(false)
      }
    }
    loadCompositeAttributes()
  }, [])

  const form = useForm<Step3FormData>({
    resolver: zodResolver(step3Schema),
    defaultValues: {
      calisanSayisi: operationalDetails.calisanSayisi || '',
      isletmeYapisi: operationalDetails.isletmeYapisi || '',
      satilanUrunKategorileri: operationalDetails.satilanUrunKategorileri || [],
      calismaKosullari: operationalDetails.calismaKosullari || [],
    },
  })

  const onSubmit = async (data: Step3FormData) => {
    // Combine form data with composite attribute values
    const formData = {
      ...data,
      ciro: ciroValues,
      musteriKitlesi: musteriKitlesiValues,
      isOrtaklari: isOrtaklariValues,
    }
    setOperationalDetails(formData)
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
      <Card>
        <CardContent className="p-8">
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)}>
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                {/* Left Section */}
                <div className="space-y-6">
                  {/* İşletme Yapısı */}
                  <div>
                    <h2 className="text-lg font-semibold text-foreground mb-4">İşletme Yapısı</h2>
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                      <FormField
                        control={form.control}
                        name="calisanSayisi"
                        render={({ field }) => (
                          <FormItem>
                            <SingleSelectAttribute
                              attributeCode="personel_sayisi"
                              value={field.value}
                              onChange={field.onChange}
                              label="Personel Sayısı"
                              required
                            />
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                      <FormField
                        control={form.control}
                        name="isletmeYapisi"
                        render={({ field }) => (
                          <FormItem>
                            <SingleSelectAttribute
                              attributeCode="isletme_yapisi"
                              value={field.value}
                              onChange={field.onChange}
                              label="İşletme Yapısı"
                              required
                            />
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                    </div>
                  </div>

                  {/* Ciro Bilgisi - Single value composite */}
                  <div>
                    {isLoadingAttributes ? (
                      <div className="flex items-center gap-2 text-muted-foreground py-4">
                        <Loader2 className="h-4 w-4 animate-spin" />
                        <span className="text-sm">Yükleniyor...</span>
                      </div>
                    ) : (
                      <>
                        <h3 className="text-lg font-semibold text-foreground mb-4">{ciroTitle}</h3>
                        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                          {ciroFields.map((field) => (
                            <div key={field.code}>
                              <label className="text-sm font-medium mb-2 block">{field.name}</label>
                              {field.type === 'select' && field.options ? (
                                <Select
                                  value={String(ciroValues[0]?.[field.code] ?? '')}
                                  onValueChange={(value) => {
                                    setCiroValues([{ ...ciroValues[0], [field.code]: value }])
                                  }}
                                >
                                  <SelectTrigger>
                                    <SelectValue placeholder={field.placeholder} />
                                  </SelectTrigger>
                                  <SelectContent>
                                    {field.options.map((option) => (
                                      <SelectItem key={option.value} value={option.value}>
                                        {option.label}
                                      </SelectItem>
                                    ))}
                                  </SelectContent>
                                </Select>
                              ) : (
                                <Input
                                  type={field.type === 'number' ? 'number' : 'text'}
                                  placeholder={field.placeholder}
                                  value={ciroValues[0]?.[field.code] ?? ''}
                                  onChange={(e) => {
                                    const value = field.type === 'number'
                                      ? (parseFloat(e.target.value) || 0)
                                      : e.target.value
                                    setCiroValues([{ ...ciroValues[0], [field.code]: value }])
                                  }}
                                />
                              )}
                            </div>
                          ))}
                        </div>
                      </>
                    )}
                  </div>

                  {/* Müşteri Kitlesi - Single value composite */}
                  <div>
                    {isLoadingAttributes ? (
                      <div className="flex items-center gap-2 text-muted-foreground py-4">
                        <Loader2 className="h-4 w-4 animate-spin" />
                        <span className="text-sm">Yükleniyor...</span>
                      </div>
                    ) : (
                      <>
                        <h3 className="text-lg font-semibold text-foreground mb-4">{musteriKitlesiTitle}</h3>
                        <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
                          {musteriKitlesiFields.map((field) => (
                            <div key={field.code}>
                              <label className="text-sm font-medium mb-2 block">{field.name}</label>
                              <Input
                                type="number"
                                placeholder={field.placeholder}
                                min={field.min}
                                max={field.max}
                                value={musteriKitlesiValues[0]?.[field.code] ?? 0}
                                onChange={(e) => {
                                  const value = parseFloat(e.target.value) || 0
                                  setMusteriKitlesiValues([{ ...musteriKitlesiValues[0], [field.code]: value }])
                                }}
                              />
                            </div>
                          ))}
                        </div>
                      </>
                    )}
                  </div>

                  {/* Satışını Gerçekleştirdiğiniz Ürün Kategorileri */}
                  <FormField
                    control={form.control}
                    name="satilanUrunKategorileri"
                    render={({ field }) => (
                      <FormItem>
                        <MultiSelectAttribute
                          attributeCode="satilan_urun_kategorileri"
                          value={field.value}
                          onChange={field.onChange}
                          label="Satışını Gerçekleştirdiğiniz Ürün Kategorileri"
                          columns={3}
                        />
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>

                {/* Right Section */}
                <div className="space-y-6">
                  {/* Çalışmakta Olduğunuz Şirketler - Multi value composite */}
                  <div>
                    {isLoadingAttributes ? (
                      <div className="flex items-center gap-2 text-muted-foreground py-8">
                        <Loader2 className="h-4 w-4 animate-spin" />
                        <span className="text-sm">Yükleniyor...</span>
                      </div>
                    ) : (
                      <CompositeAttributeInput
                        title={isOrtaklariTitle}
                        fields={isOrtaklariFields}
                        values={isOrtaklariValues}
                        onChange={setIsOrtaklariValues}
                        minRows={1}
                        maxRows={10}
                        addButtonText={`Şirket ekle`}
                      />
                    )}
                  </div>

                  {/* Talep Ettiğiniz Çalışma Koşulları */}
                  <FormField
                    control={form.control}
                    name="calismaKosullari"
                    render={({ field }) => (
                      <FormItem>
                        <MultiSelectAttribute
                          attributeCode="calisma_kosullari"
                          value={field.value}
                          onChange={field.onChange}
                          label="Talep Ettiğiniz Çalışma Koşulları"
                          columns={1}
                        />
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>
              </div>

              <div className="flex justify-between mt-8 pt-6 border-t border-border">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => router.push('/register/step-2')}
                >
                  Geri
                </Button>
                <Button type="submit" disabled={form.formState.isSubmitting}>
                  {form.formState.isSubmitting && <Loader2 className="animate-spin" />}
                  Kaydet & Devam Et
                </Button>
              </div>
            </form>
          </Form>
        </CardContent>
      </Card>
    </div>
  )
}
