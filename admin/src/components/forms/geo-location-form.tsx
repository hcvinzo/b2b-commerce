"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2 } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
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
  geoLocationSchema,
  type GeoLocationFormData,
} from "@/lib/validations/geo-location";
import { GeoLocationType, GeoLocationListItem } from "@/types/entities";

interface GeoLocationFormProps {
  defaultValues?: Partial<GeoLocationFormData>;
  geoLocationTypes: GeoLocationType[];
  parentLocations?: GeoLocationListItem[];
  onSubmit: (data: GeoLocationFormData) => Promise<void>;
  onCancel?: () => void;
  onTypeChange?: (typeId: string) => void;
  isLoading?: boolean;
  isLoadingParents?: boolean;
  isEdit?: boolean;
}

export function GeoLocationForm({
  defaultValues,
  geoLocationTypes,
  parentLocations = [],
  onSubmit,
  onCancel,
  onTypeChange,
  isLoading,
  isLoadingParents = false,
  isEdit = false,
}: GeoLocationFormProps) {
  const form = useForm<GeoLocationFormData>({
    resolver: zodResolver(geoLocationSchema),
    defaultValues: {
      geoLocationTypeId: "",
      code: "",
      name: "",
      parentId: undefined,
      latitude: undefined,
      longitude: undefined,
      metadata: "",
      ...defaultValues,
    },
  });

  const selectedTypeId = form.watch("geoLocationTypeId");
  const selectedType = geoLocationTypes.find((t) => t.id === selectedTypeId);
  const isTopLevel = selectedType?.displayOrder === 0;

  const handleTypeChange = (value: string) => {
    form.setValue("geoLocationTypeId", value);
    form.setValue("parentId", undefined);
    onTypeChange?.(value);
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <div className="grid gap-6 md:grid-cols-2">
          {!isEdit && (
            <FormField
              control={form.control}
              name="geoLocationTypeId"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Location Type *</FormLabel>
                  <Select
                    onValueChange={handleTypeChange}
                    defaultValue={field.value}
                  >
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue placeholder="Select location type" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {geoLocationTypes.map((type) => (
                        <SelectItem key={type.id} value={type.id}>
                          {type.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />
          )}

          {!isEdit && (
            <FormField
              control={form.control}
              name="parentId"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Parent Location</FormLabel>
                  <Select
                    onValueChange={(value) =>
                      field.onChange(value === "none" ? undefined : value)
                    }
                    value={field.value || "none"}
                    disabled={!selectedTypeId || isTopLevel}
                  >
                    <FormControl>
                      <SelectTrigger>
                        <SelectValue
                          placeholder={
                            !selectedTypeId
                              ? "Select a type first"
                              : isTopLevel
                              ? "Top level - no parent needed"
                              : isLoadingParents
                              ? "Loading..."
                              : "Select parent location"
                          }
                        />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      {!isTopLevel && (
                        <>
                          <SelectItem value="none">No parent</SelectItem>
                          {parentLocations.map((loc) => (
                            <SelectItem key={loc.id} value={loc.id}>
                              {loc.name}
                            </SelectItem>
                          ))}
                        </>
                      )}
                    </SelectContent>
                  </Select>
                  <FormDescription>
                    {isTopLevel
                      ? "Top-level locations don't have a parent"
                      : "Select the parent location in the hierarchy"}
                  </FormDescription>
                  <FormMessage />
                </FormItem>
              )}
            />
          )}

          <FormField
            control={form.control}
            name="code"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Code *</FormLabel>
                <FormControl>
                  <Input placeholder="e.g., TR, US, IST" {...field} />
                </FormControl>
                <FormDescription>
                  Unique code for this location (e.g., ISO code)
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
                <FormLabel>Name *</FormLabel>
                <FormControl>
                  <Input placeholder="e.g., Turkey, Istanbul" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="latitude"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Latitude</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    step="any"
                    placeholder="e.g., 41.0082"
                    {...field}
                    value={field.value ?? ""}
                    onChange={(e) => {
                      const val = e.target.value;
                      field.onChange(val === "" ? undefined : parseFloat(val));
                    }}
                  />
                </FormControl>
                <FormDescription>
                  Latitude coordinate (-90 to 90)
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="longitude"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Longitude</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    step="any"
                    placeholder="e.g., 28.9784"
                    {...field}
                    value={field.value ?? ""}
                    onChange={(e) => {
                      const val = e.target.value;
                      field.onChange(val === "" ? undefined : parseFloat(val));
                    }}
                  />
                </FormControl>
                <FormDescription>
                  Longitude coordinate (-180 to 180)
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <FormField
          control={form.control}
          name="metadata"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Metadata</FormLabel>
              <FormControl>
                <Textarea
                  placeholder='Additional data in JSON format, e.g., {"timezone": "Europe/Istanbul"}'
                  className="min-h-[100px] font-mono text-sm"
                  {...field}
                  value={field.value || ""}
                />
              </FormControl>
              <FormDescription>
                Optional JSON data for additional properties
              </FormDescription>
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
            {isEdit ? "Update Location" : "Create Location"}
          </Button>
        </div>
      </form>
    </Form>
  );
}
