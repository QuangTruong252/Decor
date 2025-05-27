'use client';

import React from 'react';

interface Step {
  id: number;
  title: string;
  description: string;
}

interface CheckoutProgressProps {
  currentStep: number;
  steps: Step[];
  className?: string;
}

export default function CheckoutProgress({ currentStep, steps, className = '' }: CheckoutProgressProps) {
  const progressPercentage = ((currentStep - 1) / (steps.length - 1)) * 100;

  return (
    <div className={`w-full ${className}`}>
      {/* Progress bar */}
      <div className="mb-8">
        <div className="flex justify-between items-center mb-2">
          <span className="text-sm font-medium text-gray-700">
            Step {currentStep} of {steps.length}
          </span>
          <span className="text-sm text-gray-500">
            {Math.round(progressPercentage)}% Complete
          </span>
        </div>
        
        <div className="w-full bg-gray-200 rounded-full h-2">
          <div
            className="bg-primary h-2 rounded-full transition-all duration-300 ease-in-out"
            style={{ width: `${progressPercentage}%` }}
          />
        </div>
      </div>

      {/* Step indicators */}
      <div className="flex justify-between">
        {steps.map((step) => {
          const isCompleted = step.id < currentStep;
          const isCurrent = step.id === currentStep;
          const isUpcoming = step.id > currentStep;

          return (
            <div key={step.id} className="flex flex-col items-center flex-1">
              {/* Step circle */}
              <div className={`
                w-8 h-8 rounded-full flex items-center justify-center text-sm font-medium transition-all duration-200
                ${isCompleted 
                  ? 'bg-primary text-white' 
                  : isCurrent
                  ? 'bg-primary text-white ring-4 ring-primary ring-opacity-20'
                  : 'bg-gray-200 text-gray-500'
                }
              `}>
                {isCompleted ? (
                  <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                    <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
                  </svg>
                ) : (
                  step.id
                )}
              </div>

              {/* Step title */}
              <div className="mt-2 text-center">
                <div className={`text-xs font-medium ${
                  isCurrent ? 'text-primary' : isCompleted ? 'text-gray-900' : 'text-gray-500'
                }`}>
                  {step.title}
                </div>
                {/* Step description - only show on larger screens */}
                <div className="hidden sm:block text-xs text-gray-500 mt-1 max-w-20">
                  {step.description}
                </div>
              </div>

              {/* Connector line */}
              {step.id < steps.length && (
                <div className="hidden sm:block absolute top-4 left-1/2 w-full h-0.5 bg-gray-200 -z-10">
                  <div 
                    className={`h-full transition-all duration-300 ${
                      isCompleted ? 'bg-primary' : 'bg-gray-200'
                    }`}
                    style={{ width: isCompleted ? '100%' : '0%' }}
                  />
                </div>
              )}
            </div>
          );
        })}
      </div>

      {/* Current step info */}
      <div className="mt-6 text-center">
        <h2 className="text-lg font-semibold text-gray-900">
          {steps.find(step => step.id === currentStep)?.title}
        </h2>
        <p className="text-sm text-gray-600 mt-1">
          {steps.find(step => step.id === currentStep)?.description}
        </p>
      </div>
    </div>
  );
}
