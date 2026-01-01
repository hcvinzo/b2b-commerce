"use client";

import { useEffect } from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Loader2 } from "lucide-react";

import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Checkbox } from "@/components/ui/checkbox";
import {
  useCreateCustomerUser,
  useUpdateCustomerUser,
  useAvailableCustomerRoles,
} from "@/hooks/use-customer-users";
import { CustomerUserListItem } from "@/types/entities";

const createUserSchema = z.object({
  email: z.string().email("Please enter a valid email address"),
  firstName: z.string().optional(),
  lastName: z.string().optional(),
  phoneNumber: z.string().optional(),
  roleIds: z.array(z.string()).min(1, "Please select at least one role"),
  temporaryPassword: z.string().optional(),
  sendWelcomeEmail: z.boolean().default(true),
});

const updateUserSchema = z.object({
  firstName: z.string().optional(),
  lastName: z.string().optional(),
  phoneNumber: z.string().optional(),
});

type CreateUserFormData = z.infer<typeof createUserSchema>;
type UpdateUserFormData = z.infer<typeof updateUserSchema>;

interface CustomerUserFormDialogProps {
  customerId: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  user?: CustomerUserListItem | null;
}

export function CustomerUserFormDialog({
  customerId,
  open,
  onOpenChange,
  user,
}: CustomerUserFormDialogProps) {
  const isEditing = !!user;

  const { data: availableRoles, isLoading: isLoadingRoles } =
    useAvailableCustomerRoles();
  const createUser = useCreateCustomerUser();
  const updateUser = useUpdateCustomerUser();

  const form = useForm<CreateUserFormData | UpdateUserFormData>({
    resolver: zodResolver(isEditing ? updateUserSchema : createUserSchema),
    defaultValues: isEditing
      ? {
          firstName: user?.firstName || "",
          lastName: user?.lastName || "",
          phoneNumber: "",
        }
      : {
          email: "",
          firstName: "",
          lastName: "",
          phoneNumber: "",
          roleIds: [],
          temporaryPassword: "",
          sendWelcomeEmail: true,
        },
  });

  // Reset form when dialog opens/closes or user changes
  useEffect(() => {
    if (open) {
      if (isEditing && user) {
        form.reset({
          firstName: user.firstName || "",
          lastName: user.lastName || "",
          phoneNumber: "",
        });
      } else {
        form.reset({
          email: "",
          firstName: "",
          lastName: "",
          phoneNumber: "",
          roleIds: [],
          temporaryPassword: "",
          sendWelcomeEmail: true,
        });
      }
    }
  }, [open, user, isEditing, form]);

  const onSubmit = async (data: CreateUserFormData | UpdateUserFormData) => {
    if (isEditing && user) {
      await updateUser.mutateAsync({
        customerId,
        userId: user.id,
        data: data as UpdateUserFormData,
      });
    } else {
      await createUser.mutateAsync({
        customerId,
        data: data as CreateUserFormData,
      });
    }
    onOpenChange(false);
  };

  const isPending = createUser.isPending || updateUser.isPending;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>{isEditing ? "Edit User" : "Add User"}</DialogTitle>
          <DialogDescription>
            {isEditing
              ? "Update the user's information."
              : "Create a new user for this dealer account."}
          </DialogDescription>
        </DialogHeader>

        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
            {!isEditing && (
              <FormField
                control={form.control}
                name="email"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Email *</FormLabel>
                    <FormControl>
                      <Input
                        type="email"
                        placeholder="user@example.com"
                        {...field}
                      />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            )}

            <div className="grid grid-cols-2 gap-4">
              <FormField
                control={form.control}
                name="firstName"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>First Name</FormLabel>
                    <FormControl>
                      <Input placeholder="John" {...field} />
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
                      <Input placeholder="Doe" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <FormField
              control={form.control}
              name="phoneNumber"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Phone Number</FormLabel>
                  <FormControl>
                    <Input placeholder="+90 555 555 5555" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            {!isEditing && (
              <>
                <FormField
                  control={form.control}
                  name="roleIds"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Roles *</FormLabel>
                      <FormDescription>
                        Select the roles for this user
                      </FormDescription>
                      {isLoadingRoles ? (
                        <div className="flex items-center gap-2 text-sm text-muted-foreground">
                          <Loader2 className="h-4 w-4 animate-spin" />
                          Loading roles...
                        </div>
                      ) : (
                        <div className="space-y-2">
                          {availableRoles?.map((role) => (
                            <div
                              key={role.id}
                              className="flex items-start space-x-2"
                            >
                              <Checkbox
                                id={role.id}
                                checked={field.value?.includes(role.id)}
                                onCheckedChange={(checked) => {
                                  const currentRoles = field.value || [];
                                  if (checked) {
                                    field.onChange([...currentRoles, role.id]);
                                  } else {
                                    field.onChange(
                                      currentRoles.filter(
                                        (id: string) => id !== role.id
                                      )
                                    );
                                  }
                                }}
                              />
                              <div className="grid gap-0.5 leading-none">
                                <label
                                  htmlFor={role.id}
                                  className="text-sm font-medium cursor-pointer"
                                >
                                  {role.name}
                                </label>
                                {role.description && (
                                  <p className="text-xs text-muted-foreground">
                                    {role.description}
                                  </p>
                                )}
                              </div>
                            </div>
                          ))}
                        </div>
                      )}
                      <FormMessage />
                    </FormItem>
                  )}
                />

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
                        />
                      </FormControl>
                      <FormDescription>
                        If empty, a secure password will be generated
                        automatically.
                      </FormDescription>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="sendWelcomeEmail"
                  render={({ field }) => (
                    <FormItem className="flex flex-row items-start space-x-3 space-y-0">
                      <FormControl>
                        <Checkbox
                          checked={field.value}
                          onCheckedChange={field.onChange}
                        />
                      </FormControl>
                      <div className="space-y-1 leading-none">
                        <FormLabel className="cursor-pointer">
                          Send welcome email
                        </FormLabel>
                        <FormDescription>
                          User will receive an email with login instructions.
                        </FormDescription>
                      </div>
                    </FormItem>
                  )}
                />
              </>
            )}

            <DialogFooter>
              <Button
                type="button"
                variant="outline"
                onClick={() => onOpenChange(false)}
              >
                Cancel
              </Button>
              <Button type="submit" disabled={isPending}>
                {isPending && <Loader2 className="h-4 w-4 mr-2 animate-spin" />}
                {isEditing ? "Save Changes" : "Create User"}
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
}
