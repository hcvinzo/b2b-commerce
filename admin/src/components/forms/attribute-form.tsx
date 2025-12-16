"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2 } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
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
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  attributeSchema,
  attributeEditSchema,
  type AttributeFormData,
  type AttributeTypeValue,
} from "@/lib/validations/attribute";
import { AttributeTypeEnum } from "@/types/entities";

// Maps integer type to display label
const ATTRIBUTE_TYPES: { value: AttributeTypeValue; label: string }[] = [
  { value: 1, label: "Text" },
  { value: 2, label: "Number" },
  { value: 3, label: "Select (Single)" },
  { value: 4, label: "Multi-Select" },
  { value: 5, label: "Boolean (Yes/No)" },
  { value: 6, label: "Date" },
];

interface AttributeFormProps {
  defaultValues?: Partial<AttributeFormData>;
  isEditing?: boolean;
  onSubmit: (data: AttributeFormData) => Promise<void>;
  onCancel?: () => void;
  isLoading?: boolean;
}

export function AttributeForm({
  defaultValues,
  isEditing = false,
  onSubmit,
  onCancel,
  isLoading,
}: AttributeFormProps) {
  const form = useForm<AttributeFormData>({
    resolver: zodResolver(isEditing ? attributeEditSchema : attributeSchema),
    defaultValues: {
      code: "",
      name: "",
      type: AttributeTypeEnum.Text, // 1
      unit: "",
      isFilterable: false,
      isRequired: false,
      isVisibleOnProductPage: true,
      displayOrder: 0,
      ...defaultValues,
    },
  });

  const watchedType = form.watch("type");
  const showUnitField = watchedType === AttributeTypeEnum.Number; // 2

  // Auto-generate code from name
  const handleNameChange = (name: string) => {
    if (!isEditing && name) {
      const code = name
        .toLowerCase()
        .replace(/[^a-z0-9\s]/g, "")
        .replace(/\s+/g, "_")
        .slice(0, 50);
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
                    placeholder="e.g., CPU Model"
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
                    placeholder="e.g., cpu_model"
                    {...field}
                    disabled={isEditing}
                    className={isEditing ? "bg-muted" : ""}
                  />
                </FormControl>
                <FormDescription>
                  Unique identifier (lowercase, no spaces)
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="type"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Type *</FormLabel>
                <Select
                  onValueChange={(val) => field.onChange(parseInt(val, 10))}
                  value={String(field.value)}
                  disabled={isEditing}
                >
                  <FormControl>
                    <SelectTrigger className={isEditing ? "bg-muted" : ""}>
                      <SelectValue placeholder="Select attribute type" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    {ATTRIBUTE_TYPES.map((type) => (
                      <SelectItem key={type.value} value={String(type.value)}>
                        {type.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FormDescription>
                  {isEditing
                    ? "Type cannot be changed after creation"
                    : "Select/Multi-Select types support predefined values"}
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />

          {showUnitField && (
            <FormField
              control={form.control}
              name="unit"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Unit</FormLabel>
                  <FormControl>
                    <Input placeholder="e.g., GB, MHz, mm" {...field} />
                  </FormControl>
                  <FormDescription>Unit of measurement</FormDescription>
                  <FormMessage />
                </FormItem>
              )}
            />
          )}

          <FormField
            control={form.control}
            name="displayOrder"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Display Order</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    placeholder="0"
                    {...field}
                    onChange={(e) => field.onChange(parseInt(e.target.value) || 0)}
                  />
                </FormControl>
                <FormDescription>Lower numbers appear first</FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <div className="space-y-4">
          <FormField
            control={form.control}
            name="isFilterable"
            render={({ field }) => (
              <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                <div className="space-y-0.5">
                  <FormLabel className="text-base">Filterable</FormLabel>
                  <FormDescription>
                    Show this attribute in product filters
                  </FormDescription>
                </div>
                <FormControl>
                  <Switch
                    checked={field.value}
                    onCheckedChange={field.onChange}
                  />
                </FormControl>
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="isRequired"
            render={({ field }) => (
              <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                <div className="space-y-0.5">
                  <FormLabel className="text-base">Required</FormLabel>
                  <FormDescription>
                    Make this attribute required on products
                  </FormDescription>
                </div>
                <FormControl>
                  <Switch
                    checked={field.value}
                    onCheckedChange={field.onChange}
                  />
                </FormControl>
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="isVisibleOnProductPage"
            render={({ field }) => (
              <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                <div className="space-y-0.5">
                  <FormLabel className="text-base">
                    Visible on Product Page
                  </FormLabel>
                  <FormDescription>
                    Display this attribute on the product detail page
                  </FormDescription>
                </div>
                <FormControl>
                  <Switch
                    checked={field.value}
                    onCheckedChange={field.onChange}
                  />
                </FormControl>
              </FormItem>
            )}
          />
        </div>

        <div className="flex justify-end gap-4">
          {onCancel && (
            <Button type="button" variant="outline" onClick={onCancel}>
              Cancel
            </Button>
          )}
          <Button type="submit" disabled={isLoading}>
            {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            {isEditing ? "Save Changes" : "Create Attribute"}
          </Button>
        </div>
      </form>
    </Form>
  );
}
