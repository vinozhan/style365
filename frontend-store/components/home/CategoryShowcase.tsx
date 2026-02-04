'use client';

import Link from 'next/link';
import Image from 'next/image';
import { ArrowRight } from 'lucide-react';
import { Skeleton } from '@/components/ui/skeleton';
import { useCategories } from '@/features/categories/hooks/useCategories';
import type { Category } from '@/types';

function CategoryCard({ category }: { category: Category }) {
  return (
    <Link
      href={`/categories/${category.slug}`}
      className="group relative aspect-square overflow-hidden rounded-lg bg-slate-100"
    >
      {category.imageUrl ? (
        <Image
          src={category.imageUrl}
          alt={category.name}
          fill
          sizes="(max-width: 640px) 50vw, (max-width: 1024px) 33vw, 25vw"
          className="object-cover transition-transform duration-300 group-hover:scale-105"
        />
      ) : (
        <div className="flex h-full items-center justify-center bg-linear-to-br from-slate-200 to-slate-300">
          <span className="text-4xl font-bold text-slate-400">
            {category.name.charAt(0)}
          </span>
        </div>
      )}
      <div className="absolute inset-0 bg-linear-to-t from-black/60 via-black/0 to-transparent" />
      <div className="absolute bottom-0 left-0 right-0 p-4 text-white">
        <h3 className="text-lg font-semibold">{category.name}</h3>
        <p className="mt-1 flex items-center text-sm text-white/80">
          Shop Now
          <ArrowRight className="ml-1 h-3 w-3 transition-transform group-hover:translate-x-1" />
        </p>
      </div>
    </Link>
  );
}

export function CategoryShowcase() {
  const { data: categories, isLoading } = useCategories();

  // Get top-level categories only
  const topCategories = categories?.filter((c) => !c.parentCategoryId).slice(0, 6) ?? [];

  if (isLoading) {
    return (
      <section className="bg-slate-50 py-16">
        <div className="container-custom">
          <div className="mb-8">
            <Skeleton className="h-8 w-48" />
            <Skeleton className="mt-2 h-4 w-64" />
          </div>
          <div className="grid grid-cols-2 gap-4 md:grid-cols-3 lg:grid-cols-6">
            {Array.from({ length: 6 }).map((_, i) => (
              <Skeleton key={i} className="aspect-square rounded-lg" />
            ))}
          </div>
        </div>
      </section>
    );
  }

  if (topCategories.length === 0) {
    return null;
  }

  return (
    <section className="bg-slate-50 py-16">
      <div className="container-custom">
        <div className="mb-8 text-center">
          <h2 className="text-2xl font-bold md:text-3xl">Shop by Category</h2>
          <p className="mt-2 text-slate-600">Browse our collections</p>
        </div>

        <div className="grid grid-cols-2 gap-4 md:grid-cols-3 lg:grid-cols-6">
          {topCategories.map((category) => (
            <CategoryCard key={category.id} category={category} />
          ))}
        </div>
      </div>
    </section>
  );
}
