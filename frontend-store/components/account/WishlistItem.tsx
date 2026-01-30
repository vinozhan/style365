'use client';

import Image from 'next/image';
import Link from 'next/link';
import { Trash2, ShoppingBag, Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { useRemoveFromWishlist } from '@/features/wishlists/hooks/useWishlist';
import { useAddToCart } from '@/features/cart/hooks/useCart';
import { formatCurrency } from '@/lib/utils';
import type { Product } from '@/types';

interface WishlistItemProps {
  product: Product;
}

export function WishlistItem({ product }: WishlistItemProps) {
  const removeFromWishlist = useRemoveFromWishlist();
  const addToCart = useAddToCart();

  const handleRemove = () => {
    removeFromWishlist.mutate(product.id);
  };

  const handleAddToCart = () => {
    addToCart.mutate({
      productId: product.id,
      quantity: 1,
    });
  };

  return (
    <div className="flex gap-4 rounded-lg border p-4">
      {/* Product image */}
      <Link href={`/products/${product.slug}`} className="flex-shrink-0">
        <div className="relative h-24 w-24 overflow-hidden rounded-lg bg-slate-100 sm:h-32 sm:w-32">
          {product.images[0]?.imageUrl ? (
            <Image
              src={product.images[0].imageUrl}
              alt={product.name}
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
          <div>
            <Link
              href={`/products/${product.slug}`}
              className="font-medium hover:text-slate-600"
            >
              {product.name}
            </Link>
            {product.brandName && (
              <p className="mt-1 text-sm text-slate-500">{product.brandName}</p>
            )}
          </div>
          <div className="text-right">
            {product.compareAtPrice && product.compareAtPrice > product.price ? (
              <>
                <p className="text-sm text-slate-500 line-through">
                  {formatCurrency(product.compareAtPrice)}
                </p>
                <p className="font-semibold text-red-600">{formatCurrency(product.price)}</p>
              </>
            ) : (
              <p className="font-semibold">{formatCurrency(product.price)}</p>
            )}
          </div>
        </div>

        <div className="mt-2">
          {product.isInStock ? (
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
            disabled={!product.isInStock || addToCart.isPending}
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
