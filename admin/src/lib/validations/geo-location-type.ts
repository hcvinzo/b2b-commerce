import { z } from "zod";

export const geoLocationTypeSchema = z.object({
  name: z
    .string()
    .min(2, "Name must be at least 2 characters")
    .max(100, "Name cannot exceed 100 characters"),
  displayOrder: z.number().int().min(0, "Display order must be 0 or greater"),
});

export type GeoLocationTypeFormData = z.infer<typeof geoLocationTypeSchema>;
