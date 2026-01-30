import { useState, useCallback } from 'react';
import { useDropzone } from 'react-dropzone';
import { X, Upload, Loader2, Image as ImageIcon } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';

interface CategoryImageUploadProps {
  currentImageUrl?: string;
  onUpload: (file: File) => Promise<string>;
  onRemove?: () => void;
  isUploading?: boolean;
  uploadProgress?: number;
  disabled?: boolean;
}

export function CategoryImageUpload({
  currentImageUrl,
  onUpload,
  onRemove,
  isUploading = false,
  uploadProgress = 0,
  disabled = false,
}: CategoryImageUploadProps) {
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const onDrop = useCallback(
    async (acceptedFiles: File[]) => {
      if (acceptedFiles.length > 0) {
        const file = acceptedFiles[0];
        setError(null);

        // Create preview
        const preview = URL.createObjectURL(file);
        setPreviewUrl(preview);

        try {
          await onUpload(file);
        } catch (err) {
          setError(err instanceof Error ? err.message : 'Failed to upload image');
          setPreviewUrl(null);
        }
      }
    },
    [onUpload]
  );

  const { getRootProps, getInputProps, isDragActive, fileRejections } = useDropzone({
    onDrop,
    accept: {
      'image/jpeg': ['.jpg', '.jpeg'],
      'image/png': ['.png'],
      'image/webp': ['.webp'],
      'image/gif': ['.gif'],
    },
    maxFiles: 1,
    maxSize: 10 * 1024 * 1024, // 10MB
    disabled: disabled || isUploading,
  });

  const handleRemove = () => {
    setPreviewUrl(null);
    setError(null);
    onRemove?.();
  };

  const displayUrl = previewUrl || currentImageUrl;
  const hasRejections = fileRejections.length > 0;

  return (
    <div className="space-y-3">
      {displayUrl ? (
        // Show current image with remove option
        <div className="relative inline-block">
          <img
            src={displayUrl}
            alt="Category"
            className="h-32 w-32 rounded-lg border object-cover"
          />
          {!isUploading && (
            <Button
              type="button"
              variant="destructive"
              size="icon"
              className="absolute -right-2 -top-2 h-6 w-6"
              onClick={handleRemove}
              disabled={disabled}
            >
              <X className="h-3 w-3" />
            </Button>
          )}
          {isUploading && (
            <div className="absolute inset-0 flex items-center justify-center rounded-lg bg-black/50">
              <div className="text-center text-white">
                <Loader2 className="mx-auto h-6 w-6 animate-spin" />
                <span className="text-xs">{uploadProgress}%</span>
              </div>
            </div>
          )}
        </div>
      ) : (
        // Dropzone for upload
        <div
          {...getRootProps()}
          className={cn(
            'relative flex h-32 w-32 cursor-pointer flex-col items-center justify-center rounded-lg border-2 border-dashed transition-colors',
            isDragActive
              ? 'border-blue-500 bg-blue-50'
              : 'border-slate-300 hover:border-slate-400',
            (disabled || isUploading) && 'cursor-not-allowed opacity-50'
          )}
        >
          <input {...getInputProps()} />

          {isUploading ? (
            <div className="flex flex-col items-center gap-1">
              <Loader2 className="h-6 w-6 animate-spin text-blue-500" />
              <span className="text-xs text-slate-600">{uploadProgress}%</span>
            </div>
          ) : (
            <>
              {isDragActive ? (
                <Upload className="h-8 w-8 text-blue-500" />
              ) : (
                <ImageIcon className="h-8 w-8 text-slate-400" />
              )}
              <span className="mt-1 text-xs text-slate-500">
                {isDragActive ? 'Drop here' : 'Upload image'}
              </span>
            </>
          )}
        </div>
      )}

      {/* Error messages */}
      {error && <p className="text-sm text-red-600">{error}</p>}
      {hasRejections && (
        <p className="text-sm text-red-600">
          {fileRejections[0].errors[0].message}
        </p>
      )}

      <p className="text-xs text-slate-500">JPEG, PNG, WebP, or GIF up to 10MB</p>
    </div>
  );
}
