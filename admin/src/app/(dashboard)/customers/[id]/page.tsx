"use client";

import { useState } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { ArrowLeft, Building2, Mail, Phone, Globe, CreditCard, MapPin } from "lucide-react";

import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Skeleton } from "@/components/ui/skeleton";
import { Separator } from "@/components/ui/separator";
import { Input } from "@/components/ui/input";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  useCustomer,
  useApproveCustomer,
  useActivateCustomer,
  useDeactivateCustomer,
  useUpdateCreditLimit,
} from "@/hooks/use-customers";
import { formatCurrency, formatDateTime } from "@/lib/utils";

export default function CustomerDetailPage() {
  const params = useParams();
  const id = params.id as string;

  const [creditDialogOpen, setCreditDialogOpen] = useState(false);
  const [newCreditLimit, setNewCreditLimit] = useState("");

  const { data: customer, isLoading } = useCustomer(id);
  const approveCustomer = useApproveCustomer();
  const activateCustomer = useActivateCustomer();
  const deactivateCustomer = useDeactivateCustomer();
  const updateCreditLimit = useUpdateCreditLimit();

  const handleCreditUpdate = async () => {
    const limit = parseFloat(newCreditLimit);
    if (!isNaN(limit) && limit >= 0) {
      await updateCreditLimit.mutateAsync({ id, creditLimit: limit });
      setCreditDialogOpen(false);
      setNewCreditLimit("");
    }
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-10 w-48" />
        <Skeleton className="h-[400px]" />
      </div>
    );
  }

  if (!customer) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" asChild>
            <Link href="/customers">
              <ArrowLeft className="h-4 w-4" />
            </Link>
          </Button>
          <h1 className="text-3xl font-bold tracking-tight">Customer not found</h1>
        </div>
      </div>
    );
  }

  const creditUsagePercent =
    customer.creditLimit > 0
      ? ((customer.usedCredit / customer.creditLimit) * 100).toFixed(1)
      : 0;

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" asChild>
            <Link href="/customers">
              <ArrowLeft className="h-4 w-4" />
            </Link>
          </Button>
          <div>
            <h1 className="text-3xl font-bold tracking-tight">
              {customer.companyName}
            </h1>
            {customer.tradeName && (
              <p className="text-muted-foreground">{customer.tradeName}</p>
            )}
          </div>
        </div>
        <div className="flex gap-2">
          {!customer.isApproved && (
            <Button
              onClick={() => approveCustomer.mutate(id)}
              disabled={approveCustomer.isPending}
            >
              Approve Customer
            </Button>
          )}
          <Button
            variant={customer.isActive ? "destructive" : "default"}
            onClick={() =>
              customer.isActive
                ? deactivateCustomer.mutate(id)
                : activateCustomer.mutate(id)
            }
            disabled={activateCustomer.isPending || deactivateCustomer.isPending}
          >
            {customer.isActive ? "Deactivate" : "Activate"}
          </Button>
        </div>
      </div>

      <div className="grid gap-6 md:grid-cols-3">
        {/* Company Info */}
        <Card className="md:col-span-2">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Building2 className="h-5 w-5" />
              Company Information
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid gap-4 md:grid-cols-2">
              <div>
                <p className="text-sm text-muted-foreground">Company Name</p>
                <p className="font-medium">{customer.companyName}</p>
              </div>
              {customer.tradeName && (
                <div>
                  <p className="text-sm text-muted-foreground">Trade Name</p>
                  <p className="font-medium">{customer.tradeName}</p>
                </div>
              )}
              <div>
                <p className="text-sm text-muted-foreground">Tax Number</p>
                <p className="font-medium">{customer.taxNumber}</p>
              </div>
              {customer.taxOffice && (
                <div>
                  <p className="text-sm text-muted-foreground">Tax Office</p>
                  <p className="font-medium">{customer.taxOffice}</p>
                </div>
              )}
            </div>

            <Separator />

            <div className="grid gap-4 md:grid-cols-2">
              <div className="flex items-center gap-2">
                <Mail className="h-4 w-4 text-muted-foreground" />
                <span>{customer.email}</span>
              </div>
              {customer.phone && (
                <div className="flex items-center gap-2">
                  <Phone className="h-4 w-4 text-muted-foreground" />
                  <span>{customer.phone}</span>
                </div>
              )}
              {customer.website && (
                <div className="flex items-center gap-2">
                  <Globe className="h-4 w-4 text-muted-foreground" />
                  <a
                    href={customer.website}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="text-primary hover:underline"
                  >
                    {customer.website}
                  </a>
                </div>
              )}
            </div>

            <Separator />

            <div className="grid gap-4 md:grid-cols-3">
              <div>
                <p className="text-sm text-muted-foreground">Status</p>
                <Badge variant={customer.isActive ? "default" : "secondary"}>
                  {customer.isActive ? "Active" : "Inactive"}
                </Badge>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Approval</p>
                <Badge
                  variant={customer.isApproved ? "default" : "outline"}
                  className={
                    customer.isApproved
                      ? "bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300"
                      : ""
                  }
                >
                  {customer.isApproved ? "Approved" : "Pending"}
                </Badge>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Price Tier</p>
                <Badge variant="outline">Tier {customer.priceTier}</Badge>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Credit Info */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <CreditCard className="h-5 w-5" />
              Credit
            </CardTitle>
            <CardDescription>
              Payment terms: {customer.paymentTermDays} days
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <p className="text-sm text-muted-foreground">Credit Limit</p>
              <p className="text-2xl font-bold">
                {formatCurrency(customer.creditLimit)}
              </p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Used Credit</p>
              <p className="text-xl font-semibold text-orange-600">
                {formatCurrency(customer.usedCredit)}
              </p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Available Credit</p>
              <p className="text-xl font-semibold text-green-600">
                {formatCurrency(customer.availableCredit)}
              </p>
            </div>

            {/* Credit Usage Bar */}
            <div className="space-y-1">
              <div className="flex justify-between text-xs">
                <span>Credit Usage</span>
                <span>{creditUsagePercent}%</span>
              </div>
              <div className="h-2 bg-muted rounded-full overflow-hidden">
                <div
                  className="h-full bg-primary transition-all"
                  style={{ width: `${Math.min(Number(creditUsagePercent), 100)}%` }}
                />
              </div>
            </div>

            {customer.discountRate > 0 && (
              <div>
                <p className="text-sm text-muted-foreground">Discount Rate</p>
                <p className="font-medium">{customer.discountRate}%</p>
              </div>
            )}

            <Button
              className="w-full"
              variant="outline"
              onClick={() => {
                setNewCreditLimit(customer.creditLimit.toString());
                setCreditDialogOpen(true);
              }}
            >
              Update Credit Limit
            </Button>
          </CardContent>
        </Card>

        {/* Addresses */}
        {customer.addresses.length > 0 && (
          <Card className="md:col-span-2">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <MapPin className="h-5 w-5" />
                Addresses
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid gap-4 md:grid-cols-2">
                {customer.addresses.map((address) => (
                  <div
                    key={address.id}
                    className="p-4 border rounded-lg space-y-2"
                  >
                    <div className="flex items-center justify-between">
                      <p className="font-medium">{address.title}</p>
                      <div className="flex gap-1">
                        {address.isDefault && (
                          <Badge variant="secondary" className="text-xs">
                            Default
                          </Badge>
                        )}
                        {address.isBilling && (
                          <Badge variant="outline" className="text-xs">
                            Billing
                          </Badge>
                        )}
                        {address.isShipping && (
                          <Badge variant="outline" className="text-xs">
                            Shipping
                          </Badge>
                        )}
                      </div>
                    </div>
                    <div className="text-sm text-muted-foreground">
                      <p>{address.addressLine1}</p>
                      {address.addressLine2 && <p>{address.addressLine2}</p>}
                      <p>
                        {address.district && `${address.district}, `}
                        {address.city} {address.postalCode}
                      </p>
                      <p>{address.country}</p>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        )}

        {/* Metadata */}
        <Card>
          <CardHeader>
            <CardTitle>Account Details</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2 text-sm">
            <div className="flex justify-between">
              <span className="text-muted-foreground">Created</span>
              <span>{formatDateTime(customer.createdAt)}</span>
            </div>
            {customer.approvedAt && (
              <div className="flex justify-between">
                <span className="text-muted-foreground">Approved</span>
                <span>{formatDateTime(customer.approvedAt)}</span>
              </div>
            )}
            {customer.updatedAt && (
              <div className="flex justify-between">
                <span className="text-muted-foreground">Updated</span>
                <span>{formatDateTime(customer.updatedAt)}</span>
              </div>
            )}
            {customer.externalId && (
              <div className="flex justify-between">
                <span className="text-muted-foreground">External ID</span>
                <span className="font-mono text-xs">{customer.externalId}</span>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* Credit Limit Dialog */}
      <Dialog open={creditDialogOpen} onOpenChange={setCreditDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Update Credit Limit</DialogTitle>
            <DialogDescription>
              Set a new credit limit for {customer.companyName}
            </DialogDescription>
          </DialogHeader>
          <div className="py-4">
            <Input
              type="number"
              placeholder="Enter new credit limit"
              value={newCreditLimit}
              onChange={(e) => setNewCreditLimit(e.target.value)}
            />
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setCreditDialogOpen(false)}>
              Cancel
            </Button>
            <Button
              onClick={handleCreditUpdate}
              disabled={updateCreditLimit.isPending}
            >
              Update
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
