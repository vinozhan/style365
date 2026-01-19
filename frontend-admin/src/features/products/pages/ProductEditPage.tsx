import { Link, useParams } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { ProductForm } from '../components/ProductForm';
import { useProduct, useUpdateProduct, useCategories } from '../hooks/useProducts';
import { PageLoader } from '@/components/common/LoadingSpinner';
import { EmptyState } from '@/components/common/EmptyState';

export function ProductEditPage() {
  const { id } = useParams<{ id: string }>();
  const { data: product, isLoading: productLoading } = useProduct(id!);
  const { data: categories, isLoading: categoriesLoading } = useCategories();
  const updateMutation = useUpdateProduct();

  if (productLoading || categoriesLoading) {
    return <PageLoader />;
  }

  if (!product) {
    return (
      <EmptyState
        title="Product not found"
        description="The product you're looking for doesn't exist or has been deleted."
        action={
          <Button asChild>
            <Link to="/products">Back to Products</Link>
          </Button>
        }
      />
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-4">
        <Button variant="ghost" size="icon" asChild>
          <Link to="/products">
            <ArrowLeft className="h-4 w-4" />
          </Link>
        </Button>
        <div>
          <h1 className="text-2xl font-bold text-slate-900">Edit Product</h1>
          <p className="text-slate-500">{product.name}</p>
        </div>
      </div>

      <ProductForm
        product={product}
        categories={categories || []}
        onSubmit={(data) => updateMutation.mutate({ id: id!, data: { ...data, id: id! } })}
        isLoading={updateMutation.isPending}
      />
    </div>
  );
}
