import { apiClient } from "./client";
import {
  CreateApiKeyDto,
  CreateApiKeyResponse,
  RevokeApiKeyDto,
  AvailableScopesResponse,
  AddIpWhitelistDto,
  IpWhitelistEntry,
  ApiKeyDetail,
} from "@/types/entities";

const API_KEYS_BASE = "/admin/integration/keys";

export async function getApiKeyDetail(id: string): Promise<ApiKeyDetail> {
  const response = await apiClient.get<ApiKeyDetail>(`${API_KEYS_BASE}/${id}`);
  return response.data;
}

export async function createApiKey(
  data: CreateApiKeyDto
): Promise<CreateApiKeyResponse> {
  const response = await apiClient.post<CreateApiKeyResponse>(
    API_KEYS_BASE,
    data
  );
  return response.data;
}

export async function revokeApiKey(
  id: string,
  data: RevokeApiKeyDto
): Promise<void> {
  await apiClient.post(`${API_KEYS_BASE}/${id}/revoke`, data);
}

export async function rotateApiKey(id: string): Promise<CreateApiKeyResponse> {
  const response = await apiClient.post<CreateApiKeyResponse>(
    `${API_KEYS_BASE}/${id}/rotate`
  );
  return response.data;
}

export async function getAvailableScopes(): Promise<AvailableScopesResponse> {
  const response = await apiClient.get<AvailableScopesResponse>(
    `${API_KEYS_BASE}/available-scopes`
  );
  return response.data;
}

export async function addIpWhitelist(
  keyId: string,
  data: AddIpWhitelistDto
): Promise<IpWhitelistEntry> {
  const response = await apiClient.post<IpWhitelistEntry>(
    `${API_KEYS_BASE}/${keyId}/ip-whitelist`,
    data
  );
  return response.data;
}

export async function removeIpWhitelist(
  keyId: string,
  whitelistId: string
): Promise<void> {
  await apiClient.delete(`${API_KEYS_BASE}/${keyId}/ip-whitelist/${whitelistId}`);
}
