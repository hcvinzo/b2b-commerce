import { z } from "zod";

export const productSchema = z.object({
  sku: z.string().min(1, "SKU is required").max(50, "SKU cannot exceed 50 characters"),
  name: z.string().min(2, "Name must be at least 2 characters").max(200, "Name cannot exceed 200 characters"),
  nameEn: z.string().max(200, "English name cannot exceed 200 characters").optional(),
  description: z.string().optional(),
  categoryId: z.string().min(1, "Category is required"),
  brandId: z.string().optional(),
  productTypeId: z.string().optional(),
  listPrice: z.number().min(0, "Price must be positive"),
  listPriceCurrency: z.string().optional(),
  dealerPrice: z.number().min(0, "Dealer price must be positive").optional(),
  stockQuantity: z.number().int().min(0, "Stock must be a positive integer").optional(),
  minOrderQuantity: z.number().int().min(1, "Minimum order quantity must be at least 1").optional(),
  maxOrderQuantity: z.number().int().min(1).optional(),
  unitOfMeasure: z.string().optional(),
  isActive: z.boolean().optional(),
  isFeatured: z.boolean().optional(),
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
