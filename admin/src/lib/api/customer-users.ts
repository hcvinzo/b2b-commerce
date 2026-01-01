import { apiClient, PaginatedResponse } from "./client";
import {
  CustomerUserListItem,
  CustomerUser,
  CustomerUserFilters,
  CreateCustomerUserDto,
  UpdateCustomerUserDto,
  SetCustomerUserRolesDto,
  CustomerUserRole,
} from "@/types/entities";

/**
 * Get customer users for a specific customer
 */
export async function getCustomerUsers(
  customerId: string,
  filters: CustomerUserFilters = {}
): Promise<PaginatedResponse<CustomerUserListItem>> {
  const params = new URLSearchParams();

  if (filters.page) params.append("page", String(filters.page));
  if (filters.pageSize) params.append("pageSize", String(filters.pageSize));
  if (filters.search) params.append("search", filters.search);

  const response = await apiClient.get<PaginatedResponse<CustomerUserListItem>>(
    `/admin/customers/${customerId}/users?${params.toString()}`
  );
  return response.data;
}

/**
 * Get a specific customer user by ID
 */
export async function getCustomerUser(
  customerId: string,
  userId: string
): Promise<CustomerUser> {
  const response = await apiClient.get<CustomerUser>(
    `/admin/customers/${customerId}/users/${userId}`
  );
  return response.data;
}

/**
 * Create a new user for a customer
 */
export async function createCustomerUser(
  customerId: string,
  data: CreateCustomerUserDto
): Promise<CustomerUser> {
  const response = await apiClient.post<CustomerUser>(
    `/admin/customers/${customerId}/users`,
    data
  );
  return response.data;
}

/**
 * Update a customer user
 */
export async function updateCustomerUser(
  customerId: string,
  userId: string,
  data: UpdateCustomerUserDto
): Promise<CustomerUser> {
  const response = await apiClient.put<CustomerUser>(
    `/admin/customers/${customerId}/users/${userId}`,
    data
  );
  return response.data;
}

/**
 * Activate a customer user
 */
export async function activateCustomerUser(
  customerId: string,
  userId: string
): Promise<void> {
  await apiClient.post(`/admin/customers/${customerId}/users/${userId}/activate`);
}

/**
 * Deactivate a customer user
 */
export async function deactivateCustomerUser(
  customerId: string,
  userId: string
): Promise<void> {
  await apiClient.post(`/admin/customers/${customerId}/users/${userId}/deactivate`);
}

/**
 * Set roles for a customer user
 */
export async function setCustomerUserRoles(
  customerId: string,
  userId: string,
  data: SetCustomerUserRolesDto
): Promise<void> {
  await apiClient.put(
    `/admin/customers/${customerId}/users/${userId}/roles`,
    data
  );
}

/**
 * Get available customer roles (for role assignment)
 */
export async function getAvailableCustomerRoles(): Promise<CustomerUserRole[]> {
  const response = await apiClient.get<CustomerUserRole[]>(
    `/admin/customer-roles`
  );
  return response.data;
}
