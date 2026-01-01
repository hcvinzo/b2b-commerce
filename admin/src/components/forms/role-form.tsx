"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2 } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
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
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { roleSchema, type RoleFormData } from "@/lib/validations/role";

interface RoleFormProps {
  defaultValues?: Partial<RoleFormData>;
  onSubmit: (data: RoleFormData) => Promise<void>;
  onCancel?: () => void;
  isLoading?: boolean;
  isEdit?: boolean;
  isSystemRole?: boolean;
}

export function RoleForm({
  defaultValues,
  onSubmit,
  onCancel,
  isLoading,
  isEdit = false,
  isSystemRole = false,
}: RoleFormProps) {
  const form = useForm<RoleFormData>({
    resolver: zodResolver(roleSchema),
    defaultValues: {
      name: "",
      description: "",
      userType: "Admin",
      ...defaultValues,
    },
  });

  const handleFormSubmit = async (data: RoleFormData) => {
    await onSubmit(data);
  };

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(handleFormSubmit)} className="space-y-6">
        <FormField
          control={form.control}
          name="name"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Role Name *</FormLabel>
              <FormControl>
                <Input
                  placeholder="e.g., Warehouse, Accountant"
                  disabled={isEdit && isSystemRole}
                  {...field}
                />
              </FormControl>
              <FormDescription>
                {isEdit && isSystemRole
                  ? "System role names cannot be changed"
                  : "Must start with a letter and contain only letters, numbers, underscores, and hyphens"}
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        {!isEdit && (
          <FormField
            control={form.control}
            name="userType"
            render={({ field }) => (
              <FormItem>
                <FormLabel>User Type *</FormLabel>
                <Select
                  onValueChange={field.onChange}
                  defaultValue={field.value}
                >
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder="Select user type" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    <SelectItem value="Admin">Admin</SelectItem>
                    <SelectItem value="Customer">Customer</SelectItem>
                  </SelectContent>
                </Select>
                <FormDescription>
                  Admin roles are for back-office users. Customer roles are for dealer/customer users.
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
        )}

        <FormField
          control={form.control}
          name="description"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Description</FormLabel>
              <FormControl>
                <Textarea
                  placeholder="Describe the purpose of this role..."
                  className="resize-none"
                  rows={3}
                  {...field}
                  value={field.value || ""}
                />
              </FormControl>
              <FormDescription>
                Optional description to help understand this role&apos;s purpose
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="flex justify-end gap-4">
          {onCancel && (
            <Button type="button" variant="outline" onClick={onCancel}>
              Cancel
            </Button>
          )}
          <Button type="submit" disabled={isLoading}>
            {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            {isEdit ? "Save Changes" : "Create Role"}
          </Button>
        </div>
      </form>
    </Form>
  );
}
