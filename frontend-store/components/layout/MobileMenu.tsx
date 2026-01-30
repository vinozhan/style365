'use client';

import Link from 'next/link';
import { ChevronRight, X, User, Heart, Package, LogOut } from 'lucide-react';
import { useState } from 'react';
import { Sheet, SheetContent, SheetHeader, SheetTitle } from '@/components/ui/sheet';
import { Button } from '@/components/ui/button';
import { Separator } from '@/components/ui/separator';
import { useUIStore } from '@/stores/uiStore';
import { useAuthStore } from '@/stores/authStore';
import { useCategories } from '@/features/categories/hooks/useCategories';
import { useLogout } from '@/features/auth/hooks/useAuth';
import type { Category } from '@/types';

export function MobileMenu() {
  const { isMobileMenuOpen, closeMobileMenu } = useUIStore();
  const { isAuthenticated, user } = useAuthStore();
  const { data: categories } = useCategories();
  const { mutate: logout } = useLogout();
  const [expandedCategory, setExpandedCategory] = useState<string | null>(null);

  const handleCategoryClick = (category: Category) => {
    if (category.subCategories?.length > 0) {
      setExpandedCategory(expandedCategory === category.id ? null : category.id);
    } else {
      closeMobileMenu();
    }
  };

  const handleLogout = () => {
    logout();
    closeMobileMenu();
  };

  return (
    <Sheet open={isMobileMenuOpen} onOpenChange={closeMobileMenu}>
      <SheetContent side="left" className="w-80 p-0">
        <SheetHeader className="border-b p-4">
          <SheetTitle className="text-left">Menu</SheetTitle>
        </SheetHeader>

        <div className="flex h-full flex-col">
          {/* User section */}
          <div className="border-b p-4">
            {isAuthenticated ? (
              <div className="flex items-center gap-3">
                <div className="flex h-10 w-10 items-center justify-center rounded-full bg-slate-100">
                  <User className="h-5 w-5" />
                </div>
                <div>
                  <p className="font-medium">{user?.firstName} {user?.lastName}</p>
                  <p className="text-sm text-slate-500">{user?.email}</p>
                </div>
              </div>
            ) : (
              <div className="flex gap-2">
                <Link href="/login" onClick={closeMobileMenu} className="flex-1">
                  <Button className="w-full">Login</Button>
                </Link>
                <Link href="/register" onClick={closeMobileMenu} className="flex-1">
                  <Button variant="outline" className="w-full">Register</Button>
                </Link>
              </div>
            )}
          </div>

          {/* Navigation */}
          <nav className="flex-1 overflow-y-auto">
            <div className="p-2">
              <Link
                href="/products"
                onClick={closeMobileMenu}
                className="flex items-center justify-between rounded-md px-3 py-3 font-medium hover:bg-slate-100"
              >
                All Products
                <ChevronRight className="h-4 w-4" />
              </Link>

              {categories?.map((category) => (
                <div key={category.id}>
                  <div
                    className="flex cursor-pointer items-center justify-between rounded-md px-3 py-3 hover:bg-slate-100"
                    onClick={() => handleCategoryClick(category)}
                  >
                    {category.subCategories?.length > 0 ? (
                      <>
                        <span className="font-medium">{category.name}</span>
                        <ChevronRight
                          className={`h-4 w-4 transition-transform ${
                            expandedCategory === category.id ? 'rotate-90' : ''
                          }`}
                        />
                      </>
                    ) : (
                      <Link
                        href={`/categories/${category.slug}`}
                        onClick={closeMobileMenu}
                        className="flex w-full items-center justify-between font-medium"
                      >
                        {category.name}
                        <ChevronRight className="h-4 w-4" />
                      </Link>
                    )}
                  </div>

                  {/* Subcategories */}
                  {expandedCategory === category.id && category.subCategories?.length > 0 && (
                    <div className="ml-4 border-l pl-4">
                      {category.subCategories.map((sub) => (
                        <Link
                          key={sub.id}
                          href={`/categories/${sub.slug}`}
                          onClick={closeMobileMenu}
                          className="block rounded-md px-3 py-2 text-sm text-slate-600 hover:bg-slate-100"
                        >
                          {sub.name}
                        </Link>
                      ))}
                    </div>
                  )}
                </div>
              ))}
            </div>

            <Separator />

            {/* Account links */}
            {isAuthenticated && (
              <div className="p-2">
                <Link
                  href="/account"
                  onClick={closeMobileMenu}
                  className="flex items-center gap-3 rounded-md px-3 py-3 hover:bg-slate-100"
                >
                  <User className="h-5 w-5" />
                  My Account
                </Link>
                <Link
                  href="/account/orders"
                  onClick={closeMobileMenu}
                  className="flex items-center gap-3 rounded-md px-3 py-3 hover:bg-slate-100"
                >
                  <Package className="h-5 w-5" />
                  Orders
                </Link>
                <Link
                  href="/account/wishlist"
                  onClick={closeMobileMenu}
                  className="flex items-center gap-3 rounded-md px-3 py-3 hover:bg-slate-100"
                >
                  <Heart className="h-5 w-5" />
                  Wishlist
                </Link>
                <button
                  onClick={handleLogout}
                  className="flex w-full items-center gap-3 rounded-md px-3 py-3 text-red-600 hover:bg-red-50"
                >
                  <LogOut className="h-5 w-5" />
                  Logout
                </button>
              </div>
            )}

            <Separator />

            {/* Other links */}
            <div className="p-2">
              <Link
                href="/track-order"
                onClick={closeMobileMenu}
                className="block rounded-md px-3 py-3 text-sm hover:bg-slate-100"
              >
                Track Order
              </Link>
              <Link
                href="/contact"
                onClick={closeMobileMenu}
                className="block rounded-md px-3 py-3 text-sm hover:bg-slate-100"
              >
                Contact Us
              </Link>
              <Link
                href="/faq"
                onClick={closeMobileMenu}
                className="block rounded-md px-3 py-3 text-sm hover:bg-slate-100"
              >
                FAQs
              </Link>
            </div>
          </nav>
        </div>
      </SheetContent>
    </Sheet>
  );
}
