"use client";

import { useState } from "react";
import { useParams } from "next/navigation";
import Link from "next/link";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import {
  ArrowLeft,
  Key,
  Mail,
  Phone,
  Plus,
  MoreHorizontal,
  Pencil,
  RotateCw,
  Ban,
  Clock,
  AlertTriangle,
  Loader2,
  ChevronDown,
  ChevronRight,
  Globe,
  Trash2,
  Shield,
} from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
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
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
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
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import { ConfirmDialog } from "@/components/shared/confirm-dialog";
import { ApiClientForm } from "@/components/forms/api-client-form";
import { ApiKeyForm } from "@/components/forms/api-key-form";
import { ApiKeyCreatedDialog } from "@/components/shared/api-key-created-dialog";
import {
  useApiClient,
  useUpdateApiClient,
  useActivateApiClient,
  useDeactivateApiClient,
} from "@/hooks/use-api-clients";
import {
  useCreateApiKey,
  useRevokeApiKey,
  useRotateApiKey,
  useApiKeyDetail,
  useAddIpWhitelist,
  useRemoveIpWhitelist,
} from "@/hooks/use-api-keys";
import { formatDateTime } from "@/lib/utils";
import {
  type ApiClientFormData,
  type ApiKeyFormData,
  revokeKeySchema,
  type RevokeKeyFormData,
  ipWhitelistSchema,
  type IpWhitelistFormData,
} from "@/lib/validations/api-client";
import { type CreateApiKeyResponse, type ApiKeyListItem, type IpWhitelistEntry } from "@/types/entities";

function RevokeKeyForm({
  onSubmit,
  onCancel,
  isLoading,
}: {
  onSubmit: (data: RevokeKeyFormData) => Promise<void>;
  onCancel: () => void;
  isLoading: boolean;
}) {
  const form = useForm<RevokeKeyFormData>({
    resolver: zodResolver(revokeKeySchema),
    defaultValues: { reason: "" },
  });

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="reason"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Revocation Reason *</FormLabel>
              <FormControl>
                <Input placeholder="Enter reason for revoking this key" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <div className="flex justify-end gap-4">
          <Button type="button" variant="outline" onClick={onCancel}>
            Cancel
          </Button>
          <Button type="submit" variant="destructive" disabled={isLoading}>
            {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Revoke Key
          </Button>
        </div>
      </form>
    </Form>
  );
}

function AddIpWhitelistForm({
  onSubmit,
  onCancel,
  isLoading,
}: {
  onSubmit: (data: IpWhitelistFormData) => Promise<void>;
  onCancel: () => void;
  isLoading: boolean;
}) {
  const form = useForm<IpWhitelistFormData>({
    resolver: zodResolver(ipWhitelistSchema),
    defaultValues: { ipAddress: "", description: "" },
  });

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
        <FormField
          control={form.control}
          name="ipAddress"
          render={({ field }) => (
            <FormItem>
              <FormLabel>IP Address *</FormLabel>
              <FormControl>
                <Input placeholder="e.g., 192.168.1.1 or 192.168.1.0/24" {...field} />
              </FormControl>
              <FormDescription>
                Enter an IPv4 address or CIDR notation for a range
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="description"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Description</FormLabel>
              <FormControl>
                <Input placeholder="e.g., Office network" {...field} />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />
        <div className="flex justify-end gap-4">
          <Button type="button" variant="outline" onClick={onCancel}>
            Cancel
          </Button>
          <Button type="submit" disabled={isLoading}>
            {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Add IP
          </Button>
        </div>
      </form>
    </Form>
  );
}

function ApiKeyRow({
  apiKey,
  onRotate,
  onRevoke,
}: {
  apiKey: ApiKeyListItem;
  onRotate: (key: ApiKeyListItem) => void;
  onRevoke: (key: ApiKeyListItem) => void;
}) {
  const [isOpen, setIsOpen] = useState(false);
  const [isAddIpOpen, setIsAddIpOpen] = useState(false);
  const [removingIp, setRemovingIp] = useState<IpWhitelistEntry | null>(null);

  const { data: keyDetail, isLoading: isLoadingDetail } = useApiKeyDetail(
    apiKey.id,
    isOpen
  );
  const addIpWhitelist = useAddIpWhitelist();
  const removeIpWhitelist = useRemoveIpWhitelist();

  const handleAddIp = async (data: IpWhitelistFormData) => {
    await addIpWhitelist.mutateAsync({
      keyId: apiKey.id,
      data: {
        ipAddress: data.ipAddress,
        description: data.description || undefined,
      },
    });
    setIsAddIpOpen(false);
  };

  const handleRemoveIp = async () => {
    if (removingIp) {
      await removeIpWhitelist.mutateAsync({
        keyId: apiKey.id,
        whitelistId: removingIp.id,
      });
      setRemovingIp(null);
    }
  };

  const isActiveKey = apiKey.isActive && !apiKey.isRevoked;

  return (
    <Collapsible open={isOpen} onOpenChange={setIsOpen}>
      <TableRow className="group">
        <TableCell>
          <CollapsibleTrigger asChild>
            <Button variant="ghost" size="icon" className="h-6 w-6">
              {isOpen ? (
                <ChevronDown className="h-4 w-4" />
              ) : (
                <ChevronRight className="h-4 w-4" />
              )}
            </Button>
          </CollapsibleTrigger>
        </TableCell>
        <TableCell className="font-medium">{apiKey.name}</TableCell>
        <TableCell>
          <code className="text-sm">{apiKey.keyPrefix}...</code>
        </TableCell>
        <TableCell className="text-center">
          {apiKey.isRevoked ? (
            <Badge variant="destructive">Revoked</Badge>
          ) : apiKey.isExpired ? (
            <Badge variant="secondary">Expired</Badge>
          ) : apiKey.isActive ? (
            <Badge variant="default">Active</Badge>
          ) : (
            <Badge variant="secondary">Inactive</Badge>
          )}
        </TableCell>
        <TableCell>{apiKey.rateLimitPerMinute}/min</TableCell>
        <TableCell>
          {apiKey.expiresAt ? (
            <span className="flex items-center gap-1">
              <Clock className="h-3 w-3" />
              {formatDateTime(apiKey.expiresAt)}
            </span>
          ) : (
            <span className="text-muted-foreground">Never</span>
          )}
        </TableCell>
        <TableCell>
          {apiKey.lastUsedAt ? (
            formatDateTime(apiKey.lastUsedAt)
          ) : (
            <span className="text-muted-foreground">Never</span>
          )}
        </TableCell>
        <TableCell>
          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="ghost" size="icon">
                <MoreHorizontal className="h-4 w-4" />
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              {isActiveKey && (
                <>
                  <DropdownMenuItem onClick={() => onRotate(apiKey)}>
                    <RotateCw className="mr-2 h-4 w-4" />
                    Rotate Key
                  </DropdownMenuItem>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem
                    className="text-destructive"
                    onClick={() => onRevoke(apiKey)}
                  >
                    <Ban className="mr-2 h-4 w-4" />
                    Revoke
                  </DropdownMenuItem>
                </>
              )}
              {(apiKey.isRevoked || !apiKey.isActive) && (
                <DropdownMenuItem disabled>
                  <AlertTriangle className="mr-2 h-4 w-4" />
                  Key is {apiKey.isRevoked ? "revoked" : "inactive"}
                </DropdownMenuItem>
              )}
            </DropdownMenuContent>
          </DropdownMenu>
        </TableCell>
      </TableRow>
      <TableRow className="hover:bg-transparent">
        <TableCell colSpan={8} className="p-0">
          <CollapsibleContent>
            <div className="border-t bg-muted/30 px-6 py-4">
              <div className="flex items-center justify-between mb-3">
                <div className="flex items-center gap-2">
                  <Shield className="h-4 w-4 text-muted-foreground" />
                  <span className="font-medium text-sm">IP Whitelist</span>
                  {keyDetail?.ipWhitelist && keyDetail.ipWhitelist.length > 0 && (
                    <Badge variant="outline" className="text-xs">
                      {keyDetail.ipWhitelist.length}
                    </Badge>
                  )}
                </div>
                {isActiveKey && (
                  <Button
                    size="sm"
                    variant="outline"
                    onClick={() => setIsAddIpOpen(true)}
                  >
                    <Plus className="mr-1 h-3 w-3" />
                    Add IP
                  </Button>
                )}
              </div>

              {isLoadingDetail ? (
                <div className="space-y-2">
                  <Skeleton className="h-8 w-full" />
                  <Skeleton className="h-8 w-full" />
                </div>
              ) : keyDetail?.ipWhitelist && keyDetail.ipWhitelist.length > 0 ? (
                <div className="rounded-md border bg-background">
                  <Table>
                    <TableHeader>
                      <TableRow>
                        <TableHead>IP Address</TableHead>
                        <TableHead>Description</TableHead>
                        {isActiveKey && <TableHead className="w-[70px]"></TableHead>}
                      </TableRow>
                    </TableHeader>
                    <TableBody>
                      {keyDetail.ipWhitelist.map((ip) => (
                        <TableRow key={ip.id}>
                          <TableCell>
                            <div className="flex items-center gap-2">
                              <Globe className="h-4 w-4 text-muted-foreground" />
                              <code className="text-sm">{ip.ipAddress}</code>
                            </div>
                          </TableCell>
                          <TableCell>
                            {ip.description || (
                              <span className="text-muted-foreground">-</span>
                            )}
                          </TableCell>
                          {isActiveKey && (
                            <TableCell>
                              <Button
                                variant="ghost"
                                size="icon"
                                className="h-8 w-8 text-destructive hover:text-destructive"
                                onClick={() => setRemovingIp(ip)}
                              >
                                <Trash2 className="h-4 w-4" />
                              </Button>
                            </TableCell>
                          )}
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </div>
              ) : (
                <div className="text-center py-6 text-muted-foreground text-sm">
                  <Globe className="mx-auto h-8 w-8 mb-2 opacity-50" />
                  <p>No IP restrictions configured.</p>
                  <p className="text-xs mt-1">
                    This key can be used from any IP address.
                  </p>
                </div>
              )}

              {/* Add IP Dialog */}
              <Dialog open={isAddIpOpen} onOpenChange={setIsAddIpOpen}>
                <DialogContent>
                  <DialogHeader>
                    <DialogTitle>Add IP to Whitelist</DialogTitle>
                    <DialogDescription>
                      Add an IP address or CIDR range that is allowed to use this API
                      key.
                    </DialogDescription>
                  </DialogHeader>
                  <AddIpWhitelistForm
                    onSubmit={handleAddIp}
                    onCancel={() => setIsAddIpOpen(false)}
                    isLoading={addIpWhitelist.isPending}
                  />
                </DialogContent>
              </Dialog>

              {/* Remove IP Confirmation */}
              <ConfirmDialog
                open={!!removingIp}
                onOpenChange={(open) => !open && setRemovingIp(null)}
                title="Remove IP from Whitelist"
                description={`Are you sure you want to remove "${removingIp?.ipAddress}" from the whitelist?`}
                confirmText="Remove"
                variant="destructive"
                onConfirm={handleRemoveIp}
                isLoading={removeIpWhitelist.isPending}
              />
            </div>
          </CollapsibleContent>
        </TableCell>
      </TableRow>
    </Collapsible>
  );
}

export default function ApiClientDetailPage() {
  const params = useParams();
  const id = params.id as string;

  const [isEditOpen, setIsEditOpen] = useState(false);
  const [isCreateKeyOpen, setIsCreateKeyOpen] = useState(false);
  const [createdKey, setCreatedKey] = useState<CreateApiKeyResponse | null>(null);
  const [revokingKey, setRevokingKey] = useState<ApiKeyListItem | null>(null);
  const [rotatingKey, setRotatingKey] = useState<ApiKeyListItem | null>(null);

  const { data: client, isLoading } = useApiClient(id);
  const updateApiClient = useUpdateApiClient();
  const activateApiClient = useActivateApiClient();
  const deactivateApiClient = useDeactivateApiClient();
  const createApiKey = useCreateApiKey();
  const revokeApiKey = useRevokeApiKey();
  const rotateApiKey = useRotateApiKey();

  const handleEditSubmit = async (data: ApiClientFormData) => {
    await updateApiClient.mutateAsync({
      id,
      data: {
        name: data.name,
        description: data.description || undefined,
        contactEmail: data.contactEmail,
        contactPhone: data.contactPhone || undefined,
      },
    });
    setIsEditOpen(false);
  };

  const handleCreateKey = async (data: ApiKeyFormData) => {
    const result = await createApiKey.mutateAsync({
      apiClientId: id,
      name: data.name,
      rateLimitPerMinute: data.rateLimitPerMinute,
      expiresAt: data.expiresAt || undefined,
      permissions: data.permissions,
    });
    setIsCreateKeyOpen(false);
    setCreatedKey(result);
  };

  const handleRevokeKey = async (data: RevokeKeyFormData) => {
    if (revokingKey) {
      await revokeApiKey.mutateAsync({
        id: revokingKey.id,
        data: { reason: data.reason },
      });
      setRevokingKey(null);
    }
  };

  const handleRotateKey = async () => {
    if (rotatingKey) {
      const result = await rotateApiKey.mutateAsync(rotatingKey.id);
      setRotatingKey(null);
      setCreatedKey(result);
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

  if (!client) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" asChild>
            <Link href="/api-clients">
              <ArrowLeft className="h-4 w-4" />
            </Link>
          </Button>
          <h1 className="text-3xl font-bold tracking-tight">Client not found</h1>
        </div>
      </div>
    );
  }

  const activeKeys = client.apiKeys.filter((k) => k.isActive && !k.isRevoked);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" asChild>
            <Link href="/api-clients">
              <ArrowLeft className="h-4 w-4" />
            </Link>
          </Button>
          <div>
            <h1 className="text-3xl font-bold tracking-tight">{client.name}</h1>
            {client.description && (
              <p className="text-muted-foreground">{client.description}</p>
            )}
          </div>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => setIsEditOpen(true)}>
            <Pencil className="mr-2 h-4 w-4" />
            Edit
          </Button>
          <Button
            variant={client.isActive ? "destructive" : "default"}
            onClick={() =>
              client.isActive
                ? deactivateApiClient.mutate(id)
                : activateApiClient.mutate(id)
            }
            disabled={activateApiClient.isPending || deactivateApiClient.isPending}
          >
            {client.isActive ? "Deactivate" : "Activate"}
          </Button>
        </div>
      </div>

      <div className="grid gap-6 md:grid-cols-3">
        {/* Client Info */}
        <Card className="md:col-span-2">
          <CardHeader>
            <CardTitle>Client Information</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="grid gap-4 md:grid-cols-2">
              <div className="flex items-center gap-2">
                <Mail className="h-4 w-4 text-muted-foreground" />
                <span>{client.contactEmail}</span>
              </div>
              {client.contactPhone && (
                <div className="flex items-center gap-2">
                  <Phone className="h-4 w-4 text-muted-foreground" />
                  <span>{client.contactPhone}</span>
                </div>
              )}
            </div>

            <Separator />

            <div className="grid gap-4 md:grid-cols-3">
              <div>
                <p className="text-sm text-muted-foreground">Status</p>
                <Badge variant={client.isActive ? "default" : "secondary"}>
                  {client.isActive ? "Active" : "Inactive"}
                </Badge>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Active Keys</p>
                <Badge variant="outline">{activeKeys.length}</Badge>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Total Keys</p>
                <Badge variant="outline">{client.apiKeys.length}</Badge>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Metadata */}
        <Card>
          <CardHeader>
            <CardTitle>Details</CardTitle>
          </CardHeader>
          <CardContent className="space-y-2 text-sm">
            <div className="flex justify-between">
              <span className="text-muted-foreground">Created</span>
              <span>{formatDateTime(client.createdAt)}</span>
            </div>
            {client.createdBy && (
              <div className="flex justify-between">
                <span className="text-muted-foreground">Created By</span>
                <span>{client.createdBy}</span>
              </div>
            )}
            {client.updatedAt && (
              <div className="flex justify-between">
                <span className="text-muted-foreground">Updated</span>
                <span>{formatDateTime(client.updatedAt)}</span>
              </div>
            )}
          </CardContent>
        </Card>
      </div>

      {/* API Keys Section */}
      <Card>
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle className="flex items-center gap-2">
                <Key className="h-5 w-5" />
                API Keys
              </CardTitle>
              <CardDescription>
                Manage API keys for this client. Click a row to manage IP whitelist.
              </CardDescription>
            </div>
            <Button onClick={() => setIsCreateKeyOpen(true)}>
              <Plus className="mr-2 h-4 w-4" />
              Create Key
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          {client.apiKeys.length === 0 ? (
            <div className="text-center py-10">
              <Key className="mx-auto h-12 w-12 text-muted-foreground" />
              <h3 className="mt-4 text-lg font-semibold">No API keys</h3>
              <p className="text-muted-foreground">
                Create an API key to enable integrations.
              </p>
              <Button className="mt-4" onClick={() => setIsCreateKeyOpen(true)}>
                <Plus className="mr-2 h-4 w-4" />
                Create Key
              </Button>
            </div>
          ) : (
            <div className="rounded-md border">
              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead className="w-[40px]"></TableHead>
                    <TableHead>Name</TableHead>
                    <TableHead>Key Prefix</TableHead>
                    <TableHead className="text-center">Status</TableHead>
                    <TableHead>Rate Limit</TableHead>
                    <TableHead>Expires</TableHead>
                    <TableHead>Last Used</TableHead>
                    <TableHead className="w-[70px]"></TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {client.apiKeys.map((key) => (
                    <ApiKeyRow
                      key={key.id}
                      apiKey={key}
                      onRotate={setRotatingKey}
                      onRevoke={setRevokingKey}
                    />
                  ))}
                </TableBody>
              </Table>
            </div>
          )}
        </CardContent>
      </Card>

      {/* Edit Client Dialog */}
      <Dialog open={isEditOpen} onOpenChange={setIsEditOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Edit API Client</DialogTitle>
            <DialogDescription>Update the API client information.</DialogDescription>
          </DialogHeader>
          <ApiClientForm
            defaultValues={{
              name: client.name,
              description: client.description ?? "",
              contactEmail: client.contactEmail,
              contactPhone: client.contactPhone ?? "",
            }}
            onSubmit={handleEditSubmit}
            onCancel={() => setIsEditOpen(false)}
            isLoading={updateApiClient.isPending}
          />
        </DialogContent>
      </Dialog>

      {/* Create Key Dialog */}
      <Dialog open={isCreateKeyOpen} onOpenChange={setIsCreateKeyOpen}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle>Create API Key</DialogTitle>
            <DialogDescription>
              Create a new API key for this client. The key will only be shown once.
            </DialogDescription>
          </DialogHeader>
          <ApiKeyForm
            onSubmit={handleCreateKey}
            onCancel={() => setIsCreateKeyOpen(false)}
            isLoading={createApiKey.isPending}
          />
        </DialogContent>
      </Dialog>

      {/* Key Created Dialog */}
      <ApiKeyCreatedDialog
        open={!!createdKey}
        onOpenChange={(open) => !open && setCreatedKey(null)}
        keyData={createdKey}
      />

      {/* Revoke Key Dialog */}
      <Dialog open={!!revokingKey} onOpenChange={(open) => !open && setRevokingKey(null)}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Revoke API Key</DialogTitle>
            <DialogDescription>
              Are you sure you want to revoke &quot;{revokingKey?.name}&quot;? This action
              cannot be undone.
            </DialogDescription>
          </DialogHeader>
          <RevokeKeyForm
            onSubmit={handleRevokeKey}
            onCancel={() => setRevokingKey(null)}
            isLoading={revokeApiKey.isPending}
          />
        </DialogContent>
      </Dialog>

      {/* Rotate Key Confirmation */}
      <ConfirmDialog
        open={!!rotatingKey}
        onOpenChange={(open) => !open && setRotatingKey(null)}
        title="Rotate API Key"
        description={`This will revoke the current key "${rotatingKey?.name}" and create a new one with the same permissions. The new key will only be shown once.`}
        confirmText="Rotate Key"
        onConfirm={handleRotateKey}
        isLoading={rotateApiKey.isPending}
      />
    </div>
  );
}
