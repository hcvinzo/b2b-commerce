"use client"

import * as React from "react"
import { InfoIcon } from "lucide-react"

import { Input } from "@/components/ui/input"
import {
  InputGroup,
  InputGroupAddon,
  InputGroupButton,
  InputGroupInput,
} from "@/components/ui/input-group"
import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip"

interface InputExtProps extends React.ComponentProps<"input"> {
  /** Tooltip info text shown with help icon */
  info?: string
}

function InputExt({ info, className, ...props }: InputExtProps) {
  if (!info) {
    return <Input className={className} {...props} />
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
      <InputGroupInput className={className} {...props} />
    </InputGroup>
  )
}

export { InputExt }
