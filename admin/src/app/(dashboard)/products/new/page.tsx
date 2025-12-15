"use client";

import { useRouter } from "next/navigation";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { ProductForm } from "@/components/forms/product-form";
import { useCreateProduct } from "@/hooks/use-products";
import { ProductFormData } from "@/lib/validations/product";

export default function NewProductPage() {
  const router = useRouter();
  const createProduct = useCreateProduct();

  const handleSubmit = async (data: ProductFormData) => {
    await createProduct.mutateAsync(data);
    router.push("/products");
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Add Product</h1>
        <p className="text-muted-foreground">
          Create a new product in your catalog
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Product Information</CardTitle>
        </CardHeader>
        <CardContent>
          <ProductForm
            onSubmit={handleSubmit}
            onCancel={() => router.push("/products")}
            isLoading={createProduct.isPending}
          />
        </CardContent>
      </Card>
    </div>
  );
}
