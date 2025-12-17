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

// Helper for optional number fields that may come as null from backend
const optionalNumber = z.number().min(0).optional().nullable().transform(val => val ?? undefined);
const optionalPositiveNumber = (message: string) => z.number().min(0, message).optional().nullable().transform(val => val ?? undefined);

export const productSchema = z.object({
  sku: z.string().min(1, "SKU is required").max(50, "SKU cannot exceed 50 characters"),
  name: z.string().min(2, "Name must be at least 2 characters").max(200, "Name cannot exceed 200 characters"),
  nameEn: z.string().max(200, "English name cannot exceed 200 characters").optional().nullable().transform(val => val ?? undefined),
  description: z.string().optional().nullable().transform(val => val ?? undefined),
  categoryIds: z.array(z.string()).min(1, "At least one category is required"),
  brandId: z.string().optional().nullable().transform(val => val ?? undefined),
  productTypeId: z.string().optional().nullable().transform(val => val ?? undefined),
  listPrice: z.number().min(0, "Price must be positive"),
  listPriceCurrency: z.string().optional().nullable().transform(val => val ?? "TRY"),
  dealerPrice: optionalPositiveNumber("Dealer price must be positive"),
  tier1Price: optionalPositiveNumber("Tier 1 price must be positive"),
  tier2Price: optionalPositiveNumber("Tier 2 price must be positive"),
  tier3Price: optionalPositiveNumber("Tier 3 price must be positive"),
  tier4Price: optionalPositiveNumber("Tier 4 price must be positive"),
  tier5Price: optionalPositiveNumber("Tier 5 price must be positive"),
  stockQuantity: z.number().int().min(0, "Stock must be a positive integer").optional().nullable().transform(val => val ?? 0),
  minOrderQuantity: z.number().int().min(1, "Minimum order quantity must be at least 1").optional().nullable().transform(val => val ?? 1),
  maxOrderQuantity: z.number().int().min(1).optional().nullable().transform(val => val ?? undefined),
  unitOfMeasure: z.string().optional().nullable().transform(val => val ?? "ADET"),
  isSerialTracked: z.boolean().optional().nullable().transform(val => val ?? false),
  taxRate: z.number().min(0, "Tax rate must be positive").max(1, "Tax rate cannot exceed 1").optional().nullable().transform(val => val ?? undefined),
  mainImageUrl: z.string().url("Invalid URL format").optional().nullable().or(z.literal("")).transform(val => val ?? ""),
  imageUrls: z.array(z.string().url("Invalid URL format")).optional().nullable().transform(val => val ?? []),
  weight: optionalNumber,
  length: optionalNumber,
  width: optionalNumber,
  height: optionalNumber,
  isActive: z.boolean().optional().nullable().transform(val => val ?? true),
  isFeatured: z.boolean().optional().nullable().transform(val => val ?? false),
  attributeValues: z.array(productAttributeValueInputSchema).optional().nullable().transform(val => val ?? []),
});

export type ProductFormData = z.infer<typeof productSchema>;

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
