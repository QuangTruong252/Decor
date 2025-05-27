'use client';

import React, { useState, useEffect } from 'react';
import { Plus, Minus } from 'lucide-react';
import { Button } from '@/components/ui/Button';

interface QuantitySelectorProps {
  value: number;
  onChange: (quantity: number) => void;
  min?: number;
  max?: number;
  step?: number;
  disabled?: boolean;
  loading?: boolean;
  size?: 'sm' | 'md' | 'lg';
  variant?: 'default' | 'outline' | 'minimal';
  className?: string;
  showInput?: boolean;
  allowZero?: boolean;
}

export function QuantitySelector({
  value,
  onChange,
  min = 1,
  max = 99,
  step = 1,
  disabled = false,
  loading = false,
  size = 'md',
  variant = 'default',
  className = '',
  showInput = false,
  allowZero = false,
}: QuantitySelectorProps) {
  const [localValue, setLocalValue] = useState(value);
  const [inputValue, setInputValue] = useState(value.toString());

  // Sync local value with prop value
  useEffect(() => {
    setLocalValue(value);
    setInputValue(value.toString());
  }, [value]);

  const sizes = {
    sm: {
      button: 'h-6 w-6 p-0',
      text: 'text-xs',
      input: 'h-6 w-12 text-xs',
      container: 'gap-1',
    },
    md: {
      button: 'h-8 w-8 p-0',
      text: 'text-sm',
      input: 'h-8 w-16 text-sm',
      container: 'gap-2',
    },
    lg: {
      button: 'h-10 w-10 p-0',
      text: 'text-base',
      input: 'h-10 w-20 text-base',
      container: 'gap-3',
    },
  };

  const variants = {
    default: 'border border-gray-300 rounded-md bg-white',
    outline: 'border border-gray-300 rounded-md bg-transparent',
    minimal: 'bg-gray-50 rounded-md',
  };

  const handleIncrement = () => {
    const newValue = Math.min(localValue + step, max);
    if (newValue !== localValue) {
      setLocalValue(newValue);
      onChange(newValue);
    }
  };

  const handleDecrement = () => {
    const minValue = allowZero ? 0 : min;
    const newValue = Math.max(localValue - step, minValue);
    if (newValue !== localValue) {
      setLocalValue(newValue);
      onChange(newValue);
    }
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const inputVal = e.target.value;
    setInputValue(inputVal);

    // Parse and validate the input
    const numValue = parseInt(inputVal, 10);
    if (!isNaN(numValue)) {
      const minValue = allowZero ? 0 : min;
      const clampedValue = Math.max(minValue, Math.min(numValue, max));
      if (clampedValue !== localValue) {
        setLocalValue(clampedValue);
        onChange(clampedValue);
      }
    }
  };

  const handleInputBlur = () => {
    // Reset input to current value if invalid
    setInputValue(localValue.toString());
  };

  const canDecrement = !disabled && !loading && localValue > (allowZero ? 0 : min);
  const canIncrement = !disabled && !loading && localValue < max;

  if (showInput) {
    return (
      <div className={`flex items-center ${sizes[size].container} ${className}`}>
        <Button
          variant="outline"
          size="sm"
          onClick={handleDecrement}
          disabled={!canDecrement}
          className={sizes[size].button}
        >
          <Minus className="w-3 h-3" />
        </Button>

        <input
          type="number"
          value={inputValue}
          onChange={handleInputChange}
          onBlur={handleInputBlur}
          min={allowZero ? 0 : min}
          max={max}
          step={step}
          disabled={disabled || loading}
          className={`${sizes[size].input} px-2 border border-gray-300 rounded text-center focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent disabled:bg-gray-100 disabled:cursor-not-allowed`}
        />

        <Button
          variant="outline"
          size="sm"
          onClick={handleIncrement}
          disabled={!canIncrement}
          className={sizes[size].button}
        >
          <Plus className="w-3 h-3" />
        </Button>
      </div>
    );
  }

  return (
    <div className={`flex items-center ${variants[variant]} ${className}`}>
      <Button
        variant="ghost"
        size="sm"
        onClick={handleDecrement}
        disabled={!canDecrement}
        className={`${sizes[size].button} hover:bg-gray-100`}
      >
        <Minus className="w-3 h-3" />
      </Button>

      <div className={`px-3 py-1 ${sizes[size].text} font-medium min-w-[2rem] text-center flex items-center justify-center`}>
        {loading ? (
          <div className="w-3 h-3 border border-gray-400 border-t-transparent rounded-full animate-spin" />
        ) : (
          localValue
        )}
      </div>

      <Button
        variant="ghost"
        size="sm"
        onClick={handleIncrement}
        disabled={!canIncrement}
        className={`${sizes[size].button} hover:bg-gray-100`}
      >
        <Plus className="w-3 h-3" />
      </Button>
    </div>
  );
}

export default QuantitySelector;
