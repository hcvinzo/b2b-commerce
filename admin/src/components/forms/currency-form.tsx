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
  currencySchema,
  type CurrencyFormData,
} from "@/lib/validations/currency";

interface CurrencyFormProps {
  mode: "create" | "update";
  defaultValues?: Partial<CurrencyFormData>;
  onSubmit: (data: CurrencyFormData) => Promise<void>;
  onCancel?: () => void;
  isLoading?: boolean;
}

export function CurrencyForm({
  mode,
  defaultValues,
  onSubmit,
  onCancel,
  isLoading,
}: CurrencyFormProps) {
  const isUpdate = mode === "update";

  const form = useForm<CurrencyFormData>({
    resolver: zodResolver(currencySchema),
    defaultValues: {
      code: "",
      name: "",
      symbol: "",
      decimalPlaces: 2,
      displayOrder: 0,
      ...defaultValues,
    },
  });

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <div className="grid gap-6 md:grid-cols-2">
          <FormField
            control={form.control}
            name="code"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Currency Code *</FormLabel>
                <FormControl>
                  <Input
                    placeholder="USD"
                    {...field}
                    disabled={isUpdate}
                    className={isUpdate ? "bg-muted" : ""}
                    onChange={(e) =>
                      field.onChange(e.target.value.toUpperCase())
                    }
                  />
                </FormControl>
                <FormDescription>
                  ISO 4217 code (e.g., USD, EUR, TRY)
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="name"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Currency Name *</FormLabel>
                <FormControl>
                  <Input placeholder="US Dollar" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="symbol"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Symbol *</FormLabel>
                <FormControl>
                  <Input placeholder="$" {...field} />
                </FormControl>
                <FormDescription>Currency symbol (e.g., $, €, ₺)</FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="decimalPlaces"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Decimal Places *</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    min={0}
                    max={4}
                    {...field}
                    onChange={(e) => field.onChange(parseInt(e.target.value) || 0)}
                  />
                </FormControl>
                <FormDescription>
                  Number of decimal places (typically 2)
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
                    min={0}
                    {...field}
                    onChange={(e) => field.onChange(parseInt(e.target.value) || 0)}
                  />
                </FormControl>
                <FormDescription>
                  Order in which the currency appears in lists
                </FormDescription>
                <FormMessage />
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
            {isUpdate ? "Update Currency" : "Create Currency"}
          </Button>
        </div>
      </form>
    </Form>
  );
}
