"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import {
  Plus,
  MoreHorizontal,
  Pencil,
  Trash2,
  Power,
  PowerOff,
  RefreshCw,
  Users,
  KeyRound,
  Search,
  Eye,
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
import { Input } from "@/components/ui/input";
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
import { AdminUserForm } from "@/components/forms/admin-user-form";
import {
  useAdminUsers,
  useCreateAdminUser,
  useDeleteAdminUser,
  useActivateAdminUser,
  useDeactivateAdminUser,
  useResetAdminUserPassword,
} from "@/hooks/use-admin-users";
import { type AdminUserFormData } from "@/lib/validations/admin-user";
import {
  type AdminUserListItem,
  type AdminUserFilters,
} from "@/types/entities";
import { formatDateTime } from "@/lib/utils";
import { useAuthStore } from "@/stores/auth-store";

export default function AdminUsersPage() {
  const router = useRouter();
  const { user: currentUser } = useAuthStore();
  const [filters, setFilters] = useState<AdminUserFilters>({
    page: 1,
    pageSize: 10,
  });
  const [searchInput, setSearchInput] = useState("");

  const [isFormOpen, setIsFormOpen] = useState(false);
  const [deletingUser, setDeletingUser] = useState<AdminUserListItem | null>(
    null
  );
  const [resettingPasswordUser, setResettingPasswordUser] =
    useState<AdminUserListItem | null>(null);

  const { data, isLoading, isFetching, refetch } = useAdminUsers(filters);
  const createAdminUser = useCreateAdminUser();
  const deleteAdminUser = useDeleteAdminUser();
  const activateAdminUser = useActivateAdminUser();
  const deactivateAdminUser = useDeactivateAdminUser();
  const resetPassword = useResetAdminUserPassword();

  const handleAddNew = () => {
    setIsFormOpen(true);
  };

  const handleView = (user: AdminUserListItem) => {
    router.push(`/admin-users/${user.id}`);
  };

  const handleFormSubmit = async (formData: AdminUserFormData) => {
    await createAdminUser.mutateAsync({
      email: formData.email,
      firstName: formData.firstName || undefined,
      lastName: formData.lastName || undefined,
      phoneNumber: formData.phoneNumber || undefined,
      roles: formData.roles,
      temporaryPassword: formData.temporaryPassword || undefined,
      sendWelcomeEmail: formData.sendWelcomeEmail,
    });
    setIsFormOpen(false);
  };

  // Wrapper to satisfy the form's union type
  const handleFormSubmitWrapper = async (
    data: AdminUserFormData | { firstName?: string; lastName?: string; phoneNumber?: string; roles: string[] }
  ) => {
    // We only use this for create, so we know it's AdminUserFormData
    await handleFormSubmit(data as AdminUserFormData);
  };

  const handleDelete = async () => {
    if (deletingUser) {
      await deleteAdminUser.mutateAsync(deletingUser.id);
      setDeletingUser(null);
    }
  };

  const handleResetPassword = async () => {
    if (resettingPasswordUser) {
      await resetPassword.mutateAsync(resettingPasswordUser.id);
      setResettingPasswordUser(null);
    }
  };

  const handleToggleActive = async (user: AdminUserListItem) => {
    if (user.isActive) {
      await deactivateAdminUser.mutateAsync(user.id);
    } else {
      await activateAdminUser.mutateAsync(user.id);
    }
  };

  const handleStatusFilterChange = (value: string) => {
    setFilters((prev) => ({
      ...prev,
      isActive: value === "all" ? undefined : value === "active",
      page: 1,
    }));
  };

  const handleSearch = () => {
    setFilters((prev) => ({
      ...prev,
      search: searchInput || undefined,
      page: 1,
    }));
  };

  const handleSearchKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === "Enter") {
      handleSearch();
    }
  };

  const isCurrentUser = (userId: string) => {
    return currentUser?.id === userId;
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">Admin Users</h1>
          <p className="text-muted-foreground">
            Manage administrators and business users
          </p>
        </div>
        <Button onClick={handleAddNew}>
          <Plus className="mr-2 h-4 w-4" />
          Add User
        </Button>
      </div>

      <Card>
        <CardHeader className="pb-4">
          <CardTitle>All Admin Users</CardTitle>
          <CardDescription>
            Users with access to the admin panel
          </CardDescription>
        </CardHeader>
        <CardContent>
          {/* Filters */}
          <div className="flex items-center gap-4 mb-6">
            <div className="flex gap-2 flex-1 max-w-sm">
              <Input
                placeholder="Search by name or email..."
                value={searchInput}
                onChange={(e) => setSearchInput(e.target.value)}
                onKeyDown={handleSearchKeyDown}
              />
              <Button variant="outline" size="icon" onClick={handleSearch}>
                <Search className="h-4 w-4" />
              </Button>
            </div>
            <Select
              value={
                filters.isActive === undefined
                  ? "all"
                  : filters.isActive
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
            <Button
              variant="outline"
              size="icon"
              onClick={() => refetch()}
              disabled={isFetching}
            >
              <RefreshCw
                className={`h-4 w-4 ${isFetching ? "animate-spin" : ""}`}
              />
            </Button>
          </div>

          {/* Table */}
          {isLoading ? (
            <div className="space-y-3">
              {[...Array(5)].map((_, i) => (
                <Skeleton key={i} className="h-16 w-full" />
              ))}
            </div>
          ) : data?.items && data.items.length > 0 ? (
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Name</TableHead>
                    <TableHead>Email</TableHead>
                    <TableHead>Roles</TableHead>
                    <TableHead className="text-center">Status</TableHead>
                    <TableHead>Last Login</TableHead>
                    <TableHead>Created</TableHead>
                    <TableHead className="w-[70px]"></TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {data.items.map((user) => (
                    <TableRow key={user.id}>
                      <TableCell>
                        <div className="flex items-center gap-2">
                          <span className="font-medium">
                            {user.fullName || "-"}
                          </span>
                          {isCurrentUser(user.id) && (
                            <Badge variant="outline" className="text-xs">
                              You
                            </Badge>
                          )}
                        </div>
                      </TableCell>
                      <TableCell>{user.email}</TableCell>
                      <TableCell>
                        <div className="flex gap-1 flex-wrap">
                          {user.roles.map((role) => (
                            <Badge key={role} variant="secondary">
                              {role}
                            </Badge>
                          ))}
                        </div>
                      </TableCell>
                      <TableCell className="text-center">
                        <Badge
                          variant={user.isActive ? "default" : "secondary"}
                        >
                          {user.isActive ? "Active" : "Inactive"}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-muted-foreground text-sm">
                        {user.lastLoginAt
                          ? formatDateTime(user.lastLoginAt)
                          : "Never"}
                      </TableCell>
                      <TableCell className="text-muted-foreground text-sm">
                        {formatDateTime(user.createdAt)}
                      </TableCell>
                      <TableCell>
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" size="icon">
                              <MoreHorizontal className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuItem onClick={() => handleView(user)}>
                              <Eye className="mr-2 h-4 w-4" />
                              View
                            </DropdownMenuItem>
                            <DropdownMenuItem onClick={() => handleView(user)}>
                              <Pencil className="mr-2 h-4 w-4" />
                              Edit
                            </DropdownMenuItem>
                            <DropdownMenuItem
                              onClick={() => setResettingPasswordUser(user)}
                            >
                              <KeyRound className="mr-2 h-4 w-4" />
                              Reset Password
                            </DropdownMenuItem>
                            <DropdownMenuItem
                              onClick={() => handleToggleActive(user)}
                              disabled={isCurrentUser(user.id)}
                            >
                              {user.isActive ? (
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
                              onClick={() => setDeletingUser(user)}
                              disabled={isCurrentUser(user.id)}
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
              <Users className="mx-auto h-12 w-12 text-muted-foreground" />
              <h3 className="mt-4 text-lg font-semibold">No admin users</h3>
              <p className="text-muted-foreground">
                Get started by creating your first admin user.
              </p>
              <Button className="mt-4" onClick={handleAddNew}>
                <Plus className="mr-2 h-4 w-4" />
                Add User
              </Button>
            </div>
          )}

          {/* Pagination */}
          {data && data.totalCount > 0 && (
            <div className="flex items-center justify-between mt-4 text-sm text-muted-foreground">
              <div>
                Showing {(data.pageNumber - 1) * data.pageSize + 1} to{" "}
                {Math.min(data.pageNumber * data.pageSize, data.totalCount)} of{" "}
                {data.totalCount} users
              </div>
              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  disabled={!data.hasPreviousPage}
                  onClick={() =>
                    setFilters((prev) => ({
                      ...prev,
                      page: (prev.page || 1) - 1,
                    }))
                  }
                >
                  Previous
                </Button>
                <Button
                  variant="outline"
                  size="sm"
                  disabled={!data.hasNextPage}
                  onClick={() =>
                    setFilters((prev) => ({
                      ...prev,
                      page: (prev.page || 1) + 1,
                    }))
                  }
                >
                  Next
                </Button>
              </div>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Form Dialog */}
      <Dialog open={isFormOpen} onOpenChange={setIsFormOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Add Admin User</DialogTitle>
            <DialogDescription>
              Fill in the details to create a new admin user.
            </DialogDescription>
          </DialogHeader>
          <AdminUserForm
            onSubmit={handleFormSubmitWrapper}
            onCancel={() => setIsFormOpen(false)}
            isLoading={createAdminUser.isPending}
          />
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation */}
      <ConfirmDialog
        open={!!deletingUser}
        onOpenChange={(open) => !open && setDeletingUser(null)}
        title="Delete Admin User"
        description={`Are you sure you want to delete "${deletingUser?.fullName || deletingUser?.email}"? This action cannot be undone.`}
        confirmText="Delete"
        variant="destructive"
        onConfirm={handleDelete}
        isLoading={deleteAdminUser.isPending}
      />

      {/* Reset Password Confirmation */}
      <ConfirmDialog
        open={!!resettingPasswordUser}
        onOpenChange={(open) => !open && setResettingPasswordUser(null)}
        title="Reset Password"
        description={`This will reset the password for "${resettingPasswordUser?.fullName || resettingPasswordUser?.email}" and send them an email with the new credentials.`}
        confirmText="Reset Password"
        variant="default"
        onConfirm={handleResetPassword}
        isLoading={resetPassword.isPending}
      />
    </div>
  );
}
