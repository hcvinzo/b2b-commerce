import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  getRoles,
  getRole,
  createRole,
  updateRole,
  deleteRole,
  getAvailablePermissions,
  getRoleClaims,
  setRoleClaims,
  addClaimToRole,
  removeClaimFromRole,
  getUsersInRole,
  addUserToRole,
  removeUserFromRole,
} from "@/lib/api/roles";
import {
  RoleFilters,
  CreateRoleDto,
  UpdateRoleDto,
  SetRoleClaimsDto,
} from "@/types/entities";

export const roleKeys = {
  all: ["roles"] as const,
  lists: () => [...roleKeys.all, "list"] as const,
  list: (filters: RoleFilters) => [...roleKeys.lists(), filters] as const,
  details: () => [...roleKeys.all, "detail"] as const,
  detail: (id: string) => [...roleKeys.details(), id] as const,
  permissions: () => [...roleKeys.all, "permissions"] as const,
  claims: (roleId: string) => [...roleKeys.all, "claims", roleId] as const,
  users: (roleId: string) => [...roleKeys.all, "users", roleId] as const,
};

// ============================================
// Role CRUD Hooks
// ============================================

export function useRoles(filters: RoleFilters = {}) {
  return useQuery({
    queryKey: roleKeys.list(filters),
    queryFn: () => getRoles(filters),
  });
}

export function useRole(id: string) {
  return useQuery({
    queryKey: roleKeys.detail(id),
    queryFn: () => getRole(id),
    enabled: !!id,
  });
}

export function useCreateRole() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateRoleDto) => createRole(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: roleKeys.all });
      toast.success("Role created", {
        description: "The role has been created successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to create role.",
      });
    },
  });
}

export function useUpdateRole() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateRoleDto }) =>
      updateRole(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: roleKeys.all });
      queryClient.invalidateQueries({ queryKey: roleKeys.detail(id) });
      toast.success("Role updated", {
        description: "The role has been updated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to update role.",
      });
    },
  });
}

export function useDeleteRole() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteRole(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: roleKeys.all });
      toast.success("Role deleted", {
        description: "The role has been deleted successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to delete role.",
      });
    },
  });
}

// ============================================
// Permission Hooks
// ============================================

export function useAvailablePermissions() {
  return useQuery({
    queryKey: roleKeys.permissions(),
    queryFn: () => getAvailablePermissions(),
  });
}

export function useRoleClaims(roleId: string) {
  return useQuery({
    queryKey: roleKeys.claims(roleId),
    queryFn: () => getRoleClaims(roleId),
    enabled: !!roleId,
  });
}

export function useSetRoleClaims() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ roleId, data }: { roleId: string; data: SetRoleClaimsDto }) =>
      setRoleClaims(roleId, data),
    onSuccess: (_, { roleId }) => {
      queryClient.invalidateQueries({ queryKey: roleKeys.all });
      queryClient.invalidateQueries({ queryKey: roleKeys.detail(roleId) });
      queryClient.invalidateQueries({ queryKey: roleKeys.claims(roleId) });
      toast.success("Permissions updated", {
        description: "The role permissions have been updated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to update permissions.",
      });
    },
  });
}

export function useAddClaimToRole() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ roleId, claimValue }: { roleId: string; claimValue: string }) =>
      addClaimToRole(roleId, claimValue),
    onSuccess: (_, { roleId }) => {
      queryClient.invalidateQueries({ queryKey: roleKeys.all });
      queryClient.invalidateQueries({ queryKey: roleKeys.detail(roleId) });
      queryClient.invalidateQueries({ queryKey: roleKeys.claims(roleId) });
      toast.success("Permission added", {
        description: "The permission has been added to the role.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to add permission.",
      });
    },
  });
}

export function useRemoveClaimFromRole() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ roleId, claimValue }: { roleId: string; claimValue: string }) =>
      removeClaimFromRole(roleId, claimValue),
    onSuccess: (_, { roleId }) => {
      queryClient.invalidateQueries({ queryKey: roleKeys.all });
      queryClient.invalidateQueries({ queryKey: roleKeys.detail(roleId) });
      queryClient.invalidateQueries({ queryKey: roleKeys.claims(roleId) });
      toast.success("Permission removed", {
        description: "The permission has been removed from the role.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to remove permission.",
      });
    },
  });
}

// ============================================
// Role User Hooks
// ============================================

export function useRoleUsers(roleId: string, page: number = 1, pageSize: number = 10) {
  return useQuery({
    queryKey: [...roleKeys.users(roleId), page, pageSize],
    queryFn: () => getUsersInRole(roleId, page, pageSize),
    enabled: !!roleId,
  });
}

export function useAddUserToRole() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ roleId, userId }: { roleId: string; userId: string }) =>
      addUserToRole(roleId, userId),
    onSuccess: (_, { roleId }) => {
      queryClient.invalidateQueries({ queryKey: roleKeys.all });
      queryClient.invalidateQueries({ queryKey: roleKeys.detail(roleId) });
      queryClient.invalidateQueries({ queryKey: roleKeys.users(roleId) });
      toast.success("User added", {
        description: "The user has been added to the role.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to add user to role.",
      });
    },
  });
}

export function useRemoveUserFromRole() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ roleId, userId }: { roleId: string; userId: string }) =>
      removeUserFromRole(roleId, userId),
    onSuccess: (_, { roleId }) => {
      queryClient.invalidateQueries({ queryKey: roleKeys.all });
      queryClient.invalidateQueries({ queryKey: roleKeys.detail(roleId) });
      queryClient.invalidateQueries({ queryKey: roleKeys.users(roleId) });
      toast.success("User removed", {
        description: "The user has been removed from the role.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to remove user from role.",
      });
    },
  });
}
