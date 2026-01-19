import { SearchInput } from '@/components/common/SearchInput';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Button } from '@/components/ui/button';
import { X } from 'lucide-react';
import type { Category } from '@/types';

interface ProductFiltersProps {
  search: string;
  onSearchChange: (value: string) => void;
  categoryId: string;
  onCategoryChange: (value: string) => void;
  isActive: string;
  onIsActiveChange: (value: string) => void;
  stockStatus: string;
  onStockStatusChange: (value: string) => void;
  categories: Category[];
  onClearFilters: () => void;
}

export function ProductFilters({
  search,
  onSearchChange,
  categoryId,
  onCategoryChange,
  isActive,
  onIsActiveChange,
  stockStatus,
  onStockStatusChange,
  categories,
  onClearFilters,
}: ProductFiltersProps) {
  const hasFilters = search || categoryId || isActive !== 'all' || stockStatus !== 'all';

  return (
    <div className="flex flex-wrap items-center gap-4">
      <SearchInput
        value={search}
        onChange={onSearchChange}
        placeholder="Search products..."
        className="w-64"
      />

      <Select value={categoryId || 'all'} onValueChange={(v) => onCategoryChange(v === 'all' ? '' : v)}>
        <SelectTrigger className="w-48">
          <SelectValue placeholder="All Categories" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">All Categories</SelectItem>
          {categories.map((category) => (
            <SelectItem key={category.id} value={category.id}>
              {category.name}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>

      <Select value={isActive} onValueChange={onIsActiveChange}>
        <SelectTrigger className="w-36">
          <SelectValue placeholder="Status" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">All Status</SelectItem>
          <SelectItem value="true">Active</SelectItem>
          <SelectItem value="false">Inactive</SelectItem>
        </SelectContent>
      </Select>

      <Select value={stockStatus} onValueChange={onStockStatusChange}>
        <SelectTrigger className="w-40">
          <SelectValue placeholder="Stock" />
        </SelectTrigger>
        <SelectContent>
          <SelectItem value="all">All Stock</SelectItem>
          <SelectItem value="inStock">In Stock</SelectItem>
          <SelectItem value="lowStock">Low Stock</SelectItem>
          <SelectItem value="outOfStock">Out of Stock</SelectItem>
        </SelectContent>
      </Select>

      {hasFilters && (
        <Button variant="ghost" size="sm" onClick={onClearFilters}>
          <X className="mr-1 h-4 w-4" />
          Clear filters
        </Button>
      )}
    </div>
  );
}
