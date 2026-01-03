'use client'

import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { CheckCircle, Loader2 } from 'lucide-react'

import { Button } from '@/components/ui/button'
import { Card, CardContent } from '@/components/ui/card'
import { Form } from '@/components/ui/form'
import { StepIndicator } from '@/components/ui/StepIndicator'
import { FileUpload } from '@/components/ui/FileUpload'
import {
  CompositeAttributeInput,
  type CompositeAttributeField,
  type CompositeAttributeValue,
} from '@/components/ui/composite-attribute-input'
import { useRegistrationStore } from '@/stores/registrationStore'
import { step4Schema, Step4FormData } from '@/lib/validations/registration.schema'
import { registerDealer, uploadRegistrationDocument, getAttributeByCode, getChildAttributes } from '@/lib/api'
import type { DealerRegistrationDto, UpsertCustomerAttributesByDefinitionDto } from '@/types'

// Map attribute type to field type for composite attributes
function mapAttributeTypeToFieldType(type: number | string, isList?: boolean): 'text' | 'number' | 'tc_kimlik' | 'select' {
  const typeValue = typeof type === 'string' ? type.toLowerCase() : type
  if (typeValue === 2 || typeValue === 3 || typeValue === 'select' || typeValue === 'multiselect' || isList) return 'select'
  if (typeValue === 1 || typeValue === 'number') return 'number'
  if (typeValue === 'tc_kimlik') return 'tc_kimlik'
  return 'text'
}

// Helper function to build customer attributes for registration
async function buildCustomerAttributes(
  businessInfo: { authorizedPersons?: Array<{ ad_soyad?: string; kimlik_no?: string; ortaklik_payi?: number }> },
  operationalDetails: {
    calisanSayisi?: string
    isletmeYapisi?: string
    ciro?: Array<Record<string, string | number | undefined>>
    musteriKitlesi?: Array<Record<string, string | number | undefined>>
    isOrtaklari?: Array<Record<string, string | number | undefined>>
    satilanUrunKategorileri?: string[]
    calismaKosullari?: string[]
  },
  bankAccountValues: CompositeAttributeValue[],
  teminatValues: CompositeAttributeValue[]
): Promise<UpsertCustomerAttributesByDefinitionDto[]> {
  const attributes: UpsertCustomerAttributesByDefinitionDto[] = []

  try {
    // Step 2 attributes
    // yetkili_ve_ortaklar (composite - multi value)
    if (businessInfo.authorizedPersons && businessInfo.authorizedPersons.length > 0) {
      const attr = await getAttributeByCode('yetkili_ve_ortaklar')
      const nonEmptyPersons = businessInfo.authorizedPersons.filter(
        p => p.ad_soyad || p.kimlik_no || (p.ortaklik_payi && p.ortaklik_payi > 0)
      )
      if (nonEmptyPersons.length > 0) {
        attributes.push({
          attributeDefinitionId: attr.id,
          items: nonEmptyPersons.map(p => ({ value: JSON.stringify(p) }))
        })
      }
    }

    // Step 3 attributes
    // personel_sayisi (single select) - wrap in JSON for jsonb column
    if (operationalDetails.calisanSayisi) {
      const attr = await getAttributeByCode('personel_sayisi')
      attributes.push({
        attributeDefinitionId: attr.id,
        items: [{ value: JSON.stringify(operationalDetails.calisanSayisi) }]
      })
    }

    // isletme_yapisi (single select) - wrap in JSON for jsonb column
    if (operationalDetails.isletmeYapisi) {
      const attr = await getAttributeByCode('isletme_yapisi')
      attributes.push({
        attributeDefinitionId: attr.id,
        items: [{ value: JSON.stringify(operationalDetails.isletmeYapisi) }]
      })
    }

    // ciro (composite - single value)
    if (operationalDetails.ciro && operationalDetails.ciro.length > 0) {
      const ciroData = operationalDetails.ciro[0]
      if (ciroData && Object.keys(ciroData).length > 0) {
        const attr = await getAttributeByCode('ciro')
        attributes.push({
          attributeDefinitionId: attr.id,
          items: [{ value: JSON.stringify(ciroData) }]
        })
      }
    }

    // musteri_kitlesi (composite - single value)
    if (operationalDetails.musteriKitlesi && operationalDetails.musteriKitlesi.length > 0) {
      const musteriData = operationalDetails.musteriKitlesi[0]
      if (musteriData && Object.keys(musteriData).length > 0) {
        const attr = await getAttributeByCode('musteri_kitlesi')
        attributes.push({
          attributeDefinitionId: attr.id,
          items: [{ value: JSON.stringify(musteriData) }]
        })
      }
    }

    // is_ortaklari (composite - multi value)
    if (operationalDetails.isOrtaklari && operationalDetails.isOrtaklari.length > 0) {
      const nonEmptyOrtaklar = operationalDetails.isOrtaklari.filter(
        o => o && Object.values(o).some(v => v !== undefined && v !== '' && v !== 0)
      )
      if (nonEmptyOrtaklar.length > 0) {
        const attr = await getAttributeByCode('is_ortaklari')
        attributes.push({
          attributeDefinitionId: attr.id,
          items: nonEmptyOrtaklar.map(o => ({ value: JSON.stringify(o) }))
        })
      }
    }

    // satilan_urun_kategorileri (multi select) - wrap each in JSON for jsonb column
    if (operationalDetails.satilanUrunKategorileri && operationalDetails.satilanUrunKategorileri.length > 0) {
      const attr = await getAttributeByCode('satilan_urun_kategorileri')
      attributes.push({
        attributeDefinitionId: attr.id,
        items: operationalDetails.satilanUrunKategorileri.map(v => ({ value: JSON.stringify(v) }))
      })
    }

    // calisma_kosullari (multi select) - wrap each in JSON for jsonb column
    if (operationalDetails.calismaKosullari && operationalDetails.calismaKosullari.length > 0) {
      const attr = await getAttributeByCode('calisma_kosullari')
      attributes.push({
        attributeDefinitionId: attr.id,
        items: operationalDetails.calismaKosullari.map(v => ({ value: JSON.stringify(v) }))
      })
    }

    // Step 4 attributes
    // banka_hesap_bilgileri (composite - multi value)
    const nonEmptyBankAccounts = bankAccountValues.filter(
      b => b && (b.banka_adi || b.iban)
    )
    if (nonEmptyBankAccounts.length > 0) {
      const attr = await getAttributeByCode('banka_hesap_bilgileri')
      attributes.push({
        attributeDefinitionId: attr.id,
        items: nonEmptyBankAccounts.map(b => ({ value: JSON.stringify(b) }))
      })
    }

    // teminatlar (composite - multi value)
    const nonEmptyTeminatlar = teminatValues.filter(
      t => t && (t.teminat_turu || (t.tutar && Number(t.tutar) > 0))
    )
    if (nonEmptyTeminatlar.length > 0) {
      const attr = await getAttributeByCode('teminatlar')
      attributes.push({
        attributeDefinitionId: attr.id,
        items: nonEmptyTeminatlar.map(t => ({ value: JSON.stringify(t) }))
      })
    }
  } catch (error) {
    console.error('Failed to build customer attributes:', error)
    // Continue without attributes if there's an error fetching definitions
  }

  return attributes
}

// Document type keys for form
type DocumentKey = 'taxCertificate' | 'signatureCircular' | 'tradeRegistry' | 'partnershipAgreement' | 'authorizedIdCopy' | 'authorizedResidenceDocument'

// Document type mapping for Customer.DocumentUrls JSON
const DOCUMENT_LABELS: Record<DocumentKey, string> = {
  taxCertificate: 'Vergi Levhası (Son)',
  signatureCircular: 'İmza Sirküleri',
  tradeRegistry: 'Sicil Gazetesi (Son)',
  partnershipAgreement: 'İş Ortağı Sözleşmesi',
  authorizedIdCopy: 'Yetkili Kimlik Fotokopisi',
  authorizedResidenceDocument: 'Yetkili İkamet Belgesi',
}

interface DocumentUrl {
  document_type: string
  file_type: string
  file_url: string
  file_name: string
  file_size: number
}

export default function RegisterStep4Page() {
  const router = useRouter()
  const [submitError, setSubmitError] = useState<string | null>(null)
  const [submitSuccess, setSubmitSuccess] = useState(false)
  const [uploadProgress, setUploadProgress] = useState<string | null>(null)
  const [isLoadingAttributes, setIsLoadingAttributes] = useState(true)

  // Dynamic attribute fields
  const [bankAccountFields, setBankAccountFields] = useState<CompositeAttributeField[]>([])
  const [bankAccountTitle, setBankAccountTitle] = useState<string>('Banka Hesap Bilgileri')
  const [teminatFields, setTeminatFields] = useState<CompositeAttributeField[]>([])
  const [teminatTitle, setTeminatTitle] = useState<string>('Teminatlar')

  const {
    contactPerson,
    businessInfo,
    operationalDetails,
    bankingDocuments,
    setBankingDocuments,
    reset,
  } = useRegistrationStore()

  // Composite attribute values (managed separately from form)
  const [bankAccountValues, setBankAccountValues] = useState<CompositeAttributeValue[]>(() => {
    const existing = bankingDocuments.bankAccounts
    if (existing && existing.length > 0) {
      return existing.map(b => ({ banka_adi: b.bankName || '', iban: b.iban || '' }))
    }
    return [{ banka_adi: '', iban: '' }]
  })

  const [teminatValues, setTeminatValues] = useState<CompositeAttributeValue[]>(() => {
    const existing = bankingDocuments.collaterals
    if (existing && existing.length > 0) {
      return existing.map(c => ({
        teminat_turu: c.type || '',
        tutar: c.amount || 0,
        para_birimi: c.currency || 'TRY'
      }))
    }
    return [{ teminat_turu: '', tutar: 0, para_birimi: 'TRY' }]
  })

  // Load composite attribute definitions
  useEffect(() => {
    async function loadAttributes() {
      try {
        // Load Bank Account attribute
        const bankAttr = await getAttributeByCode('banka_hesap_bilgileri')
        setBankAccountTitle(bankAttr.name)
        const bankChildren = await getChildAttributes(bankAttr.id)
        setBankAccountFields(
          bankChildren
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

        // Load Teminatlar attribute
        const teminatAttr = await getAttributeByCode('teminatlar')
        setTeminatTitle(teminatAttr.name)
        const teminatChildren = await getChildAttributes(teminatAttr.id)
        setTeminatFields(
          teminatChildren
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
        console.error('Failed to load attributes:', error)
        // Leave fields empty - form will show loading state or error
      } finally {
        setIsLoadingAttributes(false)
      }
    }
    loadAttributes()
  }, [])

  const form = useForm<Step4FormData>({
    resolver: zodResolver(step4Schema),
    defaultValues: {
      bankAccounts: bankingDocuments.bankAccounts || [],
      collaterals: bankingDocuments.collaterals || [],
      documents: bankingDocuments.documents || {},
    },
  })

  // Clear registration data after success is shown
  useEffect(() => {
    if (submitSuccess) {
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

    // Save to store with current values
    setBankingDocuments({
      bankAccounts: bankAccountValues.map(b => ({
        bankName: String(b.banka_adi || ''),
        iban: String(b.iban || ''),
      })),
      collaterals: teminatValues.map(t => ({
        type: String(t.teminat_turu || ''),
        amount: Number(t.tutar || 0),
        currency: (String(t.para_birimi || 'TRY')) as 'TRY' | 'USD' | 'EUR',
      })),
      documents: data.documents || {},
    })

    try {
      // Step 1: Upload all documents to S3 and build DocumentUrls array
      const documentUrls: DocumentUrl[] = []

      if (data.documents) {
        const documentEntries = Object.entries(data.documents) as [DocumentKey, File | undefined][]
        const documentsToUpload = documentEntries.filter(([, file]) => file instanceof File)

        if (documentsToUpload.length > 0) {
          setUploadProgress(`Belgeler yükleniyor... (0/${documentsToUpload.length})`)

          for (let i = 0; i < documentsToUpload.length; i++) {
            const [docKey, file] = documentsToUpload[i]
            if (file) {
              setUploadProgress(`Belgeler yükleniyor... (${i + 1}/${documentsToUpload.length})`)
              try {
                const uploadResult = await uploadRegistrationDocument(file)
                documentUrls.push({
                  document_type: docKey,
                  file_type: uploadResult.contentType,
                  file_url: uploadResult.url,
                  file_name: uploadResult.fileName,
                  file_size: uploadResult.size,
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

      // Build customer attributes from all collected data
      const customerAttributes = await buildCustomerAttributes(
        businessInfo,
        operationalDetails,
        bankAccountValues,
        teminatValues
      )

      // Build registration DTO (matches RegisterCommand)
      const registrationDto: DealerRegistrationDto = {
        // Company info
        title: businessInfo.title || '',
        taxOffice: businessInfo.taxOffice || undefined,
        taxNo: businessInfo.taxNo || undefined,
        establishmentYear: businessInfo.establishmentYear || undefined,
        website: businessInfo.website || undefined,
        // Primary contact info
        contactFirstName: contactPerson.firstName || '',
        contactLastName: contactPerson.lastName || '',
        contactEmail: contactPerson.email || '',
        contactPosition: contactPerson.position || undefined,
        contactDateOfBirth: contactPerson.dateOfBirth || undefined,
        contactGender: contactPerson.gender || undefined,
        contactPhone: contactPerson.phone || undefined,
        contactPhoneExt: contactPerson.phoneExt || undefined,
        contactGsm: contactPerson.gsm || undefined,
        // Primary address info
        addressTitle: 'Merkez',  // Default address title
        address: businessInfo.address || undefined,
        geoLocationId: businessInfo.geoLocationId || undefined,
        postalCode: businessInfo.postalCode || undefined,
        addressPhone: businessInfo.addressPhone || undefined,
        addressPhoneExt: businessInfo.addressPhoneExt || undefined,
        addressGsm: businessInfo.addressGsm || undefined,
        // User account (TODO: Add these fields to registration form)
        email: contactPerson.email || '',
        password: 'TempPassword123!',  // TODO: Get from form
        passwordConfirmation: 'TempPassword123!',  // TODO: Get from form
        acceptTerms: true,  // TODO: Get from form
        acceptKvkk: true,  // TODO: Get from form
        // Document URLs
        documentUrls: documentUrls.length > 0 ? JSON.stringify(documentUrls) : undefined,
        // Customer attributes
        attributes: customerAttributes.length > 0 ? customerAttributes : undefined,
      }

      // Submit to API
      await registerDealer(registrationDto)

      // Show success message
      setUploadProgress(null)
      setSubmitSuccess(true)
    } catch (error: unknown) {
      console.error('Registration error:', error)
      setUploadProgress(null)

      // Handle specific error codes from backend
      const axiosError = error as { response?: { data?: { code?: string; message?: string } }; message?: string }
      const errorCode = axiosError?.response?.data?.code
      const errorMessage = axiosError?.response?.data?.message

      if (errorCode === 'EMAIL_EXISTS') {
        setSubmitError('Bu e-posta adresi zaten kayıtlı. Lütfen farklı bir e-posta adresi kullanınız.')
      } else if (errorCode === 'TAX_NUMBER_EXISTS') {
        setSubmitError('Bu vergi numarası zaten kayıtlı. Lütfen vergi numaranızı kontrol ediniz.')
      } else if (axiosError.message?.includes('Belge yüklenirken')) {
        setSubmitError(axiosError.message)
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
                  {isLoadingAttributes ? (
                    <div className="flex items-center gap-2 text-muted-foreground py-8">
                      <Loader2 className="h-4 w-4 animate-spin" />
                      <span className="text-sm">Yükleniyor...</span>
                    </div>
                  ) : (
                    <CompositeAttributeInput
                      title={bankAccountTitle}
                      fields={bankAccountFields}
                      values={bankAccountValues}
                      onChange={setBankAccountValues}
                      minRows={1}
                      maxRows={10}
                      addButtonText="Banka Hesabı Ekle"
                    />
                  )}
                </div>

                {/* Right Section - Collaterals & Documents */}
                <div className="space-y-6">
                  {/* Collaterals */}
                  {isLoadingAttributes ? (
                    <div className="flex items-center gap-2 text-muted-foreground py-8">
                      <Loader2 className="h-4 w-4 animate-spin" />
                      <span className="text-sm">Yükleniyor...</span>
                    </div>
                  ) : (
                    <CompositeAttributeInput
                      title={teminatTitle}
                      fields={teminatFields}
                      values={teminatValues}
                      onChange={setTeminatValues}
                      minRows={1}
                      maxRows={10}
                      addButtonText="Teminat Ekle"
                    />
                  )}

                  {/* Documents */}
                  <div>
                    <h2 className="text-lg font-semibold text-foreground mb-4">Evrak & Belgeler</h2>
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                      {(Object.keys(DOCUMENT_LABELS) as DocumentKey[]).map((docKey) => (
                        <Controller
                          key={docKey}
                          name={`documents.${docKey}`}
                          control={form.control}
                          render={({ field }) => (
                            <FileUpload
                              label={DOCUMENT_LABELS[docKey]}
                              onChange={field.onChange}
                              value={field.value}
                            />
                          )}
                        />
                      ))}
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
