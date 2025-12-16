"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2 } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Switch } from "@/components/ui/switch";
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
  productTypeSchema,
  productTypeEditSchema,
  type ProductTypeFormData,
  type ProductTypeEditFormData,
} from "@/lib/validations/product-type";

interface ProductTypeFormProps {
  defaultValues?: Partial<ProductTypeFormData | ProductTypeEditFormData>;
  isEditing?: boolean;
  onSubmit: (data: ProductTypeFormData | ProductTypeEditFormData) => Promise<void>;
  onCancel?: () => void;
  isLoading?: boolean;
}

export function ProductTypeForm({
  defaultValues,
  isEditing = false,
  onSubmit,
  onCancel,
  isLoading,
}: ProductTypeFormProps) {
  const form = useForm<ProductTypeFormData | ProductTypeEditFormData>({
    resolver: zodResolver(isEditing ? productTypeEditSchema : productTypeSchema),
    defaultValues: {
      code: "",
      name: "",
      description: "",
      ...(isEditing ? { isActive: true } : {}),
      ...defaultValues,
    },
  });

  // Auto-generate code from name
  const handleNameChange = (name: string) => {
    if (!isEditing && name) {
      const code = name
        .toLowerCase()
        .replace(/[^a-z0-9\s]/g, "")
        .replace(/\s+/g, "_")
        .slice(0, 100);
      form.setValue("code", code);
    }
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <div className="grid gap-6 md:grid-cols-2">
          <FormField
            control={form.control}
            name="name"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Name *</FormLabel>
                <FormControl>
                  <Input
                    placeholder="e.g., Laptop, Smartphone"
                    {...field}
                    onChange={(e) => {
                      field.onChange(e);
                      handleNameChange(e.target.value);
                    }}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="code"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Code *</FormLabel>
                <FormControl>
                  <Input
                    placeholder="e.g., laptop, smartphone"
                    {...field}
                    disabled={isEditing}
                    className={isEditing ? "bg-muted" : ""}
                  />
                </FormControl>
                <FormDescription>
                  {isEditing
                    ? "Code cannot be changed after creation"
                    : "Unique identifier (lowercase, no spaces)"}
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <FormField
          control={form.control}
          name="description"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Description</FormLabel>
              <FormControl>
                <Textarea
                  placeholder="Brief description of this product type..."
                  className="resize-none"
                  rows={3}
                  {...field}
                />
              </FormControl>
              <FormDescription>
                Optional description for admin reference
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        {isEditing && (
          <FormField
            control={form.control}
            name="isActive"
            render={({ field }) => (
              <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                <div className="space-y-0.5">
                  <FormLabel className="text-base">Active</FormLabel>
                  <FormDescription>
                    When inactive, this product type cannot be assigned to new products
                  </FormDescription>
                </div>
                <FormControl>
                  <Switch
                    checked={field.value as boolean}
                    onCheckedChange={field.onChange}
                  />
                </FormControl>
              </FormItem>
            )}
          />
        )}

        <div className="flex justify-end gap-4">
          {onCancel && (
            <Button type="button" variant="outline" onClick={onCancel}>
              Cancel
            </Button>
          )}
          <Button type="submit" disabled={isLoading}>
            {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            {isEditing ? "Save Changes" : "Create Product Type"}
          </Button>
        </div>
      </form>
    </Form>
  );
}
