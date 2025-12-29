import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { toast } from "sonner";
import {
  getGeoLocationTypes,
  getAllGeoLocationTypes,
  getGeoLocationType,
  createGeoLocationType,
  updateGeoLocationType,
  deleteGeoLocationType,
} from "@/lib/api/geo-location-types";
import {
  CreateGeoLocationTypeDto,
  UpdateGeoLocationTypeDto,
  GeoLocationTypeFilters,
} from "@/types/entities";

export const geoLocationTypeKeys = {
  all: ["geoLocationTypes"] as const,
  lists: () => [...geoLocationTypeKeys.all, "list"] as const,
  list: (filters?: GeoLocationTypeFilters) =>
    [...geoLocationTypeKeys.lists(), filters] as const,
  details: () => [...geoLocationTypeKeys.all, "detail"] as const,
  detail: (id: string) => [...geoLocationTypeKeys.details(), id] as const,
};

export function useGeoLocationTypes(filters?: GeoLocationTypeFilters) {
  return useQuery({
    queryKey: geoLocationTypeKeys.list(filters),
    queryFn: () => getGeoLocationTypes(filters),
  });
}

export function useAllGeoLocationTypes() {
  return useQuery({
    queryKey: [...geoLocationTypeKeys.all, "all"],
    queryFn: getAllGeoLocationTypes,
  });
}

export function useGeoLocationType(id: string) {
  return useQuery({
    queryKey: geoLocationTypeKeys.detail(id),
    queryFn: () => getGeoLocationType(id),
    enabled: !!id,
  });
}

export function useCreateGeoLocationType() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (data: CreateGeoLocationTypeDto) => createGeoLocationType(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: geoLocationTypeKeys.all });
      toast.success("Location type created", {
        description: "The location type has been created successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to create location type.",
      });
    },
  });
}

export function useUpdateGeoLocationType() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateGeoLocationTypeDto }) =>
      updateGeoLocationType(id, data),
    onSuccess: (_, { id }) => {
      queryClient.invalidateQueries({ queryKey: geoLocationTypeKeys.all });
      queryClient.invalidateQueries({
        queryKey: geoLocationTypeKeys.detail(id),
      });
      toast.success("Location type updated", {
        description: "The location type has been updated successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to update location type.",
      });
    },
  });
}

export function useDeleteGeoLocationType() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => deleteGeoLocationType(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: geoLocationTypeKeys.all });
      toast.success("Location type deleted", {
        description: "The location type has been deleted successfully.",
      });
    },
    onError: (error: Error) => {
      toast.error("Error", {
        description: error.message || "Failed to delete location type.",
      });
    },
  });
}
