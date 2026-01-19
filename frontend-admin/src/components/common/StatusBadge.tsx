import { Badge } from '@/components/ui/badge';
import type { OrderStatus, PaymentStatus } from '@/types';

const orderStatusConfig: Record<OrderStatus, { label: string; variant: 'default' | 'secondary' | 'destructive' | 'success' | 'warning' | 'info' }> = {
  Pending: { label: 'Pending', variant: 'warning' },
  Confirmed: { label: 'Confirmed', variant: 'info' },
  Processing: { label: 'Processing', variant: 'info' },
  Shipped: { label: 'Shipped', variant: 'info' },
  Delivered: { label: 'Delivered', variant: 'success' },
  Cancelled: { label: 'Cancelled', variant: 'destructive' },
  Refunded: { label: 'Refunded', variant: 'secondary' },
};

const paymentStatusConfig: Record<PaymentStatus, { label: string; variant: 'default' | 'secondary' | 'destructive' | 'success' | 'warning' }> = {
  Pending: { label: 'Pending', variant: 'warning' },
  Paid: { label: 'Paid', variant: 'success' },
  Failed: { label: 'Failed', variant: 'destructive' },
  Refunded: { label: 'Refunded', variant: 'secondary' },
};

interface OrderStatusBadgeProps {
  status: OrderStatus;
}

export function OrderStatusBadge({ status }: OrderStatusBadgeProps) {
  const config = orderStatusConfig[status] || { label: status, variant: 'secondary' as const };
  return <Badge variant={config.variant}>{config.label}</Badge>;
}

interface PaymentStatusBadgeProps {
  status: PaymentStatus;
}

export function PaymentStatusBadge({ status }: PaymentStatusBadgeProps) {
  const config = paymentStatusConfig[status] || { label: status, variant: 'secondary' as const };
  return <Badge variant={config.variant}>{config.label}</Badge>;
}

interface StockBadgeProps {
  quantity: number;
  lowThreshold?: number;
}

export function StockBadge({ quantity, lowThreshold = 10 }: StockBadgeProps) {
  if (quantity === 0) {
    return <Badge variant="destructive">Out of Stock</Badge>;
  }
  if (quantity <= lowThreshold) {
    return <Badge variant="warning">Low Stock ({quantity})</Badge>;
  }
  return <Badge variant="success">In Stock ({quantity})</Badge>;
}

interface ActiveBadgeProps {
  isActive: boolean;
}

export function ActiveBadge({ isActive }: ActiveBadgeProps) {
  return <Badge variant={isActive ? 'success' : 'secondary'}>{isActive ? 'Active' : 'Inactive'}</Badge>;
}
