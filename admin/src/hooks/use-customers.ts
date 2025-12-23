import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  getCustomers,
  getCustomer,
  approveCustomer,
  activateCustomer,
  deactivateCustomer,
  updateCreditLimit,
  updateCustomer,
  deleteCustomer,
  getCustomerAttributes,
  upsertCustomerAttributes,
  deleteCustomerAttribute,
} from "@/lib/api/customers";
import {
  CustomerFilters,
  UpdateCustomerData,
  CustomerAttributeType,
  UpsertCustomerAttributesDto,
} from "@/types/entities";

export const customerKeys = {
  all: ["customers"] as const,
  lists: () => [...customerKeys.all, "list"] as const,
  list: (filters: CustomerFilters) => [...customerKeys.lists(), filters] as const,
  details: () => [...customerKeys.all, "detail"] as const,
  detail: (id: string) => [...customerKeys.details(), id] as const,
  attributes: (customerId: string) => [...customerKeys.detail(customerId), "attributes"] as const,
  attributesByType: (customerId: string, type: CustomerAttributeType) =>
    [...customerKeys.attributes(customerId), type] as const,
};

export function useCustomers(filters: CustomerFilters) {
  return useQuery({
    queryKey: customerKeys.list(filters),
    queryFn: () => getCustomers(filters),
  });
}

export function useCustomer(id: string) {
  return useQuery({
    queryKey: customerKeys.detail(id),
    queryFn: () => getCustomer(id),
    enabled: !!id,
  });
}

export function useApproveCustomer() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => approveCustomer(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: customerKeys.all });
      toast.success("Customer approved", {
        description: "The customer has been approved successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to approve customer.",
      });
    },
  });
}

export function useActivateCustomer() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => activateCustomer(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: customerKeys.all });
      toast.success("Customer activated", {
        description: "The customer has been activated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to activate customer.",
      });
    },
  });
}

export function useDeactivateCustomer() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deactivateCustomer(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: customerKeys.all });
      toast.success("Customer deactivated", {
        description: "The customer has been deactivated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to deactivate customer.",
      });
    },
  });
}

export function useUpdateCreditLimit() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, creditLimit }: { id: string; creditLimit: number }) =>
      updateCreditLimit(id, creditLimit),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: customerKeys.all });
      queryClient.invalidateQueries({ queryKey: customerKeys.detail(id) });
      toast.success("Credit limit updated", {
        description: "The customer credit limit has been updated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to update credit limit.",
      });
    },
  });
}

export function useUpdateCustomer() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateCustomerData }) =>
      updateCustomer(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: customerKeys.all });
      queryClient.invalidateQueries({ queryKey: customerKeys.detail(id) });
      toast.success("Customer updated", {
        description: "The customer has been updated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to update customer.",
      });
    },
  });
}

export function useDeleteCustomer() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteCustomer(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: customerKeys.all });
      toast.success("Customer deleted", {
        description: "The customer has been deleted successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to delete customer.",
      });
    },
  });
}

// Customer Attributes Hooks
export function useCustomerAttributes(
  customerId: string,
  type?: CustomerAttributeType
) {
  return useQuery({
    queryKey: type
      ? customerKeys.attributesByType(customerId, type)
      : customerKeys.attributes(customerId),
    queryFn: () => getCustomerAttributes(customerId, type),
    enabled: !!customerId,
  });
}

export function useUpsertCustomerAttributes() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      customerId,
      data,
    }: {
      customerId: string;
      data: UpsertCustomerAttributesDto;
    }) => upsertCustomerAttributes(customerId, data),
    onSuccess: (_, { customerId }) => {
      queryClient.invalidateQueries({
        queryKey: customerKeys.attributes(customerId),
      });
      toast.success("Attributes saved", {
        description: "Customer attributes have been saved successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to save customer attributes.",
      });
    },
  });
}

export function useDeleteCustomerAttribute() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      customerId,
      attributeId,
    }: {
      customerId: string;
      attributeId: string;
    }) => deleteCustomerAttribute(customerId, attributeId),
    onSuccess: (_, { customerId }) => {
      queryClient.invalidateQueries({
        queryKey: customerKeys.attributes(customerId),
      });
      toast.success("Attribute deleted", {
        description: "Customer attribute has been deleted successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to delete customer attribute.",
      });
    },
  });
}
