'use client'

import { useRouter } from 'next/navigation'
import { useForm, useFieldArray } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Loader2 } from 'lucide-react'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Card, CardContent } from '@/components/ui/card'
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
import { step2Schema, Step2FormData } from '@/lib/validations/registration.schema'

export default function RegisterStep2Page() {
  const router = useRouter()
  const { businessInfo, setBusinessInfo, setCurrentStep } = useRegistrationStore()

  const form = useForm<Step2FormData>({
    resolver: zodResolver(step2Schema),
    defaultValues: {
      companyTitle: businessInfo.companyTitle || '',
      taxOffice: businessInfo.taxOffice || '',
      taxNumber: businessInfo.taxNumber || '',
      foundedYear: businessInfo.foundedYear,
      address: businessInfo.address || '',
      country: businessInfo.country || '',
      phone: businessInfo.phone || '+90',
      website: businessInfo.website || '',
      authorizedPersons: businessInfo.authorizedPersons || [
        { fullName: '', tcNumber: '', sharePercentage: 0 },
        { fullName: '', tcNumber: '', sharePercentage: 0 },
        { fullName: '', tcNumber: '', sharePercentage: 0 },
        { fullName: '', tcNumber: '', sharePercentage: 0 },
        { fullName: '', tcNumber: '', sharePercentage: 0 },
        { fullName: '', tcNumber: '', sharePercentage: 0 },
      ],
    },
  })

  const { fields } = useFieldArray({
    control: form.control,
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
      <Card>
        <CardContent className="p-8">
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)}>
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                {/* Left Section - Business Info */}
                <div>
                  <h2 className="text-lg font-semibold text-foreground mb-4">İşletme Bilgileri</h2>

                  <div className="space-y-4">
                    <FormField
                      control={form.control}
                      name="companyTitle"
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
                        name="taxNumber"
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
                        name="foundedYear"
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

                    <h3 className="text-lg font-semibold text-foreground mt-8 mb-4">İletişim</h3>

                    <FormField
                      control={form.control}
                      name="address"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Adres</FormLabel>
                          <FormControl>
                            <Input placeholder="Adres" {...field} />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <div className="grid grid-cols-1 sm:grid-cols-3 gap-4">
                      <FormField
                        control={form.control}
                        name="country"
                        render={({ field }) => (
                          <FormItem>
                            <FormLabel>Ülke</FormLabel>
                            <FormControl>
                              <Input placeholder="Ülke" {...field} />
                            </FormControl>
                            <FormMessage />
                          </FormItem>
                        )}
                      />
                      <FormField
                        control={form.control}
                        name="phone"
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
                    </div>
                  </div>
                </div>

                {/* Right Section - Authorized Persons */}
                <div>
                  <h2 className="text-lg font-semibold text-foreground mb-4">Yetkililer & Ortaklar</h2>

                  <div className="overflow-x-auto">
                    <table className="w-full">
                      <thead>
                        <tr className="border-b border-border">
                          <th className="text-left py-2 px-2 text-sm font-medium text-muted-foreground">Adı Soyadı</th>
                          <th className="text-left py-2 px-2 text-sm font-medium text-muted-foreground">T.C. Kimlik No</th>
                          <th className="text-left py-2 px-2 text-sm font-medium text-muted-foreground">Pay Oranı (%)</th>
                        </tr>
                      </thead>
                      <tbody>
                        {fields.map((field, index) => (
                          <tr key={field.id} className="border-b border-border/50">
                            <td className="py-2 px-2">
                              <Input
                                placeholder="Adı Soyadı"
                                className="text-sm"
                                {...form.register(`authorizedPersons.${index}.fullName`)}
                              />
                            </td>
                            <td className="py-2 px-2">
                              <div>
                                <Input
                                  placeholder="T.C. Kimlik No"
                                  className="text-sm"
                                  maxLength={11}
                                  {...form.register(`authorizedPersons.${index}.tcNumber`)}
                                />
                                {form.formState.errors.authorizedPersons?.[index]?.tcNumber && (
                                  <p className="text-xs text-destructive mt-1">
                                    {form.formState.errors.authorizedPersons[index]?.tcNumber?.message}
                                  </p>
                                )}
                              </div>
                            </td>
                            <td className="py-2 px-2">
                              <Input
                                type="number"
                                placeholder="%"
                                className="text-sm w-20"
                                min={0}
                                max={100}
                                {...form.register(`authorizedPersons.${index}.sharePercentage`, { valueAsNumber: true })}
                              />
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                  {form.formState.errors.authorizedPersons?.message && (
                    <p className="text-destructive text-sm mt-2">{form.formState.errors.authorizedPersons.message}</p>
                  )}
                  {form.formState.errors.authorizedPersons?.root?.message && (
                    <p className="text-destructive text-sm mt-2">{form.formState.errors.authorizedPersons.root.message}</p>
                  )}
                </div>
              </div>

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
