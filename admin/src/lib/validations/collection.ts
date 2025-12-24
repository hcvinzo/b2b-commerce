import { z } from "zod";

export const collectionSchema = z
  .object({
    name: z
      .string()
      .min(2, "Name must be at least 2 characters")
      .max(200, "Name cannot exceed 200 characters"),
    description: z
      .string()
      .max(1000, "Description cannot exceed 1000 characters")
      .optional(),
    imageUrl: z
      .string()
      .url("Must be a valid URL")
      .or(z.literal(""))
      .optional(),
    type: z.enum(["Manual", "Dynamic"]),
    displayOrder: z.number().int().min(0, "Display order must be non-negative").optional(),
    isActive: z.boolean().optional(),
    isFeatured: z.boolean().optional(),
    startDate: z.string().optional(),
    endDate: z.string().optional(),
  })
  .refine(
    (data) => {
      if (!data.startDate || !data.endDate) return true;
      return new Date(data.endDate) >= new Date(data.startDate);
    },
    {
      message: "End date must be after start date",
      path: ["endDate"],
    }
  );

export type CollectionFormData = z.infer<typeof collectionSchema>;

// Collection filters schema (for dynamic collections)
export const collectionFiltersSchema = z.object({
  categoryIds: z.array(z.string()).optional(),
  brandIds: z.array(z.string()).optional(),
  productTypeIds: z.array(z.string()).optional(),
  minPrice: z.number().min(0, "Minimum price must be non-negative").optional(),
  maxPrice: z.number().min(0, "Maximum price must be non-negative").optional(),
});

export type CollectionFiltersFormData = z.infer<typeof collectionFiltersSchema>;

// Product in collection input (for manual collections)
export const productInCollectionInputSchema = z.object({
  productId: z.string().min(1, "Product ID is required"),
  displayOrder: z.number().int().min(0).optional(),
  isFeatured: z.boolean().optional(),
});

export type ProductInCollectionInputData = z.infer<
  typeof productInCollectionInputSchema
>;
