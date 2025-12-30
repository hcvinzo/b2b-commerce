"use client";

import { useState, useCallback, useEffect, useMemo } from "react";
import { Plus, Trash2, Save, AlertCircle, Loader2 } from "lucide-react";

import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Skeleton } from "@/components/ui/skeleton";
import { Badge } from "@/components/ui/badge";
import {
  useCustomerAttributes,
  useUpsertCustomerAttributes,
} from "@/hooks/use-customers";
import { useAttributes, useChildAttributes } from "@/hooks/use-attributes";
import {
  AttributeDefinition,
  AttributeEntityTypeEnum,
  AttributeTypeFromString,
  CustomerAttribute,
} from "@/types/entities";
import { toast } from "sonner";

interface CustomerAttributesEditorProps {
  customerId: string;
}

// Helper to parse attribute type from string or number
function getAttributeTypeNumber(type: string | number): number {
  if (typeof type === "number") return type;
  return AttributeTypeFromString[type as keyof typeof AttributeTypeFromString] || 1;
}

// Helper to parse JSON safely
function safeParseJson<T>(json: string, defaultValue: T): T {
  if (!json) return defaultValue;
  try {
    return JSON.parse(json) as T;
  } catch {
    return defaultValue;
  }
}

// Helper to normalize attribute value based on attribute type
// Handles both old format (raw values) and new format ({selected: value})
function normalizeAttributeValue(
  rawValue: string,
  attributeType: number
): Record<string, unknown> {
  const parsed = safeParseJson<unknown>(rawValue, rawValue);

  // If already in correct object format, return as-is
  if (typeof parsed === "object" && parsed !== null && !Array.isArray(parsed)) {
    return parsed as Record<string, unknown>;
  }

  // For Select (3) and MultiSelect (4), wrap raw values in {selected: value}
  if (attributeType === 3) {
    // Select - single value
    return { selected: parsed };
  }

  // For other types, wrap in {value: parsed}
  if (typeof parsed === "string" || typeof parsed === "number" || typeof parsed === "boolean") {
    return { value: parsed };
  }

  // Default: return as object or empty
  return typeof parsed === "object" ? (parsed as Record<string, unknown>) : {};
}

// Track dirty state per attribute definition
type DirtyState = Record<string, boolean>;

// Track local state per attribute definition
interface AttributeState {
  attributeDefinitionId: string;
  attributeCode: string;
  attributeName: string;
  items: Array<{ id?: string; value: Record<string, unknown> }>;
}

export function CustomerAttributesEditor({
  customerId,
}: CustomerAttributesEditorProps) {
  // Fetch attribute definitions for Customer entity type (with predefined values for Select/MultiSelect)
  const { data: attributeDefinitions, isLoading: isLoadingDefinitions } = useAttributes(
    AttributeEntityTypeEnum.Customer,
    true // includeValues
  );

  // Fetch customer attributes
  const { data: customerAttributes, isLoading: isLoadingAttributes } = useCustomerAttributes(customerId);

  const upsertMutation = useUpsertCustomerAttributes();

  // Filter to only show root-level attributes (not child attributes of composite types)
  // and sort by displayOrder
  const rootAttributeDefinitions = useMemo(() => {
    if (!attributeDefinitions) return [];
    return attributeDefinitions
      .filter((def) => !def.parentAttributeId)
      .sort((a, b) => (a.displayOrder ?? 0) - (b.displayOrder ?? 0));
  }, [attributeDefinitions]);

  // Local state for attribute values - keyed by attributeCode
  const [attributeStates, setAttributeStates] = useState<Record<string, AttributeState>>({});

  // Track dirty state per attribute
  const [dirtyState, setDirtyState] = useState<DirtyState>({});

  // Check if any section has unsaved changes
  const hasUnsavedChanges = Object.values(dirtyState).some(Boolean);

  // Count of dirty sections
  const dirtyCount = Object.values(dirtyState).filter(Boolean).length;

  // Parse customer attributes into local state when data loads
  useEffect(() => {
    if (rootAttributeDefinitions && customerAttributes) {
      const newStates: Record<string, AttributeState> = {};

      // Initialize state for each root attribute definition (not child attributes)
      rootAttributeDefinitions.forEach((def) => {
        const typeNum = getAttributeTypeNumber(def.type);
        const matchingAttributes = customerAttributes.filter(
          (ca) => ca.attributeCode === def.code
        );

        // For MultiSelect (type 4), aggregate multiple rows into a single item with array
        if (typeNum === 4 && matchingAttributes.length > 0) {
          // Collect all selected values from multiple rows
          const selectedValues: string[] = [];
          matchingAttributes.forEach((ca) => {
            const parsed = safeParseJson<unknown>(ca.value, ca.value);
            if (typeof parsed === "object" && parsed !== null && "selected" in parsed) {
              // Already in {selected: value} format
              const sel = (parsed as { selected: unknown }).selected;
              if (Array.isArray(sel)) {
                selectedValues.push(...sel.map(String));
              } else if (sel) {
                selectedValues.push(String(sel));
              }
            } else if (typeof parsed === "string") {
              // Raw string value
              selectedValues.push(parsed);
            }
          });

          newStates[def.code] = {
            attributeDefinitionId: def.id,
            attributeCode: def.code,
            attributeName: def.name,
            items: [{ value: { selected: selectedValues } }],
          };
        } else {
          // For other types, process normally
          newStates[def.code] = {
            attributeDefinitionId: def.id,
            attributeCode: def.code,
            attributeName: def.name,
            items: matchingAttributes.map((ca) => ({
              id: ca.id,
              value: normalizeAttributeValue(ca.value, typeNum),
            })),
          };
        }
      });

      setAttributeStates(newStates);
      setDirtyState({});
    }
  }, [rootAttributeDefinitions, customerAttributes]);

  // Mark a section as dirty
  const markDirty = useCallback((code: string) => {
    setDirtyState((prev) => ({ ...prev, [code]: true }));
  }, []);

  // Mark a section as clean
  const markClean = useCallback((code: string) => {
    setDirtyState((prev) => ({ ...prev, [code]: false }));
  }, []);

  // Save handler for a specific attribute
  const saveAttribute = useCallback(
    async (code: string) => {
      const state = attributeStates[code];
      if (!state) return;

      // Find the attribute definition to check its type
      const def = rootAttributeDefinitions?.find((d) => d.code === code);
      const typeNum = def ? getAttributeTypeNumber(def.type) : 0;

      try {
        let itemsToSave: Array<{ id?: string; value: string }>;

        // For MultiSelect (type 4), expand the selected array into multiple items
        if (typeNum === 4 && state.items.length > 0) {
          const selectedValues = (state.items[0]?.value?.selected as string[]) || [];
          itemsToSave = selectedValues.map((val) => ({
            value: JSON.stringify(val), // Each value as a separate item
          }));
        } else {
          // For other types, save as-is
          itemsToSave = state.items.map((item) => ({
            id: item.id,
            value: JSON.stringify(item.value),
          }));
        }

        await upsertMutation.mutateAsync({
          customerId,
          data: {
            attributeDefinitionId: state.attributeDefinitionId,
            items: itemsToSave,
          },
        });
        markClean(code);
      } catch (error) {
        console.error("Failed to save attribute:", error);
      }
    },
    [attributeStates, customerId, markClean, upsertMutation, rootAttributeDefinitions]
  );

  // Save all dirty sections
  const saveAll = useCallback(async () => {
    const dirtyCodes = Object.entries(dirtyState)
      .filter(([, isDirty]) => isDirty)
      .map(([code]) => code);

    try {
      await Promise.all(dirtyCodes.map((code) => saveAttribute(code)));
      toast.success("Tüm değişiklikler kaydedildi");
    } catch {
      toast.error("Bazı değişiklikler kaydedilemedi");
    }
  }, [dirtyState, saveAttribute]);

  // Update item value handler
  const updateItemValue = useCallback(
    (code: string, itemIndex: number, key: string, value: unknown) => {
      setAttributeStates((prev) => {
        const state = prev[code];
        if (!state) return prev;

        const newItems = [...state.items];
        newItems[itemIndex] = {
          ...newItems[itemIndex],
          value: { ...newItems[itemIndex].value, [key]: value },
        };

        return {
          ...prev,
          [code]: { ...state, items: newItems },
        };
      });
      markDirty(code);
    },
    [markDirty]
  );

  // Add new item to a list attribute
  const addItem = useCallback(
    (code: string) => {
      setAttributeStates((prev) => {
        const state = prev[code];
        if (!state) return prev;

        return {
          ...prev,
          [code]: {
            ...state,
            items: [...state.items, { value: {} }],
          },
        };
      });
      markDirty(code);
    },
    [markDirty]
  );

  // Remove item from a list attribute
  const removeItem = useCallback(
    (code: string, itemIndex: number) => {
      setAttributeStates((prev) => {
        const state = prev[code];
        if (!state) return prev;

        const newItems = state.items.filter((_, i) => i !== itemIndex);

        return {
          ...prev,
          [code]: { ...state, items: newItems },
        };
      });
      markDirty(code);
    },
    [markDirty]
  );

  // Set entire value (for non-composite types like Select, MultiSelect)
  const setAttributeValue = useCallback(
    (code: string, value: unknown) => {
      setAttributeStates((prev) => {
        const state = prev[code];
        if (!state) return prev;

        // For non-list attributes, update the first item or create one
        const items = state.items.length > 0
          ? [{ ...state.items[0], value: value as Record<string, unknown> }]
          : [{ value: value as Record<string, unknown> }];

        return {
          ...prev,
          [code]: { ...state, items },
        };
      });
      markDirty(code);
    },
    [markDirty]
  );

  if (isLoadingDefinitions || isLoadingAttributes) {
    return (
      <div className="space-y-6">
        <Skeleton className="h-[300px]" />
        <Skeleton className="h-[300px]" />
      </div>
    );
  }

  if (!rootAttributeDefinitions || rootAttributeDefinitions.length === 0) {
    return (
      <Card>
        <CardContent className="py-8">
          <p className="text-center text-muted-foreground">
            Müşteri için tanımlanmış özellik bulunamadı.
          </p>
        </CardContent>
      </Card>
    );
  }

  // Dirty indicator badge component
  const DirtyBadge = ({ isDirty }: { isDirty: boolean }) =>
    isDirty ? (
      <Badge variant="outline" className="ml-2 text-orange-600 border-orange-300 bg-orange-50">
        <AlertCircle className="h-3 w-3 mr-1" />
        Kaydedilmedi
      </Badge>
    ) : null;

  return (
    <div className="space-y-6">
      {/* Global Save All Button */}
      {hasUnsavedChanges && (
        <div className="sticky top-0 z-10 bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60 border rounded-lg p-4 flex items-center justify-between shadow-sm">
          <div className="flex items-center gap-2 text-orange-600">
            <AlertCircle className="h-5 w-5" />
            <span className="font-medium">
              {dirtyCount} bölümde kaydedilmemiş değişiklik var
            </span>
          </div>
          <Button onClick={saveAll} disabled={upsertMutation.isPending}>
            <Save className="h-4 w-4 mr-2" />
            Tümünü Kaydet
          </Button>
        </div>
      )}

      {/* Render each root attribute definition (not child attributes) */}
      {rootAttributeDefinitions.map((definition) => (
        <AttributeCard
          key={definition.id}
          definition={definition}
          state={attributeStates[definition.code]}
          isDirty={dirtyState[definition.code] || false}
          onSave={() => saveAttribute(definition.code)}
          onAddItem={() => addItem(definition.code)}
          onRemoveItem={(idx) => removeItem(definition.code, idx)}
          onUpdateItemValue={(idx, key, val) => updateItemValue(definition.code, idx, key, val)}
          onSetValue={(val) => setAttributeValue(definition.code, val)}
          isSaving={upsertMutation.isPending}
        />
      ))}
    </div>
  );
}

// Individual attribute card component
interface AttributeCardProps {
  definition: AttributeDefinition;
  state?: AttributeState;
  isDirty: boolean;
  onSave: () => void;
  onAddItem: () => void;
  onRemoveItem: (index: number) => void;
  onUpdateItemValue: (index: number, key: string, value: unknown) => void;
  onSetValue: (value: unknown) => void;
  isSaving: boolean;
}

function AttributeCard({
  definition,
  state,
  isDirty,
  onSave,
  onAddItem,
  onRemoveItem,
  onUpdateItemValue,
  onSetValue,
  isSaving,
}: AttributeCardProps) {
  const typeNum = getAttributeTypeNumber(definition.type);
  const isComposite = typeNum === 7; // Composite
  const isList = definition.isList || false;

  // Dirty indicator badge component
  const DirtyBadge = () =>
    isDirty ? (
      <Badge variant="outline" className="ml-2 text-orange-600 border-orange-300 bg-orange-50">
        <AlertCircle className="h-3 w-3 mr-1" />
        Kaydedilmedi
      </Badge>
    ) : null;

  return (
    <Card className={isDirty ? "ring-2 ring-orange-200" : ""}>
      <CardHeader>
        <div className="flex items-center justify-between">
          <div>
            <div className="flex items-center">
              <CardTitle>{definition.name}</CardTitle>
              <DirtyBadge />
            </div>
            <CardDescription>
              {definition.code}
              {isComposite && isList && " (Liste)"}
            </CardDescription>
          </div>
          <div className="flex gap-2">
            {isComposite && isList && (
              <Button variant="outline" size="sm" onClick={onAddItem}>
                <Plus className="h-4 w-4 mr-1" />
                Ekle
              </Button>
            )}
            <Button
              size="sm"
              onClick={onSave}
              disabled={isSaving || !isDirty}
              variant={isDirty ? "default" : "outline"}
            >
              {isSaving ? (
                <Loader2 className="h-4 w-4 mr-1 animate-spin" />
              ) : (
                <Save className="h-4 w-4 mr-1" />
              )}
              Kaydet
            </Button>
          </div>
        </div>
      </CardHeader>
      <CardContent>
        {isComposite ? (
          <CompositeAttributeEditor
            definition={definition}
            items={state?.items || []}
            isList={isList}
            onUpdateItemValue={onUpdateItemValue}
            onRemoveItem={onRemoveItem}
            onAddItem={onAddItem}
          />
        ) : typeNum === 3 ? ( // Select
          <SelectAttributeEditor
            definition={definition}
            value={state?.items[0]?.value}
            onSetValue={onSetValue}
          />
        ) : typeNum === 4 ? ( // MultiSelect
          <MultiSelectAttributeEditor
            definition={definition}
            value={state?.items[0]?.value}
            onSetValue={onSetValue}
          />
        ) : (
          <SimpleAttributeEditor
            definition={definition}
            value={state?.items[0]?.value}
            onSetValue={onSetValue}
          />
        )}
      </CardContent>
    </Card>
  );
}

// Composite attribute editor - handles child attributes
interface CompositeAttributeEditorProps {
  definition: AttributeDefinition;
  items: Array<{ id?: string; value: Record<string, unknown> }>;
  isList: boolean;
  onUpdateItemValue: (index: number, key: string, value: unknown) => void;
  onRemoveItem: (index: number) => void;
  onAddItem: () => void;
}

function CompositeAttributeEditor({
  definition,
  items,
  isList,
  onUpdateItemValue,
  onRemoveItem,
  onAddItem,
}: CompositeAttributeEditorProps) {
  const { data: childAttributes, isLoading } = useChildAttributes(definition.id);

  // Sort child attributes by displayOrder
  const sortedChildAttributes = useMemo(() => {
    if (!childAttributes) return [];
    return [...childAttributes].sort((a, b) => (a.displayOrder ?? 0) - (b.displayOrder ?? 0));
  }, [childAttributes]);

  if (isLoading) {
    return <Skeleton className="h-20" />;
  }

  if (!sortedChildAttributes || sortedChildAttributes.length === 0) {
    return (
      <p className="text-sm text-muted-foreground">Alt özellik tanımı bulunamadı.</p>
    );
  }

  // For list attributes
  if (isList) {
    if (items.length === 0) {
      return (
        <div className="text-center py-4">
          <p className="text-sm text-muted-foreground mb-2">Kayıt bulunamadı</p>
          <Button variant="outline" size="sm" onClick={onAddItem}>
            <Plus className="h-4 w-4 mr-1" />
            Ekle
          </Button>
        </div>
      );
    }

    return (
      <div className="space-y-4">
        {items.map((item, idx) => (
          <div key={item.id || idx} className="flex gap-4 items-end border-b pb-4 last:border-0">
            {sortedChildAttributes.map((childDef) => (
              <div key={childDef.id} className="flex-1">
                <Label>{childDef.name}</Label>
                <ChildAttributeInput
                  definition={childDef}
                  value={item.value[childDef.code]}
                  onChange={(val) => onUpdateItemValue(idx, childDef.code, val)}
                />
              </div>
            ))}
            <Button
              variant="ghost"
              size="icon"
              onClick={() => onRemoveItem(idx)}
            >
              <Trash2 className="h-4 w-4 text-destructive" />
            </Button>
          </div>
        ))}
      </div>
    );
  }

  // For single composite value
  const item = items[0] || { value: {} };
  return (
    <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
      {sortedChildAttributes.map((childDef) => (
        <div key={childDef.id}>
          <Label>{childDef.name}</Label>
          <ChildAttributeInput
            definition={childDef}
            value={item.value[childDef.code]}
            onChange={(val) => onUpdateItemValue(0, childDef.code, val)}
          />
        </div>
      ))}
    </div>
  );
}

// Child attribute input - renders appropriate input based on type
interface ChildAttributeInputProps {
  definition: AttributeDefinition;
  value: unknown;
  onChange: (value: unknown) => void;
}

function ChildAttributeInput({ definition, value, onChange }: ChildAttributeInputProps) {
  const typeNum = getAttributeTypeNumber(definition.type);

  // Sort predefined values by displayOrder
  const sortedValues = useMemo(() => {
    if (!definition.predefinedValues) return [];
    return [...definition.predefinedValues].sort((a, b) => (a.displayOrder ?? 0) - (b.displayOrder ?? 0));
  }, [definition.predefinedValues]);

  // Select type
  if (typeNum === 3 && sortedValues.length) {
    return (
      <Select
        value={value as string || ""}
        onValueChange={onChange}
      >
        <SelectTrigger>
          <SelectValue placeholder="Seçiniz" />
        </SelectTrigger>
        <SelectContent>
          {sortedValues.map((pv) => (
            <SelectItem key={pv.id} value={pv.value}>
              {pv.displayText || pv.value}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    );
  }

  // Number type
  if (typeNum === 2) {
    return (
      <Input
        type="number"
        value={(value as number) || ""}
        onChange={(e) => onChange(parseFloat(e.target.value) || 0)}
        placeholder={definition.unit ? `${definition.name} (${definition.unit})` : definition.name}
      />
    );
  }

  // Default: Text type
  return (
    <Input
      type="text"
      value={(value as string) || ""}
      onChange={(e) => onChange(e.target.value)}
      placeholder={definition.name}
    />
  );
}

// Select attribute editor
interface SelectAttributeEditorProps {
  definition: AttributeDefinition;
  value?: Record<string, unknown>;
  onSetValue: (value: unknown) => void;
}

function SelectAttributeEditor({ definition, value, onSetValue }: SelectAttributeEditorProps) {
  const selectedValue = (value?.selected as string) || "";

  // Sort predefined values by displayOrder
  const sortedValues = useMemo(() => {
    if (!definition.predefinedValues) return [];
    return [...definition.predefinedValues].sort((a, b) => (a.displayOrder ?? 0) - (b.displayOrder ?? 0));
  }, [definition.predefinedValues]);

  if (!sortedValues.length) {
    return <p className="text-sm text-muted-foreground">Seçenek tanımlanmamış.</p>;
  }

  return (
    <Select
      value={selectedValue}
      onValueChange={(val) => onSetValue({ selected: val })}
    >
      <SelectTrigger className="w-full md:w-[300px]">
        <SelectValue placeholder="Seçiniz" />
      </SelectTrigger>
      <SelectContent>
        {sortedValues.map((pv) => (
          <SelectItem key={pv.id} value={pv.value}>
            {pv.displayText || pv.value}
          </SelectItem>
        ))}
      </SelectContent>
    </Select>
  );
}

// MultiSelect attribute editor
interface MultiSelectAttributeEditorProps {
  definition: AttributeDefinition;
  value?: Record<string, unknown>;
  onSetValue: (value: unknown) => void;
}

function MultiSelectAttributeEditor({ definition, value, onSetValue }: MultiSelectAttributeEditorProps) {
  const selectedValues = (value?.selected as string[]) || [];

  // Sort predefined values by displayOrder
  const sortedValues = useMemo(() => {
    if (!definition.predefinedValues) return [];
    return [...definition.predefinedValues].sort((a, b) => (a.displayOrder ?? 0) - (b.displayOrder ?? 0));
  }, [definition.predefinedValues]);

  if (!sortedValues.length) {
    return <p className="text-sm text-muted-foreground">Seçenek tanımlanmamış.</p>;
  }

  const toggleValue = (val: string) => {
    if (selectedValues.includes(val)) {
      onSetValue({ selected: selectedValues.filter((v) => v !== val) });
    } else {
      onSetValue({ selected: [...selectedValues, val] });
    }
  };

  return (
    <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
      {sortedValues.map((pv) => (
        <div key={pv.id} className="flex items-center space-x-2">
          <Checkbox
            id={`${definition.code}-${pv.id}`}
            checked={selectedValues.includes(pv.value)}
            onCheckedChange={() => toggleValue(pv.value)}
          />
          <Label
            htmlFor={`${definition.code}-${pv.id}`}
            className="text-sm cursor-pointer"
          >
            {pv.displayText || pv.value}
          </Label>
        </div>
      ))}
    </div>
  );
}

// Simple attribute editor (Text, Number, Boolean, Date)
interface SimpleAttributeEditorProps {
  definition: AttributeDefinition;
  value?: Record<string, unknown>;
  onSetValue: (value: unknown) => void;
}

function SimpleAttributeEditor({ definition, value, onSetValue }: SimpleAttributeEditorProps) {
  const typeNum = getAttributeTypeNumber(definition.type);
  const currentValue = value?.value as string | number | boolean | undefined;

  // Boolean
  if (typeNum === 5) {
    return (
      <div className="flex items-center space-x-2">
        <Checkbox
          id={definition.code}
          checked={currentValue as boolean || false}
          onCheckedChange={(checked) => onSetValue({ value: checked })}
        />
        <Label htmlFor={definition.code} className="cursor-pointer">
          {definition.name}
        </Label>
      </div>
    );
  }

  // Number
  if (typeNum === 2) {
    return (
      <Input
        type="number"
        value={(currentValue as number) || ""}
        onChange={(e) => onSetValue({ value: parseFloat(e.target.value) || 0 })}
        placeholder={definition.unit ? `${definition.name} (${definition.unit})` : definition.name}
        className="w-full md:w-[300px]"
      />
    );
  }

  // Date
  if (typeNum === 6) {
    return (
      <Input
        type="date"
        value={(currentValue as string) || ""}
        onChange={(e) => onSetValue({ value: e.target.value })}
        className="w-full md:w-[300px]"
      />
    );
  }

  // Default: Text
  return (
    <Input
      type="text"
      value={(currentValue as string) || ""}
      onChange={(e) => onSetValue({ value: e.target.value })}
      placeholder={definition.name}
      className="w-full md:w-[300px]"
    />
  );
}
