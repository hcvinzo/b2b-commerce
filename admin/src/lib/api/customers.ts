import { apiClient, PaginatedResponse } from "./client";
import { Customer, CustomerFilters } from "@/types/entities";

const CUSTOMERS_BASE = "/customers";

export async function getCustomers(
  filters: CustomerFilters
): Promise<PaginatedResponse<Customer>> {
  const params = new URLSearchParams();

  if (filters.page) params.append("pageNumber", filters.page.toString());
  if (filters.pageSize) params.append("pageSize", filters.pageSize.toString());
  if (filters.search) params.append("search", filters.search);
  if (filters.isActive !== undefined)
    params.append("isActive", filters.isActive.toString());
  if (filters.isApproved !== undefined)
    params.append("isApproved", filters.isApproved.toString());
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

export async function rejectCustomer(id: string, reason?: string): Promise<void> {
  await apiClient.post(`${CUSTOMERS_BASE}/${id}/reject`, { reason });
}

export async function activateCustomer(id: string): Promise<void> {
  await apiClient.post(`${CUSTOMERS_BASE}/${id}/activate`);
}

export async function deactivateCustomer(id: string): Promise<void> {
  await apiClient.post(`${CUSTOMERS_BASE}/${id}/deactivate`);
}

export async function updateCreditLimit(
  id: string,
  creditLimit: number
): Promise<void> {
  await apiClient.patch(`${CUSTOMERS_BASE}/${id}/credit-limit`, { creditLimit });
}
