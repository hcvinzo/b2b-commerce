'use client'

import { useState, useEffect } from 'react'
import { Loader2 } from 'lucide-react'
import { Checkbox } from '@/components/ui/checkbox'
import { Label } from '@/components/ui/label'
import { getAttributeByCode, type AttributeDefinition } from '@/lib/api'

interface MultiSelectAttributeProps {
  attributeCode: string
  value?: string[]
  onChange: (value: string[]) => void
  label?: string
  required?: boolean
  disabled?: boolean
  className?: string
  columns?: number
}

export function MultiSelectAttribute({
  attributeCode,
  value = [],
  onChange,
  label,
  required = false,
  disabled = false,
  className,
  columns = 3,
}: MultiSelectAttributeProps) {
  const [attribute, setAttribute] = useState<AttributeDefinition | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    async function loadAttribute() {
      try {
        setIsLoading(true)
        setError(null)
        const attr = await getAttributeByCode(attributeCode)
        setAttribute(attr)
      } catch (err) {
        console.error(`Failed to load attribute ${attributeCode}:`, err)
        setError('Yüklenemedi')
      } finally {
        setIsLoading(false)
      }
    }
    loadAttribute()
  }, [attributeCode])

  const handleCheckboxChange = (optionValue: string, checked: boolean) => {
    if (checked) {
      onChange([...value, optionValue])
    } else {
      onChange(value.filter((v) => v !== optionValue))
    }
  }

  if (isLoading) {
    return (
      <div className={className}>
        {label && (
          <Label className="mb-3 block text-lg font-semibold">
            {label}
            {required && <span className="text-destructive ml-1">*</span>}
          </Label>
        )}
        <div className="flex items-center gap-2 text-muted-foreground">
          <Loader2 className="h-4 w-4 animate-spin" />
          <span className="text-sm">Yükleniyor...</span>
        </div>
      </div>
    )
  }

  if (error || !attribute) {
    return (
      <div className={className}>
        {label && (
          <Label className="mb-3 block text-lg font-semibold">
            {label}
            {required && <span className="text-destructive ml-1">*</span>}
          </Label>
        )}
        <div className="text-sm text-muted-foreground">{error || 'Yüklenemedi'}</div>
      </div>
    )
  }

  const displayLabel = label || attribute.name
  const gridColsClass = columns === 2 ? 'sm:grid-cols-2' : columns === 4 ? 'sm:grid-cols-4' : 'sm:grid-cols-3'

  return (
    <div className={className}>
      {displayLabel && (
        <h3 className="text-lg font-semibold text-foreground mb-4">
          {displayLabel}
          {required && <span className="text-destructive ml-1">*</span>}
        </h3>
      )}
      <div className={`grid grid-cols-2 ${gridColsClass} gap-3`}>
        {attribute.predefinedValues
          ?.sort((a, b) => a.displayOrder - b.displayOrder)
          .map((option) => {
            const checkboxId = `${attributeCode}-${option.id}`
            const isChecked = value.includes(option.value)
            return (
              <div key={option.id} className="flex items-center space-x-2">
                <Checkbox
                  id={checkboxId}
                  checked={isChecked}
                  onCheckedChange={(checked) =>
                    handleCheckboxChange(option.value, checked as boolean)
                  }
                  disabled={disabled}
                />
                <Label
                  htmlFor={checkboxId}
                  className="text-sm font-normal cursor-pointer"
                >
                  {option.displayText || option.value}
                </Label>
              </div>
            )
          })}
      </div>
    </div>
  )
}
