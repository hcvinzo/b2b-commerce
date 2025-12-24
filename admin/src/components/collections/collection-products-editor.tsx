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
  Star,
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
import { Switch } from "@/components/ui/switch";
import {
  useCollectionProducts,
  useSetCollectionProducts,
} from "@/hooks/use-collections";
import { useSearchProductsForSelection } from "@/hooks/use-product-relations";
import { ProductInCollection, ProductListItem, Collection } from "@/types/entities";
import { formatCurrency } from "@/lib/utils";
import { useDebounce } from "@/hooks/use-debounce";

const MAX_PRODUCTS = 50;

interface CollectionProductsEditorProps {
  collection: Collection;
}

interface SortableProductItemProps {
  product: ProductInCollection;
  onRemove: () => void;
  onToggleFeatured: () => void;
}

function SortableProductItem({
  product,
  onRemove,
  onToggleFeatured,
}: SortableProductItemProps) {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging,
  } = useSortable({ id: product.productId });

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

      {product.productImageUrl && (
        <img
          src={product.productImageUrl}
          alt={product.productName}
          className="h-10 w-10 rounded object-cover"
        />
      )}

      <div className="flex-1 min-w-0">
        <p className="font-medium truncate">{product.productName}</p>
        <p className="text-sm text-muted-foreground">{product.productSku}</p>
      </div>

      <div className="text-right">
        <p className="font-medium">{formatCurrency(product.productPrice)}</p>
        <Badge
          variant={product.productIsActive ? "default" : "secondary"}
          className="text-xs"
        >
          {product.productIsActive ? "Active" : "Inactive"}
        </Badge>
      </div>

      <div className="flex items-center gap-1">
        <Button
          variant="ghost"
          size="icon"
          onClick={onToggleFeatured}
          className={product.isFeatured ? "text-yellow-500" : "text-muted-foreground"}
          title={product.isFeatured ? "Unmark as featured" : "Mark as featured"}
        >
          <Star
            className="h-4 w-4"
            fill={product.isFeatured ? "currentColor" : "none"}
          />
        </Button>

        <Button
          variant="ghost"
          size="icon"
          onClick={onRemove}
          className="text-destructive hover:text-destructive"
        >
          <Trash2 className="h-4 w-4" />
        </Button>
      </div>
    </div>
  );
}

export function CollectionProductsEditor({
  collection,
}: CollectionProductsEditorProps) {
  const { data: productsData, isLoading } = useCollectionProducts(collection.id);
  const setProducts = useSetCollectionProducts();

  const [localProducts, setLocalProducts] = useState<ProductInCollection[]>([]);
  const [hasChanges, setHasChanges] = useState(false);
  const [searchOpen, setSearchOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState("");
  const debouncedSearch = useDebounce(searchQuery, 300);

  const { data: searchResults, isLoading: isSearching } =
    useSearchProductsForSelection(debouncedSearch);

  const sensors = useSensors(
    useSensor(PointerSensor),
    useSensor(KeyboardSensor, {
      coordinateGetter: sortableKeyboardCoordinates,
    })
  );

  // Initialize local state from server data
  useEffect(() => {
    if (productsData?.items) {
      setLocalProducts(productsData.items);
      setHasChanges(false);
    }
  }, [productsData]);

  const existingProductIds = useMemo(
    () => new Set(localProducts.map((p) => p.productId)),
    [localProducts]
  );

  const filteredSearchResults = useMemo(
    () => searchResults?.filter((p) => !existingProductIds.has(p.id)) || [],
    [searchResults, existingProductIds]
  );

  const handleDragEnd = (event: DragEndEvent) => {
    const { active, over } = event;
    if (over && active.id !== over.id) {
      const oldIndex = localProducts.findIndex((p) => p.productId === active.id);
      const newIndex = localProducts.findIndex((p) => p.productId === over.id);
      const newProducts = arrayMove(localProducts, oldIndex, newIndex).map(
        (p, index) => ({
          ...p,
          displayOrder: index,
        })
      );
      setLocalProducts(newProducts);
      setHasChanges(true);
    }
  };

  const handleRemove = (productId: string) => {
    const newProducts = localProducts
      .filter((p) => p.productId !== productId)
      .map((p, index) => ({
        ...p,
        displayOrder: index,
      }));
    setLocalProducts(newProducts);
    setHasChanges(true);
  };

  const handleToggleFeatured = (productId: string) => {
    const newProducts = localProducts.map((p) =>
      p.productId === productId ? { ...p, isFeatured: !p.isFeatured } : p
    );
    setLocalProducts(newProducts);
    setHasChanges(true);
  };

  const handleAddProduct = (product: ProductListItem) => {
    if (localProducts.length >= MAX_PRODUCTS) {
      return;
    }

    const newProduct: ProductInCollection = {
      productId: product.id,
      productName: product.name,
      productSku: product.sku,
      productImageUrl: product.mainImageUrl,
      productPrice: product.listPrice,
      productIsActive: product.isActive,
      displayOrder: localProducts.length,
      isFeatured: false,
    };

    setLocalProducts([...localProducts, newProduct]);
    setHasChanges(true);
    setSearchOpen(false);
    setSearchQuery("");
  };

  const handleSave = async () => {
    await setProducts.mutateAsync({
      collectionId: collection.id,
      data: {
        products: localProducts.map((p) => ({
          productId: p.productId,
          displayOrder: p.displayOrder,
          isFeatured: p.isFeatured,
        })),
      },
    });
    setHasChanges(false);
  };

  const handleReset = () => {
    if (productsData?.items) {
      setLocalProducts(productsData.items);
      setHasChanges(false);
    }
  };

  const canAdd = localProducts.length < MAX_PRODUCTS;

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
        <CardTitle>Collection Products</CardTitle>
        <CardDescription>
          Manage products in this collection. Drag to reorder, max {MAX_PRODUCTS}{" "}
          products.
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        {/* Unsaved changes bar */}
        {hasChanges && (
          <div className="flex items-center justify-between rounded-lg border border-yellow-200 bg-yellow-50 p-4 dark:border-yellow-900 dark:bg-yellow-950">
            <p className="text-sm text-yellow-800 dark:text-yellow-200">
              You have unsaved changes
            </p>
            <div className="flex gap-2">
              <Button variant="outline" size="sm" onClick={handleReset}>
                <X className="mr-2 h-4 w-4" />
                Reset
              </Button>
              <Button
                size="sm"
                onClick={handleSave}
                disabled={setProducts.isPending}
              >
                {setProducts.isPending && (
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                )}
                <Check className="mr-2 h-4 w-4" />
                Save Changes
              </Button>
            </div>
          </div>
        )}

        {/* Add Product Button */}
        <div className="flex items-center justify-between">
          <p className="text-sm text-muted-foreground">
            {localProducts.length} of {MAX_PRODUCTS} products
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
        {localProducts.length === 0 ? (
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
              items={localProducts.map((p) => p.productId)}
              strategy={verticalListSortingStrategy}
            >
              <div className="space-y-2">
                {localProducts.map((product) => (
                  <SortableProductItem
                    key={product.productId}
                    product={product}
                    onRemove={() => handleRemove(product.productId)}
                    onToggleFeatured={() => handleToggleFeatured(product.productId)}
                  />
                ))}
              </div>
            </SortableContext>
          </DndContext>
        )}
      </CardContent>
    </Card>
  );
}
