import { apiClient, PaginatedResponse } from "./client";
import {
  Campaign,
  CampaignListItem,
  CampaignFilters,
  CreateCampaignDto,
  UpdateCampaignDto,
  DiscountRule,
  CreateDiscountRuleDto,
  UpdateDiscountRuleDto,
  CampaignUsageStats,
  PriceTier,
} from "@/types/entities";

const CAMPAIGNS_BASE = "/admin/campaigns";

// ============================================
// Campaign CRUD
// ============================================

export async function getCampaigns(
  filters: CampaignFilters
): Promise<PaginatedResponse<CampaignListItem>> {
  const params = new URLSearchParams();

  if (filters.page) params.append("pageNumber", filters.page.toString());
  if (filters.pageSize) params.append("pageSize", filters.pageSize.toString());
  if (filters.search) params.append("search", filters.search);
  if (filters.status) params.append("status", filters.status);
  if (filters.startDateFrom) params.append("startDateFrom", filters.startDateFrom);
  if (filters.startDateTo) params.append("startDateTo", filters.startDateTo);
  if (filters.sortBy) params.append("sortBy", filters.sortBy);
  if (filters.sortDirection) params.append("sortDirection", filters.sortDirection);

  const response = await apiClient.get<PaginatedResponse<CampaignListItem>>(
    `${CAMPAIGNS_BASE}?${params.toString()}`
  );
  return response.data;
}

export async function getCampaign(id: string): Promise<Campaign> {
  const response = await apiClient.get<Campaign>(`${CAMPAIGNS_BASE}/${id}`);
  return response.data;
}

export async function createCampaign(data: CreateCampaignDto): Promise<Campaign> {
  const cleanedData = {
    ...data,
    description: data.description || undefined,
    currency: data.currency || "TRY",
  };
  const response = await apiClient.post<Campaign>(CAMPAIGNS_BASE, cleanedData);
  return response.data;
}

export async function updateCampaign(
  id: string,
  data: UpdateCampaignDto
): Promise<Campaign> {
  const cleanedData = {
    ...data,
    description: data.description || undefined,
  };
  const response = await apiClient.put<Campaign>(
    `${CAMPAIGNS_BASE}/${id}`,
    cleanedData
  );
  return response.data;
}

export async function deleteCampaign(id: string): Promise<void> {
  await apiClient.delete(`${CAMPAIGNS_BASE}/${id}`);
}

// ============================================
// Campaign Status Transitions
// ============================================

export async function scheduleCampaign(id: string): Promise<Campaign> {
  const response = await apiClient.post<Campaign>(`${CAMPAIGNS_BASE}/${id}/schedule`);
  return response.data;
}

export async function activateCampaign(id: string): Promise<Campaign> {
  const response = await apiClient.post<Campaign>(`${CAMPAIGNS_BASE}/${id}/activate`);
  return response.data;
}

export async function pauseCampaign(id: string): Promise<Campaign> {
  const response = await apiClient.post<Campaign>(`${CAMPAIGNS_BASE}/${id}/pause`);
  return response.data;
}

export async function cancelCampaign(id: string): Promise<Campaign> {
  const response = await apiClient.post<Campaign>(`${CAMPAIGNS_BASE}/${id}/cancel`);
  return response.data;
}

// ============================================
// Campaign Usage Stats
// ============================================

export async function getCampaignUsageStats(id: string): Promise<CampaignUsageStats> {
  const response = await apiClient.get<CampaignUsageStats>(
    `${CAMPAIGNS_BASE}/${id}/usage-stats`
  );
  return response.data;
}

// ============================================
// Discount Rules
// ============================================

export async function addDiscountRule(
  campaignId: string,
  data: CreateDiscountRuleDto
): Promise<DiscountRule> {
  const response = await apiClient.post<DiscountRule>(
    `${CAMPAIGNS_BASE}/${campaignId}/rules`,
    data
  );
  return response.data;
}

export async function updateDiscountRule(
  campaignId: string,
  ruleId: string,
  data: UpdateDiscountRuleDto
): Promise<DiscountRule> {
  const response = await apiClient.put<DiscountRule>(
    `${CAMPAIGNS_BASE}/${campaignId}/rules/${ruleId}`,
    data
  );
  return response.data;
}

export async function removeDiscountRule(
  campaignId: string,
  ruleId: string
): Promise<void> {
  await apiClient.delete(`${CAMPAIGNS_BASE}/${campaignId}/rules/${ruleId}`);
}

// ============================================
// Discount Rule Targets
// ============================================

export async function addProductsToRule(
  campaignId: string,
  ruleId: string,
  productIds: string[]
): Promise<void> {
  await apiClient.post(
    `${CAMPAIGNS_BASE}/${campaignId}/rules/${ruleId}/products`,
    productIds
  );
}

export async function addCategoriesToRule(
  campaignId: string,
  ruleId: string,
  categoryIds: string[]
): Promise<void> {
  await apiClient.post(
    `${CAMPAIGNS_BASE}/${campaignId}/rules/${ruleId}/categories`,
    categoryIds
  );
}

export async function addBrandsToRule(
  campaignId: string,
  ruleId: string,
  brandIds: string[]
): Promise<void> {
  await apiClient.post(
    `${CAMPAIGNS_BASE}/${campaignId}/rules/${ruleId}/brands`,
    brandIds
  );
}

export async function addCustomersToRule(
  campaignId: string,
  ruleId: string,
  customerIds: string[]
): Promise<void> {
  await apiClient.post(
    `${CAMPAIGNS_BASE}/${campaignId}/rules/${ruleId}/customers`,
    customerIds
  );
}

export async function addCustomerTiersToRule(
  campaignId: string,
  ruleId: string,
  tiers: PriceTier[]
): Promise<void> {
  await apiClient.post(
    `${CAMPAIGNS_BASE}/${campaignId}/rules/${ruleId}/customer-tiers`,
    tiers
  );
}
