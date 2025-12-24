"use client";

import { useState, useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2, Check, X, ChevronsUpDown } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
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
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { TreeMultiSelectExt } from "@/components/ui/tree-multi-select-ext";
import {
  collectionFiltersSchema,
  type CollectionFiltersFormData,
} from "@/lib/validations/collection";
import { useSetCollectionFilters } from "@/hooks/use-collections";
import { useCategories } from "@/hooks/use-categories";
import { useBrands } from "@/hooks/use-brands";
import { useProductTypes } from "@/hooks/use-product-types";
import { Collection } from "@/types/entities";
import { cn } from "@/lib/utils";

interface CollectionFiltersEditorProps {
  collection: Collection;
}

export function CollectionFiltersEditor({
  collection,
}: CollectionFiltersEditorProps) {
  const setFilters = useSetCollectionFilters();
  const { data: categories, isLoading: categoriesLoading } = useCategories();
  const { data: brandsData, isLoading: brandsLoading } = useBrands();
  const { data: productTypes, isLoading: productTypesLoading } = useProductTypes();

  const [hasChanges, setHasChanges] = useState(false);
  const [brandOpen, setBrandOpen] = useState(false);
  const [productTypeOpen, setProductTypeOpen] = useState(false);

  const form = useForm<CollectionFiltersFormData>({
    resolver: zodResolver(collectionFiltersSchema),
    defaultValues: {
      categoryIds: collection.filter?.categoryIds || [],
      brandIds: collection.filter?.brandIds || [],
      productTypeIds: collection.filter?.productTypeIds || [],
      minPrice: collection.filter?.minPrice ?? undefined,
      maxPrice: collection.filter?.maxPrice ?? undefined,
    },
  });

  // Watch for form changes
  useEffect(() => {
    const subscription = form.watch(() => {
      setHasChanges(true);
    });
    return () => subscription.unsubscribe();
  }, [form]);

  // Reset form when collection changes
  useEffect(() => {
    form.reset({
      categoryIds: collection.filter?.categoryIds || [],
      brandIds: collection.filter?.brandIds || [],
      productTypeIds: collection.filter?.productTypeIds || [],
      minPrice: collection.filter?.minPrice ?? undefined,
      maxPrice: collection.filter?.maxPrice ?? undefined,
    });
    setHasChanges(false);
  }, [collection, form]);

  const handleSave = async (data: CollectionFiltersFormData) => {
    await setFilters.mutateAsync({
      collectionId: collection.id,
      data: {
        categoryIds: data.categoryIds?.length ? data.categoryIds : undefined,
        brandIds: data.brandIds?.length ? data.brandIds : undefined,
        productTypeIds: data.productTypeIds?.length ? data.productTypeIds : undefined,
        minPrice: data.minPrice,
        maxPrice: data.maxPrice,
      },
    });
    setHasChanges(false);
  };

  const handleReset = () => {
    form.reset({
      categoryIds: collection.filter?.categoryIds || [],
      brandIds: collection.filter?.brandIds || [],
      productTypeIds: collection.filter?.productTypeIds || [],
      minPrice: collection.filter?.minPrice ?? undefined,
      maxPrice: collection.filter?.maxPrice ?? undefined,
    });
    setHasChanges(false);
  };

  const isLoading = categoriesLoading || brandsLoading || productTypesLoading;

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-8">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  // Brands come from paginated response
  const brands = brandsData?.items || [];

  // Product types come as array
  const productTypesList = productTypes || [];

  const toggleBrand = (brandId: string, currentValue: string[]) => {
    if (currentValue.includes(brandId)) {
      return currentValue.filter((id) => id !== brandId);
    }
    return [...currentValue, brandId];
  };

  const toggleProductType = (productTypeId: string, currentValue: string[]) => {
    if (currentValue.includes(productTypeId)) {
      return currentValue.filter((id) => id !== productTypeId);
    }
    return [...currentValue, productTypeId];
  };

  return (
    <Card>
      <CardHeader>
        <CardTitle>Collection Filters</CardTitle>
        <CardDescription>
          Define filter criteria for dynamic product matching. Products matching
          these filters will be automatically included.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(handleSave)} className="space-y-6">
            {/* Unsaved changes bar */}
            {hasChanges && (
              <div className="flex items-center justify-between rounded-lg border border-yellow-200 bg-yellow-50 p-4 dark:border-yellow-900 dark:bg-yellow-950">
                <p className="text-sm text-yellow-800 dark:text-yellow-200">
                  You have unsaved changes
                </p>
                <div className="flex gap-2">
                  <Button
                    type="button"
                    variant="outline"
                    size="sm"
                    onClick={handleReset}
                  >
                    <X className="mr-2 h-4 w-4" />
                    Reset
                  </Button>
                  <Button
                    type="submit"
                    size="sm"
                    disabled={setFilters.isPending}
                  >
                    {setFilters.isPending && (
                      <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    )}
                    <Check className="mr-2 h-4 w-4" />
                    Save Changes
                  </Button>
                </div>
              </div>
            )}

            <div className="grid gap-6 md:grid-cols-2">
              <FormField
                control={form.control}
                name="categoryIds"
                render={({ field }) => (
                  <FormItem className="md:col-span-2">
                    <FormLabel>Categories</FormLabel>
                    <FormControl>
                      <TreeMultiSelectExt
                        categories={categories || []}
                        value={field.value || []}
                        onChange={field.onChange}
                        placeholder="Select categories"
                      />
                    </FormControl>
                    <FormDescription>
                      Products in any of these categories will be included
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="brandIds"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Brands</FormLabel>
                    <Popover open={brandOpen} onOpenChange={setBrandOpen}>
                      <PopoverTrigger asChild>
                        <FormControl>
                          <Button
                            variant="outline"
                            role="combobox"
                            aria-expanded={brandOpen}
                            className="w-full justify-between font-normal"
                          >
                            {field.value?.length
                              ? `${field.value.length} brand(s) selected`
                              : "Select brands"}
                            <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                          </Button>
                        </FormControl>
                      </PopoverTrigger>
                      <PopoverContent className="w-full p-0" align="start">
                        <Command>
                          <CommandInput placeholder="Search brands..." />
                          <CommandList className="max-h-[200px]">
                            <CommandEmpty>No brand found.</CommandEmpty>
                            <CommandGroup>
                              {brands.map((brand) => (
                                <CommandItem
                                  key={brand.id}
                                  value={brand.name}
                                  onSelect={() => {
                                    field.onChange(
                                      toggleBrand(brand.id, field.value || [])
                                    );
                                  }}
                                >
                                  <Check
                                    className={cn(
                                      "mr-2 h-4 w-4",
                                      field.value?.includes(brand.id)
                                        ? "opacity-100"
                                        : "opacity-0"
                                    )}
                                  />
                                  {brand.name}
                                </CommandItem>
                              ))}
                            </CommandGroup>
                          </CommandList>
                        </Command>
                      </PopoverContent>
                    </Popover>
                    {field.value && field.value.length > 0 && (
                      <div className="flex flex-wrap gap-1 mt-2">
                        {field.value.map((brandId) => {
                          const brand = brands.find((b) => b.id === brandId);
                          return brand ? (
                            <Badge
                              key={brandId}
                              variant="secondary"
                              className="cursor-pointer"
                              onClick={() =>
                                field.onChange(
                                  toggleBrand(brandId, field.value || [])
                                )
                              }
                            >
                              {brand.name}
                              <X className="ml-1 h-3 w-3" />
                            </Badge>
                          ) : null;
                        })}
                      </div>
                    )}
                    <FormDescription>
                      Products from any of these brands will be included
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="productTypeIds"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Product Types</FormLabel>
                    <Popover open={productTypeOpen} onOpenChange={setProductTypeOpen}>
                      <PopoverTrigger asChild>
                        <FormControl>
                          <Button
                            variant="outline"
                            role="combobox"
                            aria-expanded={productTypeOpen}
                            className="w-full justify-between font-normal"
                          >
                            {field.value?.length
                              ? `${field.value.length} type(s) selected`
                              : "Select product types"}
                            <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
                          </Button>
                        </FormControl>
                      </PopoverTrigger>
                      <PopoverContent className="w-full p-0" align="start">
                        <Command>
                          <CommandInput placeholder="Search product types..." />
                          <CommandList className="max-h-[200px]">
                            <CommandEmpty>No product type found.</CommandEmpty>
                            <CommandGroup>
                              {productTypesList.map((pt) => (
                                <CommandItem
                                  key={pt.id}
                                  value={pt.name}
                                  onSelect={() => {
                                    field.onChange(
                                      toggleProductType(pt.id, field.value || [])
                                    );
                                  }}
                                >
                                  <Check
                                    className={cn(
                                      "mr-2 h-4 w-4",
                                      field.value?.includes(pt.id)
                                        ? "opacity-100"
                                        : "opacity-0"
                                    )}
                                  />
                                  {pt.name}
                                </CommandItem>
                              ))}
                            </CommandGroup>
                          </CommandList>
                        </Command>
                      </PopoverContent>
                    </Popover>
                    {field.value && field.value.length > 0 && (
                      <div className="flex flex-wrap gap-1 mt-2">
                        {field.value.map((ptId) => {
                          const pt = productTypesList.find((p) => p.id === ptId);
                          return pt ? (
                            <Badge
                              key={ptId}
                              variant="secondary"
                              className="cursor-pointer"
                              onClick={() =>
                                field.onChange(
                                  toggleProductType(ptId, field.value || [])
                                )
                              }
                            >
                              {pt.name}
                              <X className="ml-1 h-3 w-3" />
                            </Badge>
                          ) : null;
                        })}
                      </div>
                    )}
                    <FormDescription>
                      Products of any of these types will be included
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="minPrice"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Minimum Price</FormLabel>
                    <FormControl>
                      <Input
                        type="number"
                        min="0"
                        step="0.01"
                        placeholder="0.00"
                        value={field.value ?? ""}
                        onChange={(e) => {
                          const value = e.target.value;
                          if (value === "" || value === null || value === undefined) {
                            field.onChange(undefined);
                          } else {
                            const parsed = parseFloat(value);
                            field.onChange(isNaN(parsed) ? undefined : parsed);
                          }
                        }}
                        onBlur={field.onBlur}
                        name={field.name}
                        ref={field.ref}
                      />
                    </FormControl>
                    <FormDescription>
                      Products with list price above this amount
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="maxPrice"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Maximum Price</FormLabel>
                    <FormControl>
                      <Input
                        type="number"
                        min="0"
                        step="0.01"
                        placeholder="0.00"
                        value={field.value ?? ""}
                        onChange={(e) => {
                          const value = e.target.value;
                          if (value === "" || value === null || value === undefined) {
                            field.onChange(undefined);
                          } else {
                            const parsed = parseFloat(value);
                            field.onChange(isNaN(parsed) ? undefined : parsed);
                          }
                        }}
                        onBlur={field.onBlur}
                        name={field.name}
                        ref={field.ref}
                      />
                    </FormControl>
                    <FormDescription>
                      Products with list price below this amount
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            {/* Save button (always visible at bottom) */}
            <div className="flex justify-end gap-4 pt-4 border-t">
              <Button
                type="button"
                variant="outline"
                onClick={handleReset}
                disabled={!hasChanges}
              >
                Reset
              </Button>
              <Button type="submit" disabled={setFilters.isPending || !hasChanges}>
                {setFilters.isPending && (
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                )}
                Save Filters
              </Button>
            </div>
          </form>
        </Form>
      </CardContent>
    </Card>
  );
}
