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
import { useAvailablePermissions, useSetRoleClaims } from "@/hooks/use-roles";
import { type PermissionCategory } from "@/types/entities";

interface PermissionsEditorProps {
  roleId: string;
  currentClaims: string[];
  isProtected?: boolean;
  onSaved?: () => void;
}

export function PermissionsEditor({
  roleId,
  currentClaims,
  isProtected = false,
  onSaved,
}: PermissionsEditorProps) {
  const { data: categories, isLoading: categoriesLoading } =
    useAvailablePermissions();
  const setRoleClaims = useSetRoleClaims();

  const [selectedClaims, setSelectedClaims] = useState<Set<string>>(
    new Set(currentClaims)
  );
  const [hasChanges, setHasChanges] = useState(false);

  useEffect(() => {
    setSelectedClaims(new Set(currentClaims));
    setHasChanges(false);
  }, [currentClaims]);

  const toggleClaim = (claimValue: string) => {
    const newClaims = new Set(selectedClaims);
    if (newClaims.has(claimValue)) {
      newClaims.delete(claimValue);
    } else {
      newClaims.add(claimValue);
    }
    setSelectedClaims(newClaims);
    setHasChanges(true);
  };

  const selectAllInCategory = (category: PermissionCategory) => {
    const newClaims = new Set(selectedClaims);
    category.permissions.forEach((p) => newClaims.add(p.value));
    setSelectedClaims(newClaims);
    setHasChanges(true);
  };

  const deselectAllInCategory = (category: PermissionCategory) => {
    const newClaims = new Set(selectedClaims);
    category.permissions.forEach((p) => newClaims.delete(p.value));
    setSelectedClaims(newClaims);
    setHasChanges(true);
  };

  const isCategoryFullySelected = (category: PermissionCategory) => {
    return category.permissions.every((p) => selectedClaims.has(p.value));
  };

  const isCategoryPartiallySelected = (category: PermissionCategory) => {
    const selected = category.permissions.filter((p) =>
      selectedClaims.has(p.value)
    );
    return selected.length > 0 && selected.length < category.permissions.length;
  };

  const handleSave = async () => {
    await setRoleClaims.mutateAsync({
      roleId,
      data: { claims: Array.from(selectedClaims) },
    });
    setHasChanges(false);
    onSaved?.();
  };

  const handleReset = () => {
    setSelectedClaims(new Set(currentClaims));
    setHasChanges(false);
  };

  if (categoriesLoading) {
    return (
      <div className="flex items-center justify-center py-8">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  if (isProtected) {
    return (
      <Card>
        <CardHeader>
          <CardTitle>Permissions</CardTitle>
          <CardDescription>
            This is a protected role with full access to all permissions.
            Permissions cannot be modified.
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <Check className="h-4 w-4 text-green-500" />
            <span>Full access to all permissions (*)</span>
          </div>
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="space-y-4">
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
              disabled={setRoleClaims.isPending}
            >
              {setRoleClaims.isPending && (
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              )}
              <Check className="mr-2 h-4 w-4" />
              Save Changes
            </Button>
          </div>
        </div>
      )}

      <div className="grid gap-4 md:grid-cols-2">
        {categories?.map((category) => (
          <Card key={category.name}>
            <CardHeader className="pb-3">
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle className="text-base">{category.name}</CardTitle>
                  {category.description && (
                    <CardDescription className="text-xs">
                      {category.description}
                    </CardDescription>
                  )}
                </div>
                <div className="flex gap-1">
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-7 text-xs"
                    onClick={() => selectAllInCategory(category)}
                    disabled={isCategoryFullySelected(category)}
                  >
                    All
                  </Button>
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-7 text-xs"
                    onClick={() => deselectAllInCategory(category)}
                    disabled={
                      !isCategoryFullySelected(category) &&
                      !isCategoryPartiallySelected(category)
                    }
                  >
                    None
                  </Button>
                </div>
              </div>
            </CardHeader>
            <CardContent className="pt-0">
              <div className="space-y-2">
                {category.permissions.map((permission) => (
                  <div
                    key={permission.value}
                    className="flex items-start gap-3"
                  >
                    <Checkbox
                      id={permission.value}
                      checked={selectedClaims.has(permission.value)}
                      onCheckedChange={() => toggleClaim(permission.value)}
                    />
                    <div className="grid gap-0.5 leading-none">
                      <label
                        htmlFor={permission.value}
                        className="text-sm font-medium leading-none cursor-pointer"
                      >
                        {permission.displayName}
                      </label>
                      {permission.description && (
                        <p className="text-xs text-muted-foreground">
                          {permission.description}
                        </p>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
}
