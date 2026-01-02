import { z } from "zod";

// Schema for creating a new parameter
export const createParameterSchema = z.object({
  key: z
    .string()
    .min(3, "Key must be at least 3 characters")
    .max(100, "Key cannot exceed 100 characters")
    .regex(
      /^[a-z][a-z0-9]*(\.[a-z][a-z0-9-]*)+$/,
      "Key must be in format: module.submodule.name (e.g., cache.default-duration)"
    ),
  value: z.string().max(2000, "Value cannot exceed 2000 characters"),
  description: z.string().max(500, "Description cannot exceed 500 characters").optional(),
  parameterType: z.enum(["System", "Business"]),
  valueType: z.enum(["String", "Number", "Boolean", "DateTime", "Json"]),
  isEditable: z.boolean(),
});

export type CreateParameterFormData = z.infer<typeof createParameterSchema>;

// Schema for updating an existing parameter (only value, description, isEditable can change)
export const updateParameterSchema = z.object({
  value: z.string().max(2000, "Value cannot exceed 2000 characters").optional(),
  description: z.string().max(500, "Description cannot exceed 500 characters").optional(),
  isEditable: z.boolean().optional(),
});

export type UpdateParameterFormData = z.infer<typeof updateParameterSchema>;
