'use client';

import Link from 'next/link';
import Image from 'next/image';
import { Heart, ShoppingBag, Eye } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { useAuthStore } from '@/stores/authStore';
import { useUIStore } from '@/stores/uiStore';
import { useAddToCart } from '@/features/cart/hooks/useCart';
import { useWishlistStatus, useToggleWishlist } from '@/features/wishlists/hooks/useWishlist';
import { formatCurrency } from '@/lib/utils';
import { cn } from '@/lib/utils';
import type { Product } from '@/types';

interface ProductCardProps {
  product: Product;
  showQuickView?: boolean;
}

export function ProductCard({ product, showQuickView = true }: ProductCardProps) {
  const { isAuthenticated } = useAuthStore();
  const { openQuickView, openAuthModal } = useUIStore();
  const addToCart = useAddToCart();
  const { data: wishlistStatus } = useWishlistStatus(product.id);
  const { toggle: toggleWishlist, isLoading: isWishlistLoading } = useToggleWishlist();

  const hasDiscount = product.compareAtPrice && product.compareAtPrice > product.price;
  const discountPercentage = hasDiscount
    ? Math.round(((product.compareAtPrice! - product.price) / product.compareAtPrice!) * 100)
    : 0;

  const handleAddToCart = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (product.variants.length === 0) {
      addToCart.mutate({ productId: product.id, quantity: 1 });
    } else {
      openQuickView(product);
    }
  };

  const handleWishlistClick = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (!isAuthenticated) {
      openAuthModal('/account/wishlist');
      return;
    }
    toggleWishlist(product.id, wishlistStatus?.isInWishlist ?? false);
  };

  const handleQuickView = (e: React.MouseEvent) => {
    e.preventDefault();
    e.stopPropagation();
    openQuickView(product);
  };

  return (
    <div className="group relative">
      {/* Image container */}
      <Link href={`/products/${product.slug}`} className="block">
        <div className="relative aspect-3/4 overflow-hidden rounded-lg bg-slate-100">
          {product.mainImageUrl ? (
            <Image
              src={product.mainImageUrl}
              alt={product.name}
              fill
              sizes="(max-width: 640px) 50vw, (max-width: 1024px) 33vw, 25vw"
              className="object-cover transition-transform duration-300 group-hover:scale-105"
            />
          ) : (
            <div className="flex h-full items-center justify-center">
              <ShoppingBag className="h-12 w-12 text-slate-300" />
            </div>
          )}

          {/* Badges */}
          <div className="absolute left-2 top-2 flex flex-col gap-1">
            {hasDiscount && (
              <Badge variant="destructive" className="text-xs">
                -{discountPercentage}%
              </Badge>
            )}
            {product.isFeatured && (
              <Badge variant="secondary" className="text-xs">
                Featured
              </Badge>
            )}
            {!product.isInStock && (
              <Badge variant="outline" className="bg-white text-xs">
                Out of Stock
              </Badge>
            )}
          </div>

          {/* Wishlist button */}
          <button
            onClick={handleWishlistClick}
            disabled={isWishlistLoading}
            className={cn(
              'absolute right-2 top-2 flex h-8 w-8 items-center justify-center rounded-full bg-white shadow-md transition-all hover:scale-110',
              wishlistStatus?.isInWishlist && 'text-red-500'
            )}
            aria-label={wishlistStatus?.isInWishlist ? 'Remove from wishlist' : 'Add to wishlist'}
          >
            <Heart
              className={cn('h-4 w-4', wishlistStatus?.isInWishlist && 'fill-current')}
            />
          </button>

          {/* Hover actions */}
          <div className="absolute bottom-0 left-0 right-0 translate-y-full bg-linear-to-t from-black/60 to-transparent p-3 opacity-0 transition-all group-hover:translate-y-0 group-hover:opacity-100">
            <div className="flex gap-2">
              <Button
                size="sm"
                className="flex-1"
                onClick={handleAddToCart}
                disabled={!product.isInStock || addToCart.isPending}
              >
                {product.variants.length > 0 ? 'Select Options' : 'Add to Cart'}
              </Button>
              {showQuickView && (
                <Button size="sm" variant="secondary" onClick={handleQuickView}>
                  <Eye className="h-4 w-4" />
                </Button>
              )}
            </div>
          </div>
        </div>
      </Link>

      {/* Product info */}
      <div className="mt-3">
        <Link href={`/products/${product.slug}`}>
          <h3 className="text-sm font-medium line-clamp-2 hover:underline">{product.name}</h3>
        </Link>
        {product.categoryName && (
          <p className="mt-1 text-xs text-slate-500">{product.categoryName}</p>
        )}
        <div className="mt-2 flex items-center gap-2">
          <span className="font-semibold">{formatCurrency(product.price)}</span>
          {hasDiscount && (
            <span className="text-sm text-slate-500 line-through">
              {formatCurrency(product.compareAtPrice!)}
            </span>
          )}
        </div>
      </div>
    </div>
  );
}
