"use client";

import { useState, useMemo } from "react";
import { Check, Folder, FolderTree, Search } from "lucide-react";

import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command";
import { Category } from "@/types/entities";
import { useCategories } from "@/hooks/use-categories";

interface CategorySelectorDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  selectedIds: string[];
  onSave: (categoryIds: string[]) => void;
  isLoading?: boolean;
}

interface FlatCategory {
  id: string;
  name: string;
  level: number;
  hasChildren: boolean;
  path: string;
}

function flattenCategories(
  categories: Category[],
  level = 0,
  parentPath = ""
): FlatCategory[] {
  const result: FlatCategory[] = [];

  for (const cat of categories) {
    const path = parentPath ? `${parentPath} / ${cat.name}` : cat.name;
    result.push({
      id: cat.id,
      name: cat.name,
      level,
      hasChildren: !!cat.children && cat.children.length > 0,
      path,
    });

    if (cat.children && cat.children.length > 0) {
      result.push(...flattenCategories(cat.children, level + 1, path));
    }
  }

  return result;
}

export function CategorySelectorDialog({
  open,
  onOpenChange,
  selectedIds,
  onSave,
  isLoading = false,
}: CategorySelectorDialogProps) {
  const { data: categories = [], isLoading: isLoadingCategories } = useCategories();
  const [selection, setSelection] = useState<string[]>(selectedIds);
  const [search, setSearch] = useState("");

  // Reset selection when dialog opens
  const handleOpenChange = (newOpen: boolean) => {
    if (newOpen) {
      setSelection(selectedIds);
      setSearch("");
    }
    onOpenChange(newOpen);
  };

  const flatCategories = useMemo(
    () => flattenCategories(categories),
    [categories]
  );

  const filteredCategories = useMemo(() => {
    if (!search.trim()) return flatCategories;
    const lowerSearch = search.toLowerCase();
    return flatCategories.filter(
      (cat) =>
        cat.name.toLowerCase().includes(lowerSearch) ||
        cat.path.toLowerCase().includes(lowerSearch)
    );
  }, [flatCategories, search]);

  const handleToggle = (categoryId: string) => {
    if (selection.includes(categoryId)) {
      setSelection(selection.filter((id) => id !== categoryId));
    } else {
      setSelection([...selection, categoryId]);
    }
  };

  const handleSave = () => {
    onSave(selection);
  };

  const hasChanges = useMemo(() => {
    if (selection.length !== selectedIds.length) return true;
    const sortedSelection = [...selection].sort();
    const sortedSelected = [...selectedIds].sort();
    return !sortedSelection.every((id, index) => id === sortedSelected[index]);
  }, [selection, selectedIds]);

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent className="max-w-lg">
        <DialogHeader>
          <DialogTitle>Select Categories</DialogTitle>
          <DialogDescription>
            Choose categories to target with this discount rule.
          </DialogDescription>
        </DialogHeader>

        <Command shouldFilter={false} className="border rounded-md">
          <CommandInput
            placeholder="Search categories..."
            value={search}
            onValueChange={setSearch}
          />
          <CommandList className="max-h-[300px]">
            {isLoadingCategories ? (
              <div className="py-6 text-center text-sm text-muted-foreground">
                Loading categories...
              </div>
            ) : (
              <>
                <CommandEmpty>
                  <div className="flex flex-col items-center py-6 text-center">
                    <Search className="h-10 w-10 text-muted-foreground mb-2" />
                    <p className="text-sm text-muted-foreground">
                      No category found.
                    </p>
                  </div>
                </CommandEmpty>
                <CommandGroup>
                  {filteredCategories.map((cat) => {
                    const isSelected = selection.includes(cat.id);
                    return (
                      <CommandItem
                        key={cat.id}
                        value={cat.id}
                        onSelect={() => handleToggle(cat.id)}
                        className="flex items-center gap-2"
                        style={{ paddingLeft: `${cat.level * 16 + 8}px` }}
                      >
                        <Check
                          className={cn(
                            "h-4 w-4 shrink-0",
                            isSelected ? "opacity-100" : "opacity-0"
                          )}
                        />
                        {cat.hasChildren ? (
                          <Folder className="h-4 w-4 shrink-0 text-amber-500" />
                        ) : (
                          <FolderTree className="h-4 w-4 shrink-0 text-muted-foreground" />
                        )}
                        <span className="truncate flex-1">{cat.name}</span>
                      </CommandItem>
                    );
                  })}
                </CommandGroup>
              </>
            )}
          </CommandList>
        </Command>

        <div className="text-sm text-muted-foreground">
          {selection.length} {selection.length === 1 ? "category" : "categories"} selected
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button
            onClick={handleSave}
            disabled={isLoading || !hasChanges}
          >
            {isLoading ? "Saving..." : "Save"}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
