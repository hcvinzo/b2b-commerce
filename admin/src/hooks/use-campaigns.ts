import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  getCampaigns,
  getCampaign,
  createCampaign,
  updateCampaign,
  deleteCampaign,
  scheduleCampaign,
  activateCampaign,
  pauseCampaign,
  cancelCampaign,
  getCampaignUsageStats,
  addDiscountRule,
  updateDiscountRule,
  removeDiscountRule,
  addProductsToRule,
  addCategoriesToRule,
  addBrandsToRule,
  addCustomersToRule,
  addCustomerTiersToRule,
} from "@/lib/api/campaigns";
import {
  CampaignFilters,
  CreateCampaignDto,
  UpdateCampaignDto,
  CreateDiscountRuleDto,
  UpdateDiscountRuleDto,
  PriceTier,
} from "@/types/entities";

export const campaignKeys = {
  all: ["campaigns"] as const,
  lists: () => [...campaignKeys.all, "list"] as const,
  list: (filters: CampaignFilters) => [...campaignKeys.lists(), filters] as const,
  details: () => [...campaignKeys.all, "detail"] as const,
  detail: (id: string) => [...campaignKeys.details(), id] as const,
  usageStats: (id: string) => [...campaignKeys.detail(id), "usage-stats"] as const,
};

// ============================================
// Campaign Queries
// ============================================

export function useCampaigns(filters: CampaignFilters) {
  return useQuery({
    queryKey: campaignKeys.list(filters),
    queryFn: () => getCampaigns(filters),
  });
}

export function useCampaign(id: string) {
  return useQuery({
    queryKey: campaignKeys.detail(id),
    queryFn: () => getCampaign(id),
    enabled: !!id,
  });
}

export function useCampaignUsageStats(id: string) {
  return useQuery({
    queryKey: campaignKeys.usageStats(id),
    queryFn: () => getCampaignUsageStats(id),
    enabled: !!id,
  });
}

// ============================================
// Campaign Mutations
// ============================================

export function useCreateCampaign() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateCampaignDto) => createCampaign(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: campaignKeys.lists() });
      toast.success("Campaign created", {
        description: "The campaign has been created successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to create campaign.",
      });
    },
  });
}

export function useUpdateCampaign() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateCampaignDto }) =>
      updateCampaign(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: campaignKeys.lists() });
      queryClient.invalidateQueries({ queryKey: campaignKeys.detail(id) });
      toast.success("Campaign updated", {
        description: "The campaign has been updated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to update campaign.",
      });
    },
  });
}

export function useDeleteCampaign() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteCampaign(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: campaignKeys.lists() });
      toast.success("Campaign deleted", {
        description: "The campaign has been deleted successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to delete campaign.",
      });
    },
  });
}

// ============================================
// Campaign Status Transitions
// ============================================

export function useScheduleCampaign() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => scheduleCampaign(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: campaignKeys.lists() });
      queryClient.invalidateQueries({ queryKey: campaignKeys.detail(id) });
      toast.success("Campaign scheduled", {
        description: "The campaign has been scheduled.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to schedule campaign.",
      });
    },
  });
}

export function useActivateCampaign() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => activateCampaign(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: campaignKeys.lists() });
      queryClient.invalidateQueries({ queryKey: campaignKeys.detail(id) });
      toast.success("Campaign activated", {
        description: "The campaign is now active.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to activate campaign.",
      });
    },
  });
}

export function usePauseCampaign() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => pauseCampaign(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: campaignKeys.lists() });
      queryClient.invalidateQueries({ queryKey: campaignKeys.detail(id) });
      toast.success("Campaign paused", {
        description: "The campaign has been paused.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to pause campaign.",
      });
    },
  });
}

export function useCancelCampaign() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => cancelCampaign(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: campaignKeys.lists() });
      queryClient.invalidateQueries({ queryKey: campaignKeys.detail(id) });
      toast.success("Campaign cancelled", {
        description: "The campaign has been cancelled.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to cancel campaign.",
      });
    },
  });
}

// ============================================
// Discount Rule Mutations
// ============================================

export function useAddDiscountRule() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      campaignId,
      data,
    }: {
      campaignId: string;
      data: CreateDiscountRuleDto;
    }) => addDiscountRule(campaignId, data),
    onSuccess: (_, { campaignId }) => {
      queryClient.invalidateQueries({ queryKey: campaignKeys.detail(campaignId) });
      toast.success("Discount rule added", {
        description: "The discount rule has been added successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to add discount rule.",
      });
    },
  });
}

export function useUpdateDiscountRule() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      campaignId,
      ruleId,
      data,
    }: {
      campaignId: string;
      ruleId: string;
      data: UpdateDiscountRuleDto;
    }) => updateDiscountRule(campaignId, ruleId, data),
    onSuccess: (_, { campaignId }) => {
      queryClient.invalidateQueries({ queryKey: campaignKeys.detail(campaignId) });
      toast.success("Discount rule updated", {
        description: "The discount rule has been updated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to update discount rule.",
      });
    },
  });
}

export function useRemoveDiscountRule() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ campaignId, ruleId }: { campaignId: string; ruleId: string }) =>
      removeDiscountRule(campaignId, ruleId),
    onSuccess: (_, { campaignId }) => {
      queryClient.invalidateQueries({ queryKey: campaignKeys.detail(campaignId) });
      toast.success("Discount rule removed", {
        description: "The discount rule has been removed successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to remove discount rule.",
      });
    },
  });
}

// ============================================
// Discount Rule Target Mutations
// ============================================

export function useAddProductsToRule() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      campaignId,
      ruleId,
      productIds,
    }: {
      campaignId: string;
      ruleId: string;
      productIds: string[];
    }) => addProductsToRule(campaignId, ruleId, productIds),
    onSuccess: (_, { campaignId }) => {
      queryClient.invalidateQueries({ queryKey: campaignKeys.detail(campaignId) });
      toast.success("Products added", {
        description: "Products have been added to the discount rule.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to add products.",
      });
    },
  });
}

export function useAddCategoriesToRule() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      campaignId,
      ruleId,
      categoryIds,
    }: {
      campaignId: string;
      ruleId: string;
      categoryIds: string[];
    }) => addCategoriesToRule(campaignId, ruleId, categoryIds),
    onSuccess: (_, { campaignId }) => {
      queryClient.invalidateQueries({ queryKey: campaignKeys.detail(campaignId) });
      toast.success("Categories added", {
        description: "Categories have been added to the discount rule.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to add categories.",
      });
    },
  });
}

export function useAddBrandsToRule() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      campaignId,
      ruleId,
      brandIds,
    }: {
      campaignId: string;
      ruleId: string;
      brandIds: string[];
    }) => addBrandsToRule(campaignId, ruleId, brandIds),
    onSuccess: (_, { campaignId }) => {
      queryClient.invalidateQueries({ queryKey: campaignKeys.detail(campaignId) });
      toast.success("Brands added", {
        description: "Brands have been added to the discount rule.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to add brands.",
      });
    },
  });
}

export function useAddCustomersToRule() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      campaignId,
      ruleId,
      customerIds,
    }: {
      campaignId: string;
      ruleId: string;
      customerIds: string[];
    }) => addCustomersToRule(campaignId, ruleId, customerIds),
    onSuccess: (_, { campaignId }) => {
      queryClient.invalidateQueries({ queryKey: campaignKeys.detail(campaignId) });
      toast.success("Customers added", {
        description: "Customers have been added to the discount rule.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to add customers.",
      });
    },
  });
}

export function useAddCustomerTiersToRule() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      campaignId,
      ruleId,
      tiers,
    }: {
      campaignId: string;
      ruleId: string;
      tiers: PriceTier[];
    }) => addCustomerTiersToRule(campaignId, ruleId, tiers),
    onSuccess: (_, { campaignId }) => {
      queryClient.invalidateQueries({ queryKey: campaignKeys.detail(campaignId) });
      toast.success("Customer tiers added", {
        description: "Customer tiers have been added to the discount rule.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to add customer tiers.",
      });
    },
  });
}
