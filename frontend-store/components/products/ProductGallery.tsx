'use client';

import { useState } from 'react';
import Image from 'next/image';
import { ChevronLeft, ChevronRight, ZoomIn } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Dialog, DialogContent } from '@/components/ui/dialog';
import { cn } from '@/lib/utils';
import type { ProductImage } from '@/types';

interface ProductGalleryProps {
  images: ProductImage[];
  productName: string;
}

export function ProductGallery({ images, productName }: ProductGalleryProps) {
  const [selectedIndex, setSelectedIndex] = useState(0);
  const [isZoomed, setIsZoomed] = useState(false);

  const sortedImages = [...images].sort((a, b) => {
    if (a.isPrimary) return -1;
    if (b.isPrimary) return 1;
    return a.sortOrder - b.sortOrder;
  });

  const currentImage = sortedImages[selectedIndex];

  const goToPrevious = () => {
    setSelectedIndex((prev) => (prev === 0 ? sortedImages.length - 1 : prev - 1));
  };

  const goToNext = () => {
    setSelectedIndex((prev) => (prev === sortedImages.length - 1 ? 0 : prev + 1));
  };

  if (sortedImages.length === 0) {
    return (
      <div className="aspect-square w-full rounded-lg bg-slate-100 flex items-center justify-center">
        <span className="text-slate-400">No image available</span>
      </div>
    );
  }

  return (
    <div className="space-y-4">
      {/* Main image */}
      <div className="group relative aspect-square overflow-hidden rounded-lg bg-slate-100">
        <Image
          src={currentImage.thumbnailLargeUrl || currentImage.url}
          alt={currentImage.altText || productName}
          fill
          className="object-cover transition-transform duration-300 group-hover:scale-105"
          sizes="(max-width: 768px) 100vw, 50vw"
          priority
        />

        {/* Zoom button */}
        <button
          onClick={() => setIsZoomed(true)}
          className="absolute right-4 top-4 flex h-10 w-10 items-center justify-center rounded-full bg-white/80 opacity-0 transition-opacity group-hover:opacity-100"
        >
          <ZoomIn className="h-5 w-5" />
        </button>

        {/* Navigation arrows */}
        {sortedImages.length > 1 && (
          <>
            <Button
              variant="ghost"
              size="icon"
              className="absolute left-2 top-1/2 -translate-y-1/2 bg-white/80 opacity-0 transition-opacity group-hover:opacity-100"
              onClick={goToPrevious}
            >
              <ChevronLeft className="h-5 w-5" />
            </Button>
            <Button
              variant="ghost"
              size="icon"
              className="absolute right-2 top-1/2 -translate-y-1/2 bg-white/80 opacity-0 transition-opacity group-hover:opacity-100"
              onClick={goToNext}
            >
              <ChevronRight className="h-5 w-5" />
            </Button>
          </>
        )}
      </div>

      {/* Thumbnails */}
      {sortedImages.length > 1 && (
        <div className="flex gap-2 overflow-x-auto pb-2">
          {sortedImages.map((image, index) => (
            <button
              key={image.id}
              onClick={() => setSelectedIndex(index)}
              className={cn(
                'relative h-20 w-20 flex-shrink-0 overflow-hidden rounded-md border-2 transition-all',
                index === selectedIndex ? 'border-slate-900' : 'border-transparent hover:border-slate-300'
              )}
            >
              <Image
                src={image.thumbnailSmallUrl || image.url}
                alt={image.altText || `${productName} ${index + 1}`}
                fill
                className="object-cover"
                sizes="80px"
              />
            </button>
          ))}
        </div>
      )}

      {/* Zoom dialog */}
      <Dialog open={isZoomed} onOpenChange={setIsZoomed}>
        <DialogContent className="max-w-4xl p-0">
          <div className="relative aspect-square w-full">
            <Image
              src={currentImage.url}
              alt={currentImage.altText || productName}
              fill
              className="object-contain"
              sizes="100vw"
            />
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
