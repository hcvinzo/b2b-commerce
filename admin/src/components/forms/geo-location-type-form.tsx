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
  geoLocationTypeSchema,
  type GeoLocationTypeFormData,
} from "@/lib/validations/geo-location-type";

interface GeoLocationTypeFormProps {
  defaultValues?: Partial<GeoLocationTypeFormData>;
  onSubmit: (data: GeoLocationTypeFormData) => Promise<void>;
  onCancel?: () => void;
  isLoading?: boolean;
}

export function GeoLocationTypeForm({
  defaultValues,
  onSubmit,
  onCancel,
  isLoading,
}: GeoLocationTypeFormProps) {
  const form = useForm<GeoLocationTypeFormData>({
    resolver: zodResolver(geoLocationTypeSchema),
    defaultValues: {
      name: "",
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
            name="name"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Name *</FormLabel>
                <FormControl>
                  <Input placeholder="e.g., Country, City, District" {...field} />
                </FormControl>
                <FormDescription>
                  Name of the location type
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
                <FormLabel>Display Order *</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    min={0}
                    placeholder="0"
                    {...field}
                    onChange={(e) => field.onChange(parseInt(e.target.value) || 0)}
                  />
                </FormControl>
                <FormDescription>
                  Order in hierarchy (0 = top level, 1 = second level, etc.)
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
            Save Location Type
          </Button>
        </div>
      </form>
    </Form>
  );
}
