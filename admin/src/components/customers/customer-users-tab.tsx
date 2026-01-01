"use client";

import { useState } from "react";
import {
  Plus,
  MoreHorizontal,
  UserCheck,
  UserX,
  Search,
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
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
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
import {
  useCustomerUsers,
  useActivateCustomerUser,
  useDeactivateCustomerUser,
} from "@/hooks/use-customer-users";
import { formatDateTime } from "@/lib/utils";
import { CustomerUserListItem, CustomerUserFilters } from "@/types/entities";
import { CustomerUserFormDialog } from "./customer-user-form-dialog";
import { CustomerUserRolesDialog } from "./customer-user-roles-dialog";

interface CustomerUsersTabProps {
  customerId: string;
}

export function CustomerUsersTab({ customerId }: CustomerUsersTabProps) {
  const [filters, setFilters] = useState<CustomerUserFilters>({
    page: 1,
    pageSize: 10,
  });
  const [searchInput, setSearchInput] = useState("");

  // Dialogs state
  const [isCreateDialogOpen, setIsCreateDialogOpen] = useState(false);
  const [selectedUserForEdit, setSelectedUserForEdit] =
    useState<CustomerUserListItem | null>(null);
  const [selectedUserForRoles, setSelectedUserForRoles] =
    useState<CustomerUserListItem | null>(null);

  const {
    data,
    isLoading,
    isFetching,
    refetch,
  } = useCustomerUsers(customerId, filters);
  const activateUser = useActivateCustomerUser();
  const deactivateUser = useDeactivateCustomerUser();

  const handleSearch = () => {
    setFilters((prev) => ({
      ...prev,
      search: searchInput || undefined,
      page: 1,
    }));
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter") {
      handleSearch();
    }
  };

  const handleActivate = (userId: string) => {
    activateUser.mutate({ customerId, userId });
  };

  const handleDeactivate = (userId: string) => {
    deactivateUser.mutate({ customerId, userId });
  };

  if (isLoading) {
    return (
      <Card>
        <CardContent className="py-8">
          <Skeleton className="h-[400px]" />
        </CardContent>
      </Card>
    );
  }

  return (
    <>
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>Users</CardTitle>
              <CardDescription>
                Manage users who can access this dealer account
              </CardDescription>
            </div>
            <Button onClick={() => setIsCreateDialogOpen(true)}>
              <Plus className="h-4 w-4 mr-2" />
              Add User
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          {/* Search and filters */}
          <div className="flex items-center gap-4 mb-6">
            <div className="relative flex-1 max-w-sm">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search by name or email..."
                value={searchInput}
                onChange={(e) => setSearchInput(e.target.value)}
                onKeyDown={handleKeyDown}
                className="pl-9"
              />
            </div>
            <Button variant="outline" size="icon" onClick={() => refetch()}>
              <RefreshCw
                className={`h-4 w-4 ${isFetching ? "animate-spin" : ""}`}
              />
            </Button>
          </div>

          {/* Users table */}
          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>User</TableHead>
                  <TableHead>Roles</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Last Login</TableHead>
                  <TableHead className="w-[50px]"></TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {data?.items.length === 0 ? (
                  <TableRow>
                    <TableCell colSpan={5} className="text-center py-8">
                      <p className="text-muted-foreground">No users found</p>
                      <Button
                        variant="link"
                        onClick={() => setIsCreateDialogOpen(true)}
                        className="mt-2"
                      >
                        Add the first user
                      </Button>
                    </TableCell>
                  </TableRow>
                ) : (
                  data?.items.map((user) => (
                    <TableRow key={user.id}>
                      <TableCell>
                        <div>
                          <p className="font-medium">{user.fullName || "—"}</p>
                          <p className="text-sm text-muted-foreground">
                            {user.email}
                          </p>
                        </div>
                      </TableCell>
                      <TableCell>
                        <div className="flex flex-wrap gap-1">
                          {user.roles.length === 0 ? (
                            <span className="text-sm text-muted-foreground">
                              No roles
                            </span>
                          ) : (
                            user.roles.map((role) => (
                              <Badge
                                key={role.id}
                                variant="secondary"
                                className="text-xs"
                              >
                                {role.name}
                              </Badge>
                            ))
                          )}
                        </div>
                      </TableCell>
                      <TableCell>
                        <Badge
                          variant={user.isActive ? "default" : "secondary"}
                        >
                          {user.isActive ? "Active" : "Inactive"}
                        </Badge>
                      </TableCell>
                      <TableCell>
                        {user.lastLoginAt
                          ? formatDateTime(user.lastLoginAt)
                          : "Never"}
                      </TableCell>
                      <TableCell onClick={(e) => e.stopPropagation()}>
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" size="icon">
                              <MoreHorizontal className="h-4 w-4" />
                              <span className="sr-only">Actions</span>
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuItem
                              onClick={() => setSelectedUserForEdit(user)}
                            >
                              Edit User
                            </DropdownMenuItem>
                            <DropdownMenuItem
                              onClick={() => setSelectedUserForRoles(user)}
                            >
                              Manage Roles
                            </DropdownMenuItem>
                            <DropdownMenuSeparator />
                            {user.isActive ? (
                              <DropdownMenuItem
                                onClick={() => handleDeactivate(user.id)}
                                className="text-destructive"
                              >
                                <UserX className="h-4 w-4 mr-2" />
                                Deactivate
                              </DropdownMenuItem>
                            ) : (
                              <DropdownMenuItem
                                onClick={() => handleActivate(user.id)}
                              >
                                <UserCheck className="h-4 w-4 mr-2" />
                                Activate
                              </DropdownMenuItem>
                            )}
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
            <div className="flex items-center justify-between mt-4">
              <p className="text-sm text-muted-foreground">
                Page {data.pageNumber} of {data.totalPages} ({data.totalCount} users)
              </p>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  disabled={!data.hasPreviousPage}
                  onClick={() =>
                    setFilters((prev) => ({ ...prev, page: data.pageNumber - 1 }))
                  }
                >
                  Previous
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  disabled={!data.hasNextPage}
                  onClick={() =>
                    setFilters((prev) => ({ ...prev, page: data.pageNumber + 1 }))
                  }
                >
                  Next
                </Button>
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Create/Edit Dialog */}
      <CustomerUserFormDialog
        customerId={customerId}
        open={isCreateDialogOpen || !!selectedUserForEdit}
        onOpenChange={(open) => {
          if (!open) {
            setIsCreateDialogOpen(false);
            setSelectedUserForEdit(null);
          }
        }}
        user={selectedUserForEdit}
      />

      {/* Roles Dialog */}
      <CustomerUserRolesDialog
        customerId={customerId}
        open={!!selectedUserForRoles}
        onOpenChange={(open) => {
          if (!open) {
            setSelectedUserForRoles(null);
          }
        }}
        user={selectedUserForRoles}
      />
    </>
  );
}
