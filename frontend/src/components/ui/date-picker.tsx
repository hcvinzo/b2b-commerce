"use client"

import * as React from "react"
import { format } from "date-fns"
import { tr } from "date-fns/locale"
import { CalendarIcon } from "lucide-react"
import { cn } from "@/lib/utils"
import { Calendar } from "@/components/ui/calendar"
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover"

interface DatePickerProps extends Omit<React.ButtonHTMLAttributes<HTMLButtonElement>, 'value' | 'onChange'> {
  value?: Date | string
  onChange?: (date: Date | undefined) => void
  placeholder?: string
}

const DatePicker = React.forwardRef<HTMLButtonElement, DatePickerProps>(
  ({ value, onChange, placeholder = "Tarih seçiniz", disabled, className, id, ...props }, ref) => {
    const [open, setOpen] = React.useState(false)

    // Convert string to Date if needed
    const dateValue = React.useMemo(() => {
      if (!value) return undefined
      if (value instanceof Date) return value
      // Handle string date format (YYYY-MM-DD)
      const parsed = new Date(value)
      return isNaN(parsed.getTime()) ? undefined : parsed
    }, [value])

    const handleSelect = (date: Date | undefined) => {
      onChange?.(date)
      setOpen(false)
    }

    return (
      <Popover open={open} onOpenChange={setOpen}>
        <PopoverTrigger
          ref={ref}
          id={id}
          type="button"
          disabled={disabled}
          className={cn(
            "border bg-background shadow-xs hover:bg-accent hover:text-accent-foreground dark:bg-input/30 dark:border-input dark:hover:bg-input/50",
            "inline-flex items-center justify-center gap-2 whitespace-nowrap rounded-sm text-sm font-medium transition-all disabled:pointer-events-none disabled:opacity-50",
            "h-9 px-4 py-2",
            "outline-none focus-visible:border-ring focus-visible:ring-ring/50 focus-visible:ring-[3px]",
            "w-full justify-start text-left font-normal",
            !dateValue && "text-muted-foreground",
            className
          )}
          aria-describedby={props["aria-describedby"]}
          aria-invalid={props["aria-invalid"]}
        >
          <CalendarIcon className="mr-2 h-4 w-4" />
          {dateValue ? format(dateValue, "dd MMMM yyyy", { locale: tr }) : placeholder}
        </PopoverTrigger>
        <PopoverContent className="w-auto p-0" align="start">
          <Calendar
            mode="single"
            selected={dateValue}
            onSelect={handleSelect}
            locale={tr}
            initialFocus
            captionLayout="dropdown"
            fromYear={1940}
            toYear={new Date().getFullYear()}
          />
        </PopoverContent>
      </Popover>
    )
  }
)
DatePicker.displayName = "DatePicker"

export { DatePicker }
