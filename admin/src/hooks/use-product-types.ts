import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  getProductTypes,
  getProductType,
  createProductType,
  updateProductType,
  deleteProductType,
  addAttributeToProductType,
  removeAttributeFromProductType,
} from "@/lib/api/product-types";
import {
  CreateProductTypeDto,
  UpdateProductTypeDto,
  AddAttributeToProductTypeDto,
} from "@/types/entities";

// Query keys for cache management
export const productTypeKeys = {
  all: ["productTypes"] as const,
  lists: () => [...productTypeKeys.all, "list"] as const,
  list: (isActive?: boolean) => [...productTypeKeys.lists(), { isActive }] as const,
  details: () => [...productTypeKeys.all, "detail"] as const,
  detail: (id: string) => [...productTypeKeys.details(), id] as const,
};

// Query: Get all product types (list view)
export function useProductTypes(isActive?: boolean) {
  return useQuery({
    queryKey: productTypeKeys.list(isActive),
    queryFn: () => getProductTypes(isActive),
  });
}

// Query: Get single product type by ID (with attributes)
export function useProductType(id: string | null) {
  return useQuery({
    queryKey: productTypeKeys.detail(id ?? ""),
    queryFn: () => getProductType(id!),
    enabled: !!id,
  });
}

// Mutation: Create product type
export function useCreateProductType() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateProductTypeDto) => createProductType(data),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: productTypeKeys.all });
      toast.success("Product type created", {
        description: `"${data.name}" has been created successfully.`,
      });
    },
    onError: (error: Error) => {
      toast.error("Failed to create product type", {
        description: error.message,
      });
    },
  });
}

// Mutation: Update product type
export function useUpdateProductType() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateProductTypeDto }) =>
      updateProductType(id, data),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: productTypeKeys.all });
      toast.success("Product type updated", {
        description: `"${data.name}" has been updated successfully.`,
      });
    },
    onError: (error: Error) => {
      toast.error("Failed to update product type", {
        description: error.message,
      });
    },
  });
}

// Mutation: Delete product type
export function useDeleteProductType() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteProductType(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: productTypeKeys.all });
      toast.success("Product type deleted", {
        description: "The product type has been deleted successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Failed to delete product type", {
        description: error.message,
      });
    },
  });
}

// Mutation: Add attribute to product type
export function useAddAttributeToProductType() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      productTypeId,
      data,
    }: {
      productTypeId: string;
      data: AddAttributeToProductTypeDto;
    }) => addAttributeToProductType(productTypeId, data),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: productTypeKeys.all });
      toast.success("Attribute added", {
        description: `Attribute has been added to "${data.name}".`,
      });
    },
    onError: (error: Error) => {
      toast.error("Failed to add attribute", {
        description: error.message,
      });
    },
  });
}

// Mutation: Remove attribute from product type
export function useRemoveAttributeFromProductType() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      productTypeId,
      attributeDefinitionId,
    }: {
      productTypeId: string;
      attributeDefinitionId: string;
    }) => removeAttributeFromProductType(productTypeId, attributeDefinitionId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: productTypeKeys.all });
      toast.success("Attribute removed", {
        description: "The attribute has been removed successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Failed to remove attribute", {
        description: error.message,
      });
    },
  });
}
