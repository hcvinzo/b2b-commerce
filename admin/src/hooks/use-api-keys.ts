import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  createApiKey,
  revokeApiKey,
  rotateApiKey,
  getAvailableScopes,
  addIpWhitelist,
  removeIpWhitelist,
  getApiKeyDetail,
} from "@/lib/api/api-keys";
import { apiClientKeys } from "./use-api-clients";
import { CreateApiKeyDto, RevokeApiKeyDto, AddIpWhitelistDto } from "@/types/entities";

export const apiKeyKeys = {
  all: ["api-keys"] as const,
  scopes: () => [...apiKeyKeys.all, "scopes"] as const,
  detail: (id: string) => [...apiKeyKeys.all, "detail", id] as const,
};

export function useAvailableScopes() {
  return useQuery({
    queryKey: apiKeyKeys.scopes(),
    queryFn: getAvailableScopes,
    staleTime: 1000 * 60 * 60, // Cache for 1 hour (scopes rarely change)
  });
}

export function useApiKeyDetail(id: string, enabled = true) {
  return useQuery({
    queryKey: apiKeyKeys.detail(id),
    queryFn: () => getApiKeyDetail(id),
    enabled: enabled && !!id,
  });
}

export function useCreateApiKey() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateApiKeyDto) => createApiKey(data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({
        queryKey: apiClientKeys.detail(variables.apiClientId),
      });
      queryClient.invalidateQueries({ queryKey: apiClientKeys.lists() });
      // Note: Don't show toast here - we need to show the key dialog first
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to create API key.",
      });
    },
  });
}

export function useRevokeApiKey() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: RevokeApiKeyDto }) =>
      revokeApiKey(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: apiKeyKeys.all });
      queryClient.invalidateQueries({ queryKey: apiClientKeys.all });
      toast.success("API Key revoked", {
        description: "The API key has been revoked and can no longer be used.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to revoke API key.",
      });
    },
  });
}

export function useRotateApiKey() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => rotateApiKey(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: apiKeyKeys.all });
      queryClient.invalidateQueries({ queryKey: apiClientKeys.all });
      // Note: Don't show toast here - we need to show the new key dialog first
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to rotate API key.",
      });
    },
  });
}

export function useAddIpWhitelist() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ keyId, data }: { keyId: string; data: AddIpWhitelistDto }) =>
      addIpWhitelist(keyId, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: apiKeyKeys.all });
      queryClient.invalidateQueries({ queryKey: apiClientKeys.all });
      toast.success("IP added", {
        description: "IP address has been added to the whitelist.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to add IP address.",
      });
    },
  });
}

export function useRemoveIpWhitelist() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ keyId, whitelistId }: { keyId: string; whitelistId: string }) =>
      removeIpWhitelist(keyId, whitelistId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: apiKeyKeys.all });
      queryClient.invalidateQueries({ queryKey: apiClientKeys.all });
      toast.success("IP removed", {
        description: "IP address has been removed from the whitelist.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to remove IP address.",
      });
    },
  });
}
