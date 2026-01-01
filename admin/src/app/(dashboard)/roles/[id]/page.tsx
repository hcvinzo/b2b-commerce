"use client";

import { useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { ArrowLeft, Pencil, Shield, Loader2 } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { RoleForm } from "@/components/forms/role-form";
import { PermissionsEditor } from "@/components/roles/permissions-editor";
import { RoleUsers } from "@/components/roles/role-users";
import { useRole, useUpdateRole } from "@/hooks/use-roles";
import { type RoleFormData } from "@/lib/validations/role";
import { UserTypeLabels } from "@/types/entities";
import { formatDateTime } from "@/lib/utils";

export default function RoleDetailPage() {
  const params = useParams();
  const router = useRouter();
  const roleId = params.id as string;

  const { data: role, isLoading, refetch } = useRole(roleId);
  const updateRole = useUpdateRole();

  const [isEditOpen, setIsEditOpen] = useState(false);

  const handleEditSubmit = async (formData: RoleFormData) => {
    await updateRole.mutateAsync({
      id: roleId,
      data: {
        name: role?.isSystemRole ? undefined : formData.name,
        description: formData.description || undefined,
      },
    });
    setIsEditOpen(false);
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  if (!role) {
    return (
      <div className="text-center py-10">
        <h3 className="text-lg font-semibold">Role not found</h3>
        <p className="text-muted-foreground">
          The role you&apos;re looking for doesn&apos;t exist.
        </p>
        <Button className="mt-4" onClick={() => router.push("/roles")}>
          Back to Roles
        </Button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button
            variant="ghost"
            size="icon"
            onClick={() => router.push("/roles")}
          >
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <div className="flex items-center gap-2">
              <h1 className="text-3xl font-bold tracking-tight">{role.name}</h1>
              <Badge
                variant={role.userType === "Admin" ? "default" : "outline"}
              >
                {UserTypeLabels[role.userType]}
              </Badge>
              {role.isProtected && (
                <Badge variant="outline">
                  <Shield className="mr-1 h-3 w-3" />
                  Protected
                </Badge>
              )}
              {role.isSystemRole && !role.isProtected && (
                <Badge variant="secondary">System</Badge>
              )}
            </div>
            <p className="text-muted-foreground">
              {role.description || "No description"}
            </p>
          </div>
        </div>
        <Button onClick={() => setIsEditOpen(true)}>
          <Pencil className="mr-2 h-4 w-4" />
          Edit Role
        </Button>
      </div>

      {/* Stats */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Type</CardDescription>
            <CardTitle className="text-2xl">{UserTypeLabels[role.userType]}</CardTitle>
          </CardHeader>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Users</CardDescription>
            <CardTitle className="text-2xl">{role.userCount}</CardTitle>
          </CardHeader>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Permissions</CardDescription>
            <CardTitle className="text-2xl">
              {role.isProtected ? "All" : role.claims.length}
            </CardTitle>
          </CardHeader>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Created</CardDescription>
            <CardTitle className="text-lg">
              {formatDateTime(role.createdAt)}
            </CardTitle>
          </CardHeader>
        </Card>
      </div>

      {/* Tabs */}
      <Tabs defaultValue="permissions" className="space-y-4">
        <TabsList>
          <TabsTrigger value="permissions">Permissions</TabsTrigger>
          <TabsTrigger value="users">Users ({role.userCount})</TabsTrigger>
        </TabsList>

        <TabsContent value="permissions" className="space-y-4">
          <PermissionsEditor
            roleId={roleId}
            currentClaims={role.claims}
            isProtected={role.isProtected}
            onSaved={() => refetch()}
          />
        </TabsContent>

        <TabsContent value="users">
          <Card>
            <CardHeader>
              <CardTitle>Users in this Role</CardTitle>
              <CardDescription>
                Users who have been assigned to this role
              </CardDescription>
            </CardHeader>
            <CardContent>
              <RoleUsers roleId={roleId} />
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Edit Dialog */}
      <Dialog open={isEditOpen} onOpenChange={setIsEditOpen}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>Edit Role</DialogTitle>
            <DialogDescription>
              Update the role information below.
            </DialogDescription>
          </DialogHeader>
          <RoleForm
            defaultValues={{
              name: role.name,
              description: role.description || "",
            }}
            onSubmit={handleEditSubmit}
            onCancel={() => setIsEditOpen(false)}
            isLoading={updateRole.isPending}
            isEdit
            isSystemRole={role.isSystemRole}
          />
        </DialogContent>
      </Dialog>
    </div>
  );
}
