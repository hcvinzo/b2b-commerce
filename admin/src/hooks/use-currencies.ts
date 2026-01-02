import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  getCurrencies,
  getCurrency,
  getCurrencyByCode,
  getDefaultCurrency,
  createCurrency,
  updateCurrency,
  deleteCurrency,
  activateCurrency,
  deactivateCurrency,
  setDefaultCurrency,
} from "@/lib/api/currencies";
import {
  CurrencyFilters,
  CreateCurrencyDto,
  UpdateCurrencyDto,
} from "@/types/entities";

export const currencyKeys = {
  all: ["currencies"] as const,
  lists: () => [...currencyKeys.all, "list"] as const,
  list: (filters: CurrencyFilters) => [...currencyKeys.lists(), filters] as const,
  details: () => [...currencyKeys.all, "detail"] as const,
  detail: (id: string) => [...currencyKeys.details(), id] as const,
  byCode: (code: string) => [...currencyKeys.all, "byCode", code] as const,
  default: () => [...currencyKeys.all, "default"] as const,
};

export function useCurrencies(filters: CurrencyFilters = {}) {
  return useQuery({
    queryKey: currencyKeys.list(filters),
    queryFn: () => getCurrencies(filters),
  });
}

export function useCurrency(id: string) {
  return useQuery({
    queryKey: currencyKeys.detail(id),
    queryFn: () => getCurrency(id),
    enabled: !!id,
  });
}

export function useCurrencyByCode(code: string) {
  return useQuery({
    queryKey: currencyKeys.byCode(code),
    queryFn: () => getCurrencyByCode(code),
    enabled: !!code,
  });
}

export function useDefaultCurrency() {
  return useQuery({
    queryKey: currencyKeys.default(),
    queryFn: () => getDefaultCurrency(),
  });
}

export function useCreateCurrency() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateCurrencyDto) => createCurrency(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: currencyKeys.all });
      toast.success("Currency created", {
        description: "The currency has been created successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to create currency.",
      });
    },
  });
}

export function useUpdateCurrency() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateCurrencyDto }) =>
      updateCurrency(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: currencyKeys.all });
      queryClient.invalidateQueries({ queryKey: currencyKeys.detail(id) });
      toast.success("Currency updated", {
        description: "The currency has been updated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to update currency.",
      });
    },
  });
}

export function useDeleteCurrency() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteCurrency(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: currencyKeys.all });
      toast.success("Currency deleted", {
        description: "The currency has been deleted successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to delete currency.",
      });
    },
  });
}

export function useActivateCurrency() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => activateCurrency(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: currencyKeys.all });
      toast.success("Currency activated", {
        description: "The currency has been activated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to activate currency.",
      });
    },
  });
}

export function useDeactivateCurrency() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deactivateCurrency(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: currencyKeys.all });
      toast.success("Currency deactivated", {
        description: "The currency has been deactivated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to deactivate currency.",
      });
    },
  });
}

export function useSetDefaultCurrency() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => setDefaultCurrency(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: currencyKeys.all });
      toast.success("Default currency set", {
        description: "The default currency has been set successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to set default currency.",
      });
    },
  });
}
