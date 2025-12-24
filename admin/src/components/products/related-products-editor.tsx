"use client";

import { useState, useEffect, useMemo } from "react";
import {
  Loader2,
  Check,
  X,
  Search,
  GripVertical,
  Trash2,
  Plus,
} from "lucide-react";
import {
  DndContext,
  closestCenter,
  KeyboardSensor,
  PointerSensor,
  useSensor,
  useSensors,
  DragEndEvent,
} from "@dnd-kit/core";
import {
  arrayMove,
  SortableContext,
  sortableKeyboardCoordinates,
  useSortable,
  verticalListSortingStrategy,
} from "@dnd-kit/sortable";
import { CSS } from "@dnd-kit/utilities";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Badge } from "@/components/ui/badge";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandItem,
  CommandList,
} from "@/components/ui/command";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import {
  useProductRelations,
  useSetProductRelations,
  useSearchProductsForSelection,
} from "@/hooks/use-product-relations";
import {
  ProductRelationType,
  ProductRelationTypeEnum,
  ProductRelationTypeLabels,
  ProductRelation,
  RelatedProductInput,
  ProductListItem,
} from "@/types/entities";
import { formatCurrency } from "@/lib/utils";
import { useDebounce } from "@/hooks/use-debounce";

const MAX_RELATED_PRODUCTS = 10;

interface RelatedProductsEditorProps {
  productId: string;
}

interface SortableProductItemProps {
  relation: ProductRelation;
  onRemove: () => void;
}

function SortableProductItem({ relation, onRemove }: SortableProductItemProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: relation.relatedProductId });

  const style = {
    transform: CSS.Transform.toString(transform),
    transition,
    opacity: isDragging ? 0.5 : 1,
  };

  return (
    <div
      ref={setNodeRef}
      style={style}
      className="flex items-center gap-3 p-3 rounded-lg border bg-card hover:bg-accent/50"
    >
      <button
        type="button"
        className="cursor-grab touch-none"
        {...attributes}
        {...listeners}
      >
        <GripVertical className="h-4 w-4 text-muted-foreground" />
      </button>

      {relation.relatedProductImageUrl && (
        <img
          src={relation.relatedProductImageUrl}
          alt={relation.relatedProductName}
          className="h-10 w-10 rounded object-cover"
        />
      )}

      <div className="flex-1 min-w-0">
        <p className="font-medium truncate">{relation.relatedProductName}</p>
        <p className="text-sm text-muted-foreground">{relation.relatedProductSku}</p>
      </div>

      <div className="text-right">
        <p className="font-medium">{formatCurrency(relation.relatedProductPrice)}</p>
        <Badge
          variant={relation.relatedProductIsActive ? "default" : "secondary"}
          className="text-xs"
        >
          {relation.relatedProductIsActive ? "Active" : "Inactive"}
        </Badge>
      </div>

      <Button
        variant="ghost"
        size="icon"
        onClick={onRemove}
        className="text-destructive hover:text-destructive"
      >
        <Trash2 className="h-4 w-4" />
      </Button>
    </div>
  );
}

interface RelationTabContentProps {
  productId: string;
  relationType: ProductRelationType;
  relations: ProductRelation[];
  onRelationsChange: (relations: ProductRelation[]) => void;
  hasChanges: boolean;
  onSave: () => void;
  onReset: () => void;
  isSaving: boolean;
}

function RelationTabContent({
  productId,
  relationType,
  relations,
  onRelationsChange,
  hasChanges,
  onSave,
  onReset,
  isSaving,
}: RelationTabContentProps) {
  const [searchOpen, setSearchOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");
  const debouncedSearch = useDebounce(searchQuery, 300);

  const { data: searchResults, isLoading: isSearching } =
    useSearchProductsForSelection(debouncedSearch, productId);

  const sensors = useSensors(
    useSensor(PointerSensor),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  const existingProductIds = useMemo(
    () => new Set(relations.map((r) => r.relatedProductId)),
    [relations]
  );

  const filteredSearchResults = useMemo(
    () => searchResults?.filter((p) => !existingProductIds.has(p.id)) || [],
    [searchResults, existingProductIds]
  );

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;
    if (over && active.id !== over.id) {
      const oldIndex = relations.findIndex(
        (r) => r.relatedProductId === active.id
      );
      const newIndex = relations.findIndex(
        (r) => r.relatedProductId === over.id
      );
      const newRelations = arrayMove(relations, oldIndex, newIndex).map(
        (r, index) => ({
          ...r,
          displayOrder: index,
        })
      );
      onRelationsChange(newRelations);
    }
  };

  const handleRemove = (relatedProductId: string) => {
    const newRelations = relations
      .filter((r) => r.relatedProductId !== relatedProductId)
      .map((r, index) => ({
        ...r,
        displayOrder: index,
      }));
    onRelationsChange(newRelations);
  };

  const handleAddProduct = (product: ProductListItem) => {
    if (relations.length >= MAX_RELATED_PRODUCTS) {
      return;
    }

    const newRelation: ProductRelation = {
      id: "", // Will be assigned by backend
      relatedProductId: product.id,
      relatedProductName: product.name,
      relatedProductSku: product.sku,
      relatedProductImageUrl: product.mainImageUrl,
      relatedProductPrice: product.listPrice,
      relatedProductIsActive: product.isActive,
      relationType,
      displayOrder: relations.length,
    };

    onRelationsChange([...relations, newRelation]);
    setSearchOpen(false);
    setSearchQuery("");
  };

  const canAdd = relations.length < MAX_RELATED_PRODUCTS;

  return (
    <div className="space-y-4">
      {/* Unsaved changes bar */}
      {hasChanges && (
        <div className="flex items-center justify-between rounded-lg border border-yellow-200 bg-yellow-50 p-4 dark:border-yellow-900 dark:bg-yellow-950">
          <p className="text-sm text-yellow-800 dark:text-yellow-200">
            You have unsaved changes
          </p>
          <div className="flex gap-2">
            <Button variant="outline" size="sm" onClick={onReset}>
              <X className="mr-2 h-4 w-4" />
              Reset
            </Button>
            <Button size="sm" onClick={onSave} disabled={isSaving}>
              {isSaving && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              <Check className="mr-2 h-4 w-4" />
              Save Changes
            </Button>
          </div>
        </div>
      )}

      {/* Add Product Button */}
      <div className="flex items-center justify-between">
        <p className="text-sm text-muted-foreground">
          {relations.length} of {MAX_RELATED_PRODUCTS} products
        </p>
        <Popover open={searchOpen} onOpenChange={setSearchOpen}>
          <PopoverTrigger asChild>
            <Button variant="outline" size="sm" disabled={!canAdd}>
              <Plus className="mr-2 h-4 w-4" />
              Add Product
            </Button>
          </PopoverTrigger>
          <PopoverContent className="w-[400px] p-0" align="end">
            <Command shouldFilter={false}>
              <div className="flex items-center border-b px-3">
                <Search className="mr-2 h-4 w-4 shrink-0 opacity-50" />
                <Input
                  placeholder="Search products..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="border-0 focus-visible:ring-0 focus-visible:ring-offset-0"
                />
              </div>
              <CommandList className="max-h-[300px]">
                {isSearching ? (
                  <div className="flex items-center justify-center py-6">
                    <Loader2 className="h-6 w-6 animate-spin text-muted-foreground" />
                  </div>
                ) : debouncedSearch.length < 2 ? (
                  <CommandEmpty>Type at least 2 characters to search</CommandEmpty>
                ) : filteredSearchResults.length === 0 ? (
                  <CommandEmpty>No products found</CommandEmpty>
                ) : (
                  <CommandGroup>
                    {filteredSearchResults.map((product) => (
                      <CommandItem
                        key={product.id}
                        value={product.id}
                        onSelect={() => handleAddProduct(product)}
                        className="cursor-pointer"
                      >
                        <div className="flex items-center gap-3 w-full">
                          {product.mainImageUrl && (
                            <img
                              src={product.mainImageUrl}
                              alt={product.name}
                              className="h-8 w-8 rounded object-cover"
                            />
                          )}
                          <div className="flex-1 min-w-0">
                            <p className="font-medium truncate">{product.name}</p>
                            <p className="text-xs text-muted-foreground">
                              {product.sku}
                            </p>
                          </div>
                          <p className="text-sm font-medium">
                            {formatCurrency(product.listPrice)}
                          </p>
                        </div>
                      </CommandItem>
                    ))}
                  </CommandGroup>
                )}
              </CommandList>
            </Command>
          </PopoverContent>
        </Popover>
      </div>

      {/* Product List with Drag and Drop */}
      {relations.length === 0 ? (
        <div className="text-center py-8 text-muted-foreground">
          No products added. Click &quot;Add Product&quot; to get started.
        </div>
      ) : (
        <DndContext
          sensors={sensors}
          collisionDetection={closestCenter}
          onDragEnd={handleDragEnd}
        >
          <SortableContext
            items={relations.map((r) => r.relatedProductId)}
            strategy={verticalListSortingStrategy}
          >
            <div className="space-y-2">
              {relations.map((relation) => (
                <SortableProductItem
                  key={relation.relatedProductId}
                  relation={relation}
                  onRemove={() => handleRemove(relation.relatedProductId)}
                />
              ))}
            </div>
          </SortableContext>
        </DndContext>
      )}
    </div>
  );
}

export function RelatedProductsEditor({ productId }: RelatedProductsEditorProps) {
  const { data: relationGroups, isLoading } = useProductRelations(productId);
  const setRelations = useSetProductRelations();

  // Track local state for each relation type
  const [localRelations, setLocalRelations] = useState<
    Record<ProductRelationType, ProductRelation[]>
  >({
    Related: [],
    CrossSell: [],
    UpSell: [],
    Accessories: [],
  });

  const [changedTypes, setChangedTypes] = useState<Set<ProductRelationType>>(
    new Set()
  );

  // Initialize local state from server data
  useEffect(() => {
    if (relationGroups) {
      const newState: Record<ProductRelationType, ProductRelation[]> = {
        Related: [],
        CrossSell: [],
        UpSell: [],
        Accessories: [],
      };
      for (const group of relationGroups) {
        newState[group.relationType] = group.relations;
      }
      setLocalRelations(newState);
      setChangedTypes(new Set());
    }
  }, [relationGroups]);

  const handleRelationsChange = (
    relationType: ProductRelationType,
    relations: ProductRelation[]
  ) => {
    setLocalRelations((prev) => ({
      ...prev,
      [relationType]: relations,
    }));
    setChangedTypes((prev) => new Set([...prev, relationType]));
  };

  const handleSave = async (relationType: ProductRelationType) => {
    const relatedProducts: RelatedProductInput[] = localRelations[
      relationType
    ].map((r) => ({
      productId: r.relatedProductId,
      displayOrder: r.displayOrder,
    }));

    await setRelations.mutateAsync({
      productId,
      relationType,
      relatedProducts,
    });

    setChangedTypes((prev) => {
      const newSet = new Set(prev);
      newSet.delete(relationType);
      return newSet;
    });
  };

  const handleReset = (relationType: ProductRelationType) => {
    if (relationGroups) {
      const group = relationGroups.find((g) => g.relationType === relationType);
      setLocalRelations((prev) => ({
        ...prev,
        [relationType]: group?.relations || [],
      }));
      setChangedTypes((prev) => {
        const newSet = new Set(prev);
        newSet.delete(relationType);
        return newSet;
      });
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-8">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Related Products</CardTitle>
        <CardDescription>
          Manage product relationships for cross-selling and recommendations.
          Drag to reorder, max {MAX_RELATED_PRODUCTS} products per type.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <Tabs defaultValue="Related">
          <TabsList className="w-full justify-start">
            {ProductRelationTypeEnum.map((type) => (
              <TabsTrigger key={type} value={type} className="relative">
                {ProductRelationTypeLabels[type]}
                {localRelations[type].length > 0 && (
                  <Badge
                    variant="secondary"
                    className="ml-2 h-5 w-5 rounded-full p-0 flex items-center justify-center text-xs"
                  >
                    {localRelations[type].length}
                  </Badge>
                )}
                {changedTypes.has(type) && (
                  <span className="absolute -top-1 -right-1 h-2 w-2 rounded-full bg-yellow-500" />
                )}
              </TabsTrigger>
            ))}
          </TabsList>

          {ProductRelationTypeEnum.map((type) => (
            <TabsContent key={type} value={type} className="mt-4">
              <RelationTabContent
                productId={productId}
                relationType={type}
                relations={localRelations[type]}
                onRelationsChange={(relations) =>
                  handleRelationsChange(type, relations)
                }
                hasChanges={changedTypes.has(type)}
                onSave={() => handleSave(type)}
                onReset={() => handleReset(type)}
                isSaving={setRelations.isPending}
              />
            </TabsContent>
          ))}
        </Tabs>
      </CardContent>
    </Card>
  );
}
