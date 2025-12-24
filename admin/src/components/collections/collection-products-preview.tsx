"use client";

import { Loader2 } from "lucide-react";

import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { useCollectionProducts } from "@/hooks/use-collections";
import { Collection } from "@/types/entities";
import { formatCurrency } from "@/lib/utils";

interface CollectionProductsPreviewProps {
  collection: Collection;
}

export function CollectionProductsPreview({
  collection,
}: CollectionProductsPreviewProps) {
  const { data: productsData, isLoading } = useCollectionProducts(
    collection.id,
    1,
    50
  );

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-8">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  const products = productsData?.items || [];

  if (products.length === 0) {
    return (
      <div className="text-center py-8 text-muted-foreground">
        No products match the current filter criteria.
        <br />
        <span className="text-sm">
          Go to the &quot;Filters&quot; tab to configure the filter criteria.
        </span>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <p className="text-sm text-muted-foreground">
        Showing {products.length} of {productsData?.totalCount || 0} matching
        products
      </p>

      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead className="w-[80px]">Image</TableHead>
              <TableHead>Product</TableHead>
              <TableHead>SKU</TableHead>
              <TableHead className="text-right">Price</TableHead>
              <TableHead className="text-center">Status</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {products.map((product) => (
              <TableRow key={product.productId}>
                <TableCell>
                  {product.productImageUrl ? (
                    <img
                      src={product.productImageUrl}
                      alt={product.productName}
                      className="h-10 w-10 rounded object-cover"
                    />
                  ) : (
                    <div className="h-10 w-10 rounded bg-muted" />
                  )}
                </TableCell>
                <TableCell className="font-medium">
                  {product.productName}
                </TableCell>
                <TableCell className="text-muted-foreground">
                  {product.productSku}
                </TableCell>
                <TableCell className="text-right">
                  {formatCurrency(product.productPrice)}
                </TableCell>
                <TableCell className="text-center">
                  <Badge
                    variant={product.productIsActive ? "default" : "secondary"}
                  >
                    {product.productIsActive ? "Active" : "Inactive"}
                  </Badge>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>
    </div>
  );
}
