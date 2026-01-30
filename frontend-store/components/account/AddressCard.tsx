'use client';

import { MapPin, Edit2, Trash2, Check } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import type { OrderAddress } from '@/types';

interface AddressCardProps {
  address: OrderAddress & { id?: string; isDefault?: boolean };
  onEdit?: () => void;
  onDelete?: () => void;
  onSetDefault?: () => void;
  showActions?: boolean;
}

export function AddressCard({
  address,
  onEdit,
  onDelete,
  onSetDefault,
  showActions = true,
}: AddressCardProps) {
  return (
    <div className="rounded-lg border p-4">
      <div className="flex items-start justify-between">
        <div className="flex items-start gap-3">
          <div className="rounded-full bg-slate-100 p-2">
            <MapPin className="h-5 w-5 text-slate-600" />
          </div>
          <div>
            <div className="flex items-center gap-2">
              <p className="font-medium">
                {address.firstName} {address.lastName}
              </p>
              {address.isDefault && (
                <Badge variant="secondary" className="text-xs">
                  Default
                </Badge>
              )}
            </div>
            <p className="mt-1 text-sm text-slate-600">{address.addressLine1}</p>
            {address.addressLine2 && (
              <p className="text-sm text-slate-600">{address.addressLine2}</p>
            )}
            <p className="text-sm text-slate-600">
              {address.city}, {address.state} {address.postalCode}
            </p>
            <p className="text-sm text-slate-600">{address.country}</p>
            <p className="mt-2 text-sm text-slate-500">{address.phone}</p>
          </div>
        </div>
      </div>

      {showActions && (
        <div className="mt-4 flex items-center gap-2 border-t pt-4">
          {onEdit && (
            <Button variant="outline" size="sm" onClick={onEdit}>
              <Edit2 className="mr-1 h-4 w-4" />
              Edit
            </Button>
          )}
          {onSetDefault && !address.isDefault && (
            <Button variant="outline" size="sm" onClick={onSetDefault}>
              <Check className="mr-1 h-4 w-4" />
              Set Default
            </Button>
          )}
          {onDelete && (
            <Button
              variant="outline"
              size="sm"
              className="text-red-600 hover:bg-red-50 hover:text-red-700"
              onClick={onDelete}
            >
              <Trash2 className="mr-1 h-4 w-4" />
              Delete
            </Button>
          )}
        </div>
      )}
    </div>
  );
}
