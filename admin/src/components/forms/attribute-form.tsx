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
  type AttributeEntityTypeValue,
} from "@/lib/validations/attribute";
import { AttributeTypeEnum, AttributeEntityTypeEnum, AttributeEntityType } from "@/types/entities";

// Maps integer type to display label
const ATTRIBUTE_TYPES: { value: AttributeTypeValue; label: string }[] = [
  { value: 1, label: "Text" },
  { value: 2, label: "Number" },
  { value: 3, label: "Select (Single)" },
  { value: 4, label: "Multi-Select" },
  { value: 5, label: "Boolean (Yes/No)" },
  { value: 6, label: "Date" },
  { value: 7, label: "Composite" },
];

// Maps integer entity type to display label
const ENTITY_TYPES: { value: AttributeEntityTypeValue; label: string }[] = [
  { value: 1, label: "Product" },
  { value: 2, label: "Customer" },
];

interface AttributeFormProps {
  defaultValues?: Partial<AttributeFormData>;
  isEditing?: boolean;
  /** Pre-set entity type (used when adding attribute from within a specific entity type tab) */
  presetEntityType?: AttributeEntityType;
  /** When true, this form is for creating a child attribute of a composite parent */
  isChildAttribute?: boolean;
  onSubmit: (data: AttributeFormData) => Promise<void>;
  onCancel?: () => void;
  isLoading?: boolean;
}

// Child attributes cannot be Composite type (no nested composites)
const CHILD_ATTRIBUTE_TYPES = ATTRIBUTE_TYPES.filter(t => t.value !== 7);

export function AttributeForm({
  defaultValues,
  isEditing = false,
  presetEntityType,
  isChildAttribute = false,
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
      entityType: presetEntityType ?? AttributeEntityTypeEnum.Product, // 1
      isList: false,
      unit: "",
      isFilterable: false,
      isRequired: false,
      isVisibleOnProductPage: true,
      displayOrder: 0,
      ...defaultValues,
    },
  });

  const watchedType = form.watch("type");
  const watchedEntityType = form.watch("entityType");
  const showUnitField = watchedType === AttributeTypeEnum.Number; // 2
  // Only show isVisibleOnProductPage toggle for Product attributes
  const showProductPageVisibility = watchedEntityType === AttributeEntityTypeEnum.Product;

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

          {/* Hide entity type for child attributes - they inherit from parent */}
          {!isChildAttribute && (
            <FormField
              control={form.control}
              name="entityType"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Entity Type *</FormLabel>
                  <Select
                    onValueChange={(val) => field.onChange(parseInt(val, 10))}
                    value={String(field.value)}
                    disabled={isEditing || !!presetEntityType}
                  >
                    <FormControl>
                      <SelectTrigger className={isEditing || presetEntityType ? "bg-muted" : ""}>
                        <SelectValue placeholder="Select entity type" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {ENTITY_TYPES.map((type) => (
                        <SelectItem key={type.value} value={String(type.value)}>
                          {type.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FormDescription>
                    {isEditing
                      ? "Entity type cannot be changed after creation"
                      : "Product attributes are for product specs, Customer attributes for dealer info"}
                  </FormDescription>
                  <FormMessage />
                </FormItem>
              )}
            />
          )}

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
                    {(isChildAttribute ? CHILD_ATTRIBUTE_TYPES : ATTRIBUTE_TYPES).map((type) => (
                      <SelectItem key={type.value} value={String(type.value)}>
                        {type.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <FormDescription>
                  {isEditing
                    ? "Type cannot be changed after creation"
                    : isChildAttribute
                      ? "Child attributes cannot be Composite type"
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
                    Make this attribute required
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
            name="isList"
            render={({ field }) => (
              <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                <div className="space-y-0.5">
                  <FormLabel className="text-base">Allow Multiple Values</FormLabel>
                  <FormDescription>
                    Entity can have multiple values for this attribute
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

          {showProductPageVisibility && (
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
          )}
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
