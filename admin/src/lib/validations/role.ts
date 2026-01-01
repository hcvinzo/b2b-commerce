import { z } from "zod";

export const roleSchema = z.object({
  name: z
    .string()
    .min(2, "Name must be at least 2 characters")
    .max(50, "Name cannot exceed 50 characters")
    .regex(
      /^[a-zA-Z][a-zA-Z0-9_-]*$/,
      "Name must start with a letter and contain only letters, numbers, underscores, and hyphens"
    ),
  description: z
    .string()
    .max(200, "Description cannot exceed 200 characters")
    .optional()
    .or(z.literal("")),
  userType: z.enum(["Admin", "Customer"]).optional(),
  claims: z.array(z.string()).optional(),
});

export type RoleFormData = z.infer<typeof roleSchema>;

export const updateRoleSchema = z.object({
  name: z
    .string()
    .min(2, "Name must be at least 2 characters")
    .max(50, "Name cannot exceed 50 characters")
    .regex(
      /^[a-zA-Z][a-zA-Z0-9_-]*$/,
      "Name must start with a letter and contain only letters, numbers, underscores, and hyphens"
    )
    .optional(),
  description: z
    .string()
    .max(200, "Description cannot exceed 200 characters")
    .optional()
    .or(z.literal("")),
});

export type UpdateRoleFormData = z.infer<typeof updateRoleSchema>;
