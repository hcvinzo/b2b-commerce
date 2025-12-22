import { apiClient, PaginatedResponse } from "./client";
import {
  AdminUser,
  AdminUserListItem,
  AdminUserFilters,
  CreateAdminUserDto,
  UpdateAdminUserDto,
  AvailableRole,
  UserLogin,
  UserClaim,
  AddUserClaimDto,
  SetUserRolesDto,
} from "@/types/entities";

const ADMIN_USERS_BASE = "/admin/users";

export async function getAdminUsers(
  filters: AdminUserFilters = {}
): Promise<PaginatedResponse<AdminUserListItem>> {
  const params = new URLSearchParams();

  if (filters.page) params.append("page", String(filters.page));
  if (filters.pageSize) params.append("pageSize", String(filters.pageSize));
  if (filters.search) params.append("search", filters.search);
  if (filters.isActive !== undefined)
    params.append("isActive", String(filters.isActive));

  const response = await apiClient.get<PaginatedResponse<AdminUserListItem>>(
    `${ADMIN_USERS_BASE}?${params.toString()}`
  );
  return response.data;
}

export async function getAdminUser(id: string): Promise<AdminUser> {
  const response = await apiClient.get<AdminUser>(`${ADMIN_USERS_BASE}/${id}`);
  return response.data;
}

export async function createAdminUser(
  data: CreateAdminUserDto
): Promise<AdminUser> {
  const response = await apiClient.post<AdminUser>(ADMIN_USERS_BASE, data);
  return response.data;
}

export async function updateAdminUser(
  id: string,
  data: UpdateAdminUserDto
): Promise<AdminUser> {
  const response = await apiClient.put<AdminUser>(
    `${ADMIN_USERS_BASE}/${id}`,
    data
  );
  return response.data;
}

export async function activateAdminUser(id: string): Promise<void> {
  await apiClient.post(`${ADMIN_USERS_BASE}/${id}/activate`);
}

export async function deactivateAdminUser(id: string): Promise<void> {
  await apiClient.post(`${ADMIN_USERS_BASE}/${id}/deactivate`);
}

export async function deleteAdminUser(id: string): Promise<void> {
  await apiClient.delete(`${ADMIN_USERS_BASE}/${id}`);
}

export async function resetAdminUserPassword(id: string): Promise<void> {
  await apiClient.post(`${ADMIN_USERS_BASE}/${id}/reset-password`);
}

export async function getAvailableRoles(): Promise<AvailableRole[]> {
  const response = await apiClient.get<AvailableRole[]>(
    `${ADMIN_USERS_BASE}/available-roles`
  );
  return response.data;
}

// User Roles
export async function getUserRoles(id: string): Promise<string[]> {
  const response = await apiClient.get<string[]>(
    `${ADMIN_USERS_BASE}/${id}/roles`
  );
  return response.data;
}

export async function setUserRoles(
  id: string,
  data: SetUserRolesDto
): Promise<void> {
  await apiClient.put(`${ADMIN_USERS_BASE}/${id}/roles`, data);
}

// User Logins (external providers)
export async function getUserLogins(id: string): Promise<UserLogin[]> {
  const response = await apiClient.get<UserLogin[]>(
    `${ADMIN_USERS_BASE}/${id}/logins`
  );
  return response.data;
}

// User Claims
export async function getUserClaims(id: string): Promise<UserClaim[]> {
  const response = await apiClient.get<UserClaim[]>(
    `${ADMIN_USERS_BASE}/${id}/claims`
  );
  return response.data;
}

export async function addUserClaim(
  id: string,
  data: AddUserClaimDto
): Promise<void> {
  await apiClient.post(`${ADMIN_USERS_BASE}/${id}/claims`, data);
}

export async function removeUserClaim(
  userId: string,
  claimId: number
): Promise<void> {
  await apiClient.delete(`${ADMIN_USERS_BASE}/${userId}/claims/${claimId}`);
}
