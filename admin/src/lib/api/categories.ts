import { apiClient, PaginatedResponse } from "./client";
import { Category, CreateCategoryDto, UpdateCategoryDto } from "@/types/entities";

const CATEGORIES_BASE = "/categories";

export async function getCategories(): Promise<Category[]> {
  const response = await apiClient.get<Category[]>(`${CATEGORIES_BASE}/tree`);
  return response.data;
}

export async function getCategoriesFlat(): Promise<PaginatedResponse<Category>> {
  const response = await apiClient.get<PaginatedResponse<Category>>(
    `${CATEGORIES_BASE}?pageSize=1000`
  );
  return response.data;
}

export async function getRootCategories(): Promise<Category[]> {
  const response = await apiClient.get<Category[]>(`${CATEGORIES_BASE}/root`);
  return response.data;
}

export async function getCategory(id: string): Promise<Category> {
  const response = await apiClient.get<Category>(`${CATEGORIES_BASE}/${id}`);
  return response.data;
}

export async function getSubcategories(parentId: string): Promise<Category[]> {
  const response = await apiClient.get<Category[]>(
    `${CATEGORIES_BASE}/${parentId}/subcategories`
  );
  return response.data;
}

export async function createCategory(
  data: CreateCategoryDto
): Promise<Category> {
  const response = await apiClient.post<Category>(CATEGORIES_BASE, data);
  return response.data;
}

export async function updateCategory(
  id: string,
  data: UpdateCategoryDto
): Promise<Category> {
  const response = await apiClient.put<Category>(
    `${CATEGORIES_BASE}/${id}`,
    data
  );
  return response.data;
}

export async function deleteCategory(id: string): Promise<void> {
  await apiClient.delete(`${CATEGORIES_BASE}/${id}`);
}

export async function activateCategory(id: string): Promise<void> {
  await apiClient.post(`${CATEGORIES_BASE}/${id}/activate`);
}

export async function deactivateCategory(id: string): Promise<void> {
  await apiClient.post(`${CATEGORIES_BASE}/${id}/deactivate`);
}
