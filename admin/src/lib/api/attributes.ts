import { apiClient } from "./client";
import {
  AttributeDefinition,
  CreateAttributeDefinitionDto,
  UpdateAttributeDefinitionDto,
  CreateAttributeValueDto,
} from "@/types/entities";

const ATTRIBUTES_BASE = "/attributedefinitions";

// Get all attribute definitions (without values by default for performance)
export async function getAttributes(includeValues = false): Promise<AttributeDefinition[]> {
  const response = await apiClient.get<AttributeDefinition[]>(ATTRIBUTES_BASE, {
    params: { includeValues },
  });
  return response.data;
}

// Get single attribute definition by ID (with values)
export async function getAttribute(id: string): Promise<AttributeDefinition> {
  const response = await apiClient.get<AttributeDefinition>(
    `${ATTRIBUTES_BASE}/${id}`
  );
  return response.data;
}

// Get filterable attributes only
export async function getFilterableAttributes(): Promise<AttributeDefinition[]> {
  const response = await apiClient.get<AttributeDefinition[]>(
    `${ATTRIBUTES_BASE}/filterable`
  );
  return response.data;
}

// Create new attribute definition
export async function createAttribute(
  data: CreateAttributeDefinitionDto
): Promise<AttributeDefinition> {
  const response = await apiClient.post<AttributeDefinition>(
    ATTRIBUTES_BASE,
    data
  );
  return response.data;
}

// Update attribute definition
export async function updateAttribute(
  id: string,
  data: UpdateAttributeDefinitionDto
): Promise<AttributeDefinition> {
  const response = await apiClient.put<AttributeDefinition>(
    `${ATTRIBUTES_BASE}/${id}`,
    data
  );
  return response.data;
}

// Delete attribute definition
export async function deleteAttribute(id: string): Promise<void> {
  await apiClient.delete(`${ATTRIBUTES_BASE}/${id}`);
}

// Add predefined value to attribute
export async function addAttributeValue(
  attributeId: string,
  data: CreateAttributeValueDto
): Promise<AttributeDefinition> {
  const response = await apiClient.post<AttributeDefinition>(
    `${ATTRIBUTES_BASE}/${attributeId}/values`,
    data
  );
  return response.data;
}

// Remove predefined value from attribute
export async function removeAttributeValue(
  attributeId: string,
  valueId: string
): Promise<void> {
  await apiClient.delete(`${ATTRIBUTES_BASE}/${attributeId}/values/${valueId}`);
}
