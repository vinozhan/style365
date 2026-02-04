import Link from 'next/link';
import { ShoppingBag } from 'lucide-react';
import { Button } from '@/components/ui/button';

export function CartEmpty() {
  return (
    <div className="flex flex-col items-center justify-center py-16">
      <div className="mb-6 rounded-full bg-slate-100 p-6">
        <ShoppingBag className="h-12 w-12 text-slate-400" />
      </div>
      <h2 className="text-xl font-semibold">Your cart is empty</h2>
      <p className="mt-2 text-slate-600">
        Looks like you haven&apos;t added any items to your cart yet.
      </p>
      <Button className="mt-6" asChild>
        <Link href="/products">Start Shopping</Link>
      </Button>
    </div>
  );
}
