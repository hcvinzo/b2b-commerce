import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  getCollections,
  getCollection,
  createCollection,
  updateCollection,
  deleteCollection,
  activateCollection,
  deactivateCollection,
  getCollectionProducts,
  setCollectionProducts,
  setCollectionFilters,
} from "@/lib/api/collections";
import {
  CollectionFilters,
  CreateCollectionDto,
  UpdateCollectionDto,
  SetCollectionProductsDto,
  SetCollectionFiltersDto,
} from "@/types/entities";

export const collectionKeys = {
  all: ["collections"] as const,
  lists: () => [...collectionKeys.all, "list"] as const,
  list: (filters: CollectionFilters) =>
    [...collectionKeys.lists(), filters] as const,
  details: () => [...collectionKeys.all, "detail"] as const,
  detail: (id: string) => [...collectionKeys.details(), id] as const,
  products: (id: string) => [...collectionKeys.detail(id), "products"] as const,
};

export function useCollections(filters: CollectionFilters) {
  return useQuery({
    queryKey: collectionKeys.list(filters),
    queryFn: () => getCollections(filters),
  });
}

export function useCollection(id: string) {
  return useQuery({
    queryKey: collectionKeys.detail(id),
    queryFn: () => getCollection(id),
    enabled: !!id,
  });
}

export function useCreateCollection() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateCollectionDto) => createCollection(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: collectionKeys.lists() });
      toast.success("Collection created", {
        description: "The collection has been created successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to create collection.",
      });
    },
  });
}

export function useUpdateCollection() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateCollectionDto }) =>
      updateCollection(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: collectionKeys.lists() });
      queryClient.invalidateQueries({ queryKey: collectionKeys.detail(id) });
      toast.success("Collection updated", {
        description: "The collection has been updated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to update collection.",
      });
    },
  });
}

export function useDeleteCollection() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteCollection(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: collectionKeys.lists() });
      toast.success("Collection deleted", {
        description: "The collection has been deleted successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to delete collection.",
      });
    },
  });
}

export function useActivateCollection() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => activateCollection(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: collectionKeys.lists() });
      queryClient.invalidateQueries({ queryKey: collectionKeys.detail(id) });
      toast.success("Collection activated", {
        description: "The collection has been activated.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to activate collection.",
      });
    },
  });
}

export function useDeactivateCollection() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deactivateCollection(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: collectionKeys.lists() });
      queryClient.invalidateQueries({ queryKey: collectionKeys.detail(id) });
      toast.success("Collection deactivated", {
        description: "The collection has been deactivated.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to deactivate collection.",
      });
    },
  });
}

// Collection Products hooks (for Manual collections)

export function useCollectionProducts(
  collectionId: string,
  page = 1,
  pageSize = 20
) {
  return useQuery({
    queryKey: [...collectionKeys.products(collectionId), page, pageSize],
    queryFn: () => getCollectionProducts(collectionId, page, pageSize),
    enabled: !!collectionId,
  });
}

export function useSetCollectionProducts() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      collectionId,
      data,
    }: {
      collectionId: string;
      data: SetCollectionProductsDto;
    }) => setCollectionProducts(collectionId, data),
    onSuccess: (_, { collectionId }) => {
      queryClient.invalidateQueries({
        queryKey: collectionKeys.products(collectionId),
      });
      queryClient.invalidateQueries({
        queryKey: collectionKeys.detail(collectionId),
      });
      toast.success("Products updated", {
        description: "Collection products have been updated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to update collection products.",
      });
    },
  });
}

// Collection Filters hooks (for Dynamic collections)

export function useSetCollectionFilters() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      collectionId,
      data,
    }: {
      collectionId: string;
      data: SetCollectionFiltersDto;
    }) => setCollectionFilters(collectionId, data),
    onSuccess: (_, { collectionId }) => {
      queryClient.invalidateQueries({
        queryKey: collectionKeys.detail(collectionId),
      });
      toast.success("Filters updated", {
        description: "Collection filters have been updated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to update collection filters.",
      });
    },
  });
}
