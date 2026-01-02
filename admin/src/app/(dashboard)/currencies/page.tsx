"use client";

import { useState } from "react";
import {
  Plus,
  MoreHorizontal,
  Pencil,
  Trash2,
  Coins,
  Power,
  PowerOff,
  Star,
  RefreshCw,
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
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
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
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { CurrencyForm } from "@/components/forms/currency-form";
import {
  useCurrencies,
  useCreateCurrency,
  useUpdateCurrency,
  useDeleteCurrency,
  useActivateCurrency,
  useDeactivateCurrency,
  useSetDefaultCurrency,
} from "@/hooks/use-currencies";
import { type CurrencyFormData } from "@/lib/validations/currency";
import { type CurrencyListItem, type CurrencyFilters } from "@/types/entities";

export default function CurrenciesPage() {
  const [filters, setFilters] = useState<CurrencyFilters>({});
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingCurrency, setEditingCurrency] = useState<CurrencyListItem | null>(null);
  const [deletingCurrency, setDeletingCurrency] = useState<CurrencyListItem | null>(null);
  const [settingDefault, setSettingDefault] = useState<CurrencyListItem | null>(null);

  const { data: currencies, isLoading, isFetching, refetch } = useCurrencies(filters);
  const createCurrency = useCreateCurrency();
  const updateCurrency = useUpdateCurrency();
  const deleteCurrency = useDeleteCurrency();
  const activateCurrency = useActivateCurrency();
  const deactivateCurrency = useDeactivateCurrency();
  const setDefaultCurrency = useSetDefaultCurrency();

  const handleAddNew = () => {
    setEditingCurrency(null);
    setIsFormOpen(true);
  };

  const handleEdit = (currency: CurrencyListItem) => {
    setEditingCurrency(currency);
    setIsFormOpen(true);
  };

  const handleFormSubmit = async (formData: CurrencyFormData) => {
    if (editingCurrency) {
      await updateCurrency.mutateAsync({
        id: editingCurrency.id,
        data: {
          name: formData.name,
          symbol: formData.symbol,
          decimalPlaces: formData.decimalPlaces,
          displayOrder: formData.displayOrder,
        },
      });
    } else {
      await createCurrency.mutateAsync({
        code: formData.code,
        name: formData.name,
        symbol: formData.symbol,
        decimalPlaces: formData.decimalPlaces,
        displayOrder: formData.displayOrder,
      });
    }
    setIsFormOpen(false);
    setEditingCurrency(null);
  };

  const handleDelete = async () => {
    if (deletingCurrency) {
      await deleteCurrency.mutateAsync(deletingCurrency.id);
      setDeletingCurrency(null);
    }
  };

  const handleToggleActive = async (currency: CurrencyListItem) => {
    if (currency.isActive) {
      await deactivateCurrency.mutateAsync(currency.id);
    } else {
      await activateCurrency.mutateAsync(currency.id);
    }
  };

  const handleSetDefault = async () => {
    if (settingDefault) {
      await setDefaultCurrency.mutateAsync(settingDefault.id);
      setSettingDefault(null);
    }
  };

  const handleStatusFilterChange = (value: string) => {
    setFilters({
      activeOnly: value === "all" ? undefined : value === "active",
    });
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Currencies</h1>
          <p className="text-muted-foreground">
            Manage available currencies and set the default
          </p>
        </div>
        <Button onClick={handleAddNew}>
          <Plus className="mr-2 h-4 w-4" />
          Add Currency
        </Button>
      </div>

      <Card>
        <CardHeader className="pb-4">
          <CardTitle>All Currencies</CardTitle>
          <CardDescription>
            Configure currencies used throughout the system
          </CardDescription>
        </CardHeader>
        <CardContent>
          {/* Filters */}
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center mb-6">
            <Select
              value={
                filters.activeOnly === undefined
                  ? "all"
                  : filters.activeOnly
                    ? "active"
                    : "inactive"
              }
              onValueChange={handleStatusFilterChange}
            >
              <SelectTrigger className="w-[150px]">
                <SelectValue placeholder="Status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Status</SelectItem>
                <SelectItem value="active">Active</SelectItem>
                <SelectItem value="inactive">Inactive</SelectItem>
              </SelectContent>
            </Select>
            <div className="flex-1" />
            <Button
              variant="outline"
              size="icon"
              onClick={() => refetch()}
              disabled={isFetching}
            >
              <RefreshCw className={`h-4 w-4 ${isFetching ? "animate-spin" : ""}`} />
            </Button>
          </div>

          {/* Table */}
          {isLoading ? (
            <div className="space-y-3">
              {[...Array(5)].map((_, i) => (
                <Skeleton key={i} className="h-16 w-full" />
              ))}
            </div>
          ) : currencies && currencies.length > 0 ? (
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Code</TableHead>
                    <TableHead>Name</TableHead>
                    <TableHead className="text-center">Symbol</TableHead>
                    <TableHead className="text-center">Decimals</TableHead>
                    <TableHead className="text-center">Order</TableHead>
                    <TableHead className="text-center">Status</TableHead>
                    <TableHead className="w-[70px]"></TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {currencies.map((currency) => (
                    <TableRow key={currency.id}>
                      <TableCell>
                        <div className="flex items-center gap-2">
                          <span className="font-mono font-medium">{currency.code}</span>
                          {currency.isDefault && (
                            <Badge variant="default" className="gap-1">
                              <Star className="h-3 w-3" />
                              Default
                            </Badge>
                          )}
                        </div>
                      </TableCell>
                      <TableCell>{currency.name}</TableCell>
                      <TableCell className="text-center font-mono">
                        {currency.symbol}
                      </TableCell>
                      <TableCell className="text-center">
                        {currency.decimalPlaces}
                      </TableCell>
                      <TableCell className="text-center">
                        {currency.displayOrder}
                      </TableCell>
                      <TableCell className="text-center">
                        <Badge
                          variant={currency.isActive ? "default" : "secondary"}
                        >
                          {currency.isActive ? "Active" : "Inactive"}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" size="icon">
                              <MoreHorizontal className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuItem onClick={() => handleEdit(currency)}>
                              <Pencil className="mr-2 h-4 w-4" />
                              Edit
                            </DropdownMenuItem>
                            {!currency.isDefault && currency.isActive && (
                              <DropdownMenuItem
                                onClick={() => setSettingDefault(currency)}
                              >
                                <Star className="mr-2 h-4 w-4" />
                                Set as Default
                              </DropdownMenuItem>
                            )}
                            <DropdownMenuItem
                              onClick={() => handleToggleActive(currency)}
                            >
                              {currency.isActive ? (
                                <>
                                  <PowerOff className="mr-2 h-4 w-4" />
                                  Deactivate
                                </>
                              ) : (
                                <>
                                  <Power className="mr-2 h-4 w-4" />
                                  Activate
                                </>
                              )}
                            </DropdownMenuItem>
                            {!currency.isDefault && (
                              <>
                                <DropdownMenuSeparator />
                                <DropdownMenuItem
                                  className="text-destructive"
                                  onClick={() => setDeletingCurrency(currency)}
                                >
                                  <Trash2 className="mr-2 h-4 w-4" />
                                  Delete
                                </DropdownMenuItem>
                              </>
                            )}
                          </DropdownMenuContent>
                        </DropdownMenu>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          ) : (
            <div className="text-center py-10">
              <Coins className="mx-auto h-12 w-12 text-muted-foreground" />
              <h3 className="mt-4 text-lg font-semibold">No currencies</h3>
              <p className="text-muted-foreground">
                Get started by adding your first currency.
              </p>
              <Button className="mt-4" onClick={handleAddNew}>
                <Plus className="mr-2 h-4 w-4" />
                Add Currency
              </Button>
            </div>
          )}

          {/* Total count */}
          {currencies && currencies.length > 0 && (
            <div className="mt-4 text-sm text-muted-foreground">
              Total: {currencies.length} currencies
            </div>
          )}
        </CardContent>
      </Card>

      {/* Currency Form Dialog */}
      <Dialog open={isFormOpen} onOpenChange={setIsFormOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>
              {editingCurrency ? "Edit Currency" : "Add Currency"}
            </DialogTitle>
            <DialogDescription>
              {editingCurrency
                ? "Update the currency information below."
                : "Fill in the details to create a new currency."}
            </DialogDescription>
          </DialogHeader>
          <CurrencyForm
            mode={editingCurrency ? "update" : "create"}
            defaultValues={
              editingCurrency
                ? {
                    code: editingCurrency.code,
                    name: editingCurrency.name,
                    symbol: editingCurrency.symbol,
                    decimalPlaces: editingCurrency.decimalPlaces,
                    displayOrder: editingCurrency.displayOrder,
                  }
                : undefined
            }
            onSubmit={handleFormSubmit}
            onCancel={() => setIsFormOpen(false)}
            isLoading={createCurrency.isPending || updateCurrency.isPending}
          />
        </DialogContent>
      </Dialog>

      {/* Set Default Confirmation */}
      <ConfirmDialog
        open={!!settingDefault}
        onOpenChange={(open) => !open && setSettingDefault(null)}
        title="Set Default Currency"
        description={`Are you sure you want to set "${settingDefault?.name}" (${settingDefault?.code}) as the default currency?`}
        confirmText="Set Default"
        variant="default"
        onConfirm={handleSetDefault}
        isLoading={setDefaultCurrency.isPending}
      />

      {/* Delete Confirmation */}
      <ConfirmDialog
        open={!!deletingCurrency}
        onOpenChange={(open) => !open && setDeletingCurrency(null)}
        title="Delete Currency"
        description={`Are you sure you want to delete "${deletingCurrency?.name}" (${deletingCurrency?.code})? This action cannot be undone.`}
        confirmText="Delete"
        variant="destructive"
        onConfirm={handleDelete}
        isLoading={deleteCurrency.isPending}
      />
    </div>
  );
}
