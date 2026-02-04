'use client';

import { Suspense, useState } from 'react';
import { useSearchParams } from 'next/navigation';
import Link from 'next/link';
import { Search, ChevronRight, Filter, Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from '@/components/ui/sheet';
import { Skeleton } from '@/components/ui/skeleton';
import { ProductGrid } from '@/components/products/ProductGrid';
import { ProductFilters } from '@/components/products/ProductFilters';
import { ProductSort } from '@/components/products/ProductSort';
import { useProductSearch } from '@/features/products/hooks/useProducts';
import type { ProductFilters as FilterType } from '@/types';

function SearchContent() {
  const searchParams = useSearchParams();
  const query = searchParams.get('q') ?? '';
  const [mobileFiltersOpen, setMobileFiltersOpen] = useState(false);
  const [searchInput, setSearchInput] = useState(query);

  const [filters, setFilters] = useState<FilterType>({
    page: 1,
    pageSize: 20,
  });
  const [sortBy, setSortBy] = useState('newest');

  const { data, isLoading } = useProductSearch(query, filters.page, filters.pageSize);

  const handleFilterChange = (newFilters: FilterType) => {
    setFilters(newFilters);
  };

  const handlePageChange = (page: number) => {
    setFilters((prev) => ({ ...prev, page }));
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchInput.trim()) {
      window.location.href = `/search?q=${encodeURIComponent(searchInput.trim())}`;
    }
  };

  return (
    <div className="container-custom py-8">
      {/* Breadcrumbs */}
      <nav className="mb-6 flex items-center gap-2 text-sm text-slate-600">
        <Link href="/" className="hover:text-slate-900">
          Home
        </Link>
        <ChevronRight className="h-4 w-4" />
        <span className="text-slate-900">Search Results</span>
      </nav>

      {/* Search input */}
      <form onSubmit={handleSearch} className="mb-8">
        <div className="relative max-w-xl">
          <Search className="absolute left-3 top-1/2 h-5 w-5 -translate-y-1/2 text-slate-400" />
          <Input
            type="search"
            placeholder="Search products..."
            value={searchInput}
            onChange={(e) => setSearchInput(e.target.value)}
            className="h-12 pl-10 pr-4"
          />
        </div>
      </form>

      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold">
          {query ? `Search results for "${query}"` : 'Search Products'}
        </h1>
        {data && (
          <p className="mt-2 text-slate-600">
            {data.totalItems} {data.totalItems === 1 ? 'product' : 'products'} found
          </p>
        )}
      </div>

      {!query ? (
        <div className="py-12 text-center">
          <Search className="mx-auto mb-4 h-16 w-16 text-slate-300" />
          <h2 className="text-lg font-semibold">Enter a search term</h2>
          <p className="mt-2 text-slate-500">Type something in the search box above to find products.</p>
        </div>
      ) : (
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

            {/* No results */}
            {!isLoading && data?.items.length === 0 && (
              <div className="py-12 text-center">
                <Search className="mx-auto mb-4 h-16 w-16 text-slate-300" />
                <h2 className="text-lg font-semibold">No products found</h2>
                <p className="mt-2 text-slate-500">
                  Try adjusting your search or filters to find what you&apos;re looking for.
                </p>
                <Button className="mt-6" asChild>
                  <Link href="/products">Browse All Products</Link>
                </Button>
              </div>
            )}

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
      )}
    </div>
  );
}

export default function SearchPage() {
  return (
    <Suspense
      fallback={
        <div className="container-custom py-8">
          <Skeleton className="mb-6 h-6 w-32" />
          <Skeleton className="mb-8 h-12 max-w-xl" />
          <Skeleton className="mb-8 h-10 w-64" />
          <div className="grid grid-cols-1 gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {Array.from({ length: 6 }).map((_, i) => (
              <Skeleton key={i} className="h-80 rounded-lg" />
            ))}
          </div>
        </div>
      }
    >
      <SearchContent />
    </Suspense>
  );
}
