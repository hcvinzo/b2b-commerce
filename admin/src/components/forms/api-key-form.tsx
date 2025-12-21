"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2 } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "@/components/ui/form";
import { apiKeySchema, type ApiKeyFormData } from "@/lib/validations/api-client";
import { useAvailableScopes } from "@/hooks/use-api-keys";
import { Skeleton } from "@/components/ui/skeleton";

interface ApiKeyFormProps {
  defaultValues?: Partial<ApiKeyFormData>;
  onSubmit: (data: ApiKeyFormData) => Promise<void>;
  onCancel?: () => void;
  isLoading?: boolean;
}

export function ApiKeyForm({
  defaultValues,
  onSubmit,
  onCancel,
  isLoading,
}: ApiKeyFormProps) {
  const { data: scopesData, isLoading: scopesLoading } = useAvailableScopes();

  const form = useForm<ApiKeyFormData>({
    resolver: zodResolver(apiKeySchema),
    defaultValues: {
      name: "",
      rateLimitPerMinute: 500,
      expiresAt: "",
      permissions: [],
      ...defaultValues,
    },
  });

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <FormField
          control={form.control}
          name="name"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Key Name *</FormLabel>
              <FormControl>
                <Input placeholder="Enter key name" {...field} />
              </FormControl>
              <FormDescription>A descriptive name for this API key</FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="grid gap-6 md:grid-cols-2">
          <FormField
            control={form.control}
            name="rateLimitPerMinute"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Rate Limit (per minute) *</FormLabel>
                <FormControl>
                  <Input
                    type="number"
                    {...field}
                    onChange={(e) => field.onChange(parseInt(e.target.value) || 500)}
                  />
                </FormControl>
                <FormDescription>Maximum API requests allowed per minute</FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="expiresAt"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Expiration Date</FormLabel>
                <FormControl>
                  <Input type="datetime-local" {...field} value={field.value || ""} />
                </FormControl>
                <FormDescription>Leave empty for no expiration</FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <FormField
          control={form.control}
          name="permissions"
          render={() => (
            <FormItem>
              <FormLabel>Permissions</FormLabel>
              <FormDescription className="mb-4">
                Select the API scopes this key should have access to
              </FormDescription>
              {scopesLoading ? (
                <div className="space-y-3">
                  <Skeleton className="h-8 w-full" />
                  <Skeleton className="h-8 w-full" />
                </div>
              ) : scopesData?.categories && scopesData.categories.length > 0 ? (
                <div className="space-y-4 max-h-[300px] overflow-y-auto rounded-md border p-4">
                  {scopesData.categories.map((category) => (
                    <div key={category.name} className="space-y-2">
                      <h4 className="text-sm font-medium">{category.name}</h4>
                      <div className="flex flex-wrap gap-4">
                        {category.scopes.map((scope) => (
                          <FormField
                            key={scope}
                            control={form.control}
                            name="permissions"
                            render={({ field }) => (
                              <FormItem className="flex items-center space-x-2 space-y-0">
                                <FormControl>
                                  <Checkbox
                                    checked={field.value?.includes(scope)}
                                    onCheckedChange={(checked) => {
                                      const current = field.value || [];
                                      if (checked) {
                                        field.onChange([...current, scope]);
                                      } else {
                                        field.onChange(current.filter((s) => s !== scope));
                                      }
                                    }}
                                  />
                                </FormControl>
                                <FormLabel className="font-mono text-xs font-normal">
                                  {scope}
                                </FormLabel>
                              </FormItem>
                            )}
                          />
                        ))}
                      </div>
                    </div>
                  ))}
                </div>
              ) : (
                <p className="text-sm text-muted-foreground">
                  No permission scopes available
                </p>
              )}
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="flex justify-end gap-4">
          {onCancel && (
            <Button type="button" variant="outline" onClick={onCancel}>
              Cancel
            </Button>
          )}
          <Button type="submit" disabled={isLoading}>
            {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
            Create Key
          </Button>
        </div>
      </form>
    </Form>
  );
}
