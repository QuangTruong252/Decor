'use client';

import React from 'react';

interface Step {
  id: number;
  title: string;
  description: string;
}

interface CheckoutStepsProps {
  steps: Step[];
  currentStep: number;
  onStepClick?: (stepId: number) => void;
  allowStepNavigation?: boolean;
}

export default function CheckoutSteps({ 
  steps, 
  currentStep, 
  onStepClick,
  allowStepNavigation = false 
}: CheckoutStepsProps) {
  const handleStepClick = (stepId: number) => {
    if (allowStepNavigation && onStepClick) {
      onStepClick(stepId);
    }
  };

  const getStepStatus = (stepId: number) => {
    if (stepId < currentStep) return 'completed';
    if (stepId === currentStep) return 'current';
    return 'upcoming';
  };

  const getStepIcon = (stepId: number, status: string) => {
    if (status === 'completed') {
      return (
        <svg className="w-5 h-5 text-white" fill="currentColor" viewBox="0 0 20 20">
          <path fillRule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clipRule="evenodd" />
        </svg>
      );
    }
    return <span className="text-sm font-medium">{stepId}</span>;
  };

  return (
    <nav aria-label="Progress">
      <ol className="flex items-center">
        {steps.map((step, stepIdx) => {
          const status = getStepStatus(step.id);
          const isClickable = allowStepNavigation && (status === 'completed' || status === 'current');
          
          return (
            <li key={step.id} className={`relative ${stepIdx !== steps.length - 1 ? 'pr-8 sm:pr-20' : ''}`}>
              {/* Connector line */}
              {stepIdx !== steps.length - 1 && (
                <div className="absolute inset-0 flex items-center" aria-hidden="true">
                  <div className={`h-0.5 w-full ${
                    status === 'completed' ? 'bg-primary' : 'bg-gray-200'
                  }`} />
                </div>
              )}
              
              {/* Step content */}
              <div
                className={`relative flex items-center justify-center ${
                  isClickable ? 'cursor-pointer' : ''
                }`}
                onClick={() => handleStepClick(step.id)}
              >
                {/* Step circle */}
                <div className={`
                  flex items-center justify-center w-10 h-10 rounded-full border-2 transition-colors
                  ${status === 'completed' 
                    ? 'bg-primary border-primary' 
                    : status === 'current'
                    ? 'border-primary bg-white text-primary'
                    : 'border-gray-300 bg-white text-gray-500'
                  }
                  ${isClickable ? 'hover:border-primary-dark' : ''}
                `}>
                  {getStepIcon(step.id, status)}
                </div>
                
                {/* Step label */}
                <div className="absolute top-12 left-1/2 transform -translate-x-1/2 text-center min-w-max">
                  <div className={`text-sm font-medium ${
                    status === 'current' ? 'text-primary' : 'text-gray-900'
                  }`}>
                    {step.title}
                  </div>
                  <div className="text-xs text-gray-500 mt-1 hidden sm:block">
                    {step.description}
                  </div>
                </div>
              </div>
            </li>
          );
        })}
      </ol>
    </nav>
  );
}
