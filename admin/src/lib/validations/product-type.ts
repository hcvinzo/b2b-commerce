import { z } from "zod";

// Product type schema for creating new product types
export const productTypeSchema = z.object({
  code: z
    .string()
    .min(2, "Code must be at least 2 characters")
    .max(100, "Code cannot exceed 100 characters")
    .regex(
      /^[a-z0-9_]+$/,
      "Code can only contain lowercase letters, numbers, and underscores"
    ),
  name: z
    .string()
    .min(2, "Name must be at least 2 characters")
    .max(200, "Name cannot exceed 200 characters"),
  description: z
    .string()
    .max(1000, "Description cannot exceed 1000 characters")
    .optional()
    .or(z.literal("")),
});

export type ProductTypeFormData = z.infer<typeof productTypeSchema>;

// Product type schema for editing (code cannot be changed)
export const productTypeEditSchema = z.object({
  code: z.string(), // No validation - field is disabled when editing
  name: z
    .string()
    .min(2, "Name must be at least 2 characters")
    .max(200, "Name cannot exceed 200 characters"),
  description: z
    .string()
    .max(1000, "Description cannot exceed 1000 characters")
    .optional()
    .or(z.literal("")),
  isActive: z.boolean(),
});

export type ProductTypeEditFormData = z.infer<typeof productTypeEditSchema>;

// Add attribute to product type schema
export const addAttributeSchema = z.object({
  attributeDefinitionId: z.string().min(1, "Please select an attribute"),
  isRequired: z.boolean(),
  displayOrder: z.number().int().min(0),
});

export type AddAttributeFormData = z.infer<typeof addAttributeSchema>;
