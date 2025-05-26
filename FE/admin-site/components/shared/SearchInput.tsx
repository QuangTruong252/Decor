"use client"

import * as React from "react"
import { Search, X } from "lucide-react"
import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"
import { cn } from "@/lib/utils"
import { debounce } from "@/lib/query-utils"

interface SearchInputProps {
  value: string
  onChange: (value: string) => void
  placeholder?: string
  debounceMs?: number
  className?: string
  disabled?: boolean
}

export function SearchInput({
  value,
  onChange,
  placeholder = "Search...",
  debounceMs = 300,
  className,
  disabled = false,
}: SearchInputProps) {
  const [localValue, setLocalValue] = React.useState(value)

  // Create debounced onChange function
  const debouncedOnChange = React.useMemo(
    () => debounce(onChange, debounceMs),
    [onChange, debounceMs]
  )

  // Update local value when external value changes
  React.useEffect(() => {
    setLocalValue(value)
  }, [value])

  // Handle input change
  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const newValue = e.target.value
    setLocalValue(newValue)
    debouncedOnChange(newValue)
  }

  // Handle clear
  const handleClear = () => {
    setLocalValue("")
    onChange("")
  }

  return (
    <div className={cn("relative", className)}>
      <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
      <Input
        type="text"
        placeholder={placeholder}
        value={localValue}
        onChange={handleInputChange}
        disabled={disabled}
        className="pl-9 pr-9"
      />
      {localValue && (
        <Button
          type="button"
          variant="ghost"
          size="sm"
          className="absolute right-1 top-1/2 h-7 w-7 -translate-y-1/2 p-0 hover:bg-transparent"
          onClick={handleClear}
          disabled={disabled}
        >
          <X className="h-4 w-4" />
          <span className="sr-only">Clear search</span>
        </Button>
      )}
    </div>
  )
}
