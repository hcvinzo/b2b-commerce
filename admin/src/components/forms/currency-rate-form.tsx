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
import {
  currencyRateSchema,
  type CurrencyRateFormData,
  type UpdateCurrencyRateFormData,
} from "@/lib/validations/currency-rate";
import { CurrencyListItem } from "@/types/entities";

interface CurrencyRateFormProps {
  mode: "create" | "update";
  defaultValues?: Partial<CurrencyRateFormData>;
  currencies?: CurrencyListItem[];
  onSubmit: (data: CurrencyRateFormData | UpdateCurrencyRateFormData) => Promise<void>;
  onCancel?: () => void;
  isLoading?: boolean;
}

export function CurrencyRateForm({
  mode,
  defaultValues,
  currencies = [],
  onSubmit,
  onCancel,
  isLoading,
}: CurrencyRateFormProps) {
  const isUpdate = mode === "update";

  const form = useForm<CurrencyRateFormData>({
    resolver: zodResolver(currencyRateSchema),
    defaultValues: {
      fromCurrency: "",
      toCurrency: "",
      rate: 0,
      effectiveDate: new Date().toISOString().split("T")[0],
      ...defaultValues,
    },
  });

  const activeCurrencies = currencies.filter((c) => c.isActive);

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <div className="grid gap-6 md:grid-cols-2">
          {!isUpdate && (
            <>
              <FormField
                control={form.control}
                name="fromCurrency"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>From Currency *</FormLabel>
                    <Select
                      onValueChange={field.onChange}
                      defaultValue={field.value}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Select source currency" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {activeCurrencies.map((currency) => (
                          <SelectItem key={currency.id} value={currency.code}>
                            {currency.code} - {currency.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormDescription>
                      Source currency for the exchange rate
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="toCurrency"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>To Currency *</FormLabel>
                    <Select
                      onValueChange={field.onChange}
                      defaultValue={field.value}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Select target currency" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {activeCurrencies.map((currency) => (
                          <SelectItem key={currency.id} value={currency.code}>
                            {currency.code} - {currency.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormDescription>
                      Target currency for the exchange rate
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </>
          )}

          <FormField
            control={form.control}
            name="rate"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Exchange Rate *</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    step="0.000001"
                    min="0"
                    placeholder="0.00"
                    {...field}
                    onChange={(e) => field.onChange(parseFloat(e.target.value) || 0)}
                  />
                </FormControl>
                <FormDescription>
                  Rate value (e.g., 1 USD = X TRY)
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="effectiveDate"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Effective Date</FormLabel>
                <FormControl>
                  <Input
                    type="date"
                    {...field}
                  />
                </FormControl>
                <FormDescription>
                  Date when this rate becomes effective
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
            {isUpdate ? "Update Rate" : "Create Rate"}
          </Button>
        </div>
      </form>
    </Form>
  );
}
