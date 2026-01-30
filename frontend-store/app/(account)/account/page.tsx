'use client';

import Link from 'next/link';
import { Package, Heart, MapPin, User } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { useAuthStore } from '@/stores/authStore';
import { useOrders } from '@/features/orders/hooks/useOrders';
import { useWishlist } from '@/features/wishlists/hooks/useWishlist';

export default function AccountPage() {
  const { user } = useAuthStore();
  const { data: orders } = useOrders(1, 5);
  const { data: wishlist } = useWishlist();

  const quickLinks = [
    {
      href: '/account/orders',
      icon: Package,
      title: 'Orders',
      description: `${orders?.totalItems ?? 0} orders`,
    },
    {
      href: '/account/wishlist',
      icon: Heart,
      title: 'Wishlist',
      description: `${wishlist?.length ?? 0} items`,
    },
    {
      href: '/account/addresses',
      icon: MapPin,
      title: 'Addresses',
      description: 'Manage addresses',
    },
    {
      href: '/account/profile',
      icon: User,
      title: 'Profile',
      description: 'Edit profile',
    },
  ];

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold">
        Welcome back, {user?.firstName}!
      </h1>

      {/* Quick links */}
      <div className="grid gap-4 sm:grid-cols-2">
        {quickLinks.map((link) => {
          const Icon = link.icon;
          return (
            <Link key={link.href} href={link.href}>
              <Card className="transition-shadow hover:shadow-md">
                <CardContent className="flex items-center gap-4 p-6">
                  <div className="rounded-full bg-slate-100 p-3">
                    <Icon className="h-6 w-6 text-slate-600" />
                  </div>
                  <div>
                    <p className="font-semibold">{link.title}</p>
                    <p className="text-sm text-slate-500">{link.description}</p>
                  </div>
                </CardContent>
              </Card>
            </Link>
          );
        })}
      </div>

      {/* Recent orders */}
      {orders && orders.items.length > 0 && (
        <div className="mt-8">
          <div className="mb-4 flex items-center justify-between">
            <h2 className="text-lg font-semibold">Recent Orders</h2>
            <Link
              href="/account/orders"
              className="text-sm text-slate-600 hover:text-slate-900"
            >
              View all
            </Link>
          </div>
          <div className="space-y-4">
            {orders.items.slice(0, 3).map((order) => (
              <Card key={order.id}>
                <CardContent className="flex items-center justify-between p-4">
                  <div>
                    <p className="font-medium">Order #{order.orderNumber}</p>
                    <p className="text-sm text-slate-500">
                      {new Date(order.orderDate).toLocaleDateString()}
                    </p>
                  </div>
                  <div className="text-right">
                    <p className="font-medium">${order.totalAmount.toFixed(2)}</p>
                    <p className="text-sm text-slate-500">{order.status}</p>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>
      )}
    </div>
  );
}
