import { Link } from 'react-router-dom';
import { ArrowLeft } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { ProductForm } from '../components/ProductForm';
import { useCreateProduct, useCategories } from '../hooks/useProducts';
import { PageLoader } from '@/components/common/LoadingSpinner';

export function ProductCreatePage() {
  const { data: categories, isLoading: categoriesLoading } = useCategories();
  const createMutation = useCreateProduct();

  if (categoriesLoading) {
    return <PageLoader />;
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
          <h1 className="text-2xl font-bold text-slate-900">Add New Product</h1>
          <p className="text-slate-500">Create a new product for your catalog</p>
        </div>
      </div>

      <ProductForm
        categories={categories || []}
        onSubmit={(data) => createMutation.mutate(data)}
        isLoading={createMutation.isPending}
      />
    </div>
  );
}
