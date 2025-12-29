import { apiClient, PaginatedResponse } from "./client";
import {
  GeoLocation,
  GeoLocationListItem,
  GeoLocationTree,
  CreateGeoLocationDto,
  UpdateGeoLocationDto,
  GeoLocationFilters,
} from "@/types/entities";

const BASE_URL = "/GeoLocations";

export async function getGeoLocations(
  filters?: GeoLocationFilters
): Promise<PaginatedResponse<GeoLocationListItem>> {
  const params = new URLSearchParams();

  if (filters?.page) params.append("pageNumber", filters.page.toString());
  if (filters?.pageSize) params.append("pageSize", filters.pageSize.toString());
  if (filters?.search) params.append("search", filters.search);
  if (filters?.typeId) params.append("typeId", filters.typeId);
  if (filters?.parentId) params.append("parentId", filters.parentId);
  if (filters?.isActive !== undefined)
    params.append("isActive", filters.isActive.toString());
  if (filters?.sortBy) params.append("sortBy", filters.sortBy);
  if (filters?.sortDirection)
    params.append("sortDirection", filters.sortDirection);

  const response = await apiClient.get<PaginatedResponse<GeoLocationListItem>>(
    `${BASE_URL}?${params.toString()}`
  );
  return response.data;
}

export async function getGeoLocation(id: string): Promise<GeoLocation> {
  const response = await apiClient.get<GeoLocation>(`${BASE_URL}/${id}`);
  return response.data;
}

export async function getGeoLocationTree(
  typeId?: string
): Promise<GeoLocationTree[]> {
  const params = typeId ? `?typeId=${typeId}` : "";
  const response = await apiClient.get<GeoLocationTree[]>(
    `${BASE_URL}/tree${params}`
  );
  return response.data;
}

export async function getGeoLocationsByType(
  typeId: string
): Promise<GeoLocationListItem[]> {
  const response = await apiClient.get<GeoLocationListItem[]>(
    `${BASE_URL}/by-type/${typeId}`
  );
  return response.data;
}

export async function getGeoLocationsByParent(
  parentId?: string
): Promise<GeoLocationListItem[]> {
  // If no parentId, get root locations; otherwise get children of the parent
  const endpoint = parentId
    ? `${BASE_URL}/${parentId}/children`
    : `${BASE_URL}/root`;
  const response = await apiClient.get<GeoLocationListItem[]>(endpoint);
  return response.data;
}

export async function createGeoLocation(
  data: CreateGeoLocationDto
): Promise<GeoLocation> {
  const response = await apiClient.post<GeoLocation>(BASE_URL, data);
  return response.data;
}

export async function updateGeoLocation(
  id: string,
  data: UpdateGeoLocationDto
): Promise<GeoLocation> {
  const response = await apiClient.put<GeoLocation>(`${BASE_URL}/${id}`, data);
  return response.data;
}

export async function deleteGeoLocation(id: string): Promise<void> {
  await apiClient.delete(`${BASE_URL}/${id}`);
}
