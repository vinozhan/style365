'use client';

import { useState } from 'react';
import Link from 'next/link';
import { useParams } from 'next/navigation';
import { Heart, Share2, Truck, RefreshCcw, Shield, ChevronRight } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Separator } from '@/components/ui/separator';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Skeleton } from '@/components/ui/skeleton';
import { ProductGallery } from '@/components/products/ProductGallery';
import { ProductVariants } from '@/components/products/ProductVariants';
import { AddToCartButton } from '@/components/products/AddToCartButton';
import { ProductGrid } from '@/components/products/ProductGrid';
import { ProductReviews } from '@/components/products/ProductReviews';
import { useProduct, useProductsByCategory } from '@/features/products/hooks/useProducts';
import { useWishlistStatus, useToggleWishlist } from '@/features/wishlists/hooks/useWishlist';
import { useAuthStore } from '@/stores/authStore';
import { useUIStore } from '@/stores/uiStore';
import { formatCurrency, cn } from '@/lib/utils';
import type { ProductVariant } from '@/types';

export default function ProductDetailPage() {
  const params = useParams();
  const slug = params.slug as string;
  const { isAuthenticated } = useAuthStore();
  const { openAuthModal } = useUIStore();

  const { data: product, isLoading, error } = useProduct(slug);
  const { data: relatedProducts } = useProductsByCategory(product?.categoryId ?? '', 1, 4);
  const { data: wishlistStatus } = useWishlistStatus(product?.id ?? '');
  const { toggle: toggleWishlist, isLoading: isWishlistLoading } = useToggleWishlist();

  const [selectedVariant, setSelectedVariant] = useState<ProductVariant | null>(null);

  const handleWishlistClick = () => {
    if (!product) return;
    if (!isAuthenticated) {
      openAuthModal('/account/wishlist');
      return;
    }
    toggleWishlist(product.id, wishlistStatus?.isInWishlist ?? false);
  };

  if (isLoading) {
    return (
      <div className="container-custom py-8">
        <div className="grid gap-8 lg:grid-cols-2">
          <Skeleton className="aspect-square w-full rounded-lg" />
          <div className="space-y-6">
            <Skeleton className="h-8 w-3/4" />
            <Skeleton className="h-6 w-1/4" />
            <Skeleton className="h-24 w-full" />
            <Skeleton className="h-12 w-full" />
          </div>
        </div>
      </div>
    );
  }

  if (error || !product) {
    return (
      <div className="container-custom py-16 text-center">
        <h1 className="text-2xl font-bold">Product Not Found</h1>
        <p className="mt-2 text-slate-600">The product you&apos;re looking for doesn&apos;t exist.</p>
        <Button className="mt-6" asChild>
          <Link href="/products">Browse Products</Link>
        </Button>
      </div>
    );
  }

  const hasDiscount = product.compareAtPrice && product.compareAtPrice > product.price;
  const discountPercentage = hasDiscount
    ? Math.round(((product.compareAtPrice! - product.price) / product.compareAtPrice!) * 100)
    : 0;

  const displayPrice = selectedVariant?.price ?? product.price;

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
        {product.categoryName && (
          <>
            <Link href={`/categories/${product.categoryId}`} className="hover:text-slate-900">
              {product.categoryName}
            </Link>
            <ChevronRight className="h-4 w-4" />
          </>
        )}
        <span className="text-slate-900">{product.name}</span>
      </nav>

      <div className="grid gap-8 lg:grid-cols-2">
        {/* Gallery */}
        <ProductGallery images={product.images} productName={product.name} />

        {/* Product info */}
        <div>
          {/* Title & badges */}
          <div className="flex flex-wrap items-start gap-2">
            {product.isFeatured && <Badge>Featured</Badge>}
            {hasDiscount && <Badge variant="destructive">-{discountPercentage}%</Badge>}
            {!product.isInStock && <Badge variant="outline">Out of Stock</Badge>}
          </div>

          <h1 className="mt-2 text-2xl font-bold md:text-3xl">{product.name}</h1>

          {product.brand && (
            <p className="mt-1 text-slate-600">
              Brand: <span className="font-medium">{product.brand}</span>
            </p>
          )}

          {/* Price */}
          <div className="mt-4 flex items-baseline gap-3">
            <span className="text-3xl font-bold">{formatCurrency(displayPrice)}</span>
            {hasDiscount && (
              <span className="text-xl text-slate-500 line-through">
                {formatCurrency(product.compareAtPrice!)}
              </span>
            )}
          </div>

          {/* Short description */}
          {product.shortDescription && (
            <p className="mt-4 text-slate-600">{product.shortDescription}</p>
          )}

          <Separator className="my-6" />

          {/* Variants */}
          {product.variants.length > 0 && (
            <div className="mb-6">
              <ProductVariants
                variants={product.variants}
                selectedVariant={selectedVariant}
                onSelect={setSelectedVariant}
              />
            </div>
          )}

          {/* Add to cart */}
          <AddToCartButton product={product} selectedVariant={selectedVariant} />

          {/* Actions */}
          <div className="mt-4 flex gap-4">
            <Button
              variant="outline"
              onClick={handleWishlistClick}
              disabled={isWishlistLoading}
              className={cn(wishlistStatus?.isInWishlist && 'text-red-500')}
            >
              <Heart className={cn('mr-2 h-4 w-4', wishlistStatus?.isInWishlist && 'fill-current')} />
              {wishlistStatus?.isInWishlist ? 'In Wishlist' : 'Add to Wishlist'}
            </Button>
            <Button variant="outline">
              <Share2 className="mr-2 h-4 w-4" />
              Share
            </Button>
          </div>

          {/* Features */}
          <div className="mt-8 grid gap-4 sm:grid-cols-3">
            <div className="flex items-center gap-3 rounded-lg border p-3">
              <Truck className="h-5 w-5 text-slate-600" />
              <div className="text-sm">
                <p className="font-medium">Free Shipping</p>
                <p className="text-slate-500">On orders over Rs. 5,000</p>
              </div>
            </div>
            <div className="flex items-center gap-3 rounded-lg border p-3">
              <RefreshCcw className="h-5 w-5 text-slate-600" />
              <div className="text-sm">
                <p className="font-medium">Easy Returns</p>
                <p className="text-slate-500">30-day return policy</p>
              </div>
            </div>
            <div className="flex items-center gap-3 rounded-lg border p-3">
              <Shield className="h-5 w-5 text-slate-600" />
              <div className="text-sm">
                <p className="font-medium">Secure Payment</p>
                <p className="text-slate-500">100% secure checkout</p>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Tabs */}
      <Tabs defaultValue="description" className="mt-12">
        <TabsList>
          <TabsTrigger value="description">Description</TabsTrigger>
          <TabsTrigger value="details">Details</TabsTrigger>
          <TabsTrigger value="reviews">Reviews</TabsTrigger>
        </TabsList>
        <TabsContent value="description" className="mt-6">
          <div className="prose max-w-none">
            {product.description || <p>No description available.</p>}
          </div>
        </TabsContent>
        <TabsContent value="details" className="mt-6">
          <dl className="grid gap-4 sm:grid-cols-2">
            <div>
              <dt className="font-medium">SKU</dt>
              <dd className="text-slate-600">{product.sku}</dd>
            </div>
            <div>
              <dt className="font-medium">Category</dt>
              <dd className="text-slate-600">{product.categoryName}</dd>
            </div>
            {product.brand && (
              <div>
                <dt className="font-medium">Brand</dt>
                <dd className="text-slate-600">{product.brand}</dd>
              </div>
            )}
            {product.weight > 0 && (
              <div>
                <dt className="font-medium">Weight</dt>
                <dd className="text-slate-600">
                  {product.weight} {product.weightUnit}
                </dd>
              </div>
            )}
            {product.tags.length > 0 && (
              <div className="sm:col-span-2">
                <dt className="font-medium">Tags</dt>
                <dd className="mt-1 flex flex-wrap gap-2">
                  {product.tags.map((tag) => (
                    <Badge key={tag} variant="secondary">
                      {tag}
                    </Badge>
                  ))}
                </dd>
              </div>
            )}
          </dl>
        </TabsContent>
        <TabsContent value="reviews" className="mt-6">
          <ProductReviews productId={product.id} />
        </TabsContent>
      </Tabs>

      {/* Related products */}
      {relatedProducts && relatedProducts.items.length > 0 && (
        <section className="mt-16">
          <h2 className="mb-6 text-2xl font-bold">You May Also Like</h2>
          <ProductGrid
            products={relatedProducts.items.filter((p) => p.id !== product.id).slice(0, 4)}
            columns={4}
          />
        </section>
      )}
    </div>
  );
}
