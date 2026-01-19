import { useState } from 'react';
import { Link } from 'react-router-dom';
import { type ColumnDef } from '@tanstack/react-table';
import { Plus, MoreHorizontal, Pencil, Trash2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { DataTable } from '@/components/common/DataTable';
import { Pagination } from '@/components/common/Pagination';
import { ConfirmDialog } from '@/components/common/ConfirmDialog';
import { StockBadge, ActiveBadge } from '@/components/common/StatusBadge';
import { ProductFilters } from '../components/ProductFilters';
import { useProducts, useDeleteProduct, useCategories } from '../hooks/useProducts';
import { formatCurrency } from '@/lib/utils';
import type { Product } from '@/types';

export function ProductsPage() {
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [search, setSearch] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [isActive, setIsActive] = useState('all');
  const [stockStatus, setStockStatus] = useState('all');
  const [deleteId, setDeleteId] = useState<string | null>(null);

  const { data: categoriesData } = useCategories();
  const { data, isLoading } = useProducts({
    pageNumber: page,
    pageSize,
    search: search || undefined,
    categoryId: categoryId || undefined,
    isActive: isActive === 'all' ? undefined : isActive === 'true',
    stockStatus: stockStatus as 'all' | 'inStock' | 'lowStock' | 'outOfStock',
  });

  const deleteMutation = useDeleteProduct();

  const handleClearFilters = () => {
    setSearch('');
    setCategoryId('');
    setIsActive('all');
    setStockStatus('all');
    setPage(1);
  };

  const handleDelete = () => {
    if (deleteId) {
      deleteMutation.mutate(deleteId, {
        onSuccess: () => setDeleteId(null),
      });
    }
  };

  const columns: ColumnDef<Product>[] = [
    {
      accessorKey: 'name',
      header: 'Product',
      cell: ({ row }) => (
        <div className="flex items-center gap-3">
          {row.original.images?.[0] ? (
            <img
              src={row.original.images[0].url}
              alt={row.original.name}
              className="h-10 w-10 rounded object-cover"
            />
          ) : (
            <div className="flex h-10 w-10 items-center justify-center rounded bg-slate-100 text-slate-400">
              No img
            </div>
          )}
          <div>
            <p className="font-medium">{row.original.name}</p>
            <p className="text-sm text-slate-500">{row.original.sku}</p>
          </div>
        </div>
      ),
    },
    {
      accessorKey: 'category',
      header: 'Category',
      cell: ({ row }) => row.original.category?.name || '-',
    },
    {
      accessorKey: 'price',
      header: 'Price',
      cell: ({ row }) => (
        <div>
          <p>{formatCurrency(row.original.price)}</p>
          {row.original.compareAtPrice && row.original.compareAtPrice > row.original.price && (
            <p className="text-sm text-slate-500 line-through">
              {formatCurrency(row.original.compareAtPrice)}
            </p>
          )}
        </div>
      ),
    },
    {
      accessorKey: 'stockQuantity',
      header: 'Stock',
      cell: ({ row }) => (
        <StockBadge
          quantity={row.original.stockQuantity}
          lowThreshold={row.original.lowStockThreshold}
        />
      ),
    },
    {
      accessorKey: 'isActive',
      header: 'Status',
      cell: ({ row }) => <ActiveBadge isActive={row.original.isActive} />,
    },
    {
      id: 'actions',
      cell: ({ row }) => (
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="ghost" size="icon">
              <MoreHorizontal className="h-4 w-4" />
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end">
            <DropdownMenuItem asChild>
              <Link to={`/products/${row.original.id}/edit`}>
                <Pencil className="mr-2 h-4 w-4" />
                Edit
              </Link>
            </DropdownMenuItem>
            <DropdownMenuItem
              className="text-red-600 focus:text-red-600"
              onClick={() => setDeleteId(row.original.id)}
            >
              <Trash2 className="mr-2 h-4 w-4" />
              Delete
            </DropdownMenuItem>
          </DropdownMenuContent>
        </DropdownMenu>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Products</h1>
          <p className="text-slate-500">Manage your product catalog</p>
        </div>
        <Button asChild>
          <Link to="/products/new">
            <Plus className="mr-2 h-4 w-4" />
            Add Product
          </Link>
        </Button>
      </div>

      <ProductFilters
        search={search}
        onSearchChange={(v) => {
          setSearch(v);
          setPage(1);
        }}
        categoryId={categoryId}
        onCategoryChange={(v) => {
          setCategoryId(v);
          setPage(1);
        }}
        isActive={isActive}
        onIsActiveChange={(v) => {
          setIsActive(v);
          setPage(1);
        }}
        stockStatus={stockStatus}
        onStockStatusChange={(v) => {
          setStockStatus(v);
          setPage(1);
        }}
        categories={categoriesData || []}
        onClearFilters={handleClearFilters}
      />

      <DataTable
        columns={columns}
        data={data?.items || []}
        isLoading={isLoading}
        emptyMessage="No products found"
        emptyDescription="Get started by adding your first product"
      />

      {data && data.totalPages > 0 && (
        <Pagination
          currentPage={data.pageNumber}
          totalPages={data.totalPages}
          pageSize={pageSize}
          totalCount={data.totalCount}
          onPageChange={setPage}
          onPageSizeChange={(size) => {
            setPageSize(size);
            setPage(1);
          }}
        />
      )}

      <ConfirmDialog
        open={!!deleteId}
        onOpenChange={(open) => !open && setDeleteId(null)}
        title="Delete Product"
        description="Are you sure you want to delete this product? This action cannot be undone."
        confirmLabel="Delete"
        onConfirm={handleDelete}
        variant="destructive"
        isLoading={deleteMutation.isPending}
      />
    </div>
  );
}
