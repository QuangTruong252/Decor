"use client"

import * as React from "react"
import { format } from "date-fns"
import { Calendar as CalendarIcon } from "lucide-react"

import { cn } from "@/lib/utils"
import { Button } from "@/components/ui/button"
import { Calendar } from "@/components/ui/calendar"
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover"

interface DatePickerProps {
  date?: Date
  onDateChange?: (date: Date | undefined) => void
  placeholder?: string
  disabled?: boolean
  className?: string
}

export function DatePicker({
  date,
  onDateChange,
  placeholder = "Pick a date",
  disabled = false,
  className,
}: DatePickerProps) {
  return (
    <Popover>
      <PopoverTrigger asChild>
        <Button
          variant={"outline"}
          className={cn(
            "w-full justify-start text-left font-normal",
            !date && "text-muted-foreground",
            className
          )}
          disabled={disabled}
        >
          <CalendarIcon className="mr-2 h-4 w-4" />
          {date ? format(date, "PPP") : <span>{placeholder}</span>}
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-auto p-0">
        <Calendar
          mode="single"
          selected={date}
          onSelect={onDateChange}
          initialFocus
        />
      </PopoverContent>
    </Popover>
  )
}

interface DateRangePickerProps {
  from?: Date
  to?: Date
  onDateRangeChange?: (range: { from?: Date; to?: Date }) => void
  placeholder?: string
  disabled?: boolean
  className?: string
}

export function DateRangePicker({
  from,
  to,
  onDateRangeChange,
  placeholder = "Pick a date range",
  disabled = false,
  className,
}: DateRangePickerProps) {
  const [isOpen, setIsOpen] = React.useState(false)

  const handleSelect = (range: { from?: Date; to?: Date } | undefined) => {
    if (range) {
      onDateRangeChange?.(range)
      // Close popover when both dates are selected
      if (range.from && range.to) {
        setIsOpen(false)
      }
    }
  }

  const formatDateRange = () => {
    if (from && to) {
      return `${format(from, "LLL dd, y")} - ${format(to, "LLL dd, y")}`
    }
    if (from) {
      return `${format(from, "LLL dd, y")} - ...`
    }
    return placeholder
  }

  return (
    <Popover open={isOpen} onOpenChange={setIsOpen}>
      <PopoverTrigger asChild>
        <Button
          variant={"outline"}
          className={cn(
            "w-full justify-start text-left font-normal",
            !from && !to && "text-muted-foreground",
            className
          )}
          disabled={disabled}
        >
          <CalendarIcon className="mr-2 h-4 w-4" />
          {formatDateRange()}
        </Button>
      </PopoverTrigger>
      <PopoverContent className="w-auto p-0" align="start">
        <Calendar
          initialFocus
          mode="range"
          defaultMonth={from}
          selected={{ from, to }}
          onSelect={handleSelect}
          numberOfMonths={2}
        />
      </PopoverContent>
    </Popover>
  )
}
