import { z } from "zod";

export const rateManagementModeOptions = [
  { value: "Manual", label: "Manual Entry" },
  { value: "TCMB", label: "TCMB (Turkish Central Bank)" },
] as const;

export const currencySchema = z.object({
  code: z
    .string()
    .min(3, "Currency code must be exactly 3 characters")
    .max(3, "Currency code must be exactly 3 characters")
    .regex(/^[A-Z]{3}$/, "Currency code must be 3 uppercase letters (ISO 4217)"),
  name: z
    .string()
    .min(2, "Name must be at least 2 characters")
    .max(100, "Name cannot exceed 100 characters"),
  symbol: z
    .string()
    .min(1, "Symbol is required")
    .max(10, "Symbol cannot exceed 10 characters"),
  decimalPlaces: z
    .number()
    .int("Decimal places must be a whole number")
    .min(0, "Decimal places cannot be negative")
    .max(4, "Decimal places cannot exceed 4"),
  displayOrder: z
    .number()
    .int("Display order must be a whole number")
    .min(0, "Display order cannot be negative"),
  rateManagementMode: z.enum(["Manual", "TCMB"]),
});

export type CurrencyFormData = z.infer<typeof currencySchema>;
