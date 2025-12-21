import { z } from "zod";

// Attribute value input schema for dynamic product attributes
export const productAttributeValueInputSchema = z.object({
  attributeDefinitionId: z.string().min(1, "Attribute definition ID is required"),
  textValue: z.string().optional(),
  numericValue: z.number().optional(),
  selectValueId: z.string().optional(),
  multiSelectValueIds: z.array(z.string()).optional(),
  booleanValue: z.boolean().optional(),
  dateValue: z.string().optional(),
});

export type ProductAttributeValueInputData = z.infer<typeof productAttributeValueInputSchema>;

// Helper to handle null values from API - converts null to undefined
const nullableNumber = (min = 0, message?: string) =>
  z.number().min(min, message).nullable().optional().transform(val => val ?? undefined);

export const productSchema = z.object({
  sku: z.string().min(1, "SKU is required").max(50, "SKU cannot exceed 50 characters"),
  name: z.string().min(2, "Name must be at least 2 characters").max(200, "Name cannot exceed 200 characters"),
  description: z.string().nullable().optional().transform(val => val ?? undefined),
  categoryIds: z.array(z.string()).min(1, "At least one category is required"),
  brandId: z.string().nullable().optional().transform(val => val ?? undefined),
  productTypeId: z.string().nullable().optional().transform(val => val ?? undefined),
  listPrice: z.number().min(0, "Price must be positive"),
  listPriceCurrency: z.string().default("TRY"),
  dealerPrice: nullableNumber(0, "Dealer price must be positive"),
  tier1Price: nullableNumber(0, "Tier 1 price must be positive"),
  tier2Price: nullableNumber(0, "Tier 2 price must be positive"),
  tier3Price: nullableNumber(0, "Tier 3 price must be positive"),
  tier4Price: nullableNumber(0, "Tier 4 price must be positive"),
  tier5Price: nullableNumber(0, "Tier 5 price must be positive"),
  stockQuantity: z.number().int().min(0, "Stock must be a positive integer").default(0),
  minOrderQuantity: z.number().int().min(1, "Minimum order quantity must be at least 1").default(1),
  maxOrderQuantity: z.number().int().min(1).nullable().optional().transform(val => val ?? undefined),
  unitOfMeasure: z.string().default("ADET"),
  isSerialTracked: z.boolean().default(false),
  taxRate: nullableNumber(0, "Tax rate must be positive"),
  mainImageUrl: z.string().url("Invalid URL format").or(z.literal("")).nullable().optional().transform(val => val ?? ""),
  imageUrls: z.array(z.string().url("Invalid URL format")).default([]),
  weight: nullableNumber(0),
  length: nullableNumber(0),
  width: nullableNumber(0),
  height: nullableNumber(0),
  isActive: z.boolean().default(true),
  isFeatured: z.boolean().default(false),
  attributeValues: z.array(productAttributeValueInputSchema).default([]),
});

// Output type - what comes out after Zod parsing (defaults applied)
export type ProductFormData = z.output<typeof productSchema>;

// Input type - what the form provides before Zod parsing (defaults are optional)
export type ProductFormInput = z.input<typeof productSchema>;

export const productFilterSchema = z.object({
  search: z.string().optional(),
  categoryId: z.string().optional(),
  brandId: z.string().optional(),
  isActive: z.boolean().optional(),
  page: z.number().default(1),
  pageSize: z.number().default(10),
  sortBy: z.string().default("CreatedAt"),
  sortOrder: z.enum(["asc", "desc"]).default("desc"),
});

export type ProductFilterData = z.infer<typeof productFilterSchema>;
