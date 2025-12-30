'use client'

import { Plus, Trash2 } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'

export interface SelectOption {
  value: string
  label: string
}

export interface CompositeAttributeField {
  code: string
  name: string
  type: 'text' | 'number' | 'tc_kimlik' | 'select'
  placeholder?: string
  required?: boolean
  min?: number
  max?: number
  maxLength?: number
  options?: SelectOption[]
}

export interface CompositeAttributeValue {
  [key: string]: string | number | undefined
}

interface CompositeAttributeInputProps {
  title: string
  fields: CompositeAttributeField[]
  values: CompositeAttributeValue[]
  onChange: (values: CompositeAttributeValue[]) => void
  minRows?: number
  maxRows?: number
  addButtonText?: string
  errors?: Record<number, Record<string, string>>
}

export function CompositeAttributeInput({
  title,
  fields,
  values,
  onChange,
  minRows = 1,
  maxRows = 10,
  addButtonText,
  errors = {},
}: CompositeAttributeInputProps) {
  const buttonText = addButtonText || `${title} ekle`

  const handleAddRow = () => {
    if (values.length < maxRows) {
      const emptyRow: CompositeAttributeValue = {}
      fields.forEach((field) => {
        emptyRow[field.code] = field.type === 'number' ? 0 : ''
      })
      onChange([...values, emptyRow])
    }
  }

  const handleRemoveRow = (index: number) => {
    if (values.length > minRows) {
      const newValues = values.filter((_, i) => i !== index)
      onChange(newValues)
    }
  }

  const handleFieldChange = (
    rowIndex: number,
    fieldCode: string,
    value: string | number
  ) => {
    const newValues = [...values]
    newValues[rowIndex] = {
      ...newValues[rowIndex],
      [fieldCode]: value,
    }
    onChange(newValues)
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-semibold text-foreground">{title}</h3>
        {values.length < maxRows && (
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={handleAddRow}
            className="gap-2"
          >
            <Plus className="h-4 w-4" />
            {buttonText}
          </Button>
        )}
      </div>

      {/* Header Row */}
      <div className="overflow-x-auto">
        <table className="w-full">
          <thead>
            <tr className="border-b border-border">
              {fields.map((field) => (
                <th
                  key={field.code}
                  className="text-left py-2 px-2 text-sm font-medium text-muted-foreground"
                >
                  {field.name}
                  {field.required && <span className="text-destructive ml-1">*</span>}
                </th>
              ))}
              <th className="w-10"></th>
            </tr>
          </thead>
          <tbody>
            {values.map((row, rowIndex) => (
              <tr key={rowIndex} className="border-b border-border/50">
                {fields.map((field) => (
                  <td key={field.code} className="py-2 px-2">
                    <div>
                      {field.type === 'select' && field.options ? (
                        <Select
                          value={String(row[field.code] ?? '')}
                          onValueChange={(value) => handleFieldChange(rowIndex, field.code, value)}
                        >
                          <SelectTrigger className="text-sm">
                            <SelectValue placeholder={field.placeholder || field.name} />
                          </SelectTrigger>
                          <SelectContent>
                            {field.options.map((option) => (
                              <SelectItem key={option.value} value={option.value}>
                                {option.label}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                      ) : (
                        <Input
                          type={field.type === 'number' ? 'number' : 'text'}
                          placeholder={field.placeholder || field.name}
                          className="text-sm"
                          value={row[field.code] ?? (field.type === 'number' ? 0 : '')}
                          onChange={(e) => {
                            let value: string | number = e.target.value
                            if (field.type === 'number') {
                              // Handle empty string as 0, otherwise parse the number
                              const parsed = parseFloat(e.target.value)
                              value = isNaN(parsed) ? 0 : parsed
                            }
                            handleFieldChange(rowIndex, field.code, value)
                          }}
                          min={field.min}
                          max={field.max}
                          maxLength={field.maxLength}
                        />
                      )}
                      {errors[rowIndex]?.[field.code] && (
                        <p className="text-xs text-destructive mt-1">
                          {errors[rowIndex][field.code]}
                        </p>
                      )}
                    </div>
                  </td>
                ))}
                <td className="py-2 px-2">
                  {values.length > minRows && (
                    <Button
                      type="button"
                      variant="ghost"
                      size="icon"
                      className="h-8 w-8 text-muted-foreground hover:text-destructive"
                      onClick={() => handleRemoveRow(rowIndex)}
                    >
                      <Trash2 className="h-4 w-4" />
                    </Button>
                  )}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {values.length === 0 && (
        <div className="text-center py-8 text-muted-foreground border border-dashed rounded-lg">
          <p className="mb-2">Henüz kayıt eklenmedi</p>
          <Button
            type="button"
            variant="outline"
            size="sm"
            onClick={handleAddRow}
            className="gap-2"
          >
            <Plus className="h-4 w-4" />
            {buttonText}
          </Button>
        </div>
      )}
    </div>
  )
}
