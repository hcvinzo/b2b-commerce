"use client";

import { useQuery } from "@tanstack/react-query";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { cn, formatCurrency } from "@/lib/utils";
import { getTopProducts } from "@/lib/api/dashboard";

interface TopProductsProps {
  className?: string;
}

export function TopProducts({ className }: TopProductsProps) {
  const { data: products, isLoading } = useQuery({
    queryKey: ["top-products"],
    queryFn: getTopProducts,
  });

  return (
    <Card className={cn(className)}>
      <CardHeader>
        <CardTitle>Top Products</CardTitle>
        <CardDescription>Best selling products this month</CardDescription>
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <div className="space-y-4">
            {[...Array(5)].map((_, i) => (
              <div key={i} className="h-12 animate-pulse bg-muted rounded" />
            ))}
          </div>
        ) : (
          <div className="space-y-4">
            {products?.map((product, index) => (
              <div key={product.id} className="flex items-center">
                <div className="flex h-8 w-8 items-center justify-center rounded-full bg-muted text-sm font-medium">
                  {index + 1}
                </div>
                <div className="ml-4 flex-1 space-y-1">
                  <p className="text-sm font-medium leading-none">
                    {product.name}
                  </p>
                  <p className="text-xs text-muted-foreground">{product.sku}</p>
                </div>
                <div className="text-right">
                  <p className="text-sm font-medium">
                    {formatCurrency(product.totalSales)}
                  </p>
                  <p className="text-xs text-muted-foreground">
                    {product.quantitySold} sold
                  </p>
                </div>
              </div>
            ))}
          </div>
        )}
      </CardContent>
    </Card>
  );
}
