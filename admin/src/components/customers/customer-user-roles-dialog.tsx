"use client";

import { useState, useEffect } from "react";
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
import { Checkbox } from "@/components/ui/checkbox";
import { Label } from "@/components/ui/label";
import {
  useAvailableCustomerRoles,
  useSetCustomerUserRoles,
} from "@/hooks/use-customer-users";
import { CustomerUserListItem } from "@/types/entities";

interface CustomerUserRolesDialogProps {
  customerId: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
  user: CustomerUserListItem | null;
}

export function CustomerUserRolesDialog({
  customerId,
  open,
  onOpenChange,
  user,
}: CustomerUserRolesDialogProps) {
  const [selectedRoleIds, setSelectedRoleIds] = useState<string[]>([]);

  const { data: availableRoles, isLoading: isLoadingRoles } =
    useAvailableCustomerRoles();
  const setUserRoles = useSetCustomerUserRoles();

  // Initialize selected roles when dialog opens
  useEffect(() => {
    if (open && user) {
      setSelectedRoleIds(user.roles.map((r) => r.id));
    }
  }, [open, user]);

  const handleToggleRole = (roleId: string) => {
    setSelectedRoleIds((prev) =>
      prev.includes(roleId)
        ? prev.filter((id) => id !== roleId)
        : [...prev, roleId]
    );
  };

  const handleSave = async () => {
    if (!user) return;

    await setUserRoles.mutateAsync({
      customerId,
      userId: user.id,
      data: { roleIds: selectedRoleIds },
    });
    onOpenChange(false);
  };

  if (!user) return null;

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[450px]">
        <DialogHeader>
          <DialogTitle>Manage Roles</DialogTitle>
          <DialogDescription>
            Select the roles for {user.fullName || user.email}
          </DialogDescription>
        </DialogHeader>

        <div className="py-4">
          {isLoadingRoles ? (
            <div className="flex items-center justify-center gap-2 py-8 text-muted-foreground">
              <Loader2 className="h-5 w-5 animate-spin" />
              Loading roles...
            </div>
          ) : (
            <div className="space-y-4">
              {availableRoles?.map((role) => (
                <div key={role.id} className="flex items-start space-x-3">
                  <Checkbox
                    id={`role-${role.id}`}
                    checked={selectedRoleIds.includes(role.id)}
                    onCheckedChange={() => handleToggleRole(role.id)}
                  />
                  <div className="grid gap-0.5 leading-none">
                    <Label
                      htmlFor={`role-${role.id}`}
                      className="cursor-pointer font-medium"
                    >
                      {role.name}
                    </Label>
                    {role.description && (
                      <p className="text-sm text-muted-foreground">
                        {role.description}
                      </p>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Cancel
          </Button>
          <Button
            onClick={handleSave}
            disabled={setUserRoles.isPending || selectedRoleIds.length === 0}
          >
            {setUserRoles.isPending && (
              <Loader2 className="h-4 w-4 mr-2 animate-spin" />
            )}
            Save Roles
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}
