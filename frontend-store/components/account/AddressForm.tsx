'use client';

import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Checkbox } from '@/components/ui/checkbox';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';

const addressSchema = z.object({
  firstName: z.string().min(1, 'First name is required'),
  lastName: z.string().min(1, 'Last name is required'),
  phone: z.string().min(10, 'Phone number must be at least 10 digits'),
  addressLine1: z.string().min(1, 'Address is required'),
  addressLine2: z.string().optional(),
  city: z.string().min(1, 'City is required'),
  stateProvince: z.string().min(1, 'State/Province is required'),
  postalCode: z.string().min(1, 'Postal code is required'),
  country: z.string().min(1, 'Country is required'),
  isDefault: z.boolean().optional(),
});

export type AddressFormData = z.infer<typeof addressSchema>;

interface AddressFormProps {
  initialData?: Partial<AddressFormData>;
  onSubmit: (data: AddressFormData) => void;
  onCancel: () => void;
  isLoading?: boolean;
}

export function AddressForm({
  initialData,
  onSubmit,
  onCancel,
  isLoading,
}: AddressFormProps) {
  const {
    register,
    handleSubmit,
    setValue,
    watch,
    formState: { errors },
  } = useForm<AddressFormData>({
    resolver: zodResolver(addressSchema),
    defaultValues: {
      firstName: '',
      lastName: '',
      phone: '',
      addressLine1: '',
      addressLine2: '',
      city: '',
      stateProvince: '',
      postalCode: '',
      country: 'LK',
      isDefault: false,
      ...initialData,
    },
  });

  const selectedCountry = watch('country');

  return (
    <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
      <div className="grid gap-4 sm:grid-cols-2">
        <div>
          <Label htmlFor="firstName">First Name *</Label>
          <Input
            id="firstName"
            {...register('firstName')}
            className={errors.firstName ? 'border-red-500' : ''}
          />
          {errors.firstName && (
            <p className="mt-1 text-xs text-red-500">{errors.firstName.message}</p>
          )}
        </div>

        <div>
          <Label htmlFor="lastName">Last Name *</Label>
          <Input
            id="lastName"
            {...register('lastName')}
            className={errors.lastName ? 'border-red-500' : ''}
          />
          {errors.lastName && (
            <p className="mt-1 text-xs text-red-500">{errors.lastName.message}</p>
          )}
        </div>
      </div>

      <div>
        <Label htmlFor="phone">Phone *</Label>
        <Input
          id="phone"
          type="tel"
          {...register('phone')}
          className={errors.phone ? 'border-red-500' : ''}
        />
        {errors.phone && (
          <p className="mt-1 text-xs text-red-500">{errors.phone.message}</p>
        )}
      </div>

      <div>
        <Label htmlFor="addressLine1">Address *</Label>
        <Input
          id="addressLine1"
          {...register('addressLine1')}
          className={errors.addressLine1 ? 'border-red-500' : ''}
        />
        {errors.addressLine1 && (
          <p className="mt-1 text-xs text-red-500">{errors.addressLine1.message}</p>
        )}
      </div>

      <div>
        <Label htmlFor="addressLine2">Apartment, suite, etc. (optional)</Label>
        <Input id="addressLine2" {...register('addressLine2')} />
      </div>

      <div className="grid gap-4 sm:grid-cols-3">
        <div>
          <Label htmlFor="city">City *</Label>
          <Input
            id="city"
            {...register('city')}
            className={errors.city ? 'border-red-500' : ''}
          />
          {errors.city && (
            <p className="mt-1 text-xs text-red-500">{errors.city.message}</p>
          )}
        </div>

        <div>
          <Label htmlFor="stateProvince">State/Province *</Label>
          <Input
            id="stateProvince"
            {...register('stateProvince')}
            className={errors.stateProvince ? 'border-red-500' : ''}
          />
          {errors.stateProvince && (
            <p className="mt-1 text-xs text-red-500">{errors.stateProvince.message}</p>
          )}
        </div>

        <div>
          <Label htmlFor="postalCode">Postal Code *</Label>
          <Input
            id="postalCode"
            {...register('postalCode')}
            className={errors.postalCode ? 'border-red-500' : ''}
          />
          {errors.postalCode && (
            <p className="mt-1 text-xs text-red-500">{errors.postalCode.message}</p>
          )}
        </div>
      </div>

      <div>
        <Label htmlFor="country">Country *</Label>
        <Select
          value={selectedCountry}
          onValueChange={(value) => setValue('country', value)}
        >
          <SelectTrigger className={errors.country ? 'border-red-500' : ''}>
            <SelectValue placeholder="Select country" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="LK">Sri Lanka</SelectItem>
            <SelectItem value="IN">India</SelectItem>
            <SelectItem value="US">United States</SelectItem>
            <SelectItem value="GB">United Kingdom</SelectItem>
            <SelectItem value="AU">Australia</SelectItem>
          </SelectContent>
        </Select>
        {errors.country && (
          <p className="mt-1 text-xs text-red-500">{errors.country.message}</p>
        )}
      </div>

      <div className="flex items-center gap-2">
        <Checkbox
          id="isDefault"
          checked={watch('isDefault')}
          onCheckedChange={(checked) => setValue('isDefault', checked as boolean)}
        />
        <Label htmlFor="isDefault" className="text-sm font-normal">
          Set as default address
        </Label>
      </div>

      <div className="flex gap-4 pt-4">
        <Button type="button" variant="outline" className="flex-1" onClick={onCancel}>
          Cancel
        </Button>
        <Button type="submit" className="flex-1" disabled={isLoading}>
          {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
          {initialData ? 'Update Address' : 'Add Address'}
        </Button>
      </div>
    </form>
  );
}
