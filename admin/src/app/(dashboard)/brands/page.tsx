"use client";

import { useState } from "react";
import {
  Plus,
  Search,
  MoreHorizontal,
  Pencil,
  Trash2,
  Tag,
  ExternalLink,
  Power,
  PowerOff,
  RefreshCw,
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
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
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
import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { BrandForm } from "@/components/forms/brand-form";
import {
  useBrands,
  useCreateBrand,
  useUpdateBrand,
  useDeleteBrand,
  useActivateBrand,
  useDeactivateBrand,
} from "@/hooks/use-brands";
import { type BrandFormData } from "@/lib/validations/brand";
import { type BrandListItem, type BrandFilters } from "@/lib/api/brands";
import { useDebounce } from "@/hooks/use-debounce";

export default function BrandsPage() {
  const [filters, setFilters] = useState<BrandFilters>({
    pageNumber: 1,
    pageSize: 10,
  });
  const [searchQuery, setSearchQuery] = useState("");
  const debouncedSearch = useDebounce(searchQuery, 300);

  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingBrand, setEditingBrand] = useState<BrandListItem | null>(null);
  const [deletingBrand, setDeletingBrand] = useState<BrandListItem | null>(null);

  const { data, isLoading, isFetching, refetch } = useBrands({
    ...filters,
    search: debouncedSearch || undefined,
  });
  const createBrand = useCreateBrand();
  const updateBrand = useUpdateBrand();
  const deleteBrand = useDeleteBrand();
  const activateBrand = useActivateBrand();
  const deactivateBrand = useDeactivateBrand();

  const handleAddNew = () => {
    setEditingBrand(null);
    setIsFormOpen(true);
  };

  const handleEdit = (brand: BrandListItem) => {
    setEditingBrand(brand);
    setIsFormOpen(true);
  };

  const handleFormSubmit = async (formData: BrandFormData) => {
    if (editingBrand) {
      await updateBrand.mutateAsync({
        id: editingBrand.id,
        data: {
          name: formData.name,
          description: formData.description || undefined,
          logoUrl: formData.logoUrl || undefined,
          websiteUrl: formData.websiteUrl || undefined,
          isActive: formData.isActive,
        },
      });
    } else {
      await createBrand.mutateAsync({
        name: formData.name,
        description: formData.description || undefined,
        logoUrl: formData.logoUrl || undefined,
        websiteUrl: formData.websiteUrl || undefined,
        isActive: formData.isActive,
      });
    }
    setIsFormOpen(false);
    setEditingBrand(null);
  };

  const handleDelete = async () => {
    if (deletingBrand) {
      await deleteBrand.mutateAsync(deletingBrand.id);
      setDeletingBrand(null);
    }
  };

  const handleToggleActive = async (brand: BrandListItem) => {
    if (brand.isActive) {
      await deactivateBrand.mutateAsync(brand.id);
    } else {
      await activateBrand.mutateAsync(brand.id);
    }
  };

  const handleStatusFilterChange = (value: string) => {
    setFilters((prev) => ({
      ...prev,
      isActive: value === "all" ? undefined : value === "active",
      pageNumber: 1,
    }));
  };

  const getInitials = (name: string) => {
    return name
      .split(" ")
      .map((word) => word[0])
      .join("")
      .toUpperCase()
      .slice(0, 2);
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Brands</h1>
          <p className="text-muted-foreground">
            Manage product brands and manufacturers
          </p>
        </div>
        <Button onClick={handleAddNew}>
          <Plus className="mr-2 h-4 w-4" />
          Add Brand
        </Button>
      </div>

      <Card>
        <CardHeader className="pb-4">
          <CardTitle>All Brands</CardTitle>
          <CardDescription>
            A list of all brands in your catalog
          </CardDescription>
        </CardHeader>
        <CardContent>
          {/* Filters */}
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center mb-6">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
              <Input
                placeholder="Search brands..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-9"
              />
            </div>
            <Select
              value={
                filters.isActive === undefined
                  ? "all"
                  : filters.isActive
                    ? "active"
                    : "inactive"
              }
              onValueChange={handleStatusFilterChange}
            >
              <SelectTrigger className="w-[150px]">
                <SelectValue placeholder="Status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Status</SelectItem>
                <SelectItem value="active">Active</SelectItem>
                <SelectItem value="inactive">Inactive</SelectItem>
              </SelectContent>
            </Select>
            <Button
              variant="outline"
              size="icon"
              onClick={() => refetch()}
              disabled={isFetching}
            >
              <RefreshCw className={`h-4 w-4 ${isFetching ? "animate-spin" : ""}`} />
            </Button>
          </div>

          {/* Table */}
          {isLoading ? (
            <div className="space-y-3">
              {[...Array(5)].map((_, i) => (
                <Skeleton key={i} className="h-16 w-full" />
              ))}
            </div>
          ) : data?.items && data.items.length > 0 ? (
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Brand</TableHead>
                    <TableHead>Description</TableHead>
                    <TableHead className="text-center">Products</TableHead>
                    <TableHead className="text-center">Status</TableHead>
                    <TableHead className="w-[70px]"></TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {data.items.map((brand) => (
                    <TableRow key={brand.id}>
                      <TableCell>
                        <div className="flex items-center gap-3">
                          <Avatar className="h-10 w-10">
                            <AvatarImage src={brand.logoUrl} alt={brand.name} />
                            <AvatarFallback className="bg-primary/10 text-primary text-sm">
                              {getInitials(brand.name)}
                            </AvatarFallback>
                          </Avatar>
                          <div>
                            <div className="font-medium">{brand.name}</div>
                          </div>
                        </div>
                      </TableCell>
                      <TableCell>
                        <span className="text-muted-foreground line-clamp-1 max-w-[300px]">
                          {brand.description || "-"}
                        </span>
                      </TableCell>
                      <TableCell className="text-center">
                        <Badge variant="secondary">
                          {brand.productCount} products
                        </Badge>
                      </TableCell>
                      <TableCell className="text-center">
                        <Badge
                          variant={brand.isActive ? "default" : "secondary"}
                        >
                          {brand.isActive ? "Active" : "Inactive"}
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
                            <DropdownMenuItem onClick={() => handleEdit(brand)}>
                              <Pencil className="mr-2 h-4 w-4" />
                              Edit
                            </DropdownMenuItem>
                            <DropdownMenuItem
                              onClick={() => handleToggleActive(brand)}
                            >
                              {brand.isActive ? (
                                <>
                                  <PowerOff className="mr-2 h-4 w-4" />
                                  Deactivate
                                </>
                              ) : (
                                <>
                                  <Power className="mr-2 h-4 w-4" />
                                  Activate
                                </>
                              )}
                            </DropdownMenuItem>
                            <DropdownMenuSeparator />
                            <DropdownMenuItem
                              className="text-destructive"
                              onClick={() => setDeletingBrand(brand)}
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
              <Tag className="mx-auto h-12 w-12 text-muted-foreground" />
              <h3 className="mt-4 text-lg font-semibold">No brands</h3>
              <p className="text-muted-foreground">
                {searchQuery
                  ? `No brands match "${searchQuery}"`
                  : "Get started by creating your first brand."}
              </p>
              {!searchQuery && (
                <Button className="mt-4" onClick={handleAddNew}>
                  <Plus className="mr-2 h-4 w-4" />
                  Add Brand
                </Button>
              )}
            </div>
          )}

          {/* Pagination info */}
          {data && data.totalCount > 0 && (
            <div className="flex items-center justify-between mt-4 text-sm text-muted-foreground">
              <div>
                Showing {(data.pageNumber - 1) * data.pageSize + 1} to{" "}
                {Math.min(data.pageNumber * data.pageSize, data.totalCount)} of{" "}
                {data.totalCount} brands
              </div>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  disabled={!data.hasPreviousPage}
                  onClick={() =>
                    setFilters((prev) => ({
                      ...prev,
                      pageNumber: (prev.pageNumber || 1) - 1,
                    }))
                  }
                >
                  Previous
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  disabled={!data.hasNextPage}
                  onClick={() =>
                    setFilters((prev) => ({
                      ...prev,
                      pageNumber: (prev.pageNumber || 1) + 1,
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

      {/* Brand Form Dialog */}
      <Dialog open={isFormOpen} onOpenChange={setIsFormOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>
              {editingBrand ? "Edit Brand" : "Add Brand"}
            </DialogTitle>
            <DialogDescription>
              {editingBrand
                ? "Update the brand information below."
                : "Fill in the details to create a new brand."}
            </DialogDescription>
          </DialogHeader>
          <BrandForm
            defaultValues={
              editingBrand
                ? {
                    name: editingBrand.name,
                    description: editingBrand.description ?? "",
                    logoUrl: editingBrand.logoUrl ?? "",
                    isActive: editingBrand.isActive,
                  }
                : undefined
            }
            onSubmit={handleFormSubmit}
            onCancel={() => setIsFormOpen(false)}
            isLoading={createBrand.isPending || updateBrand.isPending}
          />
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation */}
      <ConfirmDialog
        open={!!deletingBrand}
        onOpenChange={(open) => !open && setDeletingBrand(null)}
        title="Delete Brand"
        description={
          deletingBrand?.productCount && deletingBrand.productCount > 0
            ? `This brand has ${deletingBrand.productCount} associated products. You must reassign or remove these products before deleting the brand.`
            : `Are you sure you want to delete "${deletingBrand?.name}"? This action cannot be undone.`
        }
        confirmText="Delete"
        variant="destructive"
        onConfirm={handleDelete}
        isLoading={deleteBrand.isPending}
      />
    </div>
  );
}
