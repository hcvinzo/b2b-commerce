"use client";

import { useState, useMemo } from "react";
import {
  ChevronsUpDown,
  Check,
  Folder,
  FolderTree,
  Search,
} from "lucide-react";

import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
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

interface TreeSelectProps {
  categories: Category[];
  value?: string;
  onChange: (value: string) => void;
  excludeId?: string;
  placeholder?: string;
  disabled?: boolean;
  /** Whether to show "No Parent (Root Category)" option. Default: true */
  allowEmpty?: boolean;
  /** Custom label for empty option. Default: "No Parent (Root Category)" */
  emptyLabel?: string;
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

function findCategoryPath(categories: Category[], id: string): string {
  const buildPath = (
    cats: Category[],
    targetId: string,
    currentPath: string[]
  ): string[] | null => {
    for (const cat of cats) {
      const newPath = [...currentPath, cat.name];
      if (cat.id === targetId) {
        return newPath;
      }
      if (cat.children) {
        const found = buildPath(cat.children, targetId, newPath);
        if (found) return found;
      }
    }
    return null;
  };

  const path = buildPath(categories, id, []);
  return path ? path.join(" / ") : "";
}

export function TreeSelect({
  categories,
  value,
  onChange,
  excludeId,
  placeholder = "Select category...",
  disabled = false,
  allowEmpty = true,
  emptyLabel = "No Parent (Root Category)",
}: TreeSelectProps) {
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

  // Get display text for selected value
  const selectedText = useMemo(() => {
    if (!value) return null;
    return findCategoryPath(categories, value);
  }, [categories, value]);

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button
          variant="outline"
          role="combobox"
          aria-expanded={open}
          className={cn(
            "w-full justify-between font-normal",
            !value && "text-muted-foreground"
          )}
          disabled={disabled}
        >
          <span className="truncate">
            {selectedText || placeholder}
          </span>
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
              {/* No parent / empty option */}
              {allowEmpty && (
                <CommandItem
                  value="__none__"
                  onSelect={() => {
                    onChange("");
                    setOpen(false);
                    setSearch("");
                  }}
                  className="flex items-center gap-2"
                >
                  <Check
                    className={cn(
                      "h-4 w-4 shrink-0",
                      !value ? "opacity-100" : "opacity-0"
                    )}
                  />
                  <span className="font-medium">{emptyLabel}</span>
                </CommandItem>
              )}

              {/* Category tree items */}
              {filteredCategories.map((cat) => (
                <CommandItem
                  key={cat.id}
                  value={cat.id}
                  onSelect={() => {
                    onChange(cat.id);
                    setOpen(false);
                    setSearch("");
                  }}
                  className="flex items-center gap-2"
                  style={{ paddingLeft: `${cat.level * 16 + 8}px` }}
                >
                  <Check
                    className={cn(
                      "h-4 w-4 shrink-0",
                      value === cat.id ? "opacity-100" : "opacity-0"
                    )}
                  />
                  {cat.hasChildren ? (
                    <Folder className="h-4 w-4 shrink-0 text-amber-500" />
                  ) : (
                    <FolderTree className="h-4 w-4 shrink-0 text-muted-foreground" />
                  )}
                  <span className="truncate">{cat.name}</span>
                </CommandItem>
              ))}
            </CommandGroup>
          </CommandList>
        </Command>
      </PopoverContent>
    </Popover>
  );
}
