'use client';

import { useState } from 'react';
import { CreditCard, Banknote, Building2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { cn } from '@/lib/utils';
import { useCheckoutStore } from '@/stores/checkoutStore';
import type { PaymentMethod } from '@/types';

const paymentOptions: {
  id: PaymentMethod;
  name: string;
  description: string;
  icon: React.ComponentType<{ className?: string }>;
}[] = [
  {
    id: 'CreditCard',
    name: 'Credit/Debit Card',
    description: 'Pay securely with your card',
    icon: CreditCard,
  },
  {
    id: 'CashOnDelivery',
    name: 'Cash on Delivery',
    description: 'Pay when you receive your order',
    icon: Banknote,
  },
  {
    id: 'BankTransfer',
    name: 'Bank Transfer',
    description: 'Direct bank transfer',
    icon: Building2,
  },
];

interface PaymentMethodsProps {
  onSubmit: (method: PaymentMethod) => void;
  onBack: () => void;
  isLoading?: boolean;
}

export function PaymentMethods({ onSubmit, onBack, isLoading }: PaymentMethodsProps) {
  const { paymentMethod: savedMethod } = useCheckoutStore();
  const [selectedMethod, setSelectedMethod] = useState<PaymentMethod>(
    savedMethod || 'CashOnDelivery'
  );

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    onSubmit(selectedMethod);
  };

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div className="space-y-4">
        {paymentOptions.map((option) => {
          const Icon = option.icon;
          const isSelected = selectedMethod === option.id;

          return (
            <div
              key={option.id}
              className={cn(
                'cursor-pointer rounded-lg border-2 p-4 transition-colors',
                isSelected
                  ? 'border-slate-900 bg-slate-50'
                  : 'border-slate-200 hover:border-slate-300'
              )}
              onClick={() => setSelectedMethod(option.id)}
            >
              <div className="flex items-center gap-4">
                <div
                  className={cn(
                    'flex h-10 w-10 items-center justify-center rounded-full',
                    isSelected ? 'bg-slate-900 text-white' : 'bg-slate-100 text-slate-600'
                  )}
                >
                  <Icon className="h-5 w-5" />
                </div>
                <div className="flex-1">
                  <p className="font-medium">{option.name}</p>
                  <p className="text-sm text-slate-500">{option.description}</p>
                </div>
                <div
                  className={cn(
                    'h-5 w-5 rounded-full border-2',
                    isSelected ? 'border-slate-900 bg-slate-900' : 'border-slate-300'
                  )}
                >
                  {isSelected && (
                    <div className="flex h-full items-center justify-center">
                      <div className="h-2 w-2 rounded-full bg-white" />
                    </div>
                  )}
                </div>
              </div>

              {/* Card details form (shown when card is selected) */}
              {isSelected && option.id === 'CreditCard' && (
                <div className="mt-4 space-y-4 border-t pt-4">
                  <p className="text-sm text-slate-600">
                    You will be redirected to PayHere to complete your payment securely.
                  </p>
                </div>
              )}

              {/* Bank transfer details */}
              {isSelected && option.id === 'BankTransfer' && (
                <div className="mt-4 space-y-2 border-t pt-4">
                  <p className="text-sm text-slate-600">
                    Bank transfer details will be provided after order confirmation.
                  </p>
                </div>
              )}

              {/* COD notice */}
              {isSelected && option.id === 'CashOnDelivery' && (
                <div className="mt-4 border-t pt-4">
                  <p className="text-sm text-slate-600">
                    Please have the exact amount ready when your order arrives.
                  </p>
                </div>
              )}
            </div>
          );
        })}
      </div>

      <div className="flex gap-4">
        <Button type="button" variant="outline" className="flex-1" onClick={onBack}>
          Back
        </Button>
        <Button type="submit" className="flex-1" disabled={isLoading}>
          {isLoading ? 'Processing...' : 'Continue to Review'}
        </Button>
      </div>
    </form>
  );
}
