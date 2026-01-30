'use client';

import { useState } from 'react';
import { ChevronDown, X } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Checkbox } from '@/components/ui/checkbox';
import { Label } from '@/components/ui/label';
import { Separator } from '@/components/ui/separator';
import { useBrands, usePriceRange } from '@/features/products/hooks/useProducts';
import { useCategories } from '@/features/categories/hooks/useCategories';
import { formatCurrency, cn } from '@/lib/utils';
import type { ProductFilters as FilterType } from '@/types';

interface ProductFiltersProps {
  filters: FilterType;
  onFilterChange: (filters: FilterType) => void;
  categoryId?: string;
}

export function ProductFilters({ filters, onFilterChange, categoryId }: ProductFiltersProps) {
  const { data: categories } = useCategories();
  const { data: brands } = useBrands();
  const { data: priceRange } = usePriceRange(categoryId);
  const [expandedSections, setExpandedSections] = useState<string[]>(['categories', 'price', 'brand']);

  const toggleSection = (section: string) => {
    setExpandedSections((prev) =>
      prev.includes(section) ? prev.filter((s) => s !== section) : [...prev, section]
    );
  };

  const handleCategoryChange = (catId: string) => {
    onFilterChange({ ...filters, categoryId: catId === filters.categoryId ? undefined : catId, page: 1 });
  };

  const handleBrandChange = (brand: string) => {
    onFilterChange({ ...filters, brand: brand === filters.brand ? undefined : brand, page: 1 });
  };

  const handlePriceChange = (min?: number, max?: number) => {
    onFilterChange({ ...filters, minPrice: min, maxPrice: max, page: 1 });
  };

  const clearFilters = () => {
    onFilterChange({ page: 1, pageSize: filters.pageSize });
  };

  const hasActiveFilters = filters.categoryId || filters.brand || filters.minPrice || filters.maxPrice;

  const priceRanges = [
    { label: 'Under Rs. 1,000', min: 0, max: 1000 },
    { label: 'Rs. 1,000 - Rs. 3,000', min: 1000, max: 3000 },
    { label: 'Rs. 3,000 - Rs. 5,000', min: 3000, max: 5000 },
    { label: 'Rs. 5,000 - Rs. 10,000', min: 5000, max: 10000 },
    { label: 'Over Rs. 10,000', min: 10000, max: undefined },
  ];

  return (
    <div className="space-y-4">
      {/* Clear filters */}
      {hasActiveFilters && (
        <Button variant="ghost" size="sm" onClick={clearFilters} className="w-full justify-start">
          <X className="mr-2 h-4 w-4" />
          Clear all filters
        </Button>
      )}

      {/* Categories */}
      {!categoryId && categories && categories.length > 0 && (
        <div className="rounded-lg border p-4">
          <button
            onClick={() => toggleSection('categories')}
            className="flex w-full items-center justify-between font-medium"
          >
            Categories
            <ChevronDown
              className={cn('h-4 w-4 transition-transform', expandedSections.includes('categories') && 'rotate-180')}
            />
          </button>
          {expandedSections.includes('categories') && (
            <div className="mt-4 space-y-3">
              {categories.filter((c) => !c.parentCategoryId).map((category) => (
                <div key={category.id} className="flex items-center gap-2">
                  <Checkbox
                    id={`cat-${category.id}`}
                    checked={filters.categoryId === category.id}
                    onCheckedChange={() => handleCategoryChange(category.id)}
                  />
                  <Label htmlFor={`cat-${category.id}`} className="flex-1 cursor-pointer text-sm">
                    {category.name}
                  </Label>
                  <span className="text-xs text-slate-500">({category.productCount})</span>
                </div>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Price Range */}
      <div className="rounded-lg border p-4">
        <button
          onClick={() => toggleSection('price')}
          className="flex w-full items-center justify-between font-medium"
        >
          Price Range
          <ChevronDown
            className={cn('h-4 w-4 transition-transform', expandedSections.includes('price') && 'rotate-180')}
          />
        </button>
        {expandedSections.includes('price') && (
          <div className="mt-4 space-y-3">
            {priceRanges.map((range, index) => (
              <div key={index} className="flex items-center gap-2">
                <Checkbox
                  id={`price-${index}`}
                  checked={filters.minPrice === range.min && filters.maxPrice === range.max}
                  onCheckedChange={() => {
                    if (filters.minPrice === range.min && filters.maxPrice === range.max) {
                      handlePriceChange(undefined, undefined);
                    } else {
                      handlePriceChange(range.min, range.max);
                    }
                  }}
                />
                <Label htmlFor={`price-${index}`} className="cursor-pointer text-sm">
                  {range.label}
                </Label>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Brands */}
      {brands && brands.length > 0 && (
        <div className="rounded-lg border p-4">
          <button
            onClick={() => toggleSection('brand')}
            className="flex w-full items-center justify-between font-medium"
          >
            Brand
            <ChevronDown
              className={cn('h-4 w-4 transition-transform', expandedSections.includes('brand') && 'rotate-180')}
            />
          </button>
          {expandedSections.includes('brand') && (
            <div className="mt-4 space-y-3">
              {brands.map((brand) => (
                <div key={brand} className="flex items-center gap-2">
                  <Checkbox
                    id={`brand-${brand}`}
                    checked={filters.brand === brand}
                    onCheckedChange={() => handleBrandChange(brand)}
                  />
                  <Label htmlFor={`brand-${brand}`} className="cursor-pointer text-sm">
                    {brand}
                  </Label>
                </div>
              ))}
            </div>
          )}
        </div>
      )}
    </div>
  );
}
