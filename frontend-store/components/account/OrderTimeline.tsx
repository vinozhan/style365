import { Check, Package, Truck, Home, Clock } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { OrderStatus } from '@/types';

interface OrderTimelineProps {
  status: OrderStatus;
  orderDate: string;
  shippedDate?: string;
  deliveredDate?: string;
}

const statusSteps = [
  { status: 'Pending', label: 'Order Placed', icon: Clock },
  { status: 'Processing', label: 'Processing', icon: Package },
  { status: 'Shipped', label: 'Shipped', icon: Truck },
  { status: 'Delivered', label: 'Delivered', icon: Home },
] as const;

const statusOrder: Record<OrderStatus, number> = {
  Pending: 0,
  Processing: 1,
  Shipped: 2,
  Delivered: 3,
  Cancelled: -1,
  Refunded: -1,
};

export function OrderTimeline({
  status,
  orderDate,
  shippedDate,
  deliveredDate,
}: OrderTimelineProps) {
  const currentIndex = statusOrder[status];

  if (status === 'Cancelled' || status === 'Refunded') {
    return (
      <div className="rounded-lg bg-red-50 p-4 text-center">
        <p className="font-medium text-red-800">
          Order {status === 'Cancelled' ? 'Cancelled' : 'Refunded'}
        </p>
      </div>
    );
  }

  return (
    <div className="relative">
      <div className="flex justify-between">
        {statusSteps.map((step, index) => {
          const Icon = step.icon;
          const isComplete = index <= currentIndex;
          const isCurrent = index === currentIndex;

          return (
            <div
              key={step.status}
              className={cn('flex flex-col items-center', index < statusSteps.length - 1 && 'flex-1')}
            >
              {/* Connector line */}
              {index < statusSteps.length - 1 && (
                <div
                  className={cn(
                    'absolute top-5 h-0.5 w-full -translate-y-1/2',
                    index < currentIndex ? 'bg-green-500' : 'bg-slate-200'
                  )}
                  style={{
                    left: `calc(${(100 / (statusSteps.length - 1)) * index}% + 20px)`,
                    width: `calc(${100 / (statusSteps.length - 1)}% - 40px)`,
                  }}
                />
              )}

              {/* Icon */}
              <div
                className={cn(
                  'relative z-10 flex h-10 w-10 items-center justify-center rounded-full',
                  isComplete ? 'bg-green-500 text-white' : 'bg-slate-200 text-slate-400'
                )}
              >
                {isComplete && !isCurrent ? (
                  <Check className="h-5 w-5" />
                ) : (
                  <Icon className="h-5 w-5" />
                )}
              </div>

              {/* Label */}
              <p
                className={cn(
                  'mt-2 text-center text-xs font-medium',
                  isCurrent ? 'text-green-600' : isComplete ? 'text-slate-900' : 'text-slate-400'
                )}
              >
                {step.label}
              </p>

              {/* Date */}
              <p className="mt-1 text-center text-xs text-slate-500">
                {step.status === 'Pending' && orderDate && new Date(orderDate).toLocaleDateString()}
                {step.status === 'Shipped' && shippedDate && new Date(shippedDate).toLocaleDateString()}
                {step.status === 'Delivered' && deliveredDate && new Date(deliveredDate).toLocaleDateString()}
              </p>
            </div>
          );
        })}
      </div>
    </div>
  );
}
