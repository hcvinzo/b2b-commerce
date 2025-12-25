'use client'

import { useCallback, useState } from 'react'
import { cn } from '@/lib/utils'
import { Upload, X, FileText } from 'lucide-react'

interface FileUploadProps {
  label: string
  accept?: string
  onChange?: (file: File | null) => void
  error?: string
  value?: File | null
}

export function FileUpload({ label, accept = '.pdf,.jpg,.jpeg,.png', onChange, error, value }: FileUploadProps) {
  const [dragActive, setDragActive] = useState(false)
  const [file, setFile] = useState<File | null>(value || null)

  const handleDrag = useCallback((e: React.DragEvent) => {
    e.preventDefault()
    e.stopPropagation()
    if (e.type === 'dragenter' || e.type === 'dragover') {
      setDragActive(true)
    } else if (e.type === 'dragleave') {
      setDragActive(false)
    }
  }, [])

  const handleDrop = useCallback((e: React.DragEvent) => {
    e.preventDefault()
    e.stopPropagation()
    setDragActive(false)

    if (e.dataTransfer.files && e.dataTransfer.files[0]) {
      const droppedFile = e.dataTransfer.files[0]
      setFile(droppedFile)
      onChange?.(droppedFile)
    }
  }, [onChange])

  const handleChange = useCallback((e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files[0]) {
      const selectedFile = e.target.files[0]
      setFile(selectedFile)
      onChange?.(selectedFile)
    }
  }, [onChange])

  const removeFile = useCallback(() => {
    setFile(null)
    onChange?.(null)
  }, [onChange])

  return (
    <div className="w-full">
      <label className="input-label">{label}</label>

      {file ? (
        <div className="flex items-center justify-between p-3 border border-gray-200 rounded-md bg-gray-50">
          <div className="flex items-center gap-2">
            <FileText className="w-5 h-5 text-primary" />
            <span className="text-sm text-gray-700 truncate max-w-[180px]">{file.name}</span>
          </div>
          <button
            type="button"
            onClick={removeFile}
            className="p-1 hover:bg-gray-200 rounded-full transition-colors"
          >
            <X className="w-4 h-4 text-gray-500" />
          </button>
        </div>
      ) : (
        <div
          onDragEnter={handleDrag}
          onDragLeave={handleDrag}
          onDragOver={handleDrag}
          onDrop={handleDrop}
          className={cn(
            'relative border-2 border-dashed rounded-md p-4 text-center cursor-pointer transition-colors',
            dragActive ? 'border-primary bg-primary-50' : 'border-gray-200 hover:border-primary hover:bg-gray-50',
            error && 'border-red-500'
          )}
        >
          <input
            type="file"
            accept={accept}
            onChange={handleChange}
            className="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
          />
          <Upload className="w-8 h-8 mx-auto mb-2 text-gray-400" />
          <p className="text-sm text-gray-500">
            Dosya yükleyin veya sürükleyin
          </p>
          <p className="text-xs text-gray-400 mt-1">
            PDF, JPG, PNG (maks. 10MB)
          </p>
        </div>
      )}

      {error && <p className="input-error">{error}</p>}
    </div>
  )
}
