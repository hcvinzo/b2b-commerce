import { apiClient, PaginatedResponse } from "./client";
import { Category, CreateCategoryDto, UpdateCategoryDto } from "@/types/entities";

const CATEGORIES_BASE = "/categories";

// Backend CategoryTreeDto uses subCategories, frontend uses children
interface CategoryTreeDto {
  id: string;
  name: string;
  description?: string;
  imageUrl?: string;
  displayOrder: number;
  isActive: boolean;
  externalCode?: string;
  externalId?: string;
  lastSyncedAt?: string;
  subCategories?: CategoryTreeDto[];
}

// Map backend CategoryTreeDto to frontend Category type
function mapCategoryTree(dto: CategoryTreeDto): Category {
  return {
    id: dto.id,
    name: dto.name,
    slug: "", // Not returned in tree endpoint
    description: dto.description,
    imageUrl: dto.imageUrl,
    displayOrder: dto.displayOrder,
    isActive: dto.isActive,
    level: 0, // Will be calculated client-side if needed
    path: "",
    productCount: 0,
    externalCode: dto.externalCode,
    externalId: dto.externalId,
    lastSyncedAt: dto.lastSyncedAt,
    createdAt: "",
    isDeleted: false,
    children: dto.subCategories?.map(mapCategoryTree),
  };
}

export async function getCategories(): Promise<Category[]> {
  // activeOnly=false to show all categories (including inactive) in admin
  const response = await apiClient.get<CategoryTreeDto[]>(`${CATEGORIES_BASE}/tree?activeOnly=false`);
  return response.data.map(mapCategoryTree);
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
