'use client';

import { useState } from 'react';
import { Minus, Plus, ShoppingBag, Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { useAddToCart } from '@/features/cart/hooks/useCart';
import type { Product, ProductVariant } from '@/types';

interface AddToCartButtonProps {
  product: Product;
  selectedVariant?: ProductVariant | null;
}

export function AddToCartButton({ product, selectedVariant }: AddToCartButtonProps) {
  const [quantity, setQuantity] = useState(1);
  const addToCart = useAddToCart();

  const isDisabled =
    !product.isInStock ||
    (product.variants.length > 0 && !selectedVariant) ||
    (selectedVariant && !selectedVariant.isInStock);

  const handleAddToCart = () => {
    addToCart.mutate({
      productId: product.id,
      quantity,
      variantId: selectedVariant?.id,
    });
  };

  const decrementQuantity = () => {
    if (quantity > 1) setQuantity(quantity - 1);
  };

  const incrementQuantity = () => {
    const maxStock = selectedVariant?.stockQuantity ?? product.stockQuantity;
    if (quantity < maxStock) setQuantity(quantity + 1);
  };

  return (
    <div className="flex flex-col gap-4 sm:flex-row">
      {/* Quantity selector */}
      <div className="flex items-center rounded-md border">
        <Button
          variant="ghost"
          size="icon"
          className="h-12 w-12 rounded-none"
          onClick={decrementQuantity}
          disabled={quantity <= 1}
        >
          <Minus className="h-4 w-4" />
        </Button>
        <span className="w-12 text-center font-medium">{quantity}</span>
        <Button
          variant="ghost"
          size="icon"
          className="h-12 w-12 rounded-none"
          onClick={incrementQuantity}
          disabled={quantity >= (selectedVariant?.stockQuantity ?? product.stockQuantity)}
        >
          <Plus className="h-4 w-4" />
        </Button>
      </div>

      {/* Add to cart button */}
      <Button
        size="lg"
        className="h-12 flex-1"
        onClick={handleAddToCart}
        disabled={isDisabled || addToCart.isPending}
      >
        {addToCart.isPending ? (
          <Loader2 className="mr-2 h-5 w-5 animate-spin" />
        ) : (
          <ShoppingBag className="mr-2 h-5 w-5" />
        )}
        {!product.isInStock
          ? 'Out of Stock'
          : product.variants.length > 0 && !selectedVariant
          ? 'Select Options'
          : 'Add to Cart'}
      </Button>
    </div>
  );
}
