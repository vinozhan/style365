import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { productService, type ProductFilters, type CreateProductInput, type UpdateProductInput } from '../services/productService';

export function useProducts(filters: ProductFilters = {}) {
  return useQuery({
    queryKey: ['products', filters],
    queryFn: () => productService.getProducts(filters),
    placeholderData: (previousData) => previousData,
  });
}

export function useProduct(id: string) {
  return useQuery({
    queryKey: ['product', id],
    queryFn: () => productService.getProduct(id),
    enabled: !!id,
  });
}

export function useCreateProduct() {
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  return useMutation({
    mutationFn: (data: CreateProductInput) => productService.createProduct(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
      toast.success('Product created successfully');
      navigate('/products');
    },
    onError: (error: Error & { response?: { data?: { errors?: string[]; message?: string } } }) => {
      const errors = error.response?.data?.errors;
      const message = errors && errors.length > 0
        ? errors.join(', ')
        : error.response?.data?.message || 'Failed to create product';
      toast.error(message);
    },
  });
}

export function useUpdateProduct() {
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  return useMutation({
    mutationFn: ({ id, data }: { id: string; data: UpdateProductInput }) =>
      productService.updateProduct(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
      toast.success('Product updated successfully');
      navigate('/products');
    },
    onError: (error: Error & { response?: { data?: { errors?: string[]; message?: string } } }) => {
      const errors = error.response?.data?.errors;
      const message = errors && errors.length > 0
        ? errors.join(', ')
        : error.response?.data?.message || 'Failed to update product';
      toast.error(message);
    },
  });
}

export function useDeleteProduct() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (id: string) => productService.deleteProduct(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] });
      toast.success('Product deleted successfully');
    },
    onError: (error: Error & { response?: { data?: { errors?: string[]; message?: string } } }) => {
      const errors = error.response?.data?.errors;
      const message = errors && errors.length > 0
        ? errors.join(', ')
        : error.response?.data?.message || 'Failed to delete product';
      toast.error(message);
    },
  });
}

export function useCategories() {
  return useQuery({
    queryKey: ['categories', 'all'],
    queryFn: () => productService.getCategories(),
    staleTime: 5 * 60 * 1000, // 5 minutes
  });
}

// Image Upload Hooks
export function useUploadProductImages(productId: string) {
  const queryClient = useQueryClient();
  const [uploadProgress, setUploadProgress] = useState(0);

  const mutation = useMutation({
    mutationFn: ({ files, altText }: { files: File[]; altText?: string }) =>
      productService.uploadImages(productId, files, altText, setUploadProgress),
    onSuccess: (result) => {
      queryClient.invalidateQueries({ queryKey: ['product', productId] });
      queryClient.invalidateQueries({ queryKey: ['products'] });
      if (result.totalUploaded > 0) {
        toast.success(`${result.totalUploaded} image(s) uploaded successfully`);
      }
      if (result.totalFailed > 0) {
        toast.warning(`${result.totalFailed} image(s) failed to upload`);
      }
      setUploadProgress(0);
    },
    onError: (error: Error & { response?: { data?: { errors?: string[]; message?: string } } }) => {
      const errors = error.response?.data?.errors;
      const message = errors && errors.length > 0
        ? errors.join(', ')
        : error.response?.data?.message || 'Failed to upload images';
      toast.error(message);
      setUploadProgress(0);
    },
  });

  return { ...mutation, uploadProgress };
}

export function useDeleteProductImage(productId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (imageId: string) => productService.deleteImage(productId, imageId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['product', productId] });
      queryClient.invalidateQueries({ queryKey: ['products'] });
      toast.success('Image deleted successfully');
    },
    onError: (error: Error & { response?: { data?: { errors?: string[]; message?: string } } }) => {
      const errors = error.response?.data?.errors;
      const message = errors && errors.length > 0
        ? errors.join(', ')
        : error.response?.data?.message || 'Failed to delete image';
      toast.error(message);
    },
  });
}

export function useSetPrimaryImage(productId: string) {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: (imageId: string) => productService.setPrimaryImage(productId, imageId),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['product', productId] });
      queryClient.invalidateQueries({ queryKey: ['products'] });
      toast.success('Primary image updated');
    },
    onError: (error: Error & { response?: { data?: { errors?: string[]; message?: string } } }) => {
      const errors = error.response?.data?.errors;
      const message = errors && errors.length > 0
        ? errors.join(', ')
        : error.response?.data?.message || 'Failed to update primary image';
      toast.error(message);
    },
  });
}

// CSV Import Hooks
export function useImportProducts() {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({
      file,
      validateOnly,
      skipDuplicates,
    }: {
      file: File;
      validateOnly: boolean;
      skipDuplicates: boolean;
    }) => productService.importFromCsv(file, validateOnly, skipDuplicates),
    onSuccess: (result) => {
      if (!result.validateOnly && result.successCount > 0) {
        queryClient.invalidateQueries({ queryKey: ['products'] });
        toast.success(`${result.successCount} product(s) imported successfully`);
      }
    },
    onError: (error: Error & { response?: { data?: { errors?: string[]; message?: string } } }) => {
      const errors = error.response?.data?.errors;
      const message = errors && errors.length > 0
        ? errors.join(', ')
        : error.response?.data?.message || 'Import failed';
      toast.error(message);
    },
  });
}

export function getImportTemplateUrl() {
  return productService.getImportTemplateUrl();
}
