'use client';

import { useEffect } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { useAuthStore } from '@/stores/authStore';
import { AccountSidebar } from '@/components/layout/AccountSidebar';
import { Header } from '@/components/layout/Header';
import { Footer } from '@/components/layout/Footer';
import { MobileMenu } from '@/components/layout/MobileMenu';
import { CartDrawer } from '@/components/layout/CartDrawer';
import { Skeleton } from '@/components/ui/skeleton';

export default function AccountLayout({ children }: { children: React.ReactNode }) {
  const router = useRouter();
  const { user, isLoading, isAuthenticated } = useAuthStore();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.push('/login?redirect=/account');
    }
  }, [isLoading, isAuthenticated, router]);

  if (isLoading) {
    return (
      <div className="flex min-h-screen flex-col">
        <Header />
        <main className="flex-1">
          <div className="container-custom py-8">
            <div className="flex gap-8">
              <div className="hidden w-64 lg:block">
                <Skeleton className="h-64 rounded-lg" />
              </div>
              <div className="flex-1">
                <Skeleton className="h-96 rounded-lg" />
              </div>
            </div>
          </div>
        </main>
        <Footer />
      </div>
    );
  }

  if (!isAuthenticated) {
    return null; // Will redirect
  }

  return (
    <div className="flex min-h-screen flex-col">
      <Header />
      <MobileMenu />
      <CartDrawer />

      <main className="flex-1">
        <div className="container-custom py-8">
          {/* Breadcrumbs */}
          <nav className="mb-6 flex items-center gap-2 text-sm text-slate-600">
            <Link href="/" className="hover:text-slate-900">
              Home
            </Link>
            <span>/</span>
            <span className="text-slate-900">My Account</span>
          </nav>

          <div className="flex flex-col gap-8 lg:flex-row">
            {/* Sidebar */}
            <aside className="w-full lg:w-64">
              <div className="rounded-lg border p-4">
                {/* User info */}
                <div className="mb-4 pb-4 border-b">
                  <p className="font-semibold">
                    {user?.firstName} {user?.lastName}
                  </p>
                  <p className="text-sm text-slate-500">{user?.email}</p>
                </div>
                <AccountSidebar />
              </div>
            </aside>

            {/* Main content */}
            <div className="flex-1">{children}</div>
          </div>
        </div>
      </main>

      <Footer />
    </div>
  );
}
