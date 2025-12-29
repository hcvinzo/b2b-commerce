import { z } from "zod";

export const geoLocationSchema = z.object({
  geoLocationTypeId: z.string().min(1, "Location type is required"),
  code: z
    .string()
    .min(1, "Code is required")
    .max(50, "Code cannot exceed 50 characters"),
  name: z
    .string()
    .min(2, "Name must be at least 2 characters")
    .max(200, "Name cannot exceed 200 characters"),
  parentId: z.string().optional(),
  latitude: z.number().min(-90).max(90).optional().nullable(),
  longitude: z.number().min(-180).max(180).optional().nullable(),
  metadata: z.string().max(4000, "Metadata cannot exceed 4000 characters").optional(),
});

export type GeoLocationFormData = z.infer<typeof geoLocationSchema>;
