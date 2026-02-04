'use client';

import Image from 'next/image';
import Link from 'next/link';
import { Trash2, ShoppingBag, Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { useRemoveFromWishlist } from '@/features/wishlists/hooks/useWishlist';
import { useAddToCart } from '@/features/cart/hooks/useCart';
import { formatCurrency } from '@/lib/utils';
import type { WishlistItemDto } from '@/features/wishlists/services/wishlistService';

interface WishlistItemProps {
  item: WishlistItemDto;
}

export function WishlistItem({ item }: WishlistItemProps) {
  const removeFromWishlist = useRemoveFromWishlist();
  const addToCart = useAddToCart();

  const handleRemove = () => {
    removeFromWishlist.mutate(item.productId);
  };

  const handleAddToCart = () => {
    addToCart.mutate({
      productId: item.productId,
      quantity: 1,
    });
  };

  return (
    <div className="flex gap-4 rounded-lg border p-4">
      {/* Product image */}
      <Link href={`/products/${item.productSlug}`} className="shrink-0">
        <div className="relative h-24 w-24 overflow-hidden rounded-lg bg-slate-100 sm:h-32 sm:w-32">
          {item.productImage ? (
            <Image
              src={item.productImage}
              alt={item.productName}
              fill
              className="object-cover"
            />
          ) : (
            <div className="flex h-full items-center justify-center text-slate-400">
              No image
            </div>
          )}
        </div>
      </Link>

      {/* Product details */}
      <div className="flex flex-1 flex-col">
        <div className="flex items-start justify-between gap-2">
          <Link
            href={`/products/${item.productSlug}`}
            className="font-medium hover:text-slate-600"
          >
            {item.productName}
          </Link>
          <p className="font-semibold">{formatCurrency(item.productPrice)}</p>
        </div>

        <div className="mt-2">
          {item.isInStock ? (
            <Badge variant="secondary" className="bg-green-100 text-green-800">
              In Stock
            </Badge>
          ) : (
            <Badge variant="secondary" className="bg-red-100 text-red-800">
              Out of Stock
            </Badge>
          )}
        </div>

        <div className="mt-auto flex items-center gap-2 pt-4">
          <Button
            size="sm"
            onClick={handleAddToCart}
            disabled={!item.isInStock || addToCart.isPending}
          >
            {addToCart.isPending ? (
              <Loader2 className="mr-1 h-4 w-4 animate-spin" />
            ) : (
              <ShoppingBag className="mr-1 h-4 w-4" />
            )}
            Add to Cart
          </Button>
          <Button
            variant="outline"
            size="sm"
            onClick={handleRemove}
            disabled={removeFromWishlist.isPending}
            className="text-slate-500 hover:text-red-600"
          >
            {removeFromWishlist.isPending ? (
              <Loader2 className="h-4 w-4 animate-spin" />
            ) : (
              <Trash2 className="h-4 w-4" />
            )}
          </Button>
        </div>
      </div>
    </div>
  );
}
