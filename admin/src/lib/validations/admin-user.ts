import { z } from "zod";

export const adminUserSchema = z.object({
  email: z
    .string()
    .min(1, "Email is required")
    .email("Must be a valid email address"),
  firstName: z
    .string()
    .max(50, "First name cannot exceed 50 characters")
    .optional()
    .or(z.literal("")),
  lastName: z
    .string()
    .max(50, "Last name cannot exceed 50 characters")
    .optional()
    .or(z.literal("")),
  phoneNumber: z
    .string()
    .max(20, "Phone number cannot exceed 20 characters")
    .optional()
    .or(z.literal("")),
  roles: z
    .array(z.string())
    .min(1, "At least one role is required"),
  temporaryPassword: z
    .string()
    .min(8, "Password must be at least 8 characters")
    .optional()
    .or(z.literal("")),
  sendWelcomeEmail: z.boolean().optional(),
});

export type AdminUserFormData = z.infer<typeof adminUserSchema>;

export const updateAdminUserSchema = z.object({
  firstName: z
    .string()
    .max(50, "First name cannot exceed 50 characters")
    .optional()
    .or(z.literal("")),
  lastName: z
    .string()
    .max(50, "Last name cannot exceed 50 characters")
    .optional()
    .or(z.literal("")),
  phoneNumber: z
    .string()
    .max(20, "Phone number cannot exceed 20 characters")
    .optional()
    .or(z.literal("")),
  roles: z
    .array(z.string())
    .min(1, "At least one role is required"),
});

export type UpdateAdminUserFormData = z.infer<typeof updateAdminUserSchema>;
