import { z } from 'zod'

export const loginSchema = z.object({
  businessPartnerCode: z.string()
    .min(1, 'İş ortağı kodu gereklidir')
    .min(3, 'İş ortağı kodu en az 3 karakter olmalıdır'),
  email: z.string()
    .min(1, 'E-posta adresi gereklidir')
    .email('Geçerli bir e-posta adresi giriniz'),
  password: z.string()
    .min(1, 'Şifre gereklidir')
    .min(6, 'Şifre en az 6 karakter olmalıdır'),
})

export type LoginFormData = z.infer<typeof loginSchema>
