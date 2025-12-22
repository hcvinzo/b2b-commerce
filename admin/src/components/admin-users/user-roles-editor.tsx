"use client";

import { useState, useEffect } from "react";
import { Loader2, Check, X } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  useUserRoles,
  useSetUserRoles,
  useAvailableRoles,
} from "@/hooks/use-admin-users";

interface UserRolesEditorProps {
  userId: string;
  onSaved?: () => void;
}

export function UserRolesEditor({ userId, onSaved }: UserRolesEditorProps) {
  const { data: currentRoles, isLoading: rolesLoading } = useUserRoles(userId);
  const { data: availableRoles, isLoading: availableLoading } =
    useAvailableRoles();
  const setUserRoles = useSetUserRoles();

  const [selectedRoles, setSelectedRoles] = useState<Set<string>>(new Set());
  const [hasChanges, setHasChanges] = useState(false);

  useEffect(() => {
    if (currentRoles) {
      setSelectedRoles(new Set(currentRoles));
      setHasChanges(false);
    }
  }, [currentRoles]);

  const toggleRole = (roleName: string) => {
    const newRoles = new Set(selectedRoles);
    if (newRoles.has(roleName)) {
      newRoles.delete(roleName);
    } else {
      newRoles.add(roleName);
    }
    setSelectedRoles(newRoles);
    setHasChanges(true);
  };

  const handleSave = async () => {
    await setUserRoles.mutateAsync({
      id: userId,
      data: { roles: Array.from(selectedRoles) },
    });
    setHasChanges(false);
    onSaved?.();
  };

  const handleReset = () => {
    if (currentRoles) {
      setSelectedRoles(new Set(currentRoles));
      setHasChanges(false);
    }
  };

  if (rolesLoading || availableLoading) {
    return (
      <div className="flex items-center justify-center py-8">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Roles</CardTitle>
        <CardDescription>
          Select the roles to assign to this user
        </CardDescription>
      </CardHeader>
      <CardContent className="space-y-4">
        {hasChanges && (
          <div className="flex items-center justify-between rounded-lg border border-yellow-200 bg-yellow-50 p-4 dark:border-yellow-900 dark:bg-yellow-950">
            <p className="text-sm text-yellow-800 dark:text-yellow-200">
              You have unsaved changes
            </p>
            <div className="flex gap-2">
              <Button variant="outline" size="sm" onClick={handleReset}>
                <X className="mr-2 h-4 w-4" />
                Reset
              </Button>
              <Button
                size="sm"
                onClick={handleSave}
                disabled={setUserRoles.isPending}
              >
                {setUserRoles.isPending && (
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                )}
                <Check className="mr-2 h-4 w-4" />
                Save Changes
              </Button>
            </div>
          </div>
        )}

        <div className="space-y-3">
          {availableRoles?.map((role) => (
            <div key={role.name} className="flex items-start gap-3">
              <Checkbox
                id={role.name}
                checked={selectedRoles.has(role.name)}
                onCheckedChange={() => toggleRole(role.name)}
              />
              <div className="grid gap-0.5 leading-none">
                <label
                  htmlFor={role.name}
                  className="text-sm font-medium leading-none cursor-pointer"
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
          {(!availableRoles || availableRoles.length === 0) && (
            <p className="text-sm text-muted-foreground">No roles available</p>
          )}
        </div>
      </CardContent>
    </Card>
  );
}
