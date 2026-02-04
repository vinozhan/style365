'use client';

import Image from 'next/image';
import Link from 'next/link';
import { Minus, Plus, Trash2, Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { useUpdateCartItem, useRemoveFromCart } from '@/features/cart/hooks/useCart';
import type { CartItem as CartItemType } from '@/types';
import { formatCurrency } from '@/lib/utils';

interface CartItemProps {
  item: CartItemType;
}

export function CartItem({ item }: CartItemProps) {
  const updateItem = useUpdateCartItem();
  const removeItem = useRemoveFromCart();

  const isUpdating = updateItem.isPending;
  const isRemoving = removeItem.isPending;

  const handleQuantityChange = (newQuantity: number) => {
    if (newQuantity < 1) return;
    updateItem.mutate({ itemId: item.id, quantity: newQuantity });
  };

  const handleRemove = () => {
    removeItem.mutate(item.id);
  };

  return (
    <div className="flex gap-4 py-4 border-b last:border-b-0">
      {/* Product image */}
      <Link href={`/products/${item.productSlug}`} className="shrink-0">
        <div className="relative h-24 w-24 overflow-hidden rounded-lg bg-slate-100">
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
        <div className="flex justify-between">
          <div>
            <Link
              href={`/products/${item.productSlug}`}
              className="font-medium hover:text-slate-600"
            >
              {item.productName}
            </Link>
            {item.variantName && (
              <p className="mt-1 text-sm text-slate-500">{item.variantName}</p>
            )}
          </div>
          <p className="font-medium">{formatCurrency(item.subTotal)}</p>
        </div>

        <div className="mt-auto flex items-center justify-between">
          {/* Quantity controls */}
          <div className="flex items-center gap-2">
            <Button
              variant="outline"
              size="icon"
              className="h-8 w-8"
              onClick={() => handleQuantityChange(item.quantity - 1)}
              disabled={item.quantity <= 1 || isUpdating}
            >
              <Minus className="h-3 w-3" />
            </Button>
            <span className="w-8 text-center text-sm">
              {isUpdating ? (
                <Loader2 className="mx-auto h-4 w-4 animate-spin" />
              ) : (
                item.quantity
              )}
            </span>
            <Button
              variant="outline"
              size="icon"
              className="h-8 w-8"
              onClick={() => handleQuantityChange(item.quantity + 1)}
              disabled={isUpdating}
            >
              <Plus className="h-3 w-3" />
            </Button>
          </div>

          {/* Remove button */}
          <Button
            variant="ghost"
            size="sm"
            className="text-slate-500 hover:text-red-600"
            onClick={handleRemove}
            disabled={isRemoving}
          >
            {isRemoving ? (
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
