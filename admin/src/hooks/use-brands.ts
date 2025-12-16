import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  getBrands,
  getBrand,
  createBrand,
  updateBrand,
  deleteBrand,
  activateBrand,
  deactivateBrand,
  type BrandFilters,
  type CreateBrandDto,
  type UpdateBrandDto,
} from "@/lib/api/brands";

export const brandKeys = {
  all: ["brands"] as const,
  lists: () => [...brandKeys.all, "list"] as const,
  list: (filters: BrandFilters) => [...brandKeys.lists(), filters] as const,
  details: () => [...brandKeys.all, "detail"] as const,
  detail: (id: string) => [...brandKeys.details(), id] as const,
};

export function useBrands(filters: BrandFilters = {}) {
  return useQuery({
    queryKey: brandKeys.list(filters),
    queryFn: () => getBrands(filters),
  });
}

export function useBrand(id: string) {
  return useQuery({
    queryKey: brandKeys.detail(id),
    queryFn: () => getBrand(id),
    enabled: !!id,
  });
}

export function useCreateBrand() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateBrandDto) => createBrand(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: brandKeys.all });
      toast.success("Brand created", {
        description: "The brand has been created successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to create brand.",
      });
    },
  });
}

export function useUpdateBrand() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateBrandDto }) =>
      updateBrand(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: brandKeys.all });
      queryClient.invalidateQueries({ queryKey: brandKeys.detail(id) });
      toast.success("Brand updated", {
        description: "The brand has been updated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to update brand.",
      });
    },
  });
}

export function useDeleteBrand() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteBrand(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: brandKeys.all });
      toast.success("Brand deleted", {
        description: "The brand has been deleted successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to delete brand.",
      });
    },
  });
}

export function useActivateBrand() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => activateBrand(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: brandKeys.all });
      queryClient.invalidateQueries({ queryKey: brandKeys.detail(id) });
      toast.success("Brand activated", {
        description: "The brand has been activated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to activate brand.",
      });
    },
  });
}

export function useDeactivateBrand() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deactivateBrand(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: brandKeys.all });
      queryClient.invalidateQueries({ queryKey: brandKeys.detail(id) });
      toast.success("Brand deactivated", {
        description: "The brand has been deactivated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to deactivate brand.",
      });
    },
  });
}
