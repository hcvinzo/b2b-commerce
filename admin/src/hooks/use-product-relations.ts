import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  getProductRelations,
  setProductRelations,
  searchProductsForSelection,
} from "@/lib/api/products";
import {
  ProductRelationType,
  RelatedProductInput,
} from "@/types/entities";
import { productKeys } from "./use-products";

export const productRelationKeys = {
  all: ["product-relations"] as const,
  relations: (productId: string) =>
    [...productRelationKeys.all, productId] as const,
  search: (query: string, excludeId?: string) =>
    [...productRelationKeys.all, "search", query, excludeId] as const,
};

export function useProductRelations(productId: string) {
  return useQuery({
    queryKey: productRelationKeys.relations(productId),
    queryFn: () => getProductRelations(productId),
    enabled: !!productId,
  });
}

export function useSetProductRelations() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      productId,
      relationType,
      relatedProducts,
    }: {
      productId: string;
      relationType: ProductRelationType;
      relatedProducts: RelatedProductInput[];
    }) => setProductRelations(productId, relationType, relatedProducts),
    onSuccess: (_, { productId }) => {
      queryClient.invalidateQueries({
        queryKey: productRelationKeys.relations(productId),
      });
      // Also invalidate product details as relations might affect product view
      queryClient.invalidateQueries({
        queryKey: productKeys.detail(productId),
      });
      toast.success("Relations updated", {
        description: "Product relations have been updated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to update product relations.",
      });
    },
  });
}

export function useSearchProductsForSelection(
  query: string,
  excludeId?: string
) {
  return useQuery({
    queryKey: productRelationKeys.search(query, excludeId),
    queryFn: () => searchProductsForSelection(query, excludeId),
    enabled: query.length >= 2,
    staleTime: 30000, // Cache search results for 30 seconds
  });
}
