'use client';

import Link from 'next/link';
import { Heart } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { WishlistItem } from '@/components/account';
import { useWishlist } from '@/features/wishlists/hooks/useWishlist';

export default function WishlistPage() {
  const { data: wishlists, isLoading } = useWishlist();

  // Get all items from all wishlists (typically there's one default wishlist)
  const allItems = wishlists?.flatMap((w) => w.items) ?? [];

  if (isLoading) {
    return (
      <div>
        <h1 className="mb-6 text-2xl font-bold">My Wishlist</h1>
        <div className="space-y-4">
          {Array.from({ length: 3 }).map((_, i) => (
            <Skeleton key={i} className="h-32 rounded-lg" />
          ))}
        </div>
      </div>
    );
  }

  if (allItems.length === 0) {
    return (
      <div>
        <h1 className="mb-6 text-2xl font-bold">My Wishlist</h1>
        <div className="flex flex-col items-center justify-center py-12 text-center">
          <div className="mb-4 rounded-full bg-slate-100 p-4">
            <Heart className="h-8 w-8 text-slate-400" />
          </div>
          <h2 className="text-lg font-semibold">Your wishlist is empty</h2>
          <p className="mt-2 text-slate-500">
            Save items you love by clicking the heart icon on products.
          </p>
          <Button className="mt-6" asChild>
            <Link href="/products">Browse Products</Link>
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold">My Wishlist</h1>
        <p className="text-slate-500">{allItems.length} items</p>
      </div>

      <div className="space-y-4">
        {allItems.map((item) => (
          <WishlistItem key={item.id} item={item} />
        ))}
      </div>
    </div>
  );
}
