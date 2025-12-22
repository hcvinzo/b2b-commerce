"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Search, Eye, MoreHorizontal, CheckCircle, XCircle } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import {
  useCustomers,
  useApproveCustomer,
  useActivateCustomer,
  useDeactivateCustomer,
} from "@/hooks/use-customers";
import { formatCurrency } from "@/lib/utils";
import { CustomerFilters } from "@/types/entities";

export default function CustomersPage() {
  const router = useRouter();
  const [filters, setFilters] = useState<CustomerFilters>({
    page: 1,
    pageSize: 10,
    sortBy: "CreatedAt",
    sortOrder: "desc",
  });
  const [searchInput, setSearchInput] = useState("");
  const [approveId, setApproveId] = useState<string | null>(null);
  const [toggleId, setToggleId] = useState<{
    id: string;
    isActive: boolean;
  } | null>(null);

  const { data, isLoading } = useCustomers(filters);
  const approveCustomer = useApproveCustomer();
  const activateCustomer = useActivateCustomer();
  const deactivateCustomer = useDeactivateCustomer();

  const handleSearch = () => {
    setFilters((prev) => ({ ...prev, search: searchInput, page: 1 }));
  };

  const handleStatusChange = (value: string) => {
    setFilters((prev) => ({
      ...prev,
      isActive: value === "all" ? undefined : value === "active",
      page: 1,
    }));
  };

  const handleApprovalChange = (value: string) => {
    setFilters((prev) => ({
      ...prev,
      isApproved: value === "all" ? undefined : value === "approved",
      page: 1,
    }));
  };

  const handleApprove = async () => {
    if (approveId) {
      await approveCustomer.mutateAsync(approveId);
      setApproveId(null);
    }
  };

  const handleToggle = async () => {
    if (toggleId) {
      if (toggleId.isActive) {
        await deactivateCustomer.mutateAsync(toggleId.id);
      } else {
        await activateCustomer.mutateAsync(toggleId.id);
      }
      setToggleId(null);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Customers</h1>
          <p className="text-muted-foreground">
            Manage your dealer accounts and credit limits
          </p>
        </div>
      </div>

      {/* Filters */}
      <div className="flex flex-col gap-4 md:flex-row md:items-center">
        <div className="flex flex-1 gap-2">
          <div className="relative flex-1 max-w-sm">
            <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
            <Input
              type="search"
              placeholder="Search customers..."
              className="pl-8"
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
              onKeyDown={(e) => e.key === "Enter" && handleSearch()}
            />
          </div>
          <Button variant="secondary" onClick={handleSearch}>
            Search
          </Button>
        </div>
        <div className="flex gap-2">
          <Select
            value={
              filters.isActive === undefined
                ? "all"
                : filters.isActive
                ? "active"
                : "inactive"
            }
            onValueChange={handleStatusChange}
          >
            <SelectTrigger className="w-[140px]">
              <SelectValue placeholder="All Status" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Status</SelectItem>
              <SelectItem value="active">Active</SelectItem>
              <SelectItem value="inactive">Inactive</SelectItem>
            </SelectContent>
          </Select>
          <Select
            value={
              filters.isApproved === undefined
                ? "all"
                : filters.isApproved
                ? "approved"
                : "pending"
            }
            onValueChange={handleApprovalChange}
          >
            <SelectTrigger className="w-[140px]">
              <SelectValue placeholder="All Approval" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All</SelectItem>
              <SelectItem value="approved">Approved</SelectItem>
              <SelectItem value="pending">Pending</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      {/* Table */}
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Company</TableHead>
              <TableHead>Email</TableHead>
              <TableHead>Credit Limit</TableHead>
              <TableHead>Available Credit</TableHead>
              <TableHead>Status</TableHead>
              <TableHead>Approval</TableHead>
              <TableHead className="w-[70px]"></TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {isLoading ? (
              [...Array(5)].map((_, i) => (
                <TableRow key={i}>
                  <TableCell colSpan={7}>
                    <Skeleton className="h-10 w-full" />
                  </TableCell>
                </TableRow>
              ))
            ) : data?.items.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7} className="text-center py-10">
                  <p className="text-muted-foreground">No customers found</p>
                </TableCell>
              </TableRow>
            ) : (
              data?.items.map((customer) => (
                <TableRow
                  key={customer.id}
                  className="cursor-pointer"
                  onClick={() => router.push(`/customers/${customer.id}`)}
                >
                  <TableCell>
                    <div>
                      <span className="font-medium">
                        {customer.companyName}
                      </span>
                      <p className="text-sm text-muted-foreground">
                        {customer.type}
                      </p>
                    </div>
                  </TableCell>
                  <TableCell>{customer.email}</TableCell>
                  <TableCell>{formatCurrency(customer.creditLimit, customer.currency)}</TableCell>
                  <TableCell>{formatCurrency(customer.availableCredit, customer.currency)}</TableCell>
                  <TableCell>
                    <Badge variant={customer.isActive ? "default" : "secondary"}>
                      {customer.isActive ? "Active" : "Inactive"}
                    </Badge>
                  </TableCell>
                  <TableCell>
                    <Badge
                      variant={customer.isApproved ? "default" : "outline"}
                      className={
                        customer.isApproved
                          ? "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300"
                          : "border-yellow-500 text-yellow-600"
                      }
                    >
                      {customer.isApproved ? "Approved" : "Pending"}
                    </Badge>
                  </TableCell>
                  <TableCell onClick={(e) => e.stopPropagation()}>
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button variant="ghost" size="icon">
                          <MoreHorizontal className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end">
                        <DropdownMenuItem
                          onClick={() => router.push(`/customers/${customer.id}`)}
                        >
                          <Eye className="mr-2 h-4 w-4" />
                          View Details
                        </DropdownMenuItem>
                        <DropdownMenuSeparator />
                        {!customer.isApproved && (
                          <DropdownMenuItem
                            onClick={() => setApproveId(customer.id)}
                          >
                            <CheckCircle className="mr-2 h-4 w-4" />
                            Approve
                          </DropdownMenuItem>
                        )}
                        <DropdownMenuItem
                          onClick={() =>
                            setToggleId({
                              id: customer.id,
                              isActive: customer.isActive,
                            })
                          }
                        >
                          {customer.isActive ? (
                            <>
                              <XCircle className="mr-2 h-4 w-4" />
                              Deactivate
                            </>
                          ) : (
                            <>
                              <CheckCircle className="mr-2 h-4 w-4" />
                              Activate
                            </>
                          )}
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>

      {/* Pagination */}
      {data && data.totalPages > 1 && (
        <div className="flex items-center justify-between">
          <p className="text-sm text-muted-foreground">
            Showing {(data.pageNumber - 1) * data.pageSize + 1} to{" "}
            {Math.min(data.pageNumber * data.pageSize, data.totalCount)} of{" "}
            {data.totalCount} customers
          </p>
          <div className="flex gap-2">
            <Button
              variant="outline"
              size="sm"
              disabled={!data.hasPreviousPage}
              onClick={() =>
                setFilters((prev) => ({ ...prev, page: (prev.page || 1) - 1 }))
              }
            >
              Previous
            </Button>
            <Button
              variant="outline"
              size="sm"
              disabled={!data.hasNextPage}
              onClick={() =>
                setFilters((prev) => ({ ...prev, page: (prev.page || 1) + 1 }))
              }
            >
              Next
            </Button>
          </div>
        </div>
      )}

      {/* Approve Confirmation */}
      <ConfirmDialog
        open={!!approveId}
        onOpenChange={(open) => !open && setApproveId(null)}
        title="Approve Customer"
        description="Are you sure you want to approve this customer? They will be able to place orders."
        confirmText="Approve"
        onConfirm={handleApprove}
        isLoading={approveCustomer.isPending}
      />

      {/* Toggle Status Confirmation */}
      <ConfirmDialog
        open={!!toggleId}
        onOpenChange={(open) => !open && setToggleId(null)}
        title={toggleId?.isActive ? "Deactivate Customer" : "Activate Customer"}
        description={
          toggleId?.isActive
            ? "Are you sure you want to deactivate this customer? They will not be able to place orders."
            : "Are you sure you want to activate this customer?"
        }
        confirmText={toggleId?.isActive ? "Deactivate" : "Activate"}
        variant={toggleId?.isActive ? "destructive" : "default"}
        onConfirm={handleToggle}
        isLoading={activateCustomer.isPending || deactivateCustomer.isPending}
      />
    </div>
  );
}
