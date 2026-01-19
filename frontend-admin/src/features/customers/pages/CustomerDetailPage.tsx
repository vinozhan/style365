import { Link, useParams } from 'react-router-dom';
import { ArrowLeft, Mail, Phone, Calendar, ShoppingCart } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { OrderStatusBadge } from '@/components/common/StatusBadge';
import { PageLoader } from '@/components/common/LoadingSpinner';
import { EmptyState } from '@/components/common/EmptyState';
import { useCustomer, useCustomerOrders } from '../hooks/useCustomers';
import { formatCurrency, formatDate, formatDateTime } from '@/lib/utils';

export function CustomerDetailPage() {
  const { id } = useParams<{ id: string }>();
  const { data: customer, isLoading: customerLoading } = useCustomer(id!);
  const { data: orders, isLoading: ordersLoading } = useCustomerOrders(id!);

  if (customerLoading) {
    return <PageLoader />;
  }

  if (!customer) {
    return (
      <EmptyState
        title="Customer not found"
        description="The customer you're looking for doesn't exist."
        action={
          <Button asChild>
            <Link to="/customers">Back to Customers</Link>
          </Button>
        }
      />
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" asChild>
          <Link to="/customers">
            <ArrowLeft className="h-4 w-4" />
          </Link>
        </Button>
        <div>
          <h1 className="text-2xl font-bold text-slate-900">
            {customer.firstName} {customer.lastName}
          </h1>
          <p className="text-slate-500">Customer Profile</p>
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        {/* Profile Info */}
        <Card>
          <CardHeader>
            <CardTitle>Profile Information</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-center justify-center">
              <div className="flex h-20 w-20 items-center justify-center rounded-full bg-slate-200 text-2xl font-medium text-slate-700">
                {customer.firstName.charAt(0)}
                {customer.lastName.charAt(0)}
              </div>
            </div>

            <div className="space-y-3">
              <div className="flex items-center gap-2 text-sm">
                <Mail className="h-4 w-4 text-slate-400" />
                <span>{customer.email}</span>
                {customer.isEmailVerified ? (
                  <Badge variant="success" className="text-xs">
                    Verified
                  </Badge>
                ) : (
                  <Badge variant="warning" className="text-xs">
                    Unverified
                  </Badge>
                )}
              </div>

              {customer.phone && (
                <div className="flex items-center gap-2 text-sm">
                  <Phone className="h-4 w-4 text-slate-400" />
                  <span>{customer.phone}</span>
                </div>
              )}

              <div className="flex items-center gap-2 text-sm">
                <Calendar className="h-4 w-4 text-slate-400" />
                <span>Joined {formatDate(customer.createdAt)}</span>
              </div>

              {customer.lastLoginAt && (
                <div className="flex items-center gap-2 text-sm text-slate-500">
                  Last login: {formatDateTime(customer.lastLoginAt)}
                </div>
              )}
            </div>
          </CardContent>
        </Card>

        {/* Stats */}
        <Card className="lg:col-span-2">
          <CardHeader>
            <CardTitle>Customer Stats</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="rounded-lg bg-slate-50 p-4">
                <p className="text-sm text-slate-500">Total Orders</p>
                <p className="mt-1 text-2xl font-bold">{customer.ordersCount || 0}</p>
              </div>
              <div className="rounded-lg bg-slate-50 p-4">
                <p className="text-sm text-slate-500">Total Spent</p>
                <p className="mt-1 text-2xl font-bold">{formatCurrency(customer.totalSpent || 0)}</p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Order History */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <ShoppingCart className="h-5 w-5" />
            Order History
          </CardTitle>
        </CardHeader>
        <CardContent>
          {ordersLoading ? (
            <div className="flex h-32 items-center justify-center">
              <p className="text-slate-500">Loading orders...</p>
            </div>
          ) : orders && orders.length > 0 ? (
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Order</TableHead>
                  <TableHead>Date</TableHead>
                  <TableHead>Status</TableHead>
                  <TableHead>Items</TableHead>
                  <TableHead className="text-right">Total</TableHead>
                  <TableHead></TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {orders.map((order) => (
                  <TableRow key={order.id}>
                    <TableCell className="font-medium">#{order.orderNumber}</TableCell>
                    <TableCell>{formatDate(order.createdAt)}</TableCell>
                    <TableCell>
                      <OrderStatusBadge status={order.status} />
                    </TableCell>
                    <TableCell>{order.items.length} items</TableCell>
                    <TableCell className="text-right">{formatCurrency(order.totalAmount)}</TableCell>
                    <TableCell>
                      <Button variant="ghost" size="sm" asChild>
                        <Link to={`/orders/${order.id}`}>View</Link>
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          ) : (
            <div className="flex h-32 items-center justify-center">
              <p className="text-slate-500">No orders yet</p>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
