'use client'

import { forwardRef, InputHTMLAttributes } from 'react'
import { cn } from '@/lib/utils'

interface CheckboxProps extends Omit<InputHTMLAttributes<HTMLInputElement>, 'type'> {
  label: string
}

const Checkbox = forwardRef<HTMLInputElement, CheckboxProps>(
  ({ className, label, id, ...props }, ref) => {
    const checkboxId = id || props.name

    return (
      <label htmlFor={checkboxId} className="flex items-center gap-2 cursor-pointer">
        <input
          ref={ref}
          type="checkbox"
          id={checkboxId}
          className={cn(
            'w-4 h-4 text-primary border-gray-300 rounded',
            'focus:ring-2 focus:ring-primary focus:ring-offset-2',
            className
          )}
          {...props}
        />
        <span className="text-sm text-gray-700">{label}</span>
      </label>
    )
  }
)

Checkbox.displayName = 'Checkbox'
export { Checkbox }
