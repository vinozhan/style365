import { ShoppingCart, DollarSign, Package, AlertTriangle } from 'lucide-react';
import { StatsCard } from '../components/StatsCard';
import { RecentOrdersTable } from '../components/RecentOrdersTable';
import { useDashboardStats } from '../hooks/useDashboard';
import { formatCurrency, formatNumber } from '@/lib/utils';
import { PageLoader } from '@/components/common/LoadingSpinner';

export function DashboardPage() {
  const { data: stats, isLoading } = useDashboardStats();

  if (isLoading) {
    return <PageLoader />;
  }

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-slate-900">Dashboard</h1>
        <p className="text-slate-500">Welcome back! Here's what's happening with your store.</p>
      </div>

      {/* Stats Grid */}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatsCard
          title="Total Orders"
          value={formatNumber(stats?.totalOrders || 0)}
          icon={ShoppingCart}
          description={`${stats?.pendingOrdersCount || 0} pending`}
        />
        <StatsCard
          title="Total Revenue"
          value={formatCurrency(stats?.totalRevenue || 0)}
          icon={DollarSign}
        />
        <StatsCard
          title="Total Products"
          value={formatNumber(stats?.totalProducts || 0)}
          icon={Package}
        />
        <StatsCard
          title="Low Stock Alerts"
          value={stats?.lowStockCount || 0}
          icon={AlertTriangle}
          className={stats?.lowStockCount ? 'border-yellow-200 bg-yellow-50' : ''}
        />
      </div>

      {/* Recent Orders */}
      <RecentOrdersTable orders={stats?.recentOrders || []} />
    </div>
  );
}
