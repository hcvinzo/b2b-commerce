import { apiClient, PaginatedResponse } from "./client";
import {
  Product,
  CreateProductDto,
  UpdateProductDto,
  ProductFilters,
  ProductRelationsGroup,
  ProductRelationType,
  RelatedProductInput,
  ProductListItem,
} from "@/types/entities";

const PRODUCTS_BASE = "/products";

export async function getProducts(
  filters: ProductFilters
): Promise<PaginatedResponse<Product>> {
  const params = new URLSearchParams();

  if (filters.page) params.append("pageNumber", filters.page.toString());
  if (filters.pageSize) params.append("pageSize", filters.pageSize.toString());
  if (filters.search) params.append("search", filters.search);
  if (filters.categoryId) params.append("categoryId", filters.categoryId);
  if (filters.brandId) params.append("brandId", filters.brandId);
  if (filters.isActive !== undefined)
    params.append("isActive", filters.isActive.toString());
  if (filters.sortBy) params.append("sortBy", filters.sortBy);
  if (filters.sortOrder) params.append("sortDirection", filters.sortOrder);

  const response = await apiClient.get<PaginatedResponse<Product>>(
    `${PRODUCTS_BASE}?${params.toString()}`
  );
  return response.data;
}

export async function getProduct(id: string): Promise<Product> {
  const response = await apiClient.get<Product>(`${PRODUCTS_BASE}/${id}`);
  return response.data;
}

export async function createProduct(data: CreateProductDto): Promise<Product> {
  const response = await apiClient.post<Product>(PRODUCTS_BASE, data);
  return response.data;
}

export async function updateProduct(
  id: string,
  data: UpdateProductDto
): Promise<Product> {
  const response = await apiClient.put<Product>(`${PRODUCTS_BASE}/${id}`, data);
  return response.data;
}

export async function deleteProduct(id: string): Promise<void> {
  await apiClient.delete(`${PRODUCTS_BASE}/${id}`);
}

export async function toggleProductStatus(id: string): Promise<Product> {
  const response = await apiClient.patch<Product>(
    `${PRODUCTS_BASE}/${id}/toggle-status`
  );
  return response.data;
}

export async function activateProduct(id: string): Promise<void> {
  await apiClient.post(`${PRODUCTS_BASE}/${id}/activate`);
}

export async function deactivateProduct(id: string): Promise<void> {
  await apiClient.post(`${PRODUCTS_BASE}/${id}/deactivate`);
}

// Product Relations API

export async function getProductRelations(
  productId: string
): Promise<ProductRelationsGroup[]> {
  const response = await apiClient.get<ProductRelationsGroup[]>(
    `${PRODUCTS_BASE}/${productId}/relations`
  );
  return response.data;
}

export async function setProductRelations(
  productId: string,
  relationType: ProductRelationType,
  relatedProducts: RelatedProductInput[]
): Promise<void> {
  await apiClient.put(
    `${PRODUCTS_BASE}/${productId}/relations/${relationType}`,
    relatedProducts
  );
}

export async function searchProductsForSelection(
  query: string,
  excludeId?: string
): Promise<ProductListItem[]> {
  const params = new URLSearchParams();
  params.append("search", query);
  params.append("pageSize", "20");
  params.append("isActive", "true");

  const response = await apiClient.get<PaginatedResponse<ProductListItem>>(
    `${PRODUCTS_BASE}?${params.toString()}`
  );

  // Filter out the excluded product (current product being edited)
  const items = response.data.items;
  return excludeId ? items.filter((p) => p.id !== excludeId) : items;
}
