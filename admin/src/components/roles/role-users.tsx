"use client";

import { useState } from "react";
import { Loader2, UserMinus, Users } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { useRoleUsers, useRemoveUserFromRole } from "@/hooks/use-roles";
import { type RoleUserListItem } from "@/types/entities";

interface RoleUsersProps {
  roleId: string;
}

export function RoleUsers({ roleId }: RoleUsersProps) {
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const { data, isLoading, isFetching } = useRoleUsers(roleId, page, pageSize);
  const removeUserFromRole = useRemoveUserFromRole();

  const [removingUser, setRemovingUser] = useState<RoleUserListItem | null>(
    null
  );

  const handleRemoveUser = async () => {
    if (removingUser) {
      await removeUserFromRole.mutateAsync({
        roleId,
        userId: removingUser.id,
      });
      setRemovingUser(null);
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-8">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  if (!data?.items || data.items.length === 0) {
    return (
      <div className="text-center py-10">
        <Users className="mx-auto h-12 w-12 text-muted-foreground" />
        <h3 className="mt-4 text-lg font-semibold">No users in this role</h3>
        <p className="text-muted-foreground">
          Users can be added to this role from the Admin Users page.
        </p>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="rounded-md border">
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Name</TableHead>
              <TableHead>Email</TableHead>
              <TableHead className="text-center">Status</TableHead>
              <TableHead className="w-[100px]"></TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {data.items.map((user) => (
              <TableRow key={user.id}>
                <TableCell className="font-medium">
                  {user.fullName || "-"}
                </TableCell>
                <TableCell>{user.email}</TableCell>
                <TableCell className="text-center">
                  <Badge variant={user.isActive ? "default" : "secondary"}>
                    {user.isActive ? "Active" : "Inactive"}
                  </Badge>
                </TableCell>
                <TableCell>
                  <Button
                    variant="ghost"
                    size="sm"
                    className="text-destructive hover:text-destructive"
                    onClick={() => setRemovingUser(user)}
                  >
                    <UserMinus className="h-4 w-4" />
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      {/* Pagination */}
      {data.totalCount > pageSize && (
        <div className="flex items-center justify-between text-sm text-muted-foreground">
          <div>
            Showing {(data.pageNumber - 1) * data.pageSize + 1} to{" "}
            {Math.min(data.pageNumber * data.pageSize, data.totalCount)} of{" "}
            {data.totalCount} users
          </div>
          <div className="flex gap-2">
            <Button
              variant="outline"
              size="sm"
              disabled={!data.hasPreviousPage || isFetching}
              onClick={() => setPage((p) => p - 1)}
            >
              Previous
            </Button>
            <Button
              variant="outline"
              size="sm"
              disabled={!data.hasNextPage || isFetching}
              onClick={() => setPage((p) => p + 1)}
            >
              Next
            </Button>
          </div>
        </div>
      )}

      {/* Remove User Confirmation */}
      <ConfirmDialog
        open={!!removingUser}
        onOpenChange={(open) => !open && setRemovingUser(null)}
        title="Remove User from Role"
        description={`Are you sure you want to remove "${removingUser?.fullName || removingUser?.email}" from this role?`}
        confirmText="Remove"
        variant="destructive"
        onConfirm={handleRemoveUser}
        isLoading={removeUserFromRole.isPending}
      />
    </div>
  );
}
