'use client';

import { Heart, Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { useWishlistStatus, useToggleWishlist } from '@/features/wishlists/hooks/useWishlist';
import { useAuthStore } from '@/stores/authStore';
import { useUIStore } from '@/stores/uiStore';
import { cn } from '@/lib/utils';

interface WishlistButtonProps {
  productId: string;
  variant?: 'default' | 'icon';
  size?: 'default' | 'sm' | 'lg' | 'icon';
  className?: string;
}

export function WishlistButton({
  productId,
  variant = 'default',
  size = 'default',
  className,
}: WishlistButtonProps) {
  const { isAuthenticated } = useAuthStore();
  const { openAuthModal } = useUIStore();
  const { data: wishlistStatus } = useWishlistStatus(productId);
  const { toggle, isLoading } = useToggleWishlist();

  const isInWishlist = wishlistStatus?.isInWishlist ?? false;

  const handleClick = () => {
    if (!isAuthenticated) {
      openAuthModal('/account/wishlist');
      return;
    }
    toggle(productId, isInWishlist);
  };

  if (variant === 'icon') {
    return (
      <Button
        variant="ghost"
        size="icon"
        onClick={handleClick}
        disabled={isLoading}
        className={cn(
          'h-9 w-9 rounded-full',
          isInWishlist && 'text-red-500',
          className
        )}
      >
        {isLoading ? (
          <Loader2 className="h-5 w-5 animate-spin" />
        ) : (
          <Heart className={cn('h-5 w-5', isInWishlist && 'fill-current')} />
        )}
      </Button>
    );
  }

  return (
    <Button
      variant="outline"
      size={size}
      onClick={handleClick}
      disabled={isLoading}
      className={cn(isInWishlist && 'text-red-500', className)}
    >
      {isLoading ? (
        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
      ) : (
        <Heart className={cn('mr-2 h-4 w-4', isInWishlist && 'fill-current')} />
      )}
      {isInWishlist ? 'In Wishlist' : 'Add to Wishlist'}
    </Button>
  );
}
