"use client";

import { useState, useEffect } from "react";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Switch } from "@/components/ui/switch";
import { Label } from "@/components/ui/label";
import type { ParameterValueType } from "@/types/entities";

interface ParameterValueInputProps {
  valueType: ParameterValueType;
  value: string;
  onChange: (value: string) => void;
  disabled?: boolean;
}

export function ParameterValueInput({
  valueType,
  value,
  onChange,
  disabled = false,
}: ParameterValueInputProps) {
  // For boolean, we need to track internal state
  const [boolValue, setBoolValue] = useState(value === "true");

  useEffect(() => {
    if (valueType === "Boolean") {
      setBoolValue(value === "true");
    }
  }, [value, valueType]);

  const handleBoolChange = (checked: boolean) => {
    setBoolValue(checked);
    onChange(checked ? "true" : "false");
  };

  switch (valueType) {
    case "Number":
      return (
        <Input
          type="number"
          value={value}
          onChange={(e) => onChange(e.target.value)}
          placeholder="Enter number"
          disabled={disabled}
        />
      );

    case "Boolean":
      return (
        <div className="flex items-center gap-3 rounded-lg border p-3">
          <Switch
            checked={boolValue}
            onCheckedChange={handleBoolChange}
            disabled={disabled}
          />
          <Label className="text-sm text-muted-foreground">
            {boolValue ? "True" : "False"}
          </Label>
        </div>
      );

    case "DateTime":
      return (
        <Input
          type="datetime-local"
          value={value ? formatDateTimeForInput(value) : ""}
          onChange={(e) => onChange(e.target.value ? new Date(e.target.value).toISOString() : "")}
          disabled={disabled}
        />
      );

    case "Json":
      return (
        <Textarea
          value={value}
          onChange={(e) => onChange(e.target.value)}
          placeholder='{"key": "value"}'
          className="min-h-[120px] font-mono text-sm"
          disabled={disabled}
        />
      );

    case "String":
    default:
      return (
        <Input
          type="text"
          value={value}
          onChange={(e) => onChange(e.target.value)}
          placeholder="Enter value"
          disabled={disabled}
        />
      );
  }
}

// Helper to format ISO date string for datetime-local input
function formatDateTimeForInput(isoString: string): string {
  try {
    const date = new Date(isoString);
    if (isNaN(date.getTime())) return "";
    // Format as YYYY-MM-DDTHH:mm
    return date.toISOString().slice(0, 16);
  } catch {
    return "";
  }
}
