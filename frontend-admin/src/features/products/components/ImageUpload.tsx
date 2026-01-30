import { useState, useCallback } from 'react';
import { useDropzone } from 'react-dropzone';
import { X, Upload, Star, Loader2, Image as ImageIcon } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';

interface ProductImage {
  id: string;
  url: string;
  thumbnailSmallUrl?: string;
  thumbnailMediumUrl?: string;
  isPrimary: boolean;
  sortOrder: number;
}

interface ImageUploadProps {
  productId?: string;
  images: ProductImage[];
  onUpload: (files: File[]) => Promise<void>;
  onDelete: (imageId: string) => Promise<void>;
  onSetPrimary: (imageId: string) => Promise<void>;
  isUploading?: boolean;
  uploadProgress?: number;
  maxFiles?: number;
  disabled?: boolean;
}

export function ImageUpload({
  images,
  onUpload,
  onDelete,
  onSetPrimary,
  isUploading = false,
  uploadProgress = 0,
  maxFiles = 10,
  disabled = false,
}: ImageUploadProps) {
  const [deletingId, setDeletingId] = useState<string | null>(null);
  const [settingPrimaryId, setSettingPrimaryId] = useState<string | null>(null);

  const onDrop = useCallback(
    (acceptedFiles: File[]) => {
      if (acceptedFiles.length > 0) {
        onUpload(acceptedFiles);
      }
    },
    [onUpload]
  );

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: {
      'image/jpeg': ['.jpg', '.jpeg'],
      'image/png': ['.png'],
      'image/webp': ['.webp'],
      'image/gif': ['.gif'],
    },
    maxFiles: maxFiles - images.length,
    maxSize: 10 * 1024 * 1024, // 10MB
    disabled: disabled || isUploading || images.length >= maxFiles,
  });

  const handleDelete = async (imageId: string) => {
    setDeletingId(imageId);
    try {
      await onDelete(imageId);
    } finally {
      setDeletingId(null);
    }
  };

  const handleSetPrimary = async (imageId: string) => {
    setSettingPrimaryId(imageId);
    try {
      await onSetPrimary(imageId);
    } finally {
      setSettingPrimaryId(null);
    }
  };

  return (
    <div className="space-y-4">
      {/* Dropzone */}
      <div
        {...getRootProps()}
        className={cn(
          'relative flex cursor-pointer flex-col items-center justify-center rounded-lg border-2 border-dashed p-6 transition-colors',
          isDragActive
            ? 'border-blue-500 bg-blue-50'
            : 'border-slate-300 hover:border-slate-400',
          (disabled || isUploading || images.length >= maxFiles) &&
            'cursor-not-allowed opacity-50'
        )}
      >
        <input {...getInputProps()} />

        {isUploading ? (
          <div className="flex flex-col items-center gap-2">
            <Loader2 className="h-8 w-8 animate-spin text-blue-500" />
            <p className="text-sm text-slate-600">
              Uploading... {uploadProgress}%
            </p>
            <div className="h-2 w-48 overflow-hidden rounded-full bg-slate-200">
              <div
                className="h-full bg-blue-500 transition-all duration-300"
                style={{ width: `${uploadProgress}%` }}
              />
            </div>
          </div>
        ) : (
          <>
            <Upload className="mb-2 h-8 w-8 text-slate-400" />
            <p className="text-sm font-medium text-slate-700">
              {isDragActive
                ? 'Drop images here'
                : 'Drag & drop images here, or click to select'}
            </p>
            <p className="mt-1 text-xs text-slate-500">
              JPEG, PNG, WebP, or GIF up to 10MB each ({images.length}/{maxFiles} images)
            </p>
          </>
        )}
      </div>

      {/* Image Grid */}
      {images.length > 0 && (
        <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 md:grid-cols-4">
          {images
            .sort((a, b) => a.sortOrder - b.sortOrder)
            .map((image) => (
              <div
                key={image.id}
                className={cn(
                  'group relative aspect-square overflow-hidden rounded-lg border bg-slate-100',
                  image.isPrimary && 'ring-2 ring-blue-500'
                )}
              >
                <img
                  src={image.thumbnailMediumUrl || image.url}
                  alt=""
                  className="h-full w-full object-cover"
                />

                {/* Primary Badge */}
                {image.isPrimary && (
                  <div className="absolute left-2 top-2 rounded bg-blue-500 px-2 py-0.5 text-xs font-medium text-white">
                    Primary
                  </div>
                )}

                {/* Action Buttons */}
                <div className="absolute inset-0 flex items-center justify-center gap-2 bg-black/50 opacity-0 transition-opacity group-hover:opacity-100">
                  {!image.isPrimary && (
                    <Button
                      variant="secondary"
                      size="icon"
                      className="h-8 w-8"
                      onClick={() => handleSetPrimary(image.id)}
                      disabled={settingPrimaryId === image.id || disabled}
                    >
                      {settingPrimaryId === image.id ? (
                        <Loader2 className="h-4 w-4 animate-spin" />
                      ) : (
                        <Star className="h-4 w-4" />
                      )}
                    </Button>
                  )}

                  <Button
                    variant="destructive"
                    size="icon"
                    className="h-8 w-8"
                    onClick={() => handleDelete(image.id)}
                    disabled={deletingId === image.id || disabled}
                  >
                    {deletingId === image.id ? (
                      <Loader2 className="h-4 w-4 animate-spin" />
                    ) : (
                      <X className="h-4 w-4" />
                    )}
                  </Button>
                </div>
              </div>
            ))}
        </div>
      )}

      {/* Empty State */}
      {images.length === 0 && !isUploading && (
        <div className="flex flex-col items-center justify-center rounded-lg border border-dashed border-slate-200 py-8">
          <ImageIcon className="mb-2 h-12 w-12 text-slate-300" />
          <p className="text-sm text-slate-500">No images uploaded yet</p>
        </div>
      )}
    </div>
  );
}
