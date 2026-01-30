'use client';

import { useState } from 'react';
import { useSearchParams } from 'next/navigation';
import { Filter, X } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from '@/components/ui/sheet';
import { ProductGrid } from '@/components/products/ProductGrid';
import { ProductFilters } from '@/components/products/ProductFilters';
import { ProductSort } from '@/components/products/ProductSort';
import { useProducts } from '@/features/products/hooks/useProducts';
import type { ProductFilters as FilterType } from '@/types';

export default function ProductsPage() {
  const searchParams = useSearchParams();
  const [mobileFiltersOpen, setMobileFiltersOpen] = useState(false);

  const initialFilters: FilterType = {
    searchTerm: searchParams.get('q') ?? undefined,
    categoryId: searchParams.get('category') ?? undefined,
    featuredOnly: searchParams.get('featured') === 'true',
    page: 1,
    pageSize: 20,
  };

  const [filters, setFilters] = useState<FilterType>(initialFilters);
  const [sortBy, setSortBy] = useState('newest');

  const { data, isLoading } = useProducts({ ...filters });

  const handleFilterChange = (newFilters: FilterType) => {
    setFilters(newFilters);
  };

  const handlePageChange = (page: number) => {
    setFilters((prev) => ({ ...prev, page }));
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  return (
    <div className="container-custom py-8">
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold">All Products</h1>
        <p className="mt-2 text-slate-600">
          {data?.totalItems ? `${data.totalItems} products` : 'Browse our collection'}
        </p>
      </div>

      <div className="flex gap-8">
        {/* Desktop filters */}
        <aside className="hidden w-64 flex-shrink-0 lg:block">
          <ProductFilters filters={filters} onFilterChange={handleFilterChange} />
        </aside>

        {/* Main content */}
        <div className="flex-1">
          {/* Toolbar */}
          <div className="mb-6 flex items-center justify-between">
            {/* Mobile filter button */}
            <Sheet open={mobileFiltersOpen} onOpenChange={setMobileFiltersOpen}>
              <SheetTrigger asChild>
                <Button variant="outline" className="lg:hidden">
                  <Filter className="mr-2 h-4 w-4" />
                  Filters
                </Button>
              </SheetTrigger>
              <SheetContent side="left" className="w-80">
                <SheetHeader>
                  <SheetTitle>Filters</SheetTitle>
                </SheetHeader>
                <div className="mt-6">
                  <ProductFilters filters={filters} onFilterChange={handleFilterChange} />
                </div>
              </SheetContent>
            </Sheet>

            <div className="hidden lg:block" />

            {/* Sort */}
            <ProductSort value={sortBy} onChange={setSortBy} />
          </div>

          {/* Products grid */}
          <ProductGrid products={data?.items ?? []} loading={isLoading} columns={3} />

          {/* Pagination */}
          {data && data.totalPages > 1 && (
            <div className="mt-8 flex items-center justify-center gap-2">
              <Button
                variant="outline"
                disabled={!data.hasPreviousPage}
                onClick={() => handlePageChange(data.page - 1)}
              >
                Previous
              </Button>
              <span className="px-4 text-sm text-slate-600">
                Page {data.page} of {data.totalPages}
              </span>
              <Button
                variant="outline"
                disabled={!data.hasNextPage}
                onClick={() => handlePageChange(data.page + 1)}
              >
                Next
              </Button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
