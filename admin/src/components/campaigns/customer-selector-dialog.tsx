"use client";

import { useState, useMemo } from "react";
import { Check, Search, User } from "lucide-react";

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
import { Badge } from "@/components/ui/badge";
import { ScrollArea } from "@/components/ui/scroll-area";
import { useCustomers } from "@/hooks/use-customers";

interface CustomerSelectorDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  selectedIds: string[];
  onSave: (customerIds: string[]) => void;
  isLoading?: boolean;
}

export function CustomerSelectorDialog({
  open,
  onOpenChange,
  selectedIds,
  onSave,
  isLoading = false,
}: CustomerSelectorDialogProps) {
  const [selection, setSelection] = useState<string[]>(selectedIds);
  const [search, setSearch] = useState("");

  const { data: customersData, isLoading: isLoadingCustomers } = useCustomers({
    page: 1,
    pageSize: 100,
    search: search || undefined,
    status: "Active",
  });

  const customers = customersData?.items ?? [];

  // Reset selection when dialog opens
  const handleOpenChange = (newOpen: boolean) => {
    if (newOpen) {
      setSelection(selectedIds);
      setSearch("");
    }
    onOpenChange(newOpen);
  };

  const handleToggle = (customerId: string) => {
    if (selection.includes(customerId)) {
      setSelection(selection.filter((id) => id !== customerId));
    } else {
      setSelection([...selection, customerId]);
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
          <DialogTitle>Select Customers</DialogTitle>
          <DialogDescription>
            Choose customers to target with this discount rule.
          </DialogDescription>
        </DialogHeader>

        <div className="relative">
          <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <Input
            placeholder="Search customers..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="pl-9"
          />
        </div>

        <ScrollArea className="h-[300px] border rounded-md">
          {isLoadingCustomers ? (
            <div className="py-6 text-center text-sm text-muted-foreground">
              Loading customers...
            </div>
          ) : customers.length === 0 ? (
            <div className="flex flex-col items-center py-6 text-center">
              <User className="h-10 w-10 text-muted-foreground mb-2" />
              <p className="text-sm text-muted-foreground">
                No customers found.
              </p>
            </div>
          ) : (
            <div className="p-2 space-y-1">
              {customers.map((customer) => {
                const isSelected = selection.includes(customer.id);
                return (
                  <div
                    key={customer.id}
                    className={cn(
                      "flex items-center gap-3 p-2 rounded-md cursor-pointer hover:bg-muted transition-colors",
                      isSelected && "bg-muted"
                    )}
                    onClick={() => handleToggle(customer.id)}
                  >
                    <Checkbox
                      checked={isSelected}
                      onCheckedChange={() => handleToggle(customer.id)}
                    />
                    <div className="flex-1 min-w-0">
                      <div className="font-medium truncate">
                        {customer.title}
                      </div>
                      {customer.taxNo && (
                        <div className="text-sm text-muted-foreground truncate">
                          {customer.taxNo}
                        </div>
                      )}
                    </div>
                    <Badge variant="outline" className="text-xs shrink-0">
                      {customer.status}
                    </Badge>
                  </div>
                );
              })}
            </div>
          )}
        </ScrollArea>

        <div className="text-sm text-muted-foreground">
          {selection.length} {selection.length === 1 ? "customer" : "customers"} selected
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
