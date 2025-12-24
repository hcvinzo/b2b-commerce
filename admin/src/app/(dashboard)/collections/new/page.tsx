"use client";

import { useRouter } from "next/navigation";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { CollectionForm } from "@/components/forms/collection-form";
import { useCreateCollection } from "@/hooks/use-collections";
import { CollectionFormData } from "@/lib/validations/collection";

export default function NewCollectionPage() {
  const router = useRouter();
  const createCollection = useCreateCollection();

  const handleSubmit = async (data: CollectionFormData) => {
    const result = await createCollection.mutateAsync(data);
    // Navigate to the new collection's detail page
    router.push(`/collections/${result.id}`);
  };

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">Add Collection</h1>
        <p className="text-muted-foreground">
          Create a new product collection
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Collection Information</CardTitle>
        </CardHeader>
        <CardContent>
          <CollectionForm
            onSubmit={handleSubmit}
            onCancel={() => router.push("/collections")}
            isLoading={createCollection.isPending}
          />
        </CardContent>
      </Card>
    </div>
  );
}
