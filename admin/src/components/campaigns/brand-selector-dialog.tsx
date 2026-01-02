"use client";

import { useState, useMemo } from "react";
import { Search, Building2 } from "lucide-react";

import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Checkbox } from "@/components/ui/checkbox";
import { ScrollArea } from "@/components/ui/scroll-area";
import { useBrands } from "@/hooks/use-brands";

interface BrandSelectorDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  selectedIds: string[];
  onSave: (brandIds: string[]) => void;
  isLoading?: boolean;
}

export function BrandSelectorDialog({
  open,
  onOpenChange,
  selectedIds,
  onSave,
  isLoading = false,
}: BrandSelectorDialogProps) {
  const [selection, setSelection] = useState<string[]>(selectedIds);
  const [search, setSearch] = useState("");

  const { data: brandsData, isLoading: isLoadingBrands } = useBrands({
    search: search || undefined,
    isActive: true,
    pageSize: 100,
  });

  const brands = brandsData?.items ?? [];

  // Reset selection when dialog opens
  const handleOpenChange = (newOpen: boolean) => {
    if (newOpen) {
      setSelection(selectedIds);
      setSearch("");
    }
    onOpenChange(newOpen);
  };

  const handleToggle = (brandId: string) => {
    if (selection.includes(brandId)) {
      setSelection(selection.filter((id) => id !== brandId));
    } else {
      setSelection([...selection, brandId]);
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
          <DialogTitle>Select Brands</DialogTitle>
          <DialogDescription>
            Choose brands to target with this discount rule.
          </DialogDescription>
        </DialogHeader>

        <div className="relative">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search brands..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="pl-9"
          />
        </div>

        <ScrollArea className="h-[300px] border rounded-md">
          {isLoadingBrands ? (
            <div className="py-6 text-center text-sm text-muted-foreground">
              Loading brands...
            </div>
          ) : brands.length === 0 ? (
            <div className="flex flex-col items-center py-6 text-center">
              <Building2 className="h-10 w-10 text-muted-foreground mb-2" />
              <p className="text-sm text-muted-foreground">
                No brands found.
              </p>
            </div>
          ) : (
            <div className="p-2 space-y-1">
              {brands.map((brand) => {
                const isSelected = selection.includes(brand.id);
                return (
                  <div
                    key={brand.id}
                    className={cn(
                      "flex items-center gap-3 p-2 rounded-md cursor-pointer hover:bg-muted transition-colors",
                      isSelected && "bg-muted"
                    )}
                    onClick={() => handleToggle(brand.id)}
                  >
                    <Checkbox
                      checked={isSelected}
                      onCheckedChange={() => handleToggle(brand.id)}
                    />
                    <div className="flex items-center gap-3 flex-1 min-w-0">
                      {brand.logoUrl ? (
                        <img
                          src={brand.logoUrl}
                          alt={brand.name}
                          className="h-8 w-8 object-contain rounded"
                        />
                      ) : (
                        <div className="h-8 w-8 rounded bg-muted flex items-center justify-center">
                          <Building2 className="h-4 w-4 text-muted-foreground" />
                        </div>
                      )}
                      <div className="font-medium truncate">{brand.name}</div>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </ScrollArea>

        <div className="text-sm text-muted-foreground">
          {selection.length} {selection.length === 1 ? "brand" : "brands"} selected
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
