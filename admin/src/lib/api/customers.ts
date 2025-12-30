import { apiClient, PaginatedResponse } from "./client";
import {
  Customer,
  CustomerFilters,
  UpdateCustomerData,
  CustomerAttribute,
  UpsertCustomerAttributesDto,
} from "@/types/entities";

const CUSTOMERS_BASE = "/customers";

export async function getCustomers(
  filters: CustomerFilters
): Promise<PaginatedResponse<Customer>> {
  const params = new URLSearchParams();

  if (filters.page) params.append("pageNumber", filters.page.toString());
  if (filters.pageSize) params.append("pageSize", filters.pageSize.toString());
  if (filters.search) params.append("search", filters.search);
  if (filters.status) params.append("status", filters.status);
  if (filters.sortBy) params.append("sortBy", filters.sortBy);
  if (filters.sortOrder) params.append("sortDirection", filters.sortOrder);

  const response = await apiClient.get<PaginatedResponse<Customer>>(
    `${CUSTOMERS_BASE}?${params.toString()}`
  );
  return response.data;
}

export async function getCustomer(id: string): Promise<Customer> {
  const response = await apiClient.get<Customer>(`${CUSTOMERS_BASE}/${id}`);
  return response.data;
}

export async function approveCustomer(id: string): Promise<void> {
  await apiClient.post(`${CUSTOMERS_BASE}/${id}/approve`);
}

export async function activateCustomer(id: string): Promise<void> {
  await apiClient.post(`${CUSTOMERS_BASE}/${id}/activate`);
}

export async function deactivateCustomer(id: string): Promise<void> {
  await apiClient.post(`${CUSTOMERS_BASE}/${id}/deactivate`);
}

export async function updateCustomer(
  id: string,
  data: UpdateCustomerData
): Promise<Customer> {
  const response = await apiClient.put<Customer>(`${CUSTOMERS_BASE}/${id}`, data);
  return response.data;
}

export async function deleteCustomer(id: string): Promise<void> {
  await apiClient.delete(`${CUSTOMERS_BASE}/${id}`);
}

// Customer Attributes API
export async function getCustomerAttributes(
  customerId: string,
  attributeDefinitionId?: string
): Promise<CustomerAttribute[]> {
  const params = attributeDefinitionId ? `?attributeDefinitionId=${attributeDefinitionId}` : "";
  const response = await apiClient.get<CustomerAttribute[]>(
    `${CUSTOMERS_BASE}/${customerId}/attributes${params}`
  );
  return response.data;
}

export async function upsertCustomerAttributes(
  customerId: string,
  data: UpsertCustomerAttributesDto
): Promise<CustomerAttribute[]> {
  const response = await apiClient.put<CustomerAttribute[]>(
    `${CUSTOMERS_BASE}/${customerId}/attributes`,
    data
  );
  return response.data;
}

export async function deleteCustomerAttribute(
  customerId: string,
  attributeId: string
): Promise<void> {
  await apiClient.delete(`${CUSTOMERS_BASE}/${customerId}/attributes/${attributeId}`);
}
