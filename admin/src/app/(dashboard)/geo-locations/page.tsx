"use client";

import { useState } from "react";
import {
  Plus,
  Search,
  MoreHorizontal,
  Pencil,
  Trash2,
  MapPin,
  RefreshCw,
  Settings,
  ChevronRight,
} from "lucide-react";

import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { Input } from "@/components/ui/input";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { GeoLocationTypeForm } from "@/components/forms/geo-location-type-form";
import { GeoLocationForm } from "@/components/forms/geo-location-form";
import {
  useGeoLocationTypes,
  useAllGeoLocationTypes,
  useCreateGeoLocationType,
  useUpdateGeoLocationType,
  useDeleteGeoLocationType,
} from "@/hooks/use-geo-location-types";
import {
  useGeoLocations,
  useGeoLocationsByType,
  useCreateGeoLocation,
  useUpdateGeoLocation,
  useDeleteGeoLocation,
} from "@/hooks/use-geo-locations";
import { type GeoLocationTypeFormData } from "@/lib/validations/geo-location-type";
import { type GeoLocationFormData } from "@/lib/validations/geo-location";
import {
  type GeoLocationType,
  type GeoLocationListItem,
  type GeoLocationFilters,
  type GeoLocationTypeFilters,
} from "@/types/entities";
import { useDebounce } from "@/hooks/use-debounce";

export default function GeoLocationsPage() {
  const [activeTab, setActiveTab] = useState("locations");

  // Location Types State
  const [typeFilters, setTypeFilters] = useState<GeoLocationTypeFilters>({
    page: 1,
    pageSize: 10,
  });
  const [typeSearchQuery, setTypeSearchQuery] = useState("");
  const debouncedTypeSearch = useDebounce(typeSearchQuery, 300);
  const [isTypeFormOpen, setIsTypeFormOpen] = useState(false);
  const [editingType, setEditingType] = useState<GeoLocationType | null>(null);
  const [deletingType, setDeletingType] = useState<GeoLocationType | null>(null);

  // Locations State
  const [locationFilters, setLocationFilters] = useState<GeoLocationFilters>({
    page: 1,
    pageSize: 10,
  });
  const [locationSearchQuery, setLocationSearchQuery] = useState("");
  const debouncedLocationSearch = useDebounce(locationSearchQuery, 300);
  const [isLocationFormOpen, setIsLocationFormOpen] = useState(false);
  const [editingLocation, setEditingLocation] = useState<GeoLocationListItem | null>(null);
  const [deletingLocation, setDeletingLocation] = useState<GeoLocationListItem | null>(null);
  const [parentTypeId, setParentTypeId] = useState<string | null>(null);

  // Queries
  const { data: typesData, isLoading: typesLoading, isFetching: typesFetching, refetch: refetchTypes } = useGeoLocationTypes({
    ...typeFilters,
    search: debouncedTypeSearch || undefined,
  });
  const { data: allTypes } = useAllGeoLocationTypes();
  const { data: locationsData, isLoading: locationsLoading, isFetching: locationsFetching, refetch: refetchLocations } = useGeoLocations({
    ...locationFilters,
    search: debouncedLocationSearch || undefined,
  });
  // Fetch parent locations based on the parent type (one level up in hierarchy)
  const { data: parentLocations, isLoading: isLoadingParents } = useGeoLocationsByType(parentTypeId || "");

  // Mutations - Types
  const createType = useCreateGeoLocationType();
  const updateType = useUpdateGeoLocationType();
  const deleteType = useDeleteGeoLocationType();

  // Mutations - Locations
  const createLocation = useCreateGeoLocation();
  const updateLocation = useUpdateGeoLocation();
  const deleteLocation = useDeleteGeoLocation();

  // Type handlers
  const handleAddNewType = () => {
    setEditingType(null);
    setIsTypeFormOpen(true);
  };

  const handleEditType = (type: GeoLocationType) => {
    setEditingType(type);
    setIsTypeFormOpen(true);
  };

  const handleTypeFormSubmit = async (formData: GeoLocationTypeFormData) => {
    if (editingType) {
      await updateType.mutateAsync({
        id: editingType.id,
        data: {
          name: formData.name,
          displayOrder: formData.displayOrder,
        },
      });
    } else {
      await createType.mutateAsync({
        name: formData.name,
        displayOrder: formData.displayOrder,
      });
    }
    setIsTypeFormOpen(false);
    setEditingType(null);
  };

  const handleDeleteType = async () => {
    if (deletingType) {
      await deleteType.mutateAsync(deletingType.id);
      setDeletingType(null);
    }
  };

  // Helper to find parent type based on selected type's displayOrder
  const handleFormTypeChange = (typeId: string) => {
    const selectedType = allTypes?.find((t) => t.id === typeId);
    if (selectedType && selectedType.displayOrder > 0) {
      // Find the type with displayOrder - 1 (parent level)
      const parentType = allTypes?.find(
        (t) => t.displayOrder === selectedType.displayOrder - 1
      );
      setParentTypeId(parentType?.id || null);
    } else {
      setParentTypeId(null);
    }
  };

  // Location handlers
  const handleAddNewLocation = () => {
    setEditingLocation(null);
    setParentTypeId(null);
    setIsLocationFormOpen(true);
  };

  const handleEditLocation = (location: GeoLocationListItem) => {
    setEditingLocation(location);
    setIsLocationFormOpen(true);
  };

  const handleLocationFormSubmit = async (formData: GeoLocationFormData) => {
    if (editingLocation) {
      await updateLocation.mutateAsync({
        id: editingLocation.id,
        data: {
          code: formData.code,
          name: formData.name,
          latitude: formData.latitude ?? undefined,
          longitude: formData.longitude ?? undefined,
          metadata: formData.metadata || undefined,
        },
      });
    } else {
      await createLocation.mutateAsync({
        geoLocationTypeId: formData.geoLocationTypeId,
        code: formData.code,
        name: formData.name,
        parentId: formData.parentId || undefined,
        latitude: formData.latitude ?? undefined,
        longitude: formData.longitude ?? undefined,
        metadata: formData.metadata || undefined,
      });
    }
    setIsLocationFormOpen(false);
    setEditingLocation(null);
  };

  const handleDeleteLocation = async () => {
    if (deletingLocation) {
      await deleteLocation.mutateAsync(deletingLocation.id);
      setDeletingLocation(null);
    }
  };

  const handleTypeFilterChange = (value: string) => {
    setLocationFilters((prev) => ({
      ...prev,
      typeId: value === "all" ? undefined : value,
      page: 1,
    }));
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Geographic Locations</h1>
          <p className="text-muted-foreground">
            Manage location types and geographic hierarchy
          </p>
        </div>
      </div>

      <Tabs value={activeTab} onValueChange={setActiveTab}>
        <TabsList>
          <TabsTrigger value="locations" className="gap-2">
            <MapPin className="h-4 w-4" />
            Locations
          </TabsTrigger>
          <TabsTrigger value="types" className="gap-2">
            <Settings className="h-4 w-4" />
            Location Types
          </TabsTrigger>
        </TabsList>

        {/* Locations Tab */}
        <TabsContent value="locations" className="mt-6">
          <Card>
            <CardHeader className="pb-4">
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle>All Locations</CardTitle>
                  <CardDescription>
                    Manage geographic locations in your hierarchy
                  </CardDescription>
                </div>
                <Button onClick={handleAddNewLocation}>
                  <Plus className="mr-2 h-4 w-4" />
                  Add Location
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              {/* Filters */}
              <div className="flex flex-col gap-4 sm:flex-row sm:items-center mb-6">
                <div className="relative flex-1">
                  <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                  <Input
                    placeholder="Search locations..."
                    value={locationSearchQuery}
                    onChange={(e) => setLocationSearchQuery(e.target.value)}
                    className="pl-9"
                  />
                </div>
                <Select
                  value={locationFilters.typeId || "all"}
                  onValueChange={handleTypeFilterChange}
                >
                  <SelectTrigger className="w-[180px]">
                    <SelectValue placeholder="Location Type" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">All Types</SelectItem>
                    {allTypes?.map((type) => (
                      <SelectItem key={type.id} value={type.id}>
                        {type.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
                <Button
                  variant="outline"
                  size="icon"
                  onClick={() => refetchLocations()}
                  disabled={locationsFetching}
                >
                  <RefreshCw className={`h-4 w-4 ${locationsFetching ? "animate-spin" : ""}`} />
                </Button>
              </div>

              {/* Locations Table */}
              {locationsLoading ? (
                <div className="space-y-3">
                  {[...Array(5)].map((_, i) => (
                    <Skeleton key={i} className="h-16 w-full" />
                  ))}
                </div>
              ) : locationsData?.items && locationsData.items.length > 0 ? (
                <div className="rounded-md border">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Code</TableHead>
                        <TableHead>Name</TableHead>
                        <TableHead>Type</TableHead>
                        <TableHead>Parent</TableHead>
                        <TableHead className="text-center">Coordinates</TableHead>
                        <TableHead className="w-[70px]"></TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {locationsData.items.map((location) => (
                        <TableRow key={location.id}>
                          <TableCell className="font-mono text-sm">
                            {location.code}
                          </TableCell>
                          <TableCell className="font-medium">
                            {location.name}
                          </TableCell>
                          <TableCell>
                            <Badge variant="outline">
                              {location.geoLocationTypeName}
                            </Badge>
                          </TableCell>
                          <TableCell>
                            {location.parentName ? (
                              <span className="flex items-center gap-1 text-muted-foreground">
                                <ChevronRight className="h-3 w-3" />
                                {location.parentName}
                              </span>
                            ) : (
                              <span className="text-muted-foreground">-</span>
                            )}
                          </TableCell>
                          <TableCell className="text-center">
                            {location.latitude && location.longitude ? (
                              <span className="text-xs text-muted-foreground font-mono">
                                {location.latitude.toFixed(4)}, {location.longitude.toFixed(4)}
                              </span>
                            ) : (
                              <span className="text-muted-foreground">-</span>
                            )}
                          </TableCell>
                          <TableCell>
                            <DropdownMenu>
                              <DropdownMenuTrigger asChild>
                                <Button variant="ghost" size="icon">
                                  <MoreHorizontal className="h-4 w-4" />
                                </Button>
                              </DropdownMenuTrigger>
                              <DropdownMenuContent align="end">
                                <DropdownMenuItem onClick={() => handleEditLocation(location)}>
                                  <Pencil className="mr-2 h-4 w-4" />
                                  Edit
                                </DropdownMenuItem>
                                <DropdownMenuSeparator />
                                <DropdownMenuItem
                                  className="text-destructive"
                                  onClick={() => setDeletingLocation(location)}
                                >
                                  <Trash2 className="mr-2 h-4 w-4" />
                                  Delete
                                </DropdownMenuItem>
                              </DropdownMenuContent>
                            </DropdownMenu>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
              ) : (
                <div className="text-center py-10">
                  <MapPin className="mx-auto h-12 w-12 text-muted-foreground" />
                  <h3 className="mt-4 text-lg font-semibold">No locations</h3>
                  <p className="text-muted-foreground">
                    {locationSearchQuery
                      ? `No locations match "${locationSearchQuery}"`
                      : "Get started by creating your first location."}
                  </p>
                  {!locationSearchQuery && (
                    <Button className="mt-4" onClick={handleAddNewLocation}>
                      <Plus className="mr-2 h-4 w-4" />
                      Add Location
                    </Button>
                  )}
                </div>
              )}

              {/* Pagination */}
              {locationsData && locationsData.totalCount > 0 && (
                <div className="flex items-center justify-between mt-4 text-sm text-muted-foreground">
                  <div>
                    Showing {(locationsData.pageNumber - 1) * locationsData.pageSize + 1} to{" "}
                    {Math.min(locationsData.pageNumber * locationsData.pageSize, locationsData.totalCount)} of{" "}
                    {locationsData.totalCount} locations
                  </div>
                  <div className="flex gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      disabled={!locationsData.hasPreviousPage}
                      onClick={() =>
                        setLocationFilters((prev) => ({
                          ...prev,
                          page: (prev.page || 1) - 1,
                        }))
                      }
                    >
                      Previous
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      disabled={!locationsData.hasNextPage}
                      onClick={() =>
                        setLocationFilters((prev) => ({
                          ...prev,
                          page: (prev.page || 1) + 1,
                        }))
                      }
                    >
                      Next
                    </Button>
                  </div>
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        {/* Types Tab */}
        <TabsContent value="types" className="mt-6">
          <Card>
            <CardHeader className="pb-4">
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle>Location Types</CardTitle>
                  <CardDescription>
                    Define the types of locations in your hierarchy (e.g., Country, State, City)
                  </CardDescription>
                </div>
                <Button onClick={handleAddNewType}>
                  <Plus className="mr-2 h-4 w-4" />
                  Add Type
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              {/* Filters */}
              <div className="flex flex-col gap-4 sm:flex-row sm:items-center mb-6">
                <div className="relative flex-1">
                  <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                  <Input
                    placeholder="Search location types..."
                    value={typeSearchQuery}
                    onChange={(e) => setTypeSearchQuery(e.target.value)}
                    className="pl-9"
                  />
                </div>
                <Button
                  variant="outline"
                  size="icon"
                  onClick={() => refetchTypes()}
                  disabled={typesFetching}
                >
                  <RefreshCw className={`h-4 w-4 ${typesFetching ? "animate-spin" : ""}`} />
                </Button>
              </div>

              {/* Types Table */}
              {typesLoading ? (
                <div className="space-y-3">
                  {[...Array(5)].map((_, i) => (
                    <Skeleton key={i} className="h-16 w-full" />
                  ))}
                </div>
              ) : typesData?.items && typesData.items.length > 0 ? (
                <div className="rounded-md border">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>Name</TableHead>
                        <TableHead className="text-center">Display Order</TableHead>
                        <TableHead className="text-center">Locations</TableHead>
                        <TableHead className="w-[70px]"></TableHead>
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {typesData.items.map((type) => (
                        <TableRow key={type.id}>
                          <TableCell className="font-medium">
                            {type.name}
                          </TableCell>
                          <TableCell className="text-center">
                            <Badge variant="secondary">
                              {type.displayOrder}
                            </Badge>
                          </TableCell>
                          <TableCell className="text-center">
                            <Badge variant="outline">
                              {type.locationCount} locations
                            </Badge>
                          </TableCell>
                          <TableCell>
                            <DropdownMenu>
                              <DropdownMenuTrigger asChild>
                                <Button variant="ghost" size="icon">
                                  <MoreHorizontal className="h-4 w-4" />
                                </Button>
                              </DropdownMenuTrigger>
                              <DropdownMenuContent align="end">
                                <DropdownMenuItem onClick={() => handleEditType(type)}>
                                  <Pencil className="mr-2 h-4 w-4" />
                                  Edit
                                </DropdownMenuItem>
                                <DropdownMenuSeparator />
                                <DropdownMenuItem
                                  className="text-destructive"
                                  onClick={() => setDeletingType(type)}
                                >
                                  <Trash2 className="mr-2 h-4 w-4" />
                                  Delete
                                </DropdownMenuItem>
                              </DropdownMenuContent>
                            </DropdownMenu>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
              ) : (
                <div className="text-center py-10">
                  <Settings className="mx-auto h-12 w-12 text-muted-foreground" />
                  <h3 className="mt-4 text-lg font-semibold">No location types</h3>
                  <p className="text-muted-foreground">
                    {typeSearchQuery
                      ? `No types match "${typeSearchQuery}"`
                      : "Get started by creating your first location type."}
                  </p>
                  {!typeSearchQuery && (
                    <Button className="mt-4" onClick={handleAddNewType}>
                      <Plus className="mr-2 h-4 w-4" />
                      Add Type
                    </Button>
                  )}
                </div>
              )}

              {/* Pagination */}
              {typesData && typesData.totalCount > 0 && (
                <div className="flex items-center justify-between mt-4 text-sm text-muted-foreground">
                  <div>
                    Showing {(typesData.pageNumber - 1) * typesData.pageSize + 1} to{" "}
                    {Math.min(typesData.pageNumber * typesData.pageSize, typesData.totalCount)} of{" "}
                    {typesData.totalCount} types
                  </div>
                  <div className="flex gap-2">
                    <Button
                      variant="outline"
                      size="sm"
                      disabled={!typesData.hasPreviousPage}
                      onClick={() =>
                        setTypeFilters((prev) => ({
                          ...prev,
                          page: (prev.page || 1) - 1,
                        }))
                      }
                    >
                      Previous
                    </Button>
                    <Button
                      variant="outline"
                      size="sm"
                      disabled={!typesData.hasNextPage}
                      onClick={() =>
                        setTypeFilters((prev) => ({
                          ...prev,
                          page: (prev.page || 1) + 1,
                        }))
                      }
                    >
                      Next
                    </Button>
                  </div>
                </div>
              )}
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Location Type Form Dialog */}
      <Dialog open={isTypeFormOpen} onOpenChange={setIsTypeFormOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>
              {editingType ? "Edit Location Type" : "Add Location Type"}
            </DialogTitle>
            <DialogDescription>
              {editingType
                ? "Update the location type information below."
                : "Fill in the details to create a new location type."}
            </DialogDescription>
          </DialogHeader>
          <GeoLocationTypeForm
            defaultValues={
              editingType
                ? {
                    name: editingType.name,
                    displayOrder: editingType.displayOrder,
                  }
                : undefined
            }
            onSubmit={handleTypeFormSubmit}
            onCancel={() => setIsTypeFormOpen(false)}
            isLoading={createType.isPending || updateType.isPending}
          />
        </DialogContent>
      </Dialog>

      {/* Location Form Dialog */}
      <Dialog open={isLocationFormOpen} onOpenChange={setIsLocationFormOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>
              {editingLocation ? "Edit Location" : "Add Location"}
            </DialogTitle>
            <DialogDescription>
              {editingLocation
                ? "Update the location information below."
                : "Fill in the details to create a new location."}
            </DialogDescription>
          </DialogHeader>
          <GeoLocationForm
            defaultValues={
              editingLocation
                ? {
                    geoLocationTypeId: editingLocation.geoLocationTypeId,
                    code: editingLocation.code,
                    name: editingLocation.name,
                    parentId: editingLocation.parentId,
                    latitude: editingLocation.latitude ?? undefined,
                    longitude: editingLocation.longitude ?? undefined,
                  }
                : undefined
            }
            geoLocationTypes={allTypes || []}
            parentLocations={parentLocations || []}
            onSubmit={handleLocationFormSubmit}
            onCancel={() => setIsLocationFormOpen(false)}
            onTypeChange={handleFormTypeChange}
            isLoading={createLocation.isPending || updateLocation.isPending}
            isLoadingParents={isLoadingParents}
            isEdit={!!editingLocation}
          />
        </DialogContent>
      </Dialog>

      {/* Delete Type Confirmation */}
      <ConfirmDialog
        open={!!deletingType}
        onOpenChange={(open) => !open && setDeletingType(null)}
        title="Delete Location Type"
        description={`Are you sure you want to delete "${deletingType?.name}"? This action cannot be undone. Any locations using this type should be reassigned first.`}
        confirmText="Delete"
        variant="destructive"
        onConfirm={handleDeleteType}
        isLoading={deleteType.isPending}
      />

      {/* Delete Location Confirmation */}
      <ConfirmDialog
        open={!!deletingLocation}
        onOpenChange={(open) => !open && setDeletingLocation(null)}
        title="Delete Location"
        description={`Are you sure you want to delete "${deletingLocation?.name}"? This action cannot be undone. Any child locations will also be affected.`}
        confirmText="Delete"
        variant="destructive"
        onConfirm={handleDeleteLocation}
        isLoading={deleteLocation.isPending}
      />
    </div>
  );
}
