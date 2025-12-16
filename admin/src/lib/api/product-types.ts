import { apiClient } from "./client";
import {
  ProductType,
  ProductTypeListItem,
  CreateProductTypeDto,
  UpdateProductTypeDto,
  AddAttributeToProductTypeDto,
} from "@/types/entities";

const PRODUCT_TYPES_BASE = "/producttypes";

// Get all product types (list view)
export async function getProductTypes(isActive?: boolean): Promise<ProductTypeListItem[]> {
  const response = await apiClient.get<ProductTypeListItem[]>(PRODUCT_TYPES_BASE, {
    params: isActive !== undefined ? { isActive } : undefined,
  });
  return response.data;
}

// Get single product type by ID (with attributes)
export async function getProductType(id: string): Promise<ProductType> {
  const response = await apiClient.get<ProductType>(`${PRODUCT_TYPES_BASE}/${id}`);
  return response.data;
}

// Get product type by code (with attributes)
export async function getProductTypeByCode(code: string): Promise<ProductType> {
  const response = await apiClient.get<ProductType>(`${PRODUCT_TYPES_BASE}/code/${code}`);
  return response.data;
}

// Create new product type
export async function createProductType(data: CreateProductTypeDto): Promise<ProductType> {
  const response = await apiClient.post<ProductType>(PRODUCT_TYPES_BASE, data);
  return response.data;
}

// Update product type
export async function updateProductType(
  id: string,
  data: UpdateProductTypeDto
): Promise<ProductType> {
  const response = await apiClient.put<ProductType>(`${PRODUCT_TYPES_BASE}/${id}`, data);
  return response.data;
}

// Delete product type (soft delete)
export async function deleteProductType(id: string): Promise<void> {
  await apiClient.delete(`${PRODUCT_TYPES_BASE}/${id}`);
}

// Add attribute to product type
export async function addAttributeToProductType(
  productTypeId: string,
  data: AddAttributeToProductTypeDto
): Promise<ProductType> {
  const response = await apiClient.post<ProductType>(
    `${PRODUCT_TYPES_BASE}/${productTypeId}/attributes`,
    data
  );
  return response.data;
}

// Remove attribute from product type
export async function removeAttributeFromProductType(
  productTypeId: string,
  attributeDefinitionId: string
): Promise<void> {
  await apiClient.delete(
    `${PRODUCT_TYPES_BASE}/${productTypeId}/attributes/${attributeDefinitionId}`
  );
}
