"use client";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { Loader2 } from "lucide-react";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Switch } from "@/components/ui/switch";
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
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  createParameterSchema,
  updateParameterSchema,
  type CreateParameterFormData,
  type UpdateParameterFormData,
} from "@/lib/validations/parameter";
import { ParameterValueInput } from "./parameter-value-input";
import type { ParameterValueType, ParameterType } from "@/types/entities";

interface CreateParameterFormProps {
  mode: "create";
  onSubmit: (data: CreateParameterFormData) => Promise<void>;
  onCancel?: () => void;
  isLoading?: boolean;
}

interface UpdateParameterFormProps {
  mode: "update";
  defaultValues: {
    key: string;
    value: string;
    description?: string;
    parameterType: ParameterType;
    valueType: ParameterValueType;
    isEditable: boolean;
  };
  onSubmit: (data: UpdateParameterFormData) => Promise<void>;
  onCancel?: () => void;
  isLoading?: boolean;
}

type ParameterFormProps = CreateParameterFormProps | UpdateParameterFormProps;

export function ParameterForm(props: ParameterFormProps) {
  const { mode, onSubmit, onCancel, isLoading } = props;

  if (mode === "create") {
    return <CreateForm onSubmit={onSubmit} onCancel={onCancel} isLoading={isLoading} />;
  }

  return (
    <UpdateForm
      defaultValues={props.defaultValues}
      onSubmit={onSubmit}
      onCancel={onCancel}
      isLoading={isLoading}
    />
  );
}

function CreateForm({
  onSubmit,
  onCancel,
  isLoading,
}: {
  onSubmit: (data: CreateParameterFormData) => Promise<void>;
  onCancel?: () => void;
  isLoading?: boolean;
}) {
  const form = useForm<CreateParameterFormData>({
    resolver: zodResolver(createParameterSchema),
    defaultValues: {
      key: "",
      value: "",
      description: "",
      parameterType: "Business",
      valueType: "String",
      isEditable: true,
    },
  });

  const valueType = form.watch("valueType");

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        <FormField
          control={form.control}
          name="key"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Key *</FormLabel>
              <FormControl>
                <Input placeholder="cache.default-duration" {...field} />
              </FormControl>
              <FormDescription>
                Format: module.submodule.name (e.g., auth.session.timeout)
              </FormDescription>
              <FormMessage />
            </FormItem>
          )}
        />

        <div className="grid gap-6 md:grid-cols-2">
          <FormField
            control={form.control}
            name="parameterType"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Parameter Type *</FormLabel>
                <Select onValueChange={field.onChange} defaultValue={field.value}>
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder="Select type" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    <SelectItem value="System">System</SelectItem>
                    <SelectItem value="Business">Business</SelectItem>
                  </SelectContent>
                </Select>
                <FormDescription>
                  System = technical settings, Business = business rules
                </FormDescription>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="valueType"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Value Type *</FormLabel>
                <Select onValueChange={field.onChange} defaultValue={field.value}>
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder="Select value type" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    <SelectItem value="String">String</SelectItem>
                    <SelectItem value="Number">Number</SelectItem>
                    <SelectItem value="Boolean">Boolean</SelectItem>
                    <SelectItem value="DateTime">Date/Time</SelectItem>
                    <SelectItem value="Json">JSON</SelectItem>
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            )}
          />
        </div>

        <FormField
          control={form.control}
          name="value"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Value *</FormLabel>
              <FormControl>
                <ParameterValueInput
                  valueType={valueType}
                  value={field.value}
                  onChange={field.onChange}
                />
              </FormControl>
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
                <Textarea
                  placeholder="Describe what this parameter controls"
                  className="min-h-[80px]"
                  {...field}
                  value={field.value || ""}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="isEditable"
          render={({ field }) => (
            <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
              <div className="space-y-0.5">
                <FormLabel className="text-base">Editable</FormLabel>
                <FormDescription>
                  Allow this parameter&apos;s value to be changed
                </FormDescription>
              </div>
              <FormControl>
                <Switch checked={field.value} onCheckedChange={field.onChange} />
              </FormControl>
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
            Create Parameter
          </Button>
        </div>
      </form>
    </Form>
  );
}

function UpdateForm({
  defaultValues,
  onSubmit,
  onCancel,
  isLoading,
}: {
  defaultValues: UpdateParameterFormProps["defaultValues"];
  onSubmit: (data: UpdateParameterFormData) => Promise<void>;
  onCancel?: () => void;
  isLoading?: boolean;
}) {
  const form = useForm<UpdateParameterFormData>({
    resolver: zodResolver(updateParameterSchema),
    defaultValues: {
      value: defaultValues.value,
      description: defaultValues.description || "",
      isEditable: defaultValues.isEditable,
    },
  });

  return (
    <Form {...form}>
      <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
        {/* Read-only key display */}
        <div className="space-y-2">
          <label className="text-sm font-medium">Key</label>
          <Input value={defaultValues.key} disabled className="bg-muted" />
        </div>

        <div className="grid gap-6 md:grid-cols-2">
          <div className="space-y-2">
            <label className="text-sm font-medium">Parameter Type</label>
            <Input value={defaultValues.parameterType} disabled className="bg-muted" />
          </div>

          <div className="space-y-2">
            <label className="text-sm font-medium">Value Type</label>
            <Input value={defaultValues.valueType} disabled className="bg-muted" />
          </div>
        </div>

        <FormField
          control={form.control}
          name="value"
          render={({ field }) => (
            <FormItem>
              <FormLabel>Value</FormLabel>
              <FormControl>
                <ParameterValueInput
                  valueType={defaultValues.valueType}
                  value={field.value || ""}
                  onChange={field.onChange}
                  disabled={!defaultValues.isEditable}
                />
              </FormControl>
              {!defaultValues.isEditable && (
                <FormDescription className="text-amber-600">
                  This parameter is not editable
                </FormDescription>
              )}
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
                <Textarea
                  placeholder="Describe what this parameter controls"
                  className="min-h-[80px]"
                  {...field}
                  value={field.value || ""}
                />
              </FormControl>
              <FormMessage />
            </FormItem>
          )}
        />

        <FormField
          control={form.control}
          name="isEditable"
          render={({ field }) => (
            <FormItem className="flex flex-row items-center justify-between rounded-lg border p-4">
              <div className="space-y-0.5">
                <FormLabel className="text-base">Editable</FormLabel>
                <FormDescription>
                  Allow this parameter&apos;s value to be changed
                </FormDescription>
              </div>
              <FormControl>
                <Switch checked={field.value} onCheckedChange={field.onChange} />
              </FormControl>
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
            Save Changes
          </Button>
        </div>
      </form>
    </Form>
  );
}
