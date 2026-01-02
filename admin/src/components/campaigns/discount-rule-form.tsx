"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2 } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Checkbox } from "@/components/ui/checkbox";
import {
  discountRuleSchema,
  type DiscountRuleFormData,
} from "@/lib/validations/campaign";
import {
  CreateDiscountRuleDto,
  DiscountType,
  ProductTargetType,
  CustomerTargetType,
  PriceTier,
  DiscountTypeLabels,
  ProductTargetTypeLabels,
  CustomerTargetTypeLabels,
  PriceTierLabels,
} from "@/types/entities";

interface DiscountRuleFormProps {
  onSubmit: (data: CreateDiscountRuleDto) => Promise<void>;
  onCancel: () => void;
  isLoading?: boolean;
}

const discountTypes: DiscountType[] = ["Percentage", "FixedAmount"];
const productTargetTypes: ProductTargetType[] = [
  "AllProducts",
  "SpecificProducts",
  "Categories",
  "Brands",
];
const customerTargetTypes: CustomerTargetType[] = [
  "AllCustomers",
  "SpecificCustomers",
  "CustomerTiers",
];
const priceTiers: PriceTier[] = ["Tier1", "Tier2", "Tier3", "Tier4", "Tier5"];

export function DiscountRuleForm({
  onSubmit,
  onCancel,
  isLoading,
}: DiscountRuleFormProps) {
  const form = useForm<DiscountRuleFormData>({
    resolver: zodResolver(discountRuleSchema),
    defaultValues: {
      discountType: "Percentage",
      discountValue: 10,
      maxDiscountAmount: null,
      productTargetType: "AllProducts",
      customerTargetType: "AllCustomers",
      minOrderAmount: null,
      minQuantity: null,
      productIds: [],
      categoryIds: [],
      brandIds: [],
      customerIds: [],
      customerTiers: [],
    },
  });

  const discountType = form.watch("discountType");
  const productTargetType = form.watch("productTargetType");
  const customerTargetType = form.watch("customerTargetType");

  const handleSubmit = async (data: DiscountRuleFormData) => {
    const dto: CreateDiscountRuleDto = {
      discountType: data.discountType,
      discountValue: data.discountValue,
      maxDiscountAmount: data.maxDiscountAmount ?? undefined,
      productTargetType: data.productTargetType,
      customerTargetType: data.customerTargetType,
      minOrderAmount: data.minOrderAmount ?? undefined,
      minQuantity: data.minQuantity ?? undefined,
      productIds: data.productIds,
      categoryIds: data.categoryIds,
      brandIds: data.brandIds,
      customerIds: data.customerIds,
      customerTiers: data.customerTiers,
    };
    await onSubmit(dto);
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(handleSubmit)} className="space-y-6">
        {/* Discount Configuration */}
        <div className="space-y-4">
          <h4 className="font-medium">Discount Configuration</h4>
          <div className="grid gap-4 md:grid-cols-2">
            <FormField
              control={form.control}
              name="discountType"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Discount Type *</FormLabel>
                  <Select
                    onValueChange={field.onChange}
                    value={field.value}
                    disabled={isLoading}
                  >
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue placeholder="Select type" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {discountTypes.map((type) => (
                        <SelectItem key={type} value={type}>
                          {DiscountTypeLabels[type]}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="discountValue"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>
                    {discountType === "Percentage"
                      ? "Percentage (%)"
                      : "Amount"}{" "}
                    *
                  </FormLabel>
                  <FormControl>
                    <Input
                      type="number"
                      step={discountType === "Percentage" ? "1" : "0.01"}
                      max={discountType === "Percentage" ? 100 : undefined}
                      {...field}
                      onChange={(e) =>
                        field.onChange(parseFloat(e.target.value) || 0)
                      }
                    />
                  </FormControl>
                  <FormDescription>
                    {discountType === "Percentage"
                      ? "Enter a value between 1 and 100"
                      : "Fixed discount amount"}
                  </FormDescription>
                  <FormMessage />
                </FormItem>
              )}
            />

            {discountType === "Percentage" && (
              <FormField
                control={form.control}
                name="maxDiscountAmount"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Maximum Discount Amount</FormLabel>
                    <FormControl>
                      <Input
                        type="number"
                        step="0.01"
                        placeholder="No limit"
                        {...field}
                        value={field.value ?? ""}
                        onChange={(e) =>
                          field.onChange(
                            e.target.value ? parseFloat(e.target.value) : null
                          )
                        }
                      />
                    </FormControl>
                    <FormDescription>
                      Cap the maximum discount for percentage discounts
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />
            )}
          </div>
        </div>

        {/* Product Targeting */}
        <div className="space-y-4">
          <h4 className="font-medium">Product Targeting</h4>
          <FormField
            control={form.control}
            name="productTargetType"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Apply To *</FormLabel>
                <Select
                  onValueChange={field.onChange}
                  value={field.value}
                  disabled={isLoading}
                >
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder="Select targeting" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    {productTargetTypes.map((type) => (
                      <SelectItem key={type} value={type}>
                        {ProductTargetTypeLabels[type]}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FormDescription>
                  {productTargetType === "AllProducts" &&
                    "Discount applies to all products"}
                  {productTargetType === "SpecificProducts" &&
                    "Select specific products after creating the rule"}
                  {productTargetType === "Categories" &&
                    "Select categories after creating the rule"}
                  {productTargetType === "Brands" &&
                    "Select brands after creating the rule"}
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        {/* Customer Targeting */}
        <div className="space-y-4">
          <h4 className="font-medium">Customer Targeting</h4>
          <FormField
            control={form.control}
            name="customerTargetType"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Eligible Customers *</FormLabel>
                <Select
                  onValueChange={field.onChange}
                  value={field.value}
                  disabled={isLoading}
                >
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder="Select targeting" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    {customerTargetTypes.map((type) => (
                      <SelectItem key={type} value={type}>
                        {CustomerTargetTypeLabels[type]}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            )}
          />

          {customerTargetType === "CustomerTiers" && (
            <FormField
              control={form.control}
              name="customerTiers"
              render={() => (
                <FormItem>
                  <FormLabel>Select Customer Tiers</FormLabel>
                  <div className="grid grid-cols-3 gap-4">
                    {priceTiers.map((tier) => (
                      <FormField
                        key={tier}
                        control={form.control}
                        name="customerTiers"
                        render={({ field }) => (
                          <FormItem className="flex flex-row items-start space-x-3 space-y-0">
                            <FormControl>
                              <Checkbox
                                checked={field.value?.includes(tier)}
                                onCheckedChange={(checked) => {
                                  const currentValue = field.value || [];
                                  if (checked) {
                                    field.onChange([...currentValue, tier]);
                                  } else {
                                    field.onChange(
                                      currentValue.filter((t) => t !== tier)
                                    );
                                  }
                                }}
                              />
                            </FormControl>
                            <FormLabel className="font-normal cursor-pointer">
                              {PriceTierLabels[tier]}
                            </FormLabel>
                          </FormItem>
                        )}
                      />
                    ))}
                  </div>
                  <FormMessage />
                </FormItem>
              )}
            />
          )}

          {customerTargetType === "SpecificCustomers" && (
            <p className="text-sm text-muted-foreground">
              You can add specific customers after creating the rule.
            </p>
          )}
        </div>

        {/* Conditions */}
        <div className="space-y-4">
          <h4 className="font-medium">Conditions (Optional)</h4>
          <div className="grid gap-4 md:grid-cols-2">
            <FormField
              control={form.control}
              name="minOrderAmount"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Minimum Order Amount</FormLabel>
                  <FormControl>
                    <Input
                      type="number"
                      step="0.01"
                      placeholder="No minimum"
                      {...field}
                      value={field.value ?? ""}
                      onChange={(e) =>
                        field.onChange(
                          e.target.value ? parseFloat(e.target.value) : null
                        )
                      }
                    />
                  </FormControl>
                  <FormDescription>
                    Minimum order value to apply discount
                  </FormDescription>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="minQuantity"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Minimum Quantity</FormLabel>
                  <FormControl>
                    <Input
                      type="number"
                      placeholder="No minimum"
                      {...field}
                      value={field.value ?? ""}
                      onChange={(e) =>
                        field.onChange(
                          e.target.value ? parseInt(e.target.value) : null
                        )
                      }
                    />
                  </FormControl>
                  <FormDescription>
                    Minimum quantity per product
                  </FormDescription>
                  <FormMessage />
                </FormItem>
              )}
            />
          </div>
        </div>

        <div className="flex justify-end gap-4">
          <Button type="button" variant="outline" onClick={onCancel}>
            Cancel
          </Button>
          <Button type="submit" disabled={isLoading}>
            {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Add Rule
          </Button>
        </div>
      </form>
    </Form>
  );
}
