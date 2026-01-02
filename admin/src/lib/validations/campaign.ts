import { z } from "zod";

// Campaign form validation schema
export const campaignSchema = z
  .object({
    name: z
      .string()
      .min(2, "Name must be at least 2 characters")
      .max(200, "Name cannot exceed 200 characters"),
    description: z
      .string()
      .max(1000, "Description cannot exceed 1000 characters")
      .optional(),
    startDate: z.string().min(1, "Start date is required"),
    endDate: z.string().min(1, "End date is required"),
    priority: z.number().int().min(0, "Priority must be non-negative").optional(),
    totalBudgetLimitAmount: z
      .number()
      .min(0, "Budget must be non-negative")
      .optional()
      .nullable(),
    totalUsageLimit: z
      .number()
      .int()
      .min(0, "Usage limit must be non-negative")
      .optional()
      .nullable(),
    perCustomerBudgetLimitAmount: z
      .number()
      .min(0, "Per-customer budget must be non-negative")
      .optional()
      .nullable(),
    perCustomerUsageLimit: z
      .number()
      .int()
      .min(0, "Per-customer usage limit must be non-negative")
      .optional()
      .nullable(),
    currency: z.string().optional(),
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

export type CampaignFormData = z.infer<typeof campaignSchema>;

// Discount Rule form validation schema
// Note: Product/category/brand/customer selection is done after rule creation,
// so we only validate customerTiers here (which has UI in the form)
export const discountRuleSchema = z
  .object({
    discountType: z.enum(["Percentage", "FixedAmount"]),
    discountValue: z
      .number()
      .positive("Discount value must be positive"),
    maxDiscountAmount: z
      .number()
      .positive("Max discount must be positive")
      .optional()
      .nullable(),
    productTargetType: z.enum(["AllProducts", "SpecificProducts", "Categories", "Brands"]),
    customerTargetType: z.enum(["AllCustomers", "SpecificCustomers", "CustomerTiers"]),
    minOrderAmount: z
      .number()
      .min(0, "Min order amount must be non-negative")
      .optional()
      .nullable(),
    minQuantity: z
      .number()
      .int()
      .min(1, "Min quantity must be at least 1")
      .optional()
      .nullable(),
    productIds: z.array(z.string()).optional(),
    categoryIds: z.array(z.string()).optional(),
    brandIds: z.array(z.string()).optional(),
    customerIds: z.array(z.string()).optional(),
    customerTiers: z.array(z.enum(["Tier1", "Tier2", "Tier3", "Tier4", "Tier5"])).optional(),
  })
  .refine(
    (data) => {
      if (data.discountType === "Percentage") {
        return data.discountValue <= 100;
      }
      return true;
    },
    {
      message: "Percentage discount cannot exceed 100%",
      path: ["discountValue"],
    }
  )
  .refine(
    (data) => {
      // Only validate customerTiers since it has UI in the form
      // Products/categories/brands/customers are added after rule creation
      if (data.customerTargetType === "CustomerTiers") {
        return data.customerTiers && data.customerTiers.length > 0;
      }
      return true;
    },
    {
      message: "Please select at least one customer tier",
      path: ["customerTiers"],
    }
  );

export type DiscountRuleFormData = z.infer<typeof discountRuleSchema>;
