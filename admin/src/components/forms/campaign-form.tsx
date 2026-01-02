"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2 } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
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
  campaignSchema,
  type CampaignFormData,
} from "@/lib/validations/campaign";

interface CampaignFormProps {
  defaultValues?: Partial<CampaignFormData>;
  onSubmit: (data: CampaignFormData) => Promise<void>;
  onCancel?: () => void;
  isLoading?: boolean;
  isEditing?: boolean;
}

export function CampaignForm({
  defaultValues,
  onSubmit,
  onCancel,
  isLoading,
  isEditing = false,
}: CampaignFormProps) {
  const form = useForm<CampaignFormData>({
    resolver: zodResolver(campaignSchema),
    defaultValues: {
      name: "",
      description: "",
      startDate: "",
      endDate: "",
      priority: 0,
      totalBudgetLimitAmount: null,
      totalUsageLimit: null,
      perCustomerBudgetLimitAmount: null,
      perCustomerUsageLimit: null,
      currency: "TRY",
      ...defaultValues,
    },
  });

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <div className="grid gap-6 md:grid-cols-2">
          <FormField
            control={form.control}
            name="name"
            render={({ field }) => (
              <FormItem className="md:col-span-2">
                <FormLabel>Campaign Name *</FormLabel>
                <FormControl>
                  <Input placeholder="e.g., Summer Sale 2024" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="startDate"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Start Date *</FormLabel>
                <FormControl>
                  <Input
                    type="datetime-local"
                    {...field}
                    value={field.value ? field.value.slice(0, 16) : ""}
                    onChange={(e) => {
                      const value = e.target.value;
                      field.onChange(value ? new Date(value).toISOString() : "");
                    }}
                  />
                </FormControl>
                <FormDescription>When the campaign starts</FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="endDate"
            render={({ field }) => (
              <FormItem>
                <FormLabel>End Date *</FormLabel>
                <FormControl>
                  <Input
                    type="datetime-local"
                    {...field}
                    value={field.value ? field.value.slice(0, 16) : ""}
                    onChange={(e) => {
                      const value = e.target.value;
                      field.onChange(value ? new Date(value).toISOString() : "");
                    }}
                  />
                </FormControl>
                <FormDescription>When the campaign ends</FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="priority"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Priority</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    placeholder="0"
                    {...field}
                    onChange={(e) =>
                      field.onChange(parseInt(e.target.value) || 0)
                    }
                  />
                </FormControl>
                <FormDescription>
                  Higher priority campaigns take precedence
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="currency"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Currency</FormLabel>
                <FormControl>
                  <Input placeholder="TRY" {...field} value={field.value ?? "TRY"} />
                </FormControl>
                <FormDescription>Currency for budget limits</FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        {/* Budget Limits Section */}
        <div className="space-y-4">
          <h3 className="text-lg font-medium">Budget Limits</h3>
          <div className="grid gap-6 md:grid-cols-2">
            <FormField
              control={form.control}
              name="totalBudgetLimitAmount"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Total Budget Limit</FormLabel>
                  <FormControl>
                    <Input
                      type="number"
                      step="0.01"
                      placeholder="Unlimited"
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
                    Maximum total discount amount for this campaign
                  </FormDescription>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="totalUsageLimit"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Total Usage Limit</FormLabel>
                  <FormControl>
                    <Input
                      type="number"
                      placeholder="Unlimited"
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
                    Maximum number of times this campaign can be used
                  </FormDescription>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="perCustomerBudgetLimitAmount"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Per-Customer Budget Limit</FormLabel>
                  <FormControl>
                    <Input
                      type="number"
                      step="0.01"
                      placeholder="Unlimited"
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
                    Maximum discount per customer
                  </FormDescription>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="perCustomerUsageLimit"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Per-Customer Usage Limit</FormLabel>
                  <FormControl>
                    <Input
                      type="number"
                      placeholder="Unlimited"
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
                    Maximum times per customer
                  </FormDescription>
                  <FormMessage />
                </FormItem>
              )}
            />
          </div>
        </div>

        <FormField
          control={form.control}
          name="description"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Description</FormLabel>
              <FormControl>
                <Textarea
                  placeholder="Enter campaign description (optional)"
                  className="min-h-[100px]"
                  {...field}
                  value={field.value ?? ""}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="flex justify-end gap-4">
          {onCancel && (
            <Button type="button" variant="outline" onClick={onCancel}>
              Cancel
            </Button>
          )}
          <Button type="submit" disabled={isLoading}>
            {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            {isEditing ? "Save Changes" : "Create Campaign"}
          </Button>
        </div>
      </form>
    </Form>
  );
}
