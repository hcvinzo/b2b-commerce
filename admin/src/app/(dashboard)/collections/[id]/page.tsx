"use client";

import { useParams, useRouter, useSearchParams } from "next/navigation";
import Link from "next/link";
import Image from "next/image";
import { ArrowLeft, Pencil, Star, Calendar, Package, Filter } from "lucide-react";

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
import { CollectionForm } from "@/components/forms/collection-form";
import { CollectionProductsEditor } from "@/components/collections/collection-products-editor";
import { CollectionFiltersEditor } from "@/components/collections/collection-filters-editor";
import { CollectionProductsPreview } from "@/components/collections/collection-products-preview";
import { useCollection, useUpdateCollection } from "@/hooks/use-collections";
import { formatDateTime, formatDate } from "@/lib/utils";
import { CollectionFormData } from "@/lib/validations/collection";

export default function CollectionDetailPage() {
  const params = useParams();
  const router = useRouter();
  const searchParams = useSearchParams();
  const id = params.id as string;
  const isEditing = searchParams.get("edit") === "true";

  const { data: collection, isLoading } = useCollection(id);
  const updateCollection = useUpdateCollection();

  const handleSubmit = async (data: CollectionFormData) => {
    await updateCollection.mutateAsync({ id, data });
    router.push(`/collections/${id}`);
  };

  if (isLoading) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-10 w-48" />
        <Skeleton className="h-[400px]" />
      </div>
    );
  }

  if (!collection) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" asChild>
            <Link href="/collections">
              <ArrowLeft className="h-4 w-4" />
            </Link>
          </Button>
          <h1 className="text-3xl font-bold tracking-tight">
            Collection not found
          </h1>
        </div>
      </div>
    );
  }

  if (isEditing) {
    return (
      <div className="space-y-6">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" asChild>
            <Link href={`/collections/${id}`}>
              <ArrowLeft className="h-4 w-4" />
            </Link>
          </Button>
          <div>
            <h1 className="text-3xl font-bold tracking-tight">Edit Collection</h1>
            <p className="text-muted-foreground">
              Update collection information
            </p>
          </div>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>Collection Information</CardTitle>
          </CardHeader>
          <CardContent>
            <CollectionForm
              isEditing
              defaultValues={{
                name: collection.name,
                description: collection.description,
                imageUrl: collection.imageUrl || "",
                type: collection.type,
                displayOrder: collection.displayOrder,
                isActive: collection.isActive,
                isFeatured: collection.isFeatured,
                startDate: collection.startDate || "",
                endDate: collection.endDate || "",
              }}
              onSubmit={handleSubmit}
              onCancel={() => router.push(`/collections/${id}`)}
              isLoading={updateCollection.isPending}
            />
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button variant="ghost" size="icon" asChild>
            <Link href="/collections">
              <ArrowLeft className="h-4 w-4" />
            </Link>
          </Button>
          <div className="flex items-center gap-3">
            {collection.imageUrl && (
              <div className="relative h-12 w-12 overflow-hidden rounded-md bg-muted">
                <Image
                  src={collection.imageUrl}
                  alt={collection.name}
                  fill
                  className="object-cover"
                  sizes="48px"
                />
              </div>
            )}
            <div>
              <div className="flex items-center gap-2">
                <h1 className="text-3xl font-bold tracking-tight">
                  {collection.name}
                </h1>
                {collection.isFeatured && (
                  <Star className="h-5 w-5 text-yellow-500 fill-yellow-500" />
                )}
              </div>
              <p className="text-muted-foreground">/{collection.slug}</p>
            </div>
          </div>
        </div>
        <Button asChild>
          <Link href={`/collections/${id}?edit=true`}>
            <Pencil className="mr-2 h-4 w-4" />
            Edit Collection
          </Link>
        </Button>
      </div>

      {/* Info Cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Type</CardDescription>
          </CardHeader>
          <CardContent>
            <div className="flex items-center gap-2">
              {collection.type === "Manual" ? (
                <Package className="h-4 w-4 text-muted-foreground" />
              ) : (
                <Filter className="h-4 w-4 text-muted-foreground" />
              )}
              <span className="text-2xl font-bold">{collection.type}</span>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Products</CardDescription>
          </CardHeader>
          <CardContent>
            <span className="text-2xl font-bold">{collection.productCount}</span>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Status</CardDescription>
          </CardHeader>
          <CardContent>
            <Badge
              variant={
                collection.isCurrentlyActive
                  ? "default"
                  : collection.isActive
                  ? "secondary"
                  : "outline"
              }
              className="text-sm"
            >
              {collection.isCurrentlyActive
                ? "Live"
                : collection.isActive
                ? "Scheduled"
                : "Inactive"}
            </Badge>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="pb-2">
            <CardDescription>Schedule</CardDescription>
          </CardHeader>
          <CardContent>
            {collection.startDate || collection.endDate ? (
              <div className="flex items-center gap-1 text-sm">
                <Calendar className="h-3 w-3 text-muted-foreground" />
                <span>
                  {collection.startDate
                    ? formatDate(collection.startDate)
                    : "—"}{" "}
                  →{" "}
                  {collection.endDate ? formatDate(collection.endDate) : "∞"}
                </span>
              </div>
            ) : (
              <span className="text-sm text-muted-foreground">Always active</span>
            )}
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="overview" className="space-y-4">
        <TabsList>
          <TabsTrigger value="overview">Overview</TabsTrigger>
          {collection.type === "Manual" && (
            <TabsTrigger value="products">Products</TabsTrigger>
          )}
          {collection.type === "Dynamic" && (
            <TabsTrigger value="filters">Filters</TabsTrigger>
          )}
          {collection.type === "Dynamic" && (
            <TabsTrigger value="products">Preview Products</TabsTrigger>
          )}
        </TabsList>

        <TabsContent value="overview" className="space-y-6">
          <div className="grid gap-6 md:grid-cols-3">
            {/* Main Info */}
            <Card className="md:col-span-2">
              <CardHeader>
                <CardTitle>Collection Details</CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid gap-4 md:grid-cols-2">
                  <div>
                    <p className="text-sm text-muted-foreground">Name</p>
                    <p className="font-medium">{collection.name}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Slug</p>
                    <p className="font-medium font-mono text-sm">
                      {collection.slug}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Type</p>
                    <Badge variant="outline">{collection.type}</Badge>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Display Order</p>
                    <p className="font-medium">{collection.displayOrder}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Status</p>
                    <Badge
                      variant={collection.isActive ? "default" : "secondary"}
                    >
                      {collection.isActive ? "Active" : "Inactive"}
                    </Badge>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Featured</p>
                    <Badge
                      variant={collection.isFeatured ? "default" : "outline"}
                    >
                      {collection.isFeatured ? "Yes" : "No"}
                    </Badge>
                  </div>
                </div>

                {collection.description && (
                  <>
                    <Separator />
                    <div>
                      <p className="text-sm text-muted-foreground mb-2">
                        Description
                      </p>
                      <p className="text-sm">{collection.description}</p>
                    </div>
                  </>
                )}
              </CardContent>
            </Card>

            {/* Metadata */}
            <Card>
              <CardHeader>
                <CardTitle>Metadata</CardTitle>
              </CardHeader>
              <CardContent className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Created</span>
                  <span>{formatDateTime(collection.createdAt)}</span>
                </div>
                {collection.updatedAt && (
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">Updated</span>
                    <span>{formatDateTime(collection.updatedAt)}</span>
                  </div>
                )}
                {collection.externalId && (
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">External ID</span>
                    <span className="font-mono text-xs">
                      {collection.externalId}
                    </span>
                  </div>
                )}
                {collection.startDate && (
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">Start Date</span>
                    <span>{formatDateTime(collection.startDate)}</span>
                  </div>
                )}
                {collection.endDate && (
                  <div className="flex justify-between">
                    <span className="text-muted-foreground">End Date</span>
                    <span>{formatDateTime(collection.endDate)}</span>
                  </div>
                )}
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        {/* Products Tab - For Manual collections */}
        {collection.type === "Manual" && (
          <TabsContent value="products">
            <CollectionProductsEditor collection={collection} />
          </TabsContent>
        )}

        {/* Filters Tab - For Dynamic collections */}
        {collection.type === "Dynamic" && (
          <TabsContent value="filters">
            <CollectionFiltersEditor collection={collection} />
          </TabsContent>
        )}

        {/* Preview Tab - For Dynamic collections */}
        {collection.type === "Dynamic" && (
          <TabsContent value="products">
            <Card>
              <CardHeader>
                <CardTitle>Matching Products</CardTitle>
                <CardDescription>
                  Preview of products that match the current filter criteria
                </CardDescription>
              </CardHeader>
              <CardContent>
                <CollectionProductsPreview collection={collection} />
              </CardContent>
            </Card>
          </TabsContent>
        )}
      </Tabs>
    </div>
  );
}
