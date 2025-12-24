import { apiClient, PaginatedResponse } from "./client";
import {
  Collection,
  CollectionListItem,
  CollectionFilter,
  ProductInCollection,
  CreateCollectionDto,
  UpdateCollectionDto,
  SetCollectionProductsDto,
  SetCollectionFiltersDto,
  CollectionFilters,
} from "@/types/entities";

const COLLECTIONS_BASE = "/collections";

export async function getCollections(
  filters: CollectionFilters
): Promise<PaginatedResponse<CollectionListItem>> {
  const params = new URLSearchParams();

  if (filters.page) params.append("pageNumber", filters.page.toString());
  if (filters.pageSize) params.append("pageSize", filters.pageSize.toString());
  if (filters.search) params.append("search", filters.search);
  if (filters.type) params.append("type", filters.type);
  if (filters.isActive !== undefined)
    params.append("isActive", filters.isActive.toString());
  if (filters.isFeatured !== undefined)
    params.append("isFeatured", filters.isFeatured.toString());
  if (filters.sortBy) params.append("sortBy", filters.sortBy);
  if (filters.sortDirection) params.append("sortDirection", filters.sortDirection);

  const response = await apiClient.get<PaginatedResponse<CollectionListItem>>(
    `${COLLECTIONS_BASE}?${params.toString()}`
  );
  return response.data;
}

export async function getCollection(id: string): Promise<Collection> {
  const response = await apiClient.get<Collection>(`${COLLECTIONS_BASE}/${id}`);
  return response.data;
}

export async function createCollection(
  data: CreateCollectionDto
): Promise<Collection> {
  // Clean up empty strings - backend expects null/undefined for optional date fields
  const cleanedData = {
    ...data,
    description: data.description || undefined,
    imageUrl: data.imageUrl || undefined,
    startDate: data.startDate || undefined,
    endDate: data.endDate || undefined,
  };
  const response = await apiClient.post<Collection>(COLLECTIONS_BASE, cleanedData);
  return response.data;
}

export async function updateCollection(
  id: string,
  data: UpdateCollectionDto
): Promise<Collection> {
  // Clean up empty strings - backend expects null/undefined for optional date fields
  const cleanedData = {
    ...data,
    description: data.description || undefined,
    imageUrl: data.imageUrl || undefined,
    startDate: data.startDate || undefined,
    endDate: data.endDate || undefined,
  };
  const response = await apiClient.put<Collection>(
    `${COLLECTIONS_BASE}/${id}`,
    cleanedData
  );
  return response.data;
}

export async function deleteCollection(id: string): Promise<void> {
  await apiClient.delete(`${COLLECTIONS_BASE}/${id}`);
}

export async function activateCollection(id: string): Promise<void> {
  await apiClient.post(`${COLLECTIONS_BASE}/${id}/activate`);
}

export async function deactivateCollection(id: string): Promise<void> {
  await apiClient.post(`${COLLECTIONS_BASE}/${id}/deactivate`);
}

// Collection Products API (Manual collections)

export async function getCollectionProducts(
  collectionId: string,
  page = 1,
  pageSize = 20
): Promise<PaginatedResponse<ProductInCollection>> {
  const params = new URLSearchParams();
  params.append("pageNumber", page.toString());
  params.append("pageSize", pageSize.toString());

  const response = await apiClient.get<PaginatedResponse<ProductInCollection>>(
    `${COLLECTIONS_BASE}/${collectionId}/products?${params.toString()}`
  );
  return response.data;
}

export async function setCollectionProducts(
  collectionId: string,
  data: SetCollectionProductsDto
): Promise<void> {
  await apiClient.put(`${COLLECTIONS_BASE}/${collectionId}/products`, data);
}

// Collection Filters API (Dynamic collections)

export async function setCollectionFilters(
  collectionId: string,
  data: SetCollectionFiltersDto
): Promise<CollectionFilter> {
  // Clean up the data - remove undefined/null values and empty arrays
  const cleanedData: SetCollectionFiltersDto = {};

  if (data.categoryIds && data.categoryIds.length > 0) {
    cleanedData.categoryIds = data.categoryIds;
  }
  if (data.brandIds && data.brandIds.length > 0) {
    cleanedData.brandIds = data.brandIds;
  }
  if (data.productTypeIds && data.productTypeIds.length > 0) {
    cleanedData.productTypeIds = data.productTypeIds;
  }
  if (data.minPrice !== undefined && data.minPrice !== null) {
    cleanedData.minPrice = data.minPrice;
  }
  if (data.maxPrice !== undefined && data.maxPrice !== null) {
    cleanedData.maxPrice = data.maxPrice;
  }

  const response = await apiClient.put<CollectionFilter>(
    `${COLLECTIONS_BASE}/${collectionId}/filters`,
    cleanedData
  );
  return response.data;
}
