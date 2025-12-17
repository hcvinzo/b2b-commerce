"use client";

import { useParams, useRouter, useSearchParams } from "next/navigation";
import Link from "next/link";
import { ArrowLeft, Pencil } from "lucide-react";

import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { Separator } from "@/components/ui/separator";
import { ProductForm } from "@/components/forms/product-form";
import { useProduct, useUpdateProduct } from "@/hooks/use-products";
import { formatCurrency, formatDateTime } from "@/lib/utils";
import { ProductFormData } from "@/lib/validations/product";

export default function ProductDetailPage() {
  const params = useParams();
  const router = useRouter();
  const searchParams = useSearchParams();
  const id = params.id as string;
  const isEditing = searchParams.get("edit") === "true";

  const { data: product, isLoading } = useProduct(id);
  const updateProduct = useUpdateProduct();

  const handleSubmit = async (data: ProductFormData) => {
    // Map form data to API DTO (listPriceCurrency -> currency)
    const apiData = {
      ...data,
      currency: data.listPriceCurrency || "TRY",
    };
    await updateProduct.mutateAsync({ id, data: apiData });
    router.push(`/products/${id}`);
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-10 w-48" />
        <Skeleton className="h-[400px]" />
      </div>
    );
  }

  if (!product) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" asChild>
            <Link href="/products">
              <ArrowLeft className="h-4 w-4" />
            </Link>
          </Button>
          <h1 className="text-3xl font-bold tracking-tight">Product not found</h1>
        </div>
      </div>
    );
  }

  if (isEditing) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" asChild>
            <Link href={`/products/${id}`}>
              <ArrowLeft className="h-4 w-4" />
            </Link>
          </Button>
          <div>
            <h1 className="text-3xl font-bold tracking-tight">Edit Product</h1>
            <p className="text-muted-foreground">Update product information</p>
          </div>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>Product Information</CardTitle>
          </CardHeader>
          <CardContent>
            <ProductForm
              defaultValues={{
                sku: product.sku,
                name: product.name,
                nameEn: product.nameEn,
                description: product.description,
                categoryId: product.categoryId,
                brandId: product.brandId,
                productTypeId: product.productTypeId,
                listPrice: product.listPrice,
                listPriceCurrency: product.listPriceCurrency,
                dealerPrice: product.dealerPrice,
                stockQuantity: product.stockQuantity,
                minOrderQuantity: product.minOrderQuantity,
                unitOfMeasure: product.unitOfMeasure,
                isActive: product.isActive,
                isFeatured: product.isFeatured,
              }}
              onSubmit={handleSubmit}
              onCancel={() => router.push(`/products/${id}`)}
              isLoading={updateProduct.isPending}
            />
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" asChild>
            <Link href="/products">
              <ArrowLeft className="h-4 w-4" />
            </Link>
          </Button>
          <div>
            <h1 className="text-3xl font-bold tracking-tight">{product.name}</h1>
            <p className="text-muted-foreground">SKU: {product.sku}</p>
          </div>
        </div>
        <Button asChild>
          <Link href={`/products/${id}?edit=true`}>
            <Pencil className="mr-2 h-4 w-4" />
            Edit Product
          </Link>
        </Button>
      </div>

      <div className="grid gap-6 md:grid-cols-3">
        {/* Main Info */}
        <Card className="md:col-span-2">
          <CardHeader>
            <CardTitle>Product Details</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid gap-4 md:grid-cols-2">
              <div>
                <p className="text-sm text-muted-foreground">Name</p>
                <p className="font-medium">{product.name}</p>
              </div>
              {product.nameEn && (
                <div>
                  <p className="text-sm text-muted-foreground">Name (English)</p>
                  <p className="font-medium">{product.nameEn}</p>
                </div>
              )}
              <div>
                <p className="text-sm text-muted-foreground">Category</p>
                <p className="font-medium">{product.categoryName}</p>
              </div>
              {product.brandName && (
                <div>
                  <p className="text-sm text-muted-foreground">Brand</p>
                  <p className="font-medium">{product.brandName}</p>
                </div>
              )}
              <div>
                <p className="text-sm text-muted-foreground">Status</p>
                <Badge variant={product.isActive ? "default" : "secondary"}>
                  {product.isActive ? "Active" : "Inactive"}
                </Badge>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Featured</p>
                <Badge variant={product.isFeatured ? "default" : "outline"}>
                  {product.isFeatured ? "Yes" : "No"}
                </Badge>
              </div>
            </div>

            {product.description && (
              <>
                <Separator />
                <div>
                  <p className="text-sm text-muted-foreground mb-2">Description</p>
                  <p className="text-sm">{product.description}</p>
                </div>
              </>
            )}
          </CardContent>
        </Card>

        {/* Pricing & Stock */}
        <div className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Pricing</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <p className="text-sm text-muted-foreground">List Price</p>
                <p className="text-2xl font-bold">
                  {formatCurrency(product.listPrice)}
                </p>
              </div>
              {product.dealerPrice && (
                <div>
                  <p className="text-sm text-muted-foreground">Dealer Price</p>
                  <p className="text-xl font-semibold">
                    {formatCurrency(product.dealerPrice)}
                  </p>
                </div>
              )}
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Inventory</CardTitle>
            </CardHeader>
            <CardContent className="space-y-4">
              <div>
                <p className="text-sm text-muted-foreground">Stock Quantity</p>
                <p className="text-2xl font-bold">{product.stockQuantity}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Min Order Qty</p>
                <p className="font-medium">{product.minOrderQuantity}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Unit</p>
                <p className="font-medium">{product.unitOfMeasure}</p>
              </div>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Metadata</CardTitle>
            </CardHeader>
            <CardContent className="space-y-2 text-sm">
              <div className="flex justify-between">
                <span className="text-muted-foreground">Created</span>
                <span>{formatDateTime(product.createdAt)}</span>
              </div>
              {product.updatedAt && (
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Updated</span>
                  <span>{formatDateTime(product.updatedAt)}</span>
                </div>
              )}
              {product.externalId && (
                <div className="flex justify-between">
                  <span className="text-muted-foreground">External ID</span>
                  <span className="font-mono text-xs">{product.externalId}</span>
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
}
