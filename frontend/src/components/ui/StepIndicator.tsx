'use client'

import { cn } from '@/lib/utils'
import { Check } from 'lucide-react'

interface StepIndicatorProps {
  currentStep: number
  totalSteps?: number
}

export function StepIndicator({ currentStep, totalSteps = 4 }: StepIndicatorProps) {
  return (
    <div className="flex items-center justify-center gap-0">
      {Array.from({ length: totalSteps }, (_, index) => {
        const step = index + 1
        const isActive = step === currentStep
        const isCompleted = step < currentStep

        return (
          <div key={step} className="flex items-center">
            {/* Step Circle */}
            <div
              className={cn(
                'w-10 h-10 rounded-full flex items-center justify-center text-sm font-medium border-2 transition-colors',
                isActive && 'bg-primary border-primary text-white',
                isCompleted && 'bg-primary border-primary text-white',
                !isActive && !isCompleted && 'bg-white border-gray-200 text-gray-500'
              )}
            >
              {isCompleted ? <Check className="w-5 h-5" /> : step}
            </div>

            {/* Connection Line */}
            {step < totalSteps && (
              <div
                className={cn(
                  'w-16 h-0.5',
                  isCompleted ? 'bg-primary' : 'bg-gray-200'
                )}
              />
            )}
          </div>
        )
      })}
    </div>
  )
}
