'use client';

import { cn } from '@/lib/utils';
import type { ProductVariant } from '@/types';

interface ProductVariantsProps {
  variants: ProductVariant[];
  selectedVariant: ProductVariant | null;
  onSelect: (variant: ProductVariant) => void;
}

export function ProductVariants({ variants, selectedVariant, onSelect }: ProductVariantsProps) {
  // Group variants by attribute (color, size)
  const colors = [...new Set(variants.filter((v) => v.color).map((v) => v.color!))];
  const sizes = [...new Set(variants.filter((v) => v.size).map((v) => v.size!))];

  const getVariantByAttributes = (color?: string, size?: string) => {
    return variants.find(
      (v) =>
        (!color || v.color === color) &&
        (!size || v.size === size)
    );
  };

  const selectedColor = selectedVariant?.color;
  const selectedSize = selectedVariant?.size;

  const handleColorSelect = (color: string) => {
    const variant = getVariantByAttributes(color, selectedSize);
    if (variant) onSelect(variant);
  };

  const handleSizeSelect = (size: string) => {
    const variant = getVariantByAttributes(selectedColor, size);
    if (variant) onSelect(variant);
  };

  if (variants.length === 0) return null;

  return (
    <div className="space-y-6">
      {/* Color selector */}
      {colors.length > 0 && (
        <div>
          <h3 className="mb-3 text-sm font-medium">
            Color: <span className="font-normal text-slate-600">{selectedColor}</span>
          </h3>
          <div className="flex flex-wrap gap-2">
            {colors.map((color) => {
              const variant = getVariantByAttributes(color, selectedSize);
              const isAvailable = variant?.isInStock ?? false;
              const isSelected = selectedColor === color;

              return (
                <button
                  key={color}
                  onClick={() => handleColorSelect(color)}
                  disabled={!isAvailable}
                  className={cn(
                    'relative h-10 min-w-[60px] rounded-md border px-4 text-sm font-medium transition-all',
                    isSelected
                      ? 'border-slate-900 bg-slate-900 text-white'
                      : 'border-slate-300 hover:border-slate-400',
                    !isAvailable && 'cursor-not-allowed opacity-50 line-through'
                  )}
                >
                  {color}
                </button>
              );
            })}
          </div>
        </div>
      )}

      {/* Size selector */}
      {sizes.length > 0 && (
        <div>
          <h3 className="mb-3 text-sm font-medium">
            Size: <span className="font-normal text-slate-600">{selectedSize}</span>
          </h3>
          <div className="flex flex-wrap gap-2">
            {sizes.map((size) => {
              const variant = getVariantByAttributes(selectedColor, size);
              const isAvailable = variant?.isInStock ?? false;
              const isSelected = selectedSize === size;

              return (
                <button
                  key={size}
                  onClick={() => handleSizeSelect(size)}
                  disabled={!isAvailable}
                  className={cn(
                    'relative flex h-10 w-10 items-center justify-center rounded-md border text-sm font-medium transition-all',
                    isSelected
                      ? 'border-slate-900 bg-slate-900 text-white'
                      : 'border-slate-300 hover:border-slate-400',
                    !isAvailable && 'cursor-not-allowed opacity-50 line-through'
                  )}
                >
                  {size}
                </button>
              );
            })}
          </div>
        </div>
      )}
    </div>
  );
}
