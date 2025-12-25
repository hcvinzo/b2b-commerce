'use client'

import { useRouter } from 'next/navigation'
import { useForm, useFieldArray, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Loader2 } from 'lucide-react'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Card, CardContent } from '@/components/ui/card'
import { Checkbox } from '@/components/ui/checkbox'
import { Label } from '@/components/ui/label'
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

  const form = useForm<Step3FormData>({
    resolver: zodResolver(step3Schema),
    defaultValues: {
      employeeCount: operationalDetails.employeeCount || '',
      businessStructure: operationalDetails.businessStructure || '',
      revenueYear: operationalDetails.revenueYear,
      targetRevenue: operationalDetails.targetRevenue,
      revenueCurrency: operationalDetails.revenueCurrency || 'TRY',
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
    control: form.control,
    name: 'currentPartners',
  })

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
      <Card>
        <CardContent className="p-8">
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)}>
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                {/* Left Section */}
                <div className="space-y-6">
                  {/* Personnel & Structure */}
                  <div>
                    <h2 className="text-lg font-semibold text-foreground mb-4">İşletme Yapısı</h2>
                    <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                      <FormField
                        control={form.control}
                        name="employeeCount"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>Personel Sayısı</FormLabel>
                            <Select onValueChange={field.onChange} defaultValue={field.value}>
                              <FormControl>
                                <SelectTrigger className="w-full">
                                  <SelectValue placeholder="Seçiniz" />
                                </SelectTrigger>
                              </FormControl>
                              <SelectContent>
                                <SelectItem value="1-9">1-9</SelectItem>
                                <SelectItem value="10-19">10-19</SelectItem>
                                <SelectItem value="20-29">20-29</SelectItem>
                                <SelectItem value="30-39">30-39</SelectItem>
                                <SelectItem value="40-49">40-49</SelectItem>
                                <SelectItem value="50+">50+</SelectItem>
                              </SelectContent>
                            </Select>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                      <FormField
                        control={form.control}
                        name="businessStructure"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>İşletme Yapısı</FormLabel>
                            <Select onValueChange={field.onChange} defaultValue={field.value}>
                              <FormControl>
                                <SelectTrigger className="w-full">
                                  <SelectValue placeholder="Seçiniz" />
                                </SelectTrigger>
                              </FormControl>
                              <SelectContent>
                                <SelectItem value="ofis">Ofis/Büro</SelectItem>
                                <SelectItem value="magaza">Mağaza</SelectItem>
                                <SelectItem value="home_ofis">Home Ofis</SelectItem>
                              </SelectContent>
                            </Select>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                    </div>
                  </div>

                  {/* Revenue Info */}
                  <div>
                    <h3 className="text-lg font-semibold text-foreground mb-4">Ciro Bilgisi</h3>
                    <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                      <FormField
                        control={form.control}
                        name="revenueYear"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>Geçen Yıl Cirosu</FormLabel>
                            <FormControl>
                              <Input
                                type="number"
                                placeholder="Tutar"
                                {...field}
                                onChange={(e) => field.onChange(e.target.valueAsNumber)}
                              />
                            </FormControl>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                      <FormField
                        control={form.control}
                        name="targetRevenue"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>Bu Yıl Hedeflenen</FormLabel>
                            <FormControl>
                              <Input
                                type="number"
                                placeholder="Tutar"
                                {...field}
                                onChange={(e) => field.onChange(e.target.valueAsNumber)}
                              />
                            </FormControl>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                      <FormField
                        control={form.control}
                        name="revenueCurrency"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>Para Birimi</FormLabel>
                            <Select onValueChange={field.onChange} defaultValue={field.value}>
                              <FormControl>
                                <SelectTrigger className="w-full">
                                  <SelectValue placeholder="Seçiniz" />
                                </SelectTrigger>
                              </FormControl>
                              <SelectContent>
                                <SelectItem value="TRY">TRY</SelectItem>
                                <SelectItem value="USD">USD</SelectItem>
                                <SelectItem value="EUR">EUR</SelectItem>
                              </SelectContent>
                            </Select>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                    </div>
                  </div>

                  {/* Customer Base */}
                  <div>
                    <h3 className="text-lg font-semibold text-foreground mb-4">Müşteri Kitlesi (%)</h3>
                    <div className="grid grid-cols-2 sm:grid-cols-4 gap-4">
                      <FormField
                        control={form.control}
                        name="customerBase.retailer"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>Devlet & Kamu</FormLabel>
                            <FormControl>
                              <Input
                                type="number"
                                placeholder="%"
                                min={0}
                                max={100}
                                {...field}
                                onChange={(e) => field.onChange(e.target.valueAsNumber)}
                              />
                            </FormControl>
                          </FormItem>
                        )}
                      />
                      <FormField
                        control={form.control}
                        name="customerBase.corporate"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>Kurumsal</FormLabel>
                            <FormControl>
                              <Input
                                type="number"
                                placeholder="%"
                                min={0}
                                max={100}
                                {...field}
                                onChange={(e) => field.onChange(e.target.valueAsNumber)}
                              />
                            </FormControl>
                          </FormItem>
                        )}
                      />
                      <FormField
                        control={form.control}
                        name="customerBase.construction"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>Bayi</FormLabel>
                            <FormControl>
                              <Input
                                type="number"
                                placeholder="%"
                                min={0}
                                max={100}
                                {...field}
                                onChange={(e) => field.onChange(e.target.valueAsNumber)}
                              />
                            </FormControl>
                          </FormItem>
                        )}
                      />
                      <FormField
                        control={form.control}
                        name="customerBase.retail"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>Perakende</FormLabel>
                            <FormControl>
                              <Input
                                type="number"
                                placeholder="%"
                                min={0}
                                max={100}
                                {...field}
                                onChange={(e) => field.onChange(e.target.valueAsNumber)}
                              />
                            </FormControl>
                          </FormItem>
                        )}
                      />
                    </div>
                    {form.formState.errors.customerBase?.message && (
                      <p className="text-destructive text-sm mt-2">{form.formState.errors.customerBase.message}</p>
                    )}
                    {form.formState.errors.customerBase?.root?.message && (
                      <p className="text-destructive text-sm mt-2">{form.formState.errors.customerBase.root.message}</p>
                    )}
                  </div>

                  {/* Product Categories */}
                  <div>
                    <h3 className="text-lg font-semibold text-foreground mb-4">Satışını Gerçekleştirdiğiniz Ürün Kategorileri</h3>
                    <FormField
                      control={form.control}
                      name="productCategories"
                      render={() => (
                        <FormItem>
                          <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
                            {PRODUCT_CATEGORIES.map((category) => (
                              <FormField
                                key={category.value}
                                control={form.control}
                                name="productCategories"
                                render={({ field }) => {
                                  const isChecked = field.value?.includes(category.value)
                                  const checkboxId = `category-${category.value}`
                                  return (
                                    <FormItem className="flex items-center space-x-2 space-y-0">
                                      <FormControl>
                                        <Checkbox
                                          id={checkboxId}
                                          checked={isChecked}
                                          onCheckedChange={(checked) => {
                                            const newValue = checked
                                              ? [...(field.value || []), category.value]
                                              : field.value?.filter((v: string) => v !== category.value) || []
                                            field.onChange(newValue)
                                          }}
                                        />
                                      </FormControl>
                                      <Label htmlFor={checkboxId} className="text-sm font-normal cursor-pointer">
                                        {category.label}
                                      </Label>
                                    </FormItem>
                                  )
                                }}
                              />
                            ))}
                          </div>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                  </div>
                </div>

                {/* Right Section */}
                <div className="space-y-6">
                  {/* Current Partners */}
                  <div>
                    <h2 className="text-lg font-semibold text-foreground mb-4">Çalışmakta Olduğunuz Şirketler & Çalışma Koşulları</h2>
                    <div className="overflow-x-auto">
                      <table className="w-full">
                        <thead>
                          <tr className="border-b border-border">
                            <th className="text-left py-2 px-2 text-sm font-medium text-muted-foreground">Şirket Adı</th>
                            <th className="text-left py-2 px-2 text-sm font-medium text-muted-foreground">Çalışma Koşulu</th>
                            <th className="text-left py-2 px-2 text-sm font-medium text-muted-foreground">Limit (USD)</th>
                          </tr>
                        </thead>
                        <tbody>
                          {partnerFields.map((field, index) => (
                            <tr key={field.id} className="border-b border-border/50">
                              <td className="py-2 px-2">
                                <Input
                                  placeholder="Şirket Adı"
                                  className="text-sm"
                                  {...form.register(`currentPartners.${index}.companyName`)}
                                />
                              </td>
                              <td className="py-2 px-2">
                                <Controller
                                  control={form.control}
                                  name={`currentPartners.${index}.workingCondition`}
                                  render={({ field }) => (
                                    <Select onValueChange={field.onChange} defaultValue={field.value}>
                                      <SelectTrigger className="w-full text-sm">
                                        <SelectValue placeholder="Seçiniz" />
                                      </SelectTrigger>
                                      <SelectContent>
                                        <SelectItem value="nakit_kredi">Nakit/Kredi Kartı</SelectItem>
                                        <SelectItem value="acik_hesap">Açık Hesap</SelectItem>
                                        <SelectItem value="cek">Çek</SelectItem>
                                      </SelectContent>
                                    </Select>
                                  )}
                                />
                              </td>
                              <td className="py-2 px-2">
                                <Input
                                  type="number"
                                  placeholder="USD"
                                  className="text-sm w-28"
                                  {...form.register(`currentPartners.${index}.creditLimit`, { valueAsNumber: true })}
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
                    <h3 className="text-lg font-semibold text-foreground mb-4">Talep Ettiğiniz Çalışma Koşulları</h3>
                    <FormField
                      control={form.control}
                      name="requestedConditions"
                      render={() => (
                        <FormItem>
                          <div className="space-y-3">
                            {WORKING_CONDITIONS.map((condition) => (
                              <FormField
                                key={condition.value}
                                control={form.control}
                                name="requestedConditions"
                                render={({ field }) => {
                                  const isChecked = field.value?.includes(condition.value)
                                  const checkboxId = `condition-${condition.value}`
                                  return (
                                    <FormItem className="flex items-center space-x-2 space-y-0">
                                      <FormControl>
                                        <Checkbox
                                          id={checkboxId}
                                          checked={isChecked}
                                          onCheckedChange={(checked) => {
                                            const newValue = checked
                                              ? [...(field.value || []), condition.value]
                                              : field.value?.filter((v: string) => v !== condition.value) || []
                                            field.onChange(newValue)
                                          }}
                                        />
                                      </FormControl>
                                      <Label htmlFor={checkboxId} className="text-sm font-normal cursor-pointer">
                                        {condition.label}
                                      </Label>
                                    </FormItem>
                                  )
                                }}
                              />
                            ))}
                          </div>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                  </div>
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
