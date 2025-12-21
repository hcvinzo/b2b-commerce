"use client"

import * as React from "react"
import { InfoIcon } from "lucide-react"

import { TreeSelect } from "@/components/ui/tree-select"
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

interface TreeSelectExtProps {
  categories: Category[]
  value?: string
  onChange: (value: string) => void
  excludeId?: string
  placeholder?: string
  disabled?: boolean
  /** Whether to show "No Parent (Root Category)" option. Default: true */
  allowEmpty?: boolean
  /** Custom label for empty option. Default: "No Parent (Root Category)" */
  emptyLabel?: string
  /** Tooltip info text shown with help icon */
  info?: string
}

function TreeSelectExt({ info, ...props }: TreeSelectExtProps) {
  if (!info) {
    return <TreeSelect {...props} />
  }

  return (
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
      <div className="flex-1 [&>div]:w-full [&_button]:border-0 [&_button]:shadow-none [&_button]:rounded-none [&_button]:bg-transparent [&_button]:dark:bg-transparent">
        <TreeSelect {...props} />
      </div>
    </InputGroup>
  )
}

export { TreeSelectExt }
