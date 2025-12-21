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

export const productSchema = z.object({
  sku: z.string().min(1, "SKU is required").max(50, "SKU cannot exceed 50 characters"),
  name: z.string().min(2, "Name must be at least 2 characters").max(200, "Name cannot exceed 200 characters"),
  description: z.string().optional(),
  categoryIds: z.array(z.string()).min(1, "At least one category is required"),
  brandId: z.string().optional(),
  productTypeId: z.string().optional(),
  listPrice: z.number().min(0, "Price must be positive"),
  listPriceCurrency: z.string().default("TRY"),
  dealerPrice: z.number().min(0, "Dealer price must be positive").optional(),
  tier1Price: z.number().min(0, "Tier 1 price must be positive").optional(),
  tier2Price: z.number().min(0, "Tier 2 price must be positive").optional(),
  tier3Price: z.number().min(0, "Tier 3 price must be positive").optional(),
  tier4Price: z.number().min(0, "Tier 4 price must be positive").optional(),
  tier5Price: z.number().min(0, "Tier 5 price must be positive").optional(),
  stockQuantity: z.number().int().min(0, "Stock must be a positive integer").default(0),
  minOrderQuantity: z.number().int().min(1, "Minimum order quantity must be at least 1").default(1),
  maxOrderQuantity: z.number().int().min(1).optional(),
  unitOfMeasure: z.string().default("ADET"),
  isSerialTracked: z.boolean().default(false),
  taxRate: z.number().min(0, "Tax rate must be positive").max(1, "Tax rate cannot exceed 1").optional(),
  mainImageUrl: z.string().url("Invalid URL format").or(z.literal("")).default(""),
  imageUrls: z.array(z.string().url("Invalid URL format")).default([]),
  weight: z.number().min(0).optional(),
  length: z.number().min(0).optional(),
  width: z.number().min(0).optional(),
  height: z.number().min(0).optional(),
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
