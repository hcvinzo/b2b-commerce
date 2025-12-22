"use client";

import { useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { ArrowLeft, Pencil, Loader2, Mail, Phone, Calendar } from "lucide-react";

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
import { AdminUserForm } from "@/components/forms/admin-user-form";
import { UserRolesEditor } from "@/components/admin-users/user-roles-editor";
import { UserLoginHistory } from "@/components/admin-users/user-login-history";
import { UserClaimsEditor } from "@/components/admin-users/user-claims-editor";
import { useAdminUser, useUpdateAdminUser } from "@/hooks/use-admin-users";
import { type UpdateAdminUserFormData } from "@/lib/validations/admin-user";
import { formatDateTime } from "@/lib/utils";

export default function AdminUserDetailPage() {
  const params = useParams();
  const router = useRouter();
  const userId = params.id as string;

  const { data: user, isLoading, refetch } = useAdminUser(userId);
  const updateAdminUser = useUpdateAdminUser();

  const [isEditOpen, setIsEditOpen] = useState(false);

  const handleEditSubmit = async (formData: UpdateAdminUserFormData) => {
    await updateAdminUser.mutateAsync({
      id: userId,
      data: {
        firstName: formData.firstName || undefined,
        lastName: formData.lastName || undefined,
        phoneNumber: formData.phoneNumber || undefined,
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

  if (!user) {
    return (
      <div className="text-center py-10">
        <h3 className="text-lg font-semibold">User not found</h3>
        <p className="text-muted-foreground">
          The admin user you&apos;re looking for doesn&apos;t exist.
        </p>
        <Button className="mt-4" onClick={() => router.push("/admin-users")}>
          Back to Admin Users
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
            onClick={() => router.push("/admin-users")}
          >
            <ArrowLeft className="h-4 w-4" />
          </Button>
          <div>
            <div className="flex items-center gap-2">
              <h1 className="text-3xl font-bold tracking-tight">
                {user.firstName || user.lastName
                  ? `${user.firstName || ""} ${user.lastName || ""}`.trim()
                  : user.email}
              </h1>
              <Badge variant={user.isActive ? "default" : "secondary"}>
                {user.isActive ? "Active" : "Inactive"}
              </Badge>
            </div>
            <p className="text-muted-foreground">{user.email}</p>
          </div>
        </div>
        <Button onClick={() => setIsEditOpen(true)}>
          <Pencil className="mr-2 h-4 w-4" />
          Edit User
        </Button>
      </div>

      {/* Info Cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="pb-2">
            <CardDescription className="flex items-center gap-2">
              <Mail className="h-4 w-4" />
              Email
            </CardDescription>
            <CardTitle className="text-base">{user.email}</CardTitle>
          </CardHeader>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardDescription className="flex items-center gap-2">
              <Phone className="h-4 w-4" />
              Phone
            </CardDescription>
            <CardTitle className="text-base">
              {user.phoneNumber || "-"}
            </CardTitle>
          </CardHeader>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardDescription className="flex items-center gap-2">
              <Calendar className="h-4 w-4" />
              Last Login
            </CardDescription>
            <CardTitle className="text-base">
              {user.lastLoginAt ? formatDateTime(user.lastLoginAt) : "Never"}
            </CardTitle>
          </CardHeader>
        </Card>
        <Card>
          <CardHeader className="pb-2">
            <CardDescription className="flex items-center gap-2">
              <Calendar className="h-4 w-4" />
              Created
            </CardDescription>
            <CardTitle className="text-base">
              {formatDateTime(user.createdAt)}
            </CardTitle>
          </CardHeader>
        </Card>
      </div>

      {/* Tabs */}
      <Tabs defaultValue="roles" className="space-y-4">
        <TabsList>
          <TabsTrigger value="roles">Roles</TabsTrigger>
          <TabsTrigger value="logins">Login History</TabsTrigger>
          <TabsTrigger value="claims">Claims</TabsTrigger>
        </TabsList>

        <TabsContent value="roles" className="space-y-4">
          <UserRolesEditor userId={userId} onSaved={() => refetch()} />
        </TabsContent>

        <TabsContent value="logins">
          <UserLoginHistory userId={userId} />
        </TabsContent>

        <TabsContent value="claims">
          <UserClaimsEditor userId={userId} />
        </TabsContent>
      </Tabs>

      {/* Edit Dialog */}
      <Dialog open={isEditOpen} onOpenChange={setIsEditOpen}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>Edit User</DialogTitle>
            <DialogDescription>
              Update the user information below.
            </DialogDescription>
          </DialogHeader>
          <AdminUserForm
            defaultValues={{
              email: user.email,
              firstName: user.firstName || "",
              lastName: user.lastName || "",
              phoneNumber: user.phoneNumber || "",
              roles: user.roles,
            }}
            onSubmit={handleEditSubmit}
            onCancel={() => setIsEditOpen(false)}
            isLoading={updateAdminUser.isPending}
            isEdit
            hideRoles
          />
        </DialogContent>
      </Dialog>
    </div>
  );
}
