"use client";

import { useEffect } from "react";
import { format } from "date-fns";
import { CalendarIcon } from "lucide-react";

import { useProductType } from "@/hooks/use-product-types";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Switch } from "@/components/ui/switch";
import { Button } from "@/components/ui/button";
import { Checkbox } from "@/components/ui/checkbox";
import { Calendar } from "@/components/ui/calendar";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover";
import { Skeleton } from "@/components/ui/skeleton";
import { cn } from "@/lib/utils";
import {
  ProductTypeAttribute,
  AttributeTypeEnum,
  ProductAttributeValueInput,
} from "@/types/entities";

interface ProductAttributesFormProps {
  productTypeId?: string;
  values: ProductAttributeValueInput[];
  onChange: (values: ProductAttributeValueInput[]) => void;
  errors?: Record<string, string>;
}

export function ProductAttributesForm({
  productTypeId,
  values,
  onChange,
  errors = {},
}: ProductAttributesFormProps) {
  const { data: productType, isLoading } = useProductType(productTypeId ?? null);

  // Get the current value for an attribute
  const getAttributeValue = (attributeId: string): ProductAttributeValueInput | undefined => {
    return values.find((v) => v.attributeDefinitionId === attributeId);
  };

  // Update or add an attribute value
  const updateAttributeValue = (
    attributeId: string,
    update: Partial<ProductAttributeValueInput>
  ) => {
    const existing = values.find((v) => v.attributeDefinitionId === attributeId);
    if (existing) {
      onChange(
        values.map((v) =>
          v.attributeDefinitionId === attributeId ? { ...v, ...update } : v
        )
      );
    } else {
      onChange([...values, { attributeDefinitionId: attributeId, ...update }]);
    }
  };

  if (!productTypeId) {
    return (
      <div className="flex flex-col items-center justify-center py-8 text-muted-foreground">
        <p>Select a product type to configure attributes</p>
      </div>
    );
  }

  if (isLoading) {
    return (
      <div className="space-y-4">
        {[1, 2, 3].map((i) => (
          <div key={i} className="space-y-2">
            <Skeleton className="h-4 w-24" />
            <Skeleton className="h-10 w-full" />
          </div>
        ))}
      </div>
    );
  }

  if (!productType || !productType.attributes || productType.attributes.length === 0) {
    return (
      <div className="flex flex-col items-center justify-center py-8 text-muted-foreground">
        <p>No attributes defined for this product type</p>
      </div>
    );
  }

  const renderAttributeInput = (attr: ProductTypeAttribute) => {
    const value = getAttributeValue(attr.attributeDefinitionId);
    const error = errors[attr.attributeDefinitionId];

    switch (attr.attributeType) {
      case AttributeTypeEnum.Text:
        return (
          <div className="space-y-2">
            <Label htmlFor={attr.attributeDefinitionId}>
              {attr.attributeName}
              {attr.isRequired && <span className="text-destructive ml-1">*</span>}
              {attr.unit && <span className="text-muted-foreground ml-1">({attr.unit})</span>}
            </Label>
            <Input
              id={attr.attributeDefinitionId}
              value={value?.textValue || ""}
              onChange={(e) =>
                updateAttributeValue(attr.attributeDefinitionId, {
                  textValue: e.target.value,
                })
              }
              placeholder={`Enter ${attr.attributeName.toLowerCase()}`}
            />
            {error && <p className="text-sm text-destructive">{error}</p>}
          </div>
        );

      case AttributeTypeEnum.Number:
        return (
          <div className="space-y-2">
            <Label htmlFor={attr.attributeDefinitionId}>
              {attr.attributeName}
              {attr.isRequired && <span className="text-destructive ml-1">*</span>}
              {attr.unit && <span className="text-muted-foreground ml-1">({attr.unit})</span>}
            </Label>
            <Input
              id={attr.attributeDefinitionId}
              type="number"
              step="any"
              value={value?.numericValue ?? ""}
              onChange={(e) =>
                updateAttributeValue(attr.attributeDefinitionId, {
                  numericValue: e.target.value ? parseFloat(e.target.value) : undefined,
                })
              }
              placeholder={`Enter ${attr.attributeName.toLowerCase()}`}
            />
            {error && <p className="text-sm text-destructive">{error}</p>}
          </div>
        );

      case AttributeTypeEnum.Select:
        return (
          <div className="space-y-2">
            <Label htmlFor={attr.attributeDefinitionId}>
              {attr.attributeName}
              {attr.isRequired && <span className="text-destructive ml-1">*</span>}
            </Label>
            <Select
              value={value?.selectValueId || ""}
              onValueChange={(val) =>
                updateAttributeValue(attr.attributeDefinitionId, {
                  selectValueId: val || undefined,
                })
              }
            >
              <SelectTrigger>
                <SelectValue placeholder={`Select ${attr.attributeName.toLowerCase()}`} />
              </SelectTrigger>
              <SelectContent>
                {attr.predefinedValues?.map((pv) => (
                  <SelectItem key={pv.id} value={pv.id}>
                    {pv.displayText || pv.value}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            {error && <p className="text-sm text-destructive">{error}</p>}
          </div>
        );

      case AttributeTypeEnum.MultiSelect:
        const selectedIds = value?.multiSelectValueIds || [];
        return (
          <div className="space-y-2">
            <Label>
              {attr.attributeName}
              {attr.isRequired && <span className="text-destructive ml-1">*</span>}
            </Label>
            <div className="border rounded-md p-3 space-y-2">
              {attr.predefinedValues?.map((pv) => (
                <div key={pv.id} className="flex items-center space-x-2">
                  <Checkbox
                    id={`${attr.attributeDefinitionId}-${pv.id}`}
                    checked={selectedIds.includes(pv.id)}
                    onCheckedChange={(checked) => {
                      const newIds = checked
                        ? [...selectedIds, pv.id]
                        : selectedIds.filter((id) => id !== pv.id);
                      updateAttributeValue(attr.attributeDefinitionId, {
                        multiSelectValueIds: newIds.length > 0 ? newIds : undefined,
                      });
                    }}
                  />
                  <Label
                    htmlFor={`${attr.attributeDefinitionId}-${pv.id}`}
                    className="font-normal cursor-pointer"
                  >
                    {pv.displayText || pv.value}
                  </Label>
                </div>
              ))}
            </div>
            {error && <p className="text-sm text-destructive">{error}</p>}
          </div>
        );

      case AttributeTypeEnum.Boolean:
        return (
          <div className="flex items-center justify-between rounded-lg border p-4">
            <div className="space-y-0.5">
              <Label htmlFor={attr.attributeDefinitionId} className="text-base">
                {attr.attributeName}
                {attr.isRequired && <span className="text-destructive ml-1">*</span>}
              </Label>
            </div>
            <Switch
              id={attr.attributeDefinitionId}
              checked={value?.booleanValue ?? false}
              onCheckedChange={(checked) =>
                updateAttributeValue(attr.attributeDefinitionId, {
                  booleanValue: checked,
                })
              }
            />
            {error && <p className="text-sm text-destructive">{error}</p>}
          </div>
        );

      case AttributeTypeEnum.Date:
        const dateValue = value?.dateValue ? new Date(value.dateValue) : undefined;
        return (
          <div className="space-y-2">
            <Label htmlFor={attr.attributeDefinitionId}>
              {attr.attributeName}
              {attr.isRequired && <span className="text-destructive ml-1">*</span>}
            </Label>
            <Popover>
              <PopoverTrigger asChild>
                <Button
                  variant="outline"
                  className={cn(
                    "w-full justify-start text-left font-normal",
                    !dateValue && "text-muted-foreground"
                  )}
                >
                  <CalendarIcon className="mr-2 h-4 w-4" />
                  {dateValue ? format(dateValue, "PPP") : <span>Pick a date</span>}
                </Button>
              </PopoverTrigger>
              <PopoverContent className="w-auto p-0" align="start">
                <Calendar
                  mode="single"
                  selected={dateValue}
                  onSelect={(date) =>
                    updateAttributeValue(attr.attributeDefinitionId, {
                      dateValue: date?.toISOString(),
                    })
                  }
                  initialFocus
                />
              </PopoverContent>
            </Popover>
            {error && <p className="text-sm text-destructive">{error}</p>}
          </div>
        );

      default:
        return null;
    }
  };

  return (
    <div className="space-y-6">
      {productType.attributes
        .sort((a, b) => a.displayOrder - b.displayOrder)
        .map((attr) => (
          <div key={attr.attributeDefinitionId}>{renderAttributeInput(attr)}</div>
        ))}
    </div>
  );
}
