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
import type { DealerRegistrationDto, UpsertCustomerAttributesDto } from '@/types'

// Map attribute type to field type for composite attributes
function mapAttributeTypeToFieldType(type: number | string, isList?: boolean): 'text' | 'number' | 'tc_kimlik' | 'select' {
  const typeValue = typeof type === 'string' ? type.toLowerCase() : type
  if (typeValue === 2 || typeValue === 3 || typeValue === 'select' || typeValue === 'multiselect' || isList) return 'select'
  if (typeValue === 1 || typeValue === 'number') return 'number'
  if (typeValue === 'tc_kimlik') return 'tc_kimlik'
  return 'text'
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
    return Array(6).fill(null).map(() => ({ banka_adi: '', iban: '' }))
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
    return Array(3).fill(null).map(() => ({ teminat_turu: '', tutar: 0, para_birimi: 'TRY' }))
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
        // Fallback fields
        setBankAccountFields([
          { code: 'banka_adi', name: 'Banka Adı', type: 'text', placeholder: 'Banka Adı' },
          { code: 'iban', name: 'IBAN', type: 'text', placeholder: 'TR00 0000 0000 0000 0000 0000 00' },
        ])
        setTeminatFields([
          { code: 'teminat_turu', name: 'Teminat Türü', type: 'select', placeholder: 'Seçiniz', options: [
            { value: 'cek', label: 'Çek' },
            { value: 'senet', label: 'Senet' },
            { value: 'teminat_mektubu', label: 'Teminat Mektubu' },
            { value: 'ipotek', label: 'İpotek' },
          ]},
          { code: 'tutar', name: 'Tutar', type: 'number', placeholder: '0' },
          { code: 'para_birimi', name: 'Para Birimi', type: 'select', placeholder: 'TRY', options: [
            { value: 'TRY', label: 'TRY' },
            { value: 'USD', label: 'USD' },
            { value: 'EUR', label: 'EUR' },
          ]},
        ])
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

      // Step 2: Build customer attributes from all form data
      const attributes: UpsertCustomerAttributesDto[] = []

      // Step 2: Authorized Persons → ShareholderOrDirector
      const shareholders = businessInfo.authorizedPersons?.filter(p => p.ad_soyad) || []
      if (shareholders.length > 0) {
        attributes.push({
          attributeType: 'ShareholderOrDirector',
          items: shareholders.map((s, i) => ({
            displayOrder: i,
            jsonData: JSON.stringify({
              fullName: s.ad_soyad || '',
              identityNumber: s.kimlik_no || '',
              sharePercentage: s.ortaklik_payi || 0
            })
          }))
        })
      }

      // Step 3: Business Partners (İş Ortakları) → BusinessPartner
      const partners = operationalDetails.isOrtaklari?.filter((p: Record<string, unknown>) => {
        // Check if any field has a value
        return Object.values(p).some(v => v !== '' && v !== 0 && v !== undefined)
      }) || []
      if (partners.length > 0) {
        attributes.push({
          attributeType: 'BusinessPartner',
          items: partners.map((p: Record<string, unknown>, i: number) => ({
            displayOrder: i,
            jsonData: JSON.stringify(p)
          }))
        })
      }

      // Step 3: Product Categories → ProductCategory
      const categories = operationalDetails.satilanUrunKategorileri || []
      if (categories.length > 0) {
        attributes.push({
          attributeType: 'ProductCategory',
          items: [{
            displayOrder: 0,
            jsonData: JSON.stringify({ categories })
          }]
        })
      }

      // Step 4: Bank Accounts → BankAccount (from dynamic attributes)
      const banks = bankAccountValues.filter(b => b.banka_adi || b.iban)
      if (banks.length > 0) {
        attributes.push({
          attributeType: 'BankAccount',
          items: banks.map((b, i) => ({
            displayOrder: i,
            jsonData: JSON.stringify(b)
          }))
        })
      }

      // Step 4: Collaterals → Collateral (from dynamic attributes)
      const collaterals = teminatValues.filter(c => c.teminat_turu)
      if (collaterals.length > 0) {
        attributes.push({
          attributeType: 'Collateral',
          items: collaterals.map((c, i) => ({
            displayOrder: i,
            jsonData: JSON.stringify(c)
          }))
        })
      }

      // Step 3: Requested Conditions → PaymentPreference
      const preferences = operationalDetails.calismaKosullari || []
      if (preferences.length > 0) {
        attributes.push({
          attributeType: 'PaymentPreference',
          items: [{
            displayOrder: 0,
            jsonData: JSON.stringify({ preferences })
          }]
        })
      }

      // Build registration DTO
      const registrationDto: DealerRegistrationDto = {
        // Required company info
        companyName: businessInfo.title || '',
        taxNumber: businessInfo.taxNo || '',
        taxOffice: businessInfo.taxOffice || '',
        email: contactPerson.email || '',
        phone: businessInfo.addressPhone || contactPerson.phone || '',
        contactPersonName: `${contactPerson.firstName || ''} ${contactPerson.lastName || ''}`.trim(),
        contactPersonTitle: contactPerson.position || '',
        // Optional company info
        mobilePhone: contactPerson.gsm || undefined,
        website: businessInfo.website || undefined,
        // Customer attributes
        attributes: attributes.length > 0 ? attributes : undefined,
        // Document URLs (stored as JSON in Customer.DocumentUrls)
        documentUrls: documentUrls.length > 0 ? JSON.stringify(documentUrls) : undefined,
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
                      minRows={6}
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
                      minRows={3}
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
