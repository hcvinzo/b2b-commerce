"use client"

import * as React from "react"
import { InfoIcon } from "lucide-react"

import {
  Select,
  SelectContent,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import {
  InputGroup,
  InputGroupAddon,
  InputGroupButton,
  InputGroupSelectTrigger,
} from "@/components/ui/input-group"
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip"

interface SelectExtProps {
  /** Tooltip info text shown with help icon */
  info?: string
  /** Placeholder text for the select */
  placeholder?: string
  /** Current value */
  value?: string
  /** Callback when value changes */
  onValueChange?: (value: string) => void
  /** Whether the select is disabled */
  disabled?: boolean
  /** Select content (SelectItem children) */
  children: React.ReactNode
  /** Additional class name for the trigger */
  className?: string
}

function SelectExt({
  info,
  placeholder,
  value,
  onValueChange,
  disabled,
  children,
  className,
}: SelectExtProps) {
  if (!info) {
    return (
      <Select value={value} onValueChange={onValueChange} disabled={disabled}>
        <SelectTrigger className={className}>
          <SelectValue placeholder={placeholder} />
        </SelectTrigger>
        <SelectContent>{children}</SelectContent>
      </Select>
    )
  }

  return (
    <Select value={value} onValueChange={onValueChange} disabled={disabled}>
      <InputGroup>
        <Tooltip>
          <TooltipTrigger asChild>
            <InputGroupAddon>
              <InputGroupButton variant="ghost" aria-label="Info" size="icon-xs">
                <InfoIcon />
              </InputGroupButton>
            </InputGroupAddon>
          </TooltipTrigger>
          <TooltipContent side="top">
            <p>{info}</p>
          </TooltipContent>
        </Tooltip>
        <InputGroupSelectTrigger>
          <SelectTrigger className={className}>
            <SelectValue placeholder={placeholder} />
          </SelectTrigger>
        </InputGroupSelectTrigger>
      </InputGroup>
      <SelectContent>{children}</SelectContent>
    </Select>
  )
}

export { SelectExt }
