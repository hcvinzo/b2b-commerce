import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  getAdminUsers,
  getAdminUser,
  createAdminUser,
  updateAdminUser,
  activateAdminUser,
  deactivateAdminUser,
  deleteAdminUser,
  resetAdminUserPassword,
  getAvailableRoles,
  getUserRoles,
  setUserRoles,
  getUserLogins,
  getUserClaims,
  addUserClaim,
  removeUserClaim,
} from "@/lib/api/admin-users";
import {
  AdminUserFilters,
  CreateAdminUserDto,
  UpdateAdminUserDto,
  SetUserRolesDto,
  AddUserClaimDto,
} from "@/types/entities";

export const adminUserKeys = {
  all: ["admin-users"] as const,
  lists: () => [...adminUserKeys.all, "list"] as const,
  list: (filters: AdminUserFilters) =>
    [...adminUserKeys.lists(), filters] as const,
  details: () => [...adminUserKeys.all, "detail"] as const,
  detail: (id: string) => [...adminUserKeys.details(), id] as const,
  availableRoles: () => [...adminUserKeys.all, "available-roles"] as const,
  roles: (id: string) => [...adminUserKeys.all, "roles", id] as const,
  logins: (id: string) => [...adminUserKeys.all, "logins", id] as const,
  claims: (id: string) => [...adminUserKeys.all, "claims", id] as const,
};

export function useAdminUsers(filters: AdminUserFilters = {}) {
  return useQuery({
    queryKey: adminUserKeys.list(filters),
    queryFn: () => getAdminUsers(filters),
  });
}

export function useAdminUser(id: string) {
  return useQuery({
    queryKey: adminUserKeys.detail(id),
    queryFn: () => getAdminUser(id),
    enabled: !!id,
  });
}

export function useAvailableRoles() {
  return useQuery({
    queryKey: adminUserKeys.availableRoles(),
    queryFn: () => getAvailableRoles(),
  });
}

export function useCreateAdminUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateAdminUserDto) => createAdminUser(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminUserKeys.all });
      toast.success("Admin user created", {
        description: "The admin user has been created successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to create admin user.",
      });
    },
  });
}

export function useUpdateAdminUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateAdminUserDto }) =>
      updateAdminUser(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: adminUserKeys.all });
      queryClient.invalidateQueries({ queryKey: adminUserKeys.detail(id) });
      toast.success("Admin user updated", {
        description: "The admin user has been updated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to update admin user.",
      });
    },
  });
}

export function useActivateAdminUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => activateAdminUser(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: adminUserKeys.all });
      queryClient.invalidateQueries({ queryKey: adminUserKeys.detail(id) });
      toast.success("Admin user activated", {
        description: "The admin user has been activated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to activate admin user.",
      });
    },
  });
}

export function useDeactivateAdminUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deactivateAdminUser(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: adminUserKeys.all });
      queryClient.invalidateQueries({ queryKey: adminUserKeys.detail(id) });
      toast.success("Admin user deactivated", {
        description: "The admin user has been deactivated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to deactivate admin user.",
      });
    },
  });
}

export function useDeleteAdminUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteAdminUser(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: adminUserKeys.all });
      toast.success("Admin user deleted", {
        description: "The admin user has been deleted successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to delete admin user.",
      });
    },
  });
}

export function useResetAdminUserPassword() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => resetAdminUserPassword(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: adminUserKeys.detail(id) });
      toast.success("Password reset", {
        description: "A password reset email has been sent to the user.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to reset password.",
      });
    },
  });
}

// User Roles
export function useUserRoles(id: string) {
  return useQuery({
    queryKey: adminUserKeys.roles(id),
    queryFn: () => getUserRoles(id),
    enabled: !!id,
  });
}

export function useSetUserRoles() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: SetUserRolesDto }) =>
      setUserRoles(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: adminUserKeys.roles(id) });
      queryClient.invalidateQueries({ queryKey: adminUserKeys.detail(id) });
      queryClient.invalidateQueries({ queryKey: adminUserKeys.lists() });
      toast.success("Roles updated", {
        description: "User roles have been updated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to update roles.",
      });
    },
  });
}

// User Logins
export function useUserLogins(id: string) {
  return useQuery({
    queryKey: adminUserKeys.logins(id),
    queryFn: () => getUserLogins(id),
    enabled: !!id,
  });
}

// User Claims
export function useUserClaims(id: string) {
  return useQuery({
    queryKey: adminUserKeys.claims(id),
    queryFn: () => getUserClaims(id),
    enabled: !!id,
  });
}

export function useAddUserClaim() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: AddUserClaimDto }) =>
      addUserClaim(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: adminUserKeys.claims(id) });
      toast.success("Claim added", {
        description: "The claim has been added successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to add claim.",
      });
    },
  });
}

export function useRemoveUserClaim() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ userId, claimId }: { userId: string; claimId: number }) =>
      removeUserClaim(userId, claimId),
    onSuccess: (_, { userId }) => {
      queryClient.invalidateQueries({ queryKey: adminUserKeys.claims(userId) });
      toast.success("Claim removed", {
        description: "The claim has been removed successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to remove claim.",
      });
    },
  });
}
