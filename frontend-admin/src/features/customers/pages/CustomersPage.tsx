import { useState } from 'react';
import { Link } from 'react-router-dom';
import { type ColumnDef } from '@tanstack/react-table';
import { Eye, Mail, Trash2, Loader2 } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog';
import { DataTable } from '@/components/common/DataTable';
import { Pagination } from '@/components/common/Pagination';
import { SearchInput } from '@/components/common/SearchInput';
import { useCustomers, useDeleteCustomer } from '../hooks/useCustomers';
import { formatCurrency, formatDate } from '@/lib/utils';
import type { Customer } from '@/types';

export function CustomersPage() {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [search, setSearch] = useState('');
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [customerToDelete, setCustomerToDelete] = useState<Customer | null>(null);

  const { data, isLoading } = useCustomers({
    page,
    pageSize,
    search: search || undefined,
    sortBy: 'createdAt',
    sortOrder: 'desc',
  });

  const deleteCustomer = useDeleteCustomer();

  const handleDeleteClick = (customer: Customer) => {
    setCustomerToDelete(customer);
    setDeleteDialogOpen(true);
  };

  const handleDeleteConfirm = async () => {
    if (!customerToDelete) return;

    try {
      await deleteCustomer.mutateAsync(customerToDelete.id);
      toast.success(`Customer "${customerToDelete.firstName} ${customerToDelete.lastName}" has been deleted`);
      setDeleteDialogOpen(false);
      setCustomerToDelete(null);
    } catch {
      toast.error('Failed to delete customer. Please try again.');
    }
  };

  const columns: ColumnDef<Customer>[] = [
    {
      accessorKey: 'name',
      header: 'Customer',
      cell: ({ row }) => (
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-full bg-slate-200 font-medium text-slate-700">
            {row.original.firstName.charAt(0)}
            {row.original.lastName.charAt(0)}
          </div>
          <div>
            <p className="font-medium">
              {row.original.firstName} {row.original.lastName}
            </p>
            <div className="flex items-center gap-1 text-sm text-slate-500">
              <Mail className="h-3 w-3" />
              {row.original.email}
            </div>
          </div>
        </div>
      ),
    },
    {
      accessorKey: 'isEmailVerified',
      header: 'Status',
      cell: ({ row }) => (
        <Badge variant={row.original.isEmailVerified ? 'success' : 'warning'}>
          {row.original.isEmailVerified ? 'Verified' : 'Unverified'}
        </Badge>
      ),
    },
    {
      accessorKey: 'ordersCount',
      header: 'Orders',
      cell: ({ row }) => row.original.ordersCount || 0,
    },
    {
      accessorKey: 'totalSpent',
      header: 'Total Spent',
      cell: ({ row }) => formatCurrency(row.original.totalSpent || 0),
    },
    {
      accessorKey: 'createdAt',
      header: 'Joined',
      cell: ({ row }) => formatDate(row.original.createdAt),
    },
    {
      id: 'actions',
      cell: ({ row }) => (
        <div className="flex items-center gap-1">
          <Button variant="ghost" size="icon" asChild>
            <Link to={`/customers/${row.original.id}`}>
              <Eye className="h-4 w-4" />
            </Link>
          </Button>
          <Button
            variant="ghost"
            size="icon"
            className="text-red-600 hover:bg-red-50 hover:text-red-700"
            onClick={() => handleDeleteClick(row.original)}
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-bold text-slate-900">Customers</h1>
        <p className="text-slate-500">View and manage customer accounts</p>
      </div>

      <SearchInput
        value={search}
        onChange={(v) => {
          setSearch(v);
          setPage(1);
        }}
        placeholder="Search customers..."
        className="max-w-md"
      />

      <DataTable
        columns={columns}
        data={data?.items || []}
        isLoading={isLoading}
        emptyMessage="No customers found"
        emptyDescription="Customers will appear here when they create accounts"
      />

      {data && data.totalPages > 0 && (
        <Pagination
          currentPage={data.page}
          totalPages={data.totalPages}
          pageSize={pageSize}
          totalItems={data.totalItems}
          onPageChange={setPage}
          onPageSizeChange={(size) => {
            setPageSize(size);
            setPage(1);
          }}
        />
      )}

      {/* Delete Confirmation Dialog */}
      <AlertDialog open={deleteDialogOpen} onOpenChange={setDeleteDialogOpen}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Delete Customer</AlertDialogTitle>
            <AlertDialogDescription>
              Are you sure you want to delete{' '}
              <span className="font-semibold">
                {customerToDelete?.firstName} {customerToDelete?.lastName}
              </span>
              ? This action cannot be undone. The customer will be removed from both the
              authentication system and the database.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={deleteCustomer.isPending}>Cancel</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleDeleteConfirm}
              disabled={deleteCustomer.isPending}
              className="bg-red-600 hover:bg-red-700 focus:ring-red-600"
            >
              {deleteCustomer.isPending ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Deleting...
                </>
              ) : (
                'Delete'
              )}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}
