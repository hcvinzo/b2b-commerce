import { apiClient, PaginatedResponse } from "./client";
import {
  GeoLocationType,
  CreateGeoLocationTypeDto,
  UpdateGeoLocationTypeDto,
  GeoLocationTypeFilters,
} from "@/types/entities";

const BASE_URL = "/GeoLocationTypes";

export async function getGeoLocationTypes(
  filters?: GeoLocationTypeFilters
): Promise<PaginatedResponse<GeoLocationType>> {
  // Backend returns a simple list, we convert to paginated response for consistency
  const response = await apiClient.get<GeoLocationType[]>(BASE_URL);
  const allItems = response.data;

  // Apply client-side filtering if needed
  let filteredItems = allItems;
  if (filters?.search) {
    const search = filters.search.toLowerCase();
    filteredItems = filteredItems.filter((item) =>
      item.name.toLowerCase().includes(search)
    );
  }

  // Apply pagination
  const page = filters?.page || 1;
  const pageSize = filters?.pageSize || 10;
  const startIndex = (page - 1) * pageSize;
  const paginatedItems = filteredItems.slice(startIndex, startIndex + pageSize);

  return {
    items: paginatedItems,
    totalCount: filteredItems.length,
    pageNumber: page,
    pageSize: pageSize,
    totalPages: Math.ceil(filteredItems.length / pageSize),
    hasPreviousPage: page > 1,
    hasNextPage: page * pageSize < filteredItems.length,
  };
}

export async function getAllGeoLocationTypes(): Promise<GeoLocationType[]> {
  const response = await apiClient.get<GeoLocationType[]>(BASE_URL);
  return response.data;
}

export async function getGeoLocationType(id: string): Promise<GeoLocationType> {
  const response = await apiClient.get<GeoLocationType>(`${BASE_URL}/${id}`);
  return response.data;
}

export async function createGeoLocationType(
  data: CreateGeoLocationTypeDto
): Promise<GeoLocationType> {
  const response = await apiClient.post<GeoLocationType>(BASE_URL, data);
  return response.data;
}

export async function updateGeoLocationType(
  id: string,
  data: UpdateGeoLocationTypeDto
): Promise<GeoLocationType> {
  const response = await apiClient.put<GeoLocationType>(
    `${BASE_URL}/${id}`,
    data
  );
  return response.data;
}

export async function deleteGeoLocationType(id: string): Promise<void> {
  await apiClient.delete(`${BASE_URL}/${id}`);
}
