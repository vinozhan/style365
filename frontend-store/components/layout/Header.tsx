'use client';

import Link from 'next/link';
import { useState } from 'react';
import { Menu, Search, ShoppingBag, Heart, User, ChevronDown } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { useAuthStore } from '@/stores/authStore';
import { useCartStore } from '@/stores/cartStore';
import { useUIStore } from '@/stores/uiStore';
import { useCategories } from '@/features/categories/hooks/useCategories';
import { useLogout } from '@/features/auth/hooks/useAuth';
import { SearchBar } from './SearchBar';
import { cn } from '@/lib/utils';

export function Header() {
  const { isAuthenticated, user } = useAuthStore();
  const { cart, openCart } = useCartStore();
  const { openMobileMenu, openSearch, isSearchOpen } = useUIStore();
  const { data: categories } = useCategories();
  const { mutate: logout } = useLogout();
  const [hoveredCategory, setHoveredCategory] = useState<string | null>(null);

  const totalItems = cart?.totalItems ?? 0;

  return (
    <header className="sticky top-0 z-40 w-full border-b bg-white">
      {/* Top bar */}
      <div className="hidden border-b bg-slate-900 text-white md:block">
        <div className="container-custom flex h-8 items-center justify-between text-xs">
          <p>Free shipping on orders over Rs. 5,000</p>
          <div className="flex items-center gap-4">
            <Link href="/track-order" className="hover:underline">
              Track Order
            </Link>
            <span>|</span>
            <span>Need Help? Call: +94 11 234 5678</span>
          </div>
        </div>
      </div>

      {/* Main header */}
      <div className="container-custom">
        <div className="flex h-16 items-center justify-between gap-4">
          {/* Mobile menu button */}
          <Button
            variant="ghost"
            size="icon"
            className="md:hidden"
            onClick={openMobileMenu}
            aria-label="Open menu"
          >
            <Menu className="h-6 w-6" />
          </Button>

          {/* Logo */}
          <Link href="/" className="flex items-center gap-2">
            <span className="text-xl font-bold tracking-tight">Style365</span>
          </Link>

          {/* Desktop Navigation */}
          <nav className="hidden flex-1 items-center justify-center gap-6 md:flex">
            <Link href="/products" className="text-sm font-medium hover:text-slate-600">
              All Products
            </Link>
            {categories?.slice(0, 5).map((category) => (
              <div
                key={category.id}
                className="relative"
                onMouseEnter={() => setHoveredCategory(category.id)}
                onMouseLeave={() => setHoveredCategory(null)}
              >
                <Link
                  href={`/categories/${category.slug}`}
                  className="flex items-center gap-1 text-sm font-medium hover:text-slate-600"
                >
                  {category.name}
                  {category.subCategories?.length > 0 && <ChevronDown className="h-3 w-3" />}
                </Link>
                {category.subCategories?.length > 0 && hoveredCategory === category.id && (
                  <div className="absolute left-0 top-full z-50 w-48 rounded-md border bg-white py-2 shadow-lg">
                    {category.subCategories.map((sub) => (
                      <Link
                        key={sub.id}
                        href={`/categories/${sub.slug}`}
                        className="block px-4 py-2 text-sm hover:bg-slate-100"
                      >
                        {sub.name}
                      </Link>
                    ))}
                  </div>
                )}
              </div>
            ))}
          </nav>

          {/* Actions */}
          <div className="flex items-center gap-2">
            {/* Search */}
            <Button
              variant="ghost"
              size="icon"
              onClick={openSearch}
              aria-label="Search"
              className="hidden sm:flex"
            >
              <Search className="h-5 w-5" />
            </Button>

            {/* Wishlist */}
            {isAuthenticated && (
              <Link href="/account/wishlist">
                <Button variant="ghost" size="icon" aria-label="Wishlist">
                  <Heart className="h-5 w-5" />
                </Button>
              </Link>
            )}

            {/* User menu */}
            {isAuthenticated ? (
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <Button variant="ghost" size="icon" aria-label="Account">
                    <User className="h-5 w-5" />
                  </Button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="w-48">
                  <div className="px-2 py-1.5 text-sm font-medium">
                    Hi, {user?.firstName}
                  </div>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem asChild>
                    <Link href="/account">My Account</Link>
                  </DropdownMenuItem>
                  <DropdownMenuItem asChild>
                    <Link href="/account/orders">Orders</Link>
                  </DropdownMenuItem>
                  <DropdownMenuItem asChild>
                    <Link href="/account/wishlist">Wishlist</Link>
                  </DropdownMenuItem>
                  <DropdownMenuSeparator />
                  <DropdownMenuItem onClick={() => logout()}>Logout</DropdownMenuItem>
                </DropdownMenuContent>
              </DropdownMenu>
            ) : (
              <Link href="/login">
                <Button variant="ghost" size="sm">
                  Login
                </Button>
              </Link>
            )}

            {/* Cart */}
            <Button
              variant="ghost"
              size="icon"
              className="relative"
              onClick={openCart}
              aria-label="Cart"
            >
              <ShoppingBag className="h-5 w-5" />
              {totalItems > 0 && (
                <span className="absolute -right-1 -top-1 flex h-5 w-5 items-center justify-center rounded-full bg-slate-900 text-xs text-white">
                  {totalItems > 9 ? '9+' : totalItems}
                </span>
              )}
            </Button>
          </div>
        </div>
      </div>

      {/* Search overlay */}
      {isSearchOpen && <SearchBar />}
    </header>
  );
}
