'use client'

import { cn } from '@/lib/utils'

interface StepIndicatorProps {
  currentStep: number
  totalSteps?: number
}

export function StepIndicator({ currentStep, totalSteps = 4 }: StepIndicatorProps) {
  return (
    <div className="relative flex items-center justify-center py-8">
      {/* Background line */}
      <div className="absolute left-0 right-0 h-0.5 bg-gray-200" />

      {/* Steps */}
      <div className="relative flex items-center justify-center gap-16">
        {Array.from({ length: totalSteps }, (_, index) => {
          const step = index + 1
          const isActive = step === currentStep
          const isCompleted = step < currentStep

          return (
            <div
              key={step}
              className={cn(
                'w-8 h-8 rounded-sm flex items-center justify-center text-base font-semibold transition-colors',
                isCompleted && 'bg-primary-500 text-white',
                isActive && 'bg-white border-2 border-primary-500 text-primary-500',
                !isActive && !isCompleted && 'bg-gray-200 text-white'
              )}
            >
              {step}
            </div>
          )
        })}
      </div>
    </div>
  )
}
