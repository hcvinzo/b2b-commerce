import { z } from "zod";

export const apiClientSchema = z.object({
  name: z
    .string()
    .min(2, "Name must be at least 2 characters")
    .max(100, "Name cannot exceed 100 characters"),
  description: z
    .string()
    .max(500, "Description cannot exceed 500 characters")
    .optional()
    .or(z.literal("")),
  contactEmail: z
    .string()
    .min(1, "Email is required")
    .email("Must be a valid email address"),
  contactPhone: z
    .string()
    .max(20, "Phone number cannot exceed 20 characters")
    .optional()
    .or(z.literal("")),
});

export type ApiClientFormData = z.infer<typeof apiClientSchema>;

export const apiKeySchema = z.object({
  name: z
    .string()
    .min(2, "Name must be at least 2 characters")
    .max(100, "Name cannot exceed 100 characters"),
  rateLimitPerMinute: z
    .number()
    .int()
    .min(1, "Rate limit must be at least 1")
    .max(10000, "Rate limit cannot exceed 10000"),
  expiresAt: z.string().optional().or(z.literal("")),
  permissions: z.array(z.string()).optional(),
});

export type ApiKeyFormData = z.infer<typeof apiKeySchema>;

export const revokeKeySchema = z.object({
  reason: z
    .string()
    .min(5, "Reason must be at least 5 characters")
    .max(500, "Reason cannot exceed 500 characters"),
});

export type RevokeKeyFormData = z.infer<typeof revokeKeySchema>;

// IP address or CIDR notation regex
const ipv4Regex = /^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$/;
const cidrRegex = /^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\/(?:3[0-2]|[12]?[0-9])$/;

export const ipWhitelistSchema = z.object({
  ipAddress: z
    .string()
    .min(1, "IP address is required")
    .refine(
      (val) => ipv4Regex.test(val) || cidrRegex.test(val),
      "Must be a valid IPv4 address or CIDR notation (e.g., 192.168.1.1 or 192.168.1.0/24)"
    ),
  description: z
    .string()
    .max(200, "Description cannot exceed 200 characters")
    .optional()
    .or(z.literal("")),
});

export type IpWhitelistFormData = z.infer<typeof ipWhitelistSchema>;
