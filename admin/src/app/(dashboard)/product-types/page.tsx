"use client";

import { useState, useMemo } from "react";
import {
  Plus,
  Search,
  X,
  Layers,
  Pencil,
  Trash2,
  Check,
  MoreHorizontal,
  Package,
  AlertCircle,
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
import { ScrollArea } from "@/components/ui/scroll-area";
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
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
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { ProductTypeForm } from "@/components/forms/product-type-form";
import {
  useProductTypes,
  useProductType,
  useCreateProductType,
  useUpdateProductType,
  useDeleteProductType,
  useAddAttributeToProductType,
  useRemoveAttributeFromProductType,
} from "@/hooks/use-product-types";
import { useAttributes } from "@/hooks/use-attributes";
import {
  ProductType,
  ProductTypeListItem,
  ProductTypeAttribute,
  AttributeDefinition,
  AttributeType,
  AttributeTypeEnum,
} from "@/types/entities";
import {
  ProductTypeFormData,
  ProductTypeEditFormData,
} from "@/lib/validations/product-type";
import { cn } from "@/lib/utils";

// Maps integer type to display label
const TYPE_LABELS: Record<AttributeType, string> = {
  1: "Text",
  2: "Number",
  3: "Select",
  4: "MultiSelect",
  5: "Boolean",
  6: "Date",
};

// Maps integer type to badge color
const TYPE_BADGE_COLORS: Record<AttributeType, string> = {
  1: "bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300",
  2: "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300",
  3: "bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-300",
  4: "bg-pink-100 text-pink-800 dark:bg-pink-900 dark:text-pink-300",
  5: "bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-300",
  6: "bg-cyan-100 text-cyan-800 dark:bg-cyan-900 dark:text-cyan-300",
};

// Product Type list item component
function ProductTypeListItemComponent({
  productType,
  isSelected,
  onClick,
}: {
  productType: ProductTypeListItem;
  isSelected: boolean;
  onClick: () => void;
}) {
  return (
    <div
      className={cn(
        "flex items-center justify-between p-3 border-b cursor-pointer transition-colors",
        isSelected
          ? "bg-muted border-l-2 border-l-primary"
          : "hover:bg-muted/50"
      )}
      onClick={onClick}
    >
      <div className="min-w-0 flex-1">
        <div className="flex items-center gap-2">
          <Layers className="h-4 w-4 text-muted-foreground flex-shrink-0" />
          <span className="font-medium truncate">{productType.name}</span>
          {!productType.isActive && (
            <Badge variant="secondary" className="text-xs">
              Inactive
            </Badge>
          )}
        </div>
        <div className="text-sm text-muted-foreground mt-0.5">
          Code: {productType.code}
        </div>
      </div>
      <div className="flex items-center gap-2 ml-2 flex-shrink-0">
        <Badge variant="outline" className="text-xs">
          {productType.attributeCount} attr
        </Badge>
        <Badge variant="outline" className="text-xs">
          {productType.productCount} prod
        </Badge>
      </div>
    </div>
  );
}

// Property display item
function PropertyItem({
  label,
  value,
  icon: Icon,
}: {
  label: string;
  value: string;
  icon?: React.ComponentType<{ className?: string }>;
}) {
  return (
    <div className="flex items-center justify-between py-2 px-3 rounded-md bg-muted/50">
      <span className="text-sm text-muted-foreground">{label}</span>
      <div className="flex items-center gap-1.5">
        {Icon && <Icon className="h-4 w-4 text-muted-foreground" />}
        <span className="text-sm font-medium">{value}</span>
      </div>
    </div>
  );
}

// Attribute list item in product type
function AttributeListItem({
  attribute,
  attributeDefinition,
  onRemove,
  onToggleRequired,
  isTogglingRequired,
}: {
  attribute: ProductTypeAttribute;
  attributeDefinition?: AttributeDefinition;
  onRemove: () => void;
  onToggleRequired: (isRequired: boolean) => void;
  isTogglingRequired: boolean;
}) {
  // Check if the attribute is required at the definition level
  const isRequiredAtDefinitionLevel = attributeDefinition?.isRequired ?? false;
  const canToggleRequired = !isRequiredAtDefinitionLevel;

  return (
    <div className="flex items-center justify-between py-3 px-3 rounded-md hover:bg-muted/50 group border-b last:border-b-0">
      <div className="min-w-0 flex-1">
        <div className="flex items-center gap-2">
          <span className="font-medium text-sm truncate">
            {attribute.attributeName}
          </span>
          <Badge className={cn("text-xs", TYPE_BADGE_COLORS[attribute.attributeType])}>
            {TYPE_LABELS[attribute.attributeType]}
          </Badge>
          {attribute.isRequired && (
            <Badge variant="destructive" className="text-xs">
              Required
            </Badge>
          )}
        </div>
        <div className="text-xs text-muted-foreground mt-0.5">
          Code: {attribute.attributeCode}
          {attribute.unit && ` • Unit: ${attribute.unit}`}
        </div>
      </div>
      <div className="flex items-center gap-3 ml-2 flex-shrink-0">
        <TooltipProvider>
          <Tooltip>
            <TooltipTrigger asChild>
              <div className="flex items-center gap-2">
                <Switch
                  checked={attribute.isRequired}
                  onCheckedChange={onToggleRequired}
                  disabled={!canToggleRequired || isTogglingRequired}
                  className={cn(!canToggleRequired && "opacity-50")}
                />
                <Label className="text-xs text-muted-foreground">
                  Required
                </Label>
              </div>
            </TooltipTrigger>
            {!canToggleRequired && (
              <TooltipContent>
                <p>This attribute is required at the definition level and cannot be changed</p>
              </TooltipContent>
            )}
          </Tooltip>
        </TooltipProvider>
        <Button
          variant="ghost"
          size="icon"
          className="h-7 w-7 opacity-0 group-hover:opacity-100 flex-shrink-0 text-destructive hover:text-destructive"
          onClick={(e) => {
            e.stopPropagation();
            onRemove();
          }}
        >
          <X className="h-4 w-4" />
        </Button>
      </div>
    </div>
  );
}

export default function ProductTypesPage() {
  // State
  const [selectedProductTypeId, setSelectedProductTypeId] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState("");
  const [statusFilter, setStatusFilter] = useState<string>("all");
  const [attributeSearchQuery, setAttributeSearchQuery] = useState("");

  // Dialog states
  const [isProductTypeFormOpen, setIsProductTypeFormOpen] = useState(false);
  const [editingProductType, setEditingProductType] = useState<ProductType | null>(null);
  const [deletingProductType, setDeletingProductType] = useState<ProductTypeListItem | null>(null);
  const [isAddAttributeOpen, setIsAddAttributeOpen] = useState(false);
  const [removingAttribute, setRemovingAttribute] = useState<ProductTypeAttribute | null>(null);

  // Add attribute form state
  const [selectedAttributeId, setSelectedAttributeId] = useState<string>("");
  const [newAttributeRequired, setNewAttributeRequired] = useState(false);

  // Queries and mutations
  const { data: productTypes, isLoading } = useProductTypes();
  const { data: selectedProductTypeDetail, isLoading: isLoadingDetail } = useProductType(selectedProductTypeId);
  const { data: allAttributes } = useAttributes();
  const createProductType = useCreateProductType();
  const updateProductType = useUpdateProductType();
  const deleteProductType = useDeleteProductType();
  const addAttribute = useAddAttributeToProductType();
  const removeAttribute = useRemoveAttributeFromProductType();

  // Filter product types
  const filteredProductTypes = useMemo(() => {
    if (!productTypes) return [];
    return productTypes.filter((pt) => {
      const matchesSearch =
        !searchQuery ||
        pt.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
        pt.code.toLowerCase().includes(searchQuery.toLowerCase());
      const matchesStatus =
        statusFilter === "all" ||
        (statusFilter === "active" && pt.isActive) ||
        (statusFilter === "inactive" && !pt.isActive);
      return matchesSearch && matchesStatus;
    });
  }, [productTypes, searchQuery, statusFilter]);

  // Selected product type - use detail query data which includes attributes
  const selectedProductType = selectedProductTypeDetail || null;

  // Filter attributes for search
  const filteredAttributes = useMemo(() => {
    if (!selectedProductType?.attributes) return [];
    if (!attributeSearchQuery) return selectedProductType.attributes;
    const query = attributeSearchQuery.toLowerCase();
    return selectedProductType.attributes.filter(
      (a) =>
        a.attributeName.toLowerCase().includes(query) ||
        a.attributeCode.toLowerCase().includes(query)
    );
  }, [selectedProductType?.attributes, attributeSearchQuery]);

  // Available attributes (not already assigned)
  const availableAttributes = useMemo(() => {
    if (!allAttributes || !selectedProductType) return [];
    const assignedIds = new Set(selectedProductType.attributes.map((a) => a.attributeDefinitionId));
    return allAttributes.filter((attr) => !assignedIds.has(attr.id));
  }, [allAttributes, selectedProductType]);

  // Get attribute definition by ID
  const getAttributeDefinition = (attributeDefinitionId: string) => {
    return allAttributes?.find((a) => a.id === attributeDefinitionId);
  };

  // Handlers
  const handleAddProductType = () => {
    setEditingProductType(null);
    setIsProductTypeFormOpen(true);
  };

  const handleEditProductType = () => {
    if (selectedProductType) {
      setEditingProductType(selectedProductType);
      setIsProductTypeFormOpen(true);
    }
  };

  const handleProductTypeFormSubmit = async (data: ProductTypeFormData | ProductTypeEditFormData) => {
    if (editingProductType) {
      await updateProductType.mutateAsync({
        id: editingProductType.id,
        data: {
          name: data.name,
          description: data.description || undefined,
          isActive: (data as ProductTypeEditFormData).isActive,
        },
      });
    } else {
      await createProductType.mutateAsync({
        code: (data as ProductTypeFormData).code,
        name: data.name,
        description: data.description || undefined,
      });
    }
    setIsProductTypeFormOpen(false);
    setEditingProductType(null);
  };

  const handleDeleteProductType = async () => {
    if (deletingProductType) {
      await deleteProductType.mutateAsync(deletingProductType.id);
      if (selectedProductTypeId === deletingProductType.id) {
        setSelectedProductTypeId(null);
      }
      setDeletingProductType(null);
    }
  };

  const handleAddAttribute = () => {
    setSelectedAttributeId("");
    setNewAttributeRequired(false);
    setIsAddAttributeOpen(true);
  };

  const handleAddAttributeSubmit = async () => {
    if (selectedProductType && selectedAttributeId) {
      const attrDef = getAttributeDefinition(selectedAttributeId);
      // If attribute is required at definition level, force isRequired to true
      const isRequired = attrDef?.isRequired ? true : newAttributeRequired;

      await addAttribute.mutateAsync({
        productTypeId: selectedProductType.id,
        data: {
          attributeDefinitionId: selectedAttributeId,
          isRequired,
          displayOrder: selectedProductType.attributes.length,
        },
      });
      setIsAddAttributeOpen(false);
    }
  };

  const handleRemoveAttribute = async () => {
    if (removingAttribute && selectedProductType) {
      await removeAttribute.mutateAsync({
        productTypeId: selectedProductType.id,
        attributeDefinitionId: removingAttribute.attributeDefinitionId,
      });
      setRemovingAttribute(null);
    }
  };

  const handleToggleAttributeRequired = async (attribute: ProductTypeAttribute, isRequired: boolean) => {
    if (selectedProductType) {
      // We need to remove and re-add the attribute with the new isRequired value
      // This is a workaround since there's no update endpoint for product type attributes
      await removeAttribute.mutateAsync({
        productTypeId: selectedProductType.id,
        attributeDefinitionId: attribute.attributeDefinitionId,
      });
      await addAttribute.mutateAsync({
        productTypeId: selectedProductType.id,
        data: {
          attributeDefinitionId: attribute.attributeDefinitionId,
          isRequired,
          displayOrder: attribute.displayOrder,
        },
      });
    }
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Product Types</h1>
          <p className="text-muted-foreground">
            Manage product types and their attribute requirements
          </p>
        </div>
        <Button onClick={handleAddProductType}>
          <Plus className="mr-2 h-4 w-4" />
          Add Product Type
        </Button>
      </div>

      {/* Master-Detail Layout */}
      <div className="grid gap-6 lg:grid-cols-5">
        {/* Left Panel - Product Type List */}
        <Card className="lg:col-span-2">
          <CardHeader className="pb-4">
            <CardTitle>Product Types</CardTitle>
            <CardDescription>
              {filteredProductTypes.length} product type
              {filteredProductTypes.length !== 1 ? "s" : ""}
            </CardDescription>

            {/* Filters */}
            <div className="space-y-3 pt-2">
              <div className="relative">
                <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                <Input
                  placeholder="Search product types..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="pl-9"
                />
              </div>
              <Select value={statusFilter} onValueChange={setStatusFilter}>
                <SelectTrigger>
                  <SelectValue placeholder="Status" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Status</SelectItem>
                  <SelectItem value="active">Active</SelectItem>
                  <SelectItem value="inactive">Inactive</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </CardHeader>
          <CardContent className="p-0">
            {isLoading ? (
              <div className="p-4 space-y-3">
                {[...Array(5)].map((_, i) => (
                  <Skeleton key={i} className="h-16 w-full" />
                ))}
              </div>
            ) : filteredProductTypes.length > 0 ? (
              <ScrollArea className="h-[500px]">
                {filteredProductTypes.map((pt) => (
                  <ProductTypeListItemComponent
                    key={pt.id}
                    productType={pt}
                    isSelected={pt.id === selectedProductTypeId}
                    onClick={() => setSelectedProductTypeId(pt.id)}
                  />
                ))}
              </ScrollArea>
            ) : (
              <div className="text-center py-10 px-4">
                <Layers className="mx-auto h-12 w-12 text-muted-foreground" />
                <h3 className="mt-4 text-lg font-semibold">
                  {searchQuery || statusFilter !== "all"
                    ? "No matches found"
                    : "No product types"}
                </h3>
                <p className="text-muted-foreground">
                  {searchQuery || statusFilter !== "all"
                    ? "Try adjusting your filters"
                    : "Get started by creating your first product type."}
                </p>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Right Panel - Product Type Details */}
        <Card className="lg:col-span-3">
          {selectedProductTypeId && isLoadingDetail ? (
            <div className="p-6 space-y-4">
              <div className="flex items-start justify-between">
                <div className="space-y-2">
                  <Skeleton className="h-7 w-48" />
                  <Skeleton className="h-4 w-32" />
                </div>
                <Skeleton className="h-9 w-9" />
              </div>
              <div className="grid grid-cols-2 gap-3">
                {[...Array(4)].map((_, i) => (
                  <Skeleton key={i} className="h-10 w-full" />
                ))}
              </div>
              <div className="border-t pt-6 space-y-4">
                <div className="flex justify-between">
                  <Skeleton className="h-6 w-36" />
                  <Skeleton className="h-9 w-24" />
                </div>
                <Skeleton className="h-[280px] w-full" />
              </div>
            </div>
          ) : selectedProductType ? (
            <>
              <CardHeader className="pb-4">
                <div className="flex items-start justify-between">
                  <div>
                    <div className="flex items-center gap-2">
                      <CardTitle>{selectedProductType.name}</CardTitle>
                      {!selectedProductType.isActive && (
                        <Badge variant="secondary">Inactive</Badge>
                      )}
                    </div>
                    <CardDescription className="mt-1">
                      Code: {selectedProductType.code}
                    </CardDescription>
                  </div>
                  <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                      <Button variant="outline" size="sm">
                        <MoreHorizontal className="h-4 w-4" />
                      </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                      <DropdownMenuItem onClick={handleEditProductType}>
                        <Pencil className="mr-2 h-4 w-4" />
                        Edit
                      </DropdownMenuItem>
                      <DropdownMenuItem
                        className="text-destructive"
                        onClick={() => {
                          const listItem = productTypes?.find(
                            (pt) => pt.id === selectedProductType.id
                          );
                          if (listItem) setDeletingProductType(listItem);
                        }}
                      >
                        <Trash2 className="mr-2 h-4 w-4" />
                        Delete
                      </DropdownMenuItem>
                    </DropdownMenuContent>
                  </DropdownMenu>
                </div>
              </CardHeader>
              <CardContent className="space-y-6">
                {/* Properties */}
                <div className="grid grid-cols-2 gap-3">
                  <PropertyItem
                    label="Status"
                    value={selectedProductType.isActive ? "Active" : "Inactive"}
                    icon={selectedProductType.isActive ? Check : undefined}
                  />
                  <PropertyItem
                    label="Attributes"
                    value={String(selectedProductType.attributes.length)}
                  />
                </div>

                {selectedProductType.description && (
                  <div className="rounded-md bg-muted/50 p-3">
                    <p className="text-sm text-muted-foreground">
                      {selectedProductType.description}
                    </p>
                  </div>
                )}

                {/* Attributes Section */}
                <div className="border-t pt-6">
                  <div className="flex items-center justify-between mb-4">
                    <h3 className="font-semibold">
                      Attributes ({selectedProductType.attributes.length})
                    </h3>
                    <Button size="sm" onClick={handleAddAttribute}>
                      <Plus className="mr-2 h-4 w-4" />
                      Add Attribute
                    </Button>
                  </div>

                  {selectedProductType.attributes.length > 5 && (
                    <div className="relative mb-3">
                      <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                      <Input
                        placeholder="Search attributes..."
                        value={attributeSearchQuery}
                        onChange={(e) => setAttributeSearchQuery(e.target.value)}
                        className="pl-9"
                      />
                    </div>
                  )}

                  {filteredAttributes.length > 0 ? (
                    <ScrollArea className="h-[300px] border rounded-md">
                      <div className="p-2">
                        {filteredAttributes.map((attr) => (
                          <AttributeListItem
                            key={attr.id}
                            attribute={attr}
                            attributeDefinition={getAttributeDefinition(attr.attributeDefinitionId)}
                            onRemove={() => setRemovingAttribute(attr)}
                            onToggleRequired={(isRequired) =>
                              handleToggleAttributeRequired(attr, isRequired)
                            }
                            isTogglingRequired={
                              addAttribute.isPending || removeAttribute.isPending
                            }
                          />
                        ))}
                      </div>
                    </ScrollArea>
                  ) : (
                    <div className="text-center py-8 border rounded-md">
                      <Package className="mx-auto h-10 w-10 text-muted-foreground" />
                      <p className="text-muted-foreground mt-2">
                        {attributeSearchQuery
                          ? "No attributes match your search"
                          : "No attributes assigned yet"}
                      </p>
                      {!attributeSearchQuery && (
                        <Button
                          variant="link"
                          className="mt-2"
                          onClick={handleAddAttribute}
                        >
                          Add your first attribute
                        </Button>
                      )}
                    </div>
                  )}
                </div>
              </CardContent>
            </>
          ) : (
            <div className="flex flex-col items-center justify-center h-[600px] text-center px-4">
              <Layers className="h-16 w-16 text-muted-foreground mb-4" />
              <h3 className="text-lg font-semibold">Select a Product Type</h3>
              <p className="text-muted-foreground max-w-sm mt-1">
                Choose a product type from the list to view its details and
                manage attributes.
              </p>
            </div>
          )}
        </Card>
      </div>

      {/* Product Type Form Dialog */}
      <Dialog open={isProductTypeFormOpen} onOpenChange={setIsProductTypeFormOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>
              {editingProductType ? "Edit Product Type" : "Add Product Type"}
            </DialogTitle>
            <DialogDescription>
              {editingProductType
                ? "Update the product type settings. Code cannot be changed."
                : "Create a new product type to group products by their attributes."}
            </DialogDescription>
          </DialogHeader>
          <ProductTypeForm
            defaultValues={
              editingProductType
                ? {
                    code: editingProductType.code,
                    name: editingProductType.name,
                    description: editingProductType.description,
                    isActive: editingProductType.isActive,
                  }
                : undefined
            }
            isEditing={!!editingProductType}
            onSubmit={handleProductTypeFormSubmit}
            onCancel={() => setIsProductTypeFormOpen(false)}
            isLoading={createProductType.isPending || updateProductType.isPending}
          />
        </DialogContent>
      </Dialog>

      {/* Add Attribute Dialog */}
      <Dialog open={isAddAttributeOpen} onOpenChange={setIsAddAttributeOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>Add Attribute</DialogTitle>
            <DialogDescription>
              Add an attribute to &quot;{selectedProductType?.name}&quot;
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label>Attribute *</Label>
              {availableAttributes.length > 0 ? (
                <Select value={selectedAttributeId} onValueChange={setSelectedAttributeId}>
                  <SelectTrigger>
                    <SelectValue placeholder="Select an attribute" />
                  </SelectTrigger>
                  <SelectContent>
                    {availableAttributes.map((attr) => (
                      <SelectItem key={attr.id} value={attr.id}>
                        <div className="flex items-center gap-2">
                          <span>{attr.name}</span>
                          <Badge
                            variant="outline"
                            className={cn("text-xs", TYPE_BADGE_COLORS[attr.type])}
                          >
                            {TYPE_LABELS[attr.type]}
                          </Badge>
                          {attr.isRequired && (
                            <Badge variant="destructive" className="text-xs">
                              Required
                            </Badge>
                          )}
                        </div>
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              ) : (
                <div className="flex items-center gap-2 p-3 rounded-md bg-muted border text-sm text-muted-foreground">
                  <AlertCircle className="h-4 w-4" />
                  All attributes have already been assigned to this product type
                </div>
              )}
            </div>

            {selectedAttributeId && (
              <div className="space-y-2">
                {(() => {
                  const attrDef = getAttributeDefinition(selectedAttributeId);
                  if (attrDef?.isRequired) {
                    return (
                      <div className="flex items-center gap-2 p-3 rounded-md bg-amber-50 dark:bg-amber-950 border border-amber-200 dark:border-amber-800">
                        <AlertCircle className="h-4 w-4 text-amber-600" />
                        <p className="text-sm text-amber-700 dark:text-amber-300">
                          This attribute is required at the definition level and will be required for this product type.
                        </p>
                      </div>
                    );
                  }
                  return (
                    <div className="flex items-center justify-between rounded-lg border p-4">
                      <div className="space-y-0.5">
                        <Label className="text-base">Required</Label>
                        <p className="text-sm text-muted-foreground">
                          Make this attribute required for products of this type
                        </p>
                      </div>
                      <Switch
                        checked={newAttributeRequired}
                        onCheckedChange={setNewAttributeRequired}
                      />
                    </div>
                  );
                })()}
              </div>
            )}

            <div className="flex justify-end gap-4 pt-4">
              <Button
                variant="outline"
                onClick={() => setIsAddAttributeOpen(false)}
              >
                Cancel
              </Button>
              <Button
                onClick={handleAddAttributeSubmit}
                disabled={!selectedAttributeId || addAttribute.isPending}
              >
                {addAttribute.isPending && (
                  <span className="mr-2 h-4 w-4 animate-spin">...</span>
                )}
                Add Attribute
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      {/* Delete Product Type Confirmation */}
      <ConfirmDialog
        open={!!deletingProductType}
        onOpenChange={(open) => !open && setDeletingProductType(null)}
        title="Delete Product Type"
        description={`Are you sure you want to delete "${deletingProductType?.name}"? ${
          deletingProductType?.productCount
            ? `This product type has ${deletingProductType.productCount} product(s) assigned.`
            : ""
        }`}
        confirmText="Delete"
        variant="destructive"
        onConfirm={handleDeleteProductType}
        isLoading={deleteProductType.isPending}
      />

      {/* Remove Attribute Confirmation */}
      <ConfirmDialog
        open={!!removingAttribute}
        onOpenChange={(open) => !open && setRemovingAttribute(null)}
        title="Remove Attribute"
        description={`Are you sure you want to remove "${removingAttribute?.attributeName}" from this product type? Products of this type may be affected.`}
        confirmText="Remove"
        variant="destructive"
        onConfirm={handleRemoveAttribute}
        isLoading={removeAttribute.isPending}
      />
    </div>
  );
}
