import { apiClient, PaginatedResponse } from "./client";
import { Brand } from "@/types/entities";

const BRANDS_BASE = "/brands";

// DTOs matching backend
export interface CreateBrandDto {
  name: string;
  description?: string;
  logoUrl?: string;
  websiteUrl?: string;
  isActive?: boolean;
}

export interface UpdateBrandDto {
  name?: string;
  description?: string;
  logoUrl?: string;
  websiteUrl?: string;
  isActive?: boolean;
}

export interface BrandListItem {
  id: string;
  name: string;
  description?: string;
  logoUrl?: string;
  isActive: boolean;
  productCount: number;
  createdAt: string;
}

export interface BrandFilters {
  search?: string;
  isActive?: boolean;
  pageNumber?: number;
  pageSize?: number;
  sortBy?: string;
  sortDirection?: "asc" | "desc";
}

export async function getBrands(
  filters: BrandFilters = {}
): Promise<PaginatedResponse<BrandListItem>> {
  const params = new URLSearchParams();

  if (filters.search) params.append("search", filters.search);
  if (filters.isActive !== undefined) params.append("isActive", String(filters.isActive));
  if (filters.pageNumber) params.append("pageNumber", String(filters.pageNumber));
  if (filters.pageSize) params.append("pageSize", String(filters.pageSize));
  if (filters.sortBy) params.append("sortBy", filters.sortBy);
  if (filters.sortDirection) params.append("sortDirection", filters.sortDirection);

  const response = await apiClient.get<PaginatedResponse<BrandListItem>>(
    `${BRANDS_BASE}?${params.toString()}`
  );
  return response.data;
}

export async function getBrand(id: string): Promise<Brand> {
  const response = await apiClient.get<Brand>(`${BRANDS_BASE}/${id}`);
  return response.data;
}

export async function createBrand(data: CreateBrandDto): Promise<Brand> {
  const response = await apiClient.post<Brand>(BRANDS_BASE, data);
  return response.data;
}

export async function updateBrand(
  id: string,
  data: UpdateBrandDto
): Promise<Brand> {
  const response = await apiClient.put<Brand>(`${BRANDS_BASE}/${id}`, data);
  return response.data;
}

export async function deleteBrand(id: string): Promise<void> {
  await apiClient.delete(`${BRANDS_BASE}/${id}`);
}

export async function activateBrand(id: string): Promise<Brand> {
  const response = await apiClient.post<Brand>(`${BRANDS_BASE}/${id}/activate`);
  return response.data;
}

export async function deactivateBrand(id: string): Promise<Brand> {
  const response = await apiClient.post<Brand>(`${BRANDS_BASE}/${id}/deactivate`);
  return response.data;
}
