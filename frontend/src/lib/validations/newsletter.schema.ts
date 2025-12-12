import { z } from 'zod'

export const newsletterSchema = z.object({
  email: z.string()
    .min(1, 'E-posta adresi gereklidir')
    .email('Geçerli bir e-posta adresi giriniz'),
})

export type NewsletterFormData = z.infer<typeof newsletterSchema>
