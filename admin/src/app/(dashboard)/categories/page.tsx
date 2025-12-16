"use client";

import { useState, useEffect, useMemo } from "react";
import {
  Plus,
  ChevronRight,
  ChevronDown,
  FolderTree,
  MoreHorizontal,
  Pencil,
  Trash2,
  Folder,
  FolderOpen,
  Search,
  X,
  Download,
  FileSpreadsheet,
  FileText,
} from "lucide-react";
import { toast } from "sonner";

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
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { CategoryForm } from "@/components/forms/category-form";
import {
  useCategories,
  useCreateCategory,
  useUpdateCategory,
  useDeleteCategory,
} from "@/hooks/use-categories";
import { Category } from "@/types/entities";
import { CategoryFormData } from "@/lib/validations/category";
import { cn } from "@/lib/utils";
import { exportToExcel, exportToCSV, flattenCategories } from "@/lib/export";

// Helper to highlight matching text
function HighlightText({ text, highlight }: { text: string; highlight: string }) {
  if (!highlight.trim()) {
    return <span>{text}</span>;
  }

  const regex = new RegExp(`(${highlight.replace(/[.*+?^${}()|[\]\\]/g, "\\$&")})`, "gi");
  const parts = text.split(regex);

  return (
    <span>
      {parts.map((part, i) =>
        regex.test(part) ? (
          <mark key={i} className="bg-yellow-200 dark:bg-yellow-800 rounded px-0.5">
            {part}
          </mark>
        ) : (
          <span key={i}>{part}</span>
        )
      )}
    </span>
  );
}

// Filter categories recursively and keep parent path to matches
function filterCategories(categories: Category[], query: string): Category[] {
  if (!query.trim()) return categories;

  const lowerQuery = query.toLowerCase();

  return categories.reduce<Category[]>((acc, cat) => {
    const matchesSearch = cat.name.toLowerCase().includes(lowerQuery);
    const filteredChildren = cat.children ? filterCategories(cat.children, query) : [];

    if (matchesSearch || filteredChildren.length > 0) {
      acc.push({
        ...cat,
        children: filteredChildren,
      });
    }
    return acc;
  }, []);
}

// Collect all category IDs that have children (for expanding when searching)
function collectParentIds(categories: Category[], query: string): Set<string> {
  const ids = new Set<string>();
  const lowerQuery = query.toLowerCase();

  const traverse = (cats: Category[], parentIds: string[]) => {
    cats.forEach((cat) => {
      const matchesSearch = cat.name.toLowerCase().includes(lowerQuery);
      const hasChildren = cat.children && cat.children.length > 0;

      if (hasChildren) {
        const childIds = [...parentIds, cat.id];
        traverse(cat.children!, childIds);

        // If any descendant matches, expand this category
        const hasMatchingDescendant = cat.children!.some(
          (child) =>
            child.name.toLowerCase().includes(lowerQuery) ||
            (child.children && filterCategories([child], query).length > 0)
        );

        if (matchesSearch || hasMatchingDescendant) {
          parentIds.forEach((id) => ids.add(id));
          ids.add(cat.id);
        }
      }
    });
  };

  traverse(categories, []);
  return ids;
}

interface CategoryTreeItemProps {
  category: Category;
  level: number;
  expandedIds: Set<string>;
  searchQuery: string;
  onToggle: (id: string) => void;
  onEdit: (category: Category) => void;
  onDelete: (category: Category) => void;
  onAddChild: (parentId: string) => void;
  isLastChild?: boolean;
  parentIsLast?: boolean[];
}

function CategoryTreeItem({
  category,
  level,
  expandedIds,
  searchQuery,
  onToggle,
  onEdit,
  onDelete,
  onAddChild,
  isLastChild = false,
  parentIsLast = [],
}: CategoryTreeItemProps) {
  const hasChildren = category.children && category.children.length > 0;
  const isExpanded = expandedIds.has(category.id);

  return (
    <div className="relative">
      {/* Vertical connection lines for ancestors */}
      {parentIsLast.map((isLast, idx) =>
        !isLast ? (
          <div
            key={idx}
            className="absolute top-0 bottom-0 w-px bg-border"
            style={{ left: `${idx * 24 + 16}px` }}
          />
        ) : null
      )}

      {/* Horizontal and vertical line to this item */}
      {level > 0 && (
        <>
          {/* Vertical line segment */}
          <div
            className={cn(
              "absolute w-px bg-border",
              isLastChild ? "top-0 h-[20px]" : "top-0 bottom-0"
            )}
            style={{ left: `${(level - 1) * 24 + 16}px` }}
          />
          {/* Horizontal line segment */}
          <div
            className="absolute h-px bg-border top-[20px]"
            style={{
              left: `${(level - 1) * 24 + 16}px`,
              width: "12px",
            }}
          />
        </>
      )}

      <div
        className={cn(
          "flex items-center gap-2 py-1.5 px-2 rounded-md hover:bg-muted/50 group relative"
        )}
        style={{ paddingLeft: `${level * 24 + 8}px` }}
      >
        {/* Expand/collapse button */}
        <button
          onClick={() => hasChildren && onToggle(category.id)}
          className={cn(
            "w-5 h-5 flex items-center justify-center rounded hover:bg-muted",
            hasChildren && "cursor-pointer"
          )}
          disabled={!hasChildren}
        >
          {hasChildren ? (
            isExpanded ? (
              <ChevronDown className="h-4 w-4 text-muted-foreground" />
            ) : (
              <ChevronRight className="h-4 w-4 text-muted-foreground" />
            )
          ) : (
            <span className="w-4" />
          )}
        </button>

        {/* Folder icon - changes based on state */}
        {hasChildren ? (
          isExpanded ? (
            <FolderOpen className="h-4 w-4 text-amber-500 flex-shrink-0" />
          ) : (
            <Folder className="h-4 w-4 text-amber-500 flex-shrink-0" />
          )
        ) : (
          <FolderTree className="h-4 w-4 text-muted-foreground flex-shrink-0" />
        )}

        {/* Category name and info */}
        <div className="flex-1 min-w-0">
          <div className="flex items-center gap-2">
            <span className="font-medium truncate">
              <HighlightText text={category.name} highlight={searchQuery} />
            </span>
            <Badge
              variant={category.isActive ? "default" : "secondary"}
              className="text-xs flex-shrink-0"
            >
              {category.isActive ? "Active" : "Inactive"}
            </Badge>
            {category.productCount > 0 && (
              <span className="text-xs text-muted-foreground flex-shrink-0">
                ({category.productCount} products)
              </span>
            )}
            {hasChildren && (
              <span className="text-xs text-muted-foreground flex-shrink-0">
                {category.children!.length} subcategories
              </span>
            )}
          </div>
        </div>

        {/* Quick add subcategory button */}
        <Button
          variant="ghost"
          size="icon"
          className="h-7 w-7 opacity-0 group-hover:opacity-100 flex-shrink-0"
          onClick={() => onAddChild(category.id)}
          title="Add subcategory"
        >
          <Plus className="h-4 w-4" />
        </Button>

        {/* Actions dropdown */}
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button
              variant="ghost"
              size="icon"
              className="h-7 w-7 opacity-0 group-hover:opacity-100 flex-shrink-0"
            >
              <MoreHorizontal className="h-4 w-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuItem onClick={() => onEdit(category)}>
              <Pencil className="mr-2 h-4 w-4" />
              Edit
            </DropdownMenuItem>
            <DropdownMenuItem
              className="text-destructive"
              onClick={() => onDelete(category)}
            >
              <Trash2 className="mr-2 h-4 w-4" />
              Delete
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      </div>

      {/* Children */}
      {hasChildren && isExpanded && (
        <div>
          {category.children!.map((child, idx) => (
            <CategoryTreeItem
              key={child.id}
              category={child}
              level={level + 1}
              expandedIds={expandedIds}
              searchQuery={searchQuery}
              onToggle={onToggle}
              onEdit={onEdit}
              onDelete={onDelete}
              onAddChild={onAddChild}
              isLastChild={idx === category.children!.length - 1}
              parentIsLast={[...parentIsLast, isLastChild]}
            />
          ))}
        </div>
      )}
    </div>
  );
}

export default function CategoriesPage() {
  const [expandedIds, setExpandedIds] = useState<Set<string>>(new Set());
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingCategory, setEditingCategory] = useState<Category | null>(null);
  const [deletingCategory, setDeletingCategory] = useState<Category | null>(null);
  const [parentIdForNew, setParentIdForNew] = useState<string | undefined>();
  const [searchQuery, setSearchQuery] = useState("");

  const { data: categories, isLoading } = useCategories();
  const createCategory = useCreateCategory();
  const updateCategory = useUpdateCategory();
  const deleteCategory = useDeleteCategory();

  // Filter categories based on search query
  const filteredCategories = useMemo(() => {
    if (!categories) return [];
    return filterCategories(categories, searchQuery);
  }, [categories, searchQuery]);

  // Auto-expand when searching
  useEffect(() => {
    if (searchQuery && categories) {
      const idsToExpand = collectParentIds(categories, searchQuery);
      setExpandedIds(idsToExpand);
    }
  }, [searchQuery, categories]);

  const handleToggle = (id: string) => {
    setExpandedIds((prev) => {
      const next = new Set(prev);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  };

  const handleExpandAll = () => {
    const allIds = new Set<string>();
    const collectIds = (cats: Category[]) => {
      cats.forEach((cat) => {
        if (cat.children && cat.children.length > 0) {
          allIds.add(cat.id);
          collectIds(cat.children);
        }
      });
    };
    if (categories) {
      collectIds(categories);
    }
    setExpandedIds(allIds);
  };

  const handleCollapseAll = () => {
    setExpandedIds(new Set());
  };

  const handleClearSearch = () => {
    setSearchQuery("");
  };

  const handleAddNew = (parentId?: string) => {
    setParentIdForNew(parentId);
    setEditingCategory(null);
    setIsFormOpen(true);
  };

  const handleEdit = (category: Category) => {
    setEditingCategory(category);
    setParentIdForNew(undefined);
    setIsFormOpen(true);
  };

  const handleFormSubmit = async (data: CategoryFormData) => {
    // Map form's parentId to API's parentCategoryId
    const parentCategoryId = parentIdForNew || data.parentId || undefined;

    if (editingCategory) {
      await updateCategory.mutateAsync({
        id: editingCategory.id,
        data: {
          name: data.name,
          description: data.description,
          parentCategoryId,
          imageUrl: data.imageUrl,
          displayOrder: data.displayOrder,
          isActive: data.isActive,
        },
      });
    } else {
      await createCategory.mutateAsync({
        name: data.name,
        description: data.description,
        parentCategoryId,
        imageUrl: data.imageUrl,
        displayOrder: data.displayOrder,
        isActive: data.isActive,
      });
    }
    setIsFormOpen(false);
    setEditingCategory(null);
    setParentIdForNew(undefined);
  };

  const handleDelete = async () => {
    if (deletingCategory) {
      await deleteCategory.mutateAsync(deletingCategory.id);
      setDeletingCategory(null);
    }
  };

  const handleExport = (format: "xlsx" | "csv") => {
    if (!categories?.length) return;

    const flatData = flattenCategories(categories);
    const columns = [
      { key: "name" as const, header: "Name" },
      { key: "parentName" as const, header: "Parent Category" },
      { key: "path" as const, header: "Full Path" },
      { key: "description" as const, header: "Description" },
      { key: "displayOrder" as const, header: "Display Order" },
      { key: "isActive" as const, header: "Active" },
      { key: "productCount" as const, header: "Product Count" },
      { key: "level" as const, header: "Level" },
    ];

    const filename = `categories-${new Date().toISOString().split("T")[0]}`;

    if (format === "xlsx") {
      exportToExcel(flatData, columns, filename, "Categories");
    } else {
      exportToCSV(flatData, columns, filename);
    }

    toast.success(`Categories exported as ${format.toUpperCase()}`);
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Categories</h1>
          <p className="text-muted-foreground">
            Manage your product category hierarchy
          </p>
        </div>
        <Button onClick={() => handleAddNew()}>
          <Plus className="mr-2 h-4 w-4" />
          Add Category
        </Button>
      </div>

      <Card>
        <CardHeader className="pb-4">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <CardTitle>Category Tree</CardTitle>
              <CardDescription>
                Click on categories to expand/collapse subcategories
              </CardDescription>
            </div>
            <div className="flex gap-2">
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button
                    variant="outline"
                    size="sm"
                    disabled={!categories?.length}
                  >
                    <Download className="mr-2 h-4 w-4" />
                    Export
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent>
                  <DropdownMenuItem onClick={() => handleExport("xlsx")}>
                    <FileSpreadsheet className="mr-2 h-4 w-4" />
                    Excel (.xlsx)
                  </DropdownMenuItem>
                  <DropdownMenuItem onClick={() => handleExport("csv")}>
                    <FileText className="mr-2 h-4 w-4" />
                    CSV (.csv)
                  </DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
              <Button variant="outline" size="sm" onClick={handleExpandAll}>
                Expand All
              </Button>
              <Button variant="outline" size="sm" onClick={handleCollapseAll}>
                Collapse All
              </Button>
            </div>
          </div>

          {/* Search input */}
          <div className="relative mt-4">
            <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <Input
              placeholder="Search categories..."
              value={searchQuery}
              onChange={(e) => setSearchQuery(e.target.value)}
              className="pl-9 pr-9"
            />
            {searchQuery && (
              <Button
                variant="ghost"
                size="icon"
                className="absolute right-1 top-1/2 h-7 w-7 -translate-y-1/2"
                onClick={handleClearSearch}
              >
                <X className="h-4 w-4" />
              </Button>
            )}
          </div>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="space-y-2">
              {[...Array(5)].map((_, i) => (
                <Skeleton key={i} className="h-10 w-full" />
              ))}
            </div>
          ) : filteredCategories.length > 0 ? (
            <div className="space-y-0.5">
              {filteredCategories.map((category, idx) => (
                <CategoryTreeItem
                  key={category.id}
                  category={category}
                  level={0}
                  expandedIds={expandedIds}
                  searchQuery={searchQuery}
                  onToggle={handleToggle}
                  onEdit={handleEdit}
                  onDelete={setDeletingCategory}
                  onAddChild={handleAddNew}
                  isLastChild={idx === filteredCategories.length - 1}
                />
              ))}
            </div>
          ) : searchQuery ? (
            <div className="text-center py-10">
              <Search className="mx-auto h-12 w-12 text-muted-foreground" />
              <h3 className="mt-4 text-lg font-semibold">No results found</h3>
              <p className="text-muted-foreground">
                No categories match &quot;{searchQuery}&quot;
              </p>
              <Button
                variant="outline"
                className="mt-4"
                onClick={handleClearSearch}
              >
                Clear search
              </Button>
            </div>
          ) : (
            <div className="text-center py-10">
              <FolderTree className="mx-auto h-12 w-12 text-muted-foreground" />
              <h3 className="mt-4 text-lg font-semibold">No categories</h3>
              <p className="text-muted-foreground">
                Get started by creating your first category.
              </p>
              <Button className="mt-4" onClick={() => handleAddNew()}>
                <Plus className="mr-2 h-4 w-4" />
                Add Category
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Category Form Dialog */}
      <Dialog open={isFormOpen} onOpenChange={setIsFormOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>
              {editingCategory ? "Edit Category" : "Add Category"}
            </DialogTitle>
            <DialogDescription>
              {editingCategory
                ? "Update the category information below."
                : "Fill in the details to create a new category."}
            </DialogDescription>
          </DialogHeader>
          <CategoryForm
            defaultValues={
              editingCategory
                ? {
                  name: editingCategory.name,
                  description: editingCategory.description,
                  parentId: editingCategory.parentId,
                  imageUrl: editingCategory.imageUrl,
                  displayOrder: editingCategory.displayOrder,
                  isActive: editingCategory.isActive,
                }
                : {
                  parentId: parentIdForNew,
                }
            }
            categories={categories || []}
            excludeId={editingCategory?.id}
            onSubmit={handleFormSubmit}
            onCancel={() => setIsFormOpen(false)}
            isLoading={createCategory.isPending || updateCategory.isPending}
          />
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation */}
      <ConfirmDialog
        open={!!deletingCategory}
        onOpenChange={(open) => !open && setDeletingCategory(null)}
        title="Delete Category"
        description={`Are you sure you want to delete "${deletingCategory?.name}"? This action cannot be undone and may affect associated products.`}
        confirmText="Delete"
        variant="destructive"
        onConfirm={handleDelete}
        isLoading={deleteCategory.isPending}
      />
    </div>
  );
}
