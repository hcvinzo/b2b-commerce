"use client";

import { useState, useMemo } from "react";
import {
  ChevronsUpDown,
  Check,
  Folder,
  FolderTree,
  Search,
  X,
  Star,
} from "lucide-react";

import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Command,
  CommandEmpty,
  CommandGroup,
  CommandInput,
  CommandItem,
  CommandList,
} from "@/components/ui/command";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Category } from "@/types/entities";

interface TreeMultiSelectProps {
  categories: Category[];
  value: string[];
  onChange: (value: string[]) => void;
  excludeId?: string;
  placeholder?: string;
  disabled?: boolean;
}

// Flatten tree with level info for display
interface FlatCategory {
  id: string;
  name: string;
  level: number;
  hasChildren: boolean;
  path: string;
}

function flattenCategories(
  categories: Category[],
  excludeId?: string,
  level = 0,
  parentPath = ""
): FlatCategory[] {
  const result: FlatCategory[] = [];

  for (const cat of categories) {
    if (cat.id === excludeId) continue;

    const path = parentPath ? `${parentPath} / ${cat.name}` : cat.name;
    result.push({
      id: cat.id,
      name: cat.name,
      level,
      hasChildren: !!cat.children && cat.children.length > 0,
      path,
    });

    if (cat.children && cat.children.length > 0) {
      result.push(...flattenCategories(cat.children, excludeId, level + 1, path));
    }
  }

  return result;
}

function findCategoryById(
  categories: Category[],
  id: string
): Category | undefined {
  for (const cat of categories) {
    if (cat.id === id) return cat;
    if (cat.children) {
      const found = findCategoryById(cat.children, id);
      if (found) return found;
    }
  }
  return undefined;
}

export function TreeMultiSelect({
  categories,
  value,
  onChange,
  excludeId,
  placeholder = "Select categories...",
  disabled = false,
}: TreeMultiSelectProps) {
  const [open, setOpen] = useState(false);
  const [search, setSearch] = useState("");

  // Flatten the tree for rendering
  const flatCategories = useMemo(
    () => flattenCategories(categories, excludeId),
    [categories, excludeId]
  );

  // Filter based on search
  const filteredCategories = useMemo(() => {
    if (!search.trim()) return flatCategories;
    const lowerSearch = search.toLowerCase();
    return flatCategories.filter(
      (cat) =>
        cat.name.toLowerCase().includes(lowerSearch) ||
        cat.path.toLowerCase().includes(lowerSearch)
    );
  }, [flatCategories, search]);

  // Get selected categories info
  const selectedCategories = useMemo(() => {
    return value
      .map((id) => findCategoryById(categories, id))
      .filter((cat): cat is Category => cat !== undefined);
  }, [categories, value]);

  const handleToggle = (categoryId: string) => {
    if (value.includes(categoryId)) {
      // Remove category
      onChange(value.filter((id) => id !== categoryId));
    } else {
      // Add category
      onChange([...value, categoryId]);
    }
  };

  const handleRemove = (categoryId: string, e: React.MouseEvent) => {
    e.stopPropagation();
    onChange(value.filter((id) => id !== categoryId));
  };

  const handleSetPrimary = (categoryId: string, e: React.MouseEvent) => {
    e.stopPropagation();
    // Move to first position (primary)
    const newValue = [categoryId, ...value.filter((id) => id !== categoryId)];
    onChange(newValue);
  };

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          role="combobox"
          aria-expanded={open}
          className={cn(
            "w-full min-h-10 h-auto justify-between font-normal",
            value.length === 0 && "text-muted-foreground"
          )}
          disabled={disabled}
        >
          <div className="flex flex-wrap gap-1 flex-1">
            {selectedCategories.length > 0 ? (
              selectedCategories.map((cat, index) => (
                <Badge
                  key={cat.id}
                  variant={index === 0 ? "default" : "secondary"}
                  className="mr-1 flex items-center gap-1"
                >
                  {index === 0 && (
                    <Star className="h-3 w-3 fill-current" />
                  )}
                  <span className="max-w-[150px] truncate">{cat.name}</span>
                  {index !== 0 && (
                    <button
                      type="button"
                      className="ml-1 rounded-full outline-none hover:bg-secondary"
                      onClick={(e) => handleSetPrimary(cat.id, e)}
                      title="Set as primary"
                    >
                      <Star className="h-3 w-3" />
                    </button>
                  )}
                  <button
                    type="button"
                    className="ml-1 rounded-full outline-none hover:bg-secondary"
                    onClick={(e) => handleRemove(cat.id, e)}
                    title="Remove"
                  >
                    <X className="h-3 w-3" />
                  </button>
                </Badge>
              ))
            ) : (
              <span>{placeholder}</span>
            )}
          </div>
          <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-[400px] p-0" align="start">
        <Command shouldFilter={false}>
          <CommandInput
            placeholder="Search categories..."
            value={search}
            onValueChange={setSearch}
          />
          <CommandList className="max-h-[300px]">
            <CommandEmpty>
              <div className="flex flex-col items-center py-6 text-center">
                <Search className="h-10 w-10 text-muted-foreground mb-2" />
                <p className="text-sm text-muted-foreground">
                  No category found.
                </p>
              </div>
            </CommandEmpty>
            <CommandGroup>
              {/* Category tree items */}
              {filteredCategories.map((cat) => {
                const isSelected = value.includes(cat.id);
                const isPrimary = value[0] === cat.id;
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
                    {isPrimary && (
                      <Badge variant="default" className="ml-2 text-xs">
                        <Star className="h-3 w-3 mr-1 fill-current" />
                        Primary
                      </Badge>
                    )}
                  </CommandItem>
                );
              })}
            </CommandGroup>
          </CommandList>
        </Command>
        {value.length > 0 && (
          <div className="p-2 border-t text-xs text-muted-foreground">
            First selected category is primary. Click star to change.
          </div>
        )}
      </PopoverContent>
    </Popover>
  );
}
