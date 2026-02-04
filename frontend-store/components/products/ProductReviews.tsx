'use client';

import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Star, ThumbsUp, Loader2, MessageSquare } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import { Textarea } from '@/components/ui/textarea';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { Separator } from '@/components/ui/separator';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from '@/components/ui/dialog';
import { useReviews, useReviewStats, useCreateReview } from '@/features/reviews/hooks/useReviews';
import { useAuthStore } from '@/stores/authStore';
import { formatDate, cn } from '@/lib/utils';

const reviewSchema = z.object({
  rating: z.number().min(1, 'Please select a rating').max(5),
  title: z.string().min(1, 'Title is required').max(100),
  comment: z.string().min(10, 'Review must be at least 10 characters').max(1000),
});

type ReviewFormData = z.infer<typeof reviewSchema>;

interface ProductReviewsProps {
  productId: string;
}

export function ProductReviews({ productId }: ProductReviewsProps) {
  const { isAuthenticated } = useAuthStore();
  const [page, setPage] = useState(1);
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [selectedRating, setSelectedRating] = useState(0);
  const [hoverRating, setHoverRating] = useState(0);

  const { data: reviews, isLoading: reviewsLoading } = useReviews(productId, page, 10);
  const { data: stats, isLoading: statsLoading } = useReviewStats(productId);
  const createReview = useCreateReview();

  const {
    register,
    handleSubmit,
    setValue,
    reset,
    formState: { errors },
  } = useForm<ReviewFormData>({
    resolver: zodResolver(reviewSchema),
    defaultValues: {
      rating: 0,
      title: '',
      comment: '',
    },
  });

  const handleRatingClick = (rating: number) => {
    setSelectedRating(rating);
    setValue('rating', rating);
  };

  const onSubmit = async (data: ReviewFormData) => {
    try {
      await createReview.mutateAsync({
        productId,
        rating: data.rating,
        title: data.title,
        comment: data.comment,
      });
      toast.success('Review submitted successfully!');
      setIsDialogOpen(false);
      reset();
      setSelectedRating(0);
    } catch {
      toast.error('Failed to submit review');
    }
  };

  const renderStars = (rating: number, interactive = false) => {
    return (
      <div className="flex gap-1">
        {[1, 2, 3, 4, 5].map((star) => (
          <button
            key={star}
            type={interactive ? 'button' : undefined}
            onClick={interactive ? () => handleRatingClick(star) : undefined}
            onMouseEnter={interactive ? () => setHoverRating(star) : undefined}
            onMouseLeave={interactive ? () => setHoverRating(0) : undefined}
            className={interactive ? 'focus:outline-none' : 'cursor-default'}
            disabled={!interactive}
          >
            <Star
              className={cn(
                'h-5 w-5',
                (interactive ? (hoverRating || selectedRating) >= star : rating >= star)
                  ? 'fill-yellow-400 text-yellow-400'
                  : 'text-slate-300'
              )}
            />
          </button>
        ))}
      </div>
    );
  };

  return (
    <div>
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold">Customer Reviews</h2>
        {isAuthenticated && (
          <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
            <DialogTrigger asChild>
              <Button>Write a Review</Button>
            </DialogTrigger>
            <DialogContent>
              <DialogHeader>
                <DialogTitle>Write a Review</DialogTitle>
              </DialogHeader>
              <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
                <div>
                  <Label>Rating *</Label>
                  <div className="mt-2">{renderStars(selectedRating, true)}</div>
                  {errors.rating && (
                    <p className="mt-1 text-xs text-red-500">{errors.rating.message}</p>
                  )}
                </div>

                <div>
                  <Label htmlFor="title">Title *</Label>
                  <Input
                    id="title"
                    {...register('title')}
                    placeholder="Summarize your experience"
                    className={errors.title ? 'border-red-500' : ''}
                  />
                  {errors.title && (
                    <p className="mt-1 text-xs text-red-500">{errors.title.message}</p>
                  )}
                </div>

                <div>
                  <Label htmlFor="comment">Review *</Label>
                  <Textarea
                    id="comment"
                    {...register('comment')}
                    rows={4}
                    placeholder="Share your experience with this product..."
                    className={errors.comment ? 'border-red-500' : ''}
                  />
                  {errors.comment && (
                    <p className="mt-1 text-xs text-red-500">{errors.comment.message}</p>
                  )}
                </div>

                <div className="flex gap-4">
                  <Button
                    type="button"
                    variant="outline"
                    className="flex-1"
                    onClick={() => setIsDialogOpen(false)}
                  >
                    Cancel
                  </Button>
                  <Button type="submit" className="flex-1" disabled={createReview.isPending}>
                    {createReview.isPending && (
                      <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    )}
                    Submit Review
                  </Button>
                </div>
              </form>
            </DialogContent>
          </Dialog>
        )}
      </div>

      {/* Stats summary */}
      {stats && !statsLoading && (
        <div className="mt-6 flex flex-col gap-6 rounded-lg border p-6 sm:flex-row">
          <div className="text-center sm:border-r sm:pr-6">
            <p className="text-4xl font-bold">{stats.averageRating.toFixed(1)}</p>
            <div className="mt-2 flex justify-center">{renderStars(stats.averageRating)}</div>
            <p className="mt-1 text-sm text-slate-500">{stats.totalReviews} reviews</p>
          </div>

          <div className="flex-1 space-y-2">
            {[5, 4, 3, 2, 1].map((rating) => {
              const ratingMap: Record<number, keyof typeof stats.ratingDistribution> = {
                5: 'fiveStar',
                4: 'fourStar',
                3: 'threeStar',
                2: 'twoStar',
                1: 'oneStar',
              };
              const count = stats.ratingDistribution[ratingMap[rating]] ?? 0;
              const percentage = stats.totalReviews > 0 ? (count / stats.totalReviews) * 100 : 0;

              return (
                <div key={rating} className="flex items-center gap-2">
                  <span className="w-8 text-sm">{rating} star</span>
                  <div className="h-2 flex-1 overflow-hidden rounded-full bg-slate-200">
                    <div
                      className="h-full bg-yellow-400"
                      style={{ width: `${percentage}%` }}
                    />
                  </div>
                  <span className="w-8 text-right text-sm text-slate-500">{count}</span>
                </div>
              );
            })}
          </div>
        </div>
      )}

      <Separator className="my-6" />

      {/* Reviews list */}
      {reviewsLoading ? (
        <div className="flex items-center justify-center py-12">
          <Loader2 className="h-8 w-8 animate-spin text-slate-400" />
        </div>
      ) : reviews && reviews.items.length > 0 ? (
        <div className="space-y-6">
          {reviews.items.map((review) => (
            <div key={review.id} className="border-b pb-6 last:border-b-0">
              <div className="flex items-start justify-between">
                <div>
                  <div className="flex items-center gap-2">
                    {renderStars(review.rating)}
                    {review.isVerifiedPurchase && (
                      <span className="rounded bg-green-100 px-2 py-0.5 text-xs text-green-800">
                        Verified Purchase
                      </span>
                    )}
                  </div>
                  <p className="mt-2 font-medium">{review.title}</p>
                </div>
                <p className="text-sm text-slate-500">{formatDate(review.createdAt)}</p>
              </div>

              <p className="mt-2 text-slate-600">{review.comment}</p>

              <div className="mt-4 flex items-center justify-between">
                <p className="text-sm text-slate-500">
                  By {review.userName || 'Anonymous'}
                </p>
                <Button variant="ghost" size="sm" className="text-slate-500">
                  <ThumbsUp className="mr-1 h-4 w-4" />
                  Helpful ({review.helpfulCount})
                </Button>
              </div>
            </div>
          ))}

          {/* Pagination */}
          {reviews.totalPages > 1 && (
            <div className="flex items-center justify-center gap-2 pt-4">
              <Button
                variant="outline"
                size="sm"
                disabled={!reviews.hasPreviousPage}
                onClick={() => setPage((p) => p - 1)}
              >
                Previous
              </Button>
              <span className="px-4 text-sm text-slate-600">
                Page {reviews.page} of {reviews.totalPages}
              </span>
              <Button
                variant="outline"
                size="sm"
                disabled={!reviews.hasNextPage}
                onClick={() => setPage((p) => p + 1)}
              >
                Next
              </Button>
            </div>
          )}
        </div>
      ) : (
        <div className="flex flex-col items-center justify-center py-12 text-center">
          <MessageSquare className="mb-4 h-12 w-12 text-slate-300" />
          <h3 className="font-semibold">No reviews yet</h3>
          <p className="mt-2 text-slate-500">Be the first to review this product!</p>
          {isAuthenticated && (
            <Button className="mt-4" onClick={() => setIsDialogOpen(true)}>
              Write a Review
            </Button>
          )}
        </div>
      )}
    </div>
  );
}
