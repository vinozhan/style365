'use client';

import { toast } from 'sonner';
import { Skeleton } from '@/components/ui/skeleton';
import { ProfileForm } from '@/components/account';
import type { ProfileFormData } from '@/components/account';
import { useProfile, useUpdateProfile } from '@/features/user-profile/hooks/useProfile';

export default function ProfilePage() {
  const { data: profile, isLoading } = useProfile();
  const updateProfile = useUpdateProfile();

  const handleSubmit = async (data: ProfileFormData) => {
    try {
      await updateProfile.mutateAsync(data);
      toast.success('Profile updated successfully');
    } catch {
      toast.error('Failed to update profile');
    }
  };

  if (isLoading || !profile) {
    return (
      <div>
        <h1 className="mb-6 text-2xl font-bold">My Profile</h1>
        <div className="space-y-4">
          <Skeleton className="h-12 w-full" />
          <Skeleton className="h-12 w-full" />
          <Skeleton className="h-12 w-full" />
          <Skeleton className="h-32 w-full" />
        </div>
      </div>
    );
  }

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold">My Profile</h1>
      <div className="rounded-lg border p-6">
        <ProfileForm
          user={profile}
          onSubmit={handleSubmit}
          isLoading={updateProfile.isPending}
        />
      </div>
    </div>
  );
}
