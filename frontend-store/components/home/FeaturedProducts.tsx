'use client';

import Link from 'next/link';
import { ArrowRight } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { ProductGrid } from '@/components/products/ProductGrid';
import { useFeaturedProducts } from '@/features/products/hooks/useProducts';

export function FeaturedProducts() {
  const { data: products, isLoading } = useFeaturedProducts(8);

  return (
    <section className="py-16">
      <div className="container-custom">
        <div className="mb-8 flex items-center justify-between">
          <div>
            <h2 className="text-2xl font-bold md:text-3xl">Featured Products</h2>
            <p className="mt-2 text-slate-600">Handpicked selections just for you</p>
          </div>
          <Button variant="ghost" asChild className="hidden sm:flex">
            <Link href="/products?featured=true">
              View All
              <ArrowRight className="ml-2 h-4 w-4" />
            </Link>
          </Button>
        </div>

        <ProductGrid products={products ?? []} loading={isLoading} columns={4} />

        <div className="mt-8 text-center sm:hidden">
          <Button variant="outline" asChild>
            <Link href="/products?featured=true">
              View All Products
              <ArrowRight className="ml-2 h-4 w-4" />
            </Link>
          </Button>
        </div>
      </div>
    </section>
  );
}
