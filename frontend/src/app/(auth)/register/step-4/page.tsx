'use client'

import { useState, useEffect } from 'react'
import { useRouter } from 'next/navigation'
import { useForm, useFieldArray, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { CheckCircle } from 'lucide-react'
import { Button } from '@/components/ui/Button'
import { Select } from '@/components/ui/Select'
import { Card } from '@/components/ui/Card'
import { StepIndicator } from '@/components/ui/StepIndicator'
import { FileUpload } from '@/components/ui/FileUpload'
import { useRegistrationStore } from '@/stores/registrationStore'
import { step4Schema, Step4FormData } from '@/lib/validations/registration.schema'
import { registerDealer, uploadRegistrationDocument, saveCustomerDocument } from '@/lib/api'
import type { DealerRegistrationDto, CustomerDocumentType } from '@/types'

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

  const {
    register,
    handleSubmit,
    control,
    reset: resetForm,
    formState: { errors, isSubmitting },
  } = useForm<Step4FormData>({
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
    control,
    name: 'bankAccounts',
  })

  const { fields: collateralFields } = useFieldArray({
    control,
    name: 'collaterals',
  })

  // Clear registration data after success is shown
  useEffect(() => {
    if (submitSuccess) {
      // Delay the reset to ensure success screen is rendered first
      const timer = setTimeout(() => {
        reset()
        resetForm()
      }, 100)
      return () => clearTimeout(timer)
    }
  }, [submitSuccess, reset, resetForm])

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

      // Step 2: Map form data to backend DTO and register customer
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
      }

      // Submit to API
      const registrationResult = await registerDealer(registrationDto)
      const customerId = registrationResult.id

      // Step 3: Associate uploaded documents with the customer
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
        <Card className="p-8 text-center">
          <div className="flex justify-center mb-6">
            <div className="w-20 h-20 bg-green-100 rounded-full flex items-center justify-center">
              <CheckCircle className="w-12 h-12 text-green-600" />
            </div>
          </div>
          <h2 className="text-2xl font-semibold text-gray-900 mb-4">
            Başvurunuz Alındı!
          </h2>
          <p className="text-gray-600 mb-6">
            Bayi başvurunuz başarıyla alınmıştır. Tüm bilgileriniz ilgili yetkili kişiye
            e-posta olarak iletilmiştir. Başvurunuz incelendikten sonra sizinle
            iletişime geçilecektir.
          </p>
          <div className="bg-primary-50 rounded-lg p-4 mb-6">
            <p className="text-sm text-primary-700">
              <strong>Not:</strong> Başvurunuzun değerlendirilmesi 1-3 iş günü içinde
              tamamlanacaktır. Onay durumu hakkında kayıtlı e-posta adresinize
              bilgilendirme yapılacaktır.
            </p>
          </div>
          <Button onClick={() => router.push('/login')}>
            Giriş Sayfasına Dön
          </Button>
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
      <Card className="p-8">
        <form onSubmit={handleSubmit(onSubmit)}>
          {submitError && (
            <div className="mb-6 p-4 bg-red-50 border border-red-200 rounded-md">
              <p className="text-sm text-red-600">{submitError}</p>
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
              <h2 className="form-section-title">Banka Hesap Bilgileri</h2>
              <div className="overflow-x-auto">
                <table className="w-full">
                  <thead>
                    <tr className="border-b border-gray-200">
                      <th className="text-left py-2 px-2 text-sm font-medium text-gray-700">Banka Adı</th>
                      <th className="text-left py-2 px-2 text-sm font-medium text-gray-700">IBAN</th>
                    </tr>
                  </thead>
                  <tbody>
                    {bankFields.map((field, index) => (
                      <tr key={field.id} className="border-b border-gray-100">
                        <td className="py-2 px-2">
                          <input
                            type="text"
                            placeholder="Banka Adı"
                            className="input-field text-sm"
                            {...register(`bankAccounts.${index}.bankName`)}
                          />
                        </td>
                        <td className="py-2 px-2">
                          <div>
                            <input
                              type="text"
                              placeholder="TR00 0000 0000 0000 0000 0000 00"
                              className={`input-field text-sm ${errors.bankAccounts?.[index]?.iban ? 'border-red-500' : ''}`}
                              {...register(`bankAccounts.${index}.iban`)}
                            />
                            {errors.bankAccounts?.[index]?.iban && (
                              <p className="text-xs text-red-500 mt-1">
                                {errors.bankAccounts[index]?.iban?.message}
                              </p>
                            )}
                          </div>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
              {errors.bankAccounts && (
                <p className="input-error mt-2">{errors.bankAccounts.message}</p>
              )}
            </div>

            {/* Right Section - Collaterals & Documents */}
            <div className="space-y-6">
              {/* Collaterals */}
              <div>
                <h2 className="form-section-title">Teminatlar</h2>
                <div className="overflow-x-auto">
                  <table className="w-full">
                    <thead>
                      <tr className="border-b border-gray-200">
                        <th className="text-left py-2 px-2 text-sm font-medium text-gray-700">Teminat Türü</th>
                        <th className="text-left py-2 px-2 text-sm font-medium text-gray-700">Tutar</th>
                        <th className="text-left py-2 px-2 text-sm font-medium text-gray-700">Para Birimi</th>
                      </tr>
                    </thead>
                    <tbody>
                      {collateralFields.map((field, index) => (
                        <tr key={field.id} className="border-b border-gray-100">
                          <td className="py-2 px-2">
                            <select
                              className="input-field text-sm"
                              {...register(`collaterals.${index}.type`)}
                            >
                              <option value="">Seçiniz</option>
                              {GUARANTEE_TYPES.map((type) => (
                                <option key={type.value} value={type.value}>
                                  {type.label}
                                </option>
                              ))}
                            </select>
                          </td>
                          <td className="py-2 px-2">
                            <input
                              type="number"
                              placeholder="Tutar"
                              className="input-field text-sm w-28"
                              {...register(`collaterals.${index}.amount`, { valueAsNumber: true })}
                            />
                          </td>
                          <td className="py-2 px-2">
                            <select
                              className="input-field text-sm w-24"
                              {...register(`collaterals.${index}.currency`)}
                            >
                              {CURRENCIES.map((currency) => (
                                <option key={currency.value} value={currency.value}>
                                  {currency.label}
                                </option>
                              ))}
                            </select>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>

              {/* Documents */}
              <div>
                <h2 className="form-section-title">Evrak & Belgeler</h2>
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                  <Controller
                    name="documents.taxCertificate"
                    control={control}
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
                    control={control}
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
                    control={control}
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
                    control={control}
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
                    control={control}
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
                    control={control}
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

          <div className="flex justify-between mt-8 pt-6 border-t border-gray-200">
            <Button
              type="button"
              variant="secondary"
              onClick={() => router.push('/register/step-3')}
            >
              Geri
            </Button>
            <Button type="submit" isLoading={isSubmitting}>
              Kaydet & Gönder
            </Button>
          </div>
        </form>
      </Card>
    </div>
  )
}
