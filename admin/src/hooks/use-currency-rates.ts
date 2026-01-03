import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  getCurrencyRates,
  getCurrencyRate,
  createCurrencyRate,
  updateCurrencyRate,
  deleteCurrencyRate,
  activateCurrencyRate,
  deactivateCurrencyRate,
} from "@/lib/api/currency-rates";
import {
  CurrencyRateFilters,
  CreateCurrencyRateDto,
  UpdateCurrencyRateDto,
} from "@/types/entities";

export const currencyRateKeys = {
  all: ["currency-rates"] as const,
  lists: () => [...currencyRateKeys.all, "list"] as const,
  list: (filters: CurrencyRateFilters) => [...currencyRateKeys.lists(), filters] as const,
  details: () => [...currencyRateKeys.all, "detail"] as const,
  detail: (id: string) => [...currencyRateKeys.details(), id] as const,
};

export function useCurrencyRates(filters: CurrencyRateFilters = {}) {
  return useQuery({
    queryKey: currencyRateKeys.list(filters),
    queryFn: () => getCurrencyRates(filters),
  });
}

export function useCurrencyRate(id: string) {
  return useQuery({
    queryKey: currencyRateKeys.detail(id),
    queryFn: () => getCurrencyRate(id),
    enabled: !!id,
  });
}

export function useCreateCurrencyRate() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateCurrencyRateDto) => createCurrencyRate(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: currencyRateKeys.all });
      toast.success("Currency rate created", {
        description: "The exchange rate has been created successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to create exchange rate.",
      });
    },
  });
}

export function useUpdateCurrencyRate() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateCurrencyRateDto }) =>
      updateCurrencyRate(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: currencyRateKeys.all });
      queryClient.invalidateQueries({ queryKey: currencyRateKeys.detail(id) });
      toast.success("Currency rate updated", {
        description: "The exchange rate has been updated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to update exchange rate.",
      });
    },
  });
}

export function useDeleteCurrencyRate() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteCurrencyRate(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: currencyRateKeys.all });
      toast.success("Currency rate deleted", {
        description: "The exchange rate has been deleted successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to delete exchange rate.",
      });
    },
  });
}

export function useActivateCurrencyRate() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => activateCurrencyRate(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: currencyRateKeys.all });
      toast.success("Currency rate activated", {
        description: "The exchange rate has been activated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to activate exchange rate.",
      });
    },
  });
}

export function useDeactivateCurrencyRate() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deactivateCurrencyRate(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: currencyRateKeys.all });
      toast.success("Currency rate deactivated", {
        description: "The exchange rate has been deactivated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to deactivate exchange rate.",
      });
    },
  });
}
