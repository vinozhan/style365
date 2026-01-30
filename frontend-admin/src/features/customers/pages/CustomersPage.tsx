import { useState } from 'react';
import { Link } from 'react-router-dom';
import { type ColumnDef } from '@tanstack/react-table';
import { Eye, Mail } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { DataTable } from '@/components/common/DataTable';
import { Pagination } from '@/components/common/Pagination';
import { SearchInput } from '@/components/common/SearchInput';
import { useCustomers } from '../hooks/useCustomers';
import { formatCurrency, formatDate } from '@/lib/utils';
import type { Customer } from '@/types';

export function CustomersPage() {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [search, setSearch] = useState('');

  const { data, isLoading } = useCustomers({
    page,
    pageSize,
    search: search || undefined,
    sortBy: 'createdAt',
    sortOrder: 'desc',
  });

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
        <Button variant="ghost" size="icon" asChild>
          <Link to={`/customers/${row.original.id}`}>
            <Eye className="h-4 w-4" />
          </Link>
        </Button>
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
    </div>
  );
}
