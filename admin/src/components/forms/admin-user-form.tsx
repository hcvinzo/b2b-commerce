"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2 } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import {
  adminUserSchema,
  updateAdminUserSchema,
  type AdminUserFormData,
  type UpdateAdminUserFormData,
} from "@/lib/validations/admin-user";
import { useAvailableRoles } from "@/hooks/use-admin-users";

interface AdminUserFormProps {
  defaultValues?: Partial<AdminUserFormData>;
  onSubmit: (data: AdminUserFormData | UpdateAdminUserFormData) => Promise<void>;
  onCancel?: () => void;
  isLoading?: boolean;
  isEdit?: boolean;
  hideRoles?: boolean;
}

export function AdminUserForm({
  defaultValues,
  onSubmit,
  onCancel,
  isLoading,
  isEdit = false,
  hideRoles = false,
}: AdminUserFormProps) {
  const { data: availableRoles, isLoading: rolesLoading } = useAvailableRoles();

  // Use type assertion for resolver since schema changes based on isEdit
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  const form = useForm<AdminUserFormData>({
    resolver: zodResolver(isEdit ? updateAdminUserSchema : adminUserSchema) as any,
    defaultValues: {
      email: "",
      firstName: "",
      lastName: "",
      phoneNumber: "",
      roles: [],
      temporaryPassword: "",
      sendWelcomeEmail: true,
      ...defaultValues,
    },
  });

  const handleFormSubmit = async (data: AdminUserFormData) => {
    if (isEdit) {
      // For updates, only send the update fields
      const updateData: UpdateAdminUserFormData = {
        firstName: data.firstName,
        lastName: data.lastName,
        phoneNumber: data.phoneNumber,
        roles: data.roles,
      };
      await onSubmit(updateData);
    } else {
      await onSubmit(data);
    }
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(handleFormSubmit)} className="space-y-6">
        <div className="grid gap-6 md:grid-cols-2">
          <FormField
            control={form.control}
            name="email"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Email *</FormLabel>
                <FormControl>
                  <Input
                    type="email"
                    placeholder="admin@example.com"
                    disabled={isEdit}
                    {...field}
                  />
                </FormControl>
                <FormDescription>
                  {isEdit
                    ? "Email cannot be changed"
                    : "This will be used for login"}
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="phoneNumber"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Phone Number</FormLabel>
                <FormControl>
                  <Input
                    placeholder="+90 555 123 4567"
                    {...field}
                    value={field.value || ""}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="firstName"
            render={({ field }) => (
              <FormItem>
                <FormLabel>First Name</FormLabel>
                <FormControl>
                  <Input
                    placeholder="Enter first name"
                    {...field}
                    value={field.value || ""}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="lastName"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Last Name</FormLabel>
                <FormControl>
                  <Input
                    placeholder="Enter last name"
                    {...field}
                    value={field.value || ""}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        {!hideRoles && (
          <FormField
            control={form.control}
            name="roles"
            render={() => (
              <FormItem>
                <FormLabel>Roles *</FormLabel>
                <FormDescription>
                  Select the roles for this user
                </FormDescription>
                <div className="grid gap-3 mt-2">
                  {rolesLoading ? (
                    <div className="flex items-center gap-2 text-muted-foreground">
                      <Loader2 className="h-4 w-4 animate-spin" />
                      Loading roles...
                    </div>
                  ) : (
                    availableRoles?.map((role) => (
                      <FormField
                        key={role.name}
                        control={form.control}
                        name="roles"
                        render={({ field }) => (
                          <FormItem className="flex items-start gap-3 space-y-0">
                            <FormControl>
                              <Checkbox
                                checked={field.value?.includes(role.name)}
                                onCheckedChange={(checked) => {
                                  const currentValue = field.value || [];
                                  if (checked) {
                                    field.onChange([...currentValue, role.name]);
                                  } else {
                                    field.onChange(
                                      currentValue.filter((r) => r !== role.name)
                                    );
                                  }
                                }}
                              />
                            </FormControl>
                            <div className="space-y-0.5">
                              <FormLabel className="font-normal cursor-pointer">
                                {role.name}
                              </FormLabel>
                              {role.description && (
                                <p className="text-xs text-muted-foreground">
                                  {role.description}
                                </p>
                              )}
                            </div>
                          </FormItem>
                        )}
                      />
                    ))
                  )}
                </div>
                <FormMessage />
              </FormItem>
            )}
          />
        )}

        {!isEdit && (
          <>
            <FormField
              control={form.control}
              name="temporaryPassword"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Temporary Password</FormLabel>
                  <FormControl>
                    <Input
                      type="password"
                      placeholder="Leave empty to auto-generate"
                      {...field}
                      value={field.value || ""}
                    />
                  </FormControl>
                  <FormDescription>
                    If left empty, a secure password will be generated
                    automatically
                  </FormDescription>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="sendWelcomeEmail"
              render={({ field }) => (
                <FormItem className="flex items-center gap-3 space-y-0">
                  <FormControl>
                    <Checkbox
                      checked={field.value}
                      onCheckedChange={field.onChange}
                    />
                  </FormControl>
                  <div className="space-y-0.5">
                    <FormLabel className="font-normal cursor-pointer">
                      Send welcome email
                    </FormLabel>
                    <FormDescription>
                      Send an email with login credentials to the user
                    </FormDescription>
                  </div>
                </FormItem>
              )}
            />
          </>
        )}

        <div className="flex justify-end gap-4">
          {onCancel && (
            <Button type="button" variant="outline" onClick={onCancel}>
              Cancel
            </Button>
          )}
          <Button type="submit" disabled={isLoading}>
            {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            {isEdit ? "Save Changes" : "Create User"}
          </Button>
        </div>
      </form>
    </Form>
  );
}
