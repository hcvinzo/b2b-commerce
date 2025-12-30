'use client'

import { useState, useEffect } from 'react'
import { Loader2 } from 'lucide-react'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { Label } from '@/components/ui/label'
import { getAttributeByCode, type AttributeDefinition } from '@/lib/api'

interface SingleSelectAttributeProps {
  attributeCode: string
  value?: string
  onChange: (value: string) => void
  label?: string
  placeholder?: string
  required?: boolean
  disabled?: boolean
  className?: string
  hasError?: boolean
}

export function SingleSelectAttribute({
  attributeCode,
  value,
  onChange,
  label,
  placeholder = 'Seçiniz',
  required = false,
  disabled = false,
  className,
  hasError = false,
}: SingleSelectAttributeProps) {
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

  if (isLoading) {
    return (
      <div className={className}>
        {label && (
          <Label className="mb-2 block">
            {label}
            {required && <span className="text-destructive ml-1">*</span>}
          </Label>
        )}
        <div className="flex items-center gap-2 text-muted-foreground h-10">
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
          <Label className="mb-2 block">
            {label}
            {required && <span className="text-destructive ml-1">*</span>}
          </Label>
        )}
        <div className="text-sm text-muted-foreground">{error || 'Yüklenemedi'}</div>
      </div>
    )
  }

  const displayLabel = label || attribute.name

  return (
    <div className={className}>
      {displayLabel && (
        <Label className={`mb-2 block ${hasError ? 'text-destructive' : ''}`}>
          {displayLabel}
          {required && <span className="text-destructive ml-1">*</span>}
        </Label>
      )}
      <Select
        value={value || ''}
        onValueChange={onChange}
        disabled={disabled}
      >
        <SelectTrigger className={`w-full ${hasError ? 'border-destructive' : ''}`}>
          <SelectValue placeholder={placeholder} />
        </SelectTrigger>
        <SelectContent>
          {attribute.predefinedValues
            ?.sort((a, b) => a.displayOrder - b.displayOrder)
            .map((option) => (
              <SelectItem key={option.id} value={option.value}>
                {option.displayText || option.value}
              </SelectItem>
            ))}
        </SelectContent>
      </Select>
    </div>
  )
}
