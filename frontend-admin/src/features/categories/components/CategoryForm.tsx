import { useState, useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Loader2, Info } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { Checkbox } from '@/components/ui/checkbox';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog';
import { CategoryImageUpload } from './CategoryImageUpload';
import { categoryService } from '../services/categoryService';
import type { Category } from '@/types';

const categorySchema = z.object({
  name: z.string().min(1, 'Name is required').max(100),
  slug: z.string().max(100).optional(),
  description: z.string().max(500).optional(),
  parentId: z.string().optional(),
  imageUrl: z.string().optional(), // URL is set by upload, not validated as URL since it can be empty
  isActive: z.boolean(),
  sortOrder: z.number().int().min(0),
});

type CategoryFormData = z.infer<typeof categorySchema>;

export interface CategoryFormOutput {
  name: string;
  slug?: string;
  description?: string;
  parentId?: string;
  imageUrl?: string;
  isActive: boolean;
  sortOrder: number;
}

interface CategoryFormProps {
  open: boolean;
  onClose: () => void;
  category?: Category;
  categories: Category[];
  onSubmit: (data: CategoryFormOutput) => void;
  isLoading?: boolean;
}

export function CategoryForm({
  open,
  onClose,
  category,
  categories,
  onSubmit,
  isLoading,
}: CategoryFormProps) {
  const [isUploading, setIsUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);

  const {
    register,
    handleSubmit,
    setValue,
    watch,
    reset,
    formState: { errors },
  } = useForm<CategoryFormData>({
    resolver: zodResolver(categorySchema),
    defaultValues: {
      name: category?.name || '',
      slug: category?.slug || '',
      description: category?.description || '',
      parentId: category?.parentId || '',
      imageUrl: category?.imageUrl || '',
      isActive: category?.isActive ?? true,
      sortOrder: category?.sortOrder || 0,
    },
  });

  const isActive = watch('isActive');
  const parentId = watch('parentId');
  const imageUrl = watch('imageUrl');

  const isEditMode = !!category?.id;

  // Reset form when category changes or dialog opens
  useEffect(() => {
    if (open) {
      reset({
        name: category?.name || '',
        slug: category?.slug || '',
        description: category?.description || '',
        parentId: category?.parentId || '',
        imageUrl: category?.imageUrl || '',
        isActive: category?.isActive ?? true,
        sortOrder: category?.sortOrder || 0,
      });
    }
  }, [category, open, reset]);

  // Filter out the current category and its children from parent options
  const availableParents = categories.filter((c) => {
    if (!category) return true;
    if (c.id === category.id) return false;
    // Also filter out children of the current category
    return c.parentId !== category.id;
  });

  const handleClose = () => {
    reset();
    setIsUploading(false);
    setUploadProgress(0);
    onClose();
  };

  const handleImageUpload = async (file: File): Promise<string> => {
    if (!category?.id) {
      throw new Error('Save category first before uploading image');
    }

    setIsUploading(true);
    setUploadProgress(0);

    try {
      const result = await categoryService.uploadCategoryImage(
        category.id,
        file,
        setUploadProgress
      );
      setValue('imageUrl', result.imageUrl);
      return result.imageUrl;
    } finally {
      setIsUploading(false);
    }
  };

  const handleImageRemove = () => {
    setValue('imageUrl', '');
  };

  const handleFormSubmit = (data: CategoryFormData) => {
    const output: CategoryFormOutput = {
      name: data.name,
      slug: data.slug || undefined,
      description: data.description || undefined,
      parentId: data.parentId || undefined,
      imageUrl: data.imageUrl || undefined,
      isActive: data.isActive,
      sortOrder: data.sortOrder,
    };
    onSubmit(output);
  };

  return (
    <Dialog open={open} onOpenChange={(open) => !open && handleClose()}>
      <DialogContent className="max-w-md">
        <DialogHeader>
          <DialogTitle>{category ? 'Edit Category' : 'Add Category'}</DialogTitle>
        </DialogHeader>

        <form onSubmit={handleSubmit(handleFormSubmit)} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="name">Name *</Label>
            <Input id="name" {...register('name')} />
            {errors.name && <p className="text-sm text-red-600">{errors.name.message}</p>}
          </div>

          <div className="space-y-2">
            <Label htmlFor="slug">Slug</Label>
            <Input id="slug" {...register('slug')} placeholder="auto-generated if empty" />
          </div>

          <div className="space-y-2">
            <Label htmlFor="description">Description</Label>
            <Textarea id="description" rows={3} {...register('description')} />
          </div>

          <div className="space-y-2">
            <Label>Parent Category</Label>
            <Select
              value={parentId || 'none'}
              onValueChange={(v) => setValue('parentId', v === 'none' ? '' : v)}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select parent" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="none">No parent (top-level)</SelectItem>
                {availableParents.map((c) => (
                  <SelectItem key={c.id} value={c.id}>
                    {c.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          <div className="space-y-2">
            <Label>Category Image</Label>
            {isEditMode ? (
              <CategoryImageUpload
                currentImageUrl={imageUrl}
                onUpload={handleImageUpload}
                onRemove={handleImageRemove}
                isUploading={isUploading}
                uploadProgress={uploadProgress}
                disabled={isLoading}
              />
            ) : (
              <div className="flex items-start gap-2 rounded-md border border-slate-200 bg-slate-50 p-3">
                <Info className="mt-0.5 h-4 w-4 flex-shrink-0 text-slate-500" />
                <p className="text-sm text-slate-600">
                  Save the category first, then edit it to upload an image.
                </p>
              </div>
            )}
          </div>

          <div className="space-y-2">
            <Label htmlFor="sortOrder">Sort Order</Label>
            <Input id="sortOrder" type="number" {...register('sortOrder', { valueAsNumber: true })} />
          </div>

          <div className="flex items-center space-x-2">
            <Checkbox
              id="isActive"
              checked={isActive}
              onCheckedChange={(checked) => setValue('isActive', !!checked)}
            />
            <Label htmlFor="isActive" className="font-normal">
              Category is active
            </Label>
          </div>

          <DialogFooter>
            <Button type="button" variant="outline" onClick={handleClose}>
              Cancel
            </Button>
            <Button type="submit" disabled={isLoading}>
              {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              {category ? 'Update' : 'Create'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
