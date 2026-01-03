"use client";

import { useState } from "react";
import {
  Plus,
  MoreHorizontal,
  Pencil,
  Trash2,
  TrendingUp,
  Power,
  PowerOff,
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
import { CurrencyRateForm } from "@/components/forms/currency-rate-form";
import {
  useCurrencyRates,
  useCreateCurrencyRate,
  useUpdateCurrencyRate,
  useDeleteCurrencyRate,
  useActivateCurrencyRate,
  useDeactivateCurrencyRate,
} from "@/hooks/use-currency-rates";
import { useCurrencies } from "@/hooks/use-currencies";
import type {
  CurrencyRateFormData,
  UpdateCurrencyRateFormData,
} from "@/lib/validations/currency-rate";
import { type CurrencyRateListItem, type CurrencyRateFilters } from "@/types/entities";

export default function CurrencyRatesPage() {
  const [filters, setFilters] = useState<CurrencyRateFilters>({});
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingRate, setEditingRate] = useState<CurrencyRateListItem | null>(null);
  const [deletingRate, setDeletingRate] = useState<CurrencyRateListItem | null>(null);

  const { data: rates, isLoading, isFetching, refetch } = useCurrencyRates(filters);
  const { data: currencies } = useCurrencies({ activeOnly: true });
  const createRate = useCreateCurrencyRate();
  const updateRate = useUpdateCurrencyRate();
  const deleteRate = useDeleteCurrencyRate();
  const activateRate = useActivateCurrencyRate();
  const deactivateRate = useDeactivateCurrencyRate();

  const handleAddNew = () => {
    setEditingRate(null);
    setIsFormOpen(true);
  };

  const handleEdit = (rate: CurrencyRateListItem) => {
    setEditingRate(rate);
    setIsFormOpen(true);
  };

  const handleFormSubmit = async (formData: CurrencyRateFormData | UpdateCurrencyRateFormData) => {
    if (editingRate) {
      await updateRate.mutateAsync({
        id: editingRate.id,
        data: {
          rate: formData.rate,
          effectiveDate: formData.effectiveDate,
        },
      });
    } else {
      const createData = formData as CurrencyRateFormData;
      await createRate.mutateAsync({
        fromCurrency: createData.fromCurrency,
        toCurrency: createData.toCurrency,
        rate: createData.rate,
        effectiveDate: createData.effectiveDate,
      });
    }
    setIsFormOpen(false);
    setEditingRate(null);
  };

  const handleDelete = async () => {
    if (deletingRate) {
      await deleteRate.mutateAsync(deletingRate.id);
      setDeletingRate(null);
    }
  };

  const handleToggleActive = async (rate: CurrencyRateListItem) => {
    if (rate.isActive) {
      await deactivateRate.mutateAsync(rate.id);
    } else {
      await activateRate.mutateAsync(rate.id);
    }
  };

  const handleStatusFilterChange = (value: string) => {
    setFilters((prev) => ({
      ...prev,
      activeOnly: value === "all" ? undefined : value === "active",
    }));
  };

  const handleFromCurrencyFilterChange = (value: string) => {
    setFilters((prev) => ({
      ...prev,
      fromCurrency: value === "all" ? undefined : value,
    }));
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleDateString("en-US", {
      year: "numeric",
      month: "short",
      day: "numeric",
    });
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Currency Rates</h1>
          <p className="text-muted-foreground">
            Manage exchange rates between currencies
          </p>
        </div>
        <Button onClick={handleAddNew}>
          <Plus className="mr-2 h-4 w-4" />
          Add Rate
        </Button>
      </div>

      <Card>
        <CardHeader className="pb-4">
          <CardTitle>All Exchange Rates</CardTitle>
          <CardDescription>
            Configure exchange rates for currency conversions
          </CardDescription>
        </CardHeader>
        <CardContent>
          {/* Filters */}
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center mb-6">
            <Select
              value={filters.fromCurrency || "all"}
              onValueChange={handleFromCurrencyFilterChange}
            >
              <SelectTrigger className="w-[180px]">
                <SelectValue placeholder="From Currency" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Currencies</SelectItem>
                {currencies?.map((currency) => (
                  <SelectItem key={currency.id} value={currency.code}>
                    {currency.code} - {currency.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
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
          ) : rates && rates.length > 0 ? (
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>From</TableHead>
                    <TableHead>To</TableHead>
                    <TableHead className="text-right">Rate</TableHead>
                    <TableHead className="text-center">Effective Date</TableHead>
                    <TableHead className="text-center">Status</TableHead>
                    <TableHead className="w-[70px]"></TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {rates.map((rate) => (
                    <TableRow key={rate.id}>
                      <TableCell>
                        <span className="font-mono font-medium">{rate.fromCurrency}</span>
                      </TableCell>
                      <TableCell>
                        <span className="font-mono font-medium">{rate.toCurrency}</span>
                      </TableCell>
                      <TableCell className="text-right font-mono">
                        {rate.rate.toFixed(6)}
                      </TableCell>
                      <TableCell className="text-center">
                        {formatDate(rate.effectiveDate)}
                      </TableCell>
                      <TableCell className="text-center">
                        <Badge
                          variant={rate.isActive ? "default" : "secondary"}
                        >
                          {rate.isActive ? "Active" : "Inactive"}
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
                            <DropdownMenuItem onClick={() => handleEdit(rate)}>
                              <Pencil className="mr-2 h-4 w-4" />
                              Edit
                            </DropdownMenuItem>
                            <DropdownMenuItem
                              onClick={() => handleToggleActive(rate)}
                            >
                              {rate.isActive ? (
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
                            <DropdownMenuSeparator />
                            <DropdownMenuItem
                              className="text-destructive"
                              onClick={() => setDeletingRate(rate)}
                            >
                              <Trash2 className="mr-2 h-4 w-4" />
                              Delete
                            </DropdownMenuItem>
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
              <TrendingUp className="mx-auto h-12 w-12 text-muted-foreground" />
              <h3 className="mt-4 text-lg font-semibold">No exchange rates</h3>
              <p className="text-muted-foreground">
                Get started by adding your first exchange rate.
              </p>
              <Button className="mt-4" onClick={handleAddNew}>
                <Plus className="mr-2 h-4 w-4" />
                Add Rate
              </Button>
            </div>
          )}

          {/* Total count */}
          {rates && rates.length > 0 && (
            <div className="mt-4 text-sm text-muted-foreground">
              Total: {rates.length} exchange rates
            </div>
          )}
        </CardContent>
      </Card>

      {/* Currency Rate Form Dialog */}
      <Dialog open={isFormOpen} onOpenChange={setIsFormOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>
              {editingRate ? "Edit Exchange Rate" : "Add Exchange Rate"}
            </DialogTitle>
            <DialogDescription>
              {editingRate
                ? `Update the exchange rate for ${editingRate.fromCurrency} to ${editingRate.toCurrency}.`
                : "Fill in the details to create a new exchange rate."}
            </DialogDescription>
          </DialogHeader>
          <CurrencyRateForm
            mode={editingRate ? "update" : "create"}
            defaultValues={
              editingRate
                ? {
                    fromCurrency: editingRate.fromCurrency,
                    toCurrency: editingRate.toCurrency,
                    rate: editingRate.rate,
                    effectiveDate: editingRate.effectiveDate.split("T")[0],
                  }
                : undefined
            }
            currencies={currencies || []}
            onSubmit={handleFormSubmit}
            onCancel={() => setIsFormOpen(false)}
            isLoading={createRate.isPending || updateRate.isPending}
          />
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation */}
      <ConfirmDialog
        open={!!deletingRate}
        onOpenChange={(open) => !open && setDeletingRate(null)}
        title="Delete Exchange Rate"
        description={`Are you sure you want to delete the exchange rate from ${deletingRate?.fromCurrency} to ${deletingRate?.toCurrency}? This action cannot be undone.`}
        confirmText="Delete"
        variant="destructive"
        onConfirm={handleDelete}
        isLoading={deleteRate.isPending}
      />
    </div>
  );
}
