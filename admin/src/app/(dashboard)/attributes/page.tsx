"use client";

import { useState, useMemo } from "react";
import {
  Plus,
  Search,
  X,
  Settings2,
  Pencil,
  Trash2,
  Filter,
  Check,
  Eye,
  MoreHorizontal,
  Package,
  Users,
  List,
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
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { AttributeForm } from "@/components/forms/attribute-form";
import { AttributeValueForm } from "@/components/forms/attribute-value-form";
import {
  useAttributes,
  useAttribute,
  useCreateAttribute,
  useUpdateAttribute,
  useDeleteAttribute,
  useAddAttributeValue,
  useRemoveAttributeValue,
  useChildAttributes,
} from "@/hooks/use-attributes";
import {
  AttributeDefinition,
  AttributeValue,
  AttributeType,
  AttributeTypeString,
  AttributeTypeEnum,
  AttributeTypeFromString,
  AttributeEntityType,
  AttributeEntityTypeString,
  AttributeEntityTypeEnum,
  AttributeEntityTypeFromString,
  AttributeEntityTypeLabels,
} from "@/types/entities";
import { AttributeFormData, AttributeValueFormData } from "@/lib/validations/attribute";
import { cn } from "@/lib/utils";

// Helper to normalize type to integer (handles both string and number from backend)
function normalizeType(type: AttributeType | AttributeTypeString): AttributeType {
  if (typeof type === "string") {
    return AttributeTypeFromString[type as AttributeTypeString] ?? 1;
  }
  return type;
}

// Helper to normalize entityType to integer (handles both string and number from backend)
function normalizeEntityType(entityType: AttributeEntityType | AttributeEntityTypeString | undefined): AttributeEntityType {
  if (!entityType) return AttributeEntityTypeEnum.Product;
  if (typeof entityType === "string") {
    return AttributeEntityTypeFromString[entityType as AttributeEntityTypeString] ?? 1;
  }
  return entityType;
}

// Maps type to badge color (supports both string and number keys)
const TYPE_BADGE_COLORS: Record<string | number, string> = {
  1: "bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300",    // Text
  2: "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300", // Number
  3: "bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-300", // Select
  4: "bg-pink-100 text-pink-800 dark:bg-pink-900 dark:text-pink-300",    // MultiSelect
  5: "bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-300", // Boolean
  6: "bg-cyan-100 text-cyan-800 dark:bg-cyan-900 dark:text-cyan-300",    // Date
  7: "bg-indigo-100 text-indigo-800 dark:bg-indigo-900 dark:text-indigo-300", // Composite
  // String keys for direct backend response
  Text: "bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300",
  Number: "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300",
  Select: "bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-300",
  MultiSelect: "bg-pink-100 text-pink-800 dark:bg-pink-900 dark:text-pink-300",
  Boolean: "bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-300",
  Date: "bg-cyan-100 text-cyan-800 dark:bg-cyan-900 dark:text-cyan-300",
  Composite: "bg-indigo-100 text-indigo-800 dark:bg-indigo-900 dark:text-indigo-300",
};

// Maps type to display label (supports both string and number keys)
const TYPE_LABELS: Record<string | number, string> = {
  1: "Text",
  2: "Number",
  3: "Select (Single)",
  4: "Multi-Select",
  5: "Boolean (Yes/No)",
  6: "Date",
  7: "Composite",
  Text: "Text",
  Number: "Number",
  Select: "Select (Single)",
  MultiSelect: "Multi-Select",
  Boolean: "Boolean (Yes/No)",
  Date: "Date",
  Composite: "Composite",
};

// Maps type to short name for badges (supports both string and number keys)
const TYPE_NAMES: Record<string | number, string> = {
  1: "Text",
  2: "Number",
  3: "Select",
  4: "MultiSelect",
  5: "Boolean",
  6: "Date",
  7: "Composite",
  Text: "Text",
  Number: "Number",
  Select: "Select",
  MultiSelect: "MultiSelect",
  Boolean: "Boolean",
  Date: "Date",
  Composite: "Composite",
};

// Attribute list item component
function AttributeListItem({
  attribute,
  isSelected,
  onClick,
}: {
  attribute: AttributeDefinition;
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
          <Settings2 className="h-4 w-4 text-muted-foreground flex-shrink-0" />
          <span className="font-medium truncate">{attribute.name}</span>
        </div>
        <div className="text-sm text-muted-foreground mt-0.5">
          Code: {attribute.code}
          {attribute.unit && ` • Unit: ${attribute.unit}`}
        </div>
      </div>
      <div className="flex items-center gap-2 ml-2 flex-shrink-0">
        <Badge className={cn("text-xs", TYPE_BADGE_COLORS[attribute.type])}>
          {TYPE_NAMES[attribute.type] || attribute.type}
        </Badge>
        {attribute.isList && (
          <span title="Multiple values allowed">
            <List className="h-3.5 w-3.5 text-muted-foreground" />
          </span>
        )}
        {attribute.isFilterable && (
          <span title="Filterable">
            <Filter className="h-3.5 w-3.5 text-muted-foreground" />
          </span>
        )}
      </div>
    </div>
  );
}

// Property display item
function PropertyItem({ label, value, icon: Icon }: { label: string; value: string; icon?: React.ComponentType<{ className?: string }> }) {
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

// Value list item
function ValueListItem({
  value,
  onDelete,
}: {
  value: AttributeValue;
  onDelete: () => void;
}) {
  return (
    <div className="flex items-center justify-between py-2 px-3 rounded-md hover:bg-muted/50 group">
      <div className="min-w-0 flex-1">
        <div className="font-medium text-sm truncate">
          {value.displayText || value.value}
        </div>
        {value.displayText && (
          <div className="text-xs text-muted-foreground truncate">
            Value: {value.value}
          </div>
        )}
      </div>
      <Button
        variant="ghost"
        size="icon"
        className="h-7 w-7 opacity-0 group-hover:opacity-100 flex-shrink-0 text-destructive hover:text-destructive"
        onClick={(e) => {
          e.stopPropagation();
          onDelete();
        }}
      >
        <X className="h-4 w-4" />
      </Button>
    </div>
  );
}

// Helper to check if type supports predefined values
function typeSupportsValues(type: AttributeType | AttributeTypeString): boolean {
  const normalizedType = typeof type === "string"
    ? AttributeTypeFromString[type as AttributeTypeString]
    : type;
  return normalizedType === AttributeTypeEnum.Select || normalizedType === AttributeTypeEnum.MultiSelect;
}

// Child attribute list item for Composite types
function ChildAttributeListItem({
  attribute,
  onEdit,
  onManageValues,
  onDelete,
}: {
  attribute: AttributeDefinition;
  onEdit: () => void;
  onManageValues: () => void;
  onDelete: () => void;
}) {
  const hasValuesSupport = typeSupportsValues(attribute.type);

  return (
    <div
      className="flex items-center justify-between py-2 px-3 rounded-md hover:bg-muted/50 group cursor-pointer"
      onClick={onEdit}
    >
      <div className="min-w-0 flex-1">
        <div className="flex items-center gap-2">
          <span className="font-medium text-sm truncate">{attribute.name}</span>
          <Badge className={cn("text-xs", TYPE_BADGE_COLORS[attribute.type])}>
            {TYPE_NAMES[attribute.type] || attribute.type}
          </Badge>
          {hasValuesSupport && (
            <span className="text-xs text-muted-foreground">
              ({attribute.predefinedValues?.length || 0} values)
            </span>
          )}
        </div>
        <div className="text-xs text-muted-foreground truncate">
          Code: {attribute.code}
        </div>
      </div>
      <div className="flex items-center gap-1 opacity-0 group-hover:opacity-100">
        {hasValuesSupport && (
          <Button
            variant="ghost"
            size="icon"
            className="h-7 w-7"
            title="Manage values"
            onClick={(e) => {
              e.stopPropagation();
              onManageValues();
            }}
          >
            <List className="h-3.5 w-3.5" />
          </Button>
        )}
        <Button
          variant="ghost"
          size="icon"
          className="h-7 w-7"
          title="Edit"
          onClick={(e) => {
            e.stopPropagation();
            onEdit();
          }}
        >
          <Pencil className="h-3.5 w-3.5" />
        </Button>
        <Button
          variant="ghost"
          size="icon"
          className="h-7 w-7 text-destructive hover:text-destructive"
          title="Delete"
          onClick={(e) => {
            e.stopPropagation();
            onDelete();
          }}
        >
          <X className="h-4 w-4" />
        </Button>
      </div>
    </div>
  );
}

export default function AttributesPage() {
  // State
  const [entityTypeTab, setEntityTypeTab] = useState<AttributeEntityType>(AttributeEntityTypeEnum.Product);
  const [selectedAttributeId, setSelectedAttributeId] = useState<string | null>(null);
  const [searchQuery, setSearchQuery] = useState("");
  const [typeFilter, setTypeFilter] = useState<string>("all");
  const [filterableFilter, setFilterableFilter] = useState<string>("all");
  const [valueSearchQuery, setValueSearchQuery] = useState("");

  // Dialog states
  const [isAttributeFormOpen, setIsAttributeFormOpen] = useState(false);
  const [editingAttribute, setEditingAttribute] = useState<AttributeDefinition | null>(null);
  const [deletingAttribute, setDeletingAttribute] = useState<AttributeDefinition | null>(null);
  const [isValueFormOpen, setIsValueFormOpen] = useState(false);
  const [deletingValue, setDeletingValue] = useState<AttributeValue | null>(null);
  const [isChildAttributeFormOpen, setIsChildAttributeFormOpen] = useState(false);
  const [editingChildAttribute, setEditingChildAttribute] = useState<AttributeDefinition | null>(null);
  const [deletingChildAttribute, setDeletingChildAttribute] = useState<AttributeDefinition | null>(null);
  const [managingValuesChildAttribute, setManagingValuesChildAttribute] = useState<AttributeDefinition | null>(null);
  const [childValueSearchQuery, setChildValueSearchQuery] = useState("");
  const [isChildValueFormOpen, setIsChildValueFormOpen] = useState(false);
  const [deletingChildValue, setDeletingChildValue] = useState<AttributeValue | null>(null);

  // Queries and mutations - filter by entity type
  const { data: attributes, isLoading } = useAttributes(entityTypeTab);
  const { data: selectedAttributeDetail, isLoading: isLoadingDetail } = useAttribute(selectedAttributeId);
  const createAttribute = useCreateAttribute();
  const updateAttribute = useUpdateAttribute();
  const deleteAttribute = useDeleteAttribute();
  const addAttributeValue = useAddAttributeValue();
  const removeAttributeValue = useRemoveAttributeValue();

  // Check if selected attribute is Composite type for child attributes query
  const normalizedSelectedType = selectedAttributeDetail ? normalizeType(selectedAttributeDetail.type) : null;
  const isCompositeType = normalizedSelectedType === AttributeTypeEnum.Composite;
  const { data: childAttributes, isLoading: isLoadingChildren } = useChildAttributes(
    isCompositeType ? selectedAttributeId : null
  );

  // Clear selection when switching tabs
  const handleEntityTypeChange = (value: string) => {
    const newEntityType = parseInt(value, 10) as AttributeEntityType;
    setEntityTypeTab(newEntityType);
    setSelectedAttributeId(null);
    setSearchQuery("");
    setTypeFilter("all");
    setFilterableFilter("all");
  };

  // Filter attributes - exclude child attributes (only show parent/root level)
  const filteredAttributes = useMemo(() => {
    if (!attributes) return [];
    return attributes.filter((attr) => {
      // Exclude child attributes - they are shown under their parent's detail view
      if (attr.parentAttributeId) return false;

      const matchesSearch =
        !searchQuery ||
        attr.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
        attr.code.toLowerCase().includes(searchQuery.toLowerCase());
      const matchesType = typeFilter === "all" || String(attr.type) === typeFilter;
      const matchesFilterable =
        filterableFilter === "all" ||
        (filterableFilter === "yes" && attr.isFilterable) ||
        (filterableFilter === "no" && !attr.isFilterable);
      return matchesSearch && matchesType && matchesFilterable;
    });
  }, [attributes, searchQuery, typeFilter, filterableFilter]);

  // Selected attribute - use detail query data which includes values
  const selectedAttribute = selectedAttributeDetail || null;

  // Filter values
  const filteredValues = useMemo(() => {
    if (!selectedAttribute?.predefinedValues) return [];
    if (!valueSearchQuery) return selectedAttribute.predefinedValues;
    const query = valueSearchQuery.toLowerCase();
    return selectedAttribute.predefinedValues.filter(
      (v) =>
        v.value.toLowerCase().includes(query) ||
        (v.displayText && v.displayText.toLowerCase().includes(query))
    );
  }, [selectedAttribute?.predefinedValues, valueSearchQuery]);

  // Handlers
  const handleAddAttribute = () => {
    setEditingAttribute(null);
    setIsAttributeFormOpen(true);
  };

  const handleEditAttribute = () => {
    if (selectedAttribute) {
      setEditingAttribute(selectedAttribute);
      setIsAttributeFormOpen(true);
    }
  };

  const handleAttributeFormSubmit = async (data: AttributeFormData) => {
    if (editingAttribute) {
      await updateAttribute.mutateAsync({
        id: editingAttribute.id,
        data: {
          name: data.name,
          unit: data.unit || undefined,
          isFilterable: data.isFilterable,
          isRequired: data.isRequired,
          isVisibleOnProductPage: data.isVisibleOnProductPage,
          displayOrder: data.displayOrder,
        },
      });
    } else {
      await createAttribute.mutateAsync({
        code: data.code,
        name: data.name,
        type: data.type,
        entityType: data.entityType,
        isList: data.isList,
        unit: data.unit || undefined,
        isFilterable: data.isFilterable,
        isRequired: data.isRequired,
        isVisibleOnProductPage: data.isVisibleOnProductPage,
        displayOrder: data.displayOrder,
      });
    }
    setIsAttributeFormOpen(false);
    setEditingAttribute(null);
  };

  const handleDeleteAttribute = async () => {
    if (deletingAttribute) {
      await deleteAttribute.mutateAsync(deletingAttribute.id);
      if (selectedAttributeId === deletingAttribute.id) {
        setSelectedAttributeId(null);
      }
      setDeletingAttribute(null);
    }
  };

  const handleAddValue = () => {
    setIsValueFormOpen(true);
  };

  const handleValueFormSubmit = async (data: AttributeValueFormData) => {
    if (selectedAttribute) {
      await addAttributeValue.mutateAsync({
        attributeId: selectedAttribute.id,
        data: {
          value: data.value,
          displayText: data.displayText || undefined,
          displayOrder: data.displayOrder,
        },
      });
      setIsValueFormOpen(false);
    }
  };

  const handleDeleteValue = async () => {
    if (deletingValue && selectedAttribute) {
      await removeAttributeValue.mutateAsync({
        attributeId: selectedAttribute.id,
        valueId: deletingValue.id,
      });
      setDeletingValue(null);
    }
  };

  // Handler for creating/editing child attribute
  const handleChildAttributeFormSubmit = async (data: AttributeFormData) => {
    if (editingChildAttribute) {
      // Update existing child attribute
      await updateAttribute.mutateAsync({
        id: editingChildAttribute.id,
        data: {
          name: data.name,
          unit: data.unit || undefined,
          isFilterable: data.isFilterable,
          isRequired: data.isRequired,
          isVisibleOnProductPage: data.isVisibleOnProductPage,
          displayOrder: data.displayOrder,
        },
      });
    } else if (selectedAttribute) {
      // Create new child attribute
      await createAttribute.mutateAsync({
        code: data.code,
        name: data.name,
        type: data.type,
        entityType: data.entityType,
        isList: data.isList,
        unit: data.unit || undefined,
        isFilterable: data.isFilterable,
        isRequired: data.isRequired,
        isVisibleOnProductPage: data.isVisibleOnProductPage,
        displayOrder: data.displayOrder,
        parentAttributeId: selectedAttribute.id,
      });
    }
    setIsChildAttributeFormOpen(false);
    setEditingChildAttribute(null);
  };

  // Handler for opening edit child attribute dialog
  const handleEditChildAttribute = (child: AttributeDefinition) => {
    setEditingChildAttribute(child);
    setIsChildAttributeFormOpen(true);
  };

  // Handler for opening add child attribute dialog
  const handleAddChildAttribute = () => {
    setEditingChildAttribute(null);
    setIsChildAttributeFormOpen(true);
  };

  // Handler for deleting child attribute
  const handleDeleteChildAttribute = async () => {
    if (deletingChildAttribute) {
      await deleteAttribute.mutateAsync(deletingChildAttribute.id);
      setDeletingChildAttribute(null);
    }
  };

  // Handler for managing child attribute values
  const handleManageChildValues = (child: AttributeDefinition) => {
    setManagingValuesChildAttribute(child);
    setChildValueSearchQuery("");
  };

  // Handler for adding value to child attribute
  const handleChildValueFormSubmit = async (data: AttributeValueFormData) => {
    if (managingValuesChildAttribute) {
      await addAttributeValue.mutateAsync({
        attributeId: managingValuesChildAttribute.id,
        data: {
          value: data.value,
          displayText: data.displayText || undefined,
          displayOrder: data.displayOrder,
        },
      });
      setIsChildValueFormOpen(false);
    }
  };

  // Handler for deleting child attribute value
  const handleDeleteChildValue = async () => {
    if (deletingChildValue && managingValuesChildAttribute) {
      await removeAttributeValue.mutateAsync({
        attributeId: managingValuesChildAttribute.id,
        valueId: deletingChildValue.id,
      });
      setDeletingChildValue(null);
    }
  };

  // Get the current child attribute from the fresh query data (for value updates)
  const currentChildAttribute = useMemo(() => {
    if (!managingValuesChildAttribute) return null;
    return childAttributes?.find((c) => c.id === managingValuesChildAttribute.id) || managingValuesChildAttribute;
  }, [managingValuesChildAttribute, childAttributes]);

  // Filter child attribute values
  const filteredChildValues = useMemo(() => {
    if (!currentChildAttribute?.predefinedValues) return [];
    if (!childValueSearchQuery) return currentChildAttribute.predefinedValues;
    const query = childValueSearchQuery.toLowerCase();
    return currentChildAttribute.predefinedValues.filter(
      (v) =>
        v.value.toLowerCase().includes(query) ||
        (v.displayText && v.displayText.toLowerCase().includes(query))
    );
  }, [currentChildAttribute?.predefinedValues, childValueSearchQuery]);

  // Select = 3 or "Select", MultiSelect = 4 or "MultiSelect"
  const hasValues =
    normalizedSelectedType === AttributeTypeEnum.Select ||
    normalizedSelectedType === AttributeTypeEnum.MultiSelect;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Attributes</h1>
          <p className="text-muted-foreground">
            Manage attribute definitions for products and customers
          </p>
        </div>
        <Button onClick={handleAddAttribute}>
          <Plus className="mr-2 h-4 w-4" />
          Add Attribute
        </Button>
      </div>

      {/* Entity Type Tabs */}
      <Tabs value={String(entityTypeTab)} onValueChange={handleEntityTypeChange}>
        <TabsList>
          <TabsTrigger value={String(AttributeEntityTypeEnum.Product)} className="gap-2">
            <Package className="h-4 w-4" />
            Product Attributes
          </TabsTrigger>
          <TabsTrigger value={String(AttributeEntityTypeEnum.Customer)} className="gap-2">
            <Users className="h-4 w-4" />
            Customer Attributes
          </TabsTrigger>
        </TabsList>
      </Tabs>

      {/* Master-Detail Layout */}
      <div className="grid gap-6 lg:grid-cols-5">
        {/* Left Panel - Attribute List */}
        <Card className="lg:col-span-2">
          <CardHeader className="pb-4">
            <CardTitle>Attribute List</CardTitle>
            <CardDescription>
              {filteredAttributes.length} attribute
              {filteredAttributes.length !== 1 ? "s" : ""}
            </CardDescription>

            {/* Filters */}
            <div className="space-y-3 pt-2">
              <div className="relative">
                <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                <Input
                  placeholder="Search attributes..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="pl-9"
                />
              </div>
              <div className="flex gap-2">
                <Select value={typeFilter} onValueChange={setTypeFilter}>
                  <SelectTrigger className="flex-1">
                    <SelectValue placeholder="Type" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">All Types</SelectItem>
                    <SelectItem value="1">Text</SelectItem>
                    <SelectItem value="2">Number</SelectItem>
                    <SelectItem value="3">Select</SelectItem>
                    <SelectItem value="4">Multi-Select</SelectItem>
                    <SelectItem value="5">Boolean</SelectItem>
                    <SelectItem value="6">Date</SelectItem>
                    <SelectItem value="7">Composite</SelectItem>
                  </SelectContent>
                </Select>
                <Select
                  value={filterableFilter}
                  onValueChange={setFilterableFilter}
                >
                  <SelectTrigger className="flex-1">
                    <SelectValue placeholder="Filterable" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">All</SelectItem>
                    <SelectItem value="yes">Filterable</SelectItem>
                    <SelectItem value="no">Not Filterable</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>
          </CardHeader>
          <CardContent className="p-0">
            {isLoading ? (
              <div className="p-4 space-y-3">
                {[...Array(5)].map((_, i) => (
                  <Skeleton key={i} className="h-16 w-full" />
                ))}
              </div>
            ) : filteredAttributes.length > 0 ? (
              <ScrollArea className="h-[500px]">
                {filteredAttributes.map((attr) => (
                  <AttributeListItem
                    key={attr.id}
                    attribute={attr}
                    isSelected={attr.id === selectedAttributeId}
                    onClick={() => setSelectedAttributeId(attr.id)}
                  />
                ))}
              </ScrollArea>
            ) : (
              <div className="text-center py-10 px-4">
                <Settings2 className="mx-auto h-12 w-12 text-muted-foreground" />
                <h3 className="mt-4 text-lg font-semibold">
                  {searchQuery || typeFilter !== "all" || filterableFilter !== "all"
                    ? "No matches found"
                    : "No attributes"}
                </h3>
                <p className="text-muted-foreground">
                  {searchQuery || typeFilter !== "all" || filterableFilter !== "all"
                    ? "Try adjusting your filters"
                    : "Get started by creating your first attribute."}
                </p>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Right Panel - Attribute Details */}
        <Card className="lg:col-span-3">
          {selectedAttributeId && isLoadingDetail ? (
            <div className="p-6 space-y-4">
              <div className="flex items-start justify-between">
                <div className="space-y-2">
                  <Skeleton className="h-7 w-48" />
                  <Skeleton className="h-4 w-32" />
                </div>
                <Skeleton className="h-9 w-9" />
              </div>
              <div className="grid grid-cols-2 gap-3">
                {[...Array(6)].map((_, i) => (
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
          ) : selectedAttribute ? (
            <>
              <CardHeader className="pb-4">
                <div className="flex items-start justify-between">
                  <div>
                    <div className="flex items-center gap-2">
                      <CardTitle>{selectedAttribute.name}</CardTitle>
                      <Badge
                        className={cn(
                          "text-xs",
                          TYPE_BADGE_COLORS[selectedAttribute.type]
                        )}
                      >
                        {TYPE_NAMES[selectedAttribute.type] || selectedAttribute.type}
                      </Badge>
                    </div>
                    <CardDescription className="mt-1">
                      Code: {selectedAttribute.code}
                    </CardDescription>
                  </div>
                  <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                      <Button variant="outline" size="sm">
                        <MoreHorizontal className="h-4 w-4" />
                      </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                      <DropdownMenuItem onClick={handleEditAttribute}>
                        <Pencil className="mr-2 h-4 w-4" />
                        Edit
                      </DropdownMenuItem>
                      <DropdownMenuItem
                        className="text-destructive"
                        onClick={() => setDeletingAttribute(selectedAttribute)}
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
                    label="Type"
                    value={TYPE_LABELS[selectedAttribute.type] || String(selectedAttribute.type)}
                  />
                  <PropertyItem
                    label="Unit"
                    value={selectedAttribute.unit || "—"}
                  />
                  <PropertyItem
                    label="Filterable"
                    value={selectedAttribute.isFilterable ? "Yes" : "No"}
                    icon={selectedAttribute.isFilterable ? Check : undefined}
                  />
                  <PropertyItem
                    label="Required"
                    value={selectedAttribute.isRequired ? "Yes" : "No"}
                    icon={selectedAttribute.isRequired ? Check : undefined}
                  />
                  <PropertyItem
                    label="Multiple Values"
                    value={selectedAttribute.isList ? "Yes" : "No"}
                    icon={selectedAttribute.isList ? List : undefined}
                  />
                  {normalizeEntityType(selectedAttribute.entityType) === AttributeEntityTypeEnum.Product && (
                    <PropertyItem
                      label="Visible on Product Page"
                      value={selectedAttribute.isVisibleOnProductPage ? "Yes" : "No"}
                      icon={selectedAttribute.isVisibleOnProductPage ? Eye : undefined}
                    />
                  )}
                  <PropertyItem
                    label="Display Order"
                    value={String(selectedAttribute.displayOrder)}
                  />
                </div>

                {/* Values Section - Only for Select/MultiSelect */}
                {hasValues && (
                  <div className="border-t pt-6">
                    <div className="flex items-center justify-between mb-4">
                      <h3 className="font-semibold">
                        Predefined Values ({selectedAttribute.predefinedValues?.length || 0})
                      </h3>
                      <Button size="sm" onClick={handleAddValue}>
                        <Plus className="mr-2 h-4 w-4" />
                        Add Value
                      </Button>
                    </div>

                    {(selectedAttribute.predefinedValues?.length || 0) > 5 && (
                      <div className="relative mb-3">
                        <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                        <Input
                          placeholder="Search values..."
                          value={valueSearchQuery}
                          onChange={(e) => setValueSearchQuery(e.target.value)}
                          className="pl-9"
                        />
                      </div>
                    )}

                    {filteredValues.length > 0 ? (
                      <ScrollArea className="h-[280px] border rounded-md">
                        <div className="p-2">
                          {filteredValues.map((value) => (
                            <ValueListItem
                              key={value.id}
                              value={value}
                              onDelete={() => setDeletingValue(value)}
                            />
                          ))}
                        </div>
                      </ScrollArea>
                    ) : (
                      <div className="text-center py-8 border rounded-md">
                        <p className="text-muted-foreground">
                          {valueSearchQuery
                            ? "No values match your search"
                            : "No predefined values yet"}
                        </p>
                        {!valueSearchQuery && (
                          <Button
                            variant="link"
                            className="mt-2"
                            onClick={handleAddValue}
                          >
                            Add your first value
                          </Button>
                        )}
                      </div>
                    )}
                  </div>
                )}

                {/* Child Attributes Section - Only for Composite type */}
                {isCompositeType && (
                  <div className="border-t pt-6">
                    <div className="flex items-center justify-between mb-4">
                      <h3 className="font-semibold">
                        Child Attributes ({childAttributes?.length || 0})
                      </h3>
                      <Button size="sm" onClick={handleAddChildAttribute}>
                        <Plus className="mr-2 h-4 w-4" />
                        Add Child Attribute
                      </Button>
                    </div>

                    {isLoadingChildren ? (
                      <div className="space-y-2">
                        {[...Array(3)].map((_, i) => (
                          <Skeleton key={i} className="h-14 w-full" />
                        ))}
                      </div>
                    ) : (childAttributes?.length || 0) > 0 ? (
                      <ScrollArea className="h-[280px] border rounded-md">
                        <div className="p-2">
                          {childAttributes?.map((child) => (
                            <ChildAttributeListItem
                              key={child.id}
                              attribute={child}
                              onEdit={() => handleEditChildAttribute(child)}
                              onManageValues={() => handleManageChildValues(child)}
                              onDelete={() => setDeletingChildAttribute(child)}
                            />
                          ))}
                        </div>
                      </ScrollArea>
                    ) : (
                      <div className="text-center py-8 border rounded-md">
                        <p className="text-muted-foreground">
                          No child attributes yet
                        </p>
                        <Button
                          variant="link"
                          className="mt-2"
                          onClick={handleAddChildAttribute}
                        >
                          Add your first child attribute
                        </Button>
                      </div>
                    )}
                  </div>
                )}
              </CardContent>
            </>
          ) : (
            <div className="flex flex-col items-center justify-center h-[600px] text-center px-4">
              <Settings2 className="h-16 w-16 text-muted-foreground mb-4" />
              <h3 className="text-lg font-semibold">Select an Attribute</h3>
              <p className="text-muted-foreground max-w-sm mt-1">
                Choose an attribute from the list to view its details and manage
                predefined values.
              </p>
            </div>
          )}
        </Card>
      </div>

      {/* Attribute Form Dialog */}
      <Dialog open={isAttributeFormOpen} onOpenChange={setIsAttributeFormOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>
              {editingAttribute ? "Edit Attribute" : `Add ${AttributeEntityTypeLabels[entityTypeTab]} Attribute`}
            </DialogTitle>
            <DialogDescription>
              {editingAttribute
                ? "Update the attribute settings. Code, type, and entity type cannot be changed."
                : `Create a new attribute definition for ${entityTypeTab === AttributeEntityTypeEnum.Product ? "products" : "customers"}.`}
            </DialogDescription>
          </DialogHeader>
          <AttributeForm
            defaultValues={
              editingAttribute
                ? {
                    code: editingAttribute.code,
                    name: editingAttribute.name,
                    type: normalizeType(editingAttribute.type),
                    entityType: normalizeEntityType(editingAttribute.entityType),
                    isList: editingAttribute.isList ?? false,
                    unit: editingAttribute.unit,
                    isFilterable: editingAttribute.isFilterable,
                    isRequired: editingAttribute.isRequired,
                    isVisibleOnProductPage: editingAttribute.isVisibleOnProductPage,
                    displayOrder: editingAttribute.displayOrder,
                  }
                : undefined
            }
            isEditing={!!editingAttribute}
            presetEntityType={editingAttribute ? undefined : entityTypeTab}
            onSubmit={handleAttributeFormSubmit}
            onCancel={() => setIsAttributeFormOpen(false)}
            isLoading={createAttribute.isPending || updateAttribute.isPending}
          />
        </DialogContent>
      </Dialog>

      {/* Value Form Dialog */}
      <Dialog open={isValueFormOpen} onOpenChange={setIsValueFormOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>Add Value</DialogTitle>
            <DialogDescription>
              Add a new predefined value for &quot;{selectedAttribute?.name}&quot;
            </DialogDescription>
          </DialogHeader>
          <AttributeValueForm
            onSubmit={handleValueFormSubmit}
            onCancel={() => setIsValueFormOpen(false)}
            isLoading={addAttributeValue.isPending}
          />
        </DialogContent>
      </Dialog>

      {/* Delete Attribute Confirmation */}
      <ConfirmDialog
        open={!!deletingAttribute}
        onOpenChange={(open) => !open && setDeletingAttribute(null)}
        title="Delete Attribute"
        description={`Are you sure you want to delete "${deletingAttribute?.name}"? This will remove all predefined values and may affect products using this attribute.`}
        confirmText="Delete"
        variant="destructive"
        onConfirm={handleDeleteAttribute}
        isLoading={deleteAttribute.isPending}
      />

      {/* Delete Value Confirmation */}
      <ConfirmDialog
        open={!!deletingValue}
        onOpenChange={(open) => !open && setDeletingValue(null)}
        title="Remove Value"
        description={`Are you sure you want to remove "${deletingValue?.displayText || deletingValue?.value}"? Products using this value may be affected.`}
        confirmText="Remove"
        variant="destructive"
        onConfirm={handleDeleteValue}
        isLoading={removeAttributeValue.isPending}
      />

      {/* Child Attribute Form Dialog */}
      <Dialog
        open={isChildAttributeFormOpen}
        onOpenChange={(open) => {
          setIsChildAttributeFormOpen(open);
          if (!open) setEditingChildAttribute(null);
        }}
      >
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>
              {editingChildAttribute ? "Edit Child Attribute" : "Add Child Attribute"}
            </DialogTitle>
            <DialogDescription>
              {editingChildAttribute
                ? `Update the child attribute "${editingChildAttribute.name}". Code and type cannot be changed.`
                : `Create a new child attribute for the composite attribute "${selectedAttribute?.name}".`}
            </DialogDescription>
          </DialogHeader>
          <AttributeForm
            defaultValues={
              editingChildAttribute
                ? {
                    code: editingChildAttribute.code,
                    name: editingChildAttribute.name,
                    type: normalizeType(editingChildAttribute.type),
                    entityType: normalizeEntityType(editingChildAttribute.entityType),
                    isList: editingChildAttribute.isList ?? false,
                    unit: editingChildAttribute.unit,
                    isFilterable: editingChildAttribute.isFilterable,
                    isRequired: editingChildAttribute.isRequired,
                    isVisibleOnProductPage: editingChildAttribute.isVisibleOnProductPage,
                    displayOrder: editingChildAttribute.displayOrder,
                  }
                : undefined
            }
            isEditing={!!editingChildAttribute}
            presetEntityType={selectedAttribute ? normalizeEntityType(selectedAttribute.entityType) : entityTypeTab}
            onSubmit={handleChildAttributeFormSubmit}
            onCancel={() => {
              setIsChildAttributeFormOpen(false);
              setEditingChildAttribute(null);
            }}
            isLoading={createAttribute.isPending || updateAttribute.isPending}
            isChildAttribute
          />
        </DialogContent>
      </Dialog>

      {/* Delete Child Attribute Confirmation */}
      <ConfirmDialog
        open={!!deletingChildAttribute}
        onOpenChange={(open) => !open && setDeletingChildAttribute(null)}
        title="Delete Child Attribute"
        description={`Are you sure you want to delete "${deletingChildAttribute?.name}"? This will remove this field from the composite attribute structure.`}
        confirmText="Delete"
        variant="destructive"
        onConfirm={handleDeleteChildAttribute}
        isLoading={deleteAttribute.isPending}
      />

      {/* Manage Child Attribute Values Dialog */}
      <Dialog
        open={!!managingValuesChildAttribute}
        onOpenChange={(open) => {
          if (!open) {
            setManagingValuesChildAttribute(null);
            setChildValueSearchQuery("");
          }
        }}
      >
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>
              Manage Values: {currentChildAttribute?.name}
            </DialogTitle>
            <DialogDescription>
              Add or remove predefined values for this child attribute.
            </DialogDescription>
          </DialogHeader>
          <div className="space-y-4">
            <div className="flex items-center justify-between">
              <span className="text-sm text-muted-foreground">
                {currentChildAttribute?.predefinedValues?.length || 0} value(s)
              </span>
              <Button size="sm" onClick={() => setIsChildValueFormOpen(true)}>
                <Plus className="mr-2 h-4 w-4" />
                Add Value
              </Button>
            </div>

            {(currentChildAttribute?.predefinedValues?.length || 0) > 5 && (
              <div className="relative">
                <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                <Input
                  placeholder="Search values..."
                  value={childValueSearchQuery}
                  onChange={(e) => setChildValueSearchQuery(e.target.value)}
                  className="pl-9"
                />
              </div>
            )}

            {filteredChildValues.length > 0 ? (
              <ScrollArea className="h-[280px] border rounded-md">
                <div className="p-2">
                  {filteredChildValues.map((value) => (
                    <ValueListItem
                      key={value.id}
                      value={value}
                      onDelete={() => setDeletingChildValue(value)}
                    />
                  ))}
                </div>
              </ScrollArea>
            ) : (
              <div className="text-center py-8 border rounded-md">
                <p className="text-muted-foreground">
                  {childValueSearchQuery
                    ? "No values match your search"
                    : "No predefined values yet"}
                </p>
                {!childValueSearchQuery && (
                  <Button
                    variant="link"
                    className="mt-2"
                    onClick={() => setIsChildValueFormOpen(true)}
                  >
                    Add your first value
                  </Button>
                )}
              </div>
            )}
          </div>
        </DialogContent>
      </Dialog>

      {/* Child Attribute Value Form Dialog */}
      <Dialog open={isChildValueFormOpen} onOpenChange={setIsChildValueFormOpen}>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>Add Value</DialogTitle>
            <DialogDescription>
              Add a new predefined value for &quot;{currentChildAttribute?.name}&quot;
            </DialogDescription>
          </DialogHeader>
          <AttributeValueForm
            onSubmit={handleChildValueFormSubmit}
            onCancel={() => setIsChildValueFormOpen(false)}
            isLoading={addAttributeValue.isPending}
          />
        </DialogContent>
      </Dialog>

      {/* Delete Child Attribute Value Confirmation */}
      <ConfirmDialog
        open={!!deletingChildValue}
        onOpenChange={(open) => !open && setDeletingChildValue(null)}
        title="Remove Value"
        description={`Are you sure you want to remove "${deletingChildValue?.displayText || deletingChildValue?.value}"? Products using this value may be affected.`}
        confirmText="Remove"
        variant="destructive"
        onConfirm={handleDeleteChildValue}
        isLoading={removeAttributeValue.isPending}
      />
    </div>
  );
}
