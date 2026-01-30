'use client';

import { useState } from 'react';
import { Package } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { OrderCard } from '@/components/account';
import { useOrders } from '@/features/orders/hooks/useOrders';

export default function OrdersPage() {
  const [page, setPage] = useState(1);
  const pageSize = 10;
  const { data, isLoading } = useOrders(page, pageSize);

  if (isLoading) {
    return (
      <div>
        <h1 className="mb-6 text-2xl font-bold">My Orders</h1>
        <div className="space-y-4">
          {Array.from({ length: 3 }).map((_, i) => (
            <Skeleton key={i} className="h-40 rounded-lg" />
          ))}
        </div>
      </div>
    );
  }

  if (!data || data.items.length === 0) {
    return (
      <div>
        <h1 className="mb-6 text-2xl font-bold">My Orders</h1>
        <div className="flex flex-col items-center justify-center py-12 text-center">
          <div className="mb-4 rounded-full bg-slate-100 p-4">
            <Package className="h-8 w-8 text-slate-400" />
          </div>
          <h2 className="text-lg font-semibold">No orders yet</h2>
          <p className="mt-2 text-slate-500">
            When you place orders, they will appear here.
          </p>
          <Button className="mt-6" asChild>
            <a href="/products">Start Shopping</a>
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold">My Orders</h1>

      <div className="space-y-4">
        {data.items.map((order) => (
          <OrderCard key={order.id} order={order} />
        ))}
      </div>

      {/* Pagination */}
      {data.totalPages > 1 && (
        <div className="mt-8 flex items-center justify-center gap-2">
          <Button
            variant="outline"
            disabled={!data.hasPreviousPage}
            onClick={() => setPage((p) => p - 1)}
          >
            Previous
          </Button>
          <span className="px-4 text-sm text-slate-600">
            Page {data.page} of {data.totalPages}
          </span>
          <Button
            variant="outline"
            disabled={!data.hasNextPage}
            onClick={() => setPage((p) => p + 1)}
          >
            Next
          </Button>
        </div>
      )}
    </div>
  );
}
