import { apiClient, PaginatedResponse } from "./client";
import {
  RoleListItem,
  RoleDetail,
  RoleFilters,
  CreateRoleDto,
  UpdateRoleDto,
  SetRoleClaimsDto,
  RoleUserListItem,
  PermissionCategory,
} from "@/types/entities";

const ROLES_BASE = "/admin/roles";

// ============================================
// Role CRUD
// ============================================

export async function getRoles(
  filters: RoleFilters = {}
): Promise<PaginatedResponse<RoleListItem>> {
  const params = new URLSearchParams();

  if (filters.page) params.append("page", String(filters.page));
  if (filters.pageSize) params.append("pageSize", String(filters.pageSize));
  if (filters.search) params.append("search", filters.search);
  if (filters.userType) params.append("userType", filters.userType);

  const response = await apiClient.get<PaginatedResponse<RoleListItem>>(
    `${ROLES_BASE}?${params.toString()}`
  );
  return response.data;
}

export async function getRole(id: string): Promise<RoleDetail> {
  const response = await apiClient.get<RoleDetail>(`${ROLES_BASE}/${id}`);
  return response.data;
}

export async function createRole(data: CreateRoleDto): Promise<RoleDetail> {
  const response = await apiClient.post<RoleDetail>(ROLES_BASE, data);
  return response.data;
}

export async function updateRole(
  id: string,
  data: UpdateRoleDto
): Promise<RoleDetail> {
  const response = await apiClient.put<RoleDetail>(`${ROLES_BASE}/${id}`, data);
  return response.data;
}

export async function deleteRole(id: string): Promise<void> {
  await apiClient.delete(`${ROLES_BASE}/${id}`);
}

// ============================================
// Available Permissions
// ============================================

export async function getAvailablePermissions(): Promise<PermissionCategory[]> {
  const response = await apiClient.get<PermissionCategory[]>(
    `${ROLES_BASE}/available-permissions`
  );
  return response.data;
}

// ============================================
// Role Claims
// ============================================

export async function getRoleClaims(roleId: string): Promise<string[]> {
  const response = await apiClient.get<string[]>(
    `${ROLES_BASE}/${roleId}/claims`
  );
  return response.data;
}

export async function setRoleClaims(
  roleId: string,
  data: SetRoleClaimsDto
): Promise<void> {
  await apiClient.put(`${ROLES_BASE}/${roleId}/claims`, data);
}

export async function addClaimToRole(
  roleId: string,
  claimValue: string
): Promise<void> {
  await apiClient.post(`${ROLES_BASE}/${roleId}/claims`, {
    claimValue,
  });
}

export async function removeClaimFromRole(
  roleId: string,
  claimValue: string
): Promise<void> {
  await apiClient.delete(
    `${ROLES_BASE}/${roleId}/claims/${encodeURIComponent(claimValue)}`
  );
}

// ============================================
// Role Users
// ============================================

export async function getUsersInRole(
  roleId: string,
  page: number = 1,
  pageSize: number = 10
): Promise<PaginatedResponse<RoleUserListItem>> {
  const params = new URLSearchParams();
  params.append("page", String(page));
  params.append("pageSize", String(pageSize));

  const response = await apiClient.get<PaginatedResponse<RoleUserListItem>>(
    `${ROLES_BASE}/${roleId}/users?${params.toString()}`
  );
  return response.data;
}

export async function addUserToRole(
  roleId: string,
  userId: string
): Promise<void> {
  await apiClient.post(`${ROLES_BASE}/${roleId}/users/${userId}`);
}

export async function removeUserFromRole(
  roleId: string,
  userId: string
): Promise<void> {
  await apiClient.delete(`${ROLES_BASE}/${roleId}/users/${userId}`);
}
