"use client";

import { useParams } from "next/navigation";
import Link from "next/link";
import { ArrowLeft, Building2, Mail, Phone, MapPin, User, Globe, Calendar } from "lucide-react";

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
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  useCustomer,
  useApproveCustomer,
  useActivateCustomer,
  useDeactivateCustomer,
} from "@/hooks/use-customers";
import { formatDateTime } from "@/lib/utils";
import { CustomerAttributesEditor } from "@/components/customers/customer-attributes-editor";
import { CustomerDocumentsEditor } from "@/components/customers/customer-documents-editor";
import { Customer, CustomerContact } from "@/types/entities";

// Helper function to get primary active contact
function getPrimaryContact(customer: Customer): CustomerContact | null {
  if (!customer.contacts || customer.contacts.length === 0) return null;
  return (
    customer.contacts.find((c) => c.isPrimary && c.isActive) ||
    customer.contacts.find((c) => c.isActive) ||
    customer.contacts[0]
  );
}

export default function CustomerDetailPage() {
  const params = useParams();
  const id = params.id as string;

  const { data: customer, isLoading } = useCustomer(id);
  const approveCustomer = useApproveCustomer();
  const activateCustomer = useActivateCustomer();
  const deactivateCustomer = useDeactivateCustomer();

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

  const primaryContact = getPrimaryContact(customer);

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
              {customer.title}
            </h1>
            <p className="text-muted-foreground">
              {primaryContact?.fullName && `${primaryContact.fullName}`}
            </p>
          </div>
        </div>
        <div className="flex gap-2">
          {customer.status === "Pending" && (
            <Button
              onClick={() => approveCustomer.mutate(id)}
              disabled={approveCustomer.isPending}
            >
              Approve Customer
            </Button>
          )}
          {customer.status === "Active" && (
            <Button
              variant="destructive"
              onClick={() => deactivateCustomer.mutate(id)}
              disabled={deactivateCustomer.isPending}
            >
              Suspend
            </Button>
          )}
          {customer.status === "Suspended" && (
            <Button
              onClick={() => activateCustomer.mutate(id)}
              disabled={activateCustomer.isPending}
            >
              Activate
            </Button>
          )}
        </div>
      </div>

      <Tabs defaultValue="overview" className="space-y-6">
        <TabsList>
          <TabsTrigger value="overview">Overview</TabsTrigger>
          <TabsTrigger value="contacts">Contacts</TabsTrigger>
          <TabsTrigger value="addresses">Addresses</TabsTrigger>
          <TabsTrigger value="documents">Documents</TabsTrigger>
          <TabsTrigger value="attributes">Attributes</TabsTrigger>
        </TabsList>

        <TabsContent value="overview">
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
                    <p className="text-sm text-muted-foreground">Title</p>
                    <p className="font-medium">{customer.title}</p>
                  </div>
                  {customer.taxNo && (
                    <div>
                      <p className="text-sm text-muted-foreground">Tax Number</p>
                      <p className="font-medium">{customer.taxNo}</p>
                    </div>
                  )}
                  {customer.taxOffice && (
                    <div>
                      <p className="text-sm text-muted-foreground">Tax Office</p>
                      <p className="font-medium">{customer.taxOffice}</p>
                    </div>
                  )}
                  {customer.establishmentYear && (
                    <div>
                      <p className="text-sm text-muted-foreground">Establishment Year</p>
                      <p className="font-medium">{customer.establishmentYear}</p>
                    </div>
                  )}
                  {customer.website && (
                    <div>
                      <p className="text-sm text-muted-foreground">Website</p>
                      <a
                        href={customer.website.startsWith("http") ? customer.website : `https://${customer.website}`}
                        target="_blank"
                        rel="noopener noreferrer"
                        className="font-medium text-primary hover:underline flex items-center gap-1"
                      >
                        <Globe className="h-4 w-4" />
                        {customer.website}
                      </a>
                    </div>
                  )}
                </div>

                <Separator />

                <div className="grid gap-4 md:grid-cols-2">
                  <div>
                    <p className="text-sm text-muted-foreground">Status</p>
                    <Badge
                      variant={customer.status === "Active" ? "default" : customer.status === "Rejected" ? "destructive" : "secondary"}
                      className={
                        customer.status === "Pending"
                          ? "border-yellow-500 text-yellow-600"
                          : undefined
                      }
                    >
                      {customer.status}
                    </Badge>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Primary Contact */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <User className="h-5 w-5" />
                  Primary Contact
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                {primaryContact ? (
                  <>
                    <div>
                      <p className="text-sm text-muted-foreground">Name</p>
                      <p className="font-medium">{primaryContact.fullName}</p>
                    </div>
                    {primaryContact.position && (
                      <div>
                        <p className="text-sm text-muted-foreground">Position</p>
                        <p className="font-medium">{primaryContact.position}</p>
                      </div>
                    )}

                    <Separator />

                    <div className="space-y-2">
                      {primaryContact.email && (
                        <div className="flex items-center gap-2">
                          <Mail className="h-4 w-4 text-muted-foreground" />
                          <span className="text-sm">{primaryContact.email}</span>
                        </div>
                      )}
                      {primaryContact.phone && (
                        <div className="flex items-center gap-2">
                          <Phone className="h-4 w-4 text-muted-foreground" />
                          <span className="text-sm">
                            {primaryContact.phone}
                            {primaryContact.phoneExt && ` ext. ${primaryContact.phoneExt}`}
                          </span>
                        </div>
                      )}
                      {primaryContact.gsm && (
                        <div className="flex items-center gap-2">
                          <Phone className="h-4 w-4 text-muted-foreground" />
                          <span className="text-sm">{primaryContact.gsm} (Mobile)</span>
                        </div>
                      )}
                      {primaryContact.dateOfBirth && (
                        <div className="flex items-center gap-2">
                          <Calendar className="h-4 w-4 text-muted-foreground" />
                          <span className="text-sm">{primaryContact.dateOfBirth}</span>
                        </div>
                      )}
                    </div>
                  </>
                ) : (
                  <p className="text-sm text-muted-foreground">No contact information</p>
                )}
              </CardContent>
            </Card>

            {/* Account Details */}
            <Card className="md:col-span-3">
              <CardHeader>
                <CardTitle>Account Details</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2 text-sm">
                <div className="grid gap-4 md:grid-cols-3">
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">Created</span>
                    <span>{formatDateTime(customer.createdAt)}</span>
                  </div>
                  {customer.updatedAt && (
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Updated</span>
                      <span>{formatDateTime(customer.updatedAt)}</span>
                    </div>
                  )}
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="contacts">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <User className="h-5 w-5" />
                Contacts
              </CardTitle>
              <CardDescription>
                All contacts for this customer
              </CardDescription>
            </CardHeader>
            <CardContent>
              {customer.contacts && customer.contacts.length > 0 ? (
                <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
                  {customer.contacts.map((contact) => (
                    <div key={contact.id} className="p-4 border rounded-lg space-y-2">
                      <div className="flex items-center justify-between">
                        <p className="font-medium">{contact.fullName}</p>
                        <div className="flex gap-1">
                          {contact.isPrimary && (
                            <Badge variant="default" className="text-xs">
                              Primary
                            </Badge>
                          )}
                          {!contact.isActive && (
                            <Badge variant="secondary" className="text-xs">
                              Inactive
                            </Badge>
                          )}
                        </div>
                      </div>
                      {contact.position && (
                        <p className="text-sm text-muted-foreground">{contact.position}</p>
                      )}
                      <div className="space-y-1 text-sm">
                        {contact.email && (
                          <div className="flex items-center gap-2">
                            <Mail className="h-3 w-3 text-muted-foreground" />
                            <span>{contact.email}</span>
                          </div>
                        )}
                        {contact.phone && (
                          <div className="flex items-center gap-2">
                            <Phone className="h-3 w-3 text-muted-foreground" />
                            <span>
                              {contact.phone}
                              {contact.phoneExt && ` ext. ${contact.phoneExt}`}
                            </span>
                          </div>
                        )}
                        {contact.gsm && (
                          <div className="flex items-center gap-2">
                            <Phone className="h-3 w-3 text-muted-foreground" />
                            <span>{contact.gsm}</span>
                          </div>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-sm text-muted-foreground">No contacts on file</p>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="addresses">
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <MapPin className="h-5 w-5" />
                Addresses
              </CardTitle>
              <CardDescription>
                Billing, shipping, and contact addresses
              </CardDescription>
            </CardHeader>
            <CardContent>
              {customer.addresses && customer.addresses.length > 0 ? (
                <div className="grid gap-4 md:grid-cols-2">
                  {customer.addresses.map((address) => (
                    <div key={address.id} className="p-4 border rounded-lg space-y-2">
                      <div className="flex items-center justify-between">
                        <p className="font-medium">{address.title}</p>
                        <div className="flex gap-1">
                          <Badge variant="outline" className="text-xs">
                            {address.addressType}
                          </Badge>
                          {address.isDefault && (
                            <Badge variant="secondary" className="text-xs">
                              Default
                            </Badge>
                          )}
                        </div>
                      </div>
                      {address.fullName && (
                        <p className="text-sm font-medium">{address.fullName}</p>
                      )}
                      <div className="text-sm text-muted-foreground">
                        <p>{address.address}</p>
                        {address.geoLocationPathName && (
                          <p>{address.geoLocationPathName}</p>
                        )}
                        {address.postalCode && (
                          <p>Postal Code: {address.postalCode}</p>
                        )}
                      </div>
                      <div className="space-y-1 text-sm">
                        {address.phone && (
                          <div className="flex items-center gap-2">
                            <Phone className="h-3 w-3 text-muted-foreground" />
                            <span>
                              {address.phone}
                              {address.phoneExt && ` ext. ${address.phoneExt}`}
                            </span>
                          </div>
                        )}
                        {address.gsm && (
                          <div className="flex items-center gap-2">
                            <Phone className="h-3 w-3 text-muted-foreground" />
                            <span>{address.gsm}</span>
                          </div>
                        )}
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-sm text-muted-foreground">No addresses on file</p>
              )}
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="documents">
          <CustomerDocumentsEditor customerId={id} documentUrls={customer.documentUrls} />
        </TabsContent>

        <TabsContent value="attributes">
          <CustomerAttributesEditor customerId={id} />
        </TabsContent>
      </Tabs>
    </div>
  );
}
