"use client";

import { Loader2, Globe } from "lucide-react";

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
import { useUserLogins } from "@/hooks/use-admin-users";

interface UserLoginHistoryProps {
  userId: string;
}

export function UserLoginHistory({ userId }: UserLoginHistoryProps) {
  const { data: logins, isLoading } = useUserLogins(userId);

  if (isLoading) {
    return (
      <div className="flex items-center justify-center py-8">
        <Loader2 className="h-8 w-8 animate-spin text-muted-foreground" />
      </div>
    );
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>External Login Providers</CardTitle>
        <CardDescription>
          External authentication providers linked to this account
        </CardDescription>
      </CardHeader>
      <CardContent>
        {logins && logins.length > 0 ? (
          <div className="rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Provider</TableHead>
                  <TableHead>Display Name</TableHead>
                  <TableHead>Provider Key</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {logins.map((login) => (
                  <TableRow key={login.id}>
                    <TableCell className="font-medium">
                      {login.loginProvider}
                    </TableCell>
                    <TableCell>{login.providerDisplayName}</TableCell>
                    <TableCell className="font-mono text-sm text-muted-foreground">
                      {login.providerKey || "-"}
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        ) : (
          <div className="text-center py-8">
            <Globe className="mx-auto h-12 w-12 text-muted-foreground" />
            <h3 className="mt-4 text-lg font-semibold">No external logins</h3>
            <p className="text-muted-foreground">
              This user has no external authentication providers linked.
            </p>
          </div>
        )}
      </CardContent>
    </Card>
  );
}
