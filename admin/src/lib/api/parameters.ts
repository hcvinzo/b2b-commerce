import { apiClient, PaginatedResponse } from "./client";
import {
  ParameterListItem,
  ParameterDetail,
  CreateParameterDto,
  UpdateParameterDto,
  ParameterFilters,
} from "@/types/entities";

const PARAMETERS_BASE = "/admin/parameters";

export async function getParameters(
  filters: ParameterFilters = {}
): Promise<PaginatedResponse<ParameterListItem>> {
  const params = new URLSearchParams();

  if (filters.page) params.append("page", String(filters.page));
  if (filters.pageSize) params.append("pageSize", String(filters.pageSize));
  if (filters.search) params.append("search", filters.search);
  if (filters.parameterType) params.append("parameterType", filters.parameterType);
  if (filters.category) params.append("category", filters.category);

  const response = await apiClient.get<PaginatedResponse<ParameterListItem>>(
    `${PARAMETERS_BASE}?${params.toString()}`
  );
  return response.data;
}

export async function getParameter(id: string): Promise<ParameterDetail> {
  const response = await apiClient.get<ParameterDetail>(`${PARAMETERS_BASE}/${id}`);
  return response.data;
}

export async function getParameterByKey(key: string): Promise<ParameterDetail> {
  const response = await apiClient.get<ParameterDetail>(
    `${PARAMETERS_BASE}/by-key/${encodeURIComponent(key)}`
  );
  return response.data;
}

export async function getParameterCategories(): Promise<string[]> {
  const response = await apiClient.get<string[]>(`${PARAMETERS_BASE}/categories`);
  return response.data;
}

export async function createParameter(data: CreateParameterDto): Promise<ParameterDetail> {
  const response = await apiClient.post<ParameterDetail>(PARAMETERS_BASE, data);
  return response.data;
}

export async function updateParameter(
  id: string,
  data: UpdateParameterDto
): Promise<ParameterDetail> {
  const response = await apiClient.put<ParameterDetail>(`${PARAMETERS_BASE}/${id}`, data);
  return response.data;
}

export async function deleteParameter(id: string): Promise<void> {
  await apiClient.delete(`${PARAMETERS_BASE}/${id}`);
}
