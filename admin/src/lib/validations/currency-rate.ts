import { z } from "zod";

export const currencyRateSchema = z.object({
  fromCurrency: z
    .string()
    .min(3, "Currency code must be exactly 3 characters")
    .max(3, "Currency code must be exactly 3 characters")
    .regex(/^[A-Z]{3}$/, "Currency code must be 3 uppercase letters (ISO 4217)"),
  toCurrency: z
    .string()
    .min(3, "Currency code must be exactly 3 characters")
    .max(3, "Currency code must be exactly 3 characters")
    .regex(/^[A-Z]{3}$/, "Currency code must be 3 uppercase letters (ISO 4217)"),
  rate: z
    .number()
    .positive("Exchange rate must be greater than 0"),
  effectiveDate: z.string().optional(),
});

export const updateCurrencyRateSchema = z.object({
  rate: z
    .number()
    .positive("Exchange rate must be greater than 0"),
  effectiveDate: z.string().optional(),
});

export type CurrencyRateFormData = z.infer<typeof currencyRateSchema>;
export type UpdateCurrencyRateFormData = z.infer<typeof updateCurrencyRateSchema>;
