import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  getApiClients,
  getApiClient,
  createApiClient,
  updateApiClient,
  activateApiClient,
  deactivateApiClient,
  deleteApiClient,
} from "@/lib/api/api-clients";
import {
  ApiClientFilters,
  CreateApiClientDto,
  UpdateApiClientDto,
} from "@/types/entities";

export const apiClientKeys = {
  all: ["api-clients"] as const,
  lists: () => [...apiClientKeys.all, "list"] as const,
  list: (filters: ApiClientFilters) =>
    [...apiClientKeys.lists(), filters] as const,
  details: () => [...apiClientKeys.all, "detail"] as const,
  detail: (id: string) => [...apiClientKeys.details(), id] as const,
};

export function useApiClients(filters: ApiClientFilters = {}) {
  return useQuery({
    queryKey: apiClientKeys.list(filters),
    queryFn: () => getApiClients(filters),
  });
}

export function useApiClient(id: string) {
  return useQuery({
    queryKey: apiClientKeys.detail(id),
    queryFn: () => getApiClient(id),
    enabled: !!id,
  });
}

export function useCreateApiClient() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateApiClientDto) => createApiClient(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: apiClientKeys.all });
      toast.success("API Client created", {
        description: "The API client has been created successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to create API client.",
      });
    },
  });
}

export function useUpdateApiClient() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateApiClientDto }) =>
      updateApiClient(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: apiClientKeys.all });
      queryClient.invalidateQueries({ queryKey: apiClientKeys.detail(id) });
      toast.success("API Client updated", {
        description: "The API client has been updated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to update API client.",
      });
    },
  });
}

export function useActivateApiClient() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => activateApiClient(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: apiClientKeys.all });
      queryClient.invalidateQueries({ queryKey: apiClientKeys.detail(id) });
      toast.success("API Client activated", {
        description: "The API client has been activated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to activate API client.",
      });
    },
  });
}

export function useDeactivateApiClient() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deactivateApiClient(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: apiClientKeys.all });
      queryClient.invalidateQueries({ queryKey: apiClientKeys.detail(id) });
      toast.success("API Client deactivated", {
        description:
          "The API client and all its keys have been deactivated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to deactivate API client.",
      });
    },
  });
}

export function useDeleteApiClient() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteApiClient(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: apiClientKeys.all });
      toast.success("API Client deleted", {
        description: "The API client has been deleted successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description:
          error.message ||
          "Failed to delete API client. Make sure all active keys are revoked first.",
      });
    },
  });
}
