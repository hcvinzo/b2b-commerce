"use client";

import { useState } from "react";
import Link from "next/link";
import {
  Plus,
  MoreHorizontal,
  Pencil,
  Trash2,
  Key,
  Power,
  PowerOff,
  RefreshCw,
  ExternalLink,
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
import { ApiClientForm } from "@/components/forms/api-client-form";
import {
  useApiClients,
  useCreateApiClient,
  useUpdateApiClient,
  useDeleteApiClient,
  useActivateApiClient,
  useDeactivateApiClient,
} from "@/hooks/use-api-clients";
import { type ApiClientFormData } from "@/lib/validations/api-client";
import { type ApiClientListItem, type ApiClientFilters } from "@/types/entities";
import { formatDateTime } from "@/lib/utils";

export default function ApiClientsPage() {
  const [filters, setFilters] = useState<ApiClientFilters>({
    page: 1,
    pageSize: 10,
  });

  const [isFormOpen, setIsFormOpen] = useState(false);
  const [editingClient, setEditingClient] = useState<ApiClientListItem | null>(
    null
  );
  const [deletingClient, setDeletingClient] =
    useState<ApiClientListItem | null>(null);

  const { data, isLoading, isFetching, refetch } = useApiClients(filters);
  const createApiClient = useCreateApiClient();
  const updateApiClient = useUpdateApiClient();
  const deleteApiClient = useDeleteApiClient();
  const activateApiClient = useActivateApiClient();
  const deactivateApiClient = useDeactivateApiClient();

  const handleAddNew = () => {
    setEditingClient(null);
    setIsFormOpen(true);
  };

  const handleEdit = (client: ApiClientListItem) => {
    setEditingClient(client);
    setIsFormOpen(true);
  };

  const handleFormSubmit = async (formData: ApiClientFormData) => {
    if (editingClient) {
      await updateApiClient.mutateAsync({
        id: editingClient.id,
        data: {
          name: formData.name,
          description: formData.description || undefined,
          contactEmail: formData.contactEmail,
          contactPhone: formData.contactPhone || undefined,
        },
      });
    } else {
      await createApiClient.mutateAsync({
        name: formData.name,
        description: formData.description || undefined,
        contactEmail: formData.contactEmail,
        contactPhone: formData.contactPhone || undefined,
      });
    }
    setIsFormOpen(false);
    setEditingClient(null);
  };

  const handleDelete = async () => {
    if (deletingClient) {
      await deleteApiClient.mutateAsync(deletingClient.id);
      setDeletingClient(null);
    }
  };

  const handleToggleActive = async (client: ApiClientListItem) => {
    if (client.isActive) {
      await deactivateApiClient.mutateAsync(client.id);
    } else {
      await activateApiClient.mutateAsync(client.id);
    }
  };

  const handleStatusFilterChange = (value: string) => {
    setFilters((prev) => ({
      ...prev,
      isActive: value === "all" ? undefined : value === "active",
      page: 1,
    }));
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">API Clients</h1>
          <p className="text-muted-foreground">
            Manage API clients for external integrations
          </p>
        </div>
        <Button onClick={handleAddNew}>
          <Plus className="mr-2 h-4 w-4" />
          Add Client
        </Button>
      </div>

      <Card>
        <CardHeader className="pb-4">
          <CardTitle>All Clients</CardTitle>
          <CardDescription>
            API clients with their active key counts
          </CardDescription>
        </CardHeader>
        <CardContent>
          {/* Filters */}
          <div className="flex items-center gap-4 mb-6">
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
                    <TableHead>Client Name</TableHead>
                    <TableHead>Contact Email</TableHead>
                    <TableHead className="text-center">Active Keys</TableHead>
                    <TableHead className="text-center">Status</TableHead>
                    <TableHead>Created</TableHead>
                    <TableHead className="w-[70px]"></TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {data.items.map((client) => (
                    <TableRow key={client.id}>
                      <TableCell>
                        <Link
                          href={`/api-clients/${client.id}`}
                          className="font-medium hover:underline"
                        >
                          {client.name}
                        </Link>
                      </TableCell>
                      <TableCell>{client.contactEmail}</TableCell>
                      <TableCell className="text-center">
                        <Badge variant="secondary">
                          <Key className="mr-1 h-3 w-3" />
                          {client.activeKeyCount} keys
                        </Badge>
                      </TableCell>
                      <TableCell className="text-center">
                        <Badge
                          variant={client.isActive ? "default" : "secondary"}
                        >
                          {client.isActive ? "Active" : "Inactive"}
                        </Badge>
                      </TableCell>
                      <TableCell className="text-muted-foreground text-sm">
                        {formatDateTime(client.createdAt)}
                      </TableCell>
                      <TableCell>
                        <DropdownMenu>
                          <DropdownMenuTrigger asChild>
                            <Button variant="ghost" size="icon">
                              <MoreHorizontal className="h-4 w-4" />
                            </Button>
                          </DropdownMenuTrigger>
                          <DropdownMenuContent align="end">
                            <DropdownMenuItem asChild>
                              <Link href={`/api-clients/${client.id}`}>
                                <ExternalLink className="mr-2 h-4 w-4" />
                                View Details
                              </Link>
                            </DropdownMenuItem>
                            <DropdownMenuItem onClick={() => handleEdit(client)}>
                              <Pencil className="mr-2 h-4 w-4" />
                              Edit
                            </DropdownMenuItem>
                            <DropdownMenuItem
                              onClick={() => handleToggleActive(client)}
                            >
                              {client.isActive ? (
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
                              onClick={() => setDeletingClient(client)}
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
              <Key className="mx-auto h-12 w-12 text-muted-foreground" />
              <h3 className="mt-4 text-lg font-semibold">No API clients</h3>
              <p className="text-muted-foreground">
                Get started by creating your first API client.
              </p>
              <Button className="mt-4" onClick={handleAddNew}>
                <Plus className="mr-2 h-4 w-4" />
                Add Client
              </Button>
            </div>
          )}

          {/* Pagination */}
          {data && data.totalCount > 0 && (
            <div className="flex items-center justify-between mt-4 text-sm text-muted-foreground">
              <div>
                Showing {(data.pageNumber - 1) * data.pageSize + 1} to{" "}
                {Math.min(data.pageNumber * data.pageSize, data.totalCount)} of{" "}
                {data.totalCount} clients
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
            <DialogTitle>
              {editingClient ? "Edit API Client" : "Add API Client"}
            </DialogTitle>
            <DialogDescription>
              {editingClient
                ? "Update the API client information below."
                : "Fill in the details to create a new API client."}
            </DialogDescription>
          </DialogHeader>
          <ApiClientForm
            defaultValues={
              editingClient
                ? {
                    name: editingClient.name,
                    contactEmail: editingClient.contactEmail,
                  }
                : undefined
            }
            onSubmit={handleFormSubmit}
            onCancel={() => setIsFormOpen(false)}
            isLoading={createApiClient.isPending || updateApiClient.isPending}
          />
        </DialogContent>
      </Dialog>

      {/* Delete Confirmation */}
      <ConfirmDialog
        open={!!deletingClient}
        onOpenChange={(open) => !open && setDeletingClient(null)}
        title="Delete API Client"
        description={
          deletingClient?.activeKeyCount && deletingClient.activeKeyCount > 0
            ? `This client has ${deletingClient.activeKeyCount} active API keys. You must revoke all keys before deleting the client.`
            : `Are you sure you want to delete "${deletingClient?.name}"? This action cannot be undone.`
        }
        confirmText="Delete"
        variant="destructive"
        onConfirm={handleDelete}
        isLoading={deleteApiClient.isPending}
      />
    </div>
  );
}
