import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  getParameters,
  getParameter,
  getParameterByKey,
  getParameterCategories,
  createParameter,
  updateParameter,
  deleteParameter,
} from "@/lib/api/parameters";
import {
  ParameterFilters,
  CreateParameterDto,
  UpdateParameterDto,
} from "@/types/entities";

export const parameterKeys = {
  all: ["parameters"] as const,
  lists: () => [...parameterKeys.all, "list"] as const,
  list: (filters: ParameterFilters) => [...parameterKeys.lists(), filters] as const,
  details: () => [...parameterKeys.all, "detail"] as const,
  detail: (id: string) => [...parameterKeys.details(), id] as const,
  byKey: (key: string) => [...parameterKeys.all, "byKey", key] as const,
  categories: () => [...parameterKeys.all, "categories"] as const,
};

export function useParameters(filters: ParameterFilters = {}) {
  return useQuery({
    queryKey: parameterKeys.list(filters),
    queryFn: () => getParameters(filters),
  });
}

export function useParameter(id: string) {
  return useQuery({
    queryKey: parameterKeys.detail(id),
    queryFn: () => getParameter(id),
    enabled: !!id,
  });
}

export function useParameterByKey(key: string) {
  return useQuery({
    queryKey: parameterKeys.byKey(key),
    queryFn: () => getParameterByKey(key),
    enabled: !!key,
  });
}

export function useParameterCategories() {
  return useQuery({
    queryKey: parameterKeys.categories(),
    queryFn: () => getParameterCategories(),
  });
}

export function useCreateParameter() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateParameterDto) => createParameter(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: parameterKeys.all });
      toast.success("Parameter created", {
        description: "The parameter has been created successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to create parameter.",
      });
    },
  });
}

export function useUpdateParameter() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateParameterDto }) =>
      updateParameter(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: parameterKeys.all });
      queryClient.invalidateQueries({ queryKey: parameterKeys.detail(id) });
      toast.success("Parameter updated", {
        description: "The parameter has been updated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to update parameter.",
      });
    },
  });
}

export function useDeleteParameter() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteParameter(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: parameterKeys.all });
      toast.success("Parameter deleted", {
        description: "The parameter has been deleted successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to delete parameter.",
      });
    },
  });
}
