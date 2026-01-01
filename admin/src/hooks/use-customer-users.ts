import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  getCustomerUsers,
  getCustomerUser,
  createCustomerUser,
  updateCustomerUser,
  activateCustomerUser,
  deactivateCustomerUser,
  setCustomerUserRoles,
  getAvailableCustomerRoles,
} from "@/lib/api/customer-users";
import {
  CustomerUserFilters,
  CreateCustomerUserDto,
  UpdateCustomerUserDto,
  SetCustomerUserRolesDto,
} from "@/types/entities";

export const customerUserKeys = {
  all: ["customer-users"] as const,
  lists: () => [...customerUserKeys.all, "list"] as const,
  list: (customerId: string, filters: CustomerUserFilters) =>
    [...customerUserKeys.lists(), customerId, filters] as const,
  details: () => [...customerUserKeys.all, "detail"] as const,
  detail: (customerId: string, userId: string) =>
    [...customerUserKeys.details(), customerId, userId] as const,
  availableRoles: () => [...customerUserKeys.all, "available-roles"] as const,
};

export function useCustomerUsers(
  customerId: string,
  filters: CustomerUserFilters = {}
) {
  return useQuery({
    queryKey: customerUserKeys.list(customerId, filters),
    queryFn: () => getCustomerUsers(customerId, filters),
    enabled: !!customerId,
  });
}

export function useCustomerUser(customerId: string, userId: string) {
  return useQuery({
    queryKey: customerUserKeys.detail(customerId, userId),
    queryFn: () => getCustomerUser(customerId, userId),
    enabled: !!customerId && !!userId,
  });
}

export function useAvailableCustomerRoles() {
  return useQuery({
    queryKey: customerUserKeys.availableRoles(),
    queryFn: () => getAvailableCustomerRoles(),
  });
}

export function useCreateCustomerUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      customerId,
      data,
    }: {
      customerId: string;
      data: CreateCustomerUserDto;
    }) => createCustomerUser(customerId, data),
    onSuccess: (_, { customerId }) => {
      queryClient.invalidateQueries({
        queryKey: customerUserKeys.lists(),
      });
      toast.success("User created", {
        description: "The customer user has been created successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to create customer user.",
      });
    },
  });
}

export function useUpdateCustomerUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      customerId,
      userId,
      data,
    }: {
      customerId: string;
      userId: string;
      data: UpdateCustomerUserDto;
    }) => updateCustomerUser(customerId, userId, data),
    onSuccess: (_, { customerId, userId }) => {
      queryClient.invalidateQueries({
        queryKey: customerUserKeys.lists(),
      });
      queryClient.invalidateQueries({
        queryKey: customerUserKeys.detail(customerId, userId),
      });
      toast.success("User updated", {
        description: "The customer user has been updated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to update customer user.",
      });
    },
  });
}

export function useActivateCustomerUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      customerId,
      userId,
    }: {
      customerId: string;
      userId: string;
    }) => activateCustomerUser(customerId, userId),
    onSuccess: (_, { customerId, userId }) => {
      queryClient.invalidateQueries({
        queryKey: customerUserKeys.lists(),
      });
      queryClient.invalidateQueries({
        queryKey: customerUserKeys.detail(customerId, userId),
      });
      toast.success("User activated", {
        description: "The customer user has been activated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to activate customer user.",
      });
    },
  });
}

export function useDeactivateCustomerUser() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      customerId,
      userId,
    }: {
      customerId: string;
      userId: string;
    }) => deactivateCustomerUser(customerId, userId),
    onSuccess: (_, { customerId, userId }) => {
      queryClient.invalidateQueries({
        queryKey: customerUserKeys.lists(),
      });
      queryClient.invalidateQueries({
        queryKey: customerUserKeys.detail(customerId, userId),
      });
      toast.success("User deactivated", {
        description: "The customer user has been deactivated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to deactivate customer user.",
      });
    },
  });
}

export function useSetCustomerUserRoles() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      customerId,
      userId,
      data,
    }: {
      customerId: string;
      userId: string;
      data: SetCustomerUserRolesDto;
    }) => setCustomerUserRoles(customerId, userId, data),
    onSuccess: (_, { customerId, userId }) => {
      queryClient.invalidateQueries({
        queryKey: customerUserKeys.lists(),
      });
      queryClient.invalidateQueries({
        queryKey: customerUserKeys.detail(customerId, userId),
      });
      toast.success("Roles updated", {
        description: "Customer user roles have been updated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to update customer user roles.",
      });
    },
  });
}
