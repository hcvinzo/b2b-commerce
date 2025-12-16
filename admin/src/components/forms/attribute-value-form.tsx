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
  attributeValueSchema,
  type AttributeValueFormData,
} from "@/lib/validations/attribute";

interface AttributeValueFormProps {
  defaultValues?: Partial<AttributeValueFormData>;
  onSubmit: (data: AttributeValueFormData) => Promise<void>;
  onCancel?: () => void;
  isLoading?: boolean;
}

export function AttributeValueForm({
  defaultValues,
  onSubmit,
  onCancel,
  isLoading,
}: AttributeValueFormProps) {
  const form = useForm<AttributeValueFormData>({
    resolver: zodResolver(attributeValueSchema),
    defaultValues: {
      value: "",
      displayText: "",
      displayOrder: 0,
      ...defaultValues,
    },
  });

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="value"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Value *</FormLabel>
              <FormControl>
                <Input placeholder="e.g., i7-14700K" {...field} />
              </FormControl>
              <FormDescription>Internal value used in system</FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="displayText"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Display Text</FormLabel>
              <FormControl>
                <Input
                  placeholder="e.g., Intel Core i7-14700K"
                  {...field}
                />
              </FormControl>
              <FormDescription>
                User-friendly display text (defaults to value if empty)
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

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
                  onChange={(e) =>
                    field.onChange(parseInt(e.target.value) || 0)
                  }
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="flex justify-end gap-2 pt-2">
          {onCancel && (
            <Button
              type="button"
              variant="outline"
              size="sm"
              onClick={onCancel}
            >
              Cancel
            </Button>
          )}
          <Button type="submit" size="sm" disabled={isLoading}>
            {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            {defaultValues?.value ? "Save" : "Add Value"}
          </Button>
        </div>
      </form>
    </Form>
  );
}
