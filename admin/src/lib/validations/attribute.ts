import { z } from "zod";

// Backend sends integer enum values: 1=Text, 2=Number, 3=Select, 4=MultiSelect, 5=Boolean, 6=Date
export const AttributeTypeValues = [1, 2, 3, 4, 5, 6] as const;
export type AttributeTypeValue = (typeof AttributeTypeValues)[number];

// Attribute definition schema for creating new attributes
export const attributeSchema = z.object({
  code: z
    .string()
    .min(2, "Code must be at least 2 characters")
    .max(50, "Code cannot exceed 50 characters")
    .regex(
      /^[a-z0-9_]+$/,
      "Code can only contain lowercase letters, numbers, and underscores"
    ),
  name: z
    .string()
    .min(2, "Name must be at least 2 characters")
    .max(100, "Name cannot exceed 100 characters"),
  type: z.union([
    z.literal(1),
    z.literal(2),
    z.literal(3),
    z.literal(4),
    z.literal(5),
    z.literal(6),
  ], {
    message: "Please select an attribute type",
  }),
  unit: z
    .string()
    .max(20, "Unit cannot exceed 20 characters")
    .optional()
    .or(z.literal("")),
  isFilterable: z.boolean(),
  isRequired: z.boolean(),
  isVisibleOnProductPage: z.boolean(),
  displayOrder: z.number().int().min(0),
});

export type AttributeFormData = z.infer<typeof attributeSchema>;

// Attribute definition schema for editing (code and type are not validated since they can't be changed)
export const attributeEditSchema = z.object({
  code: z.string(), // No validation - field is disabled when editing
  name: z
    .string()
    .min(2, "Name must be at least 2 characters")
    .max(100, "Name cannot exceed 100 characters"),
  type: z.union([
    z.literal(1),
    z.literal(2),
    z.literal(3),
    z.literal(4),
    z.literal(5),
    z.literal(6),
  ]), // No error message needed - field is disabled when editing
  unit: z
    .string()
    .max(20, "Unit cannot exceed 20 characters")
    .optional()
    .or(z.literal("")),
  isFilterable: z.boolean(),
  isRequired: z.boolean(),
  isVisibleOnProductPage: z.boolean(),
  displayOrder: z.number().int().min(0),
});

export type AttributeEditFormData = z.infer<typeof attributeEditSchema>;

// Attribute value schema
export const attributeValueSchema = z.object({
  value: z
    .string()
    .min(1, "Value is required")
    .max(255, "Value cannot exceed 255 characters"),
  displayText: z
    .string()
    .max(255, "Display text cannot exceed 255 characters")
    .optional()
    .or(z.literal("")),
  displayOrder: z.number().int().min(0),
});

export type AttributeValueFormData = z.infer<typeof attributeValueSchema>;
