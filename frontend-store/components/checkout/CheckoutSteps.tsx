'use client';

import { Check } from 'lucide-react';
import { cn } from '@/lib/utils';

interface CheckoutStepsProps {
  currentStep: 'shipping' | 'payment' | 'review';
}

const steps = [
  { id: 'shipping', name: 'Shipping', description: 'Delivery address' },
  { id: 'payment', name: 'Payment', description: 'Payment method' },
  { id: 'review', name: 'Review', description: 'Confirm order' },
] as const;

export function CheckoutSteps({ currentStep }: CheckoutStepsProps) {
  const currentIndex = steps.findIndex((s) => s.id === currentStep);

  return (
    <nav aria-label="Progress">
      <ol className="flex items-center">
        {steps.map((step, index) => {
          const isComplete = index < currentIndex;
          const isCurrent = index === currentIndex;

          return (
            <li
              key={step.id}
              className={cn('relative', index !== steps.length - 1 && 'flex-1')}
            >
              <div className="flex items-center">
                <div
                  className={cn(
                    'relative flex h-10 w-10 items-center justify-center rounded-full border-2 transition-colors',
                    isComplete && 'border-slate-900 bg-slate-900',
                    isCurrent && 'border-slate-900 bg-white',
                    !isComplete && !isCurrent && 'border-slate-300 bg-white'
                  )}
                >
                  {isComplete ? (
                    <Check className="h-5 w-5 text-white" />
                  ) : (
                    <span
                      className={cn(
                        'text-sm font-medium',
                        isCurrent ? 'text-slate-900' : 'text-slate-500'
                      )}
                    >
                      {index + 1}
                    </span>
                  )}
                </div>

                {/* Step label */}
                <div className="ml-4 hidden min-w-0 sm:block">
                  <p
                    className={cn(
                      'text-sm font-medium',
                      isCurrent || isComplete ? 'text-slate-900' : 'text-slate-500'
                    )}
                  >
                    {step.name}
                  </p>
                  <p className="text-xs text-slate-500">{step.description}</p>
                </div>

                {/* Connector line */}
                {index !== steps.length - 1 && (
                  <div
                    className={cn(
                      'ml-4 hidden h-0.5 flex-1 sm:block',
                      isComplete ? 'bg-slate-900' : 'bg-slate-200'
                    )}
                  />
                )}
              </div>
            </li>
          );
        })}
      </ol>
    </nav>
  );
}
