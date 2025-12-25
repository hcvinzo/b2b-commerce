'use client'

import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useForm, useFieldArray, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { CheckCircle, Loader2 } from 'lucide-react'

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
import { FileUpload } from '@/components/ui/FileUpload'
import { useRegistrationStore } from '@/stores/registrationStore'
import { step4Schema, Step4FormData } from '@/lib/validations/registration.schema'
import { registerDealer, uploadRegistrationDocument, saveCustomerDocument } from '@/lib/api'
import type { DealerRegistrationDto, CustomerDocumentType, UpsertCustomerAttributesDto } from '@/types'

// Document type mapping from form fields to API types
const DOCUMENT_TYPE_MAP: Record<keyof NonNullable<Step4FormData['documents']>, CustomerDocumentType> = {
  taxCertificate: 'TaxCertificate',
  signatureCircular: 'SignatureCircular',
  tradeRegistry: 'TradeRegistry',
  partnershipAgreement: 'PartnershipAgreement',
  authorizedIdCopy: 'AuthorizedIdCopy',
  authorizedResidenceDocument: 'AuthorizedResidenceDocument',
}

interface UploadedDocument {
  documentType: CustomerDocumentType
  url: string
  fileName: string
  fileType: string
  fileSize: number
}

const GUARANTEE_TYPES = [
  { value: 'cek', label: 'Çek' },
  { value: 'senet', label: 'Senet' },
  { value: 'teminat_mektubu', label: 'Teminat Mektubu' },
  { value: 'ipotek', label: 'İpotek' },
]

const CURRENCIES = [
  { value: 'TRY', label: 'TRY' },
  { value: 'USD', label: 'USD' },
  { value: 'EUR', label: 'EUR' },
]

export default function RegisterStep4Page() {
  const router = useRouter()
  const [submitError, setSubmitError] = useState<string | null>(null)
  const [submitSuccess, setSubmitSuccess] = useState(false)
  const [uploadProgress, setUploadProgress] = useState<string | null>(null)
  const {
    contactPerson,
    businessInfo,
    operationalDetails,
    bankingDocuments,
    setBankingDocuments,
    reset,
  } = useRegistrationStore()

  const form = useForm<Step4FormData>({
    resolver: zodResolver(step4Schema),
    defaultValues: {
      bankAccounts: bankingDocuments.bankAccounts || [
        { bankName: '', iban: '' },
        { bankName: '', iban: '' },
        { bankName: '', iban: '' },
        { bankName: '', iban: '' },
        { bankName: '', iban: '' },
        { bankName: '', iban: '' },
      ],
      collaterals: bankingDocuments.collaterals || [
        { type: '', amount: 0, currency: 'TRY' },
        { type: '', amount: 0, currency: 'TRY' },
        { type: '', amount: 0, currency: 'TRY' },
      ],
      documents: bankingDocuments.documents || {},
    },
  })

  const { fields: bankFields } = useFieldArray({
    control: form.control,
    name: 'bankAccounts',
  })

  const { fields: collateralFields } = useFieldArray({
    control: form.control,
    name: 'collaterals',
  })

  // Clear registration data after success is shown
  useEffect(() => {
    if (submitSuccess) {
      // Delay the reset to ensure success screen is rendered first
      const timer = setTimeout(() => {
        reset()
        form.reset()
      }, 100)
      return () => clearTimeout(timer)
    }
  }, [submitSuccess, reset, form])

  const onSubmit = async (data: Step4FormData) => {
    setSubmitError(null)
    setUploadProgress(null)
    setBankingDocuments(data)

    try {
      // Step 1: Upload all documents to S3 first
      const uploadedDocuments: UploadedDocument[] = []

      if (data.documents) {
        const documentEntries = Object.entries(data.documents) as [keyof typeof DOCUMENT_TYPE_MAP, File | undefined][]
        const documentsToUpload = documentEntries.filter(([, file]) => file instanceof File)

        if (documentsToUpload.length > 0) {
          setUploadProgress(`Belgeler yükleniyor... (0/${documentsToUpload.length})`)

          for (let i = 0; i < documentsToUpload.length; i++) {
            const [docKey, file] = documentsToUpload[i]
            if (file) {
              setUploadProgress(`Belgeler yükleniyor... (${i + 1}/${documentsToUpload.length})`)
              try {
                const uploadResult = await uploadRegistrationDocument(file)
                uploadedDocuments.push({
                  documentType: DOCUMENT_TYPE_MAP[docKey],
                  url: uploadResult.url,
                  fileName: uploadResult.fileName,
                  fileType: uploadResult.contentType,
                  fileSize: uploadResult.size,
                })
              } catch (uploadError) {
                console.error(`Failed to upload ${docKey}:`, uploadError)
                throw new Error(`Belge yüklenirken hata oluştu: ${file.name}`)
              }
            }
          }
        }
      }

      setUploadProgress('Kayıt işlemi yapılıyor...')

      // Step 2: Build customer attributes from all form data
      const attributes: UpsertCustomerAttributesDto[] = []

      // Step 2: Authorized Persons → ShareholderOrDirector
      const shareholders = businessInfo.authorizedPersons?.filter(p => p.fullName) || []
      if (shareholders.length > 0) {
        attributes.push({
          attributeType: 'ShareholderOrDirector',
          items: shareholders.map((s, i) => ({
            displayOrder: i,
            jsonData: JSON.stringify({
              fullName: s.fullName || '',
              identityNumber: s.tcNumber || '',
              sharePercentage: s.sharePercentage || 0
            })
          }))
        })
      }

      // Step 3: Current Partners → BusinessPartner
      const partners = operationalDetails.currentPartners?.filter(p => p.companyName) || []
      if (partners.length > 0) {
        attributes.push({
          attributeType: 'BusinessPartner',
          items: partners.map((p, i) => ({
            displayOrder: i,
            jsonData: JSON.stringify({
              companyName: p.companyName || '',
              paymentTerm: p.workingCondition || '',
              creditLimitUsd: p.creditLimit || 0
            })
          }))
        })
      }

      // Step 3: Product Categories → ProductCategory
      const categories = operationalDetails.productCategories || []
      if (categories.length > 0) {
        attributes.push({
          attributeType: 'ProductCategory',
          items: [{
            displayOrder: 0,
            jsonData: JSON.stringify({ categories })
          }]
        })
      }

      // Step 4: Bank Accounts → BankAccount
      const banks = data.bankAccounts?.filter(b => b.bankName) || []
      if (banks.length > 0) {
        attributes.push({
          attributeType: 'BankAccount',
          items: banks.map((b, i) => ({
            displayOrder: i,
            jsonData: JSON.stringify({
              bankName: b.bankName || '',
              iban: b.iban || ''
            })
          }))
        })
      }

      // Step 4: Collaterals → Collateral
      const collaterals = data.collaterals?.filter(c => c.type) || []
      if (collaterals.length > 0) {
        attributes.push({
          attributeType: 'Collateral',
          items: collaterals.map((c, i) => ({
            displayOrder: i,
            jsonData: JSON.stringify({
              type: c.type || '',
              amount: c.amount || 0,
              currency: c.currency || 'TRY'
            })
          }))
        })
      }

      // Step 3: Requested Conditions → PaymentPreference
      const preferences = operationalDetails.requestedConditions || []
      if (preferences.length > 0) {
        attributes.push({
          attributeType: 'PaymentPreference',
          items: [{
            displayOrder: 0,
            jsonData: JSON.stringify({ preferences })
          }]
        })
      }

      // Step 3: Map form data to backend DTO and register customer (with attributes)
      const registrationDto: DealerRegistrationDto = {
        // Required company info
        companyName: businessInfo.companyTitle || '',
        taxNumber: businessInfo.taxNumber || '',
        taxOffice: businessInfo.taxOffice || '',
        email: contactPerson.email || '',
        phone: businessInfo.phone || contactPerson.workPhone || '',
        contactPersonName: `${contactPerson.firstName || ''} ${contactPerson.lastName || ''}`.trim(),
        contactPersonTitle: contactPerson.position || '',
        // Optional company info
        mobilePhone: contactPerson.mobile || undefined,
        website: businessInfo.website || undefined,
        // Customer attributes
        attributes: attributes.length > 0 ? attributes : undefined,
      }

      // Submit to API
      const registrationResult = await registerDealer(registrationDto)
      const customerId = registrationResult.id

      // Step 4: Associate uploaded documents with the customer
      if (uploadedDocuments.length > 0 && customerId) {
        setUploadProgress('Belgeler kaydediliyor...')

        for (const doc of uploadedDocuments) {
          try {
            await saveCustomerDocument(
              customerId,
              doc.documentType,
              doc.url,
              doc.fileName,
              doc.fileType,
              doc.fileSize
            )
          } catch (docError) {
            console.error(`Failed to save document ${doc.documentType}:`, docError)
            // Continue with other documents even if one fails
          }
        }
      }

      // Show success message
      setUploadProgress(null)
      setSubmitSuccess(true)
      // Note: Store and form reset is handled by useEffect after success screen is rendered
    } catch (error: any) {
      console.error('Registration error:', error)
      setUploadProgress(null)

      // Handle specific error codes from backend
      const errorCode = error?.response?.data?.code
      const errorMessage = error?.response?.data?.message

      if (errorCode === 'EMAIL_EXISTS') {
        setSubmitError('Bu e-posta adresi zaten kayıtlı. Lütfen farklı bir e-posta adresi kullanınız.')
      } else if (errorCode === 'TAX_NUMBER_EXISTS') {
        setSubmitError('Bu vergi numarası zaten kayıtlı. Lütfen vergi numaranızı kontrol ediniz.')
      } else if (error.message?.includes('Belge yüklenirken')) {
        setSubmitError(error.message)
      } else {
        setSubmitError(errorMessage || 'Kayıt işlemi başarısız oldu. Lütfen tekrar deneyiniz.')
      }
    }
  }

  // Success screen
  if (submitSuccess) {
    return (
      <div className="max-w-2xl mx-auto">
        <Card>
          <CardContent className="p-8 text-center">
            <div className="flex justify-center mb-6">
              <div className="w-20 h-20 bg-green-100 rounded-full flex items-center justify-center">
                <CheckCircle className="w-12 h-12 text-green-600" />
              </div>
            </div>
            <h2 className="text-2xl font-semibold text-foreground mb-4">
              Başvurunuz Alındı!
            </h2>
            <p className="text-muted-foreground mb-6">
              Bayi başvurunuz başarıyla alınmıştır. Tüm bilgileriniz ilgili yetkili kişiye
              e-posta olarak iletilmiştir. Başvurunuz incelendikten sonra sizinle
              iletişime geçilecektir.
            </p>
            <div className="bg-primary/10 rounded-lg p-4 mb-6">
              <p className="text-sm text-primary">
                <strong>Not:</strong> Başvurunuzun değerlendirilmesi 1-3 iş günü içinde
                tamamlanacaktır. Onay durumu hakkında kayıtlı e-posta adresinize
                bilgilendirme yapılacaktır.
              </p>
            </div>
            <Button onClick={() => router.push('/login')}>
              Giriş Sayfasına Dön
            </Button>
          </CardContent>
        </Card>
      </div>
    )
  }

  return (
    <div className="max-w-6xl mx-auto">
      {/* Step Indicator */}
      <div className="mb-8">
        <StepIndicator currentStep={4} />
      </div>

      {/* Form Card */}
      <Card>
        <CardContent className="p-8">
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)}>
              {submitError && (
                <div className="mb-6 p-4 bg-destructive/10 border border-destructive/20 rounded-md">
                  <p className="text-sm text-destructive">{submitError}</p>
                </div>
              )}

              {uploadProgress && (
                <div className="mb-6 p-4 bg-blue-50 border border-blue-200 rounded-md">
                  <p className="text-sm text-blue-600">{uploadProgress}</p>
                </div>
              )}

              <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                {/* Left Section - Bank Accounts */}
                <div>
                  <h2 className="text-lg font-semibold text-foreground mb-4">Banka Hesap Bilgileri</h2>
                  <div className="overflow-x-auto">
                    <table className="w-full">
                      <thead>
                        <tr className="border-b border-border">
                          <th className="text-left py-2 px-2 text-sm font-medium text-muted-foreground">Banka Adı</th>
                          <th className="text-left py-2 px-2 text-sm font-medium text-muted-foreground">IBAN</th>
                        </tr>
                      </thead>
                      <tbody>
                        {bankFields.map((field, index) => (
                          <tr key={field.id} className="border-b border-border/50">
                            <td className="py-2 px-2">
                              <Input
                                placeholder="Banka Adı"
                                className="text-sm"
                                {...form.register(`bankAccounts.${index}.bankName`)}
                              />
                            </td>
                            <td className="py-2 px-2">
                              <div>
                                <Input
                                  placeholder="TR00 0000 0000 0000 0000 0000 00"
                                  className="text-sm"
                                  {...form.register(`bankAccounts.${index}.iban`)}
                                />
                                {form.formState.errors.bankAccounts?.[index]?.iban && (
                                  <p className="text-xs text-destructive mt-1">
                                    {form.formState.errors.bankAccounts[index]?.iban?.message}
                                  </p>
                                )}
                              </div>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                  {form.formState.errors.bankAccounts && (
                    <p className="text-destructive text-sm mt-2">{form.formState.errors.bankAccounts.message}</p>
                  )}
                </div>

                {/* Right Section - Collaterals & Documents */}
                <div className="space-y-6">
                  {/* Collaterals */}
                  <div>
                    <h2 className="text-lg font-semibold text-foreground mb-4">Teminatlar</h2>
                    <div className="overflow-x-auto">
                      <table className="w-full">
                        <thead>
                          <tr className="border-b border-border">
                            <th className="text-left py-2 px-2 text-sm font-medium text-muted-foreground">Teminat Türü</th>
                            <th className="text-left py-2 px-2 text-sm font-medium text-muted-foreground">Tutar</th>
                            <th className="text-left py-2 px-2 text-sm font-medium text-muted-foreground">Para Birimi</th>
                          </tr>
                        </thead>
                        <tbody>
                          {collateralFields.map((field, index) => (
                            <tr key={field.id} className="border-b border-border/50">
                              <td className="py-2 px-2">
                                <Controller
                                  control={form.control}
                                  name={`collaterals.${index}.type`}
                                  render={({ field }) => (
                                    <Select onValueChange={field.onChange} defaultValue={field.value}>
                                      <SelectTrigger className="w-full text-sm">
                                        <SelectValue placeholder="Seçiniz" />
                                      </SelectTrigger>
                                      <SelectContent>
                                        {GUARANTEE_TYPES.map((type) => (
                                          <SelectItem key={type.value} value={type.value}>
                                            {type.label}
                                          </SelectItem>
                                        ))}
                                      </SelectContent>
                                    </Select>
                                  )}
                                />
                              </td>
                              <td className="py-2 px-2">
                                <Input
                                  type="number"
                                  placeholder="Tutar"
                                  className="text-sm w-28"
                                  {...form.register(`collaterals.${index}.amount`, { valueAsNumber: true })}
                                />
                              </td>
                              <td className="py-2 px-2">
                                <Controller
                                  control={form.control}
                                  name={`collaterals.${index}.currency`}
                                  render={({ field }) => (
                                    <Select onValueChange={field.onChange} defaultValue={field.value}>
                                      <SelectTrigger className="w-24 text-sm">
                                        <SelectValue placeholder="TRY" />
                                      </SelectTrigger>
                                      <SelectContent>
                                        {CURRENCIES.map((currency) => (
                                          <SelectItem key={currency.value} value={currency.value}>
                                            {currency.label}
                                          </SelectItem>
                                        ))}
                                      </SelectContent>
                                    </Select>
                                  )}
                                />
                              </td>
                            </tr>
                          ))}
                        </tbody>
                      </table>
                    </div>
                  </div>

                  {/* Documents */}
                  <div>
                    <h2 className="text-lg font-semibold text-foreground mb-4">Evrak & Belgeler</h2>
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                      <Controller
                        name="documents.taxCertificate"
                        control={form.control}
                        render={({ field }) => (
                          <FileUpload
                            label="Vergi Levhası (Son)"
                            onChange={field.onChange}
                            value={field.value}
                          />
                        )}
                      />
                      <Controller
                        name="documents.signatureCircular"
                        control={form.control}
                        render={({ field }) => (
                          <FileUpload
                            label="İmza Sirküleri"
                            onChange={field.onChange}
                            value={field.value}
                          />
                        )}
                      />
                      <Controller
                        name="documents.tradeRegistry"
                        control={form.control}
                        render={({ field }) => (
                          <FileUpload
                            label="Sicil Gazetesi (Son)"
                            onChange={field.onChange}
                            value={field.value}
                          />
                        )}
                      />
                      <Controller
                        name="documents.partnershipAgreement"
                        control={form.control}
                        render={({ field }) => (
                          <FileUpload
                            label="İş Ortağı Sözleşmesi"
                            onChange={field.onChange}
                            value={field.value}
                          />
                        )}
                      />
                      <Controller
                        name="documents.authorizedIdCopy"
                        control={form.control}
                        render={({ field }) => (
                          <FileUpload
                            label="Yetkili Kimlik Fotokopisi"
                            onChange={field.onChange}
                            value={field.value}
                          />
                        )}
                      />
                      <Controller
                        name="documents.authorizedResidenceDocument"
                        control={form.control}
                        render={({ field }) => (
                          <FileUpload
                            label="Yetkili İkamet Belgesi"
                            onChange={field.onChange}
                            value={field.value}
                          />
                        )}
                      />
                    </div>
                  </div>
                </div>
              </div>

              <div className="flex justify-between mt-8 pt-6 border-t border-border">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => router.push('/register/step-3')}
                >
                  Geri
                </Button>
                <Button type="submit" disabled={form.formState.isSubmitting}>
                  {form.formState.isSubmitting && <Loader2 className="animate-spin" />}
                  Kaydet & Gönder
                </Button>
              </div>
            </form>
          </Form>
        </CardContent>
      </Card>
    </div>
  )
}
