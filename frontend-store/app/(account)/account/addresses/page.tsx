'use client';

import { useState } from 'react';
import { Plus, MapPin } from 'lucide-react';
import { toast } from 'sonner';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { AddressCard, AddressForm } from '@/components/account';
import type { AddressFormData } from '@/components/account';
import type { OrderAddress } from '@/types';

// Mock addresses for now - would come from API
type AddressWithId = OrderAddress & { id: string; isDefault: boolean };

export default function AddressesPage() {
  const [addresses, setAddresses] = useState<AddressWithId[]>([]);
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [editingAddress, setEditingAddress] = useState<AddressWithId | null>(null);
  const [isLoading, setIsLoading] = useState(false);

  const handleSubmit = async (data: AddressFormData) => {
    setIsLoading(true);
    try {
      // In real implementation, would call API
      if (editingAddress) {
        // Update
        setAddresses((prev) =>
          prev.map((addr) =>
            addr.id === editingAddress.id
              ? { ...addr, ...data, isDefault: data.isDefault ?? addr.isDefault }
              : data.isDefault
              ? { ...addr, isDefault: false }
              : addr
          )
        );
        toast.success('Address updated');
      } else {
        // Create
        const newAddress: AddressWithId = {
          id: Date.now().toString(),
          ...data,
          addressLine2: data.addressLine2 ?? undefined,
          isDefault: data.isDefault ?? false,
        };
        setAddresses((prev) =>
          data.isDefault
            ? [...prev.map((a) => ({ ...a, isDefault: false })), newAddress]
            : [...prev, newAddress]
        );
        toast.success('Address added');
      }
      setIsDialogOpen(false);
      setEditingAddress(null);
    } finally {
      setIsLoading(false);
    }
  };

  const handleEdit = (address: AddressWithId) => {
    setEditingAddress(address);
    setIsDialogOpen(true);
  };

  const handleDelete = (id: string) => {
    setAddresses((prev) => prev.filter((a) => a.id !== id));
    toast.success('Address deleted');
  };

  const handleSetDefault = (id: string) => {
    setAddresses((prev) =>
      prev.map((a) => ({ ...a, isDefault: a.id === id }))
    );
    toast.success('Default address updated');
  };

  const handleCloseDialog = () => {
    setIsDialogOpen(false);
    setEditingAddress(null);
  };

  return (
    <div>
      <div className="mb-6 flex items-center justify-between">
        <h1 className="text-2xl font-bold">My Addresses</h1>
        <Button onClick={() => setIsDialogOpen(true)}>
          <Plus className="mr-2 h-4 w-4" />
          Add Address
        </Button>
      </div>

      {addresses.length === 0 ? (
        <div className="flex flex-col items-center justify-center py-12 text-center">
          <div className="mb-4 rounded-full bg-slate-100 p-4">
            <MapPin className="h-8 w-8 text-slate-400" />
          </div>
          <h2 className="text-lg font-semibold">No addresses saved</h2>
          <p className="mt-2 text-slate-500">
            Add your shipping addresses for faster checkout.
          </p>
          <Button className="mt-6" onClick={() => setIsDialogOpen(true)}>
            <Plus className="mr-2 h-4 w-4" />
            Add Address
          </Button>
        </div>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2">
          {addresses.map((address) => (
            <AddressCard
              key={address.id}
              address={address}
              onEdit={() => handleEdit(address)}
              onDelete={() => handleDelete(address.id)}
              onSetDefault={() => handleSetDefault(address.id)}
            />
          ))}
        </div>
      )}

      <Dialog open={isDialogOpen} onOpenChange={handleCloseDialog}>
        <DialogContent className="max-w-lg">
          <DialogHeader>
            <DialogTitle>
              {editingAddress ? 'Edit Address' : 'Add New Address'}
            </DialogTitle>
          </DialogHeader>
          <AddressForm
            initialData={editingAddress || undefined}
            onSubmit={handleSubmit}
            onCancel={handleCloseDialog}
            isLoading={isLoading}
          />
        </DialogContent>
      </Dialog>
    </div>
  );
}
