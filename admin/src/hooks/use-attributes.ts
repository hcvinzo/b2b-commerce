import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  getAttributes,
  getAttribute,
  createAttribute,
  updateAttribute,
  deleteAttribute,
  addAttributeValue,
  removeAttributeValue,
  getChildAttributes,
  GetAttributesParams,
} from "@/lib/api/attributes";
import {
  AttributeEntityType,
  CreateAttributeDefinitionDto,
  UpdateAttributeDefinitionDto,
  CreateAttributeValueDto,
} from "@/types/entities";

// Query keys for cache management
export const attributeKeys = {
  all: ["attributes"] as const,
  lists: () => [...attributeKeys.all, "list"] as const,
  listByEntityType: (entityType?: AttributeEntityType) =>
    [...attributeKeys.lists(), "entityType", entityType] as const,
  details: () => [...attributeKeys.all, "detail"] as const,
  detail: (id: string) => [...attributeKeys.details(), id] as const,
  children: (parentId: string) => [...attributeKeys.all, "children", parentId] as const,
};

// Query: Get all attributes
export function useAttributes(entityType?: AttributeEntityType, includeValues: boolean = false) {
  return useQuery({
    queryKey: [...attributeKeys.listByEntityType(entityType), { includeValues }],
    queryFn: () => getAttributes({ includeValues, entityType }),
  });
}

// Query: Get single attribute by ID (with values - used for detail view)
export function useAttribute(id: string | null) {
  return useQuery({
    queryKey: attributeKeys.detail(id ?? ""),
    queryFn: () => getAttribute(id!),
    enabled: !!id,
  });
}

// Mutation: Create attribute
export function useCreateAttribute() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateAttributeDefinitionDto) => createAttribute(data),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: attributeKeys.all });
      toast.success("Attribute created", {
        description: `"${data.name}" has been created successfully.`,
      });
    },
    onError: (error: Error) => {
      toast.error("Failed to create attribute", {
        description: error.message,
      });
    },
  });
}

// Mutation: Update attribute
export function useUpdateAttribute() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      id,
      data,
    }: {
      id: string;
      data: UpdateAttributeDefinitionDto;
    }) => updateAttribute(id, data),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: attributeKeys.all });
      toast.success("Attribute updated", {
        description: `"${data.name}" has been updated successfully.`,
      });
    },
    onError: (error: Error) => {
      toast.error("Failed to update attribute", {
        description: error.message,
      });
    },
  });
}

// Mutation: Delete attribute
export function useDeleteAttribute() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteAttribute(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: attributeKeys.all });
      toast.success("Attribute deleted", {
        description: "The attribute has been deleted successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Failed to delete attribute", {
        description: error.message,
      });
    },
  });
}

// Mutation: Add value to attribute
export function useAddAttributeValue() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      attributeId,
      data,
    }: {
      attributeId: string;
      data: CreateAttributeValueDto;
    }) => addAttributeValue(attributeId, data),
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: attributeKeys.all });
      toast.success("Value added", {
        description: `Value has been added to "${data.name}".`,
      });
    },
    onError: (error: Error) => {
      toast.error("Failed to add value", {
        description: error.message,
      });
    },
  });
}

// Mutation: Remove value from attribute
export function useRemoveAttributeValue() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      attributeId,
      valueId,
    }: {
      attributeId: string;
      valueId: string;
    }) => removeAttributeValue(attributeId, valueId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: attributeKeys.all });
      toast.success("Value removed", {
        description: "The value has been removed successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Failed to remove value", {
        description: error.message,
      });
    },
  });
}

// Query: Get child attributes for a composite parent
export function useChildAttributes(parentId: string | null) {
  return useQuery({
    queryKey: attributeKeys.children(parentId ?? ""),
    queryFn: () => getChildAttributes(parentId!),
    enabled: !!parentId,
  });
}
