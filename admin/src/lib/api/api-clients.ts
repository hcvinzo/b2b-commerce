import { apiClient, PaginatedResponse } from "./client";
import {
  ApiClient,
  ApiClientListItem,
  ApiClientFilters,
  CreateApiClientDto,
  UpdateApiClientDto,
} from "@/types/entities";

const API_CLIENTS_BASE = "/admin/integration/clients";

export async function getApiClients(
  filters: ApiClientFilters = {}
): Promise<PaginatedResponse<ApiClientListItem>> {
  const params = new URLSearchParams();

  if (filters.page) params.append("page", String(filters.page));
  if (filters.pageSize) params.append("pageSize", String(filters.pageSize));
  if (filters.isActive !== undefined)
    params.append("isActive", String(filters.isActive));

  const response = await apiClient.get<PaginatedResponse<ApiClientListItem>>(
    `${API_CLIENTS_BASE}?${params.toString()}`
  );
  return response.data;
}

export async function getApiClient(id: string): Promise<ApiClient> {
  const response = await apiClient.get<ApiClient>(`${API_CLIENTS_BASE}/${id}`);
  return response.data;
}

export async function createApiClient(
  data: CreateApiClientDto
): Promise<ApiClient> {
  const response = await apiClient.post<ApiClient>(API_CLIENTS_BASE, data);
  return response.data;
}

export async function updateApiClient(
  id: string,
  data: UpdateApiClientDto
): Promise<ApiClient> {
  const response = await apiClient.put<ApiClient>(
    `${API_CLIENTS_BASE}/${id}`,
    data
  );
  return response.data;
}

export async function activateApiClient(id: string): Promise<void> {
  await apiClient.post(`${API_CLIENTS_BASE}/${id}/activate`);
}

export async function deactivateApiClient(id: string): Promise<void> {
  await apiClient.post(`${API_CLIENTS_BASE}/${id}/deactivate`);
}

export async function deleteApiClient(id: string): Promise<void> {
  await apiClient.delete(`${API_CLIENTS_BASE}/${id}`);
}
