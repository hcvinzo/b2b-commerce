"use client";

import { useState } from "react";
import { Loader2, Plus, Trash2, Key } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import {
  useUserClaims,
  useAddUserClaim,
  useRemoveUserClaim,
} from "@/hooks/use-admin-users";

interface UserClaimsEditorProps {
  userId: string;
}

export function UserClaimsEditor({ userId }: UserClaimsEditorProps) {
  const { data: claims, isLoading } = useUserClaims(userId);
  const addClaim = useAddUserClaim();
  const removeClaim = useRemoveUserClaim();

  const [isAddOpen, setIsAddOpen] = useState(false);
  const [deletingClaimId, setDeletingClaimId] = useState<number | null>(null);
  const [newClaimType, setNewClaimType] = useState("");
  const [newClaimValue, setNewClaimValue] = useState("");

  const handleAddClaim = async () => {
    if (!newClaimType.trim() || !newClaimValue.trim()) return;

    await addClaim.mutateAsync({
      id: userId,
      data: {
        type: newClaimType.trim(),
        value: newClaimValue.trim(),
      },
    });
    setIsAddOpen(false);
    setNewClaimType("");
    setNewClaimValue("");
  };

  const handleRemoveClaim = async () => {
    if (deletingClaimId === null) return;

    await removeClaim.mutateAsync({
      userId,
      claimId: deletingClaimId,
    });
    setDeletingClaimId(null);
  };

  const deletingClaim = claims?.find((c) => c.id === deletingClaimId);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-8">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  return (
    <>
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle>User Claims</CardTitle>
              <CardDescription>
                Direct claims assigned to this user
              </CardDescription>
            </div>
            <Button size="sm" onClick={() => setIsAddOpen(true)}>
              <Plus className="mr-2 h-4 w-4" />
              Add Claim
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          {claims && claims.length > 0 ? (
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>Type</TableHead>
                    <TableHead>Value</TableHead>
                    <TableHead className="w-[80px]"></TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {claims.map((claim) => (
                    <TableRow key={claim.id}>
                      <TableCell className="font-mono text-sm">
                        {claim.type}
                      </TableCell>
                      <TableCell className="font-mono text-sm">
                        {claim.value}
                      </TableCell>
                      <TableCell>
                        <Button
                          variant="ghost"
                          size="icon"
                          onClick={() => setDeletingClaimId(claim.id)}
                        >
                          <Trash2 className="h-4 w-4 text-destructive" />
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          ) : (
            <div className="text-center py-8">
              <Key className="mx-auto h-12 w-12 text-muted-foreground" />
              <h3 className="mt-4 text-lg font-semibold">No claims</h3>
              <p className="text-muted-foreground">
                This user has no direct claims assigned.
              </p>
              <Button className="mt-4" onClick={() => setIsAddOpen(true)}>
                <Plus className="mr-2 h-4 w-4" />
                Add Claim
              </Button>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Add Claim Dialog */}
      <Dialog open={isAddOpen} onOpenChange={setIsAddOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Add Claim</DialogTitle>
            <DialogDescription>
              Add a new claim to this user. Claims are key-value pairs that
              represent user attributes or permissions.
            </DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="grid gap-2">
              <Label htmlFor="claimType">Claim Type</Label>
              <Input
                id="claimType"
                placeholder="e.g., permission, department"
                value={newClaimType}
                onChange={(e) => setNewClaimType(e.target.value)}
              />
            </div>
            <div className="grid gap-2">
              <Label htmlFor="claimValue">Claim Value</Label>
              <Input
                id="claimValue"
                placeholder="e.g., products:write, Sales"
                value={newClaimValue}
                onChange={(e) => setNewClaimValue(e.target.value)}
              />
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setIsAddOpen(false)}>
              Cancel
            </Button>
            <Button
              onClick={handleAddClaim}
              disabled={
                addClaim.isPending ||
                !newClaimType.trim() ||
                !newClaimValue.trim()
              }
            >
              {addClaim.isPending && (
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              )}
              Add Claim
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Remove Claim Confirmation */}
      <ConfirmDialog
        open={deletingClaimId !== null}
        onOpenChange={(open) => !open && setDeletingClaimId(null)}
        title="Remove Claim"
        description={`Are you sure you want to remove the claim "${deletingClaim?.type}: ${deletingClaim?.value}"?`}
        confirmText="Remove"
        variant="destructive"
        onConfirm={handleRemoveClaim}
        isLoading={removeClaim.isPending}
      />
    </>
  );
}
