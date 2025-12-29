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
import {
  getGeoLocationTypes,
  getGeoLocationsByType,
  getGeoLocationsByParent,
  type GeoLocationType,
  type GeoLocation,
} from '@/lib/api'

interface GeoLocationSelectProps {
  value?: string // The final selected GeoLocation ID
  onChange: (geoLocationId: string | undefined, pathName?: string) => void
  label?: string
  placeholder?: string
  maxDepth?: number // Maximum depth to show (e.g., 4 for Ülke/Şehir/İlçe/Mahalle)
  required?: boolean
  disabled?: boolean
  className?: string
}

interface LocationLevel {
  typeId: string
  typeName: string
  selectedId?: string
  locations: GeoLocation[]
  isLoading: boolean
}

export function GeoLocationSelect({
  value,
  onChange,
  label,
  placeholder = 'Seçiniz',
  maxDepth = 4,
  required = false,
  disabled = false,
  className,
}: GeoLocationSelectProps) {
  const [locationTypes, setLocationTypes] = useState<GeoLocationType[]>([])
  const [levels, setLevels] = useState<LocationLevel[]>([])
  const [isInitializing, setIsInitializing] = useState(true)

  // Load location types on mount
  useEffect(() => {
    async function loadTypes() {
      try {
        const types = await getGeoLocationTypes()
        // Sort by displayOrder and limit to maxDepth
        const sortedTypes = types
          .sort((a, b) => a.displayOrder - b.displayOrder)
          .slice(0, maxDepth)
        setLocationTypes(sortedTypes)

        // Initialize levels
        if (sortedTypes.length > 0) {
          const firstType = sortedTypes[0]
          const firstLocations = await getGeoLocationsByType(firstType.id)
          setLevels([
            {
              typeId: firstType.id,
              typeName: firstType.name,
              locations: firstLocations,
              isLoading: false,
            },
          ])
        }
      } catch (error) {
        console.error('Failed to load location types:', error)
      } finally {
        setIsInitializing(false)
      }
    }
    loadTypes()
  }, [maxDepth])

  // Handle selection change at a specific level
  const handleLevelChange = async (levelIndex: number, selectedId: string | undefined) => {
    const newLevels = [...levels]

    // Update selection at current level
    newLevels[levelIndex] = {
      ...newLevels[levelIndex],
      selectedId,
    }

    // Remove all levels after this one
    newLevels.splice(levelIndex + 1)

    // If a value was selected and there are more types, load the next level
    if (selectedId && levelIndex + 1 < locationTypes.length) {
      const nextType = locationTypes[levelIndex + 1]

      // Add loading state for next level
      newLevels.push({
        typeId: nextType.id,
        typeName: nextType.name,
        locations: [],
        isLoading: true,
      })
      setLevels(newLevels)

      try {
        const childLocations = await getGeoLocationsByParent(selectedId)
        setLevels((prev) => {
          const updated = [...prev]
          if (updated[levelIndex + 1]) {
            updated[levelIndex + 1] = {
              ...updated[levelIndex + 1],
              locations: childLocations,
              isLoading: false,
            }
          }
          return updated
        })
      } catch (error) {
        console.error('Failed to load child locations:', error)
        setLevels((prev) => {
          const updated = [...prev]
          if (updated[levelIndex + 1]) {
            updated[levelIndex + 1] = {
              ...updated[levelIndex + 1],
              isLoading: false,
            }
          }
          return updated
        })
      }
    } else {
      setLevels(newLevels)
    }

    // Find the deepest selected location for the callback
    const deepestSelectedLevel = newLevels.reduce((deepest, level, idx) => {
      if (level.selectedId) return idx
      return deepest
    }, -1)

    if (deepestSelectedLevel >= 0) {
      const selectedLocation = newLevels[deepestSelectedLevel].locations.find(
        (loc) => loc.id === newLevels[deepestSelectedLevel].selectedId
      )
      onChange(newLevels[deepestSelectedLevel].selectedId, selectedLocation?.pathName)
    } else {
      onChange(undefined, undefined)
    }
  }

  if (isInitializing) {
    return (
      <div className={className}>
        {label && (
          <Label className="mb-2 block">
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

  return (
    <div className={className}>
      {label && (
        <Label className="mb-2 block">
          {label}
          {required && <span className="text-destructive ml-1">*</span>}
        </Label>
      )}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-3">
        {levels.map((level, index) => (
          <div key={level.typeId}>
            <Label className="text-xs text-muted-foreground mb-1 block">
              {level.typeName}
            </Label>
            <Select
              value={level.selectedId || ''}
              onValueChange={(val) => handleLevelChange(index, val || undefined)}
              disabled={disabled || level.isLoading}
            >
              <SelectTrigger className="w-full">
                {level.isLoading ? (
                  <div className="flex items-center gap-2">
                    <Loader2 className="h-4 w-4 animate-spin" />
                    <span>Yükleniyor...</span>
                  </div>
                ) : (
                  <SelectValue placeholder={`${level.typeName} seçiniz`} />
                )}
              </SelectTrigger>
              <SelectContent>
                {level.locations.map((location) => (
                  <SelectItem key={location.id} value={location.id}>
                    {location.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>
        ))}
      </div>
    </div>
  )
}
