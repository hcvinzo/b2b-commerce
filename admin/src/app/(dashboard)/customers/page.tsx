"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { Search, Eye, MoreHorizontal, CheckCircle, XCircle, RefreshCw } from "lucide-react";

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
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
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
import { formatDate } from "@/lib/utils";
import { Customer, CustomerFilters, CustomerStatus } from "@/types/entities";

// Helper function to get primary active contact
function getPrimaryContact(customer: Customer) {
  if (!customer.contacts || customer.contacts.length === 0) return null;
  // Find primary and active contact first, otherwise find any active contact, otherwise any contact
  return (
    customer.contacts.find((c) => c.isPrimary && c.isActive) ||
    customer.contacts.find((c) => c.isActive) ||
    customer.contacts[0]
  );
}

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
    status: CustomerStatus;
  } | null>(null);

  const { data, isLoading, isFetching, refetch } = useCustomers(filters);
  const approveCustomer = useApproveCustomer();
  const activateCustomer = useActivateCustomer();
  const deactivateCustomer = useDeactivateCustomer();

  const handleSearch = () => {
    setFilters((prev) => ({ ...prev, search: searchInput, page: 1 }));
  };

  const handleStatusChange = (value: string) => {
    setFilters((prev) => ({
      ...prev,
      status: value === "all" ? undefined : (value as CustomerStatus),
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
      if (toggleId.status === "Active") {
        await deactivateCustomer.mutateAsync(toggleId.id);
      } else {
        await activateCustomer.mutateAsync(toggleId.id);
      }
      setToggleId(null);
    }
  };

  const getStatusBadgeVariant = (status: CustomerStatus) => {
    switch (status) {
      case "Active":
        return "default";
      case "Pending":
        return "outline";
      case "Rejected":
        return "destructive";
      case "Suspended":
        return "secondary";
      default:
        return "secondary";
    }
  };

  const getStatusBadgeClassName = (status: CustomerStatus) => {
    switch (status) {
      case "Pending":
        return "border-yellow-500 text-yellow-600";
      default:
        return undefined;
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Customers</h1>
          <p className="text-muted-foreground">
            Manage your dealer accounts
          </p>
        </div>
      </div>

      <Card>
        <CardHeader className="pb-4">
          <CardTitle>All Customers</CardTitle>
          <CardDescription>
            View and manage dealer accounts
          </CardDescription>
        </CardHeader>
        <CardContent>
          {/* Filters */}
          <div className="flex flex-col gap-4 md:flex-row md:items-center mb-6">
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
                value={filters.status ?? "all"}
                onValueChange={handleStatusChange}
              >
                <SelectTrigger className="w-[140px]">
                  <SelectValue placeholder="All Status" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">All Status</SelectItem>
                  <SelectItem value="Pending">Pending</SelectItem>
                  <SelectItem value="Active">Active</SelectItem>
                  <SelectItem value="Suspended">Suspended</SelectItem>
                  <SelectItem value="Rejected">Rejected</SelectItem>
                </SelectContent>
              </Select>
              <Button
                variant="outline"
                size="icon"
                onClick={() => refetch()}
                disabled={isFetching}
              >
                <RefreshCw className={`h-4 w-4 ${isFetching ? "animate-spin" : ""}`} />
              </Button>
            </div>
          </div>

          {/* Table */}
          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Title</TableHead>
                  <TableHead>Contact Name</TableHead>
                  <TableHead>Email</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Applied</TableHead>
                  <TableHead className="w-[70px]"></TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {isLoading ? (
                  [...Array(5)].map((_, i) => (
                    <TableRow key={i}>
                      <TableCell colSpan={6}>
                        <Skeleton className="h-10 w-full" />
                      </TableCell>
                    </TableRow>
                  ))
                ) : data?.items.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={6} className="text-center py-10">
                      <p className="text-muted-foreground">No customers found</p>
                    </TableCell>
                  </TableRow>
                ) : (
                  data?.items.map((customer) => {
                    const primaryContact = getPrimaryContact(customer);
                    return (
                      <TableRow
                        key={customer.id}
                        className="cursor-pointer"
                        onClick={() => router.push(`/customers/${customer.id}`)}
                      >
                        <TableCell>
                          <span className="font-medium">
                            {customer.title}
                          </span>
                        </TableCell>
                        <TableCell>
                          {primaryContact?.fullName || "-"}
                        </TableCell>
                        <TableCell>
                          {primaryContact?.email || "-"}
                        </TableCell>
                        <TableCell>
                          <Badge
                            variant={getStatusBadgeVariant(customer.status)}
                            className={getStatusBadgeClassName(customer.status)}
                          >
                            {customer.status}
                          </Badge>
                        </TableCell>
                        <TableCell className="text-muted-foreground">
                          {formatDate(customer.createdAt)}
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
                              {customer.status === "Pending" && (
                                <DropdownMenuItem
                                  onClick={() => setApproveId(customer.id)}
                                >
                                  <CheckCircle className="mr-2 h-4 w-4" />
                                  Approve
                                </DropdownMenuItem>
                              )}
                              {customer.status === "Active" && (
                                <DropdownMenuItem
                                  onClick={() =>
                                    setToggleId({
                                      id: customer.id,
                                      status: customer.status,
                                    })
                                  }
                                >
                                  <XCircle className="mr-2 h-4 w-4" />
                                  Suspend
                                </DropdownMenuItem>
                              )}
                              {customer.status === "Suspended" && (
                                <DropdownMenuItem
                                  onClick={() =>
                                    setToggleId({
                                      id: customer.id,
                                      status: customer.status,
                                    })
                                  }
                                >
                                  <CheckCircle className="mr-2 h-4 w-4" />
                                  Activate
                                </DropdownMenuItem>
                              )}
                            </DropdownMenuContent>
                          </DropdownMenu>
                        </TableCell>
                      </TableRow>
                    );
                  })
                )}
              </TableBody>
            </Table>
          </div>

          {/* Pagination */}
          {data && data.totalPages > 1 && (
            <div className="flex items-center justify-between mt-4">
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
        </CardContent>
      </Card>

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
        title={toggleId?.status === "Active" ? "Suspend Customer" : "Activate Customer"}
        description={
          toggleId?.status === "Active"
            ? "Are you sure you want to suspend this customer? They will not be able to place orders."
            : "Are you sure you want to activate this customer?"
        }
        confirmText={toggleId?.status === "Active" ? "Suspend" : "Activate"}
        variant={toggleId?.status === "Active" ? "destructive" : "default"}
        onConfirm={handleToggle}
        isLoading={activateCustomer.isPending || deactivateCustomer.isPending}
      />
    </div>
  );
}
