'use client';

import { useState } from 'react';
import { useParams } from 'next/navigation';
import Link from 'next/link';
import { ChevronRight, Filter } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Sheet, SheetContent, SheetHeader, SheetTitle, SheetTrigger } from '@/components/ui/sheet';
import { Skeleton } from '@/components/ui/skeleton';
import { ProductGrid } from '@/components/products/ProductGrid';
import { ProductFilters } from '@/components/products/ProductFilters';
import { ProductSort } from '@/components/products/ProductSort';
import { useCategoryBySlug } from '@/features/categories/hooks/useCategories';
import { useProductsByCategory } from '@/features/products/hooks/useProducts';
import type { ProductFilters as FilterType } from '@/types';

export default function CategoryPage() {
  const params = useParams();
  const slug = params.slug as string;
  const [mobileFiltersOpen, setMobileFiltersOpen] = useState(false);

  const { data: category, isLoading: categoryLoading } = useCategoryBySlug(slug);

  const [filters, setFilters] = useState<FilterType>({
    page: 1,
    pageSize: 20,
  });
  const [sortBy, setSortBy] = useState('newest');

  const { data: products, isLoading: productsLoading } = useProductsByCategory(
    category?.id ?? '',
    filters.page,
    filters.pageSize
  );

  const handleFilterChange = (newFilters: FilterType) => {
    setFilters(newFilters);
  };

  const handlePageChange = (page: number) => {
    setFilters((prev) => ({ ...prev, page }));
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  if (categoryLoading) {
    return (
      <div className="container-custom py-8">
        <Skeleton className="mb-4 h-8 w-48" />
        <Skeleton className="mb-8 h-4 w-96" />
        <div className="grid gap-4 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4">
          {Array.from({ length: 8 }).map((_, i) => (
            <Skeleton key={i} className="aspect-[3/4] rounded-lg" />
          ))}
        </div>
      </div>
    );
  }

  if (!category) {
    return (
      <div className="container-custom py-16 text-center">
        <h1 className="text-2xl font-bold">Category Not Found</h1>
        <p className="mt-2 text-slate-600">The category you&apos;re looking for doesn&apos;t exist.</p>
        <Button className="mt-6" asChild>
          <Link href="/products">Browse All Products</Link>
        </Button>
      </div>
    );
  }

  return (
    <div className="container-custom py-8">
      {/* Breadcrumbs */}
      <nav className="mb-6 flex items-center gap-2 text-sm text-slate-600">
        <Link href="/" className="hover:text-slate-900">
          Home
        </Link>
        <ChevronRight className="h-4 w-4" />
        <Link href="/products" className="hover:text-slate-900">
          Products
        </Link>
        <ChevronRight className="h-4 w-4" />
        {category.parentCategoryName && (
          <>
            <span>{category.parentCategoryName}</span>
            <ChevronRight className="h-4 w-4" />
          </>
        )}
        <span className="text-slate-900">{category.name}</span>
      </nav>

      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold">{category.name}</h1>
        {category.description && <p className="mt-2 text-slate-600">{category.description}</p>}
        <p className="mt-1 text-sm text-slate-500">
          {products?.totalItems ?? 0} products
        </p>
      </div>

      {/* Subcategories */}
      {category.subCategories && category.subCategories.length > 0 && (
        <div className="mb-8 flex flex-wrap gap-2">
          {category.subCategories.map((sub) => (
            <Link
              key={sub.id}
              href={`/categories/${sub.slug}`}
              className="rounded-full border px-4 py-2 text-sm hover:border-slate-900 hover:bg-slate-50"
            >
              {sub.name}
            </Link>
          ))}
        </div>
      )}

      <div className="flex gap-8">
        {/* Desktop filters */}
        <aside className="hidden w-64 flex-shrink-0 lg:block">
          <ProductFilters
            filters={filters}
            onFilterChange={handleFilterChange}
            categoryId={category.id}
          />
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
                  <ProductFilters
                    filters={filters}
                    onFilterChange={handleFilterChange}
                    categoryId={category.id}
                  />
                </div>
              </SheetContent>
            </Sheet>

            <div className="hidden lg:block" />

            {/* Sort */}
            <ProductSort value={sortBy} onChange={setSortBy} />
          </div>

          {/* Products grid */}
          <ProductGrid products={products?.items ?? []} loading={productsLoading} columns={3} />

          {/* Pagination */}
          {products && products.totalPages > 1 && (
            <div className="mt-8 flex items-center justify-center gap-2">
              <Button
                variant="outline"
                disabled={!products.hasPreviousPage}
                onClick={() => handlePageChange(products.page - 1)}
              >
                Previous
              </Button>
              <span className="px-4 text-sm text-slate-600">
                Page {products.page} of {products.totalPages}
              </span>
              <Button
                variant="outline"
                disabled={!products.hasNextPage}
                onClick={() => handlePageChange(products.page + 1)}
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
