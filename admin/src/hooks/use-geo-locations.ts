import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  getGeoLocations,
  getGeoLocation,
  getGeoLocationTree,
  getGeoLocationsByType,
  getGeoLocationsByParent,
  createGeoLocation,
  updateGeoLocation,
  deleteGeoLocation,
} from "@/lib/api/geo-locations";
import {
  CreateGeoLocationDto,
  UpdateGeoLocationDto,
  GeoLocationFilters,
} from "@/types/entities";

export const geoLocationKeys = {
  all: ["geoLocations"] as const,
  lists: () => [...geoLocationKeys.all, "list"] as const,
  list: (filters?: GeoLocationFilters) =>
    [...geoLocationKeys.lists(), filters] as const,
  tree: (typeId?: string) => [...geoLocationKeys.all, "tree", typeId] as const,
  byType: (typeId: string) =>
    [...geoLocationKeys.all, "byType", typeId] as const,
  byParent: (parentId?: string) =>
    [...geoLocationKeys.all, "byParent", parentId] as const,
  details: () => [...geoLocationKeys.all, "detail"] as const,
  detail: (id: string) => [...geoLocationKeys.details(), id] as const,
};

export function useGeoLocations(filters?: GeoLocationFilters) {
  return useQuery({
    queryKey: geoLocationKeys.list(filters),
    queryFn: () => getGeoLocations(filters),
  });
}

export function useGeoLocation(id: string) {
  return useQuery({
    queryKey: geoLocationKeys.detail(id),
    queryFn: () => getGeoLocation(id),
    enabled: !!id,
  });
}

export function useGeoLocationTree(typeId?: string) {
  return useQuery({
    queryKey: geoLocationKeys.tree(typeId),
    queryFn: () => getGeoLocationTree(typeId),
  });
}

export function useGeoLocationsByType(typeId: string) {
  return useQuery({
    queryKey: geoLocationKeys.byType(typeId),
    queryFn: () => getGeoLocationsByType(typeId),
    enabled: !!typeId,
  });
}

export function useGeoLocationsByParent(parentId?: string) {
  return useQuery({
    queryKey: geoLocationKeys.byParent(parentId),
    queryFn: () => getGeoLocationsByParent(parentId),
  });
}

export function useCreateGeoLocation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateGeoLocationDto) => createGeoLocation(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: geoLocationKeys.all });
      toast.success("Location created", {
        description: "The location has been created successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to create location.",
      });
    },
  });
}

export function useUpdateGeoLocation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateGeoLocationDto }) =>
      updateGeoLocation(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: geoLocationKeys.all });
      queryClient.invalidateQueries({ queryKey: geoLocationKeys.detail(id) });
      toast.success("Location updated", {
        description: "The location has been updated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to update location.",
      });
    },
  });
}

export function useDeleteGeoLocation() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteGeoLocation(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: geoLocationKeys.all });
      toast.success("Location deleted", {
        description: "The location has been deleted successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to delete location.",
      });
    },
  });
}
