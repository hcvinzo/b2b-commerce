'use client'

import { useRouter } from 'next/navigation'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useState, useEffect } from 'react'
import { Loader2 } from 'lucide-react'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Card, CardContent } from '@/components/ui/card'
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
import { GeoLocationSelect } from '@/components/ui/geo-location-select'
import {
  CompositeAttributeInput,
  type CompositeAttributeField,
  type CompositeAttributeValue,
} from '@/components/ui/composite-attribute-input'
import { useRegistrationStore } from '@/stores/registrationStore'
import { step2Schema, Step2FormData } from '@/lib/validations/registration.schema'
import { getAttributeByCode, getChildAttributes, type AttributeDefinition } from '@/lib/api'

// Map attribute type to field type
function mapAttributeTypeToFieldType(type: number | string): 'text' | 'number' | 'tc_kimlik' {
  // Handle both numeric and string type values
  const typeValue = typeof type === 'string' ? type.toLowerCase() : type

  if (typeValue === 1 || typeValue === 'number') return 'number'
  if (typeValue === 'tc_kimlik') return 'tc_kimlik'
  return 'text'
}

export default function RegisterStep2Page() {
  const router = useRouter()
  const { businessInfo, setBusinessInfo, setCurrentStep } = useRegistrationStore()

  // State for composite attribute
  const [compositeFields, setCompositeFields] = useState<CompositeAttributeField[]>([])
  const [compositeTitle, setCompositeTitle] = useState<string>('Yetkililer & Ortaklar')
  const [isLoadingAttribute, setIsLoadingAttribute] = useState(true)

  // Helper to normalize person data (ensure ortaklik_payi is number)
  const normalizePersons = (persons: CompositeAttributeValue[]): CompositeAttributeValue[] => {
    return persons.map(person => ({
      ...person,
      ortaklik_payi: typeof person.ortaklik_payi === 'string'
        ? parseFloat(person.ortaklik_payi) || 0
        : (person.ortaklik_payi ?? 0)
    }))
  }

  // Authorized persons state (managed separately from form)
  const [authorizedPersons, setAuthorizedPersons] = useState<CompositeAttributeValue[]>(() => {
    const persons = businessInfo.authorizedPersons
    if (persons && persons.length > 0) {
      return normalizePersons(persons as CompositeAttributeValue[])
    }
    return [{ ad_soyad: '', kimlik_no: '', ortaklik_payi: 0 }]
  })

  // Wrapper to normalize values before setting state
  const handleAuthorizedPersonsChange = (values: CompositeAttributeValue[]) => {
    setAuthorizedPersons(normalizePersons(values))
  }

  // Load composite attribute definition
  useEffect(() => {
    async function loadCompositeAttribute() {
      try {
        // Get the parent attribute by code
        const parentAttribute = await getAttributeByCode('yetkili_ve_ortaklar')
        setCompositeTitle(parentAttribute.name)

        // Get child attributes
        const childAttributes = await getChildAttributes(parentAttribute.id)

        // Map child attributes to fields
        const fields: CompositeAttributeField[] = childAttributes
          .sort((a, b) => a.displayOrder - b.displayOrder)
          .map((attr) => ({
            code: attr.code,
            name: attr.name,
            type: mapAttributeTypeToFieldType(attr.type),
            placeholder: attr.name,
            required: attr.isRequired,
            maxLength: attr.code === 'kimlik_no' ? 11 : undefined,
            min: attr.code === 'ortaklik_payi' ? 0 : undefined,
            max: attr.code === 'ortaklik_payi' ? 100 : undefined,
          }))

        setCompositeFields(fields)
      } catch (error) {
        console.error('Failed to load composite attribute:', error)
        // Fallback to default fields if API fails
        setCompositeFields([
          { code: 'ad_soyad', name: 'Adı Soyadı', type: 'text', placeholder: 'Adı Soyadı' },
          { code: 'kimlik_no', name: 'T.C. Kimlik No', type: 'tc_kimlik', placeholder: 'T.C. Kimlik No', maxLength: 11 },
          { code: 'ortaklik_payi', name: 'Pay Oranı (%)', type: 'number', placeholder: '%', min: 0, max: 100 },
        ])
      } finally {
        setIsLoadingAttribute(false)
      }
    }
    loadCompositeAttribute()
  }, [])

  const form = useForm<Step2FormData>({
    resolver: zodResolver(step2Schema),
    defaultValues: {
      // İşletme Bilgileri
      title: businessInfo.title || '',
      taxOffice: businessInfo.taxOffice || '',
      taxNo: businessInfo.taxNo || '',
      establishmentYear: businessInfo.establishmentYear,
      website: businessInfo.website || '',
      // İletişim
      address: businessInfo.address || '',
      geoLocationId: businessInfo.geoLocationId || '',
      geoLocationPathName: businessInfo.geoLocationPathName || '',
      postalCode: businessInfo.postalCode || '',
      addressPhone: businessInfo.addressPhone || '+90',
      addressPhoneExt: businessInfo.addressPhoneExt || '',
      addressGsm: businessInfo.addressGsm || '+90',
      // Yetkililer & Ortaklar
      authorizedPersons: businessInfo.authorizedPersons || [
        { ad_soyad: '', kimlik_no: '', ortaklik_payi: 0 },
      ],
    },
  })

  // Update form when authorizedPersons change (already normalized by handleAuthorizedPersonsChange)
  useEffect(() => {
    form.setValue('authorizedPersons', authorizedPersons)
  }, [authorizedPersons, form])

  const onSubmit = async (data: Step2FormData) => {
    // Include authorizedPersons from state
    const formData = {
      ...data,
      authorizedPersons,
    }
    setBusinessInfo(formData)
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
      <Card>
        <CardContent className="p-8">
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)}>
              {/* Form Error Summary */}
              {Object.keys(form.formState.errors).length > 0 && (
                <div className="mb-6 p-4 bg-destructive/10 border border-destructive/20 rounded-md">
                  <p className="text-sm font-medium text-destructive mb-2">Lütfen aşağıdaki alanları kontrol ediniz:</p>
                  <ul className="text-sm text-destructive list-disc list-inside">
                    {form.formState.errors.title && <li>Ünvan gereklidir</li>}
                    {form.formState.errors.address && <li>Adres gereklidir</li>}
                    {form.formState.errors.website && <li>Geçerli bir web sitesi URL&apos;si giriniz</li>}
                    {form.formState.errors.establishmentYear && <li>Geçerli bir kuruluş yılı seçiniz</li>}
                    {form.formState.errors.authorizedPersons && (() => {
                      const errors = form.formState.errors.authorizedPersons
                      const errorsAsArray = Array.isArray(errors) ? errors : []
                      const hasKimlikError = errorsAsArray.some((item) => item?.kimlik_no)
                      const hasPayiError = errorsAsArray.some((item) => item?.ortaklik_payi)
                      const rootMessage = errors.root?.message
                      const directMessage = errors.message

                      // Get actual ortaklik_payi error message if exists
                      const payiErrorItem = errorsAsArray.find((item) => item?.ortaklik_payi)
                      const payiErrorMessage = payiErrorItem?.ortaklik_payi?.message

                      return (
                        <>
                          {/* Root level message (e.g., percentage validation, duplicate TC) */}
                          {directMessage && (
                            <li>{String(directMessage)}</li>
                          )}
                          {/* Root error without message (from refine) */}
                          {rootMessage && (
                            <li>{String(rootMessage)}</li>
                          )}
                          {/* Array item errors (e.g., invalid TC Kimlik) */}
                          {hasKimlikError && (
                            <li>Geçersiz T.C. Kimlik Numarası girdiniz</li>
                          )}
                          {/* Array item errors - ortaklik_payi with actual message */}
                          {hasPayiError && (
                            <li>{payiErrorMessage || 'Pay oranı 0 ile 100 arasında olmalıdır'}</li>
                          )}
                          {/* Fallback - show the actual error for debugging */}
                          {!directMessage && !rootMessage && !hasKimlikError && !hasPayiError && (
                            <li>Yetkili/ortak bilgilerini kontrol ediniz (Detay için konsolu kontrol edin)</li>
                          )}
                        </>
                      )
                    })()}
                  </ul>
                </div>
              )}

              <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                {/* İşletme Bilgileri Section */}
                <div>
                  <h2 className="text-lg font-semibold text-foreground mb-4">İşletme Bilgileri</h2>

                  <div className="space-y-4">
                    <FormField
                      control={form.control}
                      name="title"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Ünvan</FormLabel>
                          <FormControl>
                            <Input placeholder="Şirket Ünvanı" {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                      <FormField
                        control={form.control}
                        name="taxOffice"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>Vergi Dairesi</FormLabel>
                            <FormControl>
                              <Input placeholder="Vergi Dairesi" {...field} />
                            </FormControl>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                      <FormField
                        control={form.control}
                        name="taxNo"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>Vergi Numarası</FormLabel>
                            <FormControl>
                              <Input placeholder="Vergi Numarası" {...field} />
                            </FormControl>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                      <FormField
                        control={form.control}
                        name="establishmentYear"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>Kuruluş Yılı</FormLabel>
                            <Select
                              onValueChange={(value) => field.onChange(parseInt(value))}
                              defaultValue={field.value?.toString()}
                            >
                              <FormControl>
                                <SelectTrigger className="w-full">
                                  <SelectValue placeholder="Yıl Seçiniz" />
                                </SelectTrigger>
                              </FormControl>
                              <SelectContent>
                                {Array.from(
                                  { length: new Date().getFullYear() - 1900 + 1 },
                                  (_, i) => new Date().getFullYear() - i
                                ).map((year) => (
                                  <SelectItem key={year} value={year.toString()}>
                                    {year}
                                  </SelectItem>
                                ))}
                              </SelectContent>
                            </Select>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                    </div>

                    <FormField
                      control={form.control}
                      name="website"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>İnternet Sayfası</FormLabel>
                          <FormControl>
                            <Input placeholder="https://" {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                    <h3 className="text-lg font-semibold text-foreground mt-8 mb-4">İletişim</h3>
                    <FormField
                      control={form.control}
                      name="address"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Adres</FormLabel>
                          <FormControl>
                            <Input placeholder="Açık adres" {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    {/* GeoLocation Select (Cascading) */}
                    <FormField
                      control={form.control}
                      name="geoLocationId"
                      render={({ field }) => (
                        <FormItem>
                          <GeoLocationSelect
                            label="Konum"
                            value={field.value}
                            onChange={(geoLocationId, pathName) => {
                              field.onChange(geoLocationId)
                              form.setValue('geoLocationPathName', pathName || '')
                            }}
                            maxDepth={4}
                          />
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <div className="grid grid-cols-1 sm:grid-cols-4 gap-4">
                      <FormField
                        control={form.control}
                        name="postalCode"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>Posta Kodu</FormLabel>
                            <FormControl>
                              <Input placeholder="Posta Kodu" {...field} />
                            </FormControl>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                      <FormField
                        control={form.control}
                        name="addressPhone"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>Telefon</FormLabel>
                            <FormControl>
                              <Input type="tel" placeholder="+90 XXX XXX XX XX" {...field} />
                            </FormControl>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                      <FormField
                        control={form.control}
                        name="addressPhoneExt"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>Dahili</FormLabel>
                            <FormControl>
                              <Input placeholder="Dahili No" {...field} />
                            </FormControl>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                      <FormField
                        control={form.control}
                        name="addressGsm"
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
                    </div>
                  </div>
                </div>

                {/* Right Section - Yetkililer & Ortaklar */}
                <div>
                  {isLoadingAttribute ? (
                    <div className="flex items-center gap-2 text-muted-foreground py-8">
                      <Loader2 className="h-4 w-4 animate-spin" />
                      <span className="text-sm">Yükleniyor...</span>
                    </div>
                  ) : (
                    <CompositeAttributeInput
                      title={compositeTitle}
                      fields={compositeFields}
                      values={authorizedPersons}
                      onChange={handleAuthorizedPersonsChange}
                      minRows={1}
                      maxRows={10}
                      addButtonText={`${compositeTitle} ekle`}
                    />
                  )}
                  {form.formState.errors.authorizedPersons?.message && (
                    <p className="text-destructive text-sm mt-2">
                      {form.formState.errors.authorizedPersons.message}
                    </p>
                  )}
                </div>

              </div>

              {/* Navigation Buttons */}
              <div className="flex justify-between mt-8 pt-6 border-t border-border">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => router.push('/register/step-1')}
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
