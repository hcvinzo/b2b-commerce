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
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Separator } from "@/components/ui/separator";
import { productSchema, type ProductFormData } from "@/lib/validations/product";
import { useCategoriesFlat } from "@/hooks/use-categories";
import { useBrands } from "@/hooks/use-brands";
import { useProductTypes } from "@/hooks/use-product-types";
import { ProductImageManager } from "./product-image-manager";
import { ProductAttributesForm } from "./product-attributes-form";
import { ProductAttributeValueInput } from "@/types/entities";

interface ProductFormProps {
  defaultValues?: Partial<ProductFormData>;
  onSubmit: (data: ProductFormData) => Promise<void>;
  onCancel?: () => void;
  isLoading?: boolean;
}

export function ProductForm({
  defaultValues,
  onSubmit,
  onCancel,
  isLoading,
}: ProductFormProps) {
  const { data: categoriesData } = useCategoriesFlat();
  const { data: brandsData } = useBrands({ isActive: true });
  const { data: productTypesData } = useProductTypes(true);

  const categories = categoriesData?.items || [];
  const brands = brandsData?.items || [];
  const productTypes = productTypesData || [];

  const form = useForm<ProductFormData>({
    resolver: zodResolver(productSchema),
    defaultValues: {
      sku: "",
      name: "",
      nameEn: "",
      description: "",
      categoryId: "",
      brandId: "",
      productTypeId: "",
      listPrice: 0,
      listPriceCurrency: "TRY",
      dealerPrice: 0,
      tier1Price: undefined,
      tier2Price: undefined,
      tier3Price: undefined,
      tier4Price: undefined,
      tier5Price: undefined,
      stockQuantity: 0,
      minOrderQuantity: 1,
      unitOfMeasure: "ADET",
      isSerialTracked: false,
      taxRate: 0.20,
      mainImageUrl: "",
      imageUrls: [],
      weight: undefined,
      length: undefined,
      width: undefined,
      height: undefined,
      isActive: true,
      isFeatured: false,
      attributeValues: [],
      ...defaultValues,
    },
  });

  const watchProductTypeId = form.watch("productTypeId");

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <Tabs defaultValue="basic" className="w-full">
          <TabsList className="grid w-full grid-cols-6">
            <TabsTrigger value="basic">Basic Info</TabsTrigger>
            <TabsTrigger value="pricing">Pricing</TabsTrigger>
            <TabsTrigger value="inventory">Inventory</TabsTrigger>
            <TabsTrigger value="images">Images</TabsTrigger>
            <TabsTrigger value="attributes">Attributes</TabsTrigger>
            <TabsTrigger value="dimensions">Dimensions</TabsTrigger>
          </TabsList>

          {/* Basic Info Tab */}
          <TabsContent value="basic" className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle>Basic Information</CardTitle>
                <CardDescription>
                  Enter the product's basic details
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="grid gap-6 md:grid-cols-2">
                  <FormField
                    control={form.control}
                    name="sku"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>SKU *</FormLabel>
                        <FormControl>
                          <Input placeholder="Enter product SKU" {...field} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control}
                    name="categoryId"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Category *</FormLabel>
                        <Select onValueChange={field.onChange} value={field.value}>
                          <FormControl>
                            <SelectTrigger>
                              <SelectValue placeholder="Select a category" />
                            </SelectTrigger>
                          </FormControl>
                          <SelectContent>
                            {categories.map((category) => (
                              <SelectItem key={category.id} value={category.id}>
                                {category.name}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control}
                    name="name"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Product Name *</FormLabel>
                        <FormControl>
                          <Input placeholder="Enter product name" {...field} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control}
                    name="nameEn"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Product Name (English)</FormLabel>
                        <FormControl>
                          <Input placeholder="Enter product name in English" {...field} />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control}
                    name="brandId"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Brand</FormLabel>
                        <Select
                          onValueChange={(value) => field.onChange(value === "__none__" ? "" : value)}
                          value={field.value || "__none__"}
                        >
                          <FormControl>
                            <SelectTrigger>
                              <SelectValue placeholder="Select a brand" />
                            </SelectTrigger>
                          </FormControl>
                          <SelectContent>
                            <SelectItem value="__none__">No brand</SelectItem>
                            {brands.map((brand) => (
                              <SelectItem key={brand.id} value={brand.id}>
                                {brand.name}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control}
                    name="productTypeId"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Product Type</FormLabel>
                        <Select
                          onValueChange={(value) => field.onChange(value === "__none__" ? "" : value)}
                          value={field.value || "__none__"}
                        >
                          <FormControl>
                            <SelectTrigger>
                              <SelectValue placeholder="Select a product type" />
                            </SelectTrigger>
                          </FormControl>
                          <SelectContent>
                            <SelectItem value="__none__">No product type</SelectItem>
                            {productTypes.map((pt) => (
                              <SelectItem key={pt.id} value={pt.id}>
                                {pt.name}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                        <FormDescription>
                          Select a product type to enable attribute configuration
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
                          placeholder="Enter product description"
                          className="min-h-[100px]"
                          {...field}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <Separator />

                <div className="grid gap-6 md:grid-cols-2">
                  <FormField
                    control={form.control}
                    name="isActive"
                    render={({ field }) => (
                      <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                        <div className="space-y-0.5">
                          <FormLabel className="text-base">Active</FormLabel>
                          <FormDescription>
                            Make this product visible in the catalog
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
                    name="isFeatured"
                    render={({ field }) => (
                      <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                        <div className="space-y-0.5">
                          <FormLabel className="text-base">Featured</FormLabel>
                          <FormDescription>
                            Show this product in featured sections
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
              </CardContent>
            </Card>
          </TabsContent>

          {/* Pricing Tab */}
          <TabsContent value="pricing" className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle>Pricing</CardTitle>
                <CardDescription>
                  Configure product pricing and tax settings
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="grid gap-6 md:grid-cols-3">
                  <FormField
                    control={form.control}
                    name="listPrice"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>List Price *</FormLabel>
                        <FormControl>
                          <Input
                            type="number"
                            step="0.01"
                            placeholder="0.00"
                            {...field}
                            onChange={(e) => field.onChange(parseFloat(e.target.value) || 0)}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control}
                    name="listPriceCurrency"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Currency</FormLabel>
                        <Select onValueChange={field.onChange} value={field.value || "TRY"}>
                          <FormControl>
                            <SelectTrigger>
                              <SelectValue placeholder="Select currency" />
                            </SelectTrigger>
                          </FormControl>
                          <SelectContent>
                            <SelectItem value="TRY">TRY</SelectItem>
                            <SelectItem value="USD">USD</SelectItem>
                            <SelectItem value="EUR">EUR</SelectItem>
                          </SelectContent>
                        </Select>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control}
                    name="taxRate"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Tax Rate</FormLabel>
                        <FormControl>
                          <Input
                            type="number"
                            step="0.01"
                            placeholder="0.20"
                            {...field}
                            value={field.value ?? ""}
                            onChange={(e) => field.onChange(parseFloat(e.target.value) || undefined)}
                          />
                        </FormControl>
                        <FormDescription>
                          e.g., 0.20 for 20% VAT
                        </FormDescription>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>

                <Separator />

                <div>
                  <h4 className="text-sm font-medium mb-4">Tier Pricing (Optional)</h4>
                  <div className="grid gap-6 md:grid-cols-3">
                    <FormField
                      control={form.control}
                      name="dealerPrice"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Dealer Price</FormLabel>
                          <FormControl>
                            <Input
                              type="number"
                              step="0.01"
                              placeholder="0.00"
                              {...field}
                              value={field.value ?? ""}
                              onChange={(e) => field.onChange(parseFloat(e.target.value) || undefined)}
                            />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="tier1Price"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Tier 1 Price</FormLabel>
                          <FormControl>
                            <Input
                              type="number"
                              step="0.01"
                              placeholder="0.00"
                              {...field}
                              value={field.value ?? ""}
                              onChange={(e) => field.onChange(parseFloat(e.target.value) || undefined)}
                            />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="tier2Price"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Tier 2 Price</FormLabel>
                          <FormControl>
                            <Input
                              type="number"
                              step="0.01"
                              placeholder="0.00"
                              {...field}
                              value={field.value ?? ""}
                              onChange={(e) => field.onChange(parseFloat(e.target.value) || undefined)}
                            />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="tier3Price"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Tier 3 Price</FormLabel>
                          <FormControl>
                            <Input
                              type="number"
                              step="0.01"
                              placeholder="0.00"
                              {...field}
                              value={field.value ?? ""}
                              onChange={(e) => field.onChange(parseFloat(e.target.value) || undefined)}
                            />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="tier4Price"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Tier 4 Price</FormLabel>
                          <FormControl>
                            <Input
                              type="number"
                              step="0.01"
                              placeholder="0.00"
                              {...field}
                              value={field.value ?? ""}
                              onChange={(e) => field.onChange(parseFloat(e.target.value) || undefined)}
                            />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="tier5Price"
                      render={({ field }) => (
                        <FormItem>
                          <FormLabel>Tier 5 Price</FormLabel>
                          <FormControl>
                            <Input
                              type="number"
                              step="0.01"
                              placeholder="0.00"
                              {...field}
                              value={field.value ?? ""}
                              onChange={(e) => field.onChange(parseFloat(e.target.value) || undefined)}
                            />
                          </FormControl>
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                  </div>
                </div>
              </CardContent>
            </Card>
          </TabsContent>

          {/* Inventory Tab */}
          <TabsContent value="inventory" className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle>Inventory</CardTitle>
                <CardDescription>
                  Manage stock and ordering settings
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-6">
                <div className="grid gap-6 md:grid-cols-2">
                  <FormField
                    control={form.control}
                    name="stockQuantity"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Stock Quantity</FormLabel>
                        <FormControl>
                          <Input
                            type="number"
                            placeholder="0"
                            {...field}
                            onChange={(e) => field.onChange(parseInt(e.target.value) || 0)}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control}
                    name="minOrderQuantity"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Minimum Order Quantity</FormLabel>
                        <FormControl>
                          <Input
                            type="number"
                            placeholder="1"
                            {...field}
                            onChange={(e) => field.onChange(parseInt(e.target.value) || 1)}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control}
                    name="maxOrderQuantity"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Maximum Order Quantity</FormLabel>
                        <FormControl>
                          <Input
                            type="number"
                            placeholder="No limit"
                            {...field}
                            value={field.value ?? ""}
                            onChange={(e) => field.onChange(parseInt(e.target.value) || undefined)}
                          />
                        </FormControl>
                        <FormDescription>
                          Leave empty for no maximum limit
                        </FormDescription>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control}
                    name="unitOfMeasure"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Unit of Measure</FormLabel>
                        <Select onValueChange={field.onChange} value={field.value || "ADET"}>
                          <FormControl>
                            <SelectTrigger>
                              <SelectValue placeholder="Select unit" />
                            </SelectTrigger>
                          </FormControl>
                          <SelectContent>
                            <SelectItem value="ADET">Piece (ADET)</SelectItem>
                            <SelectItem value="KG">Kilogram (KG)</SelectItem>
                            <SelectItem value="MT">Meter (MT)</SelectItem>
                            <SelectItem value="LT">Liter (LT)</SelectItem>
                            <SelectItem value="M2">Square Meter (M2)</SelectItem>
                            <SelectItem value="KUTU">Box (KUTU)</SelectItem>
                            <SelectItem value="PAKET">Package (PAKET)</SelectItem>
                          </SelectContent>
                        </Select>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>

                <Separator />

                <FormField
                  control={form.control}
                  name="isSerialTracked"
                  render={({ field }) => (
                    <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
                      <div className="space-y-0.5">
                        <FormLabel className="text-base">Serial Tracking</FormLabel>
                        <FormDescription>
                          Enable serial number tracking for this product
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
              </CardContent>
            </Card>
          </TabsContent>

          {/* Images Tab */}
          <TabsContent value="images" className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle>Product Images</CardTitle>
                <CardDescription>
                  Add images by entering their URLs. The first image will be set as the main image.
                </CardDescription>
              </CardHeader>
              <CardContent>
                <FormField
                  control={form.control}
                  name="mainImageUrl"
                  render={({ field }) => (
                    <FormField
                      control={form.control}
                      name="imageUrls"
                      render={({ field: imageUrlsField }) => (
                        <ProductImageManager
                          mainImageUrl={field.value || undefined}
                          imageUrls={imageUrlsField.value || []}
                          onMainImageChange={(url) => field.onChange(url || "")}
                          onImageUrlsChange={imageUrlsField.onChange}
                        />
                      )}
                    />
                  )}
                />
              </CardContent>
            </Card>
          </TabsContent>

          {/* Attributes Tab */}
          <TabsContent value="attributes" className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle>Product Attributes</CardTitle>
                <CardDescription>
                  {watchProductTypeId
                    ? "Configure attribute values for this product"
                    : "Select a product type in the Basic Info tab to configure attributes"}
                </CardDescription>
              </CardHeader>
              <CardContent>
                <FormField
                  control={form.control}
                  name="attributeValues"
                  render={({ field }) => (
                    <ProductAttributesForm
                      productTypeId={watchProductTypeId || undefined}
                      values={(field.value || []) as ProductAttributeValueInput[]}
                      onChange={field.onChange}
                    />
                  )}
                />
              </CardContent>
            </Card>
          </TabsContent>

          {/* Dimensions Tab */}
          <TabsContent value="dimensions" className="space-y-6">
            <Card>
              <CardHeader>
                <CardTitle>Dimensions & Weight</CardTitle>
                <CardDescription>
                  Enter product physical dimensions for shipping calculations
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="grid gap-6 md:grid-cols-2">
                  <FormField
                    control={form.control}
                    name="weight"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Weight (kg)</FormLabel>
                        <FormControl>
                          <Input
                            type="number"
                            step="0.01"
                            placeholder="0.00"
                            {...field}
                            value={field.value ?? ""}
                            onChange={(e) => field.onChange(parseFloat(e.target.value) || undefined)}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control}
                    name="length"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Length (cm)</FormLabel>
                        <FormControl>
                          <Input
                            type="number"
                            step="0.01"
                            placeholder="0.00"
                            {...field}
                            value={field.value ?? ""}
                            onChange={(e) => field.onChange(parseFloat(e.target.value) || undefined)}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control}
                    name="width"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Width (cm)</FormLabel>
                        <FormControl>
                          <Input
                            type="number"
                            step="0.01"
                            placeholder="0.00"
                            {...field}
                            value={field.value ?? ""}
                            onChange={(e) => field.onChange(parseFloat(e.target.value) || undefined)}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />

                  <FormField
                    control={form.control}
                    name="height"
                    render={({ field }) => (
                      <FormItem>
                        <FormLabel>Height (cm)</FormLabel>
                        <FormControl>
                          <Input
                            type="number"
                            step="0.01"
                            placeholder="0.00"
                            {...field}
                            value={field.value ?? ""}
                            onChange={(e) => field.onChange(parseFloat(e.target.value) || undefined)}
                          />
                        </FormControl>
                        <FormMessage />
                      </FormItem>
                    )}
                  />
                </div>
              </CardContent>
            </Card>
          </TabsContent>
        </Tabs>

        <div className="flex justify-end gap-4">
          {onCancel && (
            <Button type="button" variant="outline" onClick={onCancel}>
              Cancel
            </Button>
          )}
          <Button type="submit" disabled={isLoading}>
            {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Save Product
          </Button>
        </div>
      </form>
    </Form>
  );
}
