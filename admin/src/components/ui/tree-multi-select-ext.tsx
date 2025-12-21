"use client"

import * as React from "react"
import { InfoIcon } from "lucide-react"

import { TreeMultiSelect } from "@/components/ui/tree-multi-select"
import {
  InputGroup,
  InputGroupAddon,
  InputGroupButton,
} from "@/components/ui/input-group"
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip"
import { Category } from "@/types/entities"

interface TreeMultiSelectExtProps {
  categories: Category[]
  value: string[]
  onChange: (value: string[]) => void
  excludeId?: string
  placeholder?: string
  disabled?: boolean
  /** Tooltip info text shown with help icon */
  info?: string
}

function TreeMultiSelectExt({ info, ...props }: TreeMultiSelectExtProps) {
  if (!info) {
    return <TreeMultiSelect {...props} />
  }

  return (
    <InputGroup className="h-auto">
      <Tooltip>
        <TooltipTrigger asChild>
          <InputGroupAddon className="self-center">
            <InputGroupButton variant="ghost" aria-label="Info" size="icon-xs">
              <InfoIcon />
            </InputGroupButton>
          </InputGroupAddon>
        </TooltipTrigger>
        <TooltipContent side="top">
          <p>{info}</p>
        </TooltipContent>
      </Tooltip>
      <div className="flex-1 [&>div]:w-full [&_button]:border-0 [&_button]:shadow-none [&_button]:rounded-none [&_button]:bg-transparent [&_button]:dark:bg-transparent">
        <TreeMultiSelect {...props} />
      </div>
    </InputGroup>
  )
}

export { TreeMultiSelectExt }
